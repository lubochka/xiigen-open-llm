# Рекон: SUBPROJECT-MAP — статусы сабпроектов по обучению (Done / stub)

Корпус: core · slug: core-docs-submap · для плана ILearningDynamicGraph (авто-DPO).

## Что это и где (точные ключевые пути)

- `C:/Projects/llm_mvp_core/docs/SUBPROJECT-MAP.md` — компактный реестр сабпроектов (таблица # / Dir / Name / What / Status / Key files + граф зависимостей + раздел Implementation notes). Last updated 2026-06-04.
- `C:/Projects/llm_mvp_core/docs/projects/PROJECT-DOCUMENTATION-INDEX.md` — упомянут как индекс детальной пер-проектной документации (НЕ читал в этом реконе; см. Пробелы).
- Контракты-источник иерархии (проверено в коде):
  - `C:/Projects/llm_mvp_core/00.Core/LogicalContracts.cs` — `ILogicalUnit<TInput,TOutput>` (l.97), `ILogicalFlow<TInput>` (l.166), `ILogicalOrchestrator` (l.202).
  - `C:/Projects/llm_mvp_core/00.Core/LearningMachineContracts.cs` — `ILearningMachine<TDomain>` (l.208), `ILearningMachinePair<TPersonal,TGlobal>` (l.245).

## Как устроено / работает (кратко)

Карта = единственная строка статуса на сабпроект. Я сверил несущие «обучаемые» статусы прямо в коде (Glob/Grep/Read), а не доверял только таблице.

Фактические реализации `ILearningMachine<TDomain>` в коде (Grep, не worktrees):
- `27.TranslationDomain/TranslationMachine.cs:7` → `ILearningMachine<TranslationDomain>`.
- `42.ProjectWikiDomain/OrchestrationPlanMachine.cs:7` → `ILearningMachine<OrchestrationPlanDomain>`.
- `43.SmartSkillsDomain/SkillSelectionContracts.cs:1617` → `IMachineTraining<TDomain> : ILearningMachine<TDomain>` (адаптер обучения).
- `43.SmartSkillsDomain/ResultGapLogicalHierarchy.cs:8` → `ResultGapLearningMachineAdapter`.
- `43.SmartSkillsDomain/PromptToTaskLogicalHierarchy.cs:6` → `PromptToTaskLearningMachineAdapter`.
- `43.SmartSkillsDomain/OrchestrationReasoningLogicalHierarchy.cs:10` → `OrchestrationReasoningLearningMachineAdapter`.

Статусы по обучению (из карты; ✔ = сверено в коде):

ОБУЧАЕМЫЕ / ГОТОВЫ как тренируемая инфраструктура:
- 11.DPO — DPO-тренер (`DpoTrainer.cs`), Done. Базовый механизм обучения предпочтений.
- 10.LoRA — LoRA-дообучение, Done. 04.TrainingLoop — Done. 03.TransformerModel — Done. 00.TensorCore — Done. (тренировочный низ стека).
- 14b.ContinuousLearning — Done; LoRA-адаптация на проект, feedback-routing, инкрементальное обучение (`ContinuousLearningPipeline.cs`).
- 16.AutonomousLearning — Done; ночной сбор данных + авто-цикл обучения.
- 18.CapabilityTraining — Active; DPO-пайплайн, capability trainer, replay mixer, teaching registry.
- 26.TrainableRouter — Active; `TrainableRoutingFlow`/`ITrainableRoutingFlow`, routing-DPO. ← прямо релевантно «обучаемой оркестрации/маршрутизации».
- 27.TranslationDomain — Active ✔; реальный `ILearningMachine<TranslationDomain>`, 4 направления, language rule matrix (G01–G10 DPO-семьи), `FindNearestCascade` (Full→Prefix→Structure→BootstrapFallback), lifecycle (export/import/publish/load/continue) через 27.TranslationDomain.Tools; 44 теста (phase 24). Самый зрелый обучаемый домен.
- 25.ModelServer — Active; `IModelRepository`, `IRemoteUnitLocator`, `RemoteUnitManifest`, версионирование чекпойнтов (инфраструктура публикации/локации моделей — нужна для lifecycle обучаемых юнитов).

ЧАСТИЧНО ОБУЧАЕМЫЕ / СМЕШАННОЕ СОСТОЯНИЕ:
- 42.ProjectWikiDomain — Active ✔; `OrchestrationPlanMachine : ILearningMachine<OrchestrationPlanDomain>`, `PromptFlowGroundingModel` (3 режима). Но до публикации весов рантайм-флоу обязан рапортовать `DOCUMENTATION_GRAPH_READY_TRAINING_RUNNER_MISSING` / `runner_missing` (НЕ тренирован end-to-end). `FlowReferenceDetector` — build-time corpus/audit only, НЕ runtime. 6 Bootstrap `ILogicalUnit` обёрток над статикой = стартовая точка DPO (decision A-LM-03), не обученные модели.
- 43.SmartSkillsDomain — Active (Phase 39/40 + QUALITY-02) ✔; `SkillSelectionMachine : ILearningMachine<SkillSelectionDomain>` + `IMachineTraining`; 11 trainable семантических юнитов, у всех `DeterministicFinalAuthority = false`, `Trainable = true` (сверено: `SmartSkillsSemanticUnitCatalog.cs:127`). НО: `OrchestrationReasoningMachine` и `PromptToTaskMachine` = СКЕЛЕТЫ, НЕ обучены (сверено: const `NotTrainedStatus = "not_trained_no_learned_guide_state"` `OrchestrationReasoningMachine.cs:97`; `"not_trained_no_loaded_state"` `PromptToTaskMachine.cs:80`) — отдельный паузнутый план. `ExtensionEvalOnlyQualityRunner` (QUALITY-02) — eval-only хелпер; держит semantic/trainable/runtime/token-savings/readiness claims = false.

STUB / PLANNED / НЕТ ОБУЧЕНИЯ:
- 17.DynamicDomains — Planned, кода нет (детекция новых доменов, confidence gating).
- 30.DesignConversionDomain — Planned, кода нет.
- 31.AffiliateDomain — Planned; намечен `ILearningMachinePair<TPersonal,TGlobal>` (personal+global, anonymised sync) — единственный заявленный Pair-уровень, но не реализован.
- 24.Compression — Partial. 28.SemanticNlp — Partial. 12b.SemanticCompiler — Superseded (заменён 29). 29.SemanticCompiler — Done, но это детерминированный pipeline (см. классификацию), не обучаемый домен.

Test baseline (карта): ≥ 1488 passed, 0 failed (2026-05-29); +44 orchestration (Phase 37), +6 translation lifecycle (Phase 38), +smart-skills (Phase 39), +10 skeleton-тестов для НЕобученных машин (Phase 39/40). Карта сама требует ре-верификацию live-счётчика после Phase 37–40.

## Релевантность к ILearningDynamicGraph (DPO / обучение / граф / оркестрация / измерение / teacher-provenance)

- КРИТИЧНО — несоответствие имён контрактов. Имена из задания `ILearningUnit / ILearningFlow / ILearningOrchestrator / ILearningDynamicGraph` в коде НЕ существуют (Grep по *.cs: «No files found»). Реально есть `ILogicalUnit/ILogicalFlow/ILogicalOrchestrator` (чистый inference) + `ILearningMachine<TDomain>`/`ILearningMachinePair` (обучение). План должен либо переиспользовать существующую 5-уровневую иерархию (CLAUDE.md §ILearningMachine pattern, A-LM-02: Train* только на `ILearningMachine`), либо явно вводить новые `ILearning*`/DynamicGraph контракты в `00.Core` как новую работу — это надо зафиксировать в Authority Ledger плана, иначе «удаление старого ≠ покрытие нового».
- Где уже есть граф-подобное: `07.GraphEngine` (SymbolGraph + PageRank, Done), `08.GraphRag` (HybridRetriever, Done), `22.AgentAlgorithms/DependencyGraph.cs` (граф зависимостей, переиспользуется 15-м). Это кандидаты на узлы/рёбра, но детерминированные — не trainable-граф сами по себе.
- Где уже есть обучаемая оркестрация/маршрутизация для DPO: 26.TrainableRouter (routing-DPO), 42.OrchestrationPlanMachine + ProjectWikiOrchestrationFlow, 18.CapabilityTraining (DPO pipeline + replay), 11.DPO (тренер). Это база для авто-построения DPO.
- Готовые DPO-стартовые точки: 6 Bootstrap `ILogicalUnit` в 42 (decision A-LM-03) и 11 семантических юнитов в 43 (все `DeterministicFinalAuthority=false`). Их детерминированные хелперы (BM25/regex/if-else) — только bootstrap-генераторы кандидатов.
- Lifecycle/provenance инфраструктура: 25.ModelServer (`IModelRepository`/`IRemoteUnitLocator`/manifest/checkpoint versioning), `models/` реестр (`models/index.json` + per-domain manifest/checkpoint/runner-report), `training-data/` корпус. Покрывает export/import/locate/fresh-load, требуемые для trainable-юнита.
- Measurement: 35.ExperimentsV2 (E01-E50 runner/harness), 36.LibraryComparison (бенчмарки).

## Классификация (уровень иерархии ИЛИ детерминированный хелпер)

Со ссылкой на правила скила (HOW-TO-USE-SKILLS.md, триггеры строк 327–347; CLAUDE.md «Allowed trainable architecture pattern gate»):

- `ILearningMachine<TDomain>` (Level 5, Train*-методы): 27.TranslationMachine, 42.OrchestrationPlanMachine, 43.SkillSelectionMachine (+ адаптеры ResultGap/PromptToTask/OrchestrationReasoning). Заявленный Level 1 `ILearningMachinePair`: 31.AffiliateDomain (planned, не реализован).
- `ILogicalOrchestrator` (Level 4): путь `ProjectWikiOrchestrationFlow`/`ProjectLocalWikiOrchestrator` в 42; 15.OrchestratorModel как оркестрация.
- `ILogicalFlow` (Level 3): `ProjectDocumentationPipelineFlowAdapter` (42), `TrainableRoutingFlow` (26).
- `ILogicalUnit` (Level 2): 6 Bootstrap-юнитов в 42; 11 семантических юнитов в 43.
- ДЕТЕРМИНИРОВАННЫЕ ХЕЛПЕРЫ (НЕ архитектурный уровень — по правилу скила: scorer/selector/router/retriever/KNN/Jaccard/BM25/dictionary/regex/keyword и приватные `TryDecide*` — это деталь реализации ВНУТРИ разрешённого уровня, не сам уровень):
  - 12a.SearchEngine (BM25+K-NN), 06.VectorSearch (K-NN), 08.GraphRag HybridRetriever, 07.GraphEngine PageRank, 29.SemanticCompiler (AstTokenizer→RouteClassifier→Decomposer→TemplateBeautifier — детерминированный pipeline), `FlowReferenceDetector` (42, build-time audit only), 22.DependencyGraph. Все они допустимы лишь как bootstrap-кандидаты/eval/audit, НЕ как финальный обучаемый авторитет (R4 «No hidden determinism»; CLAUDE.md gate).
  - `FindNearestCascade` (27) и KNN-состояние в чекпойнте — KNN как память юнита внутри `ILearningMachine`, допустимо как роль внутри уровня, но не как самостоятельный уровень.

## Пробелы / что доисследовать

- `docs/projects/PROJECT-DOCUMENTATION-INDEX.md` и пер-проектные доки НЕ прочитаны (защита от перегруза) — там детальные контракты и статусы; нужны для §3 кода в плане.
- Реальные сигнатуры `00.Core/LogicalContracts.cs` и `LearningMachineContracts.cs` (методы уровней, Input/Output) видел только по именам интерфейсов — для §5 hierarchy плана прочитать тела (Train*/Execute*/Run*).
- Введение новых `ILearning*`/`ILearningDynamicGraph` контрактов: нет ни кода, ни решения D-NN/A-LM — проверить `docs/DECISIONS-LOCKED.md`, существует ли санкция на новый уровень/имя, либо план обязан переиспользовать `ILogical*`+`ILearningMachine`.
- Live test-count после Phase 37–40 не подтверждён (карта сама помечает «re-verification required») — не использовать ≥1488 как доказанное без перезапуска.
- Статусы Active/Partial (15,18,21,22,23,24,25,26,28) — взяты из таблицы карты, не сверены по коду в этом реконе; при опоре на конкретный сабпроект — точечно перечитать его key files.
- Граф-движки (07/08/22) как фундамент DynamicGraph: их API (узлы/рёбра/fan-out) не читал — нужно при дизайне many-to-many edge-контрактов.
