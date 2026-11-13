# COMPONENT-INTERFACES.md — Cross-Project Interface Reference

Last updated: 2026-05-28

RAG (Retrieval-Augmented Generation) scanning reference: all public interfaces that cross subproject boundaries.
Each entry lists the declaring assembly, implementing class, and consuming projects.

---

## 00.Core — `AgentInfrastructure.Logical` namespace (declared in 00.Core)

### ILearningMachine\<TDomain\>
```csharp
Task<OperationResult<LearningBatchResult>>     TrainBatchAsync(LearningBatch<TDomain> batch, CancellationToken ct = default)
Task<OperationResult<LearningEvaluationResult>> EvaluateAsync(LearningEvaluationSet<TDomain> evalSet, CancellationToken ct = default)
Task<OperationResult<LearningMachineExport>>   ExportAsync(CancellationToken ct = default)
Task<OperationResult<bool>>                    ImportAsync(LearningMachineExport export, CancellationToken ct = default)
Task<OperationResult<ILogicalOrchestrator>>    GetOrchestratorAsync(CancellationToken ct = default)
```
Implemented by: domain machine classes (one per domain project — 27.TranslationDomain, OrchestrationPlanMachine (42.ProjectWikiDomain, Phase 37), SkillSelectionMachine (43.SmartSkillsDomain, Phase 39), etc.)
Decision A-LM-02: `Train*` methods live **only** here — never on `ILogicalUnit` or `ILogicalFlow`.

### IMachineTraining\<TDomain\> (00.Core — LearningMachineContracts.cs, Phase 39)
Product-facing training adapter that extends `ILearningMachine<TDomain>`. Implemented by `SkillSelectionMachine` (43.SmartSkillsDomain).
```csharp
public interface IMachineTraining<TDomain> : ILearningMachine<TDomain>
{
    Task<OperationResult<SkillSelectionTrainingResult>> TrainMachineBatchAsync(
        IReadOnlyList<SkillSelectionMachinePair> pairs,
        CancellationToken ct = default);
}
```

### ILearningMachinePair\<TPersonal, TGlobal\>
```csharp
ILearningMachine<TPersonal> Personal { get; }
ILearningMachine<TGlobal>   Global   { get; }
Task<OperationResult<bool>>  SyncPersonalToGlobalAsync(CancellationToken ct = default)
Task<OperationResult<bool>>  PullGlobalToPersonalAsync(CancellationToken ct = default)
```
`SyncPersonalToGlobalAsync` sends only anonymised signals — decision A-LM-08.

### ILogicalOrchestrator
```csharp
Task<OperationResult<LogicalOrchestratorResult>> RunAsync(LogicalOrchestratorRequest request, CancellationToken ct = default)
```
Implemented by: `AdaptiveLogicalOrchestrator` (15)
Consumed by: `ILearningMachine.GetOrchestratorAsync`, `ProjectWikiPluginOrchestrationAdapter` (42)

### ILogicalFlow\<TInput\>
```csharp
Task<OperationResult<LogicalFlowResult>> ExecuteAsync(LogicalFlowRequest<TInput> request, CancellationToken ct = default)
```
Implemented by: `ProjectDocumentationPipelineFlowAdapter` (42), `ProjectWikiOrchestrationFlow` (15)

### ILogicalUnit\<TInput, TOutput\>
```csharp
Task<OperationResult<LogicalUnitResult<TOutput>>> ExecuteAsync(LogicalUnitRequest<TInput> request, CancellationToken ct = default)
```
Implemented by: 6 Bootstrap classes in `42.ProjectWikiDomain/BootstrapOrchestrationUnits.cs`
Note: `ILogicalUnit` has **no** Train method — decision A-LM-02.

### Supporting records (LearningMachine family)
```csharp
record LearningBatch<TDomain>(string BatchId, string DomainTag, IReadOnlyList<LearningTrainingItem> TrainingItems, IReadOnlyList<string> SourceIds)
record LearningBatchResult(string BatchId, int RowsTrained, double AccuracyBefore, double AccuracyAfter, double RewardMargin, long CpuMs, long PeakRamMb, long ElapsedMs, IReadOnlyList<string> PromotedUnitIds)
record LearningEvaluationSet<TDomain>(string EvaluationId, string DomainTag, IReadOnlyList<LearningEvaluationItem> EvaluationItems)
record LearningEvaluationResult(string EvaluationId, int RowsEvaluated, double Accuracy, double RewardMargin, bool PassesGate, string GateNote)
record LearningMachineExport(string MachineId, string DomainTag, string Version, string CheckpointRef, string ManifestRef, double AccuracyAtExport, DateTimeOffset ExportedAtUtc)
```

---

## 21.AgentInfrastructure — `AgentInfrastructure.dll`

### ISessionStorage
```csharp
Task<OperationResult<bool>>               SaveAsync(string sessionId, AgentState state)
Task<OperationResult<AgentState>>         LoadAsync(string sessionId)
Task<OperationResult<IReadOnlyList<string>>> ListSessionsAsync()
```
Implemented by: `SessionStateStore` (21)
Consumed by: 22 (AgentOrchestrator), 14 (all services via BaseService), 15 (OrchestratorFacade)

### ICodeReader
```csharp
Task<OperationResult<string>>                    ReadFileAsync(string path)
Task<OperationResult<IReadOnlyList<string>>>     ListFilesAsync(string directory, string searchPattern)
```
Implemented by: `CodebaseIndexer` (21)
Consumed by: 14 (ProjectIndexer), 22 (RepoMapBuilder)

### ICodeWriter
```csharp
Task<OperationResult<bool>> WriteFileAsync(string path, string content)
```
Implemented by: `FileChangeApplier` (21)
Consumed by: 14 (DomainDatasetCache.FlushAsync, DomainModelRegistry)

### ILlmClient
```csharp
Task<OperationResult<LlmResponse>> SendAsync(
    IReadOnlyList<ConversationMessage> messageHistory,
    IReadOnlyList<ToolDefinition>? availableTools,
    CancellationToken cancellationToken)
```
Implemented by: (external adapter)
Consumed by: 21 (AgentLoop), 22 (ReactAgentLoop)

### IConfigurationProvider
```csharp
Task<OperationResult<Dictionary<string, object>>> ReadJsonAsync(string path)
Task<OperationResult<string>>                     ReadTextAsync(string path)
```
Implemented by: `ConfigurationLoader` (21)
Consumed by: 22 (RulesFileParser)

### SubPhaseRouterUnit / MultiSubPhaseRouterUnit
```csharp
// Implements ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision>
// Implements ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
```
Input: `SubPhaseRouterSourceDocument` with `DocumentTitle`, `DocumentType`, `CompiledText`, `CompiledPredicateIds`, and `SourcePath`.
Scoring contract: routing scores use only `DocumentTitle`, `DocumentType`, `CompiledText`, and `CompiledPredicateIds`; `SourcePath` is never part of token scoring, exact sub-phase mention scoring, or hybrid rank input.
Provenance contract: `SourcePath` is preserved only in `LogicalUnitResult.EvidenceIds` as `source_path_provenance:<path>` and warning evidence includes `source_path_used_for_scoring=false`.
Leakage warning: when path-only vocabulary points to a different sub-phase than path-free semantic evidence, warnings include `source_path_semantic_conflict:<path_sub_phase_id>-><semantic_sub_phase_id>`. This warning must not raise confidence or change the selected route.
Fallback consistency contract: when `MultiSubPhaseRouterUnit` delegates to a single router and that router returns a `PredictedSubPhaseId` absent from the active topology, the multi-router fails with error code `FALLBACK_SUBPHASE_ID_NOT_REGISTERED`. The error message includes the safe single-line missing id and says the single-router fallback id is not registered in the active topology. Unknown fallback ids must never be converted to the first registered sub-phase or to an empty successful route result.

### Читатель поправки толкования, связанный со своим источником (2026-11-03)

(Строка очереди остатков `S07`: исправление утверждения должно читаться вместе с историческим
результатом.)

**Что было.** Поправки толкования существуют как сохранённый пакет аудита
(`experiments-v2/_audits/2026-11-02-post107-audit/codex-claim-corrections.json`, схема
`post107-claim-corrections-v1`, шесть поправок) и уже несут опознание своего источника:
хранилище, срез, путь, отпечаток точных байтов и указатель внутрь документа. А ЧИТАТЕЛЯ не
было вовсе: во всём дереве это понятие встречается только в файлах данных, ни в одном файле
кода его нет, и сам пакет говорит это о себе — у каждой поправки стоит
`runtime_reader_implemented: false`, а правило чтения записано словами: сверить отпечаток и
путь источника и указатель внутрь документа, сырое значение оставить видимым, реализация
читателя остаётся запланированной.

**Что стало.** Читатель живёт в `21.AgentInfrastructure/Core/ClaimCorrectionReader.cs`.

```csharp
public sealed record ClaimCorrectionSource(            // опознание источника
    string Repository, string SourceCommit, string SourcePath,
    string SourceSha256, string JsonPointer);

public sealed class ClaimCorrection                    // поправка вместе со своим источником
{
    public static OperationResult<ClaimCorrection> Create(...);   // единственный способ создать
    public ClaimCorrectionSource Source { get; }                  // опознание внутри поправки
}

public sealed record ActualClaimSource(                // фактический источник, без заявленного отпечатка
    string Repository, string SourceCommit, string SourcePath, string Content);

public static OperationResult<ClaimCorrectionReading> ClaimCorrectionReader.Read(
    ClaimCorrection correction, ActualClaimSource actualSource);

public static OperationResult<IReadOnlyList<ClaimCorrection>> ClaimCorrectionLoader.Load(
    string packageJson);
```

**Связь неразделима по построению, а не проверкой вдогонку.** Опознание источника лежит ВНУТРИ
поправки; `Create` отвергает неполное опознание, поэтому поправка без источника не возникает
вовсе; у читателя нет довода, который принимал бы толкование отдельно от источника, — значит
рассогласовать поправку и источник нечем. Загрузчик пакета проходит через ту же единственную
проверку, поэтому дыра не возвращается с другой стороны.

**Что именно проверяет читатель.** Хранилище, затем путь, затем срез — несовпадение среза
называется КОНФЛИКТОМ ВЕРСИИ (`CLAIM_CORRECTION_SOURCE_VERSION_CONFLICT`), а не поводом тихо
применить поправку к другой попытке. Отпечаток источника считается ЗАНОВО из фактических байтов;
заявленному отпечатку не верят. Затем указатель внутрь документа должен найтись. Коды отказа:
`CLAIM_CORRECTION_SOURCE_IDENTITY_REQUIRED`, `CLAIM_CORRECTION_REPOSITORY_MISMATCH`,
`CLAIM_CORRECTION_PATH_MISMATCH`, `CLAIM_CORRECTION_SOURCE_VERSION_CONFLICT`,
`CLAIM_CORRECTION_SOURCE_CONTENT_MISMATCH`, `CLAIM_CORRECTION_POINTER_NOT_FOUND`,
`CLAIM_CORRECTION_SOURCE_NOT_PARSABLE`, `CLAIM_CORRECTION_PACKAGE_NOT_PARSABLE`.

**Сырое значение и толкование лежат раздельно.** `ClaimCorrectionReading` несёт и сырое значение
из самого документа по указателю, и авторитетное толкование аудита, и состояние толкования.
Подменить одно другим нельзя: это разные поля одного ответа.

**Чего это НЕ делает.** Не подключает поправку к построителю отчётов: такого построителя в дереве
нет, и строка очереди сама называет его будущей работой, поэтому поправка пока не доезжает до
человеческого вывода сама. Не меняет исторические сырые данные и не начинает обучения. Не
проверяет происхождение самого пакета аудита: отпечаток считается у источника поправки, а не у
пакета. Указатель поддержан в принятом виде (части через косую черту, служебные знаки как `~1` и
`~0`); выбор строки по номеру не поддержан, и это названо прямо.

Проверки: `Tests/ExperimentFixes/ClaimCorrectionSourceBindingTests.cs` — чужой путь, чужой срез,
изменённое содержимое, поправка без отпечатка и без указателя, строка пакета без опознания,
отсутствующий указатель, законный путь с видимым сырым значением и согласованность загрузчика с
читателем.

---

## 22.AgentAlgorithms — `AgentAlgorithms.dll`

### ITaskDecomposer
```csharp
OperationResult<IReadOnlyList<SubTask>> Decompose(string taskDescription)
```
Implemented by: `ComplexTaskDecomposer`, `NgramTaskDecomposer`, `RoleBasedTaskDecomposer` (22)
Consumed by: 15 (EpisodeDecomposer)

### IAgentMessageBus
```csharp
Task<OperationResult<string>> AssignTaskAsync(string subAgentName, string task)
```
Implemented by: `AgentOrchestrator` (22)
Consumed by: 15 (AgentLauncher)

### ITokenBudgetCalculator
```csharp
int Estimate(string content, ContentType type)
```
Implemented by: `AstTokenBudgetCalculator` (22)
Consumed by: 22 (ContextWindowManager), 21 (TokenCounter)

### ICodeIndex
```csharp
void IndexFile(string fileId, string csharpSource)
IReadOnlyList<(string FileId, double Score)> Search(CodeQuery query, int topK)
```
Implemented by: `AstPackedContextBuilder` (22)
Consumed by: 22 (AstRelevanceRanker, ContextWindowManager)

### ISkillFilter
```csharp
Task<OperationResult<IReadOnlyList<TextChunk>>> FilterAsync(
    IReadOnlyList<TextChunk> skillCandidates,
    string taskDescription,
    int topK = 5,
    double confidenceThreshold = 0.40)
```
Implemented by: `KnnSkillFilter` (22)
Consumed by: 22 (AgentPipeline), 21 (AgentLoop skill selection)

### ILlmGateway
```csharp
Task<OperationResult<string>> SendAsync(
    IReadOnlyList<ConversationMessage> history,
    CancellationToken cancellationToken = default)
```
Implemented by: adapter wrapping ILlmClient
Consumed by: 22 (ReactAgentLoop, LlmJudge)

### ISymbolGraphProvider
```csharp
Task<OperationResult<List<Dictionary<string, object>>>> GetAllSymbolsAsync()
Task<OperationResult<List<Dictionary<string, object>>>> GetReferencedByAsync(string symbolId)
```
Implemented by: `RepoMapBuilder` (22) via 07.GraphEngine
Consumed by: 22 (AstRelevanceRanker)

### ISemanticSearchProvider
```csharp
Task<OperationResult<List<(string ChunkId, double Score)>>> SearchAsync(string query, int topK)
```
Implemented by: adapter over 06.VectorSearch / 08.GraphRag
Consumed by: 22 (SemanticRanker, ContextCompactor)

### DependencyGraph (class, not interface — reused directly)
```csharp
// Declared in 22.AgentAlgorithms/MultiAgent/DependencyGraph.cs
void AddTask(SubTask task)
IReadOnlyList<IReadOnlyList<SubTask>> GetSortedLevels()
```
Reused by: 15 (OrchestratorFacade), 29 (Planning/DependencyGraph.cs — local copy from E35)

### SkillRetriever + обученный сигнал маршрута навыка (классы, не интерфейсы)
```csharp
// Declared in 22.AgentAlgorithms/Skills/SkillRetriever.cs
(IReadOnlyList<SkillTriggerLibrary.SkillTrigger> Skills, bool IsFallback) Retrieve(
    string taskDescription,
    int? expectedCategory = null,
    LearnedSkillRouteSignal? learnedSignal = null,
    SkillRouteConfiguration? routeConfiguration = null)
double ScoreForKnn(string taskDescription, string skillCondition)

// Declared in 22.AgentAlgorithms/Skills/LearnedSkillRouteSignal.cs
sealed record LearnedSkillRouteSignal(string SkillId, double Score, string ModelId, string DomainId)
sealed record SkillRouteConfiguration(string ModelId, string DomainId)
OperationResult<LearnedSkillRouteSignal?> LearnedSkillRouteSignalBinder.Bind(
    LearnedSkillRouteSignal? signal,
    SkillRouteConfiguration? configuration)

// Declared in 22.AgentAlgorithms/Skills/LearnedSkillRouteConsumer.cs
LearnedSkillRouteSelection LearnedSkillRouteConsumer.Select(
    SkillRetriever retriever,
    string taskDescription,
    IReadOnlyDictionary<string, object>? metadata,
    int? expectedCategory = null)
sealed record LearnedSkillRouteSelection(
    IReadOnlyList<SkillTriggerLibrary.SkillTrigger> Skills,
    bool IsFallback,
    bool LearnedSignalApplied,
    string RefusalCode)
```
Что это по-человечески. Выбор навыка опирается на детерминированное базовое
основание — меру Жаккара словесных признаков и порог 0,40. Обученную власть над
маршрутом несёт отдельная обученная единица разрешённого уровня
`ILogicalUnit<TInput,TOutput>` (обученная память ближайших соседей
`KnnLogicalUnit`). Её ответ приходит в выбор как типизированный сигнал и
становится СЛАГАЕМЫМ к базовой мере: ключ ранжирования названного навыка равен
`базовая мера Жаккара + обученная оценка`, и уже этот ключ проходит сортировку и
порог. Порог 0,40, обязательные навыки и обрезание до пяти штук не изменены.

Границы договора. Сигнал законен только для существующего навыка каталога и
только для выбранной модели с предметной темой; оценка обязана быть конечным
числом в отрезке [0,1]. Отсутствующий или нулевой сигнал сохраняет прежнее
поведение полностью. Привязка к выбранной конфигурации выполняется ДО выдачи
списка навыков: обычный потребитель получает типизированный отказ и ОФОРМЛЯЕТ
его (`RefusalCode`), а прямой вызов `Retrieve` с некорректным сигналом получает
`ArgumentException`, для негодной оценки — `ArgumentOutOfRangeException`.

Что НЕ доказано. Полный каталог из 48 навыков и совместимость отката для этого
моста ещё не приняты, поэтому включение моста в дефолтном узле продукта
запрещено: по умолчанию обученного сигнала в сведениях запроса нет и путь не
меняется. Сохранения и холодного восстановления обученного состояния для этого
маршрута в продукте пока нет.

Consumed by: 21 (`AdaptiveLogicalOrchestrator.ExecuteBindingAsync` под признаком
`UseSkillRetriever`), 35 (`E38SkillRetriever`)

---

## 29.SemanticCompiler — `SemanticCompiler.dll`

### ISemanticCompiler29
```csharp
OperationResult<PipelineOutput> Compile(PipelineInput input)
```
Implemented by: `SemanticCompilerPipeline` (29)
Consumed by: 14 (ProjectIndexer), 15 (EpisodeClassifier)
Alias required: `using SC29 = SemanticCompiler29;` to avoid OperationResult ambiguity

### IStackDetector29
```csharp
string Detect(string rawText)   // returns "csharp" | "typescript" | "python" | "unknown"
```
Implemented by: `StackDetector` (29/Routing)
Note: must run BEFORE normalization — E20 Critical Rule A

### IRouteClassifier29
```csharp
Domain Classify(string[] tokens)
```
Implemented by: `RouteClassifier` (29/Routing)
Domain enum values: `BUG, PLAN, ARCH, REFACTOR, CODE_Q`

### ISemanticNormalizer29
```csharp
string[] Normalize(string text)
```
Implemented by: `SemanticNormalizer` (29/Compression)
E15: +9.7% accuracy improvement

### IFamilyCollapser29
```csharp
string[] Collapse(string[] tokens)
```
Implemented by: `FamilyCollapser` (29/Compression)
E16: +8.4% accuracy improvement

### ILogicExtractor29
```csharp
string[] Extract(string[] tokens)
```
Implemented by: `LogicExtractor` (29/Compression)
E17: +97% predicate density improvement

---

## 15.OrchestratorModel — `OrchestratorModel.dll`

### ITemplateBeautifier
```csharp
OperationResult<string> Beautify(string text)
```
Implemented by: `TemplateBeautifierAdapter` (15/Adapters) — wraps `SC29.TemplateBeautifier`
Consumed by: `ResultMerger` (15)
Error code on miss: `TEMPLATE_NOT_FOUND` — caller falls back to raw text

### ProjectWikiOrchestrationFlow
```csharp
// Implements ILogicalFlow<string>; FlowId = ScenarioId = "project-wiki-orchestration-flow"
// Registered in LogicalOrchestratorAssistant.CreateDefault()
Task<OperationResult<LogicalFlowResult>> ExecuteAsync(LogicalFlowRequest<string> request, CancellationToken ct = default)
```
Input: user prompt string; metadata keys: `active_repo_path`, `linked_repo_paths` (CSV), `user_facing_entrypoint`
Output: `LogicalFlowResult` with `FinalHumanResult` = verified wiki answer; `OverallVerdict` = `"ORCHESTRATED"` or `"NEEDS_MORE_EVIDENCE"`
Returns `BLOCKED_MISSING_PROJECT_ROOT` verdict when `active_repo_path` is absent from metadata.

---

### Граница ответа не делит символ (2026-11-03)

Владельцы: `09.Inference/TextGenerator.cs` (владелец состояния байтов ответа) и
`01.Tokenizer/BpeTokenizer.cs` (доступ к байтам до превращения в строку).

Дефект, доказанный по коду. Выдача останавливалась по ЧИСЛУ ТОКЕНОВ — условие
`if (nextToken == eosToken || generated.Count == maxNewTokens) break;`. Словарь
токенизатора байтовый (256 однобайтовых записей), поэтому одна русская буква
занимает два токена, а составной символ четыре, и предел длины мог прийтись на
середину символа. Дальше байты превращались в строку одним вызовом, и
незаконченный символ становился символом замены: ответ рвался посреди символа
тихо, без отказа.

```csharp
byte[] BpeTokenizer.DecodeToBytes(List<int> tokens)   // байты до превращения в строку
record TextGenerationReceipt(..., int HeldBackByteCount = 0)
```

Починка структурная: в текст идёт только начальный кусок байтов, целиком
состоящий из законченных символов. Первый байт символа говорит, сколько байтов
символ занимает; если фактически осталось меньше — граница ставится перед этим
символом. Не перехват исключений и не замена на символ замены: перехват
маскирует, а символ замены — это и есть прежняя беда.

Укорачивание не тихое: число отброшенных байтов названо в `HeldBackByteCount`.
Поле стоит последним и имеет значение по умолчанию, поэтому прежние вызывающие и
прежние сборки записи работают без правок.

**Чего это НЕ лечит.** Граница держит ОДИН символ кодировки. Составной знак из
нескольких символов — буква с отдельным знаком ударения, флаг из двух указателей
страны, цепочка с нулевым соединителем, вариантная пометка — всё ещё может
разделиться, потому что каждый его символ сам законен и закончен. Это отдельный
класс случаев и отдельная работа: нужны правила разбиения на видимые знаки, а не
проверка байтов. Суррогатные пары здесь не возникают: это способ записи UTF-16, а
на границе ответа работа идёт с байтами UTF-8.

Приёмка: `Tests/ExperimentFixes/TextGeneratorUtf8BoundaryTests.cs` — граница
внутри символа (ответ цел, хвост назван числом); ответ целиком из законченных
символов (положительный контроль, зелёный до и после); предел меньше одного
символа (пустой текст, названный хвост, прежняя причина остановки, без
исключения). Проверки идут настоящим путём чтения: подставляется только источник
номеров.

---

### Допуск целого символа ДО выбора токена (2026-11-09)

Владелец: `09.Inference/Utf8AnswerAdmission.cs` (снимок «номер → байты», состояние
байтов ответа, счёт цены) и `09.Inference/TextGenerator.cs::GenerateCore` (отбор
кандидатов, повторная проверка выбранного номера).

Чем это отличается от границы ответа выше. Граница действует ПОСЛЕ порождения:
она выбрасывает незаконченный хвост у уже готовой строки байтов. Допуск действует
ДО выбора: негодные продолжения получают минус бесконечность в принадлежащей
генератору копии логитов, поэтому разорванный символ выбрать нельзя вовсе.
Обрезка хвоста сохранена как вторая, независимая защита.

```csharp
// Вход по запросу. Прежние подписи не менялись; путь без допуска побайтово прежний.
string TextGenerator.GenerateWithWholeCharacterAdmission(
    string prompt, int maxNewTokens, float temperature, int topK, float topP,
    int seed, CancellationToken cancellationToken)

TextGenerationReceipt TextGenerator.GenerateDetailedWithWholeCharacterAdmission(
    string prompt, int maxNewTokens, float temperature, int topK, float topP,
    int seed, int? verifiedEosToken, CancellationToken cancellationToken)
```

Правило допуска — по ФАКТИЧЕСКИМ байтам кодировки, одно для всех кандидатов:

1. **Неизвестный номер** непригоден по признаку промежутка известных номеров, а
   НЕ по числу байтов. Это различие существенно: `DecodeToBytes` отдаёт
   неизвестному номеру ноль байтов молча, поэтому по числу байтов он выглядел бы
   как «ничего не добавляет», то есть безопасным, и ответ терялся бы без следа.
2. **Цепочка байтов** должна быть законной записью UTF-8. Проверяются и сужения
   диапазонов: слишком длинные записи (`0xC0`, `0xC1`, второй байт после `0xE0`,
   `0xF0`), область слов-заменителей (после `0xED`) и верхний предел Unicode
   (после `0xF4`).
3. **Остаток бюджета.** Незаконченный префикс допустим только пока остаётся
   возможность дособрать символ: недособранных байтов не больше, чем осталось
   шагов. Это правило ОБОБЩАЕТ правило последнего шага (на нём остаток равен
   нулю, значит требуется законченный вид). Одного правила последнего шага
   недостаточно, и это измерено: без правила остатка порождение длиной
   двенадцать токенов упало с кодом отсутствия кандидатов, потому что
   трёхбайтовый знак был начат, когда до конца ответа остался один шаг.
4. **Знак конца ответа** задаёт ВЫЗЫВАЮЩИЙ (`verifiedEosToken`); у словаря его
   нет вовсе (`BpeTokenizer.EosToken => null`). На нём требуется законченный вид,
   сколько бы шагов ни оставалось в запасе, потому что сразу после него выдача
   прекращается. Недопустимым он становится не потому, что он знак конца, а
   потому, что на нём ответ остался бы недособранным.

Отказы типизированы и различимы по причине; вид отказа — бросок с кодом, как у
остальных невозможных состояний этого файла (конверта результата операции в
сборке `09.Inference` нет, и вводить его ради этой починки — отдельная работа):

```text
TEXT_GENERATOR_UTF8_ADMISSION_IDENTIFIER_UNKNOWN   // выбран номер вне словаря
TEXT_GENERATOR_UTF8_ADMISSION_SEQUENCE_INVALID     // выбранный номер рвёт символ
TEXT_GENERATOR_UTF8_ADMISSION_NO_CANDIDATE         // пригодных кандидатов нет (до розыгрыша)
TEXT_GENERATOR_UTF8_ADMISSION_VOCABULARY_EMPTY     // словарь пуст
```

