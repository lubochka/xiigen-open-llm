# Codex Knowledge Library — корпус `llm_mvp_core` для проекта ILearningDynamicGraph

> Назначение: индекс-библиотека по 40 рекон-отчётам корпуса `llm_mvp_core`. Используется как карта evidence при написании ПЛАНА проекта **ILearningDynamicGraph** (авто-построение DPO для ILearningUnit/ILearningFlow/ILearningOrchestrator/ILearningDynamicGraph). Это сбор evidence, не разрешение на реализацию.
>
> Источник: рекон-файлы `codex-recon-core-*.md` в `C:/Users/lubao/Documents/xiigen/llm mvp/dpo legal build/`.
> Канон кода: `C:/Projects/llm_mvp_core`.
> Всего проиндексировано рекон-отчётов: **40**.

---

## 1. Краткое резюме корпуса для проекта ILearningDynamicGraph

**Главный архитектурный факт (повторяется в 8+ реконах, BLOCKING для плана):** имён `ILearningUnit`, `ILearningFlow`, `ILearningOrchestrator`, `ILearningDynamicGraph` из названия задачи в коде **НЕ существует**. Реально есть только пятиуровневая иерархия в `00.Core`: `ILearningMachine` (Level 5) → `ILogicalOrchestrator` (Level 4) → `ILogicalFlow` (Level 3) → `ILogicalUnit` (Level 2) → `ILearningMachinePair` (Level 1). `DECISIONS-LOCKED.md` (A-LM-12) фиксирует ЗАКРЫТЫЙ набор допустимых обучаемых уровней — добавление DynamicGraph шестым уровнем требует пересмотра решения/одобрения Luba. Готовый план-прецедент `claude-ILEARNINGDYNAMICGRAPH-DPO-PLAN-v1.md` (read-only evidence, claude-*) уже трактует DynamicGraph как `ILearningMachine<DynamicGraphDomain>` с orchestrator/flow/unit внутри — это рекомендуемый путь.

**Что в корпусе УЖЕ работает как обучаемый уровень (прообразы узлов графа):**
- **27.TranslationDomain** — самый зрелый `ILearningMachine` с полным lifecycle: DPO-пары (chosen/rejected/[WRONG:]), KNN-стор, атомарный JSON-чекпойнт save/load, held-out раннер (product/exact/bootstrap), promotionDecision PROMOTE/SKIP с novelty-gate.
- **43.SmartSkillsDomain** — 11 типизированных обучаемых `ILogicalUnit` под `ILearningMachine<SkillSelectionDomain>` (argmax по обученным per-skill weight-профилям, KNN = support_evidence_only), есть CandidateOrderLeakageGuardUnit как образец анти-leakage.
- **42.ProjectWikiDomain (OrchestrationPlanMachine)** + 6 Bootstrap-`ILogicalUnit` — объявленная стартовая точка DPO (A-LM-03).
- **18.CapabilityTraining** — референс-пайплайн авто-DPO: из knowledge edge строится preference-ряд → LoRA+DPO → embedding+HNSW юнит + retry-flow → реестр + checkpoint/manifest.

**Где живёт механика DPO/обучения (стек tooling):** `11.DPO` (DpoTrainer/DpoDataset/DpoPair, настоящий reference-vs-policy лосс + AdamW), `10.LoRA` (LoraApplicator, parameter-efficient fine-tuning), `04.TrainingLoop` (pre-training трансформера, не DPO), `14.ContinuousLearning` (онлайн-петля, но IncrementalTrainer — заглушка), `16.AutonomousLearning` (8-шаговый self-improvement цикл, DPO-канал проложен но вызывается пустым []), `26.TrainableRouter` (детерминированный keyword-скоринг под именем "trainable" — НЕ обучается).

**Lifecycle/публикация:** `25.ModelServer` (IModelRepository.UploadAsync → SHA-256 + RemoteUnitManifest, IRemoteUnitLocator in-memory, загрузка весов только из локального файла; IModelServer объявлен но не реализован). Чекпойнт-тип `GeneratedCapabilityCheckpointManifest` определён в `18.CapabilityTraining`. Реальные ассеты: `C:/Projects/llm_mvp_core/models/` (index.json + 5 моделей: manifest+checkpoint+runner-report).

**Граф-субстрат:** `07.GraphEngine` (KnowledgeGraph: GraphNode/GraphEdge, out/in смежность, PageRank/Personalised PageRank, JSON-persist — всё детерминированное), `08.GraphRag` (донор паттерна fan-out local/global → fan-in merge), `17.DynamicDomains` (DynamicDomainGraph: транзитивное замыкание + мостовые узлы; реальная динамика «на лету» — в ILogicalTopologyRegistry, мутируемом Add*TopologyMutationUnit, проецируется в граф через AdaptiveLogicalOrchestrator).

**Численный субстрат (не уровни, internal detail):** `00.TensorCore` (тензоры+автодиф, но без оптимизатора/чекпойнта), `33.MatrixAlgorithms` (LU/QR/SVD, sparse, SVD-сжатие весов, квантование), `05.Embeddings` (text→вектор через transformer+mean-pooling), `06.VectorSearch` (top-K kNN, BruteForce+HNSW).

**Сквозные пробелы (критичны для legal DPO build):**
1. **Teacher-provenance полей НЕТ нигде** — ни в DPO-контрактах (LearningTrainingItem/LearningBatch), ни в манифестах (есть только dataset-provenance: SourceDatasetSha256/SourceIds). Для Legal Teacher Gate нужны `teacher_access_mode/hosting_provider/model_id/model_license/provider_terms_checked/closed_proprietary_model`.
2. **Графовых edge-контрактов НЕТ** — нет узлов/рёбер с fan-out/fan-in/many-to-many/per-edge authority и async-активацией. Всё соединение юнитов сейчас — ручная последовательная композиция внутри ILogicalFlow.
3. **Скрытый детерминизм под именем "trainable"** — 26.TrainableRouter (keyword-скоринг), counter-веса (`_candidatePositionWeights`, Dictionary<string,int>+1 вместо нормированного reward-margin) — риск FORBIDDEN_RUNTIME_SHORTCUT, требуют классификации/обёртки.

