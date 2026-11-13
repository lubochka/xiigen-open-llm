# ATTN-TRANSPOSE-STRIDES-01 — координатные массивы при перестановке голов attention


**Позиция 39; NEXT = `JSON-OPTIONS-REUSE-01` (позиция 40).** Если назначен только один ID, после него завершить работу. Если уже назначена программа или дан переход «следующий», следовать этому literal NEXT из [реестра](04-execution-registry.md), без поиска другого опыта.

## Статус и приоритет

**`DESIGN_DOCUMENTED / TODO_IMPLEMENT_EXPERIMENT_HOST / NOT_RUN`.** Исходники прочитаны, один новый фактор отделён от соседних опытов, состав source-copy host зафиксирован. Новый `Program.cs`/runner ещё не реализован и не собран. Ни обучение, ни benchmark не запускались. Измеренного улучшения или доказанной причины плохих ответов сейчас нет. Это кандидат для малого исследования, не готовое исправление продукта.

Приоритет — **реальная стоимость обучения**, после разблокирования действующей учебной цепочки и устранения конфликтов активных карточек. Запускать только после назначения её ID в актуальном реестре; прежний `E-OPT-STEP` после позднего обновления уже выполнен и не назначается повторно. Старые эксперименты/результаты остаются. `docs/plans/replanning-possible-diffs` в core — только читать.

**Поздняя reconciliation:** во время аудита core перешёл на `827a1acdbef95d220886e54f9d36bce0b405c8cd`. [Проверены одинаковые Git tree SHA всех шести source поддеревьев host](ATTN-TRANSPOSE-STRIDES-01-LATE-SOURCE-IDENTITY.json), поэтому pin669 остаётся воспроизводимой версией тех же36 исходников. Наличие нового commit не означает, что этот опыт уже исполнен. Свежие protected AST/ROPE/BPE карточки проверены отдельно в [дополнении к novelty receipt](ATTN-TRANSPOSE-STRIDES-01-NOVELTY-799.json); перенос очереди/результатов учитывает827, source pin не меняется молча.

**Любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности для этого не обязателен. Рядом показываются изменения остальных показателей, включая ухудшения.** Для одиночной пары A/B различать `observed_metric_success` и `causal_attribution`: уменьшение измеренного времени остаётся наблюдаемым успехом времени, даже если устойчивое ускорение/его причина ещё не установлены. Успех не означает перенос в продукт.

## Запрет продолжать старый опыт

Обязательный протокол инцидента: пользователь сообщила, что агент **«четыре раза вместо переходя к следущему эксперименту продолжаешь прошлую задачу!!!»**, и потребовала **«Закончить немедленно!!!!!!»**. Недопустимо воспроизводить это поведение.

При новом прямом «стоп»/«закончить немедленно» немедленно прекратить действия, включая tools, отчёт, hash-проверки, review и git, и кратко сообщить уже известное состояние. При «следующий эксперимент» прекратить улучшать/проверять/дописывать предыдущий и перейти только к названному следующему. Не оправдывать продолжение незавершённым оформлением. Нельзя повторять запуск, стилистически полировать отчёт, удалять attempt, чинить прежний опыт или продолжать его после команды перехода. Цитата выше фиксирует прошлый инцидент; она не является текущей командой остановить документарный аудит.

## Доказательство участия в настоящем обучении

Все core-ссылки закреплены на **`66947785d1318f7cbbc88118f3156cdc75001e16`**.

| Звено | Что делает конкретный код и почему это относится к обучению |
|---|---|
| [Trainer:102–145][trainer] | Из учебных token IDs строит next-token targets, вызывает `Forward(training:true)`, loss, native backward, clipping, `AdamW.Step` и очищает градиенты. |
| [LlmModel:219–242][model] | Тот же forward проходит Transformer blocks, final norm и vocabulary head. |
| [CausalSelfAttention:129–170][attention] | Выходы Q/K/V проекций направлены в `Reshape4D`; attention context — в `Reshape3D`. При q/v LoRA меняются проекции, но перестановки остаются на действующем пути. |
| [Reshape4D/Reshape3D:323–352][reshape] | Переводят `[B,T,heads,D] ↔ [B,heads,T,D]` через `TensorOps.Transpose(x,1,2)`. |
| [TensorOps.Transpose:710–783][transpose] | Оптимизация последних двух осей не подходит rank4/axes1,2. Общая ветвь на **каждом scalar** вызывает `FlatToIndices`, переставляет два индекса и копирует float в новый буфер. |
| [FlatToIndices:1008–1017][coords] | Каждый вызов создаёт `new int[shape.Rank]`; для этих attention-тензоров это `int[4]`. |
| [Transpose backward:775–779][transpose-backward] | Включает тот же transpose для градиента в `BackwardFunc`, затем `AccumulateGrad`; служебная адресация повторяется и в достижимых обратных ветвях. |

