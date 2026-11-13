# RMS-ROW-REDUCE-01 — цена вычисления среднего внутри настоящего SFT/LoRA обучения

Дата: 20.10.2026. Исходный core: `66947785d1318f7cbbc88118f3156cdc75001e16`. Статус: **гипотеза подтверждена как достижимый участок кода; влияние ещё не измерено; новый host не создан и не собран**. Текущая работа только документирует опыт. Эта карточка не объявляет найденной причину нулевых свободных ответов RP01.

**Любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности для этого не обязателен. Рядом показываются изменения остальных показателей, включая ухудшения.** Для этого опыта предполагаемая полезная ось — выделенные байты на настоящий учебный переход; точность, NLL и фактическое состояние ученика остаются отдельными измерениями.

**Стоп и переход имеют немедленный приоритет.** После команды «стоп» / «закончить немедленно» прекратить действия, включая инструменты, запись отчёта, проверку и коммит. Ответить кратко уже известным состоянием. После команды «следующий эксперимент» прекратить дорабатывать прежний опыт и перейти только к названному следующему; сохранение или оформление прежнего результата не создаёт исключения. Старые эксперименты, результаты и незавершённые попытки не удалять. Никаких повторов ради более красивого результата.

## 1. Что проверяется и где это участвует в обучении

Вычисление RMS-нормализации вызывает `Mean(..., axis:-1, keepDims:true)` для каждой строки скрытых признаков. Текущий общий алгоритм суммирования сначала создаёт массив координат строки, затем **копирует этот массив для каждого складываемого элемента**. Для используемой последней оси все элементы строки уже расположены подряд; её начало вычисляется умножением номера строки на ширину. Гипотеза: адресация через последовательный индекс уменьшит выделения памяти и, возможно, время реального обучения, сохранив математическую операцию и полученное состояние.

| Звено реального учебного пути | Доказательство на закреплённом core |
|---|---|
| Данные урока → forward → CE → backward → clipping → AdamW | [`Trainer.TrainSupervisedNextTokenStep`, 102–147][trainer] |
| Forward проходит все блоки и final norm | [`LlmModel.Forward`, 219–242][model] |
| В каждом блоке две RMS-нормализации | [`TransformerBlock.Forward`, 46–64][block] |
| Нормализация действительно использует Mean последней оси | [`RmsNorm.Forward`, 29–40][rms] |
| Mean вызывает SumAlongAxis, затем делит сумму на axisSize; граф backward сохранён | [`TensorOps.Mean`, 530–586][mean] |
| Массив строки и отдельная его копия на каждое слагаемое | [`SumAlongAxis`, 1020–1047][sum] и [`FlatToIndices`, 1008–1018][flat] |

Получатель — настоящий собственный C# Transformer с q/v-LoRA. Тема обучения — язык программирования C#; узкая задача — определить знак целочисленного вычитания. Это не отдельный домен «RMS» и не kernel-only benchmark. Ученик обязан пройти реальное обучение; замер одних индексов, forward без update либо Python-формулы не отвечают на вопрос.

Этот механизм существует также на neural DPO forward, но карточка измеряет только SFT/LoRA: DPO updates — 0. Он не участвует в чистом kNN-пополнении памяти и не доказывает перенос эффекта на kNN, другие размеры модели или другие предметные темы.

## 2. Новизна и близкие прежние работы

Проверены все **799 Markdown possibleDifs** на planning `53cbcabd3926cda714bab428d294eb666301e9e4` и **155 Markdown каталога protected cards**, включая индекс (154 карточки), на core `66947785d1318f7cbbc88118f3156cdc75001e16`. Exact `SumAlongAxis` и `RMS-ROW` отсутствуют; семь широких совпадений отдельно рассмотрены. [Квитанция поиска](../evidence/rms-row-reduce-novelty.json).

- Прежний `RMS-COORD-ALLOC-01` изменяет `MultiplyBroadcast` и **явно исключает изменение Mean**. Это родственное семейство затрат адресации, но другой выполняемый метод и отдельный фактор. Не запускать оба фактора одновременно.
- `N-SHAPE-01` описывает ошибку backward при `keepDims:false`; здесь используется штатный `keepDims:true`, backward остаётся исходным. Исправление отсутствующей оси не входит в этот опыт.
- `GRADB-LOCALITY-01` меняет обход градиента правой матрицы MatMul; здесь порядок float-суммирования Mean и весь MatMul сохраняются.
- Подготовка/проверка новизны не является измеренным успехом. Поиск покрывает указанный Markdown-корпус и карточки, а не неизвестные неопубликованные работы и не все исторические ветки.

