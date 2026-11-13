# RMS-COORD-ALLOC-01 — убрать массив координат на каждый элемент во время настоящего обучения

## Статус, цель и граница поручения

**Статус: `DESIGN_DOCUMENTED_HOST_NOT_MATERIALIZED_NOT_RUN`.** В этом аудите нет нового запуска, собранного host или измеренного улучшения. Карточка задаёт точный рецепт будущего малого опыта. Команды ниже становятся исполнимыми после указанного этапа подготовки; сейчас готовность к запуску не заявляется.

Цель — проверить стоимость **реального SFT/LoRA update** при замене одного способа вычисления индекса внутри `TensorOps.MultiplyBroadcast`. Основной полезный показатель — allocated bytes за четыре учебных шага; рядом время, CPU, GC, процессная память и качество ученических ответов. Предметная тема — язык программирования C#; задача — определить знак целочисленного вычитания. Приоритет — после разблокирования обучения и разрешения конфликтов действующих карточек. Это не утверждение, что данный механизм объяснил ухудшение ответов RP01.

**Любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности для этого не обязателен. Рядом показываются изменения остальных показателей, включая ухудшения.** Числа, рассчитанные по исходникам, не являются измеренными результатами.

**Запрет продолжать прежнюю задачу.** При прямом «стоп», «закончить немедленно» — сразу прекратить текущую работу и кратко сообщить уже известное состояние. При «следующий эксперимент» — прекратить улучшать, перепроверять, коммитить или дописывать предыдущий опыт и перейти только к названному следующему. Нельзя превращать оформление отчёта в новый запуск, поиск формата, стилистический цикл или дополнительную проверку кода. Старые опыты, результаты и незавершённые attempt-каталоги не удалять.

## Доказательства и участие в обучении

Все исходные ссылки относятся к core **`ab583032562d6656de9dd5ca6fa0a325fd4b1f30`**. Более поздний Markdown-коммит не означает изменение алгоритма; при будущем исполнении берутся именно эти исходники.

1. [`Trainer.TrainSupervisedNextTokenStep:102–145`][trainer] строит actual next-token inputs/targets, выполняет `model.Forward(training:true)`, loss, backward, clipping и `optimizer.Step`, затем увеличивает счётчик supervised-step.
2. [`LlmModel.Forward:219–242`][model] проводит этот же forward через блоки и финальный `RmsNorm`. [`TransformerBlock.Forward:46–64`][block] дополнительно вызывает два `RmsNorm` в каждом блоке. Подключение LoRA не убирает эти нормализации: attention читает их выход, LoRA меняет q/v-проекции, а native backward возвращается по сохранённому графу.
3. [`RmsNorm.Forward:29–40`][rms] дважды вызывает `MultiplyBroadcast`: сначала умножает `[B,T,H]` на `[B,T,1]`, затем на gamma `[H]`.
4. В [`MultiplyBroadcast:168–184`][broadcast] для **каждого flat-элемента** создаётся `indices = FlatToIndices(flat, a.Shape)`. [`FlatToIndices:1008–1017`][coords] каждый раз выделяет `new int[shape.Rank]`. Это служебный массив для адресации; он не является весами, словарём, доказательством или обучаемой структурой.
5. [`MultiplyBroadcast:187–210`][backward] использует сохранённый `mappedBIndices` для обратного прохода. Этот общий массив, структура графа и порядок накопления градиентов остаются одинаковыми в A/B.

Цепочка: **одинаковые учебные последовательности → native SFT forward → RMS broadcast → native backward → AdamW → изменённые LoRA параметры → независимый EXAM readout**. Опыт не сводится к benchmark вызовов голой индексации. Если native update не выполнен, результат — технический/частичный, а не нулевая точность.

RMS также достижим в DPO через [`ForwardPolicyLogProbs:1018–1030`][dpo-forward], но этот малый паспорт измеряет SFT/LoRA. Количество DPO steps в отчёте — **0**. Перенос ресурсного эффекта на DPO — отдельное, ещё не измеренное предположение; нельзя добавлять DPO-прогоны к текущему бюджету.

## A/B: один фактор и точный предлагаемый ремонт

**A:** действующий mapping loop `MultiplyBroadcast`, включая `FlatToIndices`.

