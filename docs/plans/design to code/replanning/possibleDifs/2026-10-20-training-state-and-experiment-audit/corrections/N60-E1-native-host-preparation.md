# N60-E1: ограниченное задание подготовки native host

**Статус: HOST_PREPARATION_REQUIRED / NOT_BUILT / NOT_RUN.** Этот документ закрывает выбор места и списка исходников; он не объявляет отсутствующий runner готовым. Полный контракт опыта и frozen TRAIN/EXAM находятся в `02-N60-E1-fixture-correction.md`. Это конкретизация существующего N60-E1/N57, не новая гипотеза.

## Один выбранный путь

Переносится **нативный метод `TranslationMachine.TrainStructuredBatchAsync` вместе с настоящими Unit trainers**, а не его Python/JS симуляция. A/B действительно обучают semantic-tokenizer, executor-ru_en и quality-verifier из двух projected batches. Обычный Flow/Translate не выполняется и не входит в вывод. Весь product `TranslationMachine` с publication, full checkpoint restore и orchestrator интерфейсом не компилируется; оставляется ровно указанный учебный фрагмент. Эту границу писать в report как «извлечённый batch consumer и native Units».

Нельзя самостоятельно заменить его direct-Verifier-only вариантом: это изменило бы заявленные2batch/11Unit calls. Нельзя расширить проект до общей `27.TranslationDomain.csproj` или `35.ExperimentsV2.csproj`: это другая цепь зависимостей и не нужна данному фактору.

## Буквальные места