---

## 2. Таблицы по темам

Столбцы: **Файл рекона** (имя `codex-recon-*.md`) | **Заголовок** | **Краткое** | **Релевантность**.
Путь ко всем файлам: `C:/Users/lubao/Documents/xiigen/llm mvp/dpo legal build/<имя>`.

### 2.1. Контракты / иерархия ILearningMachine

| Файл рекона | Заголовок | Краткое | Релевантность | source_category | provenance_status | teacher_access_mode | legal_verdict | source_hash_or_ref | implementation_use | gap_owner |
|---|---|---|---|---|---|---|---|---|---|---|
| codex-recon-core-00-contracts.md | 00.Core — контракты ILearningMachine (5 уровней) | Все 5 уровней как чистые контракты: Level5 ILearningMachine + Level1 ILearningMachinePair в LearningMachineContracts.cs; Level4/3/2 в LogicalContracts.cs. A-LM-02 (Train* только на ILearningMachine) подтверждён. Имён ILearning(Unit/Flow/Orchestrator/DynamicGraph) НЕТ. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| codex-recon-core-00-rest.md | 00.Core — остальное (OperationResult, флаги/конверты Logical, ITrustScorer) | 00.Core = ровно 3 файла. BaseService/реестры/локаторы НЕ в 00.Core (в 21/25). В контрактах обучения нет teacher-provenance; одиночный LogicalFlowResult+массив TransitionSignals — решить, хватит ли для fan-out/fan-in. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| codex-recon-core-21-infra.md | 21.AgentInfrastructure — инфраструктурные контракты | Базовые контракты для новых юнитов: OperationResult, BaseService(ISessionStorage), fabric-интерфейсы, канонические уровни + DPO-типы + реестр юнитов (trust-state/IsLearned) + мост ILogicalCapabilityTrainer. Без нарушений; риски на уровне реализаций. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| codex-recon-core-docs-arch.md | ARCHITECTURE.md иерархия (§10) + диаграмма (§1) | Иерархия в §10 (не §9 — §9=BaseService), подтверждена 00.Core. Train только на ILearningMachine. Gap: нет ILearning*/DynamicGraph контрактов и teacher-provenance. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| codex-recon-core-docs-interfaces.md | COMPONENT-INTERFACES.md — справочник интерфейсов | RAG-справочник 5-уровневой иерархии + 6 bootstrap-юнитов + обученные машины (Orchestration/Translation/SkillSelection) с checkpoint/export/eval-gate. Контрактов ILearningDynamicGraph нет — главный пробел. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| codex-recon-core-docs-decisions.md | Locked Decisions (D-01..D-17, A-LM-01..A-LM-13) | A-LM-12 = ЗАКРЫТЫЙ набор допустимых уровней; DynamicGraph не зафиксирован как уровень → возможен пересмотр/одобрение Luba. A-LM-03 (bootstrap=DPO-старт), D-16 (exact-prompt leakage=0). | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| codex-recon-core-docs-submap.md | SUBPROJECT-MAP — статусы по обучению (Done/stub) | Реальные обучаемые: 27 (зрелый), 42 OrchestrationPlanMachine, 43 SkillSelectionMachine. Скелеты: OrchestrationReasoningMachine, PromptToTaskMachine. Имён ILearning*/DynamicGraph нет. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |

### 2.2. DPO-построение

| Файл рекона | Заголовок | Краткое | Релевантность | source_category | provenance_status | teacher_access_mode | legal_verdict | source_hash_or_ref | implementation_use | gap_owner |
|---|---|---|---|---|---|---|---|---|---|---|
| codex-recon-core-11-dpo.md | 11.DPO — построение и использование DPO | Низкоуровневая training-infra: DpoPair/DpoBatch/DpoMetrics, JSONL-загрузчик DpoDataset, движок DpoTrainer (reference-vs-policy лосс, ручной backward, AdamW). Пару строит внешний код; provenance/teacher не хранит; потребляется только из Tests. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| codex-recon-core-18-capability.md | 18.CapabilityTraining — пайплайн авто-DPO способностей | Референс авто-DPO: knowledge edge → preference-ряд → LoRA+DPO → embedding+HNSW юнит + retry-flow → реестр trust=Probationary + manifest/checkpoint. Прямой прообраз авто-построения DPO по узлам; без edge-контрактов/held-out/ablation/export/teacher. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| codex-recon-core-26-router.md | 26.TrainableRouter — "trainable" = детерминированный keyword-скоринг | Рантайм детерминирован: KeywordUnitScorer, RoutingProfileTrainer (только словари), фазы по 10 keyword-правилам. RewardMargin/DpoAccuracy/Promoted — плейсхолдеры; chosen генерится self-label-шаблоном без teacher. Риск hidden-shortcut. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| codex-recon-core-docs-skills-dpo.md | Релевантные DPO/trainable/graph скилы | Нет отдельного graph-DPO скила: graph-разбор в trainable-architecture-decision-SKILL (Graph/DynamicGraph Reconstruction Gate + Legal Teacher Gate), форма тройки в dpo-triple-format-SKILL, решения-как-DPO в planning-dpo-authoring-SKILL, контур-пример в dcm-dpo-construction-GUIDE. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| codex-recon-core-docs-plans.md | docs/plans — релевантные планы для ILearningDynamicGraph+DPO | Целевой прецедент claude-ILEARNINGDYNAMICGRAPH-DPO-PLAN-v1 (DynamicGraph=ILearningMachine<DynamicGraphDomain>, Part A, source map, legal/provenance matrix) + 6 опорных планов (LEARNING-MACHINE-EVOLUTION, DPO-KNOWLEDGE-MAP, TRAINABLE-AUDIT-v4 и др.). claude-* = read-only. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| codex-recon-core-14-continuous.md | 14.ContinuousLearning — DpoPair и онлайн-обучение | FeedbackRouter: фидбек EDITED/REJECTED → DpoPair; ContinuousLearningPipeline → IncrementalTrainer по доменам. НО IncrementalTrainer.TrainAsync — заглушка; пары в памяти без persist; реальное применение LoRA/DPO делегировано в 16. | medium | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| codex-recon-core-16-autonomous.md | 16.AutonomousLearning — авто-DPO прецедент | 8-шаговый ночной self-improvement цикл LoRA (idle→baseline→data→replay 70/30→enhanced→promote при accuracy-дельте>0). DPO-канал проложен (AutonomousTrainer принимает IReadOnlyList<DpoPair>), но pipeline вызывает с пустым [] — авто-генерации chosen/rejected нет. | medium | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |

### 2.3. Обучение и continue-training

| Файл рекона | Заголовок | Краткое | Релевантность | source_category | provenance_status | teacher_access_mode | legal_verdict | source_hash_or_ref | implementation_use | gap_owner |
|---|---|---|---|---|---|---|---|---|---|---|
| codex-recon-core-10-lora.md | 10.LoRA — LoRA-адаптеры и их роль | Parameter-efficient FT поверх TensorCore (LoraConfig/LoraLinear/LoraApplicator): замораживает Linear + обучаемые A/B. Используется как implementation detail внутри LogicalDpoTrainingUnit (18). Wiring-пробел: Apply не делает SetValue и не меняет Forward — адаптеры возвращаются сбоку. Нет своего checkpoint/export lifecycle. | medium | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| codex-recon-core-04-trainloop.md | 04.TrainingLoop — pre-training LLM (не DPO) | Цикл pre-training трансформера (next-token CE) на TensorCore: Trainer/AdamW/CosineWithWarmup/TrainingConfig. Прямой связи с DPO/DynamicGraph нет; метрика только loss/lr в Console; чекпойнт только Save без resume. | low | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| codex-recon-core-14-ml.md | 14.MachineLearning — ML-примитивы и циклы | Автономная библиотека классических ML с нуля (LinearRegression/NaiveBayes/MLP/KMeans/Apriori/PCA/QLearning), изолирована от иерархии. Тренировочные циклы только в MLP (SGD+backprop) и QLearning (TD); без сериализации весов/checkpoint/DPO/метрик. | medium | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| codex-recon-core-27-translation.md | 27.TranslationDomain — образец обученного юнита | Рабочий референс ILearningMachine с полным lifecycle: DPO-пары по UnitId → ITranslationUnitTrainer → KNN-стор + атомарный JSON-чекпойнт (save/load без replay); раннер меряет product/exact/bootstrap на held-out с контролем контаминации; promotionDecision + novelty-gate. Прямой шаблон узлов/рёбер. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |

### 2.4. Чекпойнты / манифест / lifecycle

| Файл рекона | Заголовок | Краткое | Релевантность | source_category | provenance_status | teacher_access_mode | legal_verdict | source_hash_or_ref | implementation_use | gap_owner |
|---|---|---|---|---|---|---|---|---|---|---|
| codex-recon-core-25-modelserver.md | 25.ModelServer — lifecycle модели (IModelRepository/манифест/локатор) | Тонкий слой публикации/локации: export через IModelRepository.UploadAsync (LocalModelRepository: файлы+SHA-256+RemoteUnitManifest), регистрация/поиск через IRemoteUnitLocator (in-memory), загрузка весов только из локального файла. IModelServer не реализован. Чекпойнт-тип GeneratedCapabilityCheckpointManifest из 18. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| codex-recon-core-assets.md | Core Assets (чекпойнты/манифесты/датасеты/отчёты) | models/index.json + 5 моделей (manifest+checkpoint+runner-report); training-data/ (corpus.jsonl.gz+pointer+manifest+smart-skills *-dpo.jsonl); artifacts/ (schema/finalization/readiness/sweep/continue/host-boundary). Готовый прототип lifecycle, честно semantic_quality_claimed=false (scaffold). | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| codex-recon-core-24-compress.md | 24.Compression — сжатие в lifecycle ассетов | Детерминированные байтовые кодеки (LZ4/Zstd/Huffman/rANS/BWT/LZMA/Brotli/OpenZL), контракт ICompressor. Релевантна как инфраструктура сжатия чекпойнтов/DPO-наборов/манифестов при export/import/share + метрика размера (CompressionRatio). Локальный отдельный OperationResult — нужен alias на стыке. | low | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| codex-recon-core-33-matrix.md | 33.MatrixAlgorithms — матричный бэкенд | Детерминированная линейная алгебра (LU/QR/SVD, sparse COO/CSR/CSC, SVD-сжатие весов, int8-квантование), 12 алгоритмов через диспетчер. Релевантна как бэкенд: хранение/сжатие весов узла, разреженная матрица смежности графа, линейные шаги апдейта. Не уровень. | medium | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |

### 2.5. Измерение скорости / точности / качества