Сохранение поведения и цены. Лишних проходов модели нет; обход словаря не
обращается к генератору случайных чисел; выборщик вызывается по одному разу на
шаг, как и прежде, поэтому число розыгрышей не изменилось. Снимок словаря
строится ОДИН раз на порождение. Если маска не исключила ничего, выборщик
получает тот же самый объект вектора, а не копию. Диагностика первого шага
сохраняет ИСХОДНЫЕ числа до маскирования; проверка конечности чисел остаётся до
появления минус бесконечности.

**Включение по умолчанию НЕ сделано** — вход только по запросу. Причина названа
числом: соседний зелёный договор `TextGeneratorUtf8BoundaryTests` требует, чтобы
предел длины, пришедшийся внутрь символа, дал честно более короткий текст и
`HeldBackByteCount = 1`; при допуске начальный байт на последнем шаге выбрать
нельзя, и этот договор стал бы недостижимым. Его ожидание при этом ВЕРНО: он
проверяет вторую защиту — границу ответа, — и достигает её испытательным
выборщиком, который маску намеренно не уважает. Поэтому широкое включение — это
отдельное решение владельца проекта, а не следствие этой починки.

Приёмка: `Tests/ExperimentFixes/GeneratorAdmitsWholeCharactersBeforeTheSamplerTests.cs`
— по одной решающей проверке на каждое из четырёх правил и на каждую из четырёх
проверок поведения (обычная генерация, неизвестные номера, конечность, цена),
плюс закрепление побайтовой неизменности пути без допуска. Прежняя приёмка
`AnswerReadingRejectsIncompleteCharacterIdentifiersTests` переведена на вход с
допуском; её два прежде красных случая стали зелёными.

## 18.CapabilityTraining — `CapabilityTraining.dll` (opt-in KNN candidate ranking)

Transfer of experiment KNN-DOMAIN-PRINCIPLES-01 (KDP01). Declared in
`18.CapabilityTraining/KnnCandidatePrinciples.cs`. The ranking reorders candidates that
`TransformerEmbeddingKnnLogicalUnit<TOutput>` already retrieved from its learned memory. It never
adds a candidate and never writes an answer: the answer stays the stored Output of the selected
training example. The whole feature is opt-in — without a catalogue the unit behaves exactly as
before the transfer.

### IKnnCandidatePrinciple
```csharp
KnnCandidatePrincipleDescriptor Descriptor { get; }   // Id, Version, Profile, Scope, Weight
KnnPrincipleScore Evaluate(KnnPrincipleQuery query, KnnPrincipleCandidate candidate);
// Query:     (Profile, Text)                 — the caller text only
// Candidate: (ExampleId, Text)               — the stored input text; the stored answer is NOT passed
// Score:     (Applicable, Value, Reason)     — Value is clamped to [-1, +1]; "not applicable" is 0, not a pass
```

### Проверка независимости учебного материала ДО обучения

(2026-11-02, перенос честности проверки материала из опыта сто двенадцатого.)

Зачем это нужно. Чтобы честно сказать «модель умеет обобщать», экзаменационные
строки не должны встречаться в обучении — ни теми же байтами, ни под новым
именем. Проверить это может только тот, кто видит ФАКТИЧЕСКИЙ материал и
отдельный замороженный источник.

Владельцы: `18.CapabilityTraining/CapabilityEvaluationMaterial.cs` (договор),
`18.CapabilityTraining/CapabilityMaterialLineageGuard.cs` (сторож), правка
`DefaultLogicalCapabilityTrainer`.

```csharp
OperationResult<CapabilityMaterialAdmissionReceipt> ValidateBeforeLearning(
    CapabilityEvaluationMaterial material,
    IReadOnlyList<CapabilityActualMaterialRow> actualOwnedRows);

OperationResult<IReadOnlyList<CapabilityActualMaterialRow>> SelectTrainingRows(
    CapabilityEvaluationMaterial material,
    IReadOnlyList<CapabilityActualMaterialRow> actualOwnedRows);
```

**Где стоит проверка.** В `DefaultLogicalCapabilityTrainer.TrainAndRegisterCoreAsync`
— до сборки учебного стека и до выполнения обучения предпочтениями. Ставить её у
построения корпуса ближайших соседей было бы поздно: обещание «ошибка материала
отвергнута ДО обучения» к тому моменту перестало бы быть правдой.

**Откуда приходит материал.** Только от доверенного подготовителя
`ICapabilityEvaluationMaterialProvider`, заданного отдельным необязательным
параметром конструктора обучателя. Не из полей пользовательского запроса и не из
доменных меток: иначе проверку можно было бы пройти, просто объявив нужные
значения. Прятать семью происхождения в доменные метки запрещено. Уникальность
имени примера независимости семей НЕ доказывает.

**Два дефекта опытного проверяющего, которые НЕ перенесены.** Опытный
проверяющий (`experiments-v2/RP01-MICRO/P11201/src/MaterialLedgerVerifier.cs`)
искал совпадение по ПРЕДЪЯВЛЕННЫМ полям отпечатка (строки 61 и 64), не
пересчитывая их из содержания, и при неоднозначности брал первую запись (строки
37 и 42). Здесь отпечаток всегда считается из фактических байтов и сверяется с
предъявленным, а неоднозначное происхождение даёт типизированный отказ.

Коды отказа: `CAPABILITY_MATERIAL_PRESENTED_DIGEST_MISMATCH`,
`CAPABILITY_MATERIAL_AMBIGUOUS_CONTENT_LINEAGE`,
`CAPABILITY_MATERIAL_CONTENT_NOT_IN_SOURCE`,
`CAPABILITY_MATERIAL_LINEAGE_CONTRADICTS_CONTENT`,
`CAPABILITY_MATERIAL_FORBIDDEN_CONTENT_OVERLAP`,
`CAPABILITY_MATERIAL_FORBIDDEN_FAMILY_OVERLAP`,
`CAPABILITY_MATERIAL_DUPLICATE_SOURCE_ROW`, плюс коды отсутствия входа.

### Обучатель получает ровно проверенный материал (2026-11-03)

(Вторая половина той же находки аудита: проверка без связи с потреблением остаётся
формальностью.)

**Что было неправильно.** Проверка смотрела на строки подготовителя, а обучение
потребляло свои наборы. Связи не было ничем: расписка допуска нигде не читалась, а
отбор допущенных обучающих строк (`SelectTrainingRows`) не имел ни одного
продуктового вызова. Можно было проверить одну копию, а обучать по другой.

**Что стало.** Материал приходит в обучение единственным носителем
`CapabilityMaterialLineageGuard.AdmittedTrainingMaterial`. Конструктор закрыт,
фабрика одна, и спросить подготовителя она решает сама, поэтому собрать носитель
мимо проверки в сборке нечем.

```csharp
public static OperationResult<AdmittedTrainingMaterial> Admit(
    ICapabilityEvaluationMaterialProvider? provider, string runId, string domainTag,
    IReadOnlyList<LogicalDpoPreferenceExample> preferences, string combinedTrainingText,
    IReadOnlyList<GeneratedCapabilityKnnExample>? retrievalCorpusExamples = null)

public CapabilityMaterialAdmissionReceipt? Receipt { get; }   // null = оценки не было
public string EvaluationNote { get; }                         // ADMITTED_WITH_LINEAGE | NOT_EVALUATED_NO_LINEAGE
public string CombinedTrainingText { get; }                   // диагностическое чтение без сверки
public int PreferenceRowCount { get; }
public int RetrievalCorpusRowCount { get; }

// Сверяющие чтения: перед выдачей пересчитывают отпечаток тождества того, чем владеет
// носитель, и сверяют его с отпечатком, записанным при допуске.
public OperationResult<LogicalDpoPreferenceExample[]> ReadPreferencesForLearner()
public OperationResult<string> ReadCombinedTrainingTextForLearner()
public OperationResult<IReadOnlyList<GeneratedCapabilityKnnExample>> ReadRetrievalCorpusForLearner()
```

**Владение изменяемыми векторами.** Учебный вектор пары лежит в записи обычным
изменяемым массивом, поэтому копия массива ЗАПИСЕЙ отдавала тот же самый объект
вектора: вызывающий менял число в векторе после допуска, и обучение получало другие
числа. Теперь вектор копируется и при допуске, и при каждой выдаче, а прежний
`CopyPreferencesForOptimizer` заменён сверяющим `ReadPreferencesForLearner`.

**Отнятие возможности, а не проверка вдогонку.** `BuildTeachingStack` и
`ExecuteDpoTeachingAsync` принимают теперь ТОЛЬКО этот носитель: довода с сырой
строкой учебного текста и довода с сырым набором пар у них больше нет. Допуск стоит
раньше любого `AcquireMutationPermit`, поэтому отказ материала ничего не откатывает.

**Три набора, которые обучение потребляет, и решение по каждому.**

| Набор | Куда идёт | Решение |
|---|---|---|
| пары предпочтений | обучение предпочтениями | проходит допуск носителем |
| общий учебный текст | сборка учебного стека и словаря | проходит допуск носителем |
| корпус ближайших соседей | `BuildKnnTrainingCorpus` | проходит допуск носителем |

**Корпус поиска вошёл в допуск.** Прежде он собирался много позже, уже после обучения
предпочтениями, прямо из слепков повторного воспроизведения и планов ветвей, которых
допуск вообще не видел: экзаменационная строка в корпусе не ловилась ничем. Теперь
упорядоченные строки корпуса собираются ДО первого разрешения на изменение
(`BuildOrderedRetrievalCorpusExamples`), входят в допуск и читаются обучателем поиска
только из носителя: `BuildKnnTrainingCorpus` принимает носитель и ничего больше.
Байты корпуса при этом не изменились — строки остались чистой функцией от тех же двух
входов в том же порядке, поэтому опись и отпечаток `corpus-v1` прежние, и семантика
самого проектора не менялась.

**Что доказано о связи, и что нет.** Доказаны ОБЕ половины. Отрицательная: в том, что
обучают, нет содержания отложенных строк замороженного источника — ни по точным
байтам, ни по приведённому тексту, когда политика велит сравнивать и так. Иглы берутся
из ЗАМОРОЖЕННОГО источника, а не из предъявленных подготовителем строк: иначе
подготовитель мог бы просто не предъявлять экзаменационную строку и тем самым
разрешить её утечку. Положительная: содержание КАЖДОЙ допущенной учебной строки в том,
что обучают, присутствует — прежде подготовитель мог честно предъявить три разрешённые
учебные строки, а до обучателя доходили две, и расписка этого не замечала. Расписка
называет это полем `ConsumedMaterialBinding`: `CONSUMED_MATERIAL_NOT_BOUND` у чистой
проверки происхождения и `TRAIN_COMPLETE_AND_HELD_OUT_ABSENT_IN_CONSUMED_MATERIAL` у
допуска. НЕ доказано по-прежнему то, что каждый потребляемый байт происходит из
допущенной обучающей строки: в потребляемом материале законно есть своё оформление и
повторения, заданные явной проекцией.

**Два отпечатка расписки и зачем их два.** `ConsumedMaterialSha256` остался ТЕКСТОВЫМ
и неизменным по байтам: это опубликованный член расписки. Рядом добавлено
`ConsumedIdentitySha256` — отпечаток ТОЖДЕСТВА: общий текст, вопрос, обе стороны и
учебный ВЕКТОР каждой пары точными битами каждого числа, плюс упорядоченный корпус с
его признаками и метками. Текстовый отпечаток изменение числа в векторе не мог даже
представить; тождество его различает, и именно оно сверяется при каждой выдаче.
Добавлено также `ConsumedRetrievalCorpusRowCount`. Оба поля объявлены со значением по
умолчанию, поэтому прежние вызывающие конструктора расписки работают без правок.

**Кто читает расписку, когда и куда она сохраняется.**
`DefaultLogicalCapabilityTrainer.TraceMaterialAdmission` — сразу после допуска, до
первого разрешения на изменение: одной человеческой строкой заметка об оценке,
профиль, доказанная связь, число связанных пар и отпечаток потребляемых байтов. Сразу
за ним `PersistMaterialAdmissionReceipt` сохраняет расписку на диск через
`CapabilityMaterialAdmissionReceiptStore` (`18.CapabilityTraining/CapabilityMaterialAdmissionReceiptStore.cs`),
в каталог `<корень контрольных точек>/.capability-material-receipts`. Прежде расписка
жила ровно до выхода из метода обучения, то есть связкой материала с обучением не
была: доказательство обязано переживать перезапуск. Запись сама читает записанное
обратно и сверяет все поля; отдельное чтение сверяет отпечаток полей, поэтому
изменённый на диске файл отвергается, а не читается молча. Остаток назван прежний:
это долговечная улика, а не публикация расписки в наружный договор результата обучения
и не запись её в слепок состояния — те принадлежат владельцам своих договоров.

```csharp
internal static OperationResult<string> Save(string receiptRoot, string runId,
    CapabilityMaterialAdmissionReceipt receipt)      // пишет, читает обратно, сверяет все поля
internal static OperationResult<CapabilityMaterialAdmissionReceipt> Load(string path)
```

Новые коды отказа: `CAPABILITY_MATERIAL_CONSUMED_REQUIRED` (обучать нечем),
`CAPABILITY_MATERIAL_CONSUMED_HELD_OUT_CONTENT` (в том, что обучают, найдено
содержание отложенной строки), `CAPABILITY_MATERIAL_CONSUMED_TRAIN_INCOMPLETE`
(разрешённая учебная строка до обучателя не дошла),
`CAPABILITY_MATERIAL_CONSUMED_CHANGED_AFTER_ADMISSION` (содержание носителя после
допуска изменилось), плюс коды хранилища расписки
`CAPABILITY_MATERIAL_RECEIPT_WRITE_FAILED`, `..._READ_FAILED`, `..._NOT_FOUND`,
`..._MALFORMED`, `..._DIGEST_MISMATCH`. Проверки:
`Tests/ExperimentFixes/AdmittedTrainingMaterialConsumptionTests.cs` и
`Tests/ExperimentFixes/CapabilityMaterialConsumedIdentityTests.cs`.

**Заявка на строку источника сверяется со всеми строками её отпечатка.** Прежде сверка
требовала совпадения именно с первой строкой списка этого отпечатка. Одно и то же
содержание законно встречается в источнике дважды — именно так выглядит запрещённое
пересечение, когда экзаменационная строка присутствует ещё и как учебная, — и тогда
вторая фактическая строка не могла подтвердить свою заявку никогда: материал
отвергался ранним `CAPABILITY_MATERIAL_LINEAGE_CONTRADICTS_CONTENT`, а проверка
пересечения содержания до своего отказа не доходила вовсе. Теперь заявка верна, если
названная строка есть среди строк этого отпечатка; семья у них одна (иначе выше уже
был бы отказ о неоднозначности), а роли могут различаться, и именно разность ролей и
даёт запрещённое пересечение.

**Политика пересечений задаётся явно**, значением `CapabilityMaterialIntersectionPolicy`,
а не выводится из примера: отдельно запрет совпадения содержания обучения с
отложенным материалом, отдельно запрет пересечения семей, отдельно — сравнивать
ли по приведённому тексту.

**Граница приведения текста названа прямо.** Нижний регистр и удаление знаков
препинания — приведение короткого английского тестового корпуса опыта. Это НЕ
доказательство смысловой эквивалентности и не годится для C#, SQL и перевода,
где регистр и знаки меняют смысл. Политика по умолчанию для неизвестного
корпуса — только точные байты.

**Прежнее обучение не запрещается.** Без подготовителя материала путь обучения
остаётся прежним, а независимая оценка по нему не выдаётся: состояние называется
`NOT_EVALUATED_NO_LINEAGE`, а не молчаливое «прошло».

**Чего это не доказывает.** Роста качества: на внешнем экзамене опыта оба плеча
дали 2 из 4 — ровно постоянный ответ, и один класс строк просел с 2/2 до 0/2.
Независимости ПОСЛЕ превращения текста в числа для модели: источник-непересекающийся
не означает непересекающийся после кодирования, и в расписке это поле стоит как
`NOT_MEASURED`.

Приёмка: `Tests/ExperimentFixes/CapabilityMaterialLineageTests.cs` — честная
основа, подделанный отпечаток, неоднозначная семья, пересечение содержания,
пересечение семей и отбор только учебных строк. Подключение сторожа к обучателю
закрыто обзором кода и сборкой, но не прогоном.

### KnnCandidatePrincipleCatalog
```csharp
static OperationResult<KnnCandidatePrincipleCatalog> Create(string profile, IReadOnlyList<IKnnCandidatePrinciple> principles);
static OperationResult<KnnCandidatePrincipleCatalog> CreateEmpty(string profile);
```
Admission is refused before any answer for: `PRINCIPLE_PROFILE_REQUIRED`, `PRINCIPLES_REQUIRED`,
`INVALID_PRINCIPLE`, `INVALID_PRINCIPLE_ID`, `DUPLICATE_PRINCIPLE_ID`, `UNKNOWN_PRINCIPLE_VERSION`
(only `v1`), `UNKNOWN_PRINCIPLE_SCOPE` (only `candidate-ranking`), `FOREIGN_PRINCIPLE_PROFILE`,
`INVALID_PRINCIPLE_WEIGHT` (finite and within `[0, 1]`), `PRINCIPLE_DESCRIPTOR_READ_FAILED` (reading
the live `Descriptor` property of an external rule threw; `Create` returns the envelope and never
lets the exception escape). Admitted principles are stored in canonical descriptor-id order so the
floating-point summation is reproducible.

Each admitted entry (`KnnAdmittedPrinciple`) exposes two things: `Descriptor`, the passport copy
frozen at admission, and `Principle`, the callback itself. Every decision — ordering, weighting,
profile, version, scope, auditing — must read the frozen `Descriptor` of the entry. Reaching the
live passport through `entry.Principle.Descriptor` reintroduces the rewritable-passport defect and
is not used anywhere in the product.

### KnnCandidatePrincipleRanker
```csharp
static OperationResult<IReadOnlyList<KnnRankedCandidate>> Rank(
    KnnCandidatePrincipleCatalog catalog, KnnPrincipleQuery query, IReadOnlyList<KnnNativeCandidate> candidates);
// final_i = native_i + 0.20 * (1/m) * SUM_j ( weight_j * clip(score_ij, -1, +1) )
// a non-applicable or non-finite judgement contributes 0 and the other weights are NOT renormalised
```
Consequence of the fixed budget: one candidate pair whose native gap exceeds `0.40` can never be
reversed by the principles. Ties keep the stable `ExampleId` ordinal order of project 18.

### TransformerEmbeddingKnnLogicalUnit\<TOutput\> — opt-in entry points
```csharp
static OperationResult<...> Train(unitId, unitRole, domainTag, model, tokenizer, examples,
                                  KnnCandidatePrincipleCatalog? candidatePrinciples = null);
static OperationResult<...> Train(unitId, unitRole, domainTag, model, tokenizer, examples,
                                  IKnnAnswerSnapshot<TOutput> answerSnapshot,
                                  KnnCandidatePrincipleCatalog? candidatePrinciples = null);
OperationResult<...> WithCandidatePrinciples(KnnCandidatePrincipleCatalog candidatePrinciples);
```

#### Договор владения ответом действует и здесь (2026-11-08)

(Перенос измеренного результата опыта двадцатой строки опытов: когда единица ближайших
соседей отдаёт наружу тот же объект ответа, который лежит у неё в памяти, внешнее изменение
этого объекта портит обученный урок — рука без защиты дала 9 правильных из 12, рука с копией
12 из 12.)

Договор `IKnnAnswerSnapshot<TOutput>` и сборка политик `KnnAnswerSnapshot`
(`21.AgentInfrastructure/Logical/IKnnAnswerSnapshot.cs`) переиспользуются этой единицей как
есть — ссылка проекта 18 на проект 21 уже существует, второго договора нет. Политика
применяется дважды: при приёме урока в память (`ForStorage`, до вложения и до записи в
указатель близких векторов) и при каждой выдаче ответа наружу (`ForRead`). Память также
берёт свою копию словаря уроков, поэтому чужой словарь после создания единицы на ответы не
влияет.

Прежняя перегрузка без договора осталась на месте и сама разрешает политику по типу ответа:
заведомо неизменяемый ответ (строка, число, перечисление) берётся без копии, а любой другой
тип получает отказ `KNN_ANSWER_SNAPSHOT_DEFAULT_POLICY_REQUIRED` ДО постройки памяти, вместо
прежнего молчаливого общего доступа. Пустой договор в явной перегрузке отвергается кодом
`ANSWER_SNAPSHOT_REQUIRED`. Состояние владения названо в описи обученной модели строкой
`knn-answer-snapshot:known-immutable | custom | declared-shared`.

Границы: снимок обученной памяти (`GeneratedCapabilityCheckpointManifest`) поля политики не
несёт, поэтому восстановленная единица всегда получает политику по умолчанию; для сегодняшних
продуктовых путей это тот же `known-immutable`, потому что их ответ — строка. Покрытие:
`Tests/ExperimentFixes/TransformerKnnAnswerOwnershipTests.cs`.
`WithCandidatePrinciples` reuses the same model, tokenizer, embedding model, HNSW index and memory —
nothing is re-embedded and nothing is retrained. The catalogue profile must equal the unit
`domainTag`, otherwise `FOREIGN_PRINCIPLE_PROFILE` is returned. When a non-empty catalogue is
configured, the manifest and the result evidence carry `KDP01:CANDIDATE_PRINCIPLE_RANKING`.

### Snapshot persistence of a configured catalogue — NOT SUPPORTED YET

`GeneratedCapabilityCheckpointManifest` stores the model, the tokenizer and the learned examples; it
carries no field for the ranking catalogue, so restore always rebuilds the unit on the plain native
path. `TeachingRegistryPersistence.Capture` therefore refuses, before any snapshot is declared
restorable, a generated retrieval unit whose catalogue is non-empty:
`UNSUPPORTED_PRINCIPLE_CATALOGUE_MODE` — "Generated capability candidate principle catalogue cannot
be captured durably yet." No partial snapshot is produced. A null or empty catalogue is unaffected
and captures exactly as before; snapshots written before this guard keep meaning the empty mode.
This is the bounded first repair of A27-K2 / R27-K2: it closes the silent mode loss, it does **not**
implement durable restore of the catalogue (versioned catalogue in the manifest, a registry of
supported rule versions, digest coverage), which remains open.

### Measured profiles shipped with the transfer
`KnnDomainCandidatePrincipleProfiles.RussianTimeFrame()` and
`KnnDomainCandidatePrincipleProfiles.OrchestrationPredecessorStage()` — the two KDP01 rules whose
measured gain exceeded five per cent (RU 5/8 → 8/8, OR 5/8 → 8/8, no new wrong row in those panels).
The neutral CS/EN rules, prefix-frequency and parent-goal are deliberately NOT shipped. Hebrew uses
the same `KnnTimeFrameCandidatePrinciple` mechanics but is a separate, not yet qualified profile and
has no ready-made factory. Comparative resources (latency, allocations, peak memory, state bytes)
remain NOT_MEASURED for this transfer.

### Адресная доставка исправляющего урока выбранному владельцу (2026-11-09)

(Узкий срез двенадцатой строки переноса реестра правок по итогам опытов, опознаватель `FIX-KDP`;
дочерний владелец требования — карточка «исправление учит выбранного владельца, чужая память
остаётся прежней». Это НЕ общий допуск доменных функций ранжирования.)

**Что решает.** Когда пользователь исправил ответ, исправляющий урок обязан дойти ровно до той
обученной единицы, которая этот ответ дала. Раньше такого звена не было нигде в продукте: при двух
владельцах с одинаковым текстом запроса правильный адресат угадывался по порядку регистрации. В
опыте это дало правильный адрес в 1 случае из 2 и одно изменение ЧУЖОЙ памяти; назначенная цель
ремонта — 2 из 2 и 0 чужих изменений.

**Уровень устройства названо прямо: это ДЕТЕРМИНИРОВАННАЯ СВЯЗКА, а не обучаемое звено.** Связка
ничего не выучивает — ни веса, ни счётчика предпочтения — и получателя НЕ выбирает: она его
ПРОВЕРЯЕТ по опознанию фактического ответа. Обучение по-прежнему выполняет родной обучающий блок за
границей `IAddressedCorrectionWriter<TOutput>`. Маршрут, который получателя выбрал, может быть
обученным, но он принадлежит другому владельцу переноса и в этом срезе отсутствует.

Владелец: `18.CapabilityTraining/KnnAddressedCorrectionCoordinator.cs`. Ни одна строка
`LlmIngredientLogicalUnits.cs` и `KnnCandidatePrinciples.cs` этой правкой не изменена, поэтому путь
без адресной доставки остался побайтово прежним.

```csharp
// Опознание выбранного владельца: связка домена, модели и её версии.
public sealed record AddressedCorrectionOwnerIdentity(
    string DomainTag, string Profile, string UnitId, string UnitRole,
    string Version, long RegistrationOrder, string TrainedStateIdentity);

// Опознание фактического ответа — это и есть адрес урока.
public sealed record AddressedCorrectionResponseEvidence(
    string RunId, string AnsweredUnitId, string AnsweredDomainTag,
    string AnsweredVersion, long AnsweredRegistrationOrder, string AnsweredStateIdentity);

public sealed record AddressedCorrection<TOutput>(
    string RunId, string FeedbackId, AddressedCorrectionOwnerIdentity Owner,
    AddressedCorrectionResponseEvidence ResponseEvidence,
    string CorrectedExampleId, TOutput CorrectedAnswer);

// Правило 2 проекта: обучение и справочник приходят через интерфейсы.
public interface IAddressedCorrectionWriter<TOutput>
{
    OperationResult<AddressedCorrectionCandidate<TOutput>> TrainCorrectedOwner(
        ILogicalUnit<string, TOutput> selectedUnit, AddressedCorrectionOwnerIdentity owner,
        string correctedExampleId, TOutput correctedAnswer, CancellationToken cancellationToken);
    OperationResult<AddressedCorrectionOwnerIdentity> PublishCorrectedOwner(
        AddressedCorrectionCandidate<TOutput> candidate, CancellationToken cancellationToken);
}
public interface IAddressedCorrectionOwnerDirectory<TOutput>
{
    OperationResult<ResolvedLogicalUnitRegistryEntry<string, TOutput>> ResolveLatestOwnerById(string unitId);
}
// Переходник к продуктовому реестру единиц — единственное место, где назван конкретный реестр.
public sealed class LogicalUnitRegistryAddressedCorrectionDirectory<TOutput>
    : IAddressedCorrectionOwnerDirectory<TOutput>;

public sealed class KnnAddressedCorrectionCoordinator<TOutput>
{
    public Task<OperationResult<AddressedCorrectionReceipt>> ApplyAsync(
        AddressedCorrection<TOutput> request, CancellationToken cancellationToken = default);
}
```