Следовательно, это не отдельный тест голой перестановки: **учебные строки → native SFT/LoRA → Q/K/V и context transpose → backward → AdamW → реальные изменённые LoRA веса → readout до/после**. Наличие лишних временных массивов установлено статически; фактическая цена и влияние на доступный учебный бюджет ещё не измерены. Не все проекции обязаны иметь trainable backward в каждом профиле: число вызовов измеряется, а не предполагается одинаковым для frozen K и trainable Q/V.

## Почему это новая проверка в прочитанном корпусе

[Полный receipt поиска по 799 Markdown possibleDifs и 339 Markdown защищённого корпуса](ATTN-TRANSPOSE-STRIDES-01-NOVELTY-799.json) сохраняет запросы, количество файлов и совпадения. Для `ATTN-TRANSPOSE-STRIDES|Reshape4D|Reshape3D` совпадений нет; более широкий поиск проверен по смыслу. Это ограниченная проверка прочитанного snapshot, а не заявление об отсутствии идеи во всей истории Git.

В [защищённом обзоре 47][protected47] уже разобраны корректность перестановок и перенос градиента. **Новая гипотеза — ресурсная цена coordinate arrays внутри этих корректных active-activation перестановок.** `NEURAL-FROZEN-TRANSPOSE-01` кеширует транспонированные **замороженные матрицы весов** в `Linear`; `RMS-COORD-ALLOC-01` меняет indexing **broadcast умножения**; `GRADB-LOCALITY-01` меняет порядок доступа в **MatMul gradient**. Это другие владельцы, lifetime и вмешательства. Ни один из них не включается в плечо B здесь.

## Один фактор A/B и предлагаемый ремонт

**A:** исходный generic mapping `Transpose` на rank4 axes1↔2. **B:** для этого же случая вычислять адрес непосредственно целочисленными операциями, не выделяя `int[4]` на каждый scalar. Остальные shape/axes идут по исходному коду.

В **экспериментальную копию** `TensorOps.cs` добавить `internal static bool ExperimentAttentionTransposeOffsets`. После исходных ветвей identity и last-two-axes, перед generic `else`, вставить ветвь:

```csharp
else if (ExperimentAttentionTransposeOffsets && rank == 4 &&
         ((d1 == 1 && d2 == 2) || (d1 == 2 && d2 == 1)))
{
    int aSize = x.Shape.Dimensions[1];
    int cSize = x.Shape.Dimensions[2];
    int width = x.Shape.Dimensions[3];
    for (int flat = 0; flat < x.Shape.TotalElements; flat++)
    {
        if (flat > 0 && (flat & 4095) == 0)
        {
            TensorKernelProgress.Report("TensorOps.Transpose", flat);
            cancellationToken.ThrowIfCancellationRequested();
        }
        int d = flat % width;
        int remaining = flat / width;
        int c = remaining % cSize;
        remaining /= cSize;
        int a = remaining % aSize;
        int b = remaining / aSize;
        int destination = ((b * cSize + c) * aSize + a) * width + d;
        newData[destination] = x.Data[flat];
    }
}
```

У `[B,A,C,D]` source адрес — `((b*A+a)*C+c)*D+d`, target адрес после swap — `((b*C+c)*A+a)*D+d`. Та же формула применима на обратном transpose с обращёнными размерами. Это обоснование адресации, **не результат исполнения**.

