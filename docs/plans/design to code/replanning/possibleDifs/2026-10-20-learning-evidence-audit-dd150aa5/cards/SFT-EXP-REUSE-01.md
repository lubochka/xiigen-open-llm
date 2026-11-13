# SFT-EXP-REUSE-01 — повторно использовать экспоненты в настоящем SFT backward

**Статус: NEW_CONDITIONAL / SOURCE_TRACED / DESIGNED_NOT_RUN / NOT_RUNNABLE.** Новая проверяемая гипотеза о цене обучения; улучшение качества не заявлено. Есть конкретная незакрытая предпосылка: автор ещё не предъявил диагностичное начальное обученное состояние заданной модели. Это не поручение исполнителю искать checkpoint или обучать до успеха. Никакой scaffold, новый runner, checkpoint или результат этим документом не создан.

Core: `dd150aa5216e42b8a8bba14955337398ec5f8397`. Planning corpus: `0ea8f0b0094e234ffe7734adfa95e2a9e53203f5`. Документ предназначен для добавления в planning possibleDifs; protected core `docs/plans/replanning-possible-diffs` не изменяется.

## Зачем это проверять

В native SFT одна и та же экспонента logit сначала вычисляется при сумме denominator, затем заново при производной. Сохранение её на время одной строки позволяет убрать повторное вычисление без изменения алгоритма обучения. Это кандидат на снижение CPU/времени **реального update**, с дополнительным расходом памяти на один локальный массив. Масштаб выигрыша может оказаться ниже разрешения измерителя: transformer projections могут занимать почти всё время.

Любое фактически измеренное улучшение полезной метрики считается успехом по этой метрике и данному домену; рост accuracy не обязателен. Регрессии, включая увеличение allocations, показываются рядом. Эта гипотеза не объясняет автоматически слабое качество RP01 и не даёт основания менять production до результата и решения пользователя.

## Точная цепочка и источник

