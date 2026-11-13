# Рекон: 00.Core — контракты ILearningMachine (5 уровней)

Цель рекона: собрать evidence для ПЛАНА проекта ILearningDynamicGraph (авто-построение DPO).
Это сбор фактов, не разрешение на реализацию. Пути — только реально найденные.

## Что это и где (точные ключевые пути)

- `C:/Projects/llm_mvp_core/00.Core/LearningMachineContracts.cs` — Level 5 `ILearningMachine<TDomain>` (стр. 208–234) и Level 1 `ILearningMachinePair<TPersonal,TGlobal>` (стр. 245–261); плюс DTO обучения/оценки/экспорта и матрица покрытия пар.
- `C:/Projects/llm_mvp_core/00.Core/LogicalContracts.cs` — Level 4 `ILogicalOrchestrator` (стр. 202–207), Level 3 `ILogicalFlow<TInput>` (стр. 166–171), Level 2 `ILogicalUnit<TInput,TOutput>` (стр. 97–102); плюс все Request/Result-записи и `LogicalExecutionFlags`.
- `C:/Projects/llm_mvp_core/00.Core/OperationResult.cs` — `AgentInfrastructure.OperationResult<T>` (стр. 3–21), тип-обёртка всех возвратов.
- Адъюнкт (НЕ в 00.Core): `C:/Projects/llm_mvp_core/21.AgentInfrastructure/Logical/LogicalCapabilityTrainingContracts.cs` и `C:/Projects/llm_mvp_core/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs` — реализуют путь обучения `ILogicalCapabilityTrainer`, на который ссылается комментарий в Level 5 (стр. 204–207).

Namespace всех контрактов: `AgentInfrastructure.Logical`. `OperationResult<T>` — из `AgentInfrastructure`.

## Как устроено/работает (кратко)

Иерархия (комментарий в файле, стр. 197–202): внешнее кольцо — `ILearningMachine`, под ним оркестратор → flow → unit. Это контракты, без реализаций.

5 уровней — сигнатуры, Input/Output:

- Level 5 — `ILearningMachine<TDomain>` (LearningMachineContracts.cs:208):
  - `TrainBatchAsync(LearningBatch<TDomain> batch, CancellationToken)` → `OperationResult<LearningBatchResult>`. Input: `LearningBatch<TDomain>` (BatchId, DomainTag, `IReadOnlyList<LearningTrainingItem>` = DPO-триплеты Prompt/ChosenResponse/RejectedResponse, SourceIds). Output: `LearningBatchResult` (RowsTrained, AccuracyBefore/After, RewardMargin, CpuMs, PeakRamMb, ElapsedMs, PromotedUnitIds).
  - `EvaluateAsync(LearningEvaluationSet<TDomain>, CancellationToken)` → `OperationResult<LearningEvaluationResult>` (Accuracy, RewardMargin, PassesGate, GateNote). Только измерение, веса не меняет.
  - `ExportAsync(CancellationToken)` → `OperationResult<LearningMachineExport>` (MachineId, DomainTag, Version, CheckpointRef, ManifestRef → 25.ModelServer, AccuracyAtExport, ExportedAtUtc).
  - `ImportAsync(LearningMachineExport, CancellationToken)` → `OperationResult<bool>`.
  - `GetOrchestratorAsync(CancellationToken)` → `OperationResult<ILogicalOrchestrator>` — runtime-вход в домен поверх обученного состояния.
- Level 4 — `ILogicalOrchestrator` (LogicalContracts.cs:202):
  - `RunAsync(LogicalOrchestratorRequest, CancellationToken)` → `OperationResult<LogicalOrchestratorResult>`. Input: `LogicalOrchestratorRequest` (RunId, ScenarioId, UserRequest, `LogicalExecutionFlags?`, Metadata). Output: `LogicalOrchestratorResult` (SelectedFlowId, OverallVerdict, FinalHumanResult, AppliedFlags, FlowResult; + BranchResults[], Warnings[], OutputContextPacket).
