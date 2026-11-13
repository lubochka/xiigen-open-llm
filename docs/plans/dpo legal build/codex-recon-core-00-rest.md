# Codex Recon — 00.Core, остальное помимо контрактов ILearningMachine

Под-задача: кроме контрактов ILearningMachine — где и как используются `OperationResult<T>`,
BaseService, ISessionStorage, реестры/локаторы. Сбор evidence для ПЛАНА ILearningDynamicGraph.
Только реально найденные пути; чужие файлы не редактировались.

## Что это и где (точные ключевые пути)

`00.Core` физически содержит РОВНО 3 файла исходников + проект:
- `C:/Projects/llm_mvp_core/00.Core/OperationResult.cs` — тип `OperationResult<T>`.
- `C:/Projects/llm_mvp_core/00.Core/LogicalContracts.cs` — контракты уровней Logical (Unit/Flow/Orchestrator) + вспомогательные record + `ITrustScorer`.
- `C:/Projects/llm_mvp_core/00.Core/LearningMachineContracts.cs` — контракты `ILearningMachine`/`ILearningMachinePair` (это «контракты ILearningMachine», вне моей под-задачи; включены для границы).
- `C:/Projects/llm_mvp_core/00.Core/Core.csproj` — namespace по умолчанию `AgentInfrastructure`; единственная ссылка: `..\29.SemanticCompiler\SemanticCompiler.csproj` (для `SemanticCompiler29.Domain`).

ВАЖНО (опровержение формулировки задачи): в `00.Core` НЕТ `BaseService`, НЕТ `ISessionStorage`,
НЕТ реестров/локаторов. Они живут в других проектах:
- `BaseService` и `ISessionStorage` → `C:/Projects/llm_mvp_core/21.AgentInfrastructure/Core/Core.cs`
  (подтверждено grep `class BaseService` / `interface ISessionStorage`; в `00.Core` совпадений нет).
- Реестры/локаторы модели → `C:/Projects/llm_mvp_core/25.ModelServer/Remote/RemoteUnitManifest.cs`
  (`RemoteUnitManifest`), а `IModelRepository`/`IRemoteUnitLocator` объявлены в проекте 25.ModelServer
  (см. также `docs/SUBPROJECT-MAP.md`, `docs/COMPONENT-INTERFACES.md`, `docs/projects/25-model-server.md`).
  В `00.Core/LearningMachineContracts.cs` они лишь УПОМИНАЮТСЯ в комментариях (CheckpointRef/ManifestRef
  «point into 25.ModelServer»), но не объявлены.

Замечание о `.claude/worktrees/*`: это агентские worktree-копии (дубликаты дерева). Канонический корень —
`C:/Projects/llm_mvp_core/...`. Дубли в worktrees — evidence-only, в плане не использовать.

## Как устроено / работает (кратко)

OperationResult.cs:
- `public sealed class OperationResult<T>` в namespace `AgentInfrastructure`. Поля `IsSuccess`,
  `Value`, `ErrorMessage`, `ErrorCode`; фабрики `Ok(value)` / `Fail(message, code="ERROR")`;
  `MapError<TNew>()` для проброса ошибки между типами. Это реализация Правила 1 (никаких throw/null/bool
  без контекста). Внимание на границы сборок (CLAUDE.md «OperationResult<T> namespaces»): отдельные типы
  `AgentInfrastructure.OperationResult<T>` (этот, 00.Core), `AgentAlgorithms.OperationResult<T>` (внутри 22),
  `SemanticCompiler29.OperationResult<T>` (внутри 29) — не смешивать.

LogicalContracts.cs (namespace `AgentInfrastructure.Logical`):
- `LogicalExecutionFlags` — record с флагами исполнения/обучения, разбитыми на три группы имён:
  Optimization (`UseLearnedUnitWhenAvailable`, `UseConfidenceFloor`, `UseSkillRetriever`,
  `UseReplayFusionRanking`, `UseDomainScopedRetrieval`, `UseTraceEvidence`, `UseContextCarryForward`,
  `UseIdleLearningScheduling`, `UseE35DependencyDetection`, `UseCompiledPredicateRepresentation`),
  Safety (`UseArbiterAuthority`, `UseInternalFirstExecution`, `RequireExternalAiApproval`,
  `RejectArbiterContradictions`, `UseKnowledgeEdgeDetection`), ExperimentControl
  (`UseDeterministicFixtureBaseline`, `WriteEvidenceArtifacts`, `CompareAgainstOracle`, `FailOnMetricDrift`).
- Конверты Unit-уровня: `LogicalUnitRequest<TInput>` (RunId/UnitId/UnitRole/DomainTag/Input/Flags/Metadata)
  и `LogicalUnitResult<TOutput>` (Output, Confidence, PredicateIds, DomainTags, EvidenceIds, Warnings +
  опциональный `TransitionSignal`).