**B:** тот же `bFlat` вычисляется справа налево целочисленными division/remainder без промежуточного массива координат. Остаются исходные `data`, `mappedBIndices`, проверки форм, cancellation cadence, умножения float, создание Tensor, `Children`, `BackwardFunc`, порядок накопления и вся остальная модель.

Предлагаемый diff **только экспериментальной копии** `00.TensorCore/Core/TensorOps.cs`, внутри исходного метода:

```diff
-            var indices = FlatToIndices(flat, a.Shape);
             int bFlat = 0;
-            for (int dimension = 0; dimension < b.Shape.Rank; dimension++)
+            int remaining = flat;
+            int bStride = 1;
+            for (int dimension = b.Shape.Rank - 1; dimension >= 0; dimension--)
             {
                 int bDimension = b.Shape.Dimensions[dimension];
-                int bIndex = bDimension == 1 ? 0 : indices[rankOffset + dimension];
-                bFlat = bFlat * bDimension + bIndex;
+                int aDimension = a.Shape.Dimensions[rankOffset + dimension];
+                int coordinate = remaining % aDimension;
+                remaining /= aDimension;
+                if (bDimension != 1)
+                    bFlat += coordinate * bStride;
+                bStride *= bDimension;
             }
```

Обоснование — математическое, не результат запуска: trailing dimensions B соответствуют trailing dimensions A; broadcast-ось с размером 1 всегда даёт координату 0. Для `[B,T,1]` выбирается соответствующая строка RMS, для `[H]` — `flat % H`. Данные и порядок float-операций сохраняются. Требуется измерить весь учебный переход и увидеть фактическую численную эквивалентность.

Это отдельный фактор от уже описанных frozen transpose cache, softmax scratch reuse и двойного копирования начального gradient. Здесь **не добавляется кэш**, не меняются `TensorStorage`, `Mean`, mask, objective или trainable set. Уже исправленное прохождение autograd через RMS сохраняется; возврат исторического `new Tensor` без backward запрещён.

## Фиксированная модель, предметная тема, задача, данные и бюджет

Исполнитель — **та же модель и тот же режим, которые уже используются в назначенной сессии**. Другую модель, внешний judge или способ выполнения не искать. Ученик — собственный C# Transformer `XIIGEN-native-1block-SFT-LoRA-64`, не модель исполнителя.

| Поле | Значение |
|---|---|
| Config | Vocab256; Context64; Hidden64; 1 block; 4 Q heads; 2 KV heads; FFN176; RoPE theta10000; RMS eps1e−5 |
| Base | `Initializers.UseDeterministicSeed(62001UL)` при каждом новом процессе |
| LoRA | `LoraApplicator.ApplySeeded`, q_proj/v_proj, rank4, alpha8, dropout0, rootSeed62002, stream `rms-coordinate-probe-v1` одинаков в обоих плечах |
| Native updater | `Trainer.TrainSupervisedNextTokenStep`, weight1; 4 вызова на плечо |
| Optimizer | `AdamW(beta1:.9f, beta2:.95f, weightDecay:0f, eps:1e-8f)` |
| Schedule | warmup0; totalSteps4; maxLr=minLr=.001; clip norm1; gradAccum1 |
| Tokens | UTF-8 byte IDs0…255, ноль BPE merges; EOS не добавляется |
| Objective | Исходный full-sequence shifted next-token SFT; response mask и нормировка не меняются |
| Actual budget | A:4 updates; B:4 updates; TRAIN порядок T1,T2,T1,T2 в каждом плече; всего8 updates |
| EXAM | По2 forward до и после в каждом плече; всего8 EXAM forward; обучение на EXAM запрещено |

| Split | ID | Prompt буквально | Answer буквально |
|---|---|---|---|
| TRAIN | T1 | `C# result sign: 9-4 =` | `+` |
| TRAIN | T2 | `C# result sign: 4-9 =` | `-` |
| EXAM | E1 | `C# result sign: 8-3 =` | `+` |
| EXAM | E2 | `C# result sign: 3-8 =` | `-` |

Это узкая панель разных чисел, не доказательство общей арифметической компетенции. Ответ ученика — argmax фактического последнего логита из 256, gold служит только оценке. Корректирующее правило не генерирует `+`/`−` за модель.

## Подготовка host: заданный рецепт, не поиск

