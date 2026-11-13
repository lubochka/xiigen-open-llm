# 59. Нечисловой вход DPO, clipping и состояние SFT

16 октября 2026. **READ_ONLY_RESEARCH**. Worker `/root/rp01_code_trace_astra`, Astra ultra, без вложенных агентов. Только source/history и этот Markdown: ни helper probes, ни tests/build/inference/training не запускались; продукт, данные и веса не менялись.

**Два механизма подтверждаются по source, но не как установленная причина RP01.** Низкоуровневый DpoTrainer не проверяет конечность Weight/TeachingVector. Однако штатный RP01 materializer проверяет каждый элемент TeachingVector и задаёт Weight=1. ClipGradNorm действительно может распространить один NaN на остальные переданные градиенты; этот метод вызывают SFT/pretraining, а исследуемый DPO StepVector его не вызывает. Отказ SFT после AdamW не восстанавливает состояние самого шага, но внешний incremental caller не публикует такой адаптер. Pretraining имеет ещё один, поздний барьер — повторное открытие checkpoint в runner.

Core HEAD при чтении: `216a1f3d6af3f40ceef081dc168b07e4ce3934f9`; planning HEAD: `1ebc4bac58cf305c0ec1f60c18aa4d19eebf5f99`. Scoped diff core от helper SHA `32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7` пуст для DpoTrainer, Trainer, TensorOps, AdamW, LlmIngredientLogicalUnits, IncrementalTrainer и IncrementalTrainingSession. Перед возвратом также проверены все 15 C# файлов, на которые ссылается отчёт: diff `019cb5a43e2fb817d4353c4218f6d5a90342ea0c..HEAD` и scoped worktree status пусты. Это проверка версий исходников, не исполнявшегося бинарника.

## 1. Что именно допускает низкоуровневый DPO

[StepVectorCore:832](C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:832) отказывает пустому batch, копирует Batch и TeachingVector, проверяет null и pair admission, затем получает mutation permit. [RequireVectorPairAdmissions:1174](C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:1174) проверяет только вложенный Batch; [RequirePairAdmission:1185](C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:1185) требует passport и запрещает платную внешнюю сторону. Проверки IsFinite для Weight/TeachingVector здесь нет. Защитное копирование предотвращает последующую обычную мутацию исходного массива, но не исправляет уже скопированное нечисловое значение. Record [DpoVectorBatch:3864](C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:3864) сам численные ограничения не задаёт.

После permit вес пары вычисляется как `max(.01, Weight) / max(1e-6, sum(max(.01, Weight)))`. Затем он умножается на VectorSignal и производную objective. Это положительная относительная сила вклада, а не подписанный отрицательный урок. [Вес и градиент:864](C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:864).

| Вход Weight | Следствие source-формулы при конечном обычном objective; не runtime-результат |
|---|---|
| NaN | Math.Max сохраняет NaN; totalWeight становится NaN, и веса всех пар становятся NaN. Нечисловой gradMargin передаётся backward. |
| +Infinity | totalWeight=+Infinity; у пары с +Infinity получается Infinity/Infinity=NaN, у остальных конечных весов — 0. Нельзя свести к обычной «очень важной паре». |
| −Infinity | **Clamp даёт .01**, а не нечисловой коэффициент. Сам по себе этот вход не вызывает NaN-каскад нормировки. |
| Конечный отрицательный или 0 | Также .01. Пара не исключается и не разворачивает предпочтение. Для одной пары нормированный вес равен 1, как при исходном Weight=1. Это прежний контракт нижнего порога, не новый результат N33. |
| Конечный положительный | Значения ниже .01 поднимаются до .01, прочие нормируются. Отдельное граничное условие: конечность каждого большого веса не гарантирует конечность float-суммы; при переполнении суммы все конечные относительные веса могут стать 0. Это source-граница, не найденный вход RP01. |

Следовательно, фразу N33-C «NaN/±Infinity могут дать nonfinite totalWeight» нужно сузить: **−Infinity отдельно**. Не требуется придумывать новый runtime-дефект, чтобы заметить пропущенный admission; но нельзя объединять три специальных значения под одинаковым результатом.

[VectorSignal:1159](C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:1159) не использует координаты как численные градиенты: он считает `abs(value)>1e-6`, затем возвращает `clamp(1+active/60, 1, 2)`; пустой vector даёт 1.

| Координата TeachingVector | Что считает текущий VectorSignal |
|---|---|
| NaN | Сравнение ложно: слот считается неактивным, как 0, без отказа. |
| +Infinity или −Infinity | Слот активен, как конечное число достаточной величины; знак не различается. |
| Конечное положительное/отрицательное | Активно только при abs>1e-6; знак и величина сверх порога не переносятся. |