## 3. Единственный фактор A/B и адрес будущего исправления

**A:** исходный `SumAlongAxis`.

**B:** только при последней оси, сохранённом числе осей и результирующей последней размерности 1 — последовательное чтение той же строки. Остальные формы исполняют исходный код. Суммирование остаётся скалярным слева направо; никаких SIMD, параллельной редукции или перестановки слагаемых.

В экспериментальную копию `00.TensorCore/Core/TensorOps.cs` добавить `internal static bool ExperimentUseContiguousLastAxisSum`. В начале `SumAlongAxis` перед исходным циклом добавить только следующую ветвь:

```csharp
if (ExperimentUseContiguousLastAxisSum &&
    axis == srcShape.Rank - 1 &&
    srcShape.Rank == dstShape.Rank &&
    dstShape.Dimensions[axis] == 1)
{
    int axisSize = srcShape.Dimensions[axis];
    for (int dstFlat = 0; dstFlat < dstShape.TotalElements; dstFlat++)
    {
        cancellationToken.ThrowIfCancellationRequested();
        int start = dstFlat * axisSize;
        float sum = 0f;
        for (int k = 0; k < axisSize; k++)
        {
            if ((k & 4095) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            sum += src[start + k];
        }
        dst[dstFlat] = sum;
    }
    return;
}
```

Исходный цикл `SumAlongAxis` после этой ветви остаётся полностью. Не менять `Mean`, деление на axisSize, `Pow`, `RSqrt`, `MultiplyBroadcast`, `BroadcastGradAlongAxis`, конструкторы Tensor, `RequiresGrad`, LoRA, optimizer, данные или readout. В обоих плечах перед A0/B0 переключатель false; непосредственно перед TRAIN он получает `arm == "B"` и сохраняется до after-readout. Переключатель — инструмент изолированного опыта, а не предлагаемая конфигурация production.

Обоснование эквивалентности — вывод из адресации: при последней сохранённой оси источник текущего `(dstFlat,k)` равен `dstFlat*axisSize+k`; сохраняется `sum +=` в прежнем порядке и прежнее деление в Mean. Это ещё не native-проверка побитового равенства. Его проверяют **по уже произведённому учебному состоянию**, не дополнительным benchmark.

Адрес возможного переноса после решения Любы — этот же `TensorOps.SumAlongAxis`. Перенос в original source, исправление продуктовых тестов и полная цепочка CI сейчас не разрешены и не являются частью малого опыта.

## 4. Модель, данные и жёсткий бюджет

Исполнитель использует ту же модель и режим, что уже выбраны в назначенной сессии. Внешний teacher/judge не нужен; модель исполнителя и обучаемый собственный Transformer — разные сущности.

| Поле | Фиксированное значение |
|---|---|
| Native config | Vocab256, Context64, Hidden64, один блок, 4 Q heads, 2 KV heads, FFN176, RopeTheta10000, RmsNormEps1e−5 |
| Base initialization | `Initializers.UseDeterministicSeed(62001UL)` отдельно в каждом процессе |
| LoRA | `ApplySeeded`, q_proj/v_proj, rank4, alpha8, dropout0, seed62002UL, stream `rms-row-reduce-probe-v1` |
| Optimizer | `AdamW(.9f,.95f,weightDecay:0f,eps:1e-8f)` |
| TrainingConfig | Batch1, SeqLen64, warmup0, totalSteps4, maxLr=minLr=.001, gradAccum1, clip1, weightDecay0 |
| Токены | UTF-8 byte IDs 0…255 через `Encoding.UTF8`; BPE Train/Encode не вызываются; EOS не добавляется |
| Objective | Текущий full-sequence next-token SFT, weight1; маска ответа не добавляется |
| TRAIN | T1,T2,T1,T2: четыре вызова `TrainSupervisedNextTokenStep` на плечо; всего восемь updates |
| EXAM | Два prompt forward до и два после в каждом плече; всего восемь EXAM forward; argmax фактического последнего логита среди 256 |
| TRAIN diagnostic readout | Те же T1/T2 prompt отдельно до и после каждого плеча, без update: восемь дополнительных forward. Всего 16 readout forward:8 EXAM+8 TRAIN diagnostic |
| Другие вызовы | DPO0, внешние модели 0, warmup0, retries0, дополнительных seed0 |