| Файл рекона | Заголовок | Краткое | Релевантность | source_category | provenance_status | teacher_access_mode | legal_verdict | source_hash_or_ref | implementation_use | gap_owner |
|---|---|---|---|---|---|---|---|---|---|---|
| codex-recon-core-tests.md | Core/Tests — тестирование обучаемых юнитов и DPO | Честность необученного: skeleton-паттерн (NotTrainedStatus + NO_SEMANTIC_CLASSIFICATION_PERFORMED + рефлексия «нет Dictionary/Regex/Classifier/Lookup»). Приёмка обученного: held-out source-disjoint accuracy/PassesGate/RewardMargin + ablation (exact_prompt/path/title/source-id leakage/nearest-neighbor/no_trained_state/shuffled) + lifecycle export/import/fresh-load. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| codex-recon-core-docs-exp.md | Эксперименты E01-E50 — метрики, SCALE_UP, DPO/flow evidence | 59 прогонов (30 PASSED/5 SCALE_UP/27 FAILED) с дельтами; result-schema-v1.json = 5 метрик + verdict/next_action; experiments-v2-runners.md (DPO/KNN/teaching-runner + anti-determinism). Ближайший прообраз — DYNAMIC-FLOW-MINI-E2E (E51B): «KNN=память, арбитры=авторитет» → authority-per-edge. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| codex-recon-core-docs-skills-catalog.md | Каталог скилов — DPO/обучение/измерение/review/integrity/completion | ~200 файлов, навигатор HOW-TO-USE-SKILLS.md (v27.5). 6 кластеров: DPO, измерение качества (capability-measurement+improvement-measurement), generated-code-review (5 слоёв+Layer5A+Deterministic-Boundary Gate), implementation-integrity (13 Check-ов), phase-completion-packager (trainable-phase gate/two-PASS), trainable-architecture-decision (Graph-Style Gate + Legal Teacher Gate). | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |

### 2.6. Граф (GraphEngine / GraphRag / DynamicDomains)

| Файл рекона | Заголовок | Краткое | Релевантность | source_category | provenance_status | teacher_access_mode | legal_verdict | source_hash_or_ref | implementation_use | gap_owner |
|---|---|---|---|---|---|---|---|---|---|---|
| codex-recon-core-07-graph.md | 07.GraphEngine — детерминированный граф кода + PageRank | Ориентированный взвешенный граф (KnowledgeGraph: GraphNode/GraphEdge, out/in смежность) + power-iteration PageRank + Personalised PageRank + атомарный JSON-persist. Переиспользуемый скелет узлов/рёбер/смежности/retrieval/persist, но финальная власть выбора должна уйти в обучаемый уровень. | medium | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| codex-recon-core-08-graphrag.md | 08.GraphRag — Graph RAG (извлечение/композиция) | Retrieval-конвейер поверх 07/06/05: Indexer→векторный индекс из KnowledgeGraph; LocalSearch (эмбеддинг-KNN+статический PageRank); GlobalSearch (PPR-экспансия от seed); HybridRetriever (слияние 0.6/0.4 + бюджет токенов). Донор паттерна fan-out(local/global)→fan-in(merge); retrieval/memory ВНУТРИ уровня, не граф решений. | medium | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| codex-recon-core-17-dynamic.md | 17.DynamicDomains — прецедент динамической структуры | DynamicDomainGraph.cs: транзитивное замыкание (DomainGraphClosure) + мостовые узлы (OrchestratorExpander) над DynamicDomainTransition. Динамика «на лету» — в ILogicalTopologyRegistry (мутируется Add*TopologyMutationUnit), AdaptiveLogicalOrchestrator проецирует рёбра и пересчитывает порядок/мосты. E49/E50 дублируют Jaccard — кандидат REWRITE_TO_DPO. | medium | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |

### 2.7. Оркестрация и async-комбинации

| Файл рекона | Заголовок | Краткое | Релевантность | source_category | provenance_status | teacher_access_mode | legal_verdict | source_hash_or_ref | implementation_use | gap_owner |
|---|---|---|---|---|---|---|---|---|---|---|
| codex-recon-core-15-logical.md | 15.OrchestratorModel/Logical — регистрация flow-адаптеров и соединение юнитов | Flow-адаптеры регистрируются императивно в LogicalOrchestratorAssistant.CreateDefault через InMemoryLogicalFlowRegistry.Register/RegisterFactory (ключи FlowId/ScenarioId/SubPhaseId+InputType, latest-wins); юниты — в InMemoryLogicalUnitRegistry; AdaptiveLogicalOrchestrator связывает flow+unit+topology. Юниты соединяются НЕ декларативным графом, а ручной композицией внутри ILogicalFlow. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| codex-recon-core-15-rest.md | 15.OrchestratorModel — async input→output и контракты RunAsync | Два async-слоя: тонкие обёртки над иерархией (LogicalOrchestratorAssistant.RunAsync→ILogicalOrchestrator; ProjectWikiOrchestrationFlow.ExecuteAsync) и legacy OrchestratorFacade.ProcessAsync. Контракты RunAsync в 00.Core; движок AdaptiveLogicalOrchestrator в 21. ProjectWikiRequestRouterUnit — детерминированный baseline (isLearned:false). | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| codex-recon-core-42-wiki.md | 42.ProjectWikiDomain — 6 Bootstrap ILogicalUnit + flow-адаптеры (старт DPO) | 6 Bootstrap-юнитов (IIntentBuilderUnit..IOrchestrationVerifierUnit ⊂ ILogicalUnit), 5/6 — тонкие обёртки над статикой, 6-й несёт regex. TaskOrchestrationUnit.OrchestrateAsync секвенсит линейный DAG из 6 шагов, оставляя слот под trained-замену из 25. Flow-адаптеры честно отдают runner_missing/UNKNOWN. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| codex-recon-core-43-localwiki.md | 43.LearningLocalWikiData — данные/реестр/раннер | Слой данных/реестра/отчётов/раннера для project-local wiki (юниты/DPO-генераторы/оркестратор — в 42). Хранит DPO-jsonl ([WRONG:]), checkpoints policy_final.bin, реестр статусов батчей (accepted=19/rejected=182/experimental=193/blocked=26), CLI-раннер ProjectWikiTrainingRunner (реальный DPO-цикл: loss/reward_margin/held-out gate + export/import/locator/upgrade/rollback). 13 canonical units = конвейер-граф; WikiTraceAggregator+NEXT-TRAINING-ACTION = петля выбора слабого узла. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| codex-recon-core-09-inf.md | 09.Inference — авторегрессивная генерация токенов | TextGenerator (prefill+decode вокруг LlmModel.Forward), Sampler (greedy/temp/top-k/top-p), KvCache (не подключён). Ссылается только тест. Честный исполнитель инференса. Риск: Sampler — статический Random без seed → недетерминизм для воспроизводимых DPO-прогонов. | low | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |

### 2.8. Teacher / provenance / легальный гейт

| Файл рекона | Заголовок | Краткое | Релевантность | source_category | provenance_status | teacher_access_mode | legal_verdict | source_hash_or_ref | implementation_use | gap_owner |
|---|---|---|---|---|---|---|---|---|---|---|
| codex-recon-core-docs-decisions.md | Locked Decisions — A-LM-* анти-pseudo-trainable | Teacher-provenance не в этом файле (см. HOW-TO-USE-SKILLS ~стр.330 + trainable-architecture-decision-SKILL). D-16: exact-prompt leakage=0; A-LM-08: anonymized sync. Common-vs-adaptive placement не зафиксирован (прецедент 43 в A-LM-10/11). | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| codex-recon-core-docs-skills-dpo.md | DPO/trainable/graph скилы — provenance-гейт | Legal Teacher/Provenance Gate (Claude/OpenAI/Gemini запрещены как teacher) в trainable-architecture-decision-SKILL. Имён ILearning*/DynamicGraph как контрактов нет. Нет реестра одобренных hosted open-weight teacher-источников. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| codex-recon-core-assets.md | Core Assets — provenance в манифестах | В манифестах НЕТ teacher-provenance (teacher_access_mode/hosting_provider/model_id/model_license/provider_terms_checked) — только косвенно external_ai_used=false. dataset-provenance: SourceDatasetSha256/TrainingBranch/TrainingRunId. Тела corpus.jsonl(.gz) не прочитаны — риск target-label/source-path leakage. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| codex-recon-core-27-translation.md | 27.TranslationDomain — source-provenance прецедент | tatoeba-pilot-attribution-manifest.json (Tatoeba CC-BY) — образец source-license, нужно сверить с teacher/source-license gate. RewardMargin фиксирован (0.03/0.0) = метка наличия [WRONG:], не вычисленный DPO-сигнал. _exactSourceMap требует ablation на leakage. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| codex-recon-core-22-algos.md | 22.AgentAlgorithms — LLM-judge → teacher/provenance | Детерминированные KNN/Jaccard/BM25 хелперы (SkillRetriever/HybridRanker/AstRelevanceRanker/PredicateDatasetRouter/RouteClassifier) — НЕ уровни, baseline/candidate-retrievers. Comparison/LlmJudge.cs + комментарий «DPO-ready learning signal» в InternalFirstAiSupport — проверить на маскировку обучения/teacher. | medium | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |

### 2.9. Скилы и гайды

| Файл рекона | Заголовок | Краткое | Релевантность | source_category | provenance_status | teacher_access_mode | legal_verdict | source_hash_or_ref | implementation_use | gap_owner |
|---|---|---|---|---|---|---|---|---|---|---|
| codex-recon-core-docs-skills-catalog.md | Каталог скилов (core/docs) | Навигатор HOW-TO-USE-SKILLS.md (v27.5) + Skill index + таблицы триггеров. 6 кластеров: DPO, измерение, generated-code-review (5 слоёв+Deterministic-Boundary Gate), implementation-integrity (13 Check), phase-completion-packager (trainable-phase/two-PASS), trainable-architecture-decision (Graph-Style + Legal Teacher Gate). | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| codex-recon-core-docs-skills-dpo.md | DPO/trainable/graph скилы для плана | trainable-architecture-decision-SKILL (Graph/DynamicGraph Reconstruction Gate), dpo-triple-format-SKILL (форма тройки), planning-dpo-authoring-SKILL (решения-как-DPO), dcm-dpo-construction-GUIDE + design-conversion-dpo-authoring-SKILL (контур ILearningMachinePair→Orch→Flow→Unit). | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| codex-recon-core-docs-plans.md | docs/plans — опорные планы | claude-ILEARNINGDYNAMICGRAPH-DPO-PLAN-v1 (целевой прецедент, read-only), ARCHITECT-PLAN-LEARNING-MACHINE-EVOLUTION-v1, DPO-KNOWLEDGE-MAP-v1, TRAINABLE-ARCHITECTURE-AUDIT-v4, SKILL-SELECTION-DPO-NEGATIVE-STRATEGY-v1, ORCHESTRATION-*-FULL-EXTENSIONS-v3, ORCHESTRATION-MACHINE-TRAINING-v20. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |

### 2.10. Ассеты / данные / тесты