Поэтому TeachingVector с NaN/Infinity — пропущенная проверка смысловой валидности общего интерфейса, **не автоматическое численное отравление параметров**. Например `[NaN,1]` даёт один активный слот, `[+Inf,1]` и `[−Inf,1]` — два; scale остаётся конечным. Это сохраняет уже существовавшее различение sign-ignorance и nonfinite admission, а не выдаёт их за один дефект.

## 2. Реальный RP01 caller уже ограничивает эти входы

В [CodeDocumentationSimulationDpoExperiment:113](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs:113) строки превращаются в LogicalDpoPreferenceExample, затем вызывается LogicalDpoPreparedBatchMaterializer.Prepare. [Materializer:14](C:/Projects/llm_mvp_core/18.CapabilityTraining/LogicalDpoPreparedBatchMaterializer.cs:14) переиспользует PrepareExecution; после подготовки [runner:181](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs:181) лишь выбирает готовые batches для StepVector.

В [PrepareExecution:1711](C:/Projects/llm_mvp_core/18.CapabilityTraining/LlmIngredientLogicalUnits.cs:1711) отсутствующий vector строится из prompt; пустой и слишком длинный отклоняются. Каждый элемент копируется только после `float.IsFinite(value)` на строке 1728. [Создание DpoVectorBatch:1738](C:/Projects/llm_mvp_core/18.CapabilityTraining/LlmIngredientLogicalUnits.cs:1738) задаёт Weight ровно `1f`. Следовательно, и явный supplied TeachingVector, и автоматически построенный проходят finite-проверку. В рассмотренном RP01 runner не найдено последующего изменения этих метаданных между Prepare и StepVector.

Это более сильное контрдоказательство, чем исходное замечание helper только о Weight=1. Прямой class probe с NaN не воспроизводит этот caller. Проверка не объявляется непроходимой для любых внешних программ: DpoVectorBatch и массивы остаются публичным интерфейсом, а в других доменах могут быть иные builders.

Конкретный соседний builder найден в [DesignConversionDpoPolicyCheckpointBridge:6407](C:/Projects/llm_mvp_core/44.DesignConversionDomain/DesignConversionDpoPolicyCheckpointBridge.cs:6407) и [evaluated path:6605](C:/Projects/llm_mvp_core/44.DesignConversionDomain/DesignConversionDpoPolicyCheckpointBridge.cs:6605): координаты clamp в [−2,2], UsefulnessScore — в [.01,1]. Это ограничивает Infinity, но NaN через Math.Clamp не становится конечным. **Upstream admission DesignConversionTrainingRecord здесь не прослежен**, поэтому достижимая production-инъекция NaN в этой ветке не доказана. Это точный открытый caller lead, не основание переносить finding на RP01.

## 3. Поздняя защита DPO не равна раннему admission

Без mutation capability [AcquireMutationPermit:1207](C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:1207) остановит и нормальный, и NaN-вход после pair admission, до forward/optimizer. Поэтому предложенный helper «probe с отсутствующей capability» может показать порядок отказа, но **не может показать отравленный optimizer или изменение checkpoint**. Названия предлагаемых ошибок `DPO_VECTOR_WEIGHT_NONFINITE`/`DPO_TEACHING_VECTOR_NONFINITE` в helper — будущий контракт, не наблюдённые current error codes.

При выданном permit DPO делает [снимок до операции:1227](C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:1227). При исключении до PublicationCompleted вызывается восстановление. [DpoTrainerStepSnapshot:46](C:/Projects/llm_mvp_core/11.DPO/DpoTrainerStepSnapshot.cs:46) сохраняет policy parameters/gradients, AdamW state и optional RNG; [Restore:74](C:/Projects/llm_mvp_core/11.DPO/DpoTrainerStepSnapshot.cs:74) возвращает их. Ошибка rollback присоединяется к исходной, поэтому наличие catch ещё не подтверждает успешный rollback в конкретном запуске.

StepVector вызывает ApplyOptimizer без ClipGradNorm; [ApplyOptimizer:1218](C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:1218) передаёт разрешённые параметры прямо AdamW. Нечисловые Weight дают также NaN training metrics. [Publish:2343](C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:2343) записывает policy и metrics staging до final moves; [WriteMetricsStaging:2973](C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:2973) использует обычный System.Text.Json без AllowNamedFloatingPointLiterals, а [DpoMetrics](C:/Projects/llm_mvp_core/11.DPO/Models/DpoMetrics.cs:10) не задаёт обхода. Такой NaN нельзя считать успешно записанной JSON-метрикой: если раньше не произошёл другой отказ, сериализация должна отказать и попасть в описанный rollback.