Сохранить исходные normalization отрицательных осей, shape validation, `newData`, Tensor construction, `Children`, `BackwardFunc`, `AccumulateGrad`, float copies и порядок прохода `flat`. Cancellation/progress cadence остаётся на прежних flat offsets; существующие safeguards не удалять. Не вводить SIMD, pooled buffers, views, cache, новый objective, mask, RoPE или optimizer. `Linear.cs` берётся оригинальным из core, без frozen-transpose cache. В общую часть rank4 axes1↔2 можно добавить одинаковые для A/B два scalar счётчика `ExperimentEligibleCalls` и `ExperimentEligibleElements`; сброс вне измерений, на вызов один increment и прибавление `TotalElements`, без объектов/логирования в scalar loop. Они подтверждают фактическую достижимость и не подменяют bytes/time измерения.

## Фиксированная модель, данные и пределы домена

Исполнитель и последовательный проверяющий используют **модель своей сессии по умолчанию**, без override и поиска другой модели. Известное фактическое имя сохранить в metadata; если платформа не раскрывает — так и написать. Обучаемый Student — собственный native Transformer ниже. Проектная роль CODEX / Astra 6 ultracode для выбора кода/спецификации и оценки качества отделена от Student и от default исполнителя: в этом готовом tiny fixture нет вызовов внешнего judge или учителя. Генерация новых DPO-пар китайской open-моделью здесь не нужна; external calls0, DPO updates0. Не заменять Student внешним LLM.

| Поле | Зафиксированное значение |
|---|---|
| Student | `XIIGEN-native-1block-SFT-LoRA-64` |
| Config | Vocab256, Context64, Hidden64, 1 block, 4 Q heads, 2 KV heads, FFN176, RoPE10000, RMS eps1e−5 |
| Base initialization | `Initializers.UseDeterministicSeed(62001UL)` |
| LoRA | `ApplySeeded`, q_proj/v_proj, rank4, alpha8, dropout0, rootSeed62002, stream `attention-transpose-probe-v1` одинаков для A/B |
| Learning | `Trainer.TrainSupervisedNextTokenStep`, full-sequence shifted next-token SFT, weight1 |
| Optimizer | Native AdamW beta1=.9, beta2=.95, eps1e−8, weightDecay0 |
| Schedule | Warmup0, TotalSteps4, MaxLr=MinLr=.001, clip norm1, gradAccum1 |
| Tokens | UTF-8 bytes IDs0…255, zero BPE merges, no EOS, без смены tokenizer |
| TRAIN | 4 update/arm, порядок T1,T2,T1,T2; всего **8 optimizer updates** |
| Readout | В каждом A0/A1/B0/B1 ровно 2 TRAIN и 2 EXAM forward, **16 diagnostic forward** суммарно; backward на readout запрещён |
| Структура | 1 learner/block, 2 LoRA slots; Flow/units router не участвуют |

| Split | ID | Prompt буквально | Answer буквально |
|---|---|---|---|
| TRAIN | T1 | `C# result sign: 9-4 =` | `+` |
| TRAIN | T2 | `C# result sign: 4-9 =` | `-` |
| EXAM | E1 | `C# result sign: 8-3 =` | `+` |
| EXAM | E2 | `C# result sign: 3-8 =` | `-` |

Предметная область — программирование на C#, задача — знак целочисленного вычитания. Это базовые короткие случаи существующего host, не проверка общей компетенции или построения AST. Материалы документации, перевод и другие темы могут использовать тот же native learner и такие же tensor shapes, поэтому возможен ресурсный перенос; **они не измерены этой панелью**. Не объявлять улучшение docs/перевода без отдельного назначенного доменного опыта. Эффект зависит прежде всего от последовательности/heads/достижимости backward, а не от наличия синтаксического дерева.

## Точное место и состав будущего host

Рабочая платформа рецепта — Windows, уже используемый .NET9 SDK и Python. Единственный корень первой попытки — **`C:/x/at01/a001`**. Если он существует, сохранить его и завершить подготовку статусом `ATTEMPT_EXISTS`; автоматического удаления, продолжения и выбора новой папки нет. Новая попытка требует нового назначения. При отсутствии SDK/commit/исходника — назвать конкретную недостающую вещь и завершить этот опыт, не искать проект/версию/модель.