| Файл рекона | Заголовок | Краткое | Релевантность | source_category | provenance_status | teacher_access_mode | legal_verdict | source_hash_or_ref | implementation_use | gap_owner |
|---|---|---|---|---|---|---|---|---|---|---|
| codex-recon-core-assets.md | Core Assets — чекпойнты/манифесты/датасеты/отчёты | models/ (index.json+5 моделей), training-data/ (corpus.jsonl.gz+pointer+manifest+smart-skills *-dpo.jsonl), artifacts/ (DPO-schema/readiness/sweep/continue/host-boundary). checkpoint с flow_grounding_knn (обучаемые рёбра маршрутизации), semantic_domain_matrix на 11 юнитов, честный scaffold-статус. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| codex-recon-core-tests.md | Core/Tests — честность untrained + приёмка trained | Skeleton-тесты честности + три gate приёмки (held-out/ablation/lifecycle). Машины PromptToTask/OrchestrationReasoning/SkillSelection = ILearningMachine<TDomain>. Не покрыто: DynamicGraph рёбра/fan-out, генератор DPO-троек, legal-teacher provenance. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| codex-recon-core-00-tensor.md | 00.TensorCore — API тензоров и many→many | Чистая C# тензор-библиотека: TensorShape/Storage/Tensor + автодиф; many→many через TensorOps.MatMul + Linear.Forward поверх SimdOps.MatVecRow. Обучаемость = автодиф (Backward/Parameter/Module). Нет оптимизатора/чекпойнта/графа/DPO. Internal detail / substrate. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| codex-recon-core-05-emb.md | 05.Embeddings — построение векторов входов | 2 файла (EmbeddingModel+MeanPooling): текст → L2-вектор через LlmModel.ForwardHidden+mean-pooling по mask. Вызывается TransformerEmbeddingKnnLogicalUnit (18) и Indexer (08). OK_MODEL_COMPONENT — источник признаков внутри ILogicalUnit. Риски: GetHashCode-коллизии, недетерминизм, maxLen 512 vs 64. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| codex-recon-core-06-vsearch.md | 06.VectorSearch — векторный поиск (top-K kNN) | IVectorIndex + BruteForceIndex/HnswIndex + Distances. Search возвращает материализованный List (не IQueryable), строго один→много (нет batch/many→many fan-out). Internal детерминированный ретривер / KNN-память. Нарушения правил: не OperationResult, Save/Load=NotImplemented, мёртвые метрики. | low | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| codex-recon-core-28-nlp.md | 28.SemanticNlp — семантические NLP-юниты | 5 классических аннотаторов (FrameNet/SRL/WordNet/AMR/UD) под ISemanticAnnotator + SemanticNlpDispatcher. Всё детерминированное (словари/счётчики/эвристики/парсеры), без обучения/DPO/checkpoint. Источник признаков/разметки для узлов, но не обучаемые узлы. | medium | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| codex-recon-core-29-sc.md | 29.SemanticCompiler — что компилирует и как | Сжимает текст промпта/кода в логические предикаты + домен (D-04) + язык стека + коэффициент через фиксированную 6-стадийную цепочку. Всё детерминированное; RouteClassifier.Train только in-memory без checkpoint. Любая claim «обучаемый семантический узел» = слишком сильная до обёртки. | medium | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |
| codex-recon-core-43-smartskills.md | 43.SmartSkillsDomain — юниты выбора скилов | 11 типизированных обучаемых ILogicalUnit (argmax по обученным per-skill weight, KNN=support_evidence_only) под ILearningMachine<SkillSelectionDomain> с полным lifecycle. CandidateOrderLeakageGuardUnit (CanSelectSkill=false) — образец анти-leakage. Зрелый прототип, но веса = counter Dictionary<string,int>+1, без edge-контрактов/teacher-полей. | high | source_inventory | teacher_provenance_missing | UNKNOWN | evidence_only_not_train_ready | recon_file_ref | planning_reference | legal_gate_plan |

> Примечание о покрытии тем: каждый рекон-файл закреплён за своей ведущей темой. Несколько кросс-темных файлов (codex-recon-core-00-rest, -docs-decisions, -assets, -27-translation, -tests) дополнительно отражены в смежных таблицах (контракты, teacher/provenance, ассеты), чтобы план находил их по любой из релевантных осей. Все 40 входных рекона учтены (см. §4 для проверки полноты).

---

## 3. Самое релевантное к ILearningDynamicGraph

Приоритетный короткий список — что открыть первым при написании плана.

**Контракты-фундамент (решить: новый уровень vs ILearningMachine<DynamicGraphDomain>):**
- `codex-recon-core-00-contracts.md` — 5 уровней + A-LM-02; подтверждает отсутствие имён ILearning*.
- `codex-recon-core-docs-decisions.md` — A-LM-12 закрытый набор уровней → DynamicGraph требует решения/одобрения Luba.
- `codex-recon-core-21-infra.md` — где физически объявлять новые юниты (OperationResult/BaseService/реестр/ILogicalCapabilityTrainer).
- `codex-recon-core-docs-plans.md` — готовый прецедент claude-ILEARNINGDYNAMICGRAPH-DPO-PLAN-v1 (read-only образец трактовки DynamicGraph=ILearningMachine<DynamicGraphDomain>).

**Прообразы обучаемых узлов (копировать lifecycle, не изобретать):**
- `codex-recon-core-27-translation.md` — самый полный образец обученного ILearningMachine (DPO/checkpoint/held-out/promotion/novelty-gate).
- `codex-recon-core-43-smartskills.md` — 11 обучаемых ILogicalUnit + анти-leakage guard (CandidateOrderLeakageGuardUnit).
- `codex-recon-core-18-capability.md` — референс авто-построения DPO из knowledge edge + LoRA+DPO + embedding/HNSW юнит + реестр.
- `codex-recon-core-42-wiki.md` + `codex-recon-core-43-localwiki.md` — 6 Bootstrap-юнитов (старт DPO, A-LM-03) + реальный CLI-раннер с held-out gate + петля выбора слабого узла.