Это полезная существующая защита, но она расположена **после вычислений и optimizer**, а не вместо проверки входа. В этом исследовании не исполнялись ни отказ, ни очистка staging, ни восстановление. Нет основания утверждать, что low-level NaN обязательно публикует испорченную модель; также нет основания считать произвольный finite-vector признак качественным из-за успешного commit.

## 4. ClipGradNorm: точный численный каскад

[TensorOps:1628](C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs:1628) суммирует квадраты в float, берёт sqrt; если norm≤maxNorm — выходит, иначе умножает все Grad.Data на `maxNorm/(norm+1e-6)`. Проверки finite внутри функции нет. Метод сам не проверяет RequiresGrad: затрагивается **переданная коллекция параметров с ненулевым Grad**, а не все параметры модели вообще. Production Trainer передаёт только trainable subset; null Grad пропускается.

Для конечного положительного maxNorm следуют такие условные результаты из арифметики и исходников:

| Градиенты в переданной коллекции | Что произойдёт |
|---|---|
| Один NaN и один конечный, например 1 | norm=NaN, сравнение ложно, scale=NaN. Все координаты всех non-null Grad становятся NaN, включая прежние 0. |
| Один +Infinity или −Infinity и прочие конечные | norm=+Infinity, scale=0. Бесконечная координата становится NaN, остальные конечные — 0. **Не все становятся NaN.** |
| Все координаты конечны, но квадрат/сумма переполняет float | norm=+Infinity и scale=0 могут обнулить все конечные координаты без первоначального NaN. Проверка только IsFinite исходных grads не решает этот отдельный предел накопления нормы. |
| Обычная конечная норма ≤maxNorm | Ранний выход, градиенты сохраняются. При norm>maxNorm действует предусмотренное масштабирование; epsilon делает коэффициент чуть отличным от идеального maxNorm/norm. |

Это аналитические предсказания, не запущенная симуляция. Source helper верно описывает NaN и +Infinity; дополнены −Infinity, точный охват коллекции и finite-overflow контроль. Некорректный maxNorm возможен при прямом вызове API, но оба рассмотренных Trainer-пути проверяют его конечность и положительность. [Supervised guard:84](C:/Projects/llm_mvp_core/04.TrainingLoop/Trainer.cs:84), [pretraining config:312](C:/Projects/llm_mvp_core/04.TrainingLoop/Trainer.cs:312).

## 5. Supervised: где изменяется состояние и что останавливает caller

Порядок [TrainSupervisedNextTokenStep:67](C:/Projects/llm_mvp_core/04.TrainingLoop/Trainer.cs:67): проверить tokens, finite positive weight и clip norm → выбрать trainable parameters → Forward → scalar loss finite → Backward → Clip → LR validation → **AdamW.Step** → проверить finite parameters → очистить grads → увеличить `_supervisedStep`. Проверки finite каждого gradient перед Clip/AdamW нет. Конечный scalar loss сам по себе не доказывает конечность всех производных; конкретный источник nonfinite backward в реальном корпусе здесь не найден.

Если AdamW уже получил NaN gradient, [AdamW.Step:72](C:/Projects/llm_mvp_core/04.TrainingLoop/Optimizers/AdamW.cs:72) сначала увеличивает свой счётчик, затем меняет моменты m/v и weights. Он проверяет LR/структуру входа, но не finite каждой координаты. В supervised post-step failure [Trainer:137](C:/Projects/llm_mvp_core/04.TrainingLoop/Trainer.cs:137) очищаются только grads и выбрасывается exception. Параметры, m/v и AdamW step не возвращаются; `_supervisedStep` не увеличивается. Таким образом, повторное использование **того же** caller-owned model/optimizer после этого отказа не эквивалентно повтору исходного шага. AdamW CaptureState/RestoreState существуют, но этот метод их не вызывает. [Snapshot API:113](C:/Projects/llm_mvp_core/04.TrainingLoop/Optimizers/AdamW.cs:113).

У реального incremental consumer есть ограничения, которых нет у standalone class probe:

- [ValidateRequest:1165](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/IncrementalTrainer.cs:1165) уже отклоняет nonfinite, неположительный или превышающий float.MaxValue example.Weight. Сам Trainer повторяет finite/positive проверку после float-конверсии.
- [ProductionSupervisedTrainingStepExecutor:19](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/SupervisedTrainingExecution.cs:19) вызывает именно этот Trainer. [IncrementalTrainer:645](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/IncrementalTrainer.cs:645) при nonfatal exception возвращает SUPERVISED_ADAPTER_TRAINING_FAILED; после серии дополнительно проверяет frozen base и [AllFinite adapter parameters:688](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/IncrementalTrainer.cs:688). SaveAdapterCheckpoint находится позже, [на строке 738](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/IncrementalTrainer.cs:738).
- В прочитанном checkpoint-backed factory создаются [новая модель:363](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/IncrementalTrainingSession.cs:363), [новые Trainer и AdamW:403](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/IncrementalTrainingSession.cs:403); session получается per call [на строке 290](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/IncrementalTrainer.cs:290). Это ограничивает перенос повреждённого in-memory состояния на следующий стандартный вызов. Для любой внешней реализации IIncrementalTrainingSessionFactory такой вывод не доказан.

Итак, **внутренний шаг не атомарен при post-step numerical failure**, но current incremental wrapper отказывает до публикации нового SFT адаптера. «Faulted/Fail» не означает восстановление памяти; «память изменилась до Fail» не означает опубликованный повреждённый checkpoint. Соседний DPO этап может иметь собственные ранее завершённые эффекты; общий transaction pipeline заново здесь не аудировался.

Warmup=0 и detached Embedding/ExpandBias уже рассмотрены в [56](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/56-autograd-reachability-and-sft-caller-controls.md:7). Они не используются здесь как заново найденные причины. Наличие обновления части параметров также не доказывает корректность всех градиентов.

## 6. Pretraining: файл может появиться раньше позднего отказа

[RunPretrainingCoreAsync:223](C:/Projects/llm_mvp_core/04.TrainingLoop/Trainer.cs:223) проверяет scalar loss перед каждым backward, затем Clip → LR check → AdamW → CompletedSteps++ → ClearGradients → periodic save. Проверки finite parameters после AdamW здесь нет. Catch ставит Faulted/Cancelled и очищает grads, но не откатывает weights/moments/CompletedSteps. При нечисловой производной ошибка может обнаружиться только на следующем loss; если шаг последний, локальный Trainer способен дойти до final save и Completed.

Публичный [TrainAsync:150](C:/Projects/llm_mvp_core/04.TrainingLoop/Trainer.cs:150) передаёт `model.SaveCheckpoint` как saver. [SaveCheckpoint:553](C:/Projects/llm_mvp_core/03.TransformerModel/LlmModel.cs:553) создаёт временный файл и перемещает его в final path; [WriteKeyedCheckpoint:1449](C:/Projects/llm_mvp_core/03.TransformerModel/LlmModel.cs:1449) записывает float-значения без finite-проверки. Поэтому условное появление бинарного payload с NaN поддерживается source, а не только отсутствием guard в Trainer. **Ни один существующий checkpoint здесь не признан содержащим NaN.**

Но полный штатный [PretrainingRunner:145](C:/Projects/llm_mvp_core/tools/PretrainingRunner/Program.cs:145) после TrainAsync проверяет завершение и открывает final checkpoint в fresh model. [LoadCheckpoint:1140](C:/Projects/llm_mvp_core/03.TransformerModel/LlmModel.cs:1140) идёт через ParseKeyedRecords; [ReadValues(requireFinite:true):1629](C:/Projects/llm_mvp_core/03.TransformerModel/LlmModel.cs:1629) отклоняет NaN/Inf. Runner преобразует это в [ArtifactFailed:178](C:/Projects/llm_mvp_core/tools/PretrainingRunner/Program.cs:178), а не печатает Completed. Этот поздний guard не делает уже изменённый model/optimizer исходным и не означает, что файл не был записан; зато опровергает упрощённое утверждение «весь production runner обязательно объявляет такой артефакт успешным».

Для RP01 DPO-only эта SFT/pretraining цепочка не показана как вызванная. Называть finding доказанной причиной C#500/ENRU500 или безусловным P0 всех будущих обучений было бы шире evidence. Область риска — разрешённый SFT/pretraining шаг, **если** nonfinite/overflow действительно возникает до optimizer; частота и вклад в качество неизвестны.

## 7. Коррекция helper и будущая малая проверка

Прочитан адресный diff planning `caae0e29e8..1ebc4bac58`. В N33-D исправлена неверная ссылка «Step 132–139» на CaptureState/RestoreState 113–175; это согласуется с source. В MICRO изменился список навыков. N33-C в этом интервале не изменён. MICRO описывает HNSW/UTF8/hybrid, а не отдельный nonfinite protocol; для данного пакета прочитаны его общие требования/return gate, специфические nonfinite designs — в самих C/D. Пометки READY/ALLOWED и инструкция helper не писать core reports не переопределяют текущий прямой заказ на этот Markdown и запрет исполнения.