| Split | ID | Prompt буквально | Answer буквально |
|---|---|---|---|
| TRAIN | T1 | `C# result sign: 9-4 =` | `+` |
| TRAIN | T2 | `C# result sign: 4-9 =` | `-` |
| EXAM | E1 | `C# result sign: 8-3 =` | `+` |
| EXAM | E2 | `C# result sign: 3-8 =` | `-` |

Это две учебные пары и два других входа для чтения результата, а не доказательство освоения арифметики или программирования вообще. Ответ `+`/`-` нельзя вычислять правилом вместо модели.

## 5. Подготовка без поиска места, model и способа запуска

Статус этого раздела: **рецепт реализации host; отсутствующий host не объявлен готовой командой**. Подготовка и native execution выполняются при назначении этой карточки, в этой сессии обучения не запускали.

1. Исходный checkout — `C:/Projects/llm_mvp_core`, read-only core pin указан выше. Единственный короткий корень — `C:/xg/RD01/a001`. Если каталог уже существует, сохранить его и прочитать имеющийся status; не создавать a002/другую попытку и не перезапускать опыт лишь из-за конфликта каталога. Нужное новое исполнение определяется отдельной прямой командой пользователя, а не автоматической логикой. Старые файлы не удалять. При отсутствии .NET9 SDK записать конкретный технический блокер, не переходить к установке другого toolchain.
2. Создать в новой попытке только `src`, `Program.cs`, `Runner.csproj`, `run.py`, `one-factor.diff`, `source-copy-manifest.json`. `results` создаёт runner эксклюзивно; `bin/obj` создаёт сборка. Из core скопировать **ровно 36 файлов** [source manifest](../evidence/rms-row-reduce-source-manifest.json), сохраняя относительные пути внутри `src`. **До копирования и patch проверить actual Git blob SHA каждого из 36 исходных файлов против manifest.** Получать исходные bytes через `git -C C:/Projects/llm_mvp_core show 66947785d1318f7cbbc88118f3156cdc75001e16:<path>` как argv subprocess без shell, stdout в bytes, timeout10s на чтение; не применять CRLF-преобразования рабочего checkout. Точный Python hash: `hashlib.sha1(b"blob " + str(len(source_bytes)).encode("ascii") + bytes([0]) + source_bytes).hexdigest()`. Несовпадение или отсутствующий объект — конкретный PRECONDITION_FAILED до build/Train, без отключения guard и поиска другого source. Prepared SHA256 после patch не заменяет эту original-blob проверку. Manifest закреплён на current core; все 36 blob совпадают с составом существующего проверенного scaffold. Не заменять original Linear на cached variant.
3. Основа `Program.cs` — [точный опубликованный SFT host][host]. Сохранить model config, literal TRAIN/EXAM, updater, timing и readout. Поменять ID на `RMS-ROW-REDUCE-01`, commit на current pin, stream на `rms-row-reduce-probe-v1`. Заменить два переключения `Linear.ExperimentReuseFrozenTranspose` на `TensorOps.ExperimentUseContiguousLastAxisSum`; удалить чтение `ExperimentCacheHits/Misses/CachedFloats` и связанные cache-поля. В `mechanism` записать только `contiguous_last_axis_sum_enabled = arm == "B"`. Включить `subject = "programming-language-csharp"`, `task = "integer-subtraction-sign"`. `training_losses` подписать как online loss перед соответствующим update. Изменить helper на `Observe(LlmModel model, (string Prompt, string Answer)[] rows)`, заменив внутри `Exam.Select` на `rows.Select`. Для каждого row после получения target добавить `gold_rank = 1 + количество last_logits строго больше last_logits[target]`, `teacher_forced_exact = best == target`. До TRAIN вызвать `Observe(model, Exam)` и `Observe(model, Train)`, после TRAIN аналогично; записать `before/after` для EXAM и `training_panel_before/training_panel_after` для TRAIN. Все 16 readout forward находятся вне измеряемого TRAIN; не вычислять before/after разностью online losses разных шагов.
   В наследованном receipt заменить `domain = args[1]` на `profile = args[1]` и `domain = "programming-language-csharp"`; `code-sign` — имя профиля, не предметный домен. Заменить `byte_tokenizer = "...exactly native BPE with zero merges..."` на `"UTF-8 byte IDs 0..255 via Encoding.UTF8; no BPE Train/Encode; no EOS"`. Заменить `unit_count` на `native_learner_instances=1`, добавить `logical_unit_flow_scope="not used"`: wrapper обучает Transformer напрямую и не вызывает LogicalUnit/Flow. Это уточнение фактически выполненного пути, без изменения учебного фактора.