- Level 3 — `ILogicalFlow<TInput>` (LogicalContracts.cs:166):
  - `ExecuteAsync(LogicalFlowRequest<TInput>, CancellationToken)` → `OperationResult<LogicalFlowResult>`. Input: `LogicalFlowRequest<TInput>` (RunId, FlowId, ScenarioId, Input, Flags, Metadata). Output: `LogicalFlowResult` (FlowId, ScenarioId, OverallVerdict, FinalHumanResult, `LogicalFlowSummary`, Metrics, EvidenceIds[], Warnings[]; + `PhaseTransitionSignal[] TransitionSignals`).
- Level 2 — `ILogicalUnit<TInput,TOutput>` (LogicalContracts.cs:97):
  - `ExecuteAsync(LogicalUnitRequest<TInput>, CancellationToken)` → `OperationResult<LogicalUnitResult<TOutput>>`. Input: `LogicalUnitRequest<TInput>` (RunId, UnitId, UnitRole, DomainTag, Input, Flags, Metadata). Output: `LogicalUnitResult<TOutput>` (UnitId, UnitRole, DomainTag, Output, Confidence, PredicateIds[], DomainTags[], EvidenceIds[], Warnings[]; + опц. `PhaseTransitionSignal? TransitionSignal`).
- Level 1 — `ILearningMachinePair<TPersonal,TGlobal>` (LearningMachineContracts.cs:245):
  - `Personal { get; }` → `ILearningMachine<TPersonal>`; `Global { get; }` → `ILearningMachine<TGlobal>`.
  - `SyncPersonalToGlobalAsync(CancellationToken)` → `OperationResult<bool>` — только анонимизированный сигнал (A-LM-08, стр. 253–256: без сырого контента/PII).
  - `PullGlobalToPersonalAsync(CancellationToken)` → `OperationResult<bool>`.

Правило A-LM-02 (зафиксировано в шапке файла, стр. 8): `Train*` существует ТОЛЬКО на `ILearningMachine`. `ILogicalUnit`, `ILogicalFlow`, `ILogicalOrchestrator` — чистые inference-контракты (только `ExecuteAsync`/`RunAsync`), методов обучения не несут. Подтверждено чтением: ни в одном из трёх нижних интерфейсов метода Train нет. Связано с A-LM-01 (стр. 7): `ILearningMachine` стоит над `ILogicalOrchestrator`. Путь обучения (комментарий стр. 204–207): `TrainBatchAsync` → внутри вызывает `ILogicalCapabilityTrainer.TrainAndRegisterAsync()` → существующие `*DpoGenerator + *TrainingRunner` на каждый unit.

## Релевантность к ILearningDynamicGraph (DPO/обучение/граф/оркестрация/измерение/teacher-provenance)

- DPO: `LearningTrainingItem` (Prompt/ChosenResponse/RejectedResponse) — это и есть формат preference-триплета, который план авто-построения DPO должен наполнять; `LearningBatch<TDomain>` — единица батча с `SourceIds` (provenance-крючок).
- Обучение/измерение: `LearningBatchResult` и `LearningEvaluationResult` уже содержат поля для held-out accuracy, reward margin, gate (`PassesGate`/`GateNote`) и ресурсы (CpuMs/RamMb/ElapsedMs) — это готовый контракт для численных метрик, которых требует gate «trained feature, не scaffold».
- Граф/оркестрация: текущая модель — линейная иерархия (Machine→Orchestrator→Flow→Unit) с fan-out через `BranchResults[]`/`TransitionSignals[]`. Это НЕ DynamicGraph: нет явных узлов/рёбер, multi-output edge-контрактов, fan-in, async-graph-execution и владельца авторитета на ребре (требования Graph-Style/DynamicGraph Reconstruction Gate скила). План ILearningDynamicGraph должен будет добавить эти контракты поверх/рядом.
- Teacher-provenance: на уровне 00.Core полей teacher/provenance НЕТ (есть только `SourceIds` в батче). Legal Teacher Gate (teacher_access_mode, model_id, model_license, closed_proprietary_model и т.д.) контрактами 00.Core не покрыт — это пробел для плана «dpo legal build».
- Lifecycle (load user→repo fallback→export/import/share→continue-training): на уровне контрактов есть `Export/Import` (Level 5) и common+personal-разделение (Level 1 пара + `LearningMachinePairCoverageMatrix`), но порядка загрузки и continue-from-checkpoint контракт не описывает — это поведение реализации.

## Классификация по правилам скила (trainable-architecture-decision-SKILL.md)