1. В новом attempt создать `src`, `Program.cs`, плоский `Runner.csproj`, `run.py`, `compare.py`. **Не создавать `results/bin/obj`** при preparation. Сейчас это `TODO_IMPLEMENT_EXPERIMENT_HOST`, не файлы, которые уже доступны для запуска.
2. [Соседний source manifest](ATTN-TRANSPOSE-STRIDES-01-SOURCE-MANIFEST.json) содержит **ровно36** core paths и Git blob SHA-1 текущего pin. Из `C:/Projects/llm_mvp_core` экспортировать эти пути через `git show 66947785d1318f7cbbc88118f3156cdc75001e16:<path>` в `src/<path>` с сохранением stdout bytes; рабочий checkout core не переключать и не редактировать. Все36 blobs совпали с inventory прежнего host. Этот факт касается исходников, не успешности сборки. Особенно `Linear.cs` — оригинал core, не patched copy старого опыта.
3. Использовать ровно [существующий Program][host-program] и [Runner.csproj][host-project] frozen-transpose host на planning **`53cbcabd3926cda714bab428d294eb666301e9e4`** как шаблоны; [старый source manifest][host-manifest] задаёт происхождение состава. Если источник уже есть в локальном planning checkout, экспортировать pin через `git show`, не брать незакоммиченный изменённый вариант. Никакого поиска иного учебного проекта.
4. В новом Program заменить ID/commit/LoRA stream значениями карточки. Удалить все обращения `Linear.ExperimentReuseFrozenTranspose`, `ExperimentCacheHits`, `ExperimentCacheMisses`, `ExperimentCachedFloats` и cache-readouts. Добавить только switch/same counters из раздела A/B. Для обоих A0/B0 switch=false; непосредственно перед TRAIN switch=`arm == "B"`, сохранить это значение до A1/B1. Ровно одна сборка содержит оба плеча; каждый arm — отдельный процесс.
5. Вместо прежнего `Observe(Exam)` сделать `Observe(Train)` и `Observe(Exam)` до/после. Старый `Observe` включает NLL и `ToArray`, поэтому разделить его явно: внутри readout cost-window выполнить ровно четыре native forward и удержать возвращённые Tensor; после закрытия окна получить из этих же Tensor argmax, target NLL, rank, `ToArray`/float bits и report DTO. Дополнительных inference для вычисления метрик нет. Собрать actual `optimizer.CaptureState()` вне TRAIN, начальные/конечные trainable и frozen arrays в стабильном порядке `model.Parameters()`. Не переносить `CaptureState` внутрь step loop.
6. Плоский project: `TargetFramework=net9.0`, `AssemblyName=ATTN-TRANSPOSE-STRIDES-01`, `EnableDefaultCompileItems=false`, Compile только `src/**/*.cs` и `Program.cs`, без ProjectReference. `ImportDirectoryBuildProps=false` и `ImportDirectoryBuildTargets=false` выставить **до SDK imports**; все outputs/intermediates/extensions внутри `C:/x/at01/a001/bin` и `/obj`. Не собирать solution/dispatcher, не запускать NUnit, CI или production tests.
7. Новый no-argument `run.py` основан на [существующем runner][host-run], но не читает legacy `experiment.json`/`--results-root`: literal `root=Path(__file__).resolve().parent`, `host=root/'Runner.csproj'`, `result=root/'results'`, `jobs=[('A','code-sign'),('B','code-sign')]`. Создать manifest **последним**, после правок: SHA256 `Program.cs`, `Runner.csproj`, `run.py`, `compare.py`, `one-factor.diff`, плюс 36 copied source SHA256 и исходные blob из соседнего manifest. `one-factor.diff` — только experiment switch/counters и rank4 branch в TensorOps. Никаких изменений других core методов. Подготовка не запускает build/training.
8. Runner один раз проверяет готовые checksums, отклоняет существующие `results/bin/obj`, затем эксклюзивно создаёт `results` и копирует туда prepared manifest/diff. Ровно один build, timeout120s; затем A и B, каждый timeout60s, общий native deadline120s. До создания модели Program эксклюзивно открывает output JSON `FileMode.CreateNew`; частичный файл после сбоя не удалять. Нельзя строить модель/обучать, а лишь затем проверять конфликт output. Стандартные stdout/stderr paths заданы ниже.
9. `compare.py` **только читает готовые raw JSON**, проверяет identity/float bit equality и считает deltas; ничего не запускает, не восстанавливает и не переобучает. Сохраняет новый `results/comparison.json` эксклюзивно. При неполном raw выдаёт PARTIAL и null для неизвестного, не accuracy0. После comparison ровно один русский отчёт из этих артефактов.

### Команды после отдельного назначения и завершённого preparation