- `PhaseTransitionSignal(SuggestedDomain, TransitionPrompt, SourceNodeId, SourceSubPhaseId, EvidenceIds)` —
  advisory-запрос на повторный вход в оркестрацию после ветки.
- Контракт `ILogicalUnit<TInput,TOutput>.ExecuteAsync(...) → OperationResult<LogicalUnitResult<TOutput>>`.
- Flow-уровень: `LogicalFlowRequest<TInput>`, `LogicalFlowSummary` (baseline/enhanced метрики, Passed,
  ShouldScaleUp, NextAction, ScalingSignal…), `LogicalFlowResult` (+ массив `TransitionSignals`),
  `LogicalContextFragment` и `LogicalContextPacket` (перенос контекста между ветками/прогонами),
  контракт `ILogicalFlow<TInput>.ExecuteAsync(...) → OperationResult<LogicalFlowResult>`.
- Orchestrator-уровень: `LogicalOrchestratorRequest`, `LogicalOrchestratorResult`
  (+ `BranchResults`, `Warnings`, `OutputContextPacket`), `LogicalFlowBinding(FlowId, ILogicalFlow<string>)`
  c `ScenarioId`, контракт `ILogicalOrchestrator.RunAsync(...) → OperationResult<LogicalOrchestratorResult>`.
- E40V2 trust-scoring: enum `TrustLevel{Low,Medium,High}`, `SourceFeatures`, `TrustScorerOutput(Score,Level)`,
  контракт `ITrustScorer.Score(SourceFeatures) → OperationResult<TrustScorerOutput>` (реализация в проекте 16,
  потребитель — проект 21; см. комментарий в файле).

LearningMachineContracts.cs (граница под-задачи): record-типы тренировочного цикла
(`LearningBatch<TDomain>`, `LearningTrainingItem` = DPO-тройка prompt/chosen/rejected, `LearningBatchResult`,
`LearningEvaluationSet/Item/Result`, `LearningMachineExport` с CheckpointRef/ManifestRef в 25.ModelServer,
матрица покрытия пар `LearningMachinePair*`), контракты `ILearningMachine<TDomain>` и
`ILearningMachinePair<TPersonal,TGlobal>`.

Где используются контракты (потребители, не определения; подтверждено grep по
`LogicalExecutionFlags|PhaseTransitionSignal|LogicalContextPacket|ITrustScorer`):
`43.SmartSkillsDomain/*` (SkillSelection*/PromptToTask*/OrchestrationReasoning*/ResultGap* иерархии и Machine),
`27.TranslationDomain/*` (TranslationMachine, TatoebaPilotLoopRunner), `42.ProjectWikiDomain/*`
(OrchestrationPlanOrchestrator), `26.TrainableRouter`, и широко в `Tests/*`.

## Релевантность к ILearningDynamicGraph (DPO/обучение/граф/оркестрация/измерение/teacher-provenance)

ОЧЕНЬ ВЫСОКАЯ (это фундамент уровней). Конкретно:
- Граф/узлы/рёбра: `ILogicalUnit` = узел графа; `ILogicalFlow` = топология узлов; `ILogicalOrchestrator` =
  координатор. `PhaseTransitionSignal` + `LogicalFlowResult.TransitionSignals` = механизм рёбер/перехода
  (динамическая повторная маршрутизация). `LogicalContextFragment`/`LogicalContextPacket` = fan-in/контекст
  между ветками. Для ILearningDynamicGraph это естественные точки расширения «много-ко-многим» и async-графа.
- Измерение/гейты: `LogicalFlowSummary` (baseline vs enhanced, Passed, ScalingSignal, FailOnMetricDrift) и
  `LearningEvaluationResult.PassesGate` — готовые поля для метрик/гейтов обучения графа.
- DPO/обучение: `LearningBatch`/`LearningTrainingItem`/`LearningBatchResult` дают формат DPO-троек и метрики
  батча — каркас, который авто-построитель DPO для Unit/Flow/Orchestrator/DynamicGraph должен заполнять.
- Teacher-provenance (legal-build): в `00.Core` НЕТ ни одного поля провенанса учителя
  (teacher_access_mode/hosting_provider/model_id/model_license/provider_terms_checked/closed_proprietary_model).
  `LearningTrainingItem`/`LearningBatch.SourceIds` несут только ItemId/SourceIds. Это разрыв для «legal teacher»
  гейта (см. trainable-architecture-decision-SKILL.md «Legal Teacher / Provenance Gate»): план DynamicGraph
  должен добавить типизированные поля провенанса в контракт обучающих строк/батча, иначе DPO-данные не пройдут
  лицензионный гейт.
- Флаги: `LogicalExecutionFlags.UseLearnedUnitWhenAvailable`, `UseConfidenceFloor`, `UseSkillRetriever`,
  `UseReplayFusionRanking` напрямую управляют «учить/использовать обученное» — крючки для DynamicGraph рёбер.