**Порядок проверок — часть договора.** Состав запроса; опознание ответа обязано называть тот же
запуск и того же владельца (модель, домен, версию, порядковый номер регистрации, опознание
состояния), иначе `..._ANSWER_FROM_ANOTHER_RUN` / `..._FOREIGN_RESPONSE_EVIDENCE`; живое опознание
владельца из справочника, иначе `..._UNKNOWN_SELECTED_OWNER`; домен, профиль и роль, иначе
`..._FOREIGN_OWNER_DOMAIN` / `..._FOREIGN_OWNER_PROFILE` / `..._FOREIGN_OWNER_ROLE`; **долговечная
отметка применения читается здесь, ДО суждения об устаревшей версии**, потому что успешная доставка
сама делает прежнюю версию старой, и повторное то же обращение обязано получить прежнюю квитанцию, а
не отказ; устаревшая версия, иначе `..._STALE_SELECTED_VERSION`; обучение кандидата, которому
передаётся ФАКТИЧЕСКИЙ объект выбранной единицы из той же записи справочника; перепроверка владельца
перед публикацией, иначе `..._OWNER_MOVED_BEFORE_PUBLICATION` с сохранением оплаченного кандидата и
без повторного обучения; публикация только выбранного владельца с проверкой
`..._PUBLISHED_OTHER_OWNER`; и **только после состоявшейся публикации** — отметка применения на
диске.

**Устойчивая дедупликация ПОСЛЕ успешного применения.** Долговечный журнал
`AddressedCorrectionDeliveryJournal` (вид записи `knn-addressed-correction-delivery-v1`) кладёт
отметку применения в файл, а не в память процесса, пишет рядом отпечаток полей и читает его обратно
при каждой записи. Составной ключ доставки собирается ОДНОЗНАЧНЫМ кодированием
(`UnambiguousFieldEncoding`, длина поля перед его байтами), а не склейкой через разделитель: иначе та
же вертикальная черта внутри опознания запуска сдвинула бы границы полей и две РАЗНЫЕ доставки
получили бы один ключ. Версия и порядковый номер в ключ намеренно не входят. При НЕУДАЧНОМ
применении — отказе обучения, отказе публикации или смене владельца — отметка НЕ появляется вовсе,
поэтому урок остаётся применимым. Опытный образец заполнял набор доставок ещё на решении, ДО
обучения, и потому терял урок молча; этот образец сюда намеренно не перенесён.

**Границы названы прямо.** Опознание состояния связка только ПЕРЕНОСИТ и СРАВНИВАЕТ: нового
общедоступного отпечатка состояния здесь нет, и побайтовая сохранность состояния этим НЕ
доказывается — полный носитель состояния остаётся открытой частью своего владельца. Атомарность
«публикация + отметка» не обеспечена: окно «публикация прошла, отметка не записана» возвращается
отказом записи журнала, а восстановление только квитанции без повторной публикации принадлежит
владельцу жизненного цикла и в этом срезе не реализовано. Допущенный продуктовый писатель и
продуктовый обученный маршрутизатор профиля в этом срезе не создаются. Сравнительные ресурсы для
этой связки не измерялись.

Покрытие: `Tests/ExperimentFixes/KnnAddressedCorrectionTests.cs` — 11 решающих проверок, из них две
на настоящем родном обучении блока ближайших соседей (4 родных обучения всего: 2 подготовки и 2
исправленных кандидата) и девять чистых (родных обучений 0).

### Статистика обученного корпуса считается один раз на версию корпуса (2026-11-03)

(Перенос измеренного результата опыта сто пятнадцать.)

**Как было.** Словесная часть оценки кандидатов вызывалась внутри цикла по кандидатам, а
внутри неё весь корпус приводился к корням ДВАЖДЫ: ради средней длины документа и ради
частоты документов на каждое слово запроса. На четырёх документах и четырёх запросах это
давало 156 приведений текста к корням, 1988 просмотренных символов, 159 344 выделенных байта
и 14,2 миллисекунды.

**Как стало.** Словесная статистика корпуса — средняя длина, длины документов, приведённые к
корням слова и частота документов по слову — собирается ОДИН раз при обучении и дальше только
читается: 4 приведения, 88 символов, 28 336 байт, 1,0 миллисекунды, при совпадении оценок и
порядка кандидатов бит в бит (наибольшее расхождение ноль на 16 сравнениях). Разовая цена
подготовки названа отдельной величиной и измерена счётчиками снимка: по одному приведению на
документ плюс три пробы правила.

Владельцы: `18.CapabilityTraining/CorpusStatisticsSnapshot.cs` (снимок, правило приведения
текста к корням и проверка происхождения), правка `TransformerEmbeddingKnnLogicalUnit<TOutput>`
в `18.CapabilityTraining/LlmIngredientLogicalUnits.cs`.

```csharp
internal static OperationResult<LexicalCorpusStatistics> Build(
    IReadOnlyList<LexicalCorpusDocument> documents);

internal OperationResult<LexicalCorpusStatistics> RequireBelongsTo(
    IReadOnlyList<LexicalCorpusDocument> documents);

internal static Dictionary<string, int> Tokenize(string text);
```

**Опознание версии корпуса.** Снимок опознаётся по отпечатку фактического СОДЕРЖИМОГО
корпуса (опознание, текст и текстовая форма ответа каждого документа), по названию правила
приведения текста и по пробе этого правила. Число документов судьёй не является: у
исправленного корпуса оно остаётся прежним, и в опыте именно отпечаток содержимого отличил
подмену при равных четырёх документах. Счётчика ревизий в снимке нет намеренно: опознание
содержательное, и статистика строится ровно в одном месте, поэтому счётчик был бы постоянным
числом, выдающим себя за счётчик.

**Сброс при обучении.** Обучение всегда строит статистику заново из той памяти, которую
только что проиндексировало, поэтому новый урок получает свой снимок. Восстановление из
сохранённого слепка идёт тем же обучением: `TeachingRegistryPersistence` вызывает `Train` с
сохранёнными примерами, поэтому снимок пересобирается из живого материала и сериализовать его
не нужно.

**Структурная сторона.** Словесная оценка получает ОПОЗНАНИЕ документа, а не его текст:
читать корпус ей больше нечем, поэтому пересчитать его на каждого кандидата она не может даже
случайно. Проверка происхождения получает только документы, поэтому чужой или устаревший
снимок отвергается без единого обращения к модели.

**Чего это НЕ делает.** Не обещает выигрыша на корпусе из одного документа и одного
кандидата: там разовая цена равна экономии. Не меняет точность ответа: оба порядка выбирают
одного и того же победителя. Не проверяет правило приведения текста целиком — проба
доказывает согласие правил только на самих пробах. У ответа без собственной текстовой формы
отпечаток видит лишь имя типа; сегодня оба продуктовых пути этой единицы отвечают строкой.

**Проверки.** `Tests/ExperimentFixes/LexicalCorpusStatisticsTests.cs`: совпадение статистик с
прежними выражениями, пересборка снимка новым уроком, отказ по отпечатку содержимого при
равном числе документов, законная смена ответа обычного читателя и крайний случай одного
документа. Прогона не было: прямые команды сборки и прогона тестов запрещены политикой
разрешений проекта, поэтому написанные проверки пройденными не называются.

### Однозначное кодирование полей перед хешированием (3 ноября 2026, четвёртый раздел ремонта)

**Что было неправильно.** Оба отпечатка этой области собирались в одну строку через постоянные
разделители, и разделитель НЕ экранировался. У словесной статистики разделителями служили
управляющие знаки U+001F между полями и U+001E между документами, у сторожа происхождения
учебного материала (`18.CapabilityTraining/CapabilityMaterialLineageGuard.cs`) — перевод строки.
Поэтому тот же знак внутри значения сдвигал границы полей, и два РАЗНЫХ материала давали один
поток байтов и один отпечаток: текст с переводом строки внутри вопроса — обычное содержание, а
не подделка. Вторая половина того же дефекта: отсутствующее значение приводилось к пустой
строке, то есть «поля нет» и «поле пустое» становились одним материалом. Число документов и
число пар в поток не входили вовсе.

**Что стало.** Появился единственный владелец правила кодирования —
`18.CapabilityTraining/UnambiguousFieldEncoding.cs`. Длина каждого поля объявляется в БАЙТАХ до
его байтов, поэтому ни одна последовательность внутри значения не может быть принята за
границу; отсутствующее значение несёт свою отдельную метку; число записей входит в поток;
версия правила кодирования входит в опознание результата, чтобы смена правила не выглядела
сменой содержимого. Названный примитив отпечатка — SHA-256 в шестнадцатеричной записи нижнего
регистра; собственная свёртка платформы не используется, потому что её значение не обязано
совпадать между процессами. Порядок полей и порядок документов заданы явно: документы идут в
порядке сравнения опознаний, а два документа с одним опознанием по-прежнему отвергаются своим
названным кодом отказа.

**Граница отпечатка потребляемого материала, названная прямо.** Он ТЕКСТОВЫЙ: в него входят
общий учебный текст, вопрос и обе стороны каждой пары. Учебный вектор, происхождение строки и
иные поля, влияющие на обновитель весов, в него не входят, поэтому полным тождеством ВСЕХ
входов обучения он не является. Имя поля расписки `ConsumedMaterialSha256` оставлено прежним
намеренно: это опубликованный член договора расписки, и переименование сломало бы его
читателей.

**Чего это НЕ меняет.** Значения самих статистик, порядок кандидатов и точность ответа не
менялись. Значения отпечатков и ключа версии корпуса изменились по построению — это и есть
исправление; сохранённых на диске снимков у этой единицы нет, поэтому миграции тоже нет:
статистика строится заново при каждом обучении.

**Проверки.** `Tests/ExperimentFixes/LexicalCorpusStatisticsTests.cs`: знак-разделитель полей
внутри текста, знак-разделитель документов внутри ответа (прежнее правило сливало два документа
в один), отсутствующее против пустого значения, изменение текста вне основной латиницы, копия с
теми же байтами, иной ответ при том же опознании, независимость от порядка входа и отказ по
двум документам с одним опознанием. `Tests/ExperimentFixes/AdmittedTrainingMaterialConsumptionTests.cs`:
перевод строки внутри вопроса против перевода строки внутри предпочитаемой стороны и честный
повтор того же материала. В обоих файлах прежнее правило воспроизведено отдельной функцией ОТ
ВХОДА, и утверждение двойное: прежнее правило на том же входе отвечает неверно, действующее —
верно. Прогона не было: прямые команды сборки и прогона тестов запрещены политикой разрешений
проекта, зелёными они не называются. Выбор длины в байтах, а не в знаках, ни одной проверкой не
отличается — для счёта отпечатка однозначны оба счёта; байтовая длина выбрана ради будущего
читателя потока и проверена чтением самого кодировщика.

### Раздельный учёт стоимости словесного и структурного разбора (3 ноября 2026, вторая половина четвёртого раздела ремонта)

Что это даёт продукту. Оценка кандидата складывается из трёх частей, и две из них стоят
по-разному: словесная читает неизменяемый снимок статистики корпуса, а структурная разбирает
образец текста. Пока обе цены складывались в одно число, различие было не видно, и улучшение
одной половины могло скрыть ухудшение другой.

Как это работает без кода. Обычный вызов оценки кандидатов считает по факту, а не выводит из
размера входа: сколько раз приводился к корням текст в словесной части и сколько знаков через
неё прошло; сколько раз приводились к корням ДОКУМЕНТЫ КОРПУСА внутри оценки; сколько раз
разбирался образец и сколько знаков через структурный разбор прошло; сколько кандидатов
оценено. Эти числа публикуются одной неизменяемой величиной
`TransformerEmbeddingKnnLogicalUnit<TOutput>.LastCandidateScoringCost`
(`18.CapabilityTraining/LlmIngredientLogicalUnits.cs`).

Что по этим числам видно. Словесная цена ПЛОСКАЯ: одно приведение к корням на весь вызов —
сам запрос, и НОЛЬ приведений документов корпуса внутри оценки. Этот ноль и есть признак того,
что прежний пересчёт корпуса на каждого кандидата больше не происходит. Структурная цена
РАСТЁТ: образец запроса разбирается один раз плюс образец каждого кандидата.

Чем это НЕ является, названо прямо. Это измеритель, а не улучшение: структурная цена этой
правкой НЕ уменьшена, она только названа отдельным числом. Время не измеряется вовсе: числа
счётные и воспроизводимые, а время зависит от машины. Поле «приведений корпуса внутри оценки»
не доказывает, что корпус не приводился к корням ВООБЩЕ: один раз на версию корпуса он
приводится при сборке снимка, и это его названная разовая цена.

Проверки: `Tests/ExperimentFixes/LexicalCorpusStatisticsTests.cs` — два случая на корпусе из
четырёх уроков, через ОБЫЧНЫЙ конверт запроса (не через вспомогательную сборку снимка),
поэтому оценка проходит по всем кандидатам. Прежнее правило словесной части воспроизведено
функцией ОТ ВХОДА, и показано, что его ответ меняется и от числа документов, и от числа
кандидатов. Прогона не было: прямые команды сборки и прогона тестов запрещены политикой
разрешений проекта, зелёными они не называются.

## 42.ProjectWikiDomain — `ProjectWikiDomain.dll` (orchestration unit interfaces, TD1)

Six `ILogicalUnit` specialisations wrapping the six static orchestration steps.
All declared in `42.ProjectWikiDomain/OrchestrationUnitContracts.cs`.
Bootstrap implementations are in `42.ProjectWikiDomain/BootstrapOrchestrationUnits.cs`.

### IIntentBuilderUnit
```csharp
// Extends ILogicalUnit<string, TaskOrchestrationIntent>
// Input: user prompt string
// Output: TaskOrchestrationIntent (Goal, RequestedWorkItems[], Intent)
```
Implemented by: `BootstrapIntentBuilderUnit` — regex-based intent extraction from static logic

### IProjectScannerUnit
```csharp
// Extends ILogicalUnit<SubmitUserTaskRequest, ProjectUnderstanding>
// Input: SubmitUserTaskRequest (prompt, active root, linked roots, entrypoint)
// Output: ProjectUnderstanding (ProjectRoots[], StackFacts[], ScanSummary)
```
Implemented by: `BootstrapProjectScannerUnit` — delegates to `ProjectUnderstandingUnit.UnderstandAsync`

### IEvidencePlannerUnit
```csharp
// Extends ILogicalUnit<EvidencePlannerInput, TaskEvidencePlan>
// Input: EvidencePlannerInput(Intent, Project)
// Output: TaskEvidencePlan (EvidenceQuestions[], EvidenceGaps[])
```
Implemented by: `BootstrapEvidencePlannerUnit` — delegates to `EvidencePlanner.PlanAsync`

### IUnitSelectorUnit
```csharp
// Extends ILogicalUnit<UnitSelectorInput, TaskUnitSelection>
// Input: UnitSelectorInput(Intent, Project, EvidencePlan)
// Output: TaskUnitSelection (Units[])
```
Implemented by: `BootstrapUnitSelectorUnit` — delegates to `UnitSelector.SelectAsync`

### IRouteComposerUnit
```csharp
// Extends ILogicalUnit<RouteComposerInput, HumanTaskRoute>
// Input: RouteComposerInput(Request, Intent, Project, EvidencePlan, Units)
// Output: HumanTaskRoute (UserAnswer, HasEnoughStructure, Warnings[])
```
Implemented by: `BootstrapRouteComposerUnit` — delegates to `HumanRouteComposer.ComposeAsync`

### IOrchestrationVerifierUnit
```csharp
// Extends ILogicalUnit<VerifierInput, VerifierResult>
// Input: VerifierInput(Route, Intent, Project, EvidencePlan)
// Output: VerifierResult (Passes, Verdict)
```
Implemented by: `BootstrapOrchestrationVerifierUnit` — delegates to `OrchestrationVerifier.VerifyAsync`

### Aggregate input types
```csharp
record EvidencePlannerInput(TaskOrchestrationIntent Intent, ProjectUnderstanding Project)
record UnitSelectorInput(TaskOrchestrationIntent Intent, ProjectUnderstanding Project, TaskEvidencePlan EvidencePlan)
record RouteComposerInput(SubmitUserTaskRequest Request, TaskOrchestrationIntent Intent, ProjectUnderstanding Project, TaskEvidencePlan EvidencePlan, TaskUnitSelection Units)
record VerifierInput(HumanTaskRoute Route, TaskOrchestrationIntent Intent, ProjectUnderstanding Project, TaskEvidencePlan EvidencePlan)
record VerifierResult(bool Passes, string Verdict)
```

### OrchestrationPlanMachine (42.ProjectWikiDomain — Phase 37)

Память выбора маршрута оркестрации: машина хранит ближайшие примеры, веса
выбора конкретного маршрута, идентификаторы принятых пар и состояние контрольной точки
для генерации плана. Она реализует
`ILearningMachine<OrchestrationPlanDomain>`, но текущий путь продолжения через
неё не является нейросетевым DPO-тренером.

```csharp
public sealed class OrchestrationPlanMachine : ILearningMachine<OrchestrationPlanDomain>
{
    public const string MachineId = "orchestration-plan-machine";
    public const string DomainTag = "ProjectWikiDomain.OrchestrationPlan";
    // Именованный подкомпонент: PromptFlowGroundingModel (_promptFlowGroundingModel)
    // Три режима генерации: LearnedFlowGrounding, Mode2Generalization, Mode3HumanExplanation
    Task<OperationResult<OrchestrationGenerationResult>> GeneratePlanAsync(string prompt, CancellationToken ct = default)
    Task<OperationResult<OrchestrationTrainingBatchResult>> TrainOrchestrationBatchAsync(OrchestrationTrainingBatch batch, CancellationToken ct = default)
    OperationResult<bool> ImportCheckpoint(OrchestrationPlanMachineCheckpoint checkpoint)  // Текущая схема контрольной точки — 3; импорт принимает схему 2 или 3
    OrchestrationPlanMachineCheckpoint ExportCheckpoint(OrchestrationLoopCheckpoint? loop = null)
    Task<OperationResult<LearningMachineExport>> SaveCheckpointAsync(string checkpointPath, OrchestrationLoopCheckpoint? loop = null, CancellationToken ct = default)
    static Task<OperationResult<OrchestrationPlanMachineLoadResult>> LoadCheckpointAsync(string checkpointPath, CancellationToken ct = default)
}
```
Текущая схема `OrchestrationPlanMachineCheckpoint` — 3. Схема 3 добавляет
`TrainingProgressScore` и `LastEvaluationGate`; импорт остаётся совместимым с
контрольными точками схемы 2. `LearningMachineExport.AccuracyAtExport` должен
приходить из пройденного `LastEvaluationGate`; прогресс обучения не является
точностью при экспорте.

Для `continue-orchestration-training` вход в обучение теперь собирается из
сохранённого в репозитории DPO-набора перед вызовом
`TrainOrchestrationBatchAsync`. Команда открывает набор через
`RepositoryOwnedDpoDatasetManifestSealer.OpenSealed`, проверяет его «паспорт»
и связь с исходной задачей: домен, шаблон, версию, учителя, настройки создания,
хэши платных ответов, номер строки, идентификатор запроса и адрес набора. После
этого каждая DPO-пара привязывается к своей исходной строке по проверочным
ссылкам. В обучаемый текст попадают сохранённые на диске стороны `chosen` и
`rejected`; из исходного корпуса платных эталонов переносятся только
проверенные маршрутные метки (`FlowRef`, `Title`, `SourceBundleId`), тип
маршрута и намерение задачи, которые нужны `PromptFlowGroundingModel` и
Mode2-карте. Экзаменационные строки и уже
использованные DPO-пары пропускаются. Повреждённые, перепутанные,
отсутствующие, дублированные или непривязанные данные отклоняются до изменения
контрольной точки модели. Это обновляет существующий механизм ближайших
примеров и выбора маршрута; это не градиентное DPO-обучение. Публикация
паспорта контрольной точки модели остаётся отдельной следующей работой.

После записи контрольной точки `continue-orchestration-training` заново читает
тот же файл через `OrchestrationPlanMachine.LoadCheckpointAsync` и сравнивает
состояние с тем, что команда собиралась сохранить: наборы ближайших примеров,
веса выбора конкретного маршрута, карты паттернов, списки использованных пар, списки
принятых строк по режимам, сохранённый цикл, прогресс и последнюю оценку.
Сравнение не использует только время экспорта, потому что оно меняется при
каждом `ExportCheckpoint`. Если файл не записался, не читается обратно или
после чтения отличается от ожидаемого состояния, команда завершается ошибкой
до отчёта о продолжении.

Целевой независимый цикл качества для оркестрации остаётся будущей работой:
перед новой парой нужно сохранить исходную оценку и полный снимок до пары; после
применения одной пары нужно записать полную пробную версию после пары, открыть
её обратно и отдать отдельному проверяющему. При восстановлении незавершённой
попытки уже применённая пара не применяется второй раз: оценивается пробная
версия после пары, а снимок до пары остаётся точкой отката.

### PromptFlowGroundingModel (42.ProjectWikiDomain — Phase 37)
Named subcomponent of `OrchestrationPlanMachine`. Owns `_flowGroundingKnn` (via `OrchestrationKnnStore`) and `_flowGroundingWeights` (Dictionary keyed by flowRef).
Three generation modes: `Mode1LearnedFlowGrounding`, `Mode2Generalization`, `Mode3HumanExplanation`.
Runtime confidence uses only input-side evidence: prompt tokens plus allowed-scope tokens. `LabelDescriptorFeatures` and `EvidenceOnlyTargetFeatures` are retained for evidence/checkpoint inspection only; they must not increase confidence and are not included in learned-pair K-NN query text. `BuildLearnedPairQueryTextWithoutPredictedFlow` must not add the predicted `FlowRef` as a nearest-neighbour feature.
CONSTRAINT: `exactPromptLeakageRate` must be `0.0` — any eval prompt exactly matching a training prompt causes `PassesGate=false`.
RETENTION (FIX-RET, 2026-11): `Train` no longer overwrites a route's `InputFeatureWeights` with every new lesson. When the new lesson is a compatible paraphrase of the lessons already stored for that same route, the input-feature dictionary is merged per key, case-insensitively, as `max(old, new)`; a key absent from the fresh weights means absence, never subtraction, so a previously learned paraphrase keeps answering. All seven other `OrchestrationFlowGroundingWeights` fields are always taken from the fresh `BuildWeights`. Compatibility is decided by `CanAccumulateCompatibleInputFeatures` from the positive targets only — same `Mode1LearnedFlowGrounding`, same non-empty `FlowRef`, same `Chosen.FlowRef`, same `ExpectedRouteMode` and the same `Chosen.PlanText` after whitespace-only normalization — never by similarity of the prompt wording, and the comparison runs at training time only, so no gold reaches inference. A lesson whose positive plan body differs keeps the previous full replacement; stored K-NN pairs are never removed. Correct resolution of *contradicting* lessons is NOT claimed by this rule and remains a separate open task.
FORBIDDEN: `PromptFlowGroundingModel` must not use deterministic lookup as flow grounding. `FlowReferenceDetector` is build-time corpus/audit only.

### FlowGroundingResult (42.ProjectWikiDomain — OrchestrationPlanContracts.cs, Phase 37)
```csharp
public sealed record FlowGroundingResult(
    string FlowRef,
    double Confidence,
    IReadOnlyList<string> EvidenceFeatures,
    IReadOnlyList<string> PredictedPhaseSequence,
    IReadOnlyList<string> PredictedArtifactNames,
    IReadOnlyList<string> PredictedGateSequence);
```

### OrchestrationFlowGroundingWeights (42.ProjectWikiDomain — OrchestrationPlanContracts.cs, Phase 37)
```csharp
public sealed record OrchestrationFlowGroundingWeights(
    IReadOnlyDictionary<string, float> InputFeatureWeights,
    IReadOnlyDictionary<string, float> AllowedScopeFeatures,
    IReadOnlyDictionary<string, float> LabelDescriptorFeatures,
    IReadOnlyDictionary<string, float> EvidenceOnlyTargetFeatures,
    IReadOnlyDictionary<string, float> PhaseWeights,
    IReadOnlyDictionary<string, float> ArtifactWeights,
    IReadOnlyDictionary<string, float> GateWeights,
    IReadOnlyDictionary<string, float> StackWeights);
```

### OrchestrationGenerationResult (42.ProjectWikiDomain — OrchestrationPlanContracts.cs, Phase 37)
```csharp
public sealed record OrchestrationGenerationResult(
    string PlanText,
    GenerationRouteMode RouteMode,
    BoundarySource BoundarySource,
    bool MlGenerated,
    IntentKind IntentKind,
    FlowGroundingResult? FlowGrounding,
    IReadOnlyList<string> Warnings)
// Computed: bool ExpectedMlGeneratedByRoute => RouteMode.RequiresMlGenerated()
// Computed: bool SatisfiesMlGeneratedContract => !ExpectedMlGeneratedByRoute || MlGenerated
```
`GenerationRouteMode` values: `LearnedFlowGrounding`, `Mode2Generalization`, `Mode3HumanExplanation`, `Passthrough`

### ProjectWikiMeasuredNumberRule (42.ProjectWikiDomain — ProjectWikiMeasuredNumberRule.cs)

Правило «число измерено» — единственное определение того, можно ли верить
пришедшему числу, для всего домена страниц знаний о проекте. Измерением
считается только настоящее конечное число: «не-число» и обе бесконечности
измерением не являются. Двери спрашивают это определение и не повторяют его у
себя: копии условия однажды уже разошлись — так разошлись три отдельные записи
порога публикации.

Кто его спрашивает. Двери приёмки обучения предпочтений (DPO — Direct
Preference Optimization, прямая оптимизация по предпочтениям): дверь публикации
по метрикам обучения (`ProjectWikiDpoPublicationMetricGate`) и четырнадцать
дверей конвейеров обучения, которые обращаются к правилу через её прежние имена.
И двери выбора по обученной оценке: модель обоснования потока по подсказке
(`PromptFlowGroundingModel`), машина плана оркестрации
(`OrchestrationPlanMachine` — точность из чужой передаточной записи при импорте)
и обучаемая модель маршрутизации запросов
(`ProjectWikiRequestRoutingTrainableModel` — выбор лучшего маршрута, запас между
двумя лучшими и уверенность).

Почему правило живёт отдельно от двери публикации. Оно там родилось, но «можно
ли верить этому числу» и «можно ли публиковать эту модель» — разные вопросы.
Модель обоснования и машина плана от двери публикации зависеть не должны:
зависимость выбора от двери публикации направлена неверно. Поэтому определение
переехало в место без собственного мнения о публикации, а дверь публикации
сохранила свои публичные имена и стала таким же спрашивающим — одно понятие,
два обращения, ни одной копии условия.