```powershell
Set-Location 'C:/x/at01/a001'
python ./run.py
python ./compare.py
```

Внутри `run.py` команды фиксированы, cwd — этот attempt:

```text
dotnet build Runner.csproj -c Release --output bin -p:ImportDirectoryBuildProps=false -p:ImportDirectoryBuildTargets=false
dotnet bin/ATTN-TRANSPOSE-STRIDES-01.dll A code-sign results/A/code-sign.json
dotnet bin/ATTN-TRANSPOSE-STRIDES-01.dll B code-sign results/B/code-sign.json
```

Build logs — `results/build.stdout`, `results/build.stderr`; native logs — `results/A/code-sign.stdout`, `.stderr` и аналогично B. `results/execution.json`: status/stage/exit code/jobs/deadline/actual count из завершённого receipt/`no_auto_retry=true`. Сбой build — `results/status.json` с `BUILD_FAILED`, `BUILD_TIMEOUT` или `BUILD_TOOL_MISSING`; обучение не началось. Timeout native без полного receipt — actual steps `null`, не0. Ошибка заканчивает attempt: без cleanup, retry, смены SDK или ремонта production. Стоп/переход имеет приоритет и над записью этих файлов.

## Что измеряется и как сравнивается

**TRAIN cost — цена четырёх update A0→A1 и B0→B1**, а не фиктивный нулевой расход «до». Измерить `GC.GetAllocatedBytesForCurrentThread`, elapsed Stopwatch, process CPU с `Refresh` перед чтениями, GC0/1/2 строго вокруг четырёх native updates. Сборка, загрузка, диагностика, snapshots, serialization и отчёт — снаружи. Student/optimizer не пересоздавать между шагами. Не делать скрытых warmup updates, дополнительных seeds или повторов.

В каждой из четырёх диагностических точек A0/A1/B0/B1 измерить отдельно **readout allocation bytes** вокруг имеющихся TRAIN+EXAM forward, затем вне этого окна вычислить/записать метрики из их выходов. Не добавлять их к TRAIN cost. Их before/after deltas относятся к readout обученной модели, а не выдаются за стоимость update. `process_peak_working_set_bytes` — отдельно, область «процесс до конца TRAIN», не retained heap и не сумма allocations. Снять peak до after-readout и serialization.

| Данные в raw | Правило |
|---|---|
| TRAIN/EXAM по каждой строке в A0/A1/B0/B1 | Сохранить prompt, gold, predicted byte, exact, target NLL, target rank, 256 last logits. NLL — `logsumexp(last_logits)−target_logit`; rank — 1 + число logits больше target + число равных target logits с меньшим token ID. Argmax ties тоже по меньшему token ID. |
| Численная эквивалентность | Для того же forward сохранить shape и **float32 bit patterns всех logits**, а не только argmax. Для начальных/конечных trainable/frozen параметров — ordinal, shape, bits. Для optimizer — step, ordinal соответствующего parameter, bits обоих moments; пустой initial optimizer тоже записать. Эти массивы сохраняются за пределами TRAIN/readout cost окон. |
| Учебный переход | Ровно4 online pre-step losses, `actual_optimizer_steps=4`, `actual_parameter_write` из реального изменения trainable bits, frozen unchanged, counters eligible calls/elements отдельно для TRAIN. Счётчик Step один не доказывает обучение. |
| Парное сравнение | A0=B0 по model/config/data/state; A1=B1 по full outputs, trainable/frozen state и optimizer moments; все4 online loss bits равны. Comparator сравнивает записанные bits без новых model calls. |

Для каждого качества отдельно: `ΔA=A1−A0`, `ΔB=B1−B0`, разность `ΔB−ΔA`. TRAIN и EXAM, NLL, rank, exact не смешивать. Низкие NLL/rank/bytes/time лучше; exact выше лучше. При ответе в один byte argmax действительно задаёт весь ответ этого fixture; нельзя переносить это равенство на длинную генерацию.

Валидность причинного сравнения требует одинаковых inputs/seeds/model/init, 4 updates в каждом плече, actual parameter write, unchanged frozen state и единственного фактора. **Bit mismatch не скрывать**: это не подтверждённая точная оптимизация. Сохранить измеренные улучшения и ухудшения, установить `exact_optimization_supported=false`, объяснить изменение поведения; не продолжать «чинить» и не переименовывать mismatch в успех эквивалентности.