4. Новый плоский `Runner.csproj` содержит OutputType Exe, TargetFramework net9.0, ImplicitUsings enable, Nullable enable, EnableDefaultCompileItems false, AssemblyName `RMS-ROW-REDUCE-01`; Compile только `src/**/*.cs` и `Program.cs`, никаких ProjectReference. До SDK imports выставить `ImportDirectoryBuildProps/Targets=false`; BaseOutputPath/IntermediateOutputPath/MSBuildProjectExtensionsPath привязать к `bin/obj` внутри attempt. Существующий глубокий `ArtifactRoot` не наследовать.
5. `run.py` — новый no-argument runner с `root=Path(__file__).resolve().parent`, `result=root/'results'`, host=`root/'Runner.csproj'`. Один раз проверить prepared SHA256, отсутствие `results/bin/obj`; `result.mkdir(exist_ok=False)`; копировать prepared manifest/diff в results; одна сборка timeout120s; процессы A затем B, timeout60s каждый, общий native deadline120s. До соответствующего процесса создать `results/A` или `results/B`; конечный JSON открывается FileMode.CreateNew. Не использовать старые `--results-root`, `experiment.json` либо автоматический поиск checkout.
6. После финализации Program/runner/diff записать source-copy-manifest: core commit, path и original blob из соседнего manifest, SHA256 фактических 36 копий, Program, csproj, run.py и diff. Checksum не изменяет охваченные файлы. В обоих плечах одна сборка и один фиксированный A/B-переключатель; никаких изменений чужих проектов.

Единственная внешняя команда после подготовки новой первой попытки:

```powershell
Set-Location 'C:/xg/RD01/a001'
python ./run.py
```

Другую попытку этот рецепт автоматически не выбирает. Внутри runner — строго:

```text
dotnet build Runner.csproj -c Release --output bin -p:ImportDirectoryBuildProps=false -p:ImportDirectoryBuildTargets=false
dotnet bin/RMS-ROW-REDUCE-01.dll A code-sign results/A/code-sign.json
dotnet bin/RMS-ROW-REDUCE-01.dll B code-sign results/B/code-sign.json
```

Build logs: `results/build.stdout`, `build.stderr`; native logs: `results/A/code-sign.stdout`, `.stderr` и B counterparts. `execution.json` фиксирует stage, status, exit code, actual counts из существующего receipt и `no_auto_retry=true`. При build failure обучение не началось — это технический статус, не accuracy0. При timeout до receipt счётчик updates — null/«не измерено»; нельзя запускать ещё раз, чтобы узнать число. Папка protected read-only, включая результаты.

Экспериментальные copies, tiny literal fixtures и один переключатель допустимы только здесь; они не разрешают менять production или подменять настоящий learner на surrogate. Нет широких unit tests, solution build, Windows CI, Main500, новых BPE/KNN/restore опытов или исправления продуктовых тестов до доказанного результата.

## 6. Измерение до/после и валидность

Измерить два перехода A0→A1 и B0→B1, а не нулевую цену before против ненулевой цены after. Primary cost scope — четыре настоящих TRAIN вызова; в него входят forward/backward/clipping/AdamW. Сохранить allocated bytes (`GC.GetAllocatedBytesForCurrentThread`), elapsed (`Stopwatch`), process CPU с Refresh, GC0/1/2. Whole-process peak working set — отдельная величина, не retained heap и не сумма allocated bytes. Build, readout, hashes и сериализация снаружи TRAIN.