```csharp
public static class ProjectWikiMeasuredNumberRule
{
    // Общее человеческое название причины отказа «число не измерено»
    public const string NotMeasuredReasonSuffix = "not_measured";
    // Единственное определение измеренности для всего домена
    public static bool IsMeasured(double value)
    // Короткая форма для дверей, читающих несколько чисел; пустая ссылка —
    // отказ, потому что «не смог подтвердить» не равно «подтвердил»
    public static bool AnyNotMeasured(params double[]? values)
}
```

ПОРЯДОК: отказ по причине «число не измерено» стоит ДО сравнения с порогом.
Одного сравнения недостаточно: любое сравнение с «не-числом» ложно, поэтому
дверь «отказать, если показатель ниже порога» при неизмеренном числе молча
отвечает «порог пройден», а плюс бесконечность больше любого порога, и
положительная форма проверки объявляет её успехом. Взятие минимума и
ограничение отрезком защитой не являются: минимум единицы и «не-числа» — снова
«не-число». Причина отказа «число не измерено» отличима от «показатель ниже
порога»: первая лечится починкой измерения или обученного состояния, вторая
улучшением модели.

ГРАНИЦА: правило закрывает ровно «не-число» и обе бесконечности. Конечное
бессмысленное значение — например точность на отложенной выборке порядка
1e308 — остаётся измеренным по этому правилу и проходит порог. Это открытая
находка под решение владельца: запрет бессмысленного диапазона меняет смысл
приёмки, поэтому здесь он не введён.

### REMOVED contracts (Phase 37 — must not be re-added)
- `PromptFlowIndex` — removed; replaced by `PromptFlowGroundingModel`
- `GenerationRouteMode.KnownFlowRetrieval` — removed
- `RetrievalSource` — removed
- `_flowIndex` field — removed

`FlowReferenceDetector` is NOT a runtime component — build-time corpus/audit tool only.

---

### Заслон маршрутизации: доверенный порог и обе дороги загрузки (2026-11-03)

(Строка очереди приоритета 4 про обычный путь маршрутизации; решения владельца не требует.)

**Что было неправильно.** Заслон отложенной проверки у обучаемой модели маршрутизации
сравнивал три наблюдения с порогами, приехавшими **вместе со снимком**
(`gateEvidence.Minimum...`), а доверенные пороги файла — 0.95, 1.0 и 0.85 — в сравнении
не участвовали вовсе: ими заполнялся только пустой заслон. Снимок со своими нулевыми
порогами и признаком пройденности открывал маршрут при любых наблюдениях. Конечность
чисел не проверялась ни у наблюдений, ни у порогов, поэтому плюс бесконечность была
«больше любого порога» и проходила заслон, а «не-число» давало отказ без названия поля.

**Что стало.** Решение принимает сама проверка, в четыре шага: наблюдения обязаны быть
измеренными числами; сохранённые пороги обязаны совпадать с действующей политикой
(конечность сама по себе разрешённым порогом не является); сохранённый признак
пройденности может только **запретить**; сравнение идёт с доверенными порогами файла.
Причина отказа называет точное поле, а общая прежняя примета сохранена первой, чтобы
прежние читатели предупреждений продолжали её видеть.

Приметы отказа заслона: `request_routing_gate_route_accuracy_not_measured`,
`..._wrong_annotation_rate_not_measured`, `..._average_confidence_not_measured`,
`request_routing_gate_route_accuracy_threshold_untrusted` (и два таких же для двух
других порогов), `request_routing_gate_not_passed_in_snapshot`,
`request_routing_gate_route_accuracy_below_gate` (и два таких же).

**Обе дороги загрузки плановой машины названы и проверяются до изменения состояния.**

| Дорога | Вход | Право назначать пройденность |
|---|---|---|
| передаточная запись | `ImportAsync` → `CreateImportedEvaluationGate` | назначает по числам: происхождение числа уже проверено создателем |
| контрольная точка | `LoadCheckpointAsync` → `ImportCheckpoint` | не назначает: сохранённый признак может только запретить, а заявленный обязан подтвердиться |

Общий валидатор свидетельства один на обе дороги: точность обязана быть измеренным
числом, лежать в отрезке от нуля до единицы и быть не ниже действующего порога 0.75 —
того же, что у местной оценки. Заявленная пройденность, которую собственные числа не
подтверждают, отвергается кодом
`IMPORTED_EVALUATION_GATE_CONTRADICTS_EVIDENCE` **до** любого изменения живого
состояния, поэтому прежние состояние и ответы остаются, и новых ссылок на оценку не
появляется. Честный низкий результат загрузку не запрещает: он просто не становится
пройденным заслоном.

**Почему это важно именно здесь.** Ниже по коду `GetLastPassedEvaluationGate` и
`GetExportAccuracyFromGate` читают **только** признак пройденности: по нему подписываются
ссылка на контрольную точку и ссылка на паспорт модели как проверенные оценкой и выдаётся
точность экспорта. Поэтому сохранённый флаг и был властью, и поэтому его подтверждение
числами стоит до изменения состояния.

**Что уже было закрыто раньше и не переписывалось.** Дорога передаточной записи имела
свой валидатор происхождения: неизмеренная точность, неположительная точность, ссылки из
непроверенной основы и отсутствие опознания оценки отвергались и до этой правки. Открытыми
там были порядок изменения состояния и значение порога: любая положительная точность
объявлялась пройденной.

Проверки: `Tests/ExperimentFixes/RoutingGateTrustedThresholdTests.cs` — обычный путь через
`ProjectWikiRequestRoutingUnit.ExecuteAsync` с входным снимком и обычные члены плановой
машины. Соседняя проверка конечности из другой публикации этот пункт не закрывает: у неё
свой вход и своё доказательство.

### Обобщённый режим: выученный текст плана доходит до ответа при точном совпадении запроса (2026-11-03)

(Перенос из опыта сто восемнадцать; решения владельца не требует, но одну находку для владельца
оставляет — см. конец раздела.)

**Что было неправильно.** Сборка ответа обобщённого режима (`TrainedRouteComposerUnit.RenderMode2Plan`)
заканчивалась строкой с именем режима и возвращала текст. Из выбранного урока в ответ попадал только
его **номер** — строка `style_source=<номер пары>`. Ни одного слова выученного плана в теле не
оставалось никогда, при любом обучении и при любом совпадении запроса. Соседний выученный режим
(`RenderLearnedFlowPlan`) кладёт тело плана дословно через `SerializeChosen`, то есть способ записи у
продукта уже был — он просто не применялся на обобщённой дороге.

**Что измерил опыт.** Два плеча, подменён один фактор — текст плана у одной пары. В выученном режиме
подмена меняла тело ответа: 0 ответов из 2 с текстом урока стало 2 из 2, а по контрольному запросу
тела плеч совпали побайтово. В обобщённом режиме тела обоих плеч совпали по **обоим** запросам, и
выученный текст не дошёл ни разу — 0 из 2; в теле оставалась только ссылка на номер урока. Правка на
копии одного метода довела текст до тела при точном совпадении запроса и ухудшений на остальных
запросах не показала.

**Что стало.** При точном совпадении запроса с запросом выбранного урока в тело дописывается
сохранённый текст плана этого урока тем же продуктовым способом записи, что и в выученном режиме.
Прежние строки остаются на своих местах и выше по тексту: правка **добавляет, а не заменяет**.

| Где | Что делает | Право на тело плана |
|---|---|---|
| `RenderLearnedFlowPlan` | выученный режим | кладёт тело всегда; этой правкой не затронут |
| `RenderMode2Plan` | обобщённый режим | кладёт тело **только** при точном совпадении запроса |
| `RenderHumanExplanation` | режим человеческого объяснения | тела не кладёт; опытом не измерялся, правкой не затронут |

**Условие срабатывания, названное фактически.** В опыте это «точное совпадение запроса». В продукте
такого понятия до правки не было вовсе: единственная мера близости запроса к уроку — доля общих слов
в поиске ближайшего примера (`OrchestrationKnnStore.Jaccard`). Поэтому условие введено отдельным
методом:

| Член | Что сравнивает | Чем это **не** является |
|---|---|---|
| `TrainedRouteComposerUnit.IsExactPromptMatch(prompt, lessonPrompt)` | тексты запросов после сведения пробелов к одному и приведения к нижнему регистру | не мера похожести и не новый порог: совпадение либо точное, либо его нет |

Условие **намеренно строже** существующей меры близости. У запроса с теми же словами в другом порядке
доля общих слов равна единице, и поиск выбирает тот же урок, — но точного совпадения нет, и тела плана
такой запрос не получает. Отсутствующий текст сравнение не роняет: сведение к одной строке превращает
отсутствие в пустую строку.

**Какой режим чем ограничен.** Принадлежность урока режиму обеспечивает **вызывающий**, а не сборщик:
`OrchestrationPlanMachine.GeneratePlanAsync` ищет ближайший пример как
`FindNearest(prompt, PairMode.Mode2Generalization)` в хранилище обобщённого режима. Само хранилище по
режиму не фильтрует — фильтр приходит параметром. Поэтому «тот же режим» здесь — свойство места
вызова, и при появлении второго вызывающего это условие надо назвать заново.

**Чего правка не лечит.** Запрос без точного совпадения по-прежнему получает только номер урока:
обобщения на похожие, а не совпадающие запросы здесь нет. Предметная точность ответа правкой не
обещана — если урок учит неверному плану, теперь это станет видно в теле, а не спрячется за номером.
Режим человеческого объяснения называет номер урока вместо содержания так же, как это делал
обобщённый, и остаётся открытым.

**Следствие для местной оценки, по коду и не измеренное прогоном.** `OrchestrationPlanMachine.EvaluateAsync`
засчитывает строку, если тело ответа содержит ожидаемый ответ. До правки строка оценки, ожидающая
содержание плана, на обобщённой дороге не могла быть зачтена ни при каком обучении: тела плана там не
было вовсе. Теперь она зачитывается при точном совпадении запроса. Это вывод из чтения кода; прогона
оценки не было, доли не измерялись.

**Находка для решения владельца, правкой не закрытая.** При точном совпадении запроса обобщённый режим
всё равно называет себя `generalized_non_lookup` — «обобщённый, не поиск по совпадению». Ровно в том
случае, который правка добавляет, это имя становится неточным. Переименование — продуктовое решение
владельца.

Проверки: `Tests/ExperimentFixes/GeneralizedModePlanBodyTests.cs` — обычным путём через
`OrchestrationPlanMachine.GeneratePlanAsync` с одним обученным уроком обобщённого режима, плюс прямая
сверка строгости условия с долей общих слов у `OrchestrationKnnStore.FindNearestWithScore`.

## 26.TrainableRouter — `TrainableRouter.dll`

### TrainableRoutingFlow
```csharp
// Implements ILogicalFlow<RoutingRequest>; FlowId = "trainable-routing-flow"
Task<OperationResult<UnitScoringResult[]>> ScoreAsync(RoutingRequest request, CancellationToken ct = default)
Task<OperationResult<RoutingDecision>> RouteAsync(RoutingRequest request, CancellationToken ct = default)
Task<OperationResult<LogicalFlowResult>> ExecuteAsync(LogicalFlowRequest<RoutingRequest> request, CancellationToken ct = default)
```
Input: `RoutingRequest` with prompt, task slice, optional direction/domain hint, candidate unit IDs, and context.
Route gate: `RoutingFlowThresholds.MinimumRoutingConfidence` and `MinimumScoreGap` must both pass before a scored unit becomes `ChosenUnitId`.
Knowledge edge: low confidence or ambiguous top-two score gap returns `RoutingDecision.KnowledgeEdge=true`, `ChosenUnitId=null`, preserved `RankedCandidates`, `RoutingScoreGapEvidence`, and `RoutingKnowledgeEdge` reason evidence. `ExecuteAsync` surfaces `OverallVerdict="ROUTING_KNOWLEDGE_EDGE"` and does not claim target-unit success.
Locator boundary: `DistributedRoutingBootstrap.RouteAndLocateAsync` returns `OperationResult.Fail(..., "ROUTING_KNOWLEDGE_EDGE")` before calling `IRemoteUnitLocator.LocateAsync` when the routing decision is a knowledge edge.
Training runner behavior: `RoutingTrainingRunner` records knowledge-edge cases and decision evidence, but does not count them as accepted/correct routes and does not turn them into accepted DPO pairs.
Profile metadata boundary: `RoutingProfileTrainer` learns keywords from `RoutingTrainingExample` rows, but role, domain, baseline prior, allowed inputs, confidence floor, and provenance must come from `RoutingUnitMetadataRegistry`. Unknown unit IDs fail with `ROUTING_UNIT_METADATA_REQUIRED` unless explicit general metadata is registered. The default orchestration curriculum supplies explicit registry entries for orchestration, translation, and documentation units.

### Routing profile metadata records
```csharp
record UnitRoutingMetadata(string UnitId, string Role, string Domain, string[] AllowedInputs, double ConfidenceFloor, string MetadataSourceRef, double BaselinePrior = 0.0)
record UnitScoringProfile(string UnitId, string UnitRole, string DomainTag, string[] Keywords, string[] AllowedInputs, double ConfidenceFloor, string MetadataSourceRef, double BaseScore = 0.0)
```
`UnitScoringProfile` carries metadata provenance so a published profile snapshot does not hide why a role/domain/floor was assigned. Unit-id substring inference is not a runtime metadata source.

### Routing decision evidence records
```csharp
record RoutingFlowThresholds(double MinimumRoutingConfidence, double MinimumScoreGap, string ThresholdsRef)
record RoutingScoreGapEvidence(string TopUnitId, string? SecondUnitId, double TopScore, double? SecondScore, double ScoreGap, bool MeetsMinimumScoreGap)
record RoutingKnowledgeEdge(bool KnowledgeEdge, string ReasonCode, string Explanation)
record RoutingDecision(string RequestId, string? ChosenUnitId, UnitScoringResult[] RankedCandidates, double Confidence, string Reason)
record RoutingDecisionRecord(string RunId, string RequestId, string Prompt, string TaskSlice, string ChosenUnitId, double ChosenScore, UnitScoringResult[] RejectedCandidates, string CorrectReason, string WrongRouteConsequence, DateTimeOffset DecidedAtUtc)
```
`RoutingDecision` and `RoutingDecisionRecord` carry `KnowledgeEdge`, `ScoreGap`, and `ThresholdsRef` properties so later DPO repair can tell why a route was not accepted.

---

## 27.TranslationDomain — `TranslationDomain.dll`

Active training domain for RU↔EN and HE↔EN translation. Declared in `27.TranslationDomain/`.
Implements `ILearningMachine<TranslationDomain>` (from `00.Core/LearningMachineContracts.cs`, namespace `AgentInfrastructure.Logical`).

### TranslationMachine
```csharp
// Implements ILearningMachine<TranslationDomain>
// Three trainable units: SemanticTokenizerUnit, ExecutorUnit, QualityVerifierUnit
Task<OperationResult<LearningBatchResult>>      TrainBatchAsync(LearningBatch<TranslationDomain> batch, CancellationToken ct = default)
Task<OperationResult<LearningBatchResult>>      TrainStructuredBatchAsync(TranslationLearningBatch batch, CancellationToken ct = default)
Task<OperationResult<LearningEvaluationResult>> EvaluateAsync(LearningEvaluationSet<TranslationDomain> evalSet, CancellationToken ct = default)
Task<OperationResult<LearningMachineExport>>    ExportAsync(CancellationToken ct = default)
Task<OperationResult<bool>>                     ImportAsync(LearningMachineExport export, CancellationToken ct = default)
Task<OperationResult<ILogicalOrchestrator>>     GetOrchestratorAsync(CancellationToken ct = default)
bool                                            HasStructuredTrainerFor(string unitId)
const string                                    StateVersionMetricKey = "state_version"
```
Implemented by: `TranslationMachine` (`27.TranslationDomain/TranslationMachine.cs`)
Test status: 44 tests passing as of phase 24 (2026-05-29, commit `a5e74154`)

**Доставка подтверждённого исправления обучаемой единице** (2026-11-09, раздел
R34-FB карточки исправляющего урока и адреса перехода).

Что было сломано. Проверенную арбитрами пару «годный ответ / отвергнутый ответ»
продукт собирал и ВОЗВРАЩАЛ вызывающему как данные
(`22.AgentAlgorithms/Pipeline/InternalFirstAiSupport.cs`). Назначенная
переводческая единица её не получала, то есть исправляющий ответ уроком не
становился: модель не меняла своего ответа.

`27.TranslationDomain/TranslationCorrectionLearningAdapter.cs` закрывает именно
этот разрыв. `BuildLesson` переводит одну подтверждённую коррекцию
(`TranslationVerifiedCorrection`) в один урок той формы, которую читает
существующий получатель обучения, и НИЧЕГО не записывает; весь допуск стоит
здесь. `DeliverAsync` отдаёт этот урок в существующий
`TrainStructuredBatchAsync` назначенной единицы. Новой памяти переводов нет.

Допуск до записи называет отказы поимённо:
`CORRECTION_WITHOUT_VERIFIED_ANSWER` (отказ проверки без проверенного
правильного текста урока не создаёт), `REJECTED_EQUALS_VERIFIED_ANSWER`,
`REJECTED_ANSWER_SUBSTITUTED_BY_SOURCE` (на месте выданного ответа стоит
непереведённый источник), `VERIFICATION_PROVENANCE_REQUIRED`,
`UNKNOWN_LEARNING_RECEIVER` и `CORRECTION_LESSON_NOT_APPLIED`.

`HasStructuredTrainerFor` существует ради последних двух. Приёмник партий
строку с неизвестным получателем МОЛЧА пропускает: партия объявляется успешной,
строка считается обученной, а продвинутых единиц ноль — урок не дошёл ни до
кого. Читающий запрос позволяет спросить про получателя ДО записи; поведение
самого приёмника он не меняет.

Границы. Это НЕ общий механизм обратной связи и НЕ готовый общий адаптер
обратной связи: доставляется пара одной названной единице. Глобального
маршрутизатора и автоматического учителя здесь нет — решение «доставить»
принимает вызывающая сторона, и автоматическая связь производителя пары с
доменным получателем не создана. Повторная доставка одного опознавателя урока
остаётся явно открытой предпосылкой: договор получателя на это ещё не назван, а
молчаливое слияние по тексту вместо опознавателя запрещено.

Покрытие: `Tests/TranslationCorrectiveLearningTests.cs` — пять опор непустоты и
шесть решающих проверок, включая панель «ответ до доставки / ответ после» через
настоящего продуктового читателя и доказательство неизменности памяти соседних
единиц.

**Атомарная партия и согласованная публикация версии состояния** (2026-11-02,
пункт четвёртый адресного плана ремонта после слияния). Машина держит одну
изменяемую ссылку на неизменную опубликованную версию состояния: поток вывода,
обучатели единиц, типизированные единицы, номер версии, точность, история
обучения и опознания учебных строк лежат в ней вместе.

`TrainStructuredBatchAsync` обучает партию на закрытой копии всех изменяемых
частей, выполняет назначенное измерение на отложенных строках по этой же копии
и публикует новую версию одной заменой ссылки. Поэтому партия применяется
целиком или никак: сбой строки внутри обучения не оставляет принятой половины,
и восстанавливать состояние извне не требуется.

**Публикация условна по прочитанной версии** (2026-11-02, находка аудита о потере
правки одного из двух писателей).

Атомарная замена ссылки сама по себе от потери обновления НЕ спасает.
Согласованность каждой отдельной публикации сохранялась и прежде — читатель
никогда не видел полуготового графа, — но два писателя, прочитавшие версию семь,
каждый готовили свою версию восемь и каждый её публиковали, и работа первого
исчезала без следа и без отказа.

Теперь публикация выполняется сравнением с ожидаемой основой: если версия ушла
вперёд, записи не происходит, живое состояние остаётся прежним, обученный
кандидат сохранён, и повторное обучение автоматически НЕ запускается — повторная
попытка это отдельное решение вызывающего.

```csharp
int CurrentStateVersion { get; }        // номер опубликованной версии, видимый снаружи
Task<OperationResult<LearningBatchResult>> TrainStructuredBatchAsync(
    TranslationLearningBatch batch, CancellationToken ct = default,
    int? expectedStateVersion = null)   // необязательная оптимистичная блокировка
Task<OperationResult<LearningEvaluationResult>> EvaluateAsync(
    LearningEvaluationSet<TranslationDomain> evalSet,
    CancellationToken ct = default)     // ЧЛЕН контракта: ровно два довода
Task<OperationResult<LearningEvaluationResult>> EvaluateAtVersionAsync(
    LearningEvaluationSet<TranslationDomain> evalSet,
    int? expectedStateVersion,          // названная вызывающим версия для измерения
    CancellationToken ct = default)
```

**Почему у измерения два входа, а не один довод** (2026-11-03, починка сломанной
реализации контракта). Обучение партии (`TrainStructuredBatchAsync`) — метод
самой области перевода, поэтому необязательный довод версии стоит у него
последним и никого не ломает. Измерение качества — ЧЛЕН контракта обучаемой
машины, и его подпись обязана совпадать с объявленной дословно. В C# метод с
лишним доводом, пусть и необязательным, НЕ реализует член интерфейса: совпадать
должна подпись, а не только имя. Прежняя правка добавила к измерению третий
довод, и машина перевода перестала удовлетворять контракту — область перестала
собираться. Сборка области перевода опытами не проверяется, поэтому поломка
дошла до ветки незамеченной.

Поэтому проверка версии при измерении живёт в отдельно НАЗВАННОМ методе, а не в
перегрузке по числу доводов: необязательные доводы вместе с перегрузками дают
неоднозначность вызова. Член контракта делегирует ему с пустой ожидаемой
версией, поэтому прежние вызывающие работают без правок. Когда довод назван,
обучение или измерение не начнётся на другой версии, и отказ называет обе
величины.

Ловушка на этот класс поломки:
`Tests/ExperimentFixes/TranslationMachineContractSignatureTests.cs` присваивает
машину переменной типа `ILearningMachine<TranslationDomain>` и вызывает через неё
все пять членов. Такое присвоение не компилируется при расхождении подписи, и
расхождение падает на сборке проверок, а не доезжает до чужого проекта.

**Измерение привязано к версии, которую измеряли.** Прежде измеренная точность
записывалась поверх самой свежей версии. Довод был разумным — не отбрасывать
назад публикацию, прошедшую во время измерения, — но следствие хуже болезни:
число, снятое со старого состояния, приписывалось новой версии, то есть качество
объявлялось про то, чего никто не измерял. Теперь запись условна по измеренной
версии, и при расхождении возвращается отказ с обеими личностями.

Коды отказа версии состояния:

- `STATE_VERSION_CHANGED_DURING_TRAINING` — версия ушла вперёд с того момента,
  как писатель прочитал свою основу; публикация отменена;
- `MEASURED_STATE_VERSION_CHANGED` — измеряли одну версию, а записать число
  пытаются к другой; записи не было.

Приёмка: `Tests/ExperimentFixes/TranslationStateVersionLostUpdateTests.cs` —
четыре проверки: два писателя от одной версии (второй отвергнут, работа первого
цела и читается обычным читателем); измерение, снятое для одной версии, не
записывается к другой; при отказе публикации живое состояние не тронуто, история
обучения не выросла и повторного обучения не было; законная одиночная публикация
по-прежнему проходит, в том числе прежним вызовом без условия по версии.
Отрицательные проверки идут настоящим механизмом публикации: номер версии читается
открытым свойством, а условие по версии — обычный продуктовый довод, доступный
любому вызывающему, а не подставной шов для проверки.

Коды отказа партии (English codes, Russian meaning):
- `BATCH_ROW_UNREADABLE` — текст строки не разбирается как документ JSON
  (JavaScript Object Notation, текстовый формат записи данных); проверка идёт
  до обучения;
- `BATCH_ROW_TRAINING_FAILED` — строка негодна по типовому договору своего
  получателя обучения (например значение вне перечисления понятий); отказ несёт
  опознание виновной строки, её получателя, партию и настоящую причину сбоя;
- `BATCH_STAGING_FAILED` — закрытую копию состояния собрать не удалось;
- `BATCH_ROW_CHOSEN_OUTPUT_NOT_OBJECT` — текст правильного ответа строки корректен
  как документ, но его корень не объект: пустой массив, число, строка или истина.
  Образец предложения у такого значения прочитать нельзя. Отказ управляемый и
  происходит до сборки закрытой копии, поэтому состояние переводчика не меняется.
  Пустое значение (`null`) этим кодом НЕ отвергается: это прежний разрешённый
  тихий пропуск обучателя с обычным учётом принятой партии.

**Проверка вида значения перед чтением свойства** (2026-11-02, находка аудита по
моей же правке). Вид значения корня разобранного документа проверяется ДО любого
чтения свойства. Прежде проход сразу читал свойство у корня: для корректного, но
не объектного значения библиотека разбора выбрасывает исключение неверной
операции, а перехвачено было только исключение разбора текста — поэтому на таком
входе падал весь проход и собственный отказ при столкновении ключей до него не
доходил. Проверка вида предпочтена расширению перехвата: перехват маскирует, а
проверка вида позволяет назвать фактический вид в причине отказа.

Тот же класс защиты добавлен ещё в двух местах слоя перевода, найденных свипом:
загрузчик семенного корпуса (`SeedCorpus/TranslationSeedCorpusLoader.cs`, строка
файла может быть корректным документом и не объектом) и чтение паспорта учебных
данных (`TranslationTrainingDataArtifactStore.cs`). В каноническом вычислителе
ключа, конструкторе партий и обучаемых единицах чтений свойств нет вовсе —
защищать там нечего. Остальные чтения в слое уже были защищены проверкой вида
выше по потоку.

Приёмка: `Tests/ExperimentFixes/TranslationBatchChosenOutputValueKindTests.cs` —
три входа: пустой массив (отказ с названным видом и без публикации), пустое
значение (прежний пропуск обучателя и обычный учёт партии), обычный объект
(столкновение ключей по-прежнему обнаруживается).

`GetOrchestratorAsync` выдаёт дирижёра с решателем текущей версии: дирижёр,
выданный ДО публикации, после неё работает с новой версией. Версия разрешается
ровно один раз на каждый `RunAsync`, и её номер возвращается в показаниях
результата потока под ключом `StateVersionMetricKey`, поэтому по ответу видно,
какая версия состояния его обслужила.

Приёмка: `Tests/TranslationBatchAtomicVersionPublicationTests.cs`; запускатель
прогона и сохранённые выводы — `Tests/BatchAtomicPublicationGate/`.

**Единый рабочий ключ урока и отдельное происхождение** (2026-11-02, пункт пятый
адресного плана ремонта после слияния, подшаги «а» и «б»).

Рабочий ключ урока — строка структурного адреса, под которой урок кладут в память
при обучении и по которой его ищут при обычном чтении. Ключ зависит только от
смысла и структуры текста. Происхождение урока — номер учебной пары — живёт
отдельным полем учебной строки и в ключе не присутствует.