Рабочая машина данного рецепта — Windows с уже используемым **.NET9 SDK**, как задано в прочитанном исходном экспериментальном host. Новый короткий корень **`C:/xg/RMS01`**. Первая новая попытка — `a001`; если она существует, для **отдельно назначенного нового исполнения** использовать следующий `aNNN` после максимального существующего. Существующий attempt не очищается и не продолжается автоматически. Внутри текущего поручения нет автоматического retry. Отсутствие SDK фиксируется `BUILD_TOOL_MISSING`; не устанавливать другой toolchain и не искать новый проект.

1. При **отдельно назначенном исполнении** в новом `C:/xg/RMS01/aNNN` создать `src`, `Runner.csproj`, `Program.cs`, `run.py`. **Не создавать `results`, `bin` или `obj` на этапе подготовки**: `results` эксклюзивно создаст runner, `bin/obj` — единственная сборка. Ни один из этих файлов не создаётся текущим документарным аудитом.
2. Взять **ровно 36 файлов**, явно перечисленных в [новом соседнем source manifest](RMS-COORD-ALLOC-01-SOURCE-MANIFEST.json), и скопировать их из read-only core на `ab583032562d6656de9dd5ca6fa0a325fd4b1f30`, сохраняя относительные пути внутри `src`. Новый manifest содержит проверенный Git blob SHA-1 каждого исходного файла. [Прежний manifest][source-manifest] привязан к `62bcc8f2368840e22d2ea98ef16d9bd526ad9a39`: из него унаследован только состав файлов, он не задаёт версию нового опыта. Все 36 текущих blob совпали с его SHA; это установлено чтением исходников, не запуском. Исходный корень: `C:/Projects/llm_mvp_core`; использовать уже доступную локальную версию указанного файла/коммита. При несовпадении конкретного файла сообщить его и остановить preparation; не искать замену в другом проекте. `Linear.cs` должен быть **оригинальным core**, без экспериментального transpose cache.
3. За основу `Program.cs` взять [точный существующий SFT host][host-program] на planning `f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9`, сохранить модель, TRAIN, EXAM, updater, таймеры и readouts. Заменить ID/commit/stream на значения карточки. В raw записать `subject = "programming-language-csharp"`, `task = "integer-subtraction-sign"`; `code-sign` оставить техническим именем профиля. Удалить обращения `Linear.ExperimentReuseFrozenTranspose`, `ExperimentCacheHits`, `ExperimentCacheMisses`, `ExperimentCachedFloats`, cache fields и cache limitations. В `mechanism` записывать только `arithmetic_broadcast_index_enabled = arm == "B"`. Указать, что `training_losses` — online loss **перед соответствующим update**, а не оценка после четвёртого update.
4. В экспериментальной копии `TensorOps` добавить `internal static bool ExperimentUseArithmeticBroadcastIndex`. После общих проверок/выделения `data` и `mappedBIndices` — один выбор ветви: A выполняет исходный mapping loop, B — loop с diff выше. Общая конструкция graph/backward остаётся после выбора. Program ставит переключатель `false` на оба A0/B0, затем `arm == "B"` перед TRAIN и сохраняет до A1/B1. Одна сборка включает оба варианта; branch выбирается один раз на вызов метода. Experiment-only switch, literal tiny fixtures и отдельный host из копий исходников разрешены **только в данном опыте**; эти исключения не разрешают менять prod или подменять native Tensor/Trainer/LoRA на mock.
5. Новый плоский `Runner.csproj` основан на [существующем проекте][host-project]: `TargetFramework=net9.0`, `AssemblyName=RMS-COORD-ALLOC-01`, `EnableDefaultCompileItems=false`, Compile только `src/**/*.cs` и `Program.cs`, без ProjectReference. Все `bin`, `obj` и NuGet project extensions — внутри короткого attempt; до SDK imports выключить parent `Directory.Build.props/targets`. Нельзя собирать большой solution или dispatcher.
6. Написать **новый no-argument `run.py`** с фиксированным контрактом ниже. [Прежний runner][host-run] — источник ограниченного build/timeout поведения, а не готовая команда нового опыта. Удалить legacy `argparse/--results-root`, чтение `experiment.json`, old experiment kind и длинные nested paths. Задать буквально `root = Path(__file__).resolve().parent`, `host = root / "Runner.csproj"`, `artifact = root`, `result = root / "results"`, `experiment_id = "RMS-COORD-ALLOC-01"`, `jobs = [("A", "code-sign"), ("B", "code-sign")]`; прочитать manifest только из `root / "source-copy-manifest.json"`.
7. После завершения единственного предлагаемого source diff подготовить **новый** `C:/xg/RMS01/aNNN/source-copy-manifest.json`: `id`, `core_commit`, SHA256 фактических `Program.cs`, `Runner.csproj`, `run.py`, а для каждого из 36 `src` файлов — `path`, исходный `git_blob_sha1` из соседнего manifest и `copied_sha256` после экспериментального изменения. Manifest фиксируется **последним**, после окончательного текста run.py из шага6; вычисление checksum не изменяет ни один охваченный файл. Нельзя брать checksum изменённого Linear/Program прежнего опыта. Одновременно сохранить `C:/xg/RMS01/aNNN/one-factor.diff` — unified diff исходного `00.TensorCore/Core/TensorOps.cs` против его экспериментальной копии; он включает только переключатель и две ветви mapping внутри этого метода, без других алгоритмических изменений. Его SHA256 также записать в manifest. Подготовка не запускает build, tests или обучение.
8. При одном запуске runner один раз сверяет свои подготовленные checksum, отклоняет существующие `results/bin/obj` и вызывает `result.mkdir(exist_ok=False)`. Только затем копирует уже подготовленный `root/one-factor.diff` в **`results/one-factor.diff`**, пишет source manifest в `results/source-copy-manifest.json` и выполняет одну сборку timeout120s. До соответствующего native процесса эксклюзивно создаёт `results/A` либо `results/B`; Program открывает конечный JSON через `FileMode.CreateNew`. Каждый из двух процессов имеет timeout60s, общий native deadline120s. Все эти служебные действия вне measured TRAIN.
9. Build stdout/stderr записать в `results/build.stdout`, `results/build.stderr`; native stdout/stderr — в `results/A/code-sign.stdout`, `results/A/code-sign.stderr` и аналогично B. `results/execution.json` содержит jobs, stage, status, exit code, actual counts из завершённого receipt и `no_auto_retry=true`. Build failure также записывает `results/status.json`. Если внешний timeout оборвал процесс до receipt, `actual_optimizer_steps = null` («не измерено»), не0; число нельзя получать ещё одним запуском. При известном build failure обучение не началось; это технический статус, не accuracy0. Любая ошибка заканчивает текущий run; нет повторов, cleanup и «починки» постороннего проекта.