**Граф-субстрат и async-композиция (узлы/рёбра/fan-out/fan-in/authority-per-edge):**
- `codex-recon-core-07-graph.md` — скелет узлов/рёбер/смежности + PageRank + persist.
- `codex-recon-core-08-graphrag.md` — донор паттерна fan-out(local/global)→fan-in(merge).
- `codex-recon-core-17-dynamic.md` — прецедент динамики «на лету» (ILogicalTopologyRegistry + Add*TopologyMutationUnit + AdaptiveLogicalOrchestrator).
- `codex-recon-core-15-logical.md` + `codex-recon-core-15-rest.md` — как сейчас регистрируются flow-адаптеры и соединяются юниты (ручная композиция, не декларативный граф); движок AdaptiveLogicalOrchestrator в 21.

**DPO-механика и измерение (что переиспользовать как tooling внутри уровня):**
- `codex-recon-core-11-dpo.md` — DpoTrainer/DpoDataset/DpoPair (настоящий лосс).
- `codex-recon-core-10-lora.md` — LoRA adapter-tier (с известным wiring-пробелом).
- `codex-recon-core-tests.md` — паттерны честности untrained (skeleton) + три gate приёмки trained (held-out/ablation/lifecycle).
- `codex-recon-core-docs-exp.md` — result-schema-v1 (5 метрик) + правило «KNN=память, арбитры=авторитет» (E51B) для authority-per-edge.

**Legal/provenance (обязательно для «dpo legal build»):**
- `codex-recon-core-docs-skills-dpo.md` + `codex-recon-core-docs-skills-catalog.md` — Legal Teacher/Provenance Gate + Graph-Style Reconstruction Gate (правила классификации).
- `codex-recon-core-assets.md` — текущие манифесты БЕЗ teacher-provenance (что добавить).
- `codex-recon-core-25-modelserver.md` — куда добавить teacher-поля в RemoteUnitManifest + IModelRepository/IRemoteUnitLocator.

**Lifecycle/публикация:**
- `codex-recon-core-25-modelserver.md` — export/import/locator (IModelServer не реализован).
- `codex-recon-core-assets.md` — реальные models/index.json + training-data/ + artifacts/.

---

## 4. Сводные пробелы

Объединённые пробелы из всех 40 реконов, сгруппированные по риску для плана ILearningDynamicGraph.

### 4.1. BLOCKING — архитектурные решения до старта плана
- **Контрактов ILearningUnit/ILearningFlow/ILearningOrchestrator/ILearningDynamicGraph НЕ существует** (00-contracts, 21-infra, docs-arch, docs-interfaces, docs-decisions, docs-submap, docs-skills-dpo). Есть только ILogical*/ILearningMachine*. A-LM-12 — ЗАКРЫТЫЙ набор уровней. План обязан явно выбрать: (а) DynamicGraph = `ILearningMachine<DynamicGraphDomain>` с orchestrator/flow/unit внутри (путь claude-плана-прецедента), либо (б) ввести новый уровень с одобрением Luba и пересмотром A-LM-12. Без этого решения весь план повисает.
- **Graph edge-контрактов нет** (00-rest, 07-graph, 08-graphrag, 17-dynamic, 15-logical, 18-capability, 25-modelserver, 43-smartskills): нет типизированных узлов/рёбер с fan-out/fan-in/many-to-many, per-edge authority (deterministic-vs-trainable), async-активацией. Текущее соединение — ручная последовательная композиция внутри ILogicalFlow + проекция ILogicalTopologyRegistry в граф 17. Нужен новый контракт рёбер; решить — расширять GraphNode/GraphEdge (07) или вводить отдельные типизированные обучаемые записи состояния поверх контейнера.

### 4.2. BLOCKING для «legal build» — teacher/provenance
- **Полей teacher-provenance нет нигде** (00-contracts, 00-rest, 14-continuous, 16-autonomous, 25-modelserver, 26-router, 27-translation, assets, docs-arch, docs-decisions, docs-exp, docs-skills-dpo): отсутствуют `teacher_access_mode/hosting_provider/model_id/model_license/provider_terms_checked/teacher_output_used_for_training/closed_proprietary_model` и в DPO-контрактах (LearningTrainingItem/LearningBatch), и в манифестах (есть только dataset-provenance: SourceDatasetSha256/SourceIds/TrainingBranch/TrainingRunId). План должен решить, где их хранить (строка DPO / батч / манифест) и добавить.
- **Реестра одобренных hosted open-weight teacher-источников нет** (docs-skills-dpo, docs-exp): Claude/OpenAI/Gemini запрещены как teacher; E51B запрещает внешнего поставщика в v1. Источники chosen/rejected сейчас — синтетический self-label/seed (26-router, 18, 43-localwiki) или правка живого пользователя (14 — адаптивная нога). План обязан определить происхождение chosen/rejected отдельно.
- **Лицензии источников не сверены** (27-translation: Tatoeba CC-BY в attribution-manifest; 16: SourceEntry без лицензии/URL; assets: тела corpus.jsonl(.gz) не прочитаны → риск target-label/source-path leakage, source_path `training-bundle://` в чекпойнте).

### 4.3. HIGH — скрытый детерминизм под видом trainable (R4 / FORBIDDEN_RUNTIME_SHORTCUT)
- **26.TrainableRouter** (26-router): весь рантайм-путь детерминирован (KeywordUnitScorer); RewardMargin/DpoAccuracy/Promoted — плейсхолдеры; обучения/чекпойнтов нет. Кандидат REWRITE_TO_DPO или честная пометка eval-only.
- **Counter-веса вместо reward-margin** (43-smartskills, docs-interfaces): веса узлов = Dictionary<string,int>+1, а не нормированный preference-margin; `_candidatePositionWeights` (Dictionary<int,int>) — потенциальная leakage-authority (обёрнута в guard в 43, но проверить).
- **Детерминированные роутеры на ребре** (15-logical, 15-rest, 42, 22): ProjectWikiRequestRouterUnit/SubPhaseRouterUnit/PredicateDatasetRouter/ITrustScorer/ResolveLatest(tie-break по RegistrationOrder) — если станут финальной маршрутной авторитетностью без обёртки уровнем → FORBIDDEN_RUNTIME_SHORTCUT. Классифицировать каждый: baseline/eval-only vs обучаемый.
- **E49/E50 дублируют Jaccard** (17-dynamic) не используя классы 17 — решить: единый обучаемый предиктор рёбер vs две детерминированные копии.