```csharp
// 27.TranslationDomain/TranslationSemanticKey.cs
static class TranslationSemanticKey
{
    const int    SchemaVersion       = 2        // ключ канонический, происхождение отдельно
    const int    LegacySchemaVersion = 1        // ключ мог нести отрезок происхождения
    const string ProvenanceSegmentPrefix = "PAIR:"

    string               Compose(string? structuralAddress)   // собрать канонический ключ
    string               Canonicalize(string? semanticKey)    // убрать отрезки происхождения
    bool                 CarriesProvenance(string? semanticKey)
    IReadOnlyList<string> ExtractProvenance(string? semanticKey)
}

// 27.TranslationDomain/TranslationUnitContracts.cs — два поля учебной строки
record TranslationLearningExample(
    ...,
    string? PairId = null,                                   // происхождение урока
    int? KeySchemaVersion = null)                            // форма выводится, если не названа
```

Канонический ключ считается ОДНИМ способом у всех читателей урока:

| Читатель (человеческое название) | Где считает ключ |
|---|---|
| Конструктор учебных партий (писатель) | `TranslationStructuredBatchBuilder.BuildGeneralBatch` — `Compose` |
| Память ближайших соседей | `ExecutorUnit.TrainOnExample` при записи, `TranslationKnnStore.ImportCheckpoint` при загрузке |
| Разборщик смысла | `SemanticTokenizerUnit.TrainOnExample`, `Tokenize`, `ImportCheckpoint` |
| Исполнитель | `ExecutorUnit.TrySelectKnnCandidate` — `Canonicalize` полного, префиксного и структурного ключей перед каскадом; `Execute` выбирает ветвь без учёта происхождения |
| Проверяющий качества | `QualityVerifierUnit.TrainOnExample`, `Verify`, `ImportCheckpoint` |

`Canonicalize` убирает только отрезки происхождения независимо от их места.
Текущий конструктор хранит `PairId` отдельно и добавляет проверенное
`RULE:TENSE=PAST`. В предъявленном старом ключе `RULE:TENSE_PAST` сохраняется
дословно, в том числе после `PAIR`: срезка всего хвоста уничтожила бы правило.

Загрузка контрольной точки прежней формы приводит ключи к канонической форме в
памяти; сам файл контрольной точки только читается и остаётся воспроизводимым.

Новый код отказа:

- `TRANSLATION_SEMANTIC_KEY_COLLISION` — две разные строки заняли один рабочий
  ключ несовместимым содержимым. Возникает в двух местах, и оба раза ДО единой
  записи в живое состояние: при обучении партии (`TranslationMachine.
  ValidateStructuredBatchKeysDoNotCollide`, до сборки закрытой копии) и при
  загрузке сохранённой памяти (`ValidateCheckpointStructure`, до переноса
  значений). Причина отказа называет сам ключ, оба номера строк, оба
  происхождения и оба несовместимых образца предложения.

Порог уверенности `0.60` (`ExecutorUnit.EnRuOrdinaryKnnMinimumSourceSimilarity`)
привязан к НАПРАВЛЕНИЮ английский→русский и к отсутствию известных смысловых
понятий в разборе, а не к происхождению ключа. Там, где смысловые понятия есть,
уже разрешённый пол `0.0` сохраняется; общим запретом порог не становится.
Прежде наличие чужого отрезка происхождения в ключе само уводило обычную строку
в ветку без порога — этой дыры больше нет.

Поправка Q02 от 4 ноября: каскад читает те же канонические адреса, под которыми
память пишет и восстанавливает уроки. Внутреннее наблюдение `BeforeKnnSourceFloor`
передаёт копию реально найденных кандидатов только для чтения до отдельного
порога исполнителя. Пороги самого каскада `0.35/0.25/0.35` сохранены; смысловая
ветвь с полом `0.0` не отменяет эти пороги допуска. Проверки различают пустой
поиск, найденного кандидата и отказ обычного направления при доле общих слов `0.40`.

Приёмка старой формы semantic keys использует явно искусственную запись с `PAIR`,
сохраняя текущую тематическую память версии 2. Это не обещание восстановить тему
из старой source-only карты. Проверка чувствительности меняет только значение
использованной `TopicSourceMap` и повторяет обычный ответ той же загруженной машины;
обучения целевой машины нет, исходные текст и gold остаются прежними.

Названный остаток, НЕ входящий в этот ремонт: кроме происхождения ключ урока
несёт проверенные грамматические пометки (`RULE:...`), которых сырой разборщик в
подпись не кладёт — он держит свои пометки отдельным полем разбора. Это второй
носитель того же класса расхождения, и он требует собственного наряда с
измерением: пометки писателя приходят из проверенной человеком разметки пары, а
пометки разбора — из правил, и совпадение этих двух источников ничем не
обеспечено.

**Объявленная форма ключа сверяется с данными** (2026-11-02, находка аудита по
этой же правке). Поле `KeySchemaVersion` стало необязательным: пустое значение
означает «не названо», и тогда форма ВЫВОДИТСЯ из содержимого ключа методом
`TranslationSemanticKey.DeriveSchemaVersion` — есть отрезок происхождения, значит
форма прежняя. Если строка форму НАЗВАЛА и она противоречит ключу, партия
отвергается кодом `BATCH_ROW_KEY_SCHEMA_VERSION_MISMATCH`, и причина называет обе
величины: объявленную и выведенную. Прежде поле имело значение по умолчанию
«каноническая», и строка со старым ключом молча помечалась перенесённой — это
была ложь о данных. Прежний договор писателей не ломается: строка без названной
формы проходит как раньше. Ни вспомогательный вычислитель, ни само поле записи
переносом не являются.

Приёмка: `Tests/ExperimentFixes/TranslationPairTailSignatureLearningTests.cs`.
Состояние «до» воспроизводимо на вершине `36866e5bcf0`.

Две поправки в самой приёмке по находкам аудита:

- **существование кандидата доказывается до сравнения с порогом.** Прежняя
  проверка порога сразу утверждала «ответа из памяти нет», и такое утверждение
  проходило бы и при пустом наборе кандидатов — сравнивать было не с чем. Теперь
  проверка сначала доказывает, что кандидат существует и достижим обычным
  чтением при доле общих слов единица, и только потом показывает, что при доле
  0,40 его отвергает именно порог направления: 0,40 выше порога допуска каскада
  0,35 и ниже порога направления 0,60;
- **схема и поведение загруженного объекта проверяются отдельно.** Утверждение о
  форме сохранённой подписи вынесено в собственную проверку. Прежде оно стояло
  внутри поведенческой проверки и остановило бы её до обращения к
  восстановленному читателю, то есть поведение загруженного объекта фактически
  осталось бы непроверенным. Падение одной проверки больше не скрывает другую.

### TranslationRoutingFlow
```csharp
// Implements ILogicalFlow<TranslationRoutingRequest>
// FlowId: "translation-routing-flow"
// Registered in LogicalOrchestratorAssistant.CreateDefault()
Task<OperationResult<LogicalFlowResult>> ExecuteAsync(LogicalFlowRequest<TranslationRoutingRequest> request, CancellationToken ct = default)
```
Input: `TranslationRoutingRequest(SourceLanguage, TargetLanguage, TopicDomain, InputText)`
Adapters: RU_EN, EN_RU, HE_EN, EN_HE (in `TranslationDirectionAdapters.cs`)

### Translation DPO reproducibility context

`11.DPO/DpoTrainingReproducibilityContext` is the neutral contract that carries
the ModelServer-owned training context into Translation DPO without making
`27.TranslationDomain` depend on `44.DesignConversionDomain`.

```csharp
record DpoTrainingReproducibilityContext(
    int SchemaVersion,
    ulong RootInitializationSeed,
    string TrainingConfigurationFingerprint,
    string RandomDerivationScheme,
    string InitializationDomain,
    string DropoutDomain,
    string PolicyStreamDomain,
    string ReferenceStreamDomain,
    string FreshLoadStreamDomain,
    string ReproducibilityScope)

record DpoTrainingReproducibilityEvidence(
    DpoTrainingReproducibilityContext Context,
    string ContextSha256,
    IReadOnlyList<string> EvidenceRefs)
```

The coordinator adapts the server random context to this DPO contract and passes
it into `TranslationDpoTrainEvaluateRunnerOptions` and
`TranslationDpoCheckpointLifecycleRunnerOptions`. The runner rejects missing,
invalid, or mismatched context before corpus access, output/checkpoint path
creation, model factories, mutation-admission lease requests, and report writes.

New reports write nullable `ReproducibilityEvidence`. Nullable keeps old JSON
readable, but continuation treats missing evidence as not continuable because it
cannot prove which server seed/config created the initial checkpoint. This
contract currently protects authority and reporting; bit-identical checkpoint
proof and seeded adapter preparation remain the next reproducibility slice.

### TranslationKnnStore — FindNearestCascade
```csharp
// KNN (K-Nearest Neighbours) lookup cascade for translation unit inference
// Full → Prefix → Structure → BootstrapFallback
OperationResult<TranslationKnnMatch> FindNearestCascade(string structuralSignature, string directionTag)
```
Error codes on similarity guard violation: `TRAIN_HELDOUT_OVERLAP`, `TRAIN_HELDOUT_SIMILARITY_OVERLAP` (≥70% Jaccard token overlap)
Declared in: `TranslationTrainedUnits.cs`

Source-overlap floor applied on every tier before the neighbour cap: the
full-signature tier uses the broad floor `SourceAwareBroadSignatureSimilarity`
(0.35), the prefix tier uses `SourceAwarePrefixSimilarity` (0.25), and the
structure tier uses the broad floor again. Overlap is the share of the query's
informative words that appear in the stored source. The full tier previously had
a floor of zero whenever the signature looked detailed, so it returned the first
stored pair of that shape regardless of its words; a structural signature proves
only that two sentences are built alike, never that they are about the same
thing. A tier that admits nothing falls through to the next tier rather than
ending the search, and training stores every learned pair under all three
addresses, so a paraphrase refused by the full tier at 0.35 is still answered by
the prefix tier at 0.25: raising the full floor loses no correct answer. A query
that retains no informative word after the low-information filter is refused by
every floored tier, and so is a call that supplies no query source text.

An optional trailing `candidatePredicate` lets the caller say which stored pairs
it can actually use. It is applied inside each tier BEFORE the neighbour cap, so
the cap keeps the first k of the pairs that already passed admission, and a tier
left empty by admission falls through to the next tier instead of ending the
search; passing nothing keeps the previous behaviour. `ExecutorUnit` supplies its
FULL admission, so a tier holding nothing but unusable pairs no longer reports
"nothing learned" while a usable answer sits one tier down, and a usable entry
standing behind unusable ones in the same tier is now reachable. The tier-stop
decision is taken on the admitted result, not on the capped one. Admission covers
both halves of the caller's judgement: the target must be non-empty and usable for
the direction, and the stored source must meet the caller's source-overlap floor.
No candidate is reordered and no threshold value changed — the executor's ordinary
English→Russian floor is still 0.60 and the lawful semantic branch still has a
floor of zero; only the place where the floor is asked moved.

Historical wording (first transfer, until 9 November 2026, kept on purpose):
"It is applied to each tier's already-ranked and already-capped result ... Because
the filter runs after the neighbour cap, a usable entry that the cap already
dropped is still unreachable, and no candidate is reordered or re-admitted." That
was the real, honestly stated boundary of the first transfer; the assigned
remainder closed it. Pointer: the work row "каскад ищет следующий пригодный
ответ" (row 146 of the work registry
`docs/plans/replanning-possible-diffs/fixes-based-on-experiments/01-registry.md`),
with its child remainder 124-S10 about the executor's own output. Note,
separate from this floor change: the signature block above is
stale against the live declaration in `TranslationUnitContracts.cs` and is
tracked as documentation debt.

### TranslationSemanticTokenizer — language rule tags
```
RU:SUBJECT:FEM, RU:SUBJECT:MASC
RU:TENSE:PAST
RU:VERB_GENDER:FEM, RU:VERB_GENDER:MASC, RU:VERB_GENDER:PLUR
RU:AGREEMENT:SUBJECT_FEM_PAST_FEM
RU:CASE:DATIVE
```
Emitted by `TranslationSemanticTokenizer` (`TranslationSemanticTokenizer.cs`).
UTF-8 Cyrillic methods called first; mojibake fallback (phase 25 repair pending).

### Сохранение проигравшего обученного кандидата (2026-11-03, раздел ремонта R117-03)

**Уточнение Q01, 4 ноября 2026.** CAS возвращает наблюдённую именно этой атомарной операцией версию; все пять вызывающих используют её в отказе, структурный Train — также в артефакте. Если после проигрыша A версии B опубликована C, поле `WinningStateVersionNumber` остаётся B. Поздняя отмена `EvaluateAtVersionAsync` после измерительного барьера возвращает `CANCELLED` без публикации версии/точности/истории; обычный неверный ответ по-прежнему даёт законно измеренную точность 0.

`TatoebaPilotLoopRunOptions.LostCandidateDirectory` задаёт явный каталог кандидатов. Без него default — `lost-candidates` рядом с назначенным `CheckpointPath`; без обоих работает честный no-store режим. Путь входного корпуса не используется как output. Store подключён до обучения у fresh, disk-load и cache-hit, включая source rollover. CLI-команды `regenerate-translation-model-from-training-data` и `continue-translation-training` назначают `output/lost-candidates` до Train; глобальные конструкторы этого не делают.

Отдельный технический читатель `Tests.Tools/TranslationLostCandidateReader` строится зависимостью тестового проекта, загружает checkpoint кандидата и вызывает обычный orchestrator без Train. Source wiring CLI и выполнение публичного Tatoeba API — разные уровни доказательства; установленный CLI runtime и новый семантический прирост этим срезом не заявляются. Свежая приёмка Q01 записывается в [отчёте](../qa-results/q01-concurrency-2026-11-04-20261104091631764/codex-report.md); нижние упоминания отсутствовавшего прогона относятся к историческому состоянию 3 ноября.

Что пользователь получает теперь. Когда два писателя готовят обучение от одной
версии состояния, проигравший больше не теряет свою уже оплаченную работу. Пример:
победитель публикует урок «Он проверил файл» → «He checked the file»; проигравший,
готовивший «Он удалил файл» → «He deleted the file», получает отказ публикации — и
вместе с отказом путь к записи своего обученного состояния, которое другая машина
может прочитать и ответить его уроком.

Как это работает без кода. Кандидат называется отдельной величиной ДО попытки
публикации. Если условная публикация проиграла, фактическое состояние кандидата
собирается из ЕГО держателя версии и ЕГО закрытой копии исполнения тем же чистым
сбором, что и обычная выгрузка, и записывается в отдельную запись. Обычный путь
сохранения здесь непригоден: он читает текущую опубликованную версию, а после
проигрыша текущей является победитель, и под именем кандидата записали бы чужое
состояние.

Где это лежит:

- `27.TranslationDomain/TranslationLostCandidateArtifact.cs` — запись кандидата
  (`TranslationLostCandidateArtifact`), проверяемый указатель
  (`TranslationLostCandidateArtifactRef`), договор хранилища
  (`ITranslationLostCandidateStore`), файловое хранилище
  (`TranslationFileLostCandidateStore`) и общий неделимый писатель текста
  (`TranslationAtomicJsonWriter`).
- `27.TranslationDomain/TranslationMachine.cs` — назначение хранилища
  (`UseLostCandidateStore`, отдельная точка входа: подпись члена интерфейса не
  расширяется), сборка и запись кандидата (`PersistLostCandidate`), условная
  публикация структурной партии.

Границы, которые здесь названы прямо:

- `UseLostCandidateStore` — не член интерфейса обучения. Лишний довод у члена
  интерфейса, даже необязательный, ломает реализацию члена, поэтому назначение
  хранилища живёт своей точкой входа.
- Слово «сохранён» звучит ТОЛЬКО после подтверждения записи: файл перечитывается с
  диска и свёртка перечитанных байтов сверяется со свёрткой записанного текста.
- Поле победившей версии в записи может быть ПУСТЫМ, и это отдельный смысл: победителя
  не было вовсе, публикация не состоялась по отмене, а не проиграла чужой работе.
  Прежде это поле заполнялось номером основы на любом пути отказа, и запись читалась
  так, будто основа «победила» кандидата. Основа всегда названа своим полем.
- Ошибка хранения — отдельный названный отказ `CANDIDATE_PERSISTENCE_FAILED`; живое
  состояние при нём цело, и обещания сохранности нет.
- Пока хранилище не назначено, отказ публикации прямо говорит, что кандидат НЕ
  сохранён. Прежде та же фраза утверждала сохранность на любом пути, ничего не
  записывая.
- Плоский путь обучения (`TrainBatchAsync`) памяти единиц не меняет, поэтому записи
  кандидата там нет по существу: записать «контрольную точку кандидата» из живого
  исполнения значило бы сохранить состояние победителя под чужим именем.
- Свёртка записанных байтов подтверждает сохранённые байты; сверка восстановленного
  состояния идёт по каноническому составу БЕЗ времени выгрузки, поэтому новое время
  выгрузки не создаёт ложного расхождения.
- `BeforeConditionalPublishBarrier` и `BeforeMeasuredAccuracyPublishBarrier` —
  внутренние точки наблюдения перед условной публикацией партии и перед условной
  записью измеренного числа, открытые только сборке проверок. В продукте они пусты и
  поведение продукта не меняют.

### Поздняя отмена не публикует ничего (2026-11-03, тот же раздел ремонта)

Что было неправильно. Отмена проверялась только на входе в метод обучения партии.
Отмена, пришедшая ПОСЛЕ обучения закрытой копии и ДО публикации, публикацию уже не
останавливала: отменённая работа всё равно вставала в живое состояние.

Что стало. Отмена проверяется последним действием перед сравнением-с-заменой
(`27.TranslationDomain/TranslationMachine.cs`, путь структурной партии). При
отменённой работе публикации нет вовсе, живое состояние остаётся прежним целиком,
частичного состояния не возникает, а уже оплаченный обученный кандидат сохраняется
той же записью с причиной отказа «отмена», а не «конфликт версий». Отмена — не
причина терять оплаченную работу молча.

### Измерение держит одну версию (проверка написана, прогона не было)

Завершившееся измерение, снятое на одной версии, не меняет число и личность версии,
опубликованной за время измерения: запись числа условна по измеренной версии
(`EvaluateAtVersionAsync`). Одиночное измерение без конкурента свою оценку
публикует законно.

Проверки этих случаев: `Tests/ExperimentFixes/TranslationWriterConflictAcceptanceTests.cs`
(применение урока ровно один раз, измерение одной версии, одиночное измерение,
ранний отказ по устаревшей названной версии без обучения и без записи кандидата,
поздняя отмена) и `Tests/ExperimentFixes/TranslationLostCandidateArtifactTests.cs`
(сама запись кандидата и её чтение новой машиной).

Чего этот раздел ещё НЕ закрывает: обычный продуктовый потребитель собственной
конкурентной приёмки, применение сохранённой записи к другой основе как отдельная
операция с новым допуском версии; «ровно один раз» доказано по наблюдаемому
следствию — составу обученной памяти и истории обучения, — а не перехватом вызовов
обучателя; сверка восстановленного состояния по каноническому составу остаётся
ГИПОТЕЗОЙ с условием опровержения (если загрузка контрольной точки не является
точным обратным к её сборке по порядку исполнителей, падает именно сверка, а не
доказательство сохранности записи); прогона проверок не было — сборка и тесты в
среде исполнителя запрещены политикой, зелёными они не называются.

---

## 14.ContinuousLearning — `ContinuousLearning.dll`

### IProjectIndexer
```csharp
OperationResult<IReadOnlyList<TrainingExample>> Index(string rootPath)
```
Implemented by: `ProjectIndexer` (14/Indexer)
Injection: requires `ISemanticCompiler29` — never reimplements StackDetector (Principle 4)

### IDomainDatasetCache
```csharp
OperationResult<bool>                       Add(TrainingExample example)
OperationResult<IReadOnlyList<TrainingExample>> GetByDomain(string domain)
OperationResult<int>                        TotalCount()
Task<OperationResult<bool>>                 FlushAsync()
```
Implemented by: `DomainDatasetCache` (14/Cache)

### IFeedbackRouter
```csharp
OperationResult<bool>                     Route(FeedbackEvent feedback)
OperationResult<IReadOnlyList<DpoPair>>   GetDpoPairs()
```
Implemented by: `FeedbackRouter` (14/Feedback)
Weight formula: `D-09: maxWeight = total / (domains × rareExamples)`

### SupervisedTextSequence (internal, 14/Training)
```csharp
const string ProfileVersion = "sft-joint-lf-v1"
const string Separator = "\n"
const string BoundaryUnresolvedErrorCode = "response_boundary_unresolved"
string                                  ComposeJointText(string prompt, string response)
IReadOnlyList<int>                      EncodeJointLf(BpeTokenizer tokenizer, string prompt, string response)
OperationResult<SupervisedResponseSpan> ResolveResponseSpan(BpeTokenizer tokenizer, string prompt, string response)
string                                  ComputeSequenceDigest(string profileVersion, IReadOnlyList<int> fullSequence, int promptLength)
```
Объявлено в: `14.ContinuousLearning/Training/SupervisedTextSequence.cs`
Потребители: `IncrementalTrainer` (писатель обучения),
`Tests/ExperimentFixes/SupervisedResponseReadout.cs` (читатель ответа из
сохранённого состояния и диагностика остановки `SftAcquisitionStop`).

Смысл: это единственный владелец формата учебной последовательности обучения с
учителем (SFT — supervised fine-tuning, дообучение по готовым парам «запрос →
ответ»). Запрос и ответ склеиваются одним переводом строки и кодируются одним
вызовом словаря; тот же формат читают проверка усвоенного ответа и предикат
остановки, поэтому обучение и чтение больше не могут разойтись в разделителе.
Раньше склейка жила прямо внутри тренера, а панель читала ответ без разделителя
и объявляла правильно выученный первый знак ответа ошибкой.

`ResolveResponseSpan` отдельно разбирает границу ответа: знаки запроса вместе с
разделителем обязаны дословно остаться началом учебной строки, иначе чтение
отказывается словами `response_boundary_unresolved`. Этот отказ закрывает только
чтение ответа и право на остановку; обычное обучение на такой строке остаётся
разрешённым, и `EncodeJointLf` по-прежнему отдаёт полные знаки. Номера ответных
целей считаются на сдвинутых целях родного тренера и начинаются с «граница минус
один», иначе первый знак ответа снова потерялся бы.

Границы: это детерминированное представление данных, а не обучаемый выбиратель и
не включённая в продукт политика остановки. Формат прямого сравнения
предпочтений (DPO — Direct Preference Optimization) остаётся прежним и
разделителя не получает: `DpoExactPairMaterial` сохраняет версии
`segmented-byte-BPE-v2` и `prompt-response-segments-no-separator-v2` и раздельное
кодирование запроса, выбранного и отвергнутого ответов.

### DpoSupervisedCorrectionProjection (internal, 14/Training)
```csharp
const string Version = "dpo-sft-correction-v1"
const string PairBatchDigestVersion = "xiigen.dpo-supervised-correction-pair-batch.v1"
const string AmbiguousErrorCode = "DPO_SUPERVISED_CORRECTION_AMBIGUOUS"
const string PairIdentityRequiredErrorCode = "DPO_SUPERVISED_CORRECTION_PAIR_IDENTITY_REQUIRED"
const string InvalidRequestErrorCode = "DPO_SUPERVISED_CORRECTION_REQUEST_INVALID"
bool                                       HasCorrectionCandidate(string domain, IReadOnlyList<TrainingExample>? examples, IReadOnlyList<DpoPair>? admittedPairs)
OperationResult<ProjectedSupervisedBatch>  Project(string domain, IReadOnlyList<TrainingExample>? examples, IReadOnlyList<DpoSupervisedCorrectionCandidatePair>? admittedPairs, string originalBatchSha256)
string                                     ComputeAdmittedPairBatchSha256(IReadOnlyList<DpoSupervisedCorrectionCandidatePair> admittedPairs)
```
Объявлено в: `14.ContinuousLearning/Training/DpoSupervisedCorrectionProjection.cs`
Потребитель: `IncrementalTrainer.TrainCoreAsync` — сразу после допуска пар и до
построения происхождения, знаков учебной строки и отпечатка кандидата.

Смысл: пользователь исправил ответ модели, поэтому плохой текст стал отвергнутым,
а хороший — выбранным. Порядок обучения такой: один шаг прямого сравнения
предпочтений (DPO — Direct Preference Optimization) уводит модель от отвергнутого
текста, затем уроки обучения с учителем (SFT — supervised fine-tuning) закрепляют
ответы. Если ответом урока всё ещё стоит тот самый отвергнутый текст, последний
шаг учит модель именно ему и гасит только что полученное исправление. Проекция
заменяет в учебном наборе ровно такой ответ на выбранный текст той же пары.

Правило выбора: сравниваются домен, запрос и ответ строго дословно
(`StringComparer.Ordinal`), без обрезки пробелов, без приведения регистра и без
разбора синтаксиса. Исправляющей считается только допущенная пара своего домена с
непустым выбранным текстом, дословно отличным от отвергнутого. Ровно одно
совпадение даёт замену; два и более — отказ `DPO_SUPERVISED_CORRECTION_AMBIGUOUS`
до любой мутации модели и до начала мутации транзакции. Нет совпадения — урок
проходит дальше тем же объектом.

Что сохраняется при замене: запрос, домен, вес, время снятия, позиция урока и
число уроков. Меняются только ответ и личность источника. Производная личность
строится штатным `TrainingSourceProvenance.CreateSourceId`: происхождение —
`dpo-sft-correction-v1:{прежний источник}:{номер пары}:{отпечаток набора пар}`,
содержимое — точные байты выбранного ответа в кодировке UTF-8. Номер пары берётся
из записи долговечной транзакции (`TransactionPairSnapshot.PairId`), сверенной с
допущенным набором в исходном порядке; сама пара своего номера не хранит.

Происхождение и отпечаток: сначала штатный `BuildManifest` проверяет уникальность
прежних личностей источника, затем прежние и производные личности объединяются в
родословную — обе идентичности видны в происхождении, новых учебных строк не
появляется. Отпечаток учебного набора без исправления остаётся прежним
`xiigen.dpo-supervised-example-batch.v1`; для исправленного набора
`IncrementalTrainer.BuildCorrectedTrainingExampleBatchSha256` считает
`xiigen.dpo-supervised-example-batch.v2`, который связывает те же поля уроков,
отпечаток набора до замены и упорядоченные записи замен. Повторный расчёт на тех
же данных даёт то же значение, поэтому восстановление попадает в того же
записанного кандидата, а не во второй проход обучения.

Границы: это детерминированный ремонт источника урока внутри уже существующего
пути обучения, а не обучаемый выбиратель, не смысловой поиск коллизий и не новый
уровень архитектуры. Пока пара не допущена, власти менять учебный урок у неё нет.
Существующий `DpoPairTrainingContentAuthority.Project` проверяет саму пару
предпочтений и учебный урок не переписывает — эти две роли не смешиваются.
Формат точного сравнения предпочтений и формат учебной строки
(`SupervisedTextSequence`) не изменяются.

