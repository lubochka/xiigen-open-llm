# DPO-TOKENIZER-SNAPSHOT-01: одна закрытая копия токенизатора на подготовку двух учебных пар

**Статус: DESIGNED_NOT_RUN — спроектировано; host ещё требует реализации.** Выполнен анализ исходного механизма и сопоставление с прежними карточками. Обучение, измерение выигрыша и изменение продукта не выполнялись. Эта карточка — добавленный кандидат, не доказанное объяснение неудачи первого PR.

## Механизм и связь с реальным обучением

Тема — документация программ; задача — описать порядок аргументов вычитания в C#. Учится исходный `TransformerModel.LlmModel`, профиль `rp01-csharp-small/v1`, через q/v-LoRA и AdamW. Изменяется подготовка точных текстовых предпочтений, которые **этот же learner затем потребляет в настоящем StepVector**.

Для каждой пары `DpoExactPairMaterial.CreateBatch` сериализует весь BPE tokenizer в MemoryStream, загружает закрытую копию и вычисляет её canonical SHA256. `LogicalDpoTrainingUnit.PrepareExecution` вызывает этот метод внутри цикла по preference rows. При двух парах с одним неизменяемым источником получаются два Save/Load/hash прохода одного tokenizer. Они сохраняют важную гарантию: кодирование сегментов одной пары нельзя менять внешним изменением tokenizer.

Гипотеза: один **закрытый снимок на одну подготовку** может сохранить точный материал всех пар при меньшей цене подготовки. Удалять саму защиту freeze, кэшировать mutable tokenizer глобально или использовать только указатель на внешний объект нельзя.

### Исходные доказательства

Ветка `codex/dpo-implementation`, commit `ab583032562d6656de9dd5ca6fa0a325fd4b1f30`.