| Назначение | Путь |
|---|---|
| Read-only оригинал | `C:/Projects/llm_mvp_core/` |
| Сохранённые исходные bytes и copy manifest | `C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/Rp01Micro/N60E1VerifierAdmission/reference/` |
| Изолированные компилируемые исходники A/B | `C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/Rp01Micro/N60E1VerifierAdmission/A/` и `.../B/` |
| Общие настоящие DTO | `C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/Rp01Micro/N60E1VerifierAdmission/shared/` |
| Host | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N60-E1-VERIFIER-ADMISSION/Runner.csproj`, `Program.cs`, `AArm.cs`, `BArm.cs` |
| Read-only входы | `.../possibleDifs/2026-10-20-training-state-and-experiment-audit/corrections/N60-E1-TRAIN.jsonl` и `N60-E1-EXAM.json` |
| Только новые результаты | `C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/results/N60-E1-VERIFIER-ADMISSION-01/` |

Полный путь вместо многоточия для входов тот же: `C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/corrections/`. Read-only source commit — `62bcc8f2368840e22d2ea98ef16d9bd526ad9a39`. JSON manifest рядом фиксирует SHA256, Git blob и размер каждого файла.

## Исчерпывающий список исходных компонентов этого дизайна

| Оригинальный файл | Что именно переносится | Почему нужен |
|---|---|---|
| `27.TranslationDomain/TranslationContracts.cs` | Весь файл | Direction enum и domain DTO |
| `27.TranslationDomain/TranslationUnitContracts.cs` | Весь файл | Semantic tokens, typed lessons, batch, ITranslationUnitTrainer, настоящий KNN store |
| `27.TranslationDomain/TranslationSemanticTokenizer.cs` | Весь файл, неизменный A/B | Реальная POS/marker ветвь |
| `27.TranslationDomain/TranslationStructuredBatchBuilder.cs` | Весь файл | ParseJsonl, projection и единственное B-вмешательство в128–141 |
| `27.TranslationDomain/TranslationTrainedUnits.cs` | Весь файл | Все действительные TrainOnExample, Verify и Unit.ExportCheckpoint |
| `27.TranslationDomain/TranslationMemorySourceLaw.cs` | Весь файл | Уже существующая зависимость KNN store; сохраняется без изменений |
| `27.TranslationDomain/TranslationCheckpointContracts.cs` | Весь файл | Unit-state DTOs для наблюдения; наличие этих DTO не означает cold restore опыт |
| `27.TranslationDomain/TranslationMachine.cs` | Константы14–16; поля19–21 и27–29; **полный неизменный метод89–146**; helpers662–684 и707–712 | Тот же batch admission/dispatch, history/identity bookkeeping и return metrics |
| Тот же `TranslationMachine.cs` | Runtime construction716–724 и словарь trainers760–770 | Все Unit объекты той же конфигурации, общий KNN store; не заменять trainers stubs |
| `27.TranslationDomain/TranslationRoutingFlow.cs` | Только `FlowId` constant26 с исходным значением | Builder пишет metadata FlowId; сам Flow имеет0 вызовов |
| `00.Core/LearningMachineContracts.cs` | Только record `LearningBatchResult`, строки48–67 | Нативный return DTO; не тянуть ILearningMachine/ILogicalOrchestrator |
| `00.Core/OperationResult.cs` | Весь файл | Нативный return wrapper |

Это12 строк/компонентов из11 исходных файлов. Номера строк относятся только к закреплённому commit; manifest hashes защищают от молчаливого выбора другого кода. Не извлекать строки из другой версии по старым номерам. Никакие веса/tokenizer artifacts, teacher APIs, модели, внешние Packages/ProjectReference данному Unit-пути не нужны. .NET9 SDK необходим только для будущей сборки.

## Точная последовательность подготовки

1. Запустить приложенный `N60-E1-prepare-sources.ps1` по команде ниже. Он **только** проверяет11 исходных файлов по SHA256 и копирует их в назначенный reference-каталог, сохраняет manifest и замороженные TRAIN/EXAM. Нет Train, Tokenize, build, tests или сборки целого проекта.
2. Из семи полных TranslationDomain-файлов таблицы создать A и B copies. В этих copies единственное инфраструктурное преобразование — namespace `TranslationDomain;` в `N60E1.A;` / `N60E1.B;`. Одинаковые using/BCL dependencies сохраняются; никаких других механических правок. Copyright headers сохраняются.
3. В `A/TranslationMachine.cs` и `B/TranslationMachine.cs` создать класс `TranslationMachine` без `ILearningMachine<>`, из перечисленных native blocks. Добавить одинаковый минимальный конструктор: создать объекты716–724, словарь760–770, присвоить `_unitTrainers = trainers`. Сохранить typed ссылки на tokenizer и verifier только для внешнего наблюдения; не реализовывать новый Train. Метод89–146 и три helper блока остаются source-identical после namespace transformation. Не включать `_flow`, `_runtime`, save/load/promotion/legacy TrainBatchAsync. `TranslationMachineLoadResult` из полных DTO может ссылаться на этот extracted class, но не вызывается.
4. В `A/TranslationRoutingFlow.cs` и `B/TranslationRoutingFlow.cs` оставить metadata-класс с единственной исходной константой `FlowId`; он не реализует routing и не должен иметь вызовов. В `shared/` сохранить полный `OperationResult.cs` и ровно исходный `LearningBatchResult` с namespace `AgentInfrastructure.Logical` и copyright. Обеим extracted machine добавить исходные `using AgentInfrastructure; using AgentInfrastructure.Logical;`.
5. Только в B-builder обернуть существующий `examples.Add(...:verifier...)`128–141 условием `!string.Equals(rejectedCandidate, pair.Chosen, StringComparison.Ordinal)`. Остальные generated rows, helpers, regexes, tokenizer, Unit writers/readers, signature mode и objects идентичны. Не вставлять новое required-term правило и не править N57. A/B diff отделяет одинаковую namespace/extraction инфраструктуру от этого единственного причинного изменения.
6. В назначенном `Runner.csproj` задать `OutputType=Exe`, `TargetFramework=net9.0`, `AssemblyName=N60E1VerifierAdmission`, `ImplicitUsings=enable`, `Nullable=enable`, `EnableDefaultCompileItems=false`. Явные Compile Include: Program.cs, AArm.cs, BArm.cs, перечисленные A/B файлы и два shared-файла. **Не использовать wildcard Compile, PackageReference или ProjectReference.** В host-only коде `AArm` и `BArm` имеют одинаковый control flow, различается namespace вызовов.
7. Program принимает ровно `--arm A|B --output <новый каталог>`. Встроенные неизменные absolute input paths заданы выше. Он один раз читает TRAIN через native ParseJsonl, создаёт пустой extracted machine, выполняет четыре Verify до Train, один native BuildGeneralBatch(`heldOut=false`, `Structural`), один `TrainStructuredBatchAsync`, четыре Verify после; gold читает только reporting слой. Для Verify токены получает от **того же SemanticTokenizerUnit данного arm**, не от ручного mapping. Никакие tokenizer corrections из EXAM не добавляются.
8. Сохранить source/copy/host SHA256, JSON generated rows, A0/A1 либо B0/B1 Unit.ExportCheckpoint, четыре actual Verify records до/после и counters. Обёртка счётчиков делегирует настоящим Unit.TrainOnExample и не заменяет их; одинаковая обёртка вA/B. В итоговом счёте semantic+executor+verifier=6/5, verifier subset2/1. Все export/read-only observations записываются отдельно; они не считаются уроками.
9. Ресурсные окна: BuildGeneralBatch и Train отдельно; Verify-панель отдельно. Stopwatch wall, Process.TotalProcessorTime delta и managed allocated bytes; process peak working set подписывается whole-process peak. Возвращённые native `CpuMs` и `PeakRamMb` показываются как elapsed и managed-heap snapshot. Seed/RNG/LoRA/neural layers=N/A: здесь deterministic память. Хэши и счётчики не заменяют решения Verify. Native `AccuracyAfter` при отсутствии heldOut — `MeasureUnitCoverage` (coverage UnitId), не качество Verifier; `RowsTrained` — projected rows6/5, а не2 source pairs. Эти поля сохраняются с исходными именами и явным пояснением.
10. Провести только source review extraction/diff и сборку этого csproj в порученной фазе подготовки. Build failure — конкретный технический результат подготовки; не запускать CI, не чинить product tests и не переносить опыт в другой проект. Затем одинA и одинB процесс по командам correction-карточки — **только если дано поручение выполнить опыт**. В этой аудит-сессии ни подготовительный script, ни build/runtime не выполнялись.

## Команда подготовки и разрешение конфликтов

```powershell
$e1Docs = 'C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-training-state-and-experiment-audit/corrections'
& "$e1Docs/N60-E1-prepare-sources.ps1"
```

Скрипт не выбирает альтернативные пути. Если reference/source/host/output каталог уже существует — фиксированный запуск не перезаписывается; сохраняется точный конфликт. Если source bytes не совпадают с manifest — `SOURCE_VERSION_DIFFERENT`, без замены новой версии старым hash. Документный HEAD сам по себе это условие не вызывает. Отсутствие dotnet — `SDK_NOT_AVAILABLE` на этапе будущей сборки; оно не превращает source-derived ожидание в native результат. Неизвестный compile type фиксируется с именем и вызывающей строкой, без автоматического подключения product project. Эти ветви заранее назначены; исполнитель не ищет новый ID, другой dispatcher или второй вариант эксперимента.

**Промпт подготовки:** Подготовь ровно11-source extracted batch host по этому work order и source manifest, сохраняя настоящий TrainStructuredBatchAsync и Units. Никакого другого проекта, direct-unit замены или product edits. В этой фазе не заявляй native результат. Для будущего исполнения действует правило: любое измеренное улучшение полезного показателя — успех по показателю и RU→EN, рост accuracy не обязателен; все ухудшения рядом. Передай actual подготовленные paths/hashes, source-identical blocks и единственный admission diff. После измерений полный перенос решает пользователь, не host.