Не реализовано этим срезом: типизированное долговечное поле версии проекции в
записи кандидата (`CandidateProvenanceSeed.SupervisedProjectionIdentity`) и
чтение этой записи обратно через снимок домена транзакции. Сейчас совпадение при
восстановлении обеспечивается повторным детерминированным расчётом отпечатка
второй версии, а не чтением записанных связок.

### IIncrementalTrainer
```csharp
Task<OperationResult<TrainedAdapterArtifact>> TrainAsync(
    string domain,
    IReadOnlyList<TrainingExample> examples,
    IReadOnlyList<DpoPair> dpoPairs,
    CancellationToken cancellationToken = default)
```
Реализация: `IncrementalTrainer` (14/Training)
Делегирует в: `LoraApplicator.Apply` (10.LoRA), `StableDpoTrainingExecutor` →
`LogicalDpoTrainingUnit` → `DpoTrainer` (11.DPO)
Код ошибки: `EMPTY_EXAMPLES`, когда `examples.Count == 0`

Смысл интерфейса: это общий путь изменения LoRA/DPO-весов после допуска
учебных данных. Он возвращает не строку пути, а сохранённый артефакт с
происхождением. В DPO-ветке пары сначала проходят правовой допуск и устойчивую
транзакцию; исполнитель строит `LogicalDpoPreferenceExample`, вызывает
стабильный `11.DPO:DpoTrainer`, проверяет опубликованные файлы контрольной
точки и метрик и только после принятой транзакции может публиковать
репозиторную дочернюю контрольную точку. Это реальное изменение весов общего тренера,
но оно не доказывает, что `continue-orchestration-training` уже использует этот
путь.

Контракт репозиторного старта: перед публикацией новой версии обучение получает
не просто папку вывода, а подготовленный запрос старта. В нём указано, от какой
предыдущей контрольной точки продолжается модель, или почему разрешён первый
холодный старт. После принятой устойчивой DPO-транзакции публикуется новая
версия полной модели: для продолжения это следующий шаг после родителя, для
первого запуска — начальный шаг с записанной причиной. Перед записью весов и
манифеста код сверяет, что запрос старта связан с тем же сохранённым DPO-набором
и тем же паспортом набора, который прошёл допуск для принятых строк. Если связь
отсутствует или указывает на другие файлы, публикация останавливается до записи
новой контрольной точки. Этот первый опубликованный шаг нужен следующему
процессу: без него продолжение снова стартовало бы с нуля.

Техническая справка: запрос представлен
`RepositoryOwnedIncrementalTrainingSessionStartRequest`; продолжение публикует
`parent.Step + 1`; холодный старт публикует шаг `000001` с
`ParentCheckpoint = null`; связь с DPO-набором сверяется по пути манифеста и
его SHA-256 до записи `weights.bin`, `manifest.json` и промежуточных файлов
публикующего кода.

**Вес пары ровно ноль исключает её вклад** (2026-11-02, перенос результата опыта
сто четырнадцатого в продукт).

Владелец изменения: `11.DPO/DpoTrainer.cs`, метод `DpoTrainer.StepVectorCore`
(векторный путь шага).

Что было неправильно. Вес каждой пары дважды проходил через нижний порог одной
сотой — в знаменателе усреднения и в весе самой пары. Поэтому пара с весом
РОВНО ноль вносила вклад двумя путями: считала собственный градиент и раздувала
знаменатель, отнимая влияние у нужной пары. Платил набор за такую пару полную
цену обычной пары.

Что измерено. Опыт `experiments-v2/RP01-MICRO/P11401` снял это на двух парах из
одного восстановленного обученного старта:

| Показание | Прежний код | Исключение нуля | Контроль без лишней пары |
|---|---:|---:|---:|
| Знаменатель усреднения | 1,01 | 1,0 | 1,0 |
| Относительный вес нужной пары | 0,990099 | 1,0 | 1,0 |
| Собственный градиент нулевой пары | −0,0053630364 | не считался | пары нет |
| Прямых/обратных проходов на нулевую пару | 4 / 2 | 0 / 0 | пары нет |
| Отпечаток обучаемых весов после шага | отличается | совпал с контролем | — |
| Наибольшее расхождение значения веса с контролем | 3,2 · 10⁻⁷ | 0 | — |

Что стало. Активные пары отбираются по условию «сырой вес не равен нулю» —
после допуска и до живого серверного разрешения на изменение весов. Знаменатель
и нормированные веса считаются только по активным парам; цикл идёт только по
ним, поэтому исключённая пара не стоит ни одного прохода модели. Положительные
веса нормируются как прежде, нижний порог одной сотой для них сохранён.

Допуск весом не подменяется: нулевой вес исключает ВКЛАД пары, а не её допуск к
обучению. Проверка допуска проходит по всем парам набора, включая исключённые.

Случай без обновления. Набор, в котором активных пар не осталось, возвращает
показания с новым полем `DpoMetrics.UpdateApplied = false` и не делает шага:
серверное разрешение не берётся, имя контрольной точки не резервируется,
обновитель весов не вызывается, файлы контрольной точки и показаний не
публикуются. Поле необязательное: пусто означает «не сказано» — так выглядят все
остальные пути шага и все квитанции, записанные до его появления, поэтому
уже записанные квитанции читаются и записываются обратно теми же байтами.

**Названная разница в поведении.** Набор, где ВСЕ веса равны нулю, прежде обучал
в полную силу: нижний порог поднимал каждый вес до одной сотой, и пары делили
влияние поровну (знаменатель 0,02, нормированные веса по половине). Теперь такой
набор не обучает ничему. Это исправление ошибки, а не ухудшение: вес ноль
означает «эта пара не учит», и набор из одних таких пар не может учить сильнее,
чем набор из единиц.

**Чего эта правка НЕ лечит.** Любой ПОЛОЖИТЕЛЬНЫЙ вес ниже одной сотой нижний
порог по-прежнему поднимает до одной сотой, поэтому вес одна миллионная и вес
одна сотая остаются неотличимы: знаменатель 1,01 и нормированные веса
0,990099 / 0,00990099 в обоих случаях. Это вторая половина той же проблемы, она
требует своего наряда. Про отрицательные веса, не-а-числа и бесконечности эта
правка не говорит ничего: подслучай не запускался.

Чего измерение НЕ доказывает: улучшения качества ответов. В опыте при одном
обновлении ни один ответ панели не сдвинулся, и доля верных ответов осталась
равной порогу вырождения.

Потребители механизма в продукте: обучающая логическая единица подготовки
способностей (`18.CapabilityTraining/LlmIngredientLogicalUnits.cs`) и
непрерывное обучение на документации кода
(`14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs`).

Приёмка: `Tests/ExperimentFixes/DpoZeroWeightPairExclusionTests.cs` — три
проверки: нулевая пара не меняет обученное состояние по сравнению с обучением
без неё; нужная пара не разбавляется и исключённая пара не стоит ни одного
прохода модели; набор из одних нулей даёт случай без обновления, а не шаг.

Целевой контракт независимого цикла качества оркестрации на границе интерфейса
ещё отсутствует: вызывающий код должен открыть последнее принятое состояние,
запустить отдельного проверяющего оркестрации до запроса к учителю, сохранить
одну новую DPO-пару до применения, сохранить полный снимок до пары, применить
только эту пару, сохранить и открыть полную пробную версию после пары, оценить
её снова, затем принять только улучшение выше `MeasurementNoise` или вернуть
снимок до пары. Отложенные экзаменационные строки не должны попадать в запрос
учителю; старые сохранённые учебные пары могут входить только как примеры, а не
как автоматический повтор прошлых изменений весов.

### IDomainModelRegistry
```csharp
OperationResult<bool>          Register(ModelAdapter adapter)
OperationResult<ModelAdapter?> GetAdapter(string domain)   // Value null = not found, still IsSuccess
OperationResult<IReadOnlyList<ModelAdapter>> ListAll()
```
Implemented by: `DomainModelRegistry` (14/Registry)

### Frozen reference corpus foundation
```csharp
enum FrozenReferencePartition { Train, HeldOut }
record PaidReferenceProvenance
record FrozenReferenceEntry
record FrozenReferenceCorpusSplitProof
record FrozenReferenceCorpus
record ReferenceMatchingRuleIdentity
record ReferenceMatchingCalibrationPair
record ReferenceMatchingCalibrationInput
delegate bool ReferenceMatchingRulePredicate(string candidateAnswer, string referenceAnswer)
record ReferenceMatchingRuleCalibrationResult
static class ReferenceMatchingRuleCalibrationEvaluator
record RepositoryOwnedReferenceMatchingCalibrationManifest
record RepositoryOwnedReferenceMatchingCalibrationOpenResult
static class RepositoryOwnedReferenceMatchingCalibrationManifestSealer
record DpoPairReferencePreferenceMatchingResult
static class DpoPairReferencePreferenceMatchingRule
record OrchestrationReferenceStep
record OrchestrationReferenceDependencyEdge
record OrchestrationReferenceDecompositionPayload
record OrchestrationReferenceMatchResult
static class OrchestrationReferenceMatchingRule
record ProjectWikiReferenceEvidencePayload
static class ProjectWikiCodeDocumentationReferenceMatchingRule
static class ProjectWikiAngularCodeDocumentationReferenceMatchingRule
static class ProjectWikiSqlCodeDocumentationReferenceMatchingRule
```
Declared in: `14.ContinuousLearning/Core/FrozenReferenceCorpus.cs`
ProjectWiki adapter declared in:
`42.ProjectWikiDomain/ProjectWikiReferenceEvidencePayload.cs` and
`42.ProjectWikiDomain/ProjectWikiCodeDocumentationReferenceMatchingRule.cs`
and
`42.ProjectWikiDomain/ProjectWikiAngularCodeDocumentationReferenceMatchingRule.cs`
and
`42.ProjectWikiDomain/ProjectWikiSqlCodeDocumentationReferenceMatchingRule.cs`

Гарантии этого слоя: frozen corpus строится только из одного `DomainTag` и
одного `SubjectId`; `CorpusVersion` и caller-supplied UTC `FrozenAtUtc` входят в
canonical SHA-256; train/held-out не могут делить один
`SourceEntityFingerprint`; calibration pair обязан ссылаться на конкретный
`ReferenceEntryId`, а hash его reference text должен совпасть с paid-reference
hash этого entry. `PaidReferenceProvenance` намеренно закрыт как paid reference:
его output не trainable и не может стать chosen/rejected стороной DPO.

Generic DPO preference measurement consumer —
`14.ContinuousLearning/Core/DpoPairReferencePreferenceMatchingRule.cs` — accepts
an existing rich `ContinuousLearning.DpoPair`, the matching
`DpoTrainerPairAdmission`, a concrete `FrozenReferenceEntry`, a subject id, the
already-selected subject `ReferenceMatchingRuleIdentity`, and the already
existing `ReferenceMatchingRulePredicate`. It verifies domain/subject/owner
input binding before matching: pair domain, rule identity domain and reference
entry domain must agree; caller subject id must equal the entry subject; the
normalized pair prompt must equal `FrozenReferenceEntry.OwnerInput`.

It also checks legal side provenance without minting a new trainer authority:
the admission fingerprint must bind to the exact Prompt/Chosen/Rejected bytes,
the rich pair side authorship must be present and agree with admission author
kinds, and `PaidExternalModel` is forbidden on either side. The paid reference
answer is used only as the subject predicate's reference argument and is not
stored in chosen/rejected/result content; if its normalized hash equals either
DPO side, evaluation returns copied-paid-reference status before predicate
execution. Accepted means exactly chosen matches reference and rejected does
not. Both-match tie, neither-match and rejected-only are distinct fail-closed
statuses. Nonfatal predicate exceptions return
`DPO_PAIR_REFERENCE_PREFERENCE_PREDICATE_FAILED`; cancellation propagates under
the same snapshot exception policy as the generic calibration evaluator. There
is no score, distance, threshold or self-score surface. The result carries the
exact `DpoTrainerPairAdmission.ContentFingerprintSha256` and binds it into the
canonical result hash under `admission_content_fingerprint_sha256`; this gives
the measurement an audit handle for the exact admitted pair without storing raw
Prompt/Chosen/Rejected bytes or the paid answer.

Generic sealer/opener for repository-owned frozen snapshots уже существует
поверх этого foundation layer. Первый предметный consumer —
`27.TranslationDomain/LanguageTranslateReferenceMatchingRule.cs`: он строит
translation-only `ReferenceMatchingRuleIdentity`, отдаёт
`ReferenceMatchingRulePredicate(string candidateAnswer, string referenceAnswer)`
и запускает общий `ReferenceMatchingRuleCalibrationEvaluator` без копирования
generic evaluator. Predicate использует
`LanguageTranslateHeldOutScorer.ScoreOne(referenceAnswer, candidateAnswer)` и
fail-closed возвращает `false` на invalid/oversized/invalid-Unicode scoring
input.

DesignConversion consumer —
`44.DesignConversionDomain/DesignConversionReferenceMatchingRule.cs` — строит
domain-only identity для `design-conversion`, subject
`design-tree-to-markup` и pattern `design-to-code`. Predicate сохраняет generic
delegate shape `ReferenceMatchingRulePredicate(string candidateAnswer, string
referenceAnswer)`, парсит canonical DesignConversion payloads и проверяет raw
JSON как contract surface, а не как доказательство серверной аутентификации:
caller должен передать сериализованный canonicalized trust-boundary output.
Rule требует роли `student_candidate` → `paid_reference` и fail-closed
отвергает malformed, wrong-schema, wrong-domain/subject/pattern, swapped-role,
unrecognized visual contract и below-threshold visual/markup scores. Local PNG
contract recognized только при exact field-bound refs, которые выдаёт live
`ServerOwnedLocalPngComparisonProvider`: `local-png-comparison:{ComparisonRequestId}`,
`expected-sha256:{ExpectedImageSha256}`, `rendered-sha256:{ActualImageSha256}`,
`diff-sha256:{DiffImageSha256}`, ProviderKind, Boundary и canonicalized marker.
Copied marker-only JSON остаётся rejected, а external receipt policy здесь не
включён. Ключевые связи: candidate comparison должен иметь тот же
`expected_image_id` и `expected_image_sha256`, что frozen paid reference, и тот
же `markup_axis.expected_markup_fingerprint`; две независимые self-score записи
с разными expected reference/markup fingerprint не являются match. Calibration
numbers снова считает общий `ReferenceMatchingRuleCalibrationEvaluator`, без
предметного copy-paste evaluator.

Orchestration consumer —
`26.TrainableRouter/Training/OrchestrationReferenceMatchingRule.cs` — строит
identity для domain `programming-task-orchestration`, subject
`orchestration-task-decomposition`, pattern `ilearning-orchestrator`, schema
`xiigen.orchestration-reference-decomposition.v1` и asymmetric roles
`student_candidate` → `paid_reference`. Его canonical typed payload содержит
non-circular bindings `reference_case_id`, `scenario_id` и
`source_entity_fingerprint`; `FrozenReferenceEntry.EntryId` не попадает в paid
answer, потому что он хеширует сам answer и был бы circular binding. Steps
имеют `step_id`, contiguous `order`, `phase_tag` и audit-only `description`;
dependency edges имеют `from_step_id` → `to_step_id`.

Predicate сравнивает candidate/reference only: exact step-id coverage, exact
ordered step sequence, exact DAG dependency edge set, exact phase-tag set и
exact step-to-phase assignment. Dependency edges сортируются как set semantics,
phase tags сравниваются как set, а step order остаётся ordered sequence. Каждый
edge обязан идти topologically forward по canonical step order; cycle guard
выполняется явно до forward-order guard, поэтому двухузловые и длинные циклы
получают стабильный cycle error, а одиночный backward edge без цикла —
стабильный order-invalid error. Unknown fields, unsafe ids, malformed JSON,
duplicate step/order/edge, missing endpoint, self-edge, cycle, backward edge,
swapped role, wrong schema/domain/subject/pattern, wrong binding или arbitrary
high-score/self-score property rejected fail closed. Calibration numbers снова
считает generic `ReferenceMatchingRuleCalibrationEvaluator`; перед метриками
adapter проверяет rule identity и `FrozenReferenceCorpus.SubjectId`.
`OrchestrationAcceptanceGateEvaluator` остаётся отдельным acceptance gate и не
становится authority для этого прибора. Этот interface не означает наличие
production paid corpus, DPO, model call, training run, checkpoint или качества
orchestration модели.

Второй предметный consumer — семейство ProjectWiki code-documentation adapters.
C# consumer
`42.ProjectWikiDomain/ProjectWikiCodeDocumentationReferenceMatchingRule.cs`
использует subject `code-documentation-csharp`, Angular consumer
`42.ProjectWikiDomain/ProjectWikiAngularCodeDocumentationReferenceMatchingRule.cs`
использует subject `code-documentation-angular`, а SQL consumer
`42.ProjectWikiDomain/ProjectWikiSqlCodeDocumentationReferenceMatchingRule.cs`
использует subject `code-documentation-sql`. Каждый строит ProjectWiki-only
identity для domain `project-local-wiki`, schema
`xiigen.project-wiki-code-documentation-reference-evidence.v2` и policy
`policy-exact-set-ordinal`. Predicate парсит typed payload, fail-closed отвергает
malformed/wrong-schema/duplicate/unsafe ids, unsafe subject и cross-subject
payloads, затем сравнивает только exact ordinal set evidence ids. `answer`
хранится в payload для traceability, но не становится текстовой метрикой.
SQL adapter также не выводит предмет из `.sql`, `procedure:` или prose.
Все три ProjectWiki consumer дополнительно проверяют
`FrozenReferenceCorpus.SubjectId` до вызова generic evaluator, потому что общий
calibration input связывает только DomainTag и иначе мог бы смешать
C#/Angular/SQL внутри `project-local-wiki`.

Repository-owned calibration persistence lives in
`14.ContinuousLearning/Training/RepositoryOwnedReferenceMatchingCalibrationManifest.cs`.
The public flow is:
`ReferenceMatchingCalibrationInput.Create(...)` → subject adapter
`EvaluateCalibration(...)` → `RepositoryOwnedTrainingArtifactLayout.CreateReferenceMatchingCalibrationAddress(...)`
→ `RepositoryOwnedReferenceMatchingCalibrationManifestSealer.PersistAndSeal(address, input, result)`
→ `OpenSealed(address)`. The sealer opens the linked frozen corpus through
`RepositoryOwnedReferenceCorpusManifestSealer.OpenSealed` before both persist and
open. The manifest stores only IDs, hashes, counts and rates: positive/negative
calibration pair IDs, separate false-miss and false-hit IDs, corpus hash/count,
rule domain/id/version, human-readable identity description, identity SHA,
calibration-set SHA and evaluator `ResultSha256`. It does not store raw paid answers, raw candidate answers, DPO
sides, weights or model output. Open validates the manifest self-hash, linked
corpus manifest/hash, address segments, denominators, finite rates, false-id
membership and result hash before returning the typed manifest. The evaluator
hash is the membership-bound v2 form: ordered known-positive and known-negative
PairId arrays are included in the canonical input, so substituting calibration
membership while preserving counts/false IDs is rejected. Open also reconstructs
the rule identity from the stored domain/id/version/canonical description and
exact-compares the normalized description plus identity SHA.

For production-style domain code the shorter path is
`RepositoryOwnedReferenceMatchingCalibrationManifestSealer.EvaluatePersistAndSeal(repositoryRoot, subjectSlug, input, matchingRule)`.
It builds the address from the input identities, validates that destination by
strictly reopening the linked sealed corpus, then evaluates the predicate and
calls `PersistAndSeal`. The caller still supplies the subject slug explicitly,
because one broad domain can contain several subjects, but a wrong repo, wrong
subject or missing sealed corpus fails before predicate execution or tree
creation, and the caller can no longer forget to bind the result to the rule
identity and calibration-set hash.

Честная граница: production paid-reference corpora, production calibration
manifests for real paid datasets and remaining persisted ProjectWiki reference
datasets ещё отсутствуют. Translation, DesignConversion, Orchestration и
ProjectWiki adapters — приборы сопоставления с frozen reference, не runtime
teacher, не training path и не DPO chosen/rejected authority.

### IContinuousLearningPipeline
```csharp
Task<OperationResult<IReadOnlyList<ModelAdapter>>> RunCycleAsync(string rootPath)
```
Implemented by: `ContinuousLearningPipeline` (14/Pipeline)
For injection by: 16.AutonomousLearning — must use this interface, not the concrete class

---

## Core Data Types (cross-project)

### AgentInfrastructure types
```csharp
record ConversationMessage(string Role, string Content)
record AgentState(string SessionId, IReadOnlyList<ConversationMessage> History, string CurrentTask, IReadOnlyList<string> TodoItems)
record LlmResponse(string Content, ToolCall? ToolCall)
record ToolDefinition(string Name, string Description, object Schema)
```

### SemanticCompiler29 types
```csharp
record PipelineInput(string Text)
record PipelineOutput(Domain Domain, string[] Predicates, string Stack, double CompressionRatio)
enum Domain { BUG, PLAN, ARCH, REFACTOR, CODE_Q }
```

### AgentAlgorithms types
```csharp
record SubTask(string Id, string Description, string Role, IReadOnlyList<string> DependsOn)
record TextChunk(string Id, string Content, double Score = 0)
enum ContentType { CSharpCode, Documentation, NaturalLanguage, Json, SkillDocument }
```

### ContinuousLearning types
```csharp
record TrainingExample(string Prompt, string Response, string Domain, double Weight, DateTimeOffset CapturedAt)
record DpoPair(string Prompt, string Chosen, string Rejected, string Domain, DateTimeOffset CapturedAt)
record FeedbackEvent(string Prompt, string ModelResponse, string? UserCorrection, FeedbackSignal Signal, string Domain)
record ModelAdapter(string Domain, string AdapterPath, int TrainingExamples, DateTimeOffset TrainedAt)
enum FeedbackSignal { Accepted, Edited, Rejected }
```

---

## §43 — 43.SmartSkillsDomain (Smart Skills Domain — Phase 39/40)

Namespace: `SmartSkillsDomain`. All types declared in `43.SmartSkillsDomain/`.

### XiigenExtensionRequest / XiigenExtensionResponse (XiigenExtensionApiContracts.cs — Phase 40)
```csharp
public sealed record XiigenExtensionRequest(
    string HostName,
    string HostType,
    string Method,
    string UserPromptOrTaskText,
    IReadOnlyDictionary<string, string> HostFacts,
    bool DebugEnabled,
    IReadOnlyList<string> RequestedSkills,
    IReadOnlyList<string> AvailableSkills,
    IReadOnlyDictionary<string, string> CheckSetInputs,
    IReadOnlyDictionary<string, string> Provenance,
    string CorrelationId);

public sealed record XiigenExtensionResponse(
    bool Success,
    string Status,
    string ChatDisplayText,
    IReadOnlyList<string> SelectedSkills,
    IReadOnlyDictionary<string, string> CheckSetResults,
    IReadOnlyList<string> StructuredTaskRefs,
    IReadOnlyList<string> ScenarioRefs,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string> Errors,
    XiigenDebugTrace? DebugTrace);

public sealed record XiigenDebugTrace(
    string Method,
    string InputPayloadText,
    string OutputPayloadText,
    bool ContainsRawPrivateContent = false,
    bool ContainsSecrets = false);

public sealed record XiigenExtensionRequestValidationResult(
    bool IsValid,
    IReadOnlyList<string> Errors);
```

### XiigenExtensionApiContract (XiigenExtensionApiContracts.cs — Phase 40)
Static class providing request validation and display-text helpers.
```csharp
public static class XiigenExtensionApiContract
{
    public static XiigenExtensionRequestValidationResult ValidateRequest(XiigenExtensionRequest request)
    public static string FormatDebugTraceForChat(bool debugEnabled, XiigenDebugTrace? debugTrace)
    public static string BuildChatDisplayText(string chatDisplayText, bool debugEnabled, XiigenDebugTrace? debugTrace)
}
```
`ValidateRequest` requires non-empty: `HostName`, `HostType`, `Method`, `CorrelationId`.
`FormatDebugTraceForChat` returns empty string when `ContainsRawPrivateContent` or `ContainsSecrets` is true.

### Phase40 enums (Phase40ExtensionContracts.cs — Phase 40)
```csharp
public enum Phase40ExtensionHostKind { Unknown = 0, Claude = 1, Codex = 2, Cursor = 3 }

public enum Phase40EvidenceScope
{
    Unknown = 0,
    HostRuntimeUserFacingEntrypoint = 1,
    PackageStaticOrAdapterContractOnly = 2,
    ProductDecisionRequired = 3
}

public enum Phase40CoreDecisionSourceKind
{
    Unknown = 0,
    CoreTrainableSkillSelection = 1,
    CoreCheckSetContract = 2,
    ExactOrStaticEvidenceOnly = 3,
    HostProvidedEvidenceOnly = 4
}
```

### Phase40 contract records (Phase40ExtensionContracts.cs — Phase 40)
Key records for extension API request/response cycle and evidence tracking:
```csharp
record Phase40ExtensionApiRequest(string RequestId, Phase40ExtensionHostKind HostKind, string HostRepositoryRoot,
    string UserFacingEntrypoint, string ConcreteTask, SkillSelectionRequest SkillSelectionRequest,
    Phase40PromptSuiteRequest PromptSuiteRequest, Phase40AssetReferenceSet AssetReferences,
    IReadOnlyList<Phase40TraceLogReference> TraceLogReferences)

record SkillSelectionResponse(string ResponseId, SkillSelectionDecision Decision,
    Phase40CoreDecisionEvidence DecisionEvidence, IReadOnlyList<Phase40CheckSetResult> CheckSetResults,
    Phase40ContractStatus ContractStatus, Phase40ExternalToolUsageEvidence ExternalToolUsageEvidence,
    Phase40AssetReferenceSet AssetReferences, IReadOnlyList<Phase40TraceLogReference> TraceLogReferences)

record Phase40ContractStatus(Phase40EvidenceScope EvidenceScope, bool HostRuntimeUnavailable,
    bool FullPhase40ImplementationProven, bool ContractOnlyDowngradeRequired,
    string DowngradeReason, string NextRequiredAction)

record Phase40CoreDecisionEvidence(Phase40CoreDecisionSourceKind DecisionSourceKind,
    string CoreDecisionReference, string ExactOrStaticEvidenceReference,
    bool DeterministicEvidenceUsedAsLearning, bool HostProvidedSemanticDecision,
    bool MasqueradeProhibited, string ReviewNote)

record Phase40AssetReferenceSet(string CommonModelManifestRef, string CommonTrainingDataManifestRef,
    string CommonModelLocatorRef, string AdaptiveStateRootRef, string ProjectLocalStateRef,
    bool CommonAssetsRepoOwned, bool AdaptiveStateSeparatedFromHostPackage)
```