Для A0/B0 и A1/B1 сохранить каждый EXAM prompt и каждый фиксированный T1/T2 prompt,256 last logits, predicted token, teacher-forced exact, target NLL и gold rank (1 плюс число логитов строго выше gold). TRAIN diagnostic readout выполняется без update и вне TRAIN cost; он показывает before/after на одной и той же панели, тогда как online losses разных шагов её не заменяют. Зафиксировать четыре online losses на плечо; реальные optimizer steps4; trainable hashes до/после, optimizer moments hash после, frozen hashes без изменений. Размер модели/Unit/Flow отчитать прямо: один Transformer block, два LoRA slots, один neural learner, Flow не участвует. `actual_parameter_write` требует изменённого trainable state; счётчик шагов сам по себе его не доказывает.

Валидность: одинаковые начальные states/config/data; только одна source-интервенция; все 4 actual updates на плечо; EXAM не обучает; after снимается после завершения updates. Сравнить ΔA=A1−A0, ΔB=B1−B0 и ΔB−ΔA. Для NLL и cost меньше лучше; для exact больше лучше. Ожидается одинаковое состояние A1/B1 и полезная экономия allocations. Если возникло численное расхождение, сохранить его и ограничить утверждение: нельзя называть замену семантически нейтральной без разбора, но фактическое улучшение отдельной полезной оси не скрывать.

Ни один выигрыш пока не измерен. Один фиксированный порядок A→B не доказывает устойчивое причинное ускорение по wall time; любые фактические timings показываются с этим ограничением. Изменение RAM, CPU, NLL, exact и GC, включая ухудшения, обязательно рядом.

## 7. Отчёт и следующий шаг

Порядок в объединённом реестре: `VT-APOSTROPHE-01` → **`RMS-ROW-REDUCE-01`** → `IDENTITY-M1-01`. `NEXT = IDENTITY-M1-01` после закрытия этой карточки — указатель следующей работы, не разрешение автоматически запускать второй опыт. Прямая команда пользователя о следующем эксперименте имеет приоритет.

Русский Markdown, `C:/xg/RD01/a001/results/report.md`, максимум 350 слов плюс таблица. Готовые разделы: «Что меняли»; «Тема, задача и ученик»; «A0/A1/B0/B1 и стоимость TRAIN»; «Что улучшилось и ухудшилось»; «Ограничения и кандидат». Неизмеренное — «не измерено», не 0. Links относительно results: `A/code-sign.json`, `B/code-sign.json`, `execution.json`, `one-factor.diff`, `source-copy-manifest.json`, `build.stdout`, `build.stderr` и A/B logs. Не добавлять второй `results/` в ссылки.

При измеренном улучшении полезной оси — `results/candidate.md` до 120 слов: source `TensorOps.SumAlongAxis`, предлагаемый last-axis diff, ось/домен, все существенные ухудшения, `[report](report.md)`. Кандидат требует решения Любы для переноса; прежние успехи и эксперименты не удалять. После одного чтения готового отчёта исправить только факт/ссылку/кодировку и закончить. Новые запуски и оформление до бесконечности запрещены.

Скилы для исполнения заданы прямо: `docs/skills/architect-behavior-classifier-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `code-examination-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `human-readable-architecture-audit-SKILL.md`, `human-communication-GLOSSARY.md`; применяются из уже назначенного core pin. Прямые текущие правила пользователя выше исторических требований mandatory accuracy/порогов и полной production-проверки. Для отчёта повторно искать skills, шаблон, язык или модель не нужно.

[trainer]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/04.TrainingLoop/Trainer.cs#L102-L147
[model]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/03.TransformerModel/LlmModel.cs#L219-L242
[block]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/03.TransformerModel/TransformerBlock.cs#L46-L64
[rms]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/03.TransformerModel/Layers/RmsNorm.cs#L29-L40
[mean]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/00.TensorCore/Core/TensorOps.cs#L530-L586
[sum]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/00.TensorCore/Core/TensorOps.cs#L1020-L1047
[flat]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/00.TensorCore/Core/TensorOps.cs#L1008-L1018
[host]: https://github.com/lubochka/xiigen-planning/blob/53cbcabd3926cda714bab428d294eb666301e9e4/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/linear-transpose/scaffold/experiments-v2/RP01-MICRO/NEURAL-FROZEN-TRANSPOSE-01/Program.cs