После подготовки **единственная команда будущего исполнения** из нового attempt:

```powershell
Set-Location 'C:/xg/RMS01/a001'
python ./run.py
```

`a001` в этой буквальной команде — первая свободная попытка; при заранее определённом конфликте `run.py` создаётся и запускается в следующем `aNNN`. Внутри него команды определены однозначно:

```text
dotnet build Runner.csproj -c Release --output bin -p:ImportDirectoryBuildProps=false -p:ImportDirectoryBuildTargets=false
dotnet bin/RMS-COORD-ALLOC-01.dll A code-sign results/A/code-sign.json
dotnet bin/RMS-COORD-ALLOC-01.dll B code-sign results/B/code-sign.json
```

Каждое плечо — отдельный процесс с теми же seed/config/input. Не нужны NUnit, GitHub Actions, Windows solution tests, дополнительные seed, warmup-уроки, оценка других доменов, восстановление checkpoint или production-публикация. Если build/native deadline не выполнен, сохранить достигнутый этап и закончить без нового запуска. Эти подготовка и исполнение возможны лишь после отдельного поручения на этот опыт; текущий аудит ограничен документами.

## Измерение и критерий причинного сравнения

Стоимость измеряется **только вокруг четырёх настоящих TRAIN updates**: `GC.GetAllocatedBytesForCurrentThread`, `Stopwatch`, process CPU с `Refresh` перед вторым чтением, GC0/1/2. Подготовка, чтение файлов, сборка, hashes/readout и запись отчёта находятся вне этого участка и учитываются отдельно. Process peak working set — отдельная строка с честной областью «весь процесс до окончания TRAIN», не retained heap и не выделения.