### SmartSkillsSemanticUnitDefinition / SmartSkillsSemanticUnitCatalog (SmartSkillsSemanticUnitCatalog.cs — Phase 39)
```csharp
public sealed record SmartSkillsSemanticUnitDefinition(
    string DomainId,
    string MachineName,
    string TrainingAdapterName,
    string DpoRowShapeName,
    string CheckpointSectionName,
    string HeldOutFixtureId,
    bool Trainable,
    bool DeterministicFinalAuthority,
    bool RuntimeSemanticQualityClaimed);
```
`SmartSkillsSemanticUnitCatalog.ExpectedDomainIds` (11 units, all `Trainable=true`, `DeterministicFinalAuthority=false`, `RuntimeSemanticQualityClaimed=false`):
- `prompt_understanding` → `PromptUnderstandingMachine`
- `task_understanding` → `TaskUnderstandingMachine`
- `task_type` → `TaskTypeMachine`
- `routing` → `RoutingMachine`
- `skill_candidates` → `SkillCandidateMachine`
- `skill_selection` → `SkillSelectionMachine` (training adapter: `ISkillSelectionTraining;IMachineTraining<SkillSelectionDomain>`)
- `prompt_context` → `PromptContextMachine`
- `result_gap` → `ResultGapMachine`
- `output_validation` → `OutputValidationMachine`
- `task_completion` → `TaskCompletionMachine`
- `intermediate_checks` → `IntermediateCheckMachine`

### SkillSelectionMachine (SkillSelectionContracts.cs + SkillSelectionMachine.cs — Phase 39)
Implements both `ILearningMachine<SkillSelectionDomain>` and `IMachineTraining<SkillSelectionDomain>`.
```csharp
public sealed class SkillSelectionMachine : ILearningMachine<SkillSelectionDomain>, IMachineTraining<SkillSelectionDomain>
{
    IMachineTraining<SkillSelectionDomain> TrainingAdapter { get; }
    Task<OperationResult<SkillSelectionTrainingResult>> TrainMachineBatchAsync(IReadOnlyList<SkillSelectionMachinePair> pairs, CancellationToken ct = default)
    OperationResult<SkillSelectionDecision> DecideFromTrainedState(SkillSelectionRequest request)
    SkillSelectionCheckpoint ExportCheckpoint()
    void LoadCheckpoint(SkillSelectionCheckpoint checkpoint)
    static string NormalizePromptKey(string prompt)
    // + full ILearningMachine<SkillSelectionDomain> implementation
}
```
Key contract records:
```csharp
record SkillSelectionRequest(string Prompt, string ProjectRoot, IReadOnlyList<string> AvailableSkillIds,
    IReadOnlyDictionary<string, string> HostFacts, string UserPromptSourceEntryId)

record SkillSelectionDecision(IReadOnlyList<string> SelectedSkillIds, IReadOnlyList<string> RejectedSkillIds,
    IReadOnlyList<string> EvidenceFeatures, double Confidence, bool RequiresSecondPass, string DecisionText)

record SkillSelectionMachinePair(string PairId, SkillSelectionRequest Request,
    SkillSelectionDecision Chosen, SkillSelectionDecision Rejected, string RejectedType,
    string SourcePromptEntryId, string TargetTrainingAdapter)

record SkillSelectionTrainingResult(string BatchId, int PairCount, bool ContractOnly, string GateNote)

record SkillSelectionCheckpoint(int SchemaVersion, IReadOnlyDictionary<int, int> CandidatePositionWeights,
    IReadOnlyList<string> TrainedPairIds, IReadOnlyList<string> TrainedPromptKeys,
    string RouteMode, DateTimeOffset SavedAtUtc)
```

### ServerSkillLibraryRegistry (SkillSelectionContracts.cs — Phase 39)
```csharp
public sealed class ServerSkillLibraryRegistry
{
    public IReadOnlyList<ServerSkillLibraryEntry> LoadUniversalSkills()
}
```
Returns 3 universal skills: `planning-session-startup`, `plan-review`, `human-language-gate`.
```csharp
record ServerSkillLibraryEntry(string SkillId, string Version, string Summary, double DefaultWeight, string PortabilityLabel = "universal")
record ProjectLocalSkillState(IReadOnlyDictionary<string, string> WikiNotes, IReadOnlyDictionary<string, string> Preferences,
    IReadOnlyList<string> Feedback, IReadOnlyDictionary<string, double> CustomSkillWeights,
    IReadOnlyList<ServerSkillLibraryEntry> ServerLibraryEntries)
```

### SkillLibraryUpgradePlanner (SkillSelectionContracts.cs — Phase 39)
```csharp
public sealed class SkillLibraryUpgradePlanner
{
    public SkillLibraryUpgradeResult PlanUpgrade(SkillLibraryUpgradeRequest request)
}
```
Builds preference-patch proposals when old universal skill IDs are not present in the new library. Sets `SemanticQualityClaimed=false`, `EvidenceMode="schema_upgrade_boundary_not_semantic_authority"`.

### OrchestrationReasoningMachine (OrchestrationReasoningMachine.cs — Phase 40)
Skeleton machine. NOT trained — no orchestration inference implemented.
```csharp
public sealed class OrchestrationReasoningMachine
{
    public const string NotTrainedStatus = "not_trained_no_learned_guide_state";
    public const string LoadedGuideStateReadyStatus = "loaded_guide_state_ready_no_inference_implemented";
    public const string NoInferenceScenarioId = "not-ready-no-orchestration-inference";

    public OrchestrationReasoningMachine()
    public OrchestrationReasoningMachine(IOrchestrationReasoningLearnedGuideStateLoader learnedGuideStateLoader)
    public OperationResult<OrchestrationReasoningResponse> Plan(OrchestrationReasoningRequest request)
}

public interface IOrchestrationReasoningLearnedGuideStateLoader
{
    OperationResult<OrchestrationReasoningLearnedGuideStateDescriptor> Load();
}

public sealed record OrchestrationReasoningLearnedGuideStateDescriptor(
    string StateId, string GuideStateRef, string ManifestId, string ManifestSha256,
    string MachineType, string SchemaVersion, string SourceBaseCommit, string StateSha256);
```
When no learned guide state is loaded: `ContractStatus = NotTrainedStatus`.
When guide state descriptor is loaded but inference not yet implemented: `ContractStatus = LoadedGuideStateReadyStatus`.

`OrchestrationScenarioPlan.HostCheckSets` is a list of local host labels that the
plan wants reviewers to check, not a place to name the teacher model. Good:
`["claude", "cursor", "codex"]` means the same scenario should be checked against
those host surfaces. Bad: `["anthropic/claude-3-5-sonnet"]`, `["gpt-5"]`, a path,
URI, digest, or padded value, because that is a model/source reference disguised
as a host label. Source refs and scenario refs still go through the normal
forbidden-model identity gate.

### ExtensionEvalOnlyQualityRunner (ExtensionEvalOnlyQualityRunner.cs - QUALITY-02)
Eval-only helper runner for QUALITY-01/QUALITY-02 score evidence. It validates
approved local/private eval-only rows, scores only `USER_ADAPTIVE_EVAL_ONLY`
rows through `ExtensionQualityScoring.Score`, excludes `EVIDENCE_ONLY` and
`COMMON_BLOCKED` rows from scoring, and writes JSON, JSONL, and Markdown
reports. This deterministic rubric runner is labelled eval-only/helper/internal
baseline evidence only; it is not semantic, trainable, runtime, token-savings,
learnability, target-promotion, locator-update, common-readiness, or final
readiness authority. Scored rows must declare `prompt_sha256`,
`mode=baseline_vs_adaptive_pair`, and
`evidence_scope=local_private_eval_only` before scoring. Generated reports carry
first-class `synthetic_fixture_only` and `actual_approved_eval_executed`
boundary fields so synthetic fixture evidence cannot be confused with an
approved eval execution.
```csharp
public sealed class ExtensionEvalOnlyQualityRunner
{
    public OperationResult<ExtensionEvalOnlyQualityRunnerReport> Run(ExtensionEvalOnlyQualityRunnerOptions options)
    public OperationResult<IReadOnlyList<ExtensionEvalOnlyQualityRunnerInputRow>> ReadInputRows(string inputJsonlPath)
    public OperationResult<ExtensionEvalOnlyQualityRunnerValidation> ValidateRows(
        IReadOnlyList<ExtensionEvalOnlyQualityRunnerInputRow> rows)
    public OperationResult<IReadOnlyList<string>> WriteReportFiles(
        ExtensionEvalOnlyQualityRunnerOptions options,
        ExtensionEvalOnlyQualityRunnerReport report)
}
```

### PromptToTaskMachine (PromptToTaskMachine.cs — Phase 40)
Skeleton machine. NOT trained — no semantic task inference implemented.
```csharp
public sealed class PromptToTaskMachine
{
    public const string NotTrainedStatus = "not_trained_no_loaded_state";
    public const string LoadedStateReadyStatus = "loaded_state_ready_no_inference_implemented";
    public const string NotReadyTaskType = "not_ready_no_semantic_task";

    public PromptToTaskMachine()
    public PromptToTaskMachine(IPromptToTaskLearnedStateLoader learnedStateLoader)
    public OperationResult<PromptToTaskResponse> Translate(PromptToTaskRequest request)
}

public interface IPromptToTaskLearnedStateLoader
{
    OperationResult<PromptToTaskLearnedStateDescriptor> Load();
}

public sealed record PromptToTaskLearnedStateDescriptor(
    string StateId, string ManifestId, string ManifestSha256, string MachineType,
    string SchemaVersion, string SourceBaseCommit, string StateSha256);
```

---

## Quick Lookup: "Which interface do I need?"

| I want to… | Interface | Assembly |
|---|---|---|
| Train a domain learning machine | `ILearningMachine<TDomain>` | AgentInfrastructure.Logical (00.Core) |
| Pair personal + global machines | `ILearningMachinePair<TP, TG>` | AgentInfrastructure.Logical (00.Core) |
| Run the orchestrator | `ILogicalOrchestrator` | AgentInfrastructure.Logical (00.Core) |
| Implement a logical flow | `ILogicalFlow<TInput>` | AgentInfrastructure.Logical (00.Core) |
| Implement a single executable cell | `ILogicalUnit<TIn, TOut>` | AgentInfrastructure.Logical (00.Core) |
| Save/load agent session | `ISessionStorage` | AgentInfrastructure |
| Read files from disk | `ICodeReader` | AgentInfrastructure |
| Write files to disk | `ICodeWriter` | AgentInfrastructure |
| Call the LLM | `ILlmClient` | AgentInfrastructure |
| Decompose a task | `ITaskDecomposer` | AgentAlgorithms |
| Sort tasks by dependency | `DependencyGraph` (class) | AgentAlgorithms |
| Launch sub-agents | `IAgentMessageBus` | AgentAlgorithms |
| Filter skill list | `ISkillFilter` | AgentAlgorithms |
| Classify text → domain | `ISemanticCompiler29` | SemanticCompiler29 |
| Detect programming language | `IStackDetector29` | SemanticCompiler29 |
| Run full orchestration | `OrchestratorFacade` | OrchestratorModel |
| Run project-wiki plugin prompt | `ProjectWikiOrchestrationFlow` | OrchestratorModel |
| Quality-check output | `QualityOrchestrator` | OrchestratorModel |
| Run project-local documentation/wiki flow | `ProjectDocumentationPipelineFlowAdapter` | ProjectWikiDomain |
| Answer project-local wiki questions | `ProjectLocalWikiOrchestrator` | ProjectWikiDomain |
| Build wiki intent from user prompt | `IIntentBuilderUnit` | ProjectWikiDomain |
| Scan project file system | `IProjectScannerUnit` | ProjectWikiDomain |
| Plan wiki evidence requirements | `IEvidencePlannerUnit` | ProjectWikiDomain |
| Select orchestration units | `IUnitSelectorUnit` | ProjectWikiDomain |
| Compose human answer route | `IRouteComposerUnit` | ProjectWikiDomain |
| Verify orchestration result | `IOrchestrationVerifierUnit` | ProjectWikiDomain |
| Index codebase for training | `IProjectIndexer` | ContinuousLearning |
| Route user feedback | `IFeedbackRouter` | ContinuousLearning |
| Run LoRA adaptation cycle | `IContinuousLearningPipeline` | ContinuousLearning |
| Generate orchestration plan (trained only after passed eval gate) | `OrchestrationPlanMachine` | ProjectWikiDomain |
| Learn flow grounding from DPO pairs | `PromptFlowGroundingModel` | ProjectWikiDomain |
| Select skills for a prompt (typed baseline/contract; semantic quality unclaimed) | `SkillSelectionMachine` / `IMachineTraining<TDomain>` | SmartSkillsDomain |
| Load universal skill library | `ServerSkillLibraryRegistry` | SmartSkillsDomain |
| Plan skill library upgrade | `SkillLibraryUpgradePlanner` | SmartSkillsDomain |
| Validate xiigen extension request | `XiigenExtensionApiContract.ValidateRequest` | SmartSkillsDomain |
| Run QUALITY-02 eval-only score rows | `ExtensionEvalOnlyQualityRunner` | SmartSkillsDomain |
| Orchestration reasoning (skeleton) | `OrchestrationReasoningMachine` | SmartSkillsDomain |
| Prompt-to-task translation (skeleton) | `PromptToTaskMachine` | SmartSkillsDomain |

## Translation DPO reproducibility interfaces

| I want to… | Interface / type | Assembly |
|---|---|---|
| Carry server-owned DPO seed plus LoRA topology | `DpoTrainingReproducibilityContext` | DPO |
| Describe the authority-bound seeded LoRA shape | `DpoTrainingSeededLoraTopology` | DPO |
| Apply one atomic seeded topology to policy, reference and fresh-load | `DpoTrainingSeededLoraTopologyPreparer.ApplyFreshSeededTriple` | DPO |
| Validate continuation fresh-load/reference provenance before checkpoint load | `DpoTrainingSeededLoraTopologyPreparer.ValidateSeededRole` | DPO |
| Expose DesignConversion’s canonical LoRA shape without leaking constants into translation | `DesignConversionDpoPolicySeededLoraTopologyBridge.CreateCanonicalSeededLoraTopology` | DesignConversionDomain |
| Run initial Translation DPO with all three server-owned model roles | `TranslationDpoTrainEvaluateRunner.Run(... policy, reference, freshLoad, broker, options)` | TranslationDomain |

The two-model Translation DPO runner entry point is legacy and fails closed. The live initial-training path requires three model factories so the fresh-load model is seeded before any future continuation can load a policy checkpoint into it.

## 12.SemanticCompiler — отрицание сохраняется на учебном пути без цены для сжатия (3 ноября 2026)

**Что было сломано.** Слова «not» и «no» стоят в общем наборе служебных слов, поэтому разбивка
первого уровня их удаляет, и обучаемая единица, которая учится по словам имени, физически не
может отличить `IsNotEmpty` от `IsEmpty`: на входе у неё остаются одни и те же слова. Опыт
измерил это прямо: сохранность отрицаний 0 из 2 без правки и 2 из 2 с правкой.

**Почему нельзя было просто убрать два слова из набора.** Эта форма тоже измерена и дорога:
запутанность продуктового оценщика 3,8458932055013695 → 4,779225984271398, то есть **хуже на
24,27 %**, словарь 6 → 8 слов. Правило допуска переносов запрещает ухудшение больше 5 % на
полезной метрике, и процессорное время тут ни при чём. Набор — один общий статический, его
потребители: нормализатор, собиратель семейств и сама разбивка, поэтому «узкая» правка набора
ничем не отличалась бы от общей.

**Что стало.**

```csharp
public static readonly IReadOnlySet<string> PolarityKeepWords;              // ровно {"not","no"}
public static bool IsStopword(string w, IReadOnlySet<string>? keepWords = null);
public static List<string> Level1Tokens(string csharpSource, IReadOnlySet<string>? keepWords = null);
public static List<string> Level2Tokens(string csharpSource, IReadOnlySet<string>? keepWords = null);
public static IReadOnlyList<string> AstTokenizer.TokenizeForPolaritySensitiveTraining(string csharpSource);
```

Общий набор служебных слов **не тронут ни на одно слово**. Исключения — необязательный последний
довод со значением «пусто», поэтому без довода поведение побайтово прежнее и все действующие
вызывающие работают без правок. Исключения передаёт **только** учебный вход, чувствительный к
полярности; нормализатор, собиратель семейств и путь сжатия, питающий оценщик запутанности,
довода не передают. Поэтому измеренные 24,27 % здесь не возникают **по построению**, а не по
обещанию: это проверяется первым случаем приёмки — при пустом списке исключений выход совпадает с
прежним слово в слово.

**Чего это НЕ делает.** Ни одна единица не называется обученной, и ни один действующий
вызывающий не переключён на новый вход: переключение общего пути изменило бы уже записанные
потоки других опытов. Подключение конкретной обучаемой единицы к учебному входу — отдельный
кусок со своим измерением до и после.

**Проверки.** `Tests/ExperimentFixes/StopwordPolarityExceptionTests.cs`: нулевая цена по
построению на обоих уровнях и на обычном исходнике, учебный путь сохраняет отрицание, путь сжатия
и нормализатор его по-прежнему удаляют, прочие служебные слова ведут себя как прежде на обоих
путях, набор исключений назван ровно двумя словами, краевой пустой исходник. Прежнее правило
воспроизведено функцией от входа, зависимость от входа показана вторым входом. Прогона не было:
прямые команды сборки и прогона тестов запрещены политикой разрешений, зелёными они не
называются.

## 35.ExperimentsV2 — договор базового типа узнаётся по личности, а не по подстроке (3 ноября 2026)

**Что было сломано, по двум сторонам сразу.** Различитель договора узнавал базовый тип по
БУКВАЛЬНОЙ подстроке: требуемое имя стояло постоянной строкой в коде и сравнивалось подстрокой с
текстом договора, а фактическое — подстрокой с вольными текстовыми полями пакета
(`35.ExperimentsV2/Experiments/P3LearningFromFailureProof.cs`, прежние строки 259 и 449–458). У
подстроки две тихие беды. Составное имя: класс от `BaseServiceAdapter` содержит подстроку
`BaseService`, и ответ «договор соблюдён» выдаётся уверенно и неправильно. Переименование: если
договор требует `ProjectService`, постоянная `BaseService` не совпадёт никогда, и ответ «не
соблюдён» выдаётся тоже уверенно и неправильно — оба раза молча, без громкой ошибки.

**Что стало.** `35.ExperimentsV2/Experiments/BaseContractIdentityDiscriminator.cs`:

```csharp
public static BaseContractVerdict Decide(
    string? requiredBaseTypeIdentity,
    IReadOnlyList<string>? observedBaseTypeIdentities,
    LearnedCoverageSignal? learnedSignal = null);
```

Требуемое имя приходит из договора времени работы, фактическое — из списка объявленных базовых
типов, и сравниваются ЛИЧНОСТИ: точное совпадение имени. Приведение личности названо: краевые
пробелы и уточнение пространства имён личность не меняют, уточнение общими параметрами тоже, а
РЕГИСТР меняет — имена типов в этом языке различаются регистром. Решения: `BASE_CONTRACT_SATISFIED`,
`BASE_CONTRACT_MISSING`, `BASE_CONTRACT_UNDECIDABLE_BY_IDENTITY` с кодами причины
`BASE_CONTRACT_REQUIREMENT_NOT_DECLARED` и `BASE_CONTRACT_BASE_TYPES_NOT_OBSERVED`.

**Обученная память — сигнал, а не власть.** `LearnedCoverageSignal` (ответ памяти, покрытие,
уверенность) входит в вердикт как наблюдение и НИКОГДА не перебивает сравнение личностей; когда
личности сравнить нельзя, сигнал тоже не становится ответом — ответом становится честное «решить
нельзя». Причина требования измерена опытом: плечо, обученное на неверной метке, воспроизвело
неверный ответ с уверенностью 1,0000, значит уверенность порогом доверия быть не может.

**Названная граница записанных чисел.** У пакетов исторического опыта списка объявленных базовых
типов нет: поле `DeclaredBaseTypes` добавлено необязательным последним доводом (все нынешние
построения пакетов работают без правок) и в сохранённых данных пусто. Для таких пакетов ответ
остаётся прежним текстовым сигналом и помечен как «по личностям решить нельзя» — ровно затем,
чтобы измеренные и записанные числа прошлого прогона не переписывались этой правкой. Там, где
список объявлен, решают личности, и подстрока их перебить не может.

**Проверки.** `Tests/ExperimentFixes/BaseContractIdentityDiscriminatorTests.cs`: составное имя
базового типа, переименованный договор, память с неверным ответом при уверенности 1,0000 и её
обратная половина, честный отказ при отсутствии наблюдения и при неназванном требовании,
приведение личности (пространство имён и общие параметры — да, регистр — нет), положительный
контроль обычного совпадения. Прежние правила воспроизведены функциями от входа, зависимость от
входа показана вторым входом. Прогона не было: прямые команды сборки и прогона тестов запрещены
политикой разрешений, зелёными они не называются.

## 27.TranslationDomain — номер версии состояния читается длинным целым (3 ноября 2026, починка сборки)

**Что было сломано.** Счётчик публикаций версии состояния объявлен **длинным** целым
(`TranslationMachine.cs`, запись опубликованной версии), а три места читали его как обычное
целое: открытое свойство номера версии, поле основы в записи проигравшего кандидата и поле
победившей версии. В C# длинное в обычное неявно не преобразуется — **область перевода не
собиралась**. Объявление длинным пришло коммитом `a28bc355146` 2 ноября; третью ошибку добавил
кусок про запись кандидата 3 ноября.

**Почему не приведение, а расширение.** Приведение к обычному целому молча усекало бы номер
после предела обычного целого: две разные публикации получили бы один номер, и условная
публикация по номеру версии перестала бы их различать. Молчаливое усечение здесь хуже отказа
сборки. Свойство не может честно отказать — отказ из свойства это исключение, а правило проекта
запрещает исключения для обычной логики. Поэтому читатели расширены до типа самой величины:
номер публикации — монотонная величина, и длинное целое для неё канон, уже выбранный в записи
версии.

**Что изменилось по типам:** `CurrentStateVersion` → `long`; необязательные доводы
`expectedStateVersion` у обучения структурной партии и у измерения на названной версии →
`long?`; поля записи кандидата `BaseStateVersionNumber` → `long` и `WinningStateVersionNumber` →
`long?`, те же типы в проверяемом указателе на запись.

**Кого затронула смена типа.** Продуктовых вызывающих у `CurrentStateVersion` нет вовсе —
только проверки; все их использования либо выводят тип сами, либо сравнивают длинное с длинным.
Уже записанные файлы записей кандидата читаются как прежде: единственное правило текста не имеет
числовых преобразователей, номер пишется и читается обычным числом, и прежние небольшие значения
разбираются в длинное без потери.

**Проверки.** `Tests/ExperimentFixes/StateVersionWideningTests.cs`: типы закреплены напрямую
(сужение обратно немедленно упадёт), номер выше предела обычного целого проходит настоящий
продуктовый путь записи текста без потери, уже записанное небольшое значение читается тем же
числом и остаётся в тексте обычным числом без кавычек.

**Честная граница.** Компилятором это **не проверено**: сборка в среде исполнителя отказывает по
разрешениям. Доказательство — разбор типов по каждому месту плюс написанные проверки. Зелёной
сборкой это не называется.

## 11.DPO — сверка владельца на границе изменения живых весов (3 ноября 2026)

**Что было сломано.** На границе изменения весов не было объявленного владельца вовсе. Кто
получатель — решал тот, кто передал модель: попросив обучить один проект и передав модель
другого, вызывающий получал обновление весов другого проекта без единого возражения. Опыт сто
двадцать один измерил это прямо: правильный адрес получателя 0 из 1 до правки и 1 из 1 после;
отпечаток весов в обоих мирах одинаковый, расхождение продуктового измерителя ровно ноль —
отличался ТОЛЬКО владелец, и поймал это проверяющий по отпечаткам, а не продукт.

**Что стало.** Владельца объявляет та сторона, которая ВЫДАЁТ возможность изменения
(`DpoMutationAdmissionCapability.Issue(issuer, declaredOwnerId)` — довод необязательный и
последний, поэтому все нынешние выдачи работают без правок). Отпечаток фактически изменяемого
набора весов считает сам обучатель из привязок получателя и личности замороженного эталона
(`11.DPO/DpoTrainer.cs`, `MutatedWeightsIdentity`). Учёт пар «владелец ↔ набор весов» живёт
внутри выданной возможности (`11.DPO/DpoMutationOwnerLedger.cs`), то есть принадлежит выдавшей
стороне, а не вызывающему. Отказ выражается тем же исключением допуска, что и прежде, с новыми
названными кодами: `DPO_TRAINER_DECLARED_OWNER_OVER_FOREIGN_WEIGHTS` (тот же владелец над другим
набором весов), `DPO_TRAINER_WEIGHTS_CLAIMED_BY_ANOTHER_OWNER` (тот же набор весов под другим
владельцем), `DPO_TRAINER_EMPTY_WEIGHTS_IDENTITY` (пустой отпечаток не проходит молча).

**На чём держится личность и кто её может подделать.** Личность держится на тождестве ССЫЛОК
фактических объектов величин: привязки получателя отобраны по ссылкам на параметры установленных
накладок, личность эталона посчитана получателем один раз при захвате. Вызывающий не может
ЗАЯВИТЬ отпечаток о себе — он либо передал те же объекты, либо другие. Чего эта сверка **не**
закрывает: внутренний доступ к сборке обучения выдаётся по ИМЕНИ сборки, поэтому под разрешённым
именем можно выдать себе возможность и объявить любого владельца. Сверка ловит противоречие
объявлений над одним набором весов; она не делает имя сборки проверяемой личностью, и
переименованием та дыра не чинится.

**Решение владельца проекта, которое здесь не выбрано.** Внешний контур закрывается двумя
взаимоисключающими путями, и оба продуктовые: заменить выдачу внутреннего доступа по имени
сборки проверяемой личностью (сильное имя или подпись), либо убрать выдачу внутреннего доступа и
открыть узкий публичный способ получить возможность для стенда. Первый путь ломает интеграцию
для всех, кто собирает проект без подписи; второй расширяет публичную поверхность там, где её
сегодня нет намеренно. Выбор остаётся за владельцем проекта и описан в записи 117 единого
реестра переносов.

**Проверки.** `Tests/ExperimentFixes/DpoMutationOwnerIdentityTests.cs`: действующий обход
воспроизведён прямо (сборка проверок, не являющаяся ни одной из трёх продуктовых частей, сама
выдаёт себе возможность и получает разрешение — только потому, что её имя внесено в список);
два владельца над одним набором весов; один владелец над двумя наборами; честный повтор;
пустой отпечаток; граница возможности отказывает тем же исключением допуска и сохраняет прежний
путь без объявленного владельца. Прежнее правило границы было САМО константой — она принимала
любую пару, — и это сказано прямо: входа ему не придумывается. Прогона не было: прямые команды
сборки и прогона тестов запрещены политикой разрешений, зелёными они не называются.