Production caller [SupervisedTrainingExecution.cs:19–33](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/14.ContinuousLearning/Training/SupervisedTrainingExecution.cs#L19-L33) передаёт модель, optimizer, tokenIds и weight в `Trainer.TrainSupervisedNextTokenStep`. В эксперименте вызывается непосредственно этот **публичный native Trainer**, как уже делает опубликованный маленький SFT host; это не копия optimizer на Python и не benchmark отдельного массива.

[Trainer.cs:102–145](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/04.TrainingLoop/Trainer.cs#L102-L145) сдвигает targets, выполняет `model.Forward(training:true)`, CE, `BackwardLoss`, clipping, `AdamW.Step`, очистку gradient и увеличение счётчика. Изменённые веса затем потребляются следующим SFT шагом и обычным `model.Forward(training:false)` при классификации знака.

В [BackwardLoss:408–419](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/04.TrainingLoop/Trainer.cs#L408-L419) две петли вычисляют `MathF.Exp(logits[position*V+token] - maxValue)` для тех же V значений. Между петлями logits не изменяются. Затем [421–426](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/04.TrainingLoop/Trainer.cs#L421-L426) прибавляет `(softmax-oneHot)*scale` к gradient и вызывает настоящий autograd. Отдельный исходный [CrossEntropy:1600–1624](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/00.TensorCore/Core/TensorOps.cs#L1600-L1624) считает ещё одну экспоненту каждого logit для loss; он не меняется.

Следовательно, по исходному коду на N target rows: построение начального gradient в BackwardLoss A вызывает Exp `2*N*V` раз, B — `N*V`; вместе с CE это `3*N*V` в A и `2*N*V` в B. Здесь не учитываются экспоненты внутри transformer forward/autograd: это подсчёт двух названных loss-операций по исходнику, не всех операций neural update, не измеренное ускорение и не число model forwards.

## Одно изменение A/B

- **A:** исходный `Trainer.BackwardLoss` из указанного core SHA.
- **B:** только в экспериментальной копии `04.TrainingLoop/Trainer.cs` создать один `float[vocabulary]` **на вызов BackwardLoss**, до position-loop. В denominator-loop записывать рассчитанный `MathF.Exp` в этот массив и прибавлять к `exponentialSum` в прежнем порядке token0…V−1. В следующей gradient-loop читать сохранённый элемент и делить на ту же сумму. Все V элементов полностью переписываются перед чтением следующей строки.
- Оставить неизменными max-pass, типы float/MathF, порядок суммирования и деления, oneHot, scale, `+=`, `logits.Backward`, loss, masks, denominator, clipping, scheduler, optimizer, seed и обучение q/v LoRA. Не изменять TensorCore, DpoTrainer, CrossEntropy, LM-head, dropout или исходный production Trainer.
- Не вводить static/shared cache, ArrayPool, buffer между шагами, fusion CE+backward или заполнение массива диагностическим forward. B несёт дополнительный payload `4*V` bytes на вызов, плюс header/alignment; lifetime заканчивается после метода. Для V256 — 1024 bytes payload/step, 4096 bytes на четыре B updates. Общий allocation delta измеряется, а не равняется автоматически этой формуле.

Сохранение значения float после `MathF.Exp` должно сохранить арифметику. Это ожидание проверяется learned/optimizer state и logits, а не объявляется доказанным заранее. Потеря градиентного ребра либо иной update делает такой B неправильной реализацией заявленного фактора.

## Новизна и близкие известные факторы

**N62-R2 уже завершён и не повторяется.** Его [исходное описание](https://github.com/lubochka/xiigen-planning/blob/0ea8f0b0094e234ffe7734adfa95e2a9e53203f5/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-update-effect-audit/06-new-R2-softmax-buffer.md) меняет срок жизни `expVals` в **DpoTrainer.BackwardLogProbs**: там Exp уже считается один раз, а новый массив раньше выделялся на каждую строку. Его результат — отдельное сохранённое наблюдение; [protected N62:14](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/docs/plans/replanning-possible-diffs/experimets/cards/N62-neural-update-costs.md#L14) фиксирует completed updates.

Здесь другой caller (`Trainer.BackwardLoss`), другое исходное лишнее действие (повторный Exp) и другой tradeoff (меньше вычислений, **больше** памяти). DLT-2/B2 меняет точность DPO reductions/log-sum-exp, LF-03 — копирование initial gradient, GRAD-ACCUM — ownership накопления, MASK — objective. Ничего из этого не включается в B.

Поиск выполнен по **всем 886 planning Markdown и 362 protected Markdown** снимков. Проверены `BackwardLoss`, `exponentialSum`, `expVals`, `CrossEntropy`, `softmax`, сочетания SFT/Exp/reuse/cache/fusion, «экспонент…дважды/трижды/повторно». Найденные BackwardLoss-упоминания относятся к SFT caller, masking и исторической reachability; expVals — к DPO R2. Точного SFT-фактора повторного Exp не найдено. Это ограниченная новизна относительно этих двух каталогов, не мировая новизна оптимизации softmax.

## Назначенный домен, модель и данные

Первый домен — узкая задача **знак результата C# вычитания**, ответ одним ASCII byte `+` либо `-`. Это не свободное объяснение C# и не общий benchmark языка. Механизм SFT математически доменно общий; перенесённый cost effect на большие vocab/длинные ответы остаётся гипотезой. Новые домены в этот опыт не добавляются.

Профиль Student фиксирован по [прочитанному native host](https://github.com/lubochka/xiigen-planning/blob/0ea8f0b0094e234ffe7734adfa95e2a9e53203f5/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/linear-transpose/scaffold/experiments-v2/RP01-MICRO/NEURAL-FROZEN-TRANSPOSE-01/Program.cs): vocab256/context64/hidden64/1 block/4heads/2KV/FFN176/RopeTheta10000/RmsNormEps1e−5; q/v LoRA rank4/alpha8/dropout0; base seed62001, adapter seed62002. Для нового host stream `sft-exp-reuse-probe-v1` фиксируется при создании будущего начального состояния и не меняется между A/B. Отдельно модель **исполнителя** — модель его текущей сессии по умолчанию, без override; она не заменяет перечисленный профиль Student.

| Роль | Prompt | Answer |
|---|---|---|
| TRAIN T1 | `C# result sign: 9-4 =` | `+` |
| TRAIN T2 | `C# result sign: 4-9 =` | `-` |
| EXAM Q1 | `C# result sign: 8-3 =` | `+` |
| EXAM Q2 | `C# result sign: 3-8 =` | `-` |

TokenIds — буквально UTF-8 bytes `Prompt+Answer`, **без LF/EOS**. BPE Train/Encode не вызывается и не приписывается. Full-sequence SFT mask остаётся прежней. Для readout подаётся только prompt, берётся argmax полного vocab на последней позиции; ties — меньший ID. Нельзя ограничить выбор только `+/-`, подставить gold или укоротить target до первого byte у иной задачи. Здесь вся response — один byte по определению данных.

## Авторская предпосылка, которая сейчас не закрыта

Исходный четырёхшаговый LFT опыт имел 0→0, а известная sanity 1/2 не превосходила constant baseline и сама по себе не предъявляет совместимый snapshot. Они **не назначаются автоматически** прогретым состоянием и не переигрываются. Никаких claims, hashes или чужих results не переписывать.

До передачи этой карточки исполнителю **автор пакета** должен положить в назначенный input `SEXP01/input/S0/` реальный совместимый полный snapshot и evidence, полученные отдельным явно описанным подготовительным обучением. Требуется показать на заданных TRAIN T1/T2 **хотя бы один правильный полный однобайтный ответ** до A/B, а также фактическую динамику подготовки. Constant-sign baseline1/2 показать рядом: равенство ему не доказывает освоения условного правила и не отменяет допустимость измерения стоимости настоящего обучения. Полный fit2/2 не является дополнительным обязательным gate; указать before/after этой подготовки, exact updates/order, цену и отдельную calibration-роль TRAIN. EXAM Q1/Q2 не участвуют в выборе checkpoint, числе preparation updates или подборе параметров. Нельзя сертифицировать S0 по ответам EXAM либо незаметно обучать до PASS.

Предписанные будущие input имена: `input/S0/weights.bin`, `optimizer.bin`, `rng.json`, `state.json`, `preparation-report.ru.md`. `state.json` должен содержать фактические config/topology/parameter keys, optimizer m/v/global step, RNG/seed/stream, provenance подготовки, исходный/конечный task readout и конечный заранее назначенный preparation budget. SHA и counts заполняются **по фактическим bytes/выполненной подготовке**, не выдумываются сейчас. Ни snapshot, ни достаточный бюджет подготовки в текущем аудите не установлены.

S0 — private experimental learned state; запрещено выдавать его за RP01 `InitialBaselineNoUpdate`, использовать canonical Step1/no-parent guard с ложными markers или заменять saved RP01 B0. Сначала под тем же Student profile создать независимые model objects с attached LoRA, затем загрузить одинаковые полные экспериментальные weights и keyed optimizer state; RNG одинаков. Checkpoint import не является новым уроком. Проверяются same S0 states A/B. Новый Trainer имеет flat LR и нулевой scheduler counter в обоих плечах; optimizer **продолжает S0 moments/global step**. Это явно заданная research composition, не утверждение полного product resume.

Для materialization автору назначен native codec: `LlmModel.LoadCheckpoint(weightsPath)` после установки того же LoRA; bindings из `GetCheckpointParameterEntries()` через `new AdamWParameterBinding(e.Key, e.Parameter)`; `AdamWPortableState.FromBytes(File.ReadAllBytes(optimizerPath))` и `TryImportPortableState(bindings, portable)` с обязательным true. Сохранение — `SaveCheckpoint` и `ExportPortableState(bindings).ToBytes()`. RNG — `CaptureRandomState(topologySha256, loraConfigurationSha256, randomStateStep)` / `RestoreRandomState(snapshot, new LlmModelRandomStateIdentity(topologySha256, loraConfigurationSha256))`; exact identity hashes и сохранённый cursor назначает автор S0 в state.json, fallback/выдуманных значений нет. Это описание API, не выполненный import. Native RNG source: [LlmModel:286–335](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/03.TransformerModel/LlmModel.cs#L286-L335).

При отсутствии этого пакета итог `NEEDS_AUTHOR_DIAGNOSTIC_STATE`, **0 builds/forwards/updates**. Исполнитель не ищет donor, не запускает дополнительные400steps, не выбирает другую модель/датасет и не просит пользователя повторно разрешить GitHub. Это конкретный недостаток автора этой карточки; другие memory/BPE/KNN опыты не задерживаются.

## Future host, source scope и единственная команда

Назначено: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SEXP01`. Результат: `SEXP01/results/attempt-0001`, внутри `A/`, `B/`; русский `report.md`, `candidate.md`, raw JSON, snapshots. Будущие `Runner.csproj`, `Program.cs`, `Run.ps1`, props/manifests находятся только в SEXP01; сейчас их нет. Никаких длинных вложенных копий полного пути другого scaffold.

Источник минимальной source closure — **36 точных .cs paths** из [прочитанного source-copy-manifest](https://github.com/lubochka/xiigen-planning/blob/0ea8f0b0094e234ffe7734adfa95e2a9e53203f5/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/experiments/linear-transpose/scaffold/experiments-v2/RP01-MICRO/NEURAL-FROZEN-TRANSPOSE-01/source-copy-manifest.json); это whitelist путей, **не перенос его старых mutated bytes или старых hash**. Для SEXP01 все исходные native bytes берутся из core SHA этой карточки. Из них единственный вариантный файл — `04.TrainingLoop/Trainer.cs`; common native `Linear.cs` не получает старый transpose-cache patch. До runtime автор фиксирует exact closure и её source identities в SEXP01/source-manifest.json; при несовпавшем dependency closure/compile — точный отказ, без поиска другого проекта.

net9.0, Release, Nullable/ImplicitUsings, default compile items отключены; explicit Compile Include по whitelist. Common source лежит в `src/Common/<native-path>`, A/B Trainer — `src/A/04.TrainingLoop/Trainer.cs` и `src/B/04.TrainingLoop/Trainer.cs`. Выбор `Variant=A|B`, independent `obj/A|B`, `bin/A|B`, assembly `SEXP01Runner`. Parent Directory.Build.props/targets отключены. Product solution/SDK installs/tests/CI не запускаются. Common Program/reader/observer одинаковы, не меняют нейронные методы вне фактора.

Единственная будущая внешняя команда после авторского закрытия предпосылки:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SEXP01/Run.ps1
```

Это **спецификация**, не обещание существующего работающего script. Run.ps1 сначала проверяет author-ready S0 evidence, сохранённые results/claim/partial и provenance; при старой попытке сохраняет её и завершает EXISTS_OR_UNKNOWN без удаления/нового attempt. Затем атомарный единственный host execution.claim, один build A и один build B, один A-arm и один B-arm последовательно с exit-проверками, одна read-only summary. Arm получает только `--arm A|B <exact-arm-root>`; иных data/model/path args нет. Каждый arm claim создаётся до load/Train. Не запускать script одновременно с его записью, не создавать второй launcher/параллельную цепочку. При неизвестном исходе наблюдать тот же process/session ID, не перезапускать.

## Бюджет и измерение

После готового S0: **по4 native SFT updates** в каждом arm, порядок T1,T2,T1,T2; всего8 forwards8backwards8AdamW updates. DPO/teacher calls0. `weight=1`; LR max=min.001; warmup0; TotalSteps4; GradAccum1; clip1; AdamW beta1.9/beta2.95/decay0/eps1e−8. Параметры не подбираются после результата. S0 preparation cost и updates — отдельные, сейчас неизвестные поля; не считать их бесплатными и не включать молча в8updates.

Readout — TRAIN2+EXAM2 в A0/A1/B0/B1: **16 дополнительных policy forwards**, вне TRAIN timing. Generate API не нужен для single-byte task; это полный one-byte answer argmax, не доказательство многотокенной генерации. Никакого дополнительного readback/reviewer rerun; общий input snapshot readback выполняется штатной загрузкой arms.

Измерять весь native TrainSupervisedNextTokenStep interval: elapsed, process CPU, allocated bytes, GC counts; load/save/readout отдельно, process peak отдельно. Не выдавать сумма component peaks за общий peak. Не ставить per-exp stopwatch/counter: он сам создаёт иной hot-loop overhead. Формула числа Exp — источник, timing/allocations — наблюдение. Фиксированный A→B single run даёт локальное наблюдение, не устойчивую причинную оценку speedup.

Сохранить начальные/конечные named trainable/frozen tensors, optimizer m/v/global step, RNG, online pre-update losses, readout logits/NLL/gold rank/full one-byte exact по каждой строке. A0/B0 совпадают; A1/B1 comparison finite, исходный договор `abs(a-b) <= 1e-6 + 1e-4*max(abs(a),abs(b))`; bitwise parity отдельно, не скрывать отличия внутри tolerance. Frozen base неизменен. Online losses разных уроков не выдавать за before/after learning delta. До/после одной и той же панели записываются отдельно.

Ресурсный успех и learning integrity — отдельные поля. Любая полезная измеренная ось сохраняется с регрессиями; при нарушенной эквивалентности заявленная оптимизация не доказана. Если главная цена неразличима или CPU увеличился из-за scratch, сохранить NO_GAIN/REGRESS. Никаких повторов «для статистики» и увеличения шагов. Если S0 на реальном load не воспроизводит заявленный ненулевой TRAIN full-answer результат, эта карточка ещё не готова: не обучать четыре шага на заведомо неподтверждённом исходном состоянии.

## STOP/NEXT и отчёт

Внутри этой отдельной карточки документная работа заканчивается после записи; это не инструкция root прекратить общий аудит. В будущей исполненной отдельной карточке NEXT=END после единственного русского report; общий program NEXT задаёт root registry, исполнитель его не придумывает. По «заканчивай и переходи» немедленно перейти к названной следующей задаче в разрешённой фазе и не возвращаться к hashes/report/review/retry прошлого опыта. По «остановить всё» немедленно прекратить всё; оформление не исключение. Сохранённые partial/raw остаются.

Report.md на русском: что поменяли, native learning path, таблица A0/A1/B0/B1 с denominator и ресурсами, все выигрыши/регрессии, лимиты и кандидат исходного fix. Candidate.md до120слов при полезном измеренном выигрыше; адрес original fix `04.TrainingLoop/Trainer.cs::BackwardLoss`. Сейчас результат NOT_MEASURED; нет права писать «успешная оптимизация» по одному подсчёту Exp. Не изменять protected required-changes: новый кандидат добавляется в разрешённый planning пакет до решения пользователя о product переносе.

## Полный промпт будущему исполнителю

«Используй модель своей текущей сессии по умолчанию без override. Язык отчёта русский. Применяй нужные ранее не прочитанные части `C:/Projects/llm_mvp_core/docs/skills/architect-behavior-classifier-SKILL.md`, `planning-session-startup-SKILL.md`, `plan-review-SKILL.md`, `code-examination-SKILL.md`, `generated-code-review-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `output-readability-gate-SKILL.md`, `human-communication-GLOSSARY.md`; не превращай skills в новый production workflow. Выполняй только полный SFT-EXP-REUSE-01 после авторской пометки готовности с фактическим S0/evidence и отдельного поручения на исполнение; эта исследовательская карточка сама runtime не назначает. Единственный host SEXP01, output results/attempt-0001, команда Run.ps1 выше. Если доказанного S0 нет, сообщи NEEDS_AUTHOR_DIAGNOSTIC_STATE; не ищи donor/модель/данные, не запускай training-to-PASS и не меняй бюджет. A — current Trainer; B — один local Exp buffer в BackwardLoss, без смены objective/precision. Только назначенные8updates/16readouts и одна A→B последовательность, без CI/product/tests/retries/новых доменов. Любое измеренное улучшение полезной метрики — успех именно этой метрики и домена, accuracy growth необязателен; все регрессии рядом, state disagreement не скрывать. Исходный продукт и protected docs не менять; прежние01–08 не перезапускать. При команде “заканчивай и переходи” немедленно NEXT, без дописывания отчёта/hashes/review/retry; при “остановить всё” сразу остановить всё. Необходимые простые DTO/throw/try-catch/void в изолированном эксперименте допустимы. Отчёт и candidate по заданному формату; никакого fake checkpoint, InitialBaselineNoUpdate для прогретых weights или вымышленных measured results».

## Проверка поздней дельты

Core `81141d57b847bc050881c2fabba8302ac18c0f8a` добавил ATTN-TRANSPOSE-STRIDES-01, JSON-OPTIONS-REUSE-01 и отчёт RESTORE-MINIMAL-SET. Текущий Trainer этими изменениями не затронут; root дополнительно проверяет новые документы на точный дубль. H16 trained state из RESTORE-MINIMAL-SET не является назначенным H64 S0 этой карточки. Новый report содержит улучшение NLL/rank и правильные отдельные токены, но явно сообщает отсутствие полностью правильных ответов. Подробности в [позднем readback](../11-latest-core-delta-and-restore.md).