A0/B0 и A1/B1 содержат по каждому EXAM 256 последних logits, argmax token, exact и target NLL. Сохранить4 online pre-step losses на плечо, actual optimizer step4, отпечатки trainable state до/после и optimizer moments после, frozen-state unchanged. `actual_parameter_write` должен подтверждаться различием записанных trainable hashes; сам счётчик Step не заменяет запись параметров. Структура:1 block,2 LoRA slots,1 learner, Flow отсутствует. Не делать новых hash-сверок ради оформления отчёта; использовать уже записанный raw.

Сравнение: `ΔA=A1−A0`, `ΔB=B1−B0`, `ΔB−ΔA`. Для NLL/стоимости меньше лучше, для exact больше лучше. TRAIN cost — две цены переходов A0→A1 и B0→B1, **не** нулевая стоимость «до» против ненулевой «после».

Условие валидности: одинаковые входные байты/config/начальный state; действительно достигнуты все4 optimizer steps на плечо; B меняет только mapping; EXAM не попал в TRAIN. Если это не выполнено — `INVALID_CAUSAL_COMPARISON`/partial, без вывода об эффекте. Ожидаемая численная эквивалентность — отдельный результат, не предлог отрицать измеренный выигрыш allocations. Любое ухудшение NLL, exact, CPU, wall или process peak показать рядом. Один короткий запуск не определяет устойчивую среднюю производительность; шум времени не объяснять без фактических данных.

## Отчёт и кандидат для решения Любы

Язык — русский; обычный Markdown, **`C:/xg/RMS01/aNNN/results/report.md`**, до350 слов плюс фактическая таблица. Скилы из уже прочитанной сессии: `architect-behavior-classifier-SKILL.md`, `planning-session-startup-SKILL.md`, `plan-review-SKILL.md`/фактически применённый plan-self-validation, `generated-code-review-SKILL.md`, `output-readability-gate-SKILL.md`; термины — `human-communication-GLOSSARY.md`. Для отчёта не искать другой шаблон/модель/навык и не перечитывать библиотеку.

Разделы: **что меняли → на чём проверяли → таблица A0/A1/B0/B1 и цены TRAIN → выигрыши и ухудшения → границы/кандидат**. При отсутствии значения писать «не измерено». Относительные Markdown links из `results/report.md`: `A/code-sign.json`, `B/code-sign.json`, `build.stdout`, `build.stderr`, `A/code-sign.stdout`, `A/code-sign.stderr`, соответствующие B logs, `execution.json`, `one-factor.diff` и `source-copy-manifest.json`. Не добавлять ещё один префикс `results/`. Отдельный `results/candidate.md` до120 слов — только при измеренном полезном улучшении: исходный метод и предлагаемый mapping diff, полезная ось, ухудшения, ссылка `[report](report.md)` из candidate. Успешный опыт становится кандидатом, основной код автоматически не меняется. Перенос и полная цепочка исправлений — по отдельному решению Любы.

После одного чтения созданного отчёта исправить только фактическую ошибку/битую ссылку/кодировку и закончить. Запрет на продолжение предыдущего опыта после команды перехода действует и на финальное оформление.

[trainer]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/04.TrainingLoop/Trainer.cs#L102-L145
[model]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/03.TransformerModel/LlmModel.cs#L219-L242
[block]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/03.TransformerModel/TransformerBlock.cs#L46-L64
[rms]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/03.TransformerModel/Layers/RmsNorm.cs#L29-L40
[broadcast]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/00.TensorCore/Core/TensorOps.cs#L168-L184
[coords]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/00.TensorCore/Core/TensorOps.cs#L1008-L1017
[backward]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/00.TensorCore/Core/TensorOps.cs#L187-L210
[dpo-forward]: https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/11.DPO/DpoTrainer.cs#L1018-L1030
[source-manifest]: https://github.com/lubochka/xiigen-planning/blob/f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/linear-transpose/scaffold/source-manifest.json
[host-program]: https://github.com/lubochka/xiigen-planning/blob/f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/linear-transpose/scaffold/experiments-v2/RP01-MICRO/NEURAL-FROZEN-TRANSPOSE-01/Program.cs
[host-project]: https://github.com/lubochka/xiigen-planning/blob/f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/linear-transpose/scaffold/35.ExperimentsV2/Experiments/Rp01Micro/NEURAL-FROZEN-TRANSPOSE-01/Runner.csproj
[host-run]: https://github.com/lubochka/xiigen-planning/blob/f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/linear-transpose/scaffold/run.py