| Source | Участие в учебном переходе |
|---|---|
| [DpoExactPairMaterial.cs:64–90](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/11.DPO/Models/DpoExactPairMaterial.cs#L64-L90) | Проверка текстов, Save→Load, три сегментных Encode, hash, response masks и detached tensors для одной пары. |
| [PrepareExecution:1667–1759](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L1667-L1759) | Per-pair bounds/authorship/vector checks, затем BuildBatch внутри цикла. |
| [BuildBatch:2351–2380](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L2351-L2380) | Typed admission и test observer идут до вызова exact CreateBatch. Порядок нельзя бездумно переставлять. |
| [LogicalDpoPreparedBatchMaterializer.cs:14–21](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/18.CapabilityTraining/LogicalDpoPreparedBatchMaterializer.cs#L14-L21) | Реальный public caller явно выбирает exact material version; generic PrepareExecution по умолчанию legacy. |
| [CodeDocumentationSimulationDpoExperiment.cs:106–125](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L106-L125) | Saved B0 load → programmatic typed authors → governance/content projection → **Prepare**. |
| [Тот же owner:175–189](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L175-L189), [DpoTrainer.cs:832–877](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/11.DPO/DpoTrainer.cs#L832-L877), [ApplyOptimizer:1219–1225](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/11.DPO/DpoTrainer.cs#L1219-L1225) | Prepared batches действительно поступают в policy/reference forward, backward и исходный optimizer update. |

Изолированное измерение сериализации tokenizer без завершённого учебного перехода не закрывает карточку. При отказе подготовки/lease/update это диагностические данные, а не успешное обучение.

## A/B: ровно один фактор

**A:** исходная закрытая копия + hash на каждую exact pair.

**B:** lazy private `FrozenTokenizerSnapshot` живёт только в одном `PrepareExecution`; после typed admission и исходных CheckText первой допустимой exact pair создаётся один clone с hash. Следующие допустимые пары используют этот закрытый clone. Каждая по-прежнему независимо проверяется, получает собственные IDs/masks/tensors/content digest. Оригинальный путь отдельного CreateBatch и legacy version остаются прежними.

Не создавать снимок в начале PrepareExecution: это перенесло бы токенизацию до отрицательного authorship/text guard. Не менять порядок `BatchTokenizationObserver`. Для активного internal observer сохранить исходный per-pair путь, без reuse; в данном обычном учебном caller он отсутствует. В эксперименте tokenizer принадлежит одному arm, не обучается заново и не изменяется параллельно. Эквивалентность для стороннего concurrent mutation не доказана; это отдельный риск будущего переноса, а не повод выдумывать lock/callback обход в опыте.

Точная спецификация меняемых методов, assembly boundary и неизменных authority calls — [dpo-source-copy-design.md](dpo-source-copy-design.md). Только копии `11.DPO/Models/DpoExactPairMaterial.cs` и `18.CapabilityTraining/LlmIngredientLogicalUnits.cs` получают treatment. Нельзя одновременно менять reference cache, optimizer exports, gradient копии, objective, response sum/mean, masking, padding, LoRA rank или training vector.

## Данные и неизменяемый learner

Точные две JSONL строки — [fixtures/dpo-tokenizer-snapshot.jsonl](fixtures/dpo-tokenizer-snapshot.jsonl), UTF-8 без BOM, с одним конечным LF. SHA256: `91f9409b03dce081e9cd8f431f52fa4157715c44c9d8d22d561fe87fd79a8a95`. Первая задача `first-second`, вторая `second-first`; chosen/rejected меняются местами вместе со смыслом. Перед префиксом или после `Code: ` ничего не добавлять. Никакие id/provenance/labels не поступают в модель как текст.

В обоих arms **одно** `Prepare(preferencesOfTwo, tokenizer)`, затем **одно** `StepVector(allTwo, step:1)`. Последовательные два Prepare по одной строке уничтожили бы проверяемую экономию. Четыре предъявления пар и два optimizer updates суммарно A/B; оба arms начинают с одной полной сохранённой B0, с независимым frozen reference.

| Параметр | Точное значение |
|---|---|
| Канонический источник | `C:/Projects/llm_mvp_core`; `tools/Rp01SimulationDpo/Program.cs:307–332` ReadConfiguration + LoadModels, без вызова большого Run |
| Конфигурация | `config/code-documentation/generation-v1.json`, hash файла `e1f960b8c7d883f84c9b4e55d24c7284794cee2fecfd71beac5835d199746f40` |
| Начальные веса | B0 SHA256 `a65887cc9d97c793d00e77632013adeba5e2d36efd7e6cebe7e49ba8a9b37911` |
| B0 manifest | `c5f4f7da80f427b8e9aac1dabded5849f0cbc6cb444b99d2bf7daac44ec0246d`, Step1, ParentCheckpoint=null, InitialBaselineNoUpdate |
| Tokenizer | canonical SHA256 `e51c91d85ec486863f1862e49b378fa9f34873ee31a993114ba41995f38e5066`; vocab629 |
| Transformer | context2048, hidden512, layers4, heads8, kvHeads2, FFN1408, RoPE10000, RMS epsilon1e-5 |
| LoRA | rank8, alpha16, dropout0, q_proj/v_proj; 55 named parameter tensors: 39 base +16 adapters |
| Seeds | root initialization20261014; `rp01-policy` / `rp01-reference`; реальные RNG cursors сохраняются отдельно |
| Optimizer/objective | исходный CreateForProjectLora, LR1e-5, DPO beta0.1, response-sum-softplus-signal-weighted-v3; фактические AdamW beta/epsilon/decay записать без переопределения |
| Исходный optimizer | новая сессия над B0, step0 и пустые моменты; не выдавать manifest Step1 за optimizer step1 |

Источники конфигурации: [generation-v1.json](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/config/code-documentation/generation-v1.json), [constants:14–17](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/tools/Rp01SimulationDpo/Program.cs#L14-L17), [saved-root loader:307–332](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/tools/Rp01SimulationDpo/Program.cs#L307-L332).

Physical B0 path берётся только из проверенного ParentManifest.RepositoryRelativeWeightsPath через этот loader. Если файл/config/binary boundary отсутствует или identity не совпадает, сохранить ошибку и завершить назначенную попытку. Не искать соседний checkpoint, не уменьшать модель, не обучать random fallback, не заменять B0 C50. Доступность Windows файлов здесь не проверялась.

## Фазы и честные показатели до/после

Перед MeasurePrepare tokenizer и models загружены одинаково; материализация измеряется отдельно от загрузки. В A0/B0 записать named policy/reference hashes, optimizer step0/state bytes, RNG state, tokenizer hash. Native Prepare измерить один раз; его результат напрямую передать единственному StepVector. В конце сохранить A1/B1 и reopen через исходные codecs без нового forward.

| Показатель | Как считать |
|---|---|
| Primary полезная стоимость | Allocated bytes текущего потока, CPU, elapsed **за единственный Prepare**, внутри той же completed learning attempt. A имеет два capture, ожидаемый B — один. Счётчик capture — объяснение работы, не сам полезный результат. |
| Полный учебный переход | Load/materialize, полный StepVector и native checkpoint/readback — отдельные фазы; полный elapsed/CPU/allocations дополнительно. Нельзя складывать вложенную фазу с её родителем. |
| Фактическое обучение | optimizer step0→1; actual keyed LoRA delta, включая возможный ноль; base/reference до/после неизменны; реально completed StepVector, не только issued. Нулевой численный delta записывается как отсутствие изменения весов, а не выдуманный рост качества; он не отменяет реально исполненную работу optimizer. |
| Сохранность точных входов | Два material digests, tokenizer SHA, lengths, actual IDs и masks A/B; bounds/authorship/response-only проверки должны пройти. |
| Восстановление | Записанные private candidate weights/optimizer/RNG прочитаны теми же codecs и равны live состоянию; это readback одного update, не новая continuation-серия. |
| Учебная метрика DPO | Вернуть actual loss/margin из StepVector с подписью **измерено до применения этого update**. Это не оценка A1/B1. |
| Прочее | Peak working set процесса, GC counts, actual runtime/build; accuracy документации, held-out ответы и throughput полного продукта — «не измерено». |

Новые Generate/Evaluate/teacher/scoring calls, warmup, calibration, 50/500-step цепочки и повторные учебные запуски — **ноль**. У опыта конечный бюджет два StepVector. Не добавлять evaluation, чтобы задним числом назвать pre-update loss послеучебной точностью. Цена самого фактического учебного перехода и изменение learned state — валидные показатели этого cost-опыта.

Любое измеренное улучшение полезного показателя считается успехом по этой метрике и теме; точность не обязана вырасти. Рядом показать все ухудшения и расхождения состояния. При одинаковых входах и completed updates уменьшение materialization allocations — самостоятельный успех, даже если полный wall-time шумный или вырос. При нарушенном начальном равенстве/материале или недошедшем update causal comparison недействительно. Отказ раньше обучения не является ускорением обучения.

## Доменные пределы

Механизм зависит от числа exact pairs и размера tokenizer, а не синтаксического дерева. Прямой опыт сейчас только C# documentation B0. Потенциально полезен exact DPO других тем при общей неизменяемой tokenizer snapshot, но этого результата пока нет. При одной паре ожидаемой экономии нет; при legacy framing этот B вообще не активируется. Общая стоимость обучения может почти не измениться, если доминирует transformer/backward/serialization weights. Не объявлять CPU прогноз измеренным выигрышем.

## Место, будущие команды и конфликты

Будущий короткий пакет — `C:/xg/ts01`; проект — `C:/xg/ts01/Runner.csproj`; оба arms — два настоящих .NET child процесса и один controller без обучения. Подготовка конкретных файлов и binding указаны в source-copy design. После реализации в отдельно порученной стадии:

```powershell
$ErrorActionPreference = 'Stop'
dotnet build C:/xg/ts01/Runner.csproj -c Release -p:Variant=A
if ($LASTEXITCODE -ne 0) { throw 'PREPARATION_FAILED: build A; training not started; no retry' }
dotnet build C:/xg/ts01/Runner.csproj -c Release -p:Variant=B
if ($LASTEXITCODE -ne 0) { throw 'PREPARATION_FAILED: build B; training not started; no retry' }
dotnet C:/xg/ts01/bin/A/Release/net9.0/TokenizerSnapshotRunner.dll run-pair --peer C:/xg/ts01/bin/B/Release/net9.0/TokenizerSnapshotRunner.dll --fixture C:/xg/ts01/in/train.jsonl --attempt 0001
if ($LASTEXITCODE -ne 0) { throw 'ATTEMPT_FAILED: preserve partial artifacts; no retry' }
```

Host пока отсутствует: это **спецификация будущего интерфейса, не READY_TO_RUN**. Результат — `C:/xg/ts01/r/0001/report.md`; raw/status/checkpoints A и B рядом. Directory/FileMode.CreateNew; существующий attempt или uncertain update не удалять/не повторять/не обходить новым RunId. Новая попытка допускается только отдельным прямым поручением. При отсутствующих исходниках/dependencies ошибка подготовки сохраняется до обучения; не запускать полный solution для её «исправления». Защищённая папка `docs/plans/replanning-possible-diffs` остаётся read-only.

## Исполнитель, навыки, отчёт, STOP/NEXT

Исполнитель и последовательный reviewer используют **модель текущей сессии по умолчанию**; новый provider/model не искать. Это не обучаемая Student: её B0/profile отдельно зафиксированы выше.

Навыки из `C:/Projects/llm_mvp_core/docs/skills/`: `architect-behavior-classifier-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `code-examination-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `human-readable-architecture-audit-SKILL.md`, `human-communication-GLOSSARY.md`. Если уже прочитаны и не изменились, не перечитывать ради отчёта. Исходное разрешение эксперимента действует для изолированной native копии, а не для prod/submitting. Текущий READ_ONLY_RESEARCH ограничивает работу документами; старые full CI/продвижение/правка всех тестов не становятся этапом этой карточки.

Русский report.md, до 350 слов пояснений плюс таблицы raw. Разделы: «Что меняли», «На чём проверяли», «Что получилось», «Выигрыши и ухудшения», «Граница вывода и кандидат исправления». Указать A0/A1/B0/B1, стоимость фаз A/B/B−A с единицами, actual issued/completed и все не измеренные величины. При полезном measured улучшении добавить `candidate.md` до 120 слов с точным source method и ссылкой на raw; только Люба решает перенос. Проверяющий не повторяет обучение.

**Запрет продолжать прежнее поручение после переключения обязателен.** При прямом STOP немедленно прекратить действия, без нового tool call, памяти, оформления или «последней» проверки. Допускается только необходимое прекращение уже работающего собственного процесса, чтобы он не продолжал обучение; не cleanup, readback или новый запуск. При NEXT прекратить прошлый опыт и перейти к назначенному следующему, не чинить прошлые tests/report/registry, не пересчитывать hashes и не повторять learner. Если ID следующего опыта отсутствует — сообщить статус и спросить ID, не возвращаясь к прежней работе. При обычном завершении один readback для фактических ошибок и ссылок; без циклов полировки, full CI, commit/push или автоматического product promotion.