- `ILearningMachine<TDomain>`, `ILogicalOrchestrator`, `ILogicalFlow<TInput>`, `ILogicalUnit<TInput,TOutput>`, `ILearningMachinePair<TPersonal,TGlobal>` — это РОВНО пять разрешённых trainable-уровней архитектуры (скил, «only allowed trainable architecture levels», стр. 91–98). Дженерик-сигнатуры (`<TDomain>`, `<TInput>`, `<TInput,TOutput>`, `<TPersonal,TGlobal>`) — только сигнатуры контрактов, новых уровней не создают (скил, стр. 100–103). Вердикт по самим контрактам: соответствуют разрешённым уровням (контракт-only, без реализации — это не доказательство обученности, а каркас).
- `OperationResult<T>` — НЕ архитектурный уровень: внутренняя обёртка возврата (проектное Rule 1). Внутренняя деталь.
- `LogicalExecutionFlags`, все Request/Result/Context-записи, `LearningBatch*`, `LearningEvaluation*`, `LearningMachineExport`, `LearningMachinePair*CoverageRow/Matrix` — DTO/контракты данных, не уровни и не trainable-механизмы.
- `ITrustScorer` / `TrustScorerOutput` / `SourceFeatures` / `TrustLevel` (LogicalContracts.cs:209–229) — это scorer-контракт (E40V2 trust). По правилу скила scorer сам по себе НЕ архитектурный уровень и НЕ финальный trainable-авторитет; допустим только как внутренняя деталь/компонент внутри разрешённого уровня или как помеченный baseline. Для плана: если trust-скоринг участвует в выборе на ребре графа — он должен быть обёрнут разрешённым уровнем либо явно помечен eval-only/helper, иначе риск `FORBIDDEN_RUNTIME_SHORTCUT`. В 00.Core это только интерфейс — детерминистических весов/словарей/счётчиков/`TryDecide*`/KNN/regex в самих контрактах НЕТ (нечего классифицировать как скрытый детерминизм на этом уровне).

Граничный вывод deterministic-vs-learned для 00.Core: на уровне контрактов скрытых детерминистических механизмов нет; вопрос авторитета (trained unit против детерминистического хелпера) переносится в реализации (18/21/15) и в будущие edge-контракты графа.

## Пробелы / что доисследовать

- КРИТИЧНО: имён `ILearningDynamicGraph`, `ILearningUnit`, `ILearningFlow`, `ILearningOrchestrator` (из названия задачи) в коде НЕТ — ни в `00.Core`, ни в основном дереве (Grep по `*.cs`). Существует только пара `ILearningMachine` / `ILogicalOrchestrator` / `ILogicalFlow` / `ILogicalUnit`. План должен либо ввести новые контракты, либо переиспользовать существующие — это архитектурное решение, не факт кода.
- `ILogicalCapabilityTrainer.TrainAndRegisterAsync` (на него ссылается Level 5) живёт в `21.AgentInfrastructure/Logical/LogicalCapabilityTrainingContracts.cs` и `18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs`, НЕ в 00.Core. Реальный механизм обучения/DPO/runner надо исследовать там (вне scope этого рекона).
- DynamicGraph-контракты (узлы, рёбра, multi-output, fan-in/out, async, владелец авторитета на ребре) — отсутствуют; нужны для соответствия Graph-Style Gate.
- Teacher/legal-provenance поля отсутствуют на уровне контрактов; для «dpo legal build» нужно решить, где их хранить (батч/строка/манифест).
- Реальная реализация `ILearningMachine<TDomain>` (есть ли конкретный класс домена, например TranslationDomain, и кто его регистрирует/локатит через 15/25) — не проверялась здесь.
- Папки `.claude/worktrees/*` содержат зеркальные копии этих файлов (evidence-only); канонические — в основном дереве `C:/Projects/llm_mvp_core/00.Core/`.
- Факт-чтение (конец файлов): `LearningMachineContracts.cs` заканчивается методом `PullGlobalToPersonalAsync` пары (стр. 260); `LogicalContracts.cs` заканчивается интерфейсом `ITrustScorer.Score` (стр. 226–229); `OperationResult.cs` заканчивается `MapError<TNew>()` (стр. 19–20).