## Классификация (правила trainable-architecture-decision-SKILL.md)

- `OperationResult<T>` — НЕ архитектурный уровень. Утилита результата/ошибок (инфраструктурный примитив).
  Не trainable, не детерминистический «скрытый» решатель. Вне триггеров скила.
- `LogicalExecutionFlags`, `LogicalUnit/Flow/Orchestrator*Request/Result`, `PhaseTransitionSignal`,
  `LogicalContextFragment/Packet`, `LogicalFlowBinding`, `LearningBatch/Item/Result`,
  `LearningEvaluation*`, `LearningMachineExport`, `LearningMachinePair*` — это record/DTO и интерфейсные
  ПОДПИСИ контрактов. По скилу (строки про «generic interface signatures … may appear only as implementation
  contract signatures»): сами по себе они НЕ создают trainable-уровни и НЕ являются authority. Это контрактная
  поверхность.
- Разрешённые trainable-уровни, объявленные здесь как контракты (определения, не реализации):
  `ILogicalUnit` (Level 2), `ILogicalFlow` (Level 3), `ILogicalOrchestrator` (Level 4),
  `ILearningMachine` (Level 5), `ILearningMachinePair` (Level 1). Реализации и доказательства жизненного
  цикла — в других проектах (43/27/42/26/25 и т.д.), не в `00.Core`.
- `ITrustScorer` + `SourceFeatures`/`TrustScorerOutput`/`TrustLevel` — КОНТРАКТ скорера. Это кандидат на
  аудит-триггер скила (scorer). Внутри `00.Core` — только подпись (allowed как contract signature). Сам по
  себе скорер НЕ является разрешённым trainable-уровнем: если в реализации (проект 16) он станет финальной
  runtime-authority для семантического/обучаемого решения без обёртки в `ILogicalUnit`/`ILearningMachine` и без
  lifecycle-доказательств — это `FORBIDDEN_RUNTIME_SHORTCUT` / `BASELINE_MUST_BE_LABELLED`. В `00.Core`
  вердикт: `OK_INTERNAL_DETAIL` как контракт; реализацию нужно классифицировать отдельно (вне моей цели).
- `BaseService`/`ISessionStorage` — НЕ в `00.Core` (в 21.AgentInfrastructure). Это инфраструктура
  персистентности сессии (паттерн BaseService(ISessionStorage)). Не trainable-уровень; персистентность —
  потенциальный носитель чекпойнтов/состояния для lifecycle, но классифицировать по коду проекта 21.
- Реестры/локаторы (`IModelRepository`/`IRemoteUnitLocator`/`RemoteUnitManifest`) — НЕ в `00.Core`
  (в 25.ModelServer). Это локатор/манифест публикации чекпойнтов (часть lifecycle-доказательств:
  locator/manifest/export-import), не trainable-решатель. Классифицировать по коду проекта 25.

Скрытых детерминистических решателей внутри `00.Core` НЕ обнаружено: файлы содержат только контракты, record
и фабрику результата (нет словарей/счётчиков/regex/`TryDecide*`/scorer-реализаций).

## Пробелы / что доисследовать

- Реализации уровней и их lifecycle (checkpoint/manifest/locator/fresh-load/continue-training/ablation):
  смотреть `25.ModelServer/*`, `43.SmartSkillsDomain/*`, `27.TranslationDomain/*`, `42.ProjectWikiDomain/*`,
  `26.TrainableRouter/*` — вне `00.Core`.
- `BaseService`/`ISessionStorage`: точное содержимое и потребители — `21.AgentInfrastructure/Core/Core.cs`
  (не читал в этой под-задаче; цель — `00.Core`).
- `IModelRepository`/`IRemoteUnitLocator`: точные объявления и сигнатуры — `25.ModelServer/Remote/*`
  и `docs/COMPONENT-INTERFACES.md` (для плана граф-узлов/публикации чекпойнтов).
- `ITrustScorer`: классификация РЕАЛИЗАЦИИ (проект 16) — trainable vs baseline; нужно отдельным рекон-проходом.
- Teacher-provenance поля: в контрактах обучения их нет — план ILearningDynamicGraph должен решить, где
  завести типизированный record провенанса (расширение `LearningTrainingItem`/`LearningBatch` или новый тип).
- Для границы графа («много-ко-многим», multi-output edge contracts, async-граф) текущие контракты дают
  одиночный `LogicalFlowResult` + массив `TransitionSignals`; нужно проверить, хватит ли этого для fan-out/fan-in
  DynamicGraph или потребуется новый контракт рёбер (решение плана, не факт кода).
- Канонический документ-сверка: `docs/projects/00-core.md` (не читал; бюджет файлов) — для перекрёстной
  проверки описания 00.Core.