**Следующий протокол — только проект, не выполненная работа.** До какого-либо запуска требуется отдельная прямая авторизация эксперимента. Достаточно трёх коротких различений; новая серия из 500 пар не нужна:

1. **Admission:** одинаковая законно допущенная exact pair; отдельные NaN/+Inf/−Inf/0/negative Weight и NaN/+Inf/−Inf TeachingVector, конечный контроль. Проверить public PrepareExecution отдельно от low-level StepVector. Без mutation capability фиксировать точную стадию/ошибку; не заявлять из такого отказа эффект optimizer. При будущем разрешённом disposable recipient отдельно проверять snapshot/rollback после нечислового weight. Нулевой/отрицательный weight — самостоятельный согласуемый контракт, не подмена finite-проверки.
2. **Clipping:** два небольших параметра, один нечисловой grad и второй конечный; дополнить −Inf, обычный [3,4], конечный большой grad с переполнением float-нормы, null Grad. Записать buffers до/после. Guard всех исходных finite не выдавать за исправление overflow norm: для этого нужен отдельный численно устойчивый norm-контроль. Никакого model training для проверки самой формулы не требуется.
3. **SFT state/caller:** только при разрешённом безопасном seam и disposable session вызвать один контролируемый nonfinite gradient перед Clip, с валидными tokens/weight/LR и finite scalar loss. Сравнить параметры, AdamW moments/step, supervised counter либо CompletedSteps, код отказа и созданные файлы. Затем провести тот же отказ через production incremental wrapper или pretraining runner: distinguish memory rollback, отсутствие публикации и поздний отказ чтения. Если seam недоступен, не создавать обход прав; ограничиться source-вердиктом.

Из этих проверок следует только numerical/admission результат. Улучшение обучаемости/heldout/free answers потребует затем отдельного сравнения на реальных конечных входах; synthetic invalid-input pass не является learned-quality gain.

## Покрытие и оставшиеся первичные источники

Полностью прочитаны [N33-C](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/33-NOVEL-DPO-VECTOR-INGRESS-NONFINITE-ADMISSION-2026-10-16.md:1>) и [N33-D](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/33-NEW-NONFINITE-GRAD-CLIP-CASCADE-AND-SFT-STATE-2026-10-16.md:1>). В [MICRO](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/33-MICRO-CAUSAL-EXECUTOR-AND-EXPERIMENT-2026-10-16.md:1>) прочитаны заголовки/поиск relevant terms, строки 1–12 и 53–58, плюс указанный git diff. C# прочитан адресно: ingress/step/publication DPO, ClipGradNorm, supervised/pretraining order, AdamW Step/snapshots, прямые RP01/SFT callers и checkpoint writer/reader. Это не полный аудит ядра, attention или optimizer; существующие тесты не запускались и как отдельное подтверждение не использованы.

Governance/skills: local AGENTS 1–38; canonical AGENTS 1590–1610, CLAUDE 1029–1042; architect 340–346, planning startup 305–313, plan-review 674–684, generated-code-review 75–84, prompt-patch 26–34, readability 26–35, human-readable architecture 100–113; context-mode SKILL первые 95 строк. Все перечисленные источники найдены, указанные выдержки прочитаны. Это **targeted load, не полный reload**, без заявления независимых review-проходов. Применены требования ссылок на source, различения одноимённых путей и ограничения силы вывода.

При первоначальном возврате оставались открыты: точная upstream finite-валидация `DesignConversionDpoTrainingRecord` перед builder `DesignConversionDpoPolicyCheckpointBridge.cs:6407/6605`; наличие nonfinite градиента в конкретном разрешённом SFT/pretraining run; native подтверждение rollback и файлового исхода. В RP01 trace не найден nonfinite metadata-вход, а штатный materializer прямо блокирует его. Этот bounded пакет не завершает общее исследование.

**Продолжение по первому следу:** [64](64-design-conversion-finite-admission-callers.md) проследил штатные standalone/evaluated входы DesignConversion. Они проверяют finite score/vector при capture и повторно проверяют готовые float inputs до lease. Поэтому один NaN-пропускающий clamp не доказывает достижимый дефект этих маршрутов. Низкоуровневые, SFT и pretraining вопросы выше остаются отдельными; экспериментальное подтверждение не выполнялось.