### 4.4. HIGH — незавершённый/неподключённый lifecycle обучения
- **IncrementalTrainer.TrainAsync — заглушка** (14-continuous): DPO-пары в памяти без persist/fresh-load; реальное применение LoRA/DPO делегировано в 16, но pipeline в 16 вызывает AutonomousTrainer с пустым [] (16-autonomous) — авто-генерации chosen/rejected нет.
- **LoRA wiring-пробел** (10-lora): LoraApplicator.Apply не делает field.SetValue и не меняет LlmModel.Forward — адаптеры возвращаются сбоку; кто реально оптимизирует lora_a/lora_b и где сохраняется adapter-file — не подтверждено.
- **IModelServer не реализован** (25-modelserver): передачи весов по сети нет, serverBaseUrl игнорируется, import только из локального файла. Два дублирующих реестра (InMemoryRemoteUnitLocator vs ModelEndpointRegistry). split common/adaptive (R5, ILearningMachinePair) на уровне 25 не выражен.
- **Боевого потребителя DPO/раннера вне Tests нет** (11-dpo, 04-trainloop): рантайм-пайплайн обучения через 11.DPO не подтверждён подключённым; вызывающая сторона (runner/Program) не найдена в области.
- **RewardMargin не вычисляется** (27-translation: фикс 0.03/0.0 = метка [WRONG:]; 26-router: плейсхолдер) — нужен настоящий DPO-сигнал.

### 4.5. MEDIUM — не дочитанный код / неподтверждённые потребители (нужны точечные доисследования для §0/§3 плана)
- **Реализации с риском shortcut не читались построчно** (21-infra): InMemoryLogicalFlowRegistry, LogicalTopologyRegistry/Commands, AdaptiveLogicalOrchestrator (реальный движок рёбер/fan-out/fan-in/authority — RunAsync:85 в 21), KnnLogicalUnit, *RouterUnit, PromptDomainClassifier, CompiledDomainClassifierUnit.
- **Ключевые для графа классы не классифицированы** (42): OrchestrationKnnStore.cs, OrchestrationCorpusBuilder.cs, FlowReferenceDetector.cs, OrchestrationPlanOrchestrator.cs, ProjectWikiOrchestrationFlow (TD2), ProjectWikiPluginOrchestrationAdapter.
- **DPO-генераторы троек не читались** (43-localwiki, tests): *DpoGenerator.cs в 42, ResultGapPhase07DpoWriter, CompiledPromptTraining — ядро авто-построения DPO, читать при написании плана.
- **Реальная топология A→B между 6 Bootstrap-юнитами** (42, 15-logical): точные контракты входа/выхода между юнитами — ключ для графовой реконструкции; SubmitUserTaskApi не прочитан.
- **Потребители изолированных библиотек не подтверждены grep-ом** (08-graphrag, 06-vsearch, 24-compress, 28-nlp, 14-ml): подключены ли GraphRag/VectorSearch/Compression/SemanticNlp/MachineLearning к реальному runtime-пути графа или остаются изолированными.
- **Тела тренировочных контрактов и численные пороги** (docs-arch, docs-submap, tests): точные сигнатуры Level 2-4 (Request/Result, fan-out/fan-in), MinRewardMargin/PassesGate в 43.SmartSkillsDomain/SkillSelectionContracts.cs.

### 4.6. LOW — несоответствия правилам проекта и воспроизводимость
- Несколько библиотек нарушают Rule 1 (не OperationResult<T>): 08.GraphRag, 06.VectorSearch (+ Save/Load=NotImplemented, мёртвые метрики Euclidean/InnerProduct).
- Отдельные локальные OperationResult<T> (24.Compression, 14.MachineLearning, 29.SemanticCompiler) — на стыке сборок нужен alias/мост.
- Недетерминизм для воспроизводимых DPO-меток: Sampler без seed (09-inf); float-суммирование + Task.Run в Indexer/Embeddings (05-emb, 08-graphrag); GetHashCode-коллизии в EnsureNonZeroUnitVector (05-emb).
- Дрейф baseline тест-каунта: HOW-TO-USE-SKILLS прочитан частично (1-633 из 782 в catalog; 1-433 в нескольких docs-реконах); даты MAP/state расходятся с baseline ≥1488 в CLAUDE.md (docs-exp, docs-submap, 28-nlp) — перепроверить.
- Тест-проекты физически не найдены для 24.Compression (заявлены только в state.json); тесты не запускались (рекон без исполнения).

### 4.7. Замечание о границах этого индекса
- Проиндексированы **только 40 реконов корпуса `core`** (вход JSON). На диске присутствуют также `codex-recon-mvp-*.md` (корпус `llm mvp` / отдельные ветки language_translate, doc-learning, local-wiki, extensions и т.д.) — они **вне данного задания** и в этот индекс не включены; при необходимости их нужно индексировать отдельным проходом.
- `claude-*` артефакты (в т.ч. `claude-ILEARNINGDYNAMICGRAPH-DPO-PLAN-v1.md`) и `dcm-*` — read-only evidence, не редактировать.