## 27.TranslationDomain — сверка настоящей памяти с независимым журналом (3 ноября 2026, шестой раздел ремонта)

**Что было неправильно.** Приёмка состояния памяти ближайших соседей довольствовалась числом
записей («девять») и свёрткой ожидаемого журнала. Ни то, ни другое о памяти ничего не говорит.
Число девять совпадает у множества разных состояний: испортить один перевод, переставить две
записи внутри ячейки, переименовать ключ, перенести запись из одной ячейки в другую — число
останется девять. А свёртка ожидаемого журнала, сравниваемая сама с собой, равна себе **всегда**:
такая проверка не может упасть ни на одном состоянии. Если журнал ожидаемого строится тем же
кодом, что и проверяемое состояние, их согласие — это согласие кода с самим собой.

**Что стало.** Появился `27.TranslationDomain/TranslationKnnStateAudit.cs`:

```csharp
public sealed record TranslationKnnExpectedWrite(
    string Signature, string Source, string Target);

public static OperationResult<bool> RequireMemoryMatchesJournal(
    TranslationKnnStoreCheckpoint actual,
    IReadOnlyList<TranslationKnnExpectedWrite> expectedJournal);
```

Журнал ожидаемого приходит как **данные** того, кто учил. Сверка не вызывает ни хранилище
памяти, ни разборщик текста, ни построитель ключей — иначе она проверяла бы код его же кодом.
Отдельный тип ожидаемой записи выбран намеренно: журнал нельзя случайно собрать из проверяемой
контрольной точки, потому что у неё свой тип записи и подстановка не скомпилируется.
Сравнивается всё, что назвал раздел плана: состав ячеек, источник и перевод каждой записи,
кратности и порядок внутри ячейки. Порядок значим — запись, легшая позже, несёт более сильный
учебный сигнал (`27.TranslationDomain/TranslationUnitContracts.cs`, `AddPair`).

**Названные отказы.** `KNN_AUDIT_MISSING_SIGNATURE` (нет объявленной ячейки),
`KNN_AUDIT_UNEXPECTED_SIGNATURE` (есть необъявленная), `KNN_AUDIT_MULTIPLICITY_MISMATCH`
(перенос записи между ячейками при том же общем числе), `KNN_AUDIT_SOURCE_MISMATCH`,
`KNN_AUDIT_TARGET_MISMATCH`, `KNN_AUDIT_ORDER_MISMATCH` (та же пара есть, но на другом месте),
`KNN_AUDIT_EMPTY_JOURNAL` (пустой журнал не является доказательством совпадения),
`KNN_AUDIT_ENTRY_COUNT_MISMATCH` — последний проверяется **последним** и только как
дополнительный признак: именно на равенстве числа держался прежний дефект.

**Названные ненаблюдённые границы, которые эта сверка НЕ закрывает.** Первое: фактическое
состояние наблюдается через продуктовую выгрузку, а не чтением живых структур в памяти процесса,
поэтому сверка видит картину выгрузки; потерю записей самой выгрузкой она заметить не может.
Второе: сверка доказывает, что обучение **записало** ожидаемое, и не доказывает, что чтение
этим пользуется — какой читатель ответил, проверяется отдельно своим следом. Третье: правило
построения ключа разборщиком не перепроверяется, ключи приходят объявленными данными. Четвёртое:
у контрольной точки памяти нет пространства имён направления перевода — это известный отдельный
пункт плана, и он здесь намеренно не расширяется.

**Проверки.** `Tests/ExperimentFixes/TranslationKnnStateAuditTests.cs`: испорченный один перевод
при том же числе записей, свёртка журнала сама с собой на памяти без единой общей записи,
подменённый ключ ячейки, перестановка внутри ячейки, перенос записи между ячейками, подменённый
источник при том же переводе, пустой журнал и живой случай. Прежнее правило воспроизведено двумя
функциями **от входа**; зависимость от входа показана вторым входом. В живом случае урок
проходит обычным продуктовым путём обучения, состояние снимается продуктовой выгрузкой,
независимо проверяется инвариант «в памяти только источник и перевод этого урока», а журнал,
снятый с чистой выгрузки, используется **только** как проба чувствительности к порче и прямо так
назван: доказательством совпадения он быть не может, потому что снят с самого проверяемого
состояния. Прогона не было: прямые команды сборки и прогона тестов запрещены политикой
разрешений, зелёными они не называются.

## 35.ExperimentsV2 — происхождение концов учебной пары (3 ноября 2026, пятый раздел ремонта)

**Что было неправильно.** Нарезка учебных пар в буфере повторов складывала предикаты ВСЕХ
файлов в один список и брала соседние элементы (`35.ExperimentsV2/Experiments/E43ReplayBuffer.cs`,
прежние строки 78–87). На каждой границе файлов это давало пару-шов: последний предикат одного
файла становился «предыдущим» для первого предиката следующего. Такой пары в материале нет
вовсе — её изобрела склейка, а обучение считало её законным переходом. У пары при этом не было
ни проекта, ни файла, ни позиции, поэтому совпадение числа пар и совпадение значений выглядело
доказательством одинакового материала, хотя материал мог быть из другого файла или из другого
проекта.

**Что стало.** Появился `35.ExperimentsV2/Experiments/E43PairProvenance.cs`:

```csharp
public sealed record E43TrustedStream(
    string ProjectId, string FilePath, IReadOnlyList<int> TokenIds, string ContentHash);

public sealed record E43TrustedPair(
    string ProjectId, string FilePath, int Position, int Current, int Next);

public static OperationResult<E43TrustedStream> BuildStream(
    string projectId, string filePath, IReadOnlyList<int> tokenIds);

public static OperationResult<IReadOnlyList<E43TrustedStream>> RequireTrustedStreams(
    IReadOnlyList<E43TrustedStream> streams);

public static OperationResult<IReadOnlyList<E43TrustedPair>> ExtractPairsWithinStreams(
    IReadOnlyList<E43TrustedStream> streams);

public static OperationResult<E43TrustedPair> RequirePairBelongsToStream(
    IReadOnlyList<E43TrustedStream> streams, E43TrustedPair pair);
```

Поток объявляется вместе с ключом (проект и файл) и отпечатком, пересчитанным из фактических
опознаний предикатов. Пары берутся ТОЛЬКО внутри одного потока, поэтому шва на границе файлов не
возникает по построению. Каждая пара несёт ключ, позицию и оба значения, и проверка сверяет всё
это с фактическим потоком, включая то, что второй конец — действительно следующий элемент.

**Названные отказы вместо исключений.** `E43_DUPLICATE_STREAM_KEY` (два потока назвали один
ключ), `E43_MALFORMED_STREAM_HASH` (отпечаток не шестнадцатеричный), `E43_SHORT_STREAM_HASH`
(обрезок отпечатка), `E43_STREAM_HASH_MISMATCH` (объявленный отпечаток не совпал с
пересчитанным), `E43_EMPTY_STREAM_KEY` (пустой ключ), `E43_UNKNOWN_STREAM` (неизвестный
проект/файл), `E43_WRONG_POSITION` (позиция вне потока или второй конец не следующий),
`E43_VALUE_MISMATCH` (значение конца не то, что в потоке). Для этого у конверта результата
сборки опытов (`35.ExperimentsV2/Core/ResultTypes.cs`) появилось поле кода отказа и перегрузка
отказа с кодом — добавлены, а не изменены: прежняя перегрузка и все вызывающие работают как
раньше.

**Правило отпечатка — одно на два места, и это названо прямо.** Владелец правила кодирования
полей объявлен в `18.CapabilityTraining/UnambiguousFieldEncoding.cs`, но он внутренний, а ссылки
на ту сборку у сборки опытов нет, поэтому в опытах живёт вторая реализация того же правила.
Согласие двух реализаций закреплено проверкой, которая считает один и тот же вход обоими
способами и требует равенства отпечатков и равенства версии правила.

**Вход буфера повторов.** `Run(files, tier, projectId)` — проект добавлен необязательным
последним доводом со значением `experiments-v2-local`, поэтому прежние вызывающие работают без
правок; подставленное значение видно в отчёте опыта рядом с числом убранных швов
(`seamsRemoved`).

**Чего это НЕ делает.** Измерительная часть опыта не менялась: формула удержания, доли
смешивания и порядок обучения те же. Новое обучение этим ремонтом не назначается. Экспериментные
владельцы того же раздела (`P11601/src/BoundaryChecker.cs`, `E43PairExtractionCopy.cs`,
`Program.cs`) лежат под узлами опытов и этим куском не трогались — там работает линия опытов.
Поэтому причинный контраст «3 шва → 0» на сохранённых данных опыта здесь не перемеряется:
правка доказывает отсутствие швов по построению и по проверкам, а не новым прогоном.

**Проверки.** `Tests/ExperimentFixes/E43PairProvenanceTests.cs`: законная пара внутри файла и
изобретённый прежним правилом шов, отказ пары через границу файлов, та же метка файла в другом
проекте, подмена только проекта при прежних токенах, замена содержимого при том же числе
токенов, двойной ключ, короткий и испорченный отпечаток, отпечаток не от этого материала, пустой
ключ, а также отдельный случай про то, что вырожденные проверяющие «только по числу» и «только по
значениям» полный набор не проходят. Прежнее правило воспроизведено функцией от входа, и
зависимость от входа показана вторым входом. Прогона не было: прямые команды сборки и прогона
тестов запрещены политикой разрешений, зелёными они не называются.

## 44.DesignConversionDomain — геометрия сцены доходит до зрительного критика (3 ноября 2026, перенос по опыту сто двадцать три)

**Что было сломано ровно.** Единица критика разметки выдавала вердикт о сцене, в договоре входа
которой геометрии не было вовсе: `BuildVerdictFromInputEvidence` складывала ответ из
`verdicts.Min(Score)` и `verdicts.All(Passed)` по итогам проверки качества и сравнительным
артефактам. Опыт измерил следствие прямо: на подменённом входе критик отвечал одно и то же на
всех четырёх сценах — ровно порог вырождения, — а на фактических относительных прямоугольниках
различал их. Отсутствовал мост между сценой и критиком, а не разум критика.

**Новая запись геометрии.** `DesignConversionRelativeGeometry(ElementId, RelativeLeft,
RelativeTop, RelativeWidth, RelativeHeight)` в `44.DesignConversionDomain/DesignConversionUnitContracts.cs`.
Доли, а не точки: один и тот же разбор годится для разных размеров полотна, и сравнение не
ломается от масштаба.

**Договор входа критика.** `DesignConversionCriticUnitInput` получил **последний необязательный**
довод `IReadOnlyList<DesignConversionRelativeGeometry>? SceneGeometry = null`. Члены договора не
расширялись по смыслу: `ILogicalUnit` и `IDesignConversionCriticUnit` не менялись ни на одну
подпись, а единственное нынешнее построение входа (`DesignConversionRoutingFlow`) использует
именованные доводы и предметный инициализатор, поэтому ни один вызывающий не правится.

**Договор входа потока.** `DesignConversionRuntimeFlowInput` получил такое же последнее
необязательное поле, и поток передаёт его критику **как есть**, без доработки и без домысливания.
Так путь существует целиком: от вызывающего поток до доказательств критика. Обёртка обученных
весов (`DesignConversionMachine.LearnedWeightCriticUnit`) передаёт запрос в каркас без
пересборки входа, поэтому геометрия проходит через неё без потери.

**Что делает геометрический потребитель.** Он стоит **рядом** с агрегатором, а не вместо него, и
не решает ничего: переданные ряды он превращает в ссылки-доказательства
`scene-geometry:<элемент>=<левый>,<верхний>,<ширина>,<высота>` (запись долей неизменна на любой
машине), добавляет счёт `scene-geometry-usable-rows:<пригодных>/<передано>`, ставит отметку
`DESIGN_CONVERSION_CRITIC_SCENE_GEOMETRY_SUPPLIED` и предупреждение
`SCENE_GEOMETRY_SEEN_BUT_CRITIC_IS_STILL_UNTRAINED_SCAFFOLD`. Непригодный ряд **называется**
причиной и местом — `scene-geometry-unusable:<номер>:<missing-row|empty-element-id|non-finite-number|non-positive-size>`,
— а не судится молча. Пустой переданный список назван отдельно
(`...SCENE_GEOMETRY_SUPPLIED_WITHOUT_ROWS`) и за увиденную геометрию не выдаётся.

**Чего это НЕ делает.** Оценка, решение «прошло или не прошло», перечень дефектов и уверенность
единицы от геометрии не зависят ни на одном пути — их по-прежнему даёт только свод итогов
проверки качества. Когда геометрии нет, потребитель не добавляет ни ссылок, ни предупреждений,
поэтому исторические пакеты отвечают прежним составом ряд в ряд, а доступ к геометрии для них
обязательным не становится. Критик обученным не называется ни в каком виде: он честный
необученный каркас, который теперь видит геометрию, и его собственное предупреждение говорит
именно это. Обученная память геометрию не потребляет: набор её признаков
(`BuildCriticRuntimeFeatures`) не менялся ни на одно поле — это ровно граница самого кандидата,
«до переноса нужен опыт на независимых семьях геометрии», которого ещё нет. Отдельно и прямо:
сегодня ни один продуктовый вызывающий геометрию не передаёт, поэтому путь открыт, но пуст;
производитель прямоугольников сцены — отдельный кусок, и подменять его прямоугольниками дефектов
нельзя, это разные вещи.

**Проверки.** `Tests/ExperimentFixes/VisualCriticSeesSceneGeometryTests.cs`: геометрия видна в
доказательствах вердикта и единицы там, где прежнее правило её не возвращало; непригодные ряды
названы причиной; пустой список назван пустым; каркас остался каркасом при увиденной геометрии;
поток доносит геометрию до критика через настоящий производственный путь — пять случаев, красных
на прежней логике, потому что поля геометрии там не существовало. Плюс два охранных: без
геометрии состав доказательств и обе прежние строки предупреждений сверяются дословно, и
геометрия не сдвигает ни оценку, ни решение, ни перечень дефектов. Прежнее правило воспроизведено
функцией входа `PriorEvidenceRefsWithoutGeometry`: зависимость от набора ссылок доказана вторым
входом, а по геометрии прежнее поведение названо **постоянным** прямо, потому что таким оно и
было. Прогона не было: сборка и прогон проверок запрещены средой у исполнителя и правилом у
координатора, зелёными они не называются; правка доказана разбором типов по всем затронутым
местам.

## 27.TranslationDomain — байты записи кандидата определены один раз (3 ноября 2026, приёмка Q01)

**Что было сломано.** Подтверждение записи проигравшего кандидата сравнивало свёртку текста без
метки порядка байтов со свёрткой файла, записанного текстовым писателем, который эту метку
выдаёт. Совпасть они не могли никогда: расхождение ровно в три первых байта. Файл на диске был
правильным, но правдивое «сохранено» не возвращалось, и настоящая причина отказа — смена версии
состояния или отмена — подменялась кодом «сохранение кандидата не удалось».

**Новое единственное определение байтов.** `TranslationAtomicJsonWriter.Utf8BytesOf(string?)` —
кодирование объявлено явно и без метки порядка байтов. Свёртка текста, запись на диск и сравнение
после перечитывания используют РОВНО этот массив. Оба писателя — синхронный `WriteAtomically` и
асинхронный `WriteAtomicallyAsync` — пишут байты, а не текст, потому что подтверждение не должно
зависеть от того, каким из двух путей файл попал на диск.

**Чего это НЕ меняет.** Подписи обоих писателей прежние, поэтому ни один вызывающий не правился:
ими пользуются запись кандидата и выгрузка контрольной точки машины перевода. Текстовым читателям
отсутствие метки не вредит — они её и так снимали сами, а разбор JSON с меткой в начале вообще
невозможен. Ожидания проверок не менялись ни в одной строке: дефект был в продукте, и исправлен
продукт.

**Измерено.** Целевой прогон в Release до правки: 29 проверок, 26 прошло, 3 упало. После правки и
полной пересборки: 36 проверок, 36 прошло, 0 упало; в срез добавлена приёмка устаревшего вызова
оценки. Отпечатки сборок до и после различаются, то есть прогон шёл по новой сборке.

## 11.DPO — обучение предпочтениями использует сдвинутый вход и всю полезную длину окна (8 ноября 2026)

**Что было сломано.** Чтобы оценить ответ, модель читает запрос вместе с ответом и предсказывает
каждый следующий знак: по знаку с номером i предсказывается знак с номером i + 1. Модели подавали
ВСЮ строку целиком, включая самый последний знак, хотя предсказывать по нему уже нечего — за ним
ничего нет. Из-за этого в окно модели обязана была влезть вся строка, и законный урок длиной на
один знак больше окна отклонялся целиком по вместимости, ещё до первого прохода модели: ноль
обновлений весов вместо обучения. Опыт `experiments-v2/RP01-MICRO/SC01` (попытка 0001) измерил это
прямо: для строки длиной 65 при окне 64 обновлений стало 0 → 200, у первой учебной строки верных
знаков 3 из 49 → 4 из 49. Полных правильных ответов 0 из 2 → 0 из 2, то есть это ремонт вместимости
входа, а не освоение задачи.

**Новая версия исполнения.** `DpoExecutionProfile` получил поле `InputVersion` и две его
допустимые величины:

```csharp
const string FullSequenceInputVersion   = "full-sequence-input-v1"     // прежний полный вход
const string ShiftedPrefixInputVersion  = "shifted-prefix-input-v2"    // сдвинутый префикс
string       InputVersion { get; }
static DpoExecutionProfile Capture(DpoBatch validatedBatch, IReadOnlyList<AdamWParameterBinding> bindings,
                                   string inputVersion = FullSequenceInputVersion)
static DpoExecutionProfile FromVerifiedManifest(string material, string framing, string objective,
                                   IEnumerable<DpoAllowedParameterIdentity> parameters,
                                   string inputVersion = FullSequenceInputVersion)
```

Объявлено в: `11.DPO/DpoTrainer.cs`. Версия входа — часть личности исполнения наравне с версиями
материала, обрамления и цели: `Matches` сверяет её дословно, поэтому продолжение, снятое на одном
входе, не подходит другому. Квитанция прежнего формата версию входа не записывала, поэтому
восстановленный профиль описывает полный вход — это отказ в безопасную сторону.

**Как это назначается тренеру.** `DpoTrainer.CreateForProjectLora` получил последний параметр
`string inputVersion = DpoExecutionProfile.FullSequenceInputVersion`. Версия входа назначается при
создании и дальше не меняется, поэтому один тренер не может подать модели полный вход на одном шаге
и префикс на другом. Несогласованная пара останавливается в конструкторе кодом
`DPO_CONTINUATION_PROFILE_MISMATCH`: объявленный профиль обязан нести ту же версию входа, а без
профиля допустим только прежний полный вход.

**Новое правило вместимости.** `DpoExactPairMaterial.CaptureAndValidate` получил последний параметр
`bool shiftedInput = false` и передаёт его в проверку материала. При лжи — прежний договор: в окно
обязаны влезть все T знаков. При истине модель получает префикс без последнего знака, поэтому в окно
обязаны влезть T − 1 знаков, то есть допустимо T = C + 1, а T = C + 2 по-прежнему отклоняется тем же
кодом `DPO_EXACT_MATERIAL_CAPACITY_EXCEEDED`. Цели не обрезаются, и вместимость входа не подменяется
числом оцениваемых знаков. Меньшее из двух окон остаётся прежним: `min(policyC, referenceC)`.

**Общий счёт оцениваемых строк.** `11.DPO/DpoTrainer.cs`, `TargetRowCount(logits, ids, mask,
shiftedInput)` — один помощник и для прямого, и для обратного прохода. Оцениваемых строк всегда
T − 1; строк, которые дала модель, при полном входе T, а при сдвинутом ровно T − 1. Форма, не
совпавшая ни с тем, ни с другим, останавливает счёт новым кодом
`DPO_TARGET_ROW_COUNT_INVALID`; знаки, из которых нельзя взять префикс, — кодом
`DPO_SHIFTED_INPUT_SHAPE_INVALID`. Граф оценки хранит ПОЛНЫЕ знаки и полную маску ответа, поэтому
последняя цель при входе на один знак короче не исчезает; прежняя нормализация суммой сохранена,
прежний материал без точных версий сохраняет полный вход.

**Личность памяти ответов эталона.** Ключ `DpoReferenceScoreCache.Key` получил последнее поле
`string inputVersion`. Ответ, снятый на полном входе, не выдаётся за ответ на префиксе, даже когда
материал, словарь и замороженный эталон те же.

**Чего это НЕ меняет.** Путь без нового профиля остаётся побайтово прежним: число оцениваемых строк
теперь берётся из длины знаков, но при полном входе это ровно прежнее «строк модели минус одна»,
порядок дробных действий и сами действия не менялись. Прежние формы вызова сохранены как значения по
умолчанию, поэтому ни один вызывающий в `14.ContinuousLearning` не правился. Подписи приватных
помощников `ComputeTargetLogProbs` (четыре параметра) и первичного конструктора графа оценки (пять
значений) сохранены дословно, потому что их вызывают по позициям из уже существующих проверок.

**Измерено.** Сборка проектов обучения предпочтениями, непрерывного обучения и тестового проекта —
код выхода 0, 0 ошибок. Целевой прогон нового класса проверок — 4 из 4 зелёных. Доказательство
чувствительности: при временно отключённом новом правиле вместимости упала РОВНО решающая проверка
(ожидался принятый урок длиной окно плюс один, получен отказ
`DPO_EXACT_MATERIAL_CAPACITY_EXCEEDED`), остальные три остались зелёными; после отката поломки снова
4 из 4. Назначенная карточкой тяжёлая проверка на настоящих обученных весах донора (200 обновлений,
824 прямых и 404 обратных прохода) в этом срезе НЕ выполнялась и зелёной не называется.

## 44.DesignConversionDomain — сверочный набор моделей приходит от поставщика, отпечаток остаётся у моста (9 ноября 2026)

**Сборка.** `44.DesignConversionDomain.dll`, файл `44.DesignConversionDomain/DesignConversionDpoPolicyBaseProvider.cs`.

**Назначение одной фразой.** Узкий договор продолжения получил вход, которым мост контрольных
точек политики просит у внедрённого поставщика три независимые сверочные модели по ролям вместо
того, чтобы строить языковую модель внутри себя.

**Зачем это понадобилось.** До переноса мост сам создавал языковую модель в закрытом методе и тем
нарушал правило 2 проекта (внешние зависимости приходят через интерфейсы, а не через конкретные
типы). Инвариант «единственный источник моделей для моста — внедрённый поставщик» перестал быть
истинным, и это ловила проверка по байт-коду. Замысел разделения уже был записан в самом мосте
комментарием прежнего автора: отпечаток принадлежит мосту, а модели отдаёт поставщик.

### IDesignConversionDpoPolicyContinuationProvider

**Подпись нового члена.**

```csharp
public interface IDesignConversionDpoPolicyContinuationProvider
    : IDesignConversionDpoPolicyBaseProvider
{
    // Код отказа отдельный от кода про описатель подготовки, чтобы отказ на
    // пути продолжения был опознаваем именно как отсутствие сверочного входа.
    public const string VerificationModelSetUnavailableErrorCode =
        "DESIGN_CONVERSION_DPO_POLICY_BASE_VERIFICATION_MODEL_SET_UNAVAILABLE";

    Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>
        CreateContinuationVerificationModelSetAsync(
            ModelConfig config,
            DesignConversionDpoPolicyRandomContext randomContext,
            CancellationToken cancellationToken)
        => Task.FromResult(OperationResult<DesignConversionDpoPolicyBaseModelSet>.Fail(
            "The DesignConversion DPO continuation verification model set is unavailable.",
            VerificationModelSetUnavailableErrorCode));
}
```

**Отказ по умолчанию закрыт.** Поставщик, который сверочный вход не переопределил, путь
продолжения молча не проходит: он получает отказ с кодом
`DESIGN_CONVERSION_DPO_POLICY_BASE_VERIFICATION_MODEL_SET_UNAVAILABLE`. Тем же кодом мост отвечает,
когда внедрённый поставщик договором продолжения не является (приведение типа не прошло) — прогон с
продолжением на поставщике без продолжения недопустим по построению.

**Список реализаций — полный, все шесть.** Частичная синхронизация хуже отсутствующей, поэтому
перечислены и боевые, и тестовые:

| Реализация | Файл | Как реализует |
|---|---|---|
| `DesignConversionCheckpointBackedDpoPolicyBaseProvider` | `44.DesignConversionDomain/DesignConversionCheckpointBackedDpoPolicyBaseProvider.cs` | строит сам: копия настройки плюс посев канонной топологии по роли |
| `DesignConversionRetainedDpoPolicyBaseProviderGateway` | `25.ModelServer/DesignConversion/DesignConversionRetainedDpoPolicyBaseComposition.cs` | передаёт связанному доверенному поставщику; без связывания отказывает |
| `DesignConversionDpoPolicyBaseTestProvider` | `Tests/DesignConversionDpoPolicyBaseTestProvider.cs` | строит сам, отдельный счётчик сверочных обращений |
| `CountingContinuationProvider` | `Tests/DesignConversionDpoPolicyContinuationTests.cs` | строит сам, отдельный счётчик |
| `RecordingContinuationProvider` | `Tests/DesignConversionDpoPromotedParentContinuationTests.cs` | передаёт внутреннему, отдельный счётчик |
| `RecordingEvaluatedContinuationProvider` | `Tests/DesignConversionDpoStorageTests.cs` | передаёт внутреннему, отдельный счётчик |

**Список потребителей.** Один: `DesignConversionDpoPolicyCheckpointBridge`, метод построения
доказательства неизменности продолжения
(`CreateBridgeOwnedContinuationMutationProofAsync` в
`44.DesignConversionDomain/DesignConversionDpoPolicyCheckpointBridge.cs`). Он приводит
`preparation.Provider` к узкому договору, берёт набор, загружает в три модели допущенную
контрольную точку и считает отпечаток **у себя**. Создание языковой модели в файле моста после
переноса не встречается ни разу.

**Что сверочный набор НЕ делает.** Он расходный: в его модели загружается контрольная точка, и
обучаемые модели он не затрагивает. Поэтому он приходит отдельным входом, а не повторным
обращением за набором обучаемых моделей — существующий счётчик обращений к поставщику этим входом
не увеличивается, у сверочных обращений свой счётчик.

**Наблюдение для будущих проверок.** Роль модели (политика, опорная, свежая загрузка) влияет на
посев канонной топологии малых поправок, но **не выживает загрузку контрольной точки**: после
загрузки состояние трёх ролей определяется самой точкой. Поэтому сравнение ролей имеет смысл
только до загрузки и только под детерминированным посевом.