У результата две независимые оси: `observed_metric_success` (для каждой полезной метрики по её фактической дельте) и `causal_attribution` (`SUPPORTED_IN_THIS_PAIRED_RUN` / `NOT_ESTABLISHED` / `INVALID_COMPARISON`). Одиночный fixed-order A→B запуск не устанавливает устойчивое среднее время/CPU/peak; его полезная измеренная дельта всё равно фиксируется. Missing/failed values — «не измерено». Статически ожидаемое уменьшение числа массивов не заполняет измеренный allocated bytes.

## Отчёт, скилы и кандидат

Результат будущего исполнения — **`C:/x/at01/a001/results/report.md`**, русский Markdown, **до350 слов плюс таблица**. Разделы: что меняли; на чём проверяли; A0/A1/B0/B1; цена TRAIN; выигрыши и ухудшения; causal attribution/ограничения/кандидат. Таблица качества содержит по split/метрике A0,A1,B0,B1,ΔA,ΔB,ΔB−ΔA; отдельная таблица стоимости — A TRAIN,B TRAIN,B−A,observed success,attribution. В обеих нет придуманных чисел.

Использовать уже заданные project skills: `trainable-architecture-decision-SKILL.md`, `intent-to-plan-SKILL.md`, `plan-self-validation-SKILL.md`, `generated-code-review-SKILL.md`, `experiment-design-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `human-readable-architecture-audit-SKILL.md`, `human-communication-GLOSSARY.md` из `docs/skills` pinned core. Пользовательский малый бюджет и запреты выше общих skill-требований к production tests/promotion. Не искать ещё один формат, язык, модель или стек скилов.

Links из report: `A/code-sign.json`, `B/code-sign.json`, `comparison.json`, `execution.json`, `one-factor.diff`, `source-copy-manifest.json` и существующие logs без лишнего префикса `results/`. При любом измеренном полезном улучшении создать **`results/fix-candidate.md`**, до120 слов: исходный `TensorOps.Transpose`, proposed rank4 branch, метрика/домен/дельта, все известные ухудшения, attribution и `[report](report.md)`. Статус «кандидат для решения Любы», не prod-ready. Нельзя добавлять кандидат с выдуманным успехом, удалять прежние результаты, писать в protected каталог, автоматически переносить diff в original core или запускать полную цепочку тестов.

После одного чтения отчёта исправить только фактическую ошибку, битую ссылку или кодировку и закончить. Если пользователь уже велела следующий опыт — не выполнять и это чтение. Дальнейшие исправления original `00.TensorCore/Core/TensorOps.cs` и полная проверка возможны после её отдельного решения.

[trainer]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/04.TrainingLoop/Trainer.cs#L102-L145
[model]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/03.TransformerModel/LlmModel.cs#L219-L242
[attention]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/03.TransformerModel/Layers/CausalSelfAttention.cs#L129-L170
[reshape]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/03.TransformerModel/Layers/CausalSelfAttention.cs#L323-L352
[transpose]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/00.TensorCore/Core/TensorOps.cs#L710-L783
[coords]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/00.TensorCore/Core/TensorOps.cs#L1008-L1017
[transpose-backward]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/00.TensorCore/Core/TensorOps.cs#L775-L779
[protected47]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/47-current-attention-backprop-and-position-contract.md#L30
[host-program]: https://github.com/lubochka/xiigen-planning/blob/53cbcabd3926cda714bab428d294eb666301e9e4/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/linear-transpose/scaffold/experiments-v2/RP01-MICRO/NEURAL-FROZEN-TRANSPOSE-01/Program.cs
[host-project]: https://github.com/lubochka/xiigen-planning/blob/53cbcabd3926cda714bab428d294eb666301e9e4/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/linear-transpose/scaffold/35.ExperimentsV2/Experiments/Rp01Micro/NEURAL-FROZEN-TRANSPOSE-01/Runner.csproj
[host-manifest]: https://github.com/lubochka/xiigen-planning/blob/53cbcabd3926cda714bab428d294eb666301e9e4/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/linear-transpose/scaffold/source-manifest.json
[host-run]: https://github.com/lubochka/xiigen-planning/blob/53cbcabd3926cda714bab428d294eb666301e9e4/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/linear-transpose/scaffold/run.py
