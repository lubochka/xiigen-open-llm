# Codex Recon — core/15 Logical: регистрация flow-адаптеров и соединение юнитов в поток

Рекон-evidence для плана ILearningDynamicGraph. Только чтение. Узкий вопрос: как ILogicalOrchestrator/ILogicalFlow регистрируются и как юниты соединяются в поток.

## Что это и где (точные пути)

Цель `15.OrchestratorModel/Logical` содержит ровно 3 файла:
- `C:/Projects/llm_mvp_core/15.OrchestratorModel/Logical/LogicalOrchestratorAssistant.cs` — точка сборки: создаёт реестры, регистрирует все flow/unit, конструирует оркестратор.
- `C:/Projects/llm_mvp_core/15.OrchestratorModel/Logical/ProjectWikiOrchestrationFlow.cs` — пример flow (`ILogicalFlow<string>`), который оборачивает 6 Bootstrap-юнитов TD1 через `SubmitUserTaskApi`.
- `C:/Projects/llm_mvp_core/15.OrchestratorModel/Logical/TeachingSessionBuilder.cs` — не прочитан подробно (вне вопроса регистрации; teaching-сборка).

Опорные файлы вне цели (load-bearing для вопроса):
- `C:/Projects/llm_mvp_core/00.Core/LogicalContracts.cs` — контракты уровней: `ILogicalUnit<TInput,TOutput>` (стр.97), `ILogicalFlow<TInput>` (стр.166), `ILogicalOrchestrator` (стр.202); записи `LogicalFlowRequest<TInput>` (105), `LogicalFlowResult` (131), `LogicalOrchestratorRequest` (174), `LogicalOrchestratorResult` (182), `LogicalFlowBinding` (196), `PhaseTransitionSignal` (89), `LogicalContextPacket`/`LogicalContextFragment` (146–163).
- `C:/Projects/llm_mvp_core/21.AgentInfrastructure/Logical/InMemoryLogicalFlowRegistry.cs` — реестр flow: `Register`, `RegisterFactory`, `Resolve*`, `LogicalFlowDescriptor`.
- `C:/Projects/llm_mvp_core/21.AgentInfrastructure/Logical/LogicalFlowFactoryContracts.cs` — `ILogicalFlowFactory<TInput>` (стр.18), `LogicalFlowFactoryRequest` (не прочитан целиком).

## Как устроено / работает (кратко)

Иерархия 4 уровней (контракты в 00.Core, реализации в 15/21/42/27 и др.):
- Уровень 2 `ILogicalUnit<TInput,TOutput>.ExecuteAsync` — атомная ячейка; in: `LogicalUnitRequest<TInput>`, out: `LogicalUnitResult<TOutput>` (есть опц. `TransitionSignal`).
- Уровень 3 `ILogicalFlow<TInput>.ExecuteAsync` — топология юнитов; in: `LogicalFlowRequest<TInput>`, out: `LogicalFlowResult` (есть `TransitionSignals[]`).
- Уровень 4 `ILogicalOrchestrator.RunAsync` — координатор над flow; in: `LogicalOrchestratorRequest`, out: `LogicalOrchestratorResult` (есть `BranchResults[]`, `OutputContextPacket`).

Соединение юнитов в поток (на живом примере `ProjectWikiOrchestrationFlow`):
- Flow НЕ вызывает реестр юнитов сам. Он держит зависимость `SubmitUserTaskApi` (конструктор, дефолт = 6 Bootstrap-юнитов: intent → scan → evidence → select → compose → verify) и в `ExecuteAsync` последовательно прогоняет их через `_submitUserTaskApi.SubmitAsync(...)`.
- Вход — `request.Input` (string = промпт); параметры топологии (`active_repo_path`, `linked_repo_paths`, `user_facing_entrypoint`) читаются из `request.Metadata`. Нет root → возврат `BLOCKED_MISSING_PROJECT_ROOT` (мягкий OK-результат, не throw).
- Результат маппится в `LogicalFlowResult` (+`LogicalFlowSummary`, метрики, `EvidenceIds` вида `wiki-unit:<unit>`). То есть «соединение в поток» = ручная композиция в коде flow/в `SubmitUserTaskApi`, а не декларативный граф из реестра.

Регистрация (точка — `LogicalOrchestratorAssistant.CreateDefault`, стр.59–205):
- `new InMemoryLogicalFlowRegistry()` → серия вызовов `registry.Register(flowId, scenarioId, version[, phaseId, subPhaseId, taskType, domainTags], flowInstance)`.
- Зарегистрированы: T34 (`Register` инстансом), CB4/Security (`RegisterFactory` с фабрикой `SecurityVerification34FlowFactory`), `ProjectDocumentationPipelineFlowAdapter` (route 2/2.1), `ProjectWikiTrainingFlowAdapter` (7/7.5), `ProjectWikiOrchestrationFlow`, `TranslationRoutingFlow`. Плюс `DevelopmentLifecycleTopologySeeder.Seed(...)` сеет 7 фаз/31 подфазу как 9-нодовые flow в `InMemoryLogicalTopologyRegistry`.
- Юниты регистрируются в ОТДЕЛЬНОМ реестре `InMemoryLogicalUnitRegistry.Register(id, role, domainTag, version, trustState, isLearned, unit)` — напр. `PromptRoutingInputCompilerUnit`, `DomainClassifierUnit` (оба `isLearned:false`, Trusted).
- Оркестратор: `new AdaptiveLogicalOrchestrator(topologyRegistry, promptCompiler, router, registry, unitRegistry, multiRouter, capabilityTrainer: new DefaultLogicalCapabilityTrainer())` — он связывает реестры flow+unit+topology и роутеры.

Реестр flow (`InMemoryLogicalFlowRegistry`):
- Хранит `List<(LogicalFlowDescriptor, object FlowOrFactory)>`; ключи поиска — `FlowId`/`ScenarioId`/`SubPhaseId` + `InputType`; берётся НОВЕЙШИЙ по `RegistrationOrder` (latest-wins → так trained-юнит может заменить Bootstrap, A-LM-03).
- `Resolve*`/`ResolveBindingByScenario`/`ResolveBindingBySubPhase` возвращают `ILogicalFlow<TInput>`; фабрика разворачивается только при наличии `LogicalFlowFactoryRequest` (иначе `FLOW_FACTORY_REQUEST_REQUIRED`).
- Всё через `OperationResult<T>`; null-flow/пустые id отклоняются; есть `LoadEntries(...)` для загрузки персистнутых дескрипторов (reflection-проверка `ILogicalFlow<>`/`ILogicalFlowFactory<>`).

Связь scenario→flow в `LogicalOrchestratorAssistant.RunAsync`: явный `scenarioId` → прямой роут; иначе adaptive, плюс детерминистический префильтр `ProjectWikiRequestRouterUnit.Route(userRequest)` переключает scenario на documentation/training; затем `_orchestrator.RunAsync(LogicalOrchestratorRequest{...metadata...})`.

## Релевантность к ILearningDynamicGraph

- Это уже работающая основа «юнит → flow → orchestrator» с реестрами и latest-wins подменой — естественный субстрат для DynamicGraph (узлы=юниты/flow, рёбра=композиция).
- ВАЖНЫЙ ПРОБЕЛ для графа: текущая «топология» внутри flow — это императивная последовательность в коде (`SubmitUserTaskApi`), а НЕ декларативный набор рёбер с контрактами. Для ILearningDynamicGraph по Graph-Style Reconstruction Gate скила нужны явные nodes/edges, input/output-контракты на рёбрах, fan-out/fan-in, many-to-many, async-исполнение и владелец final authority на каждом ребре — этого в `15.Logical` нет.
- DPO/обучение/измерение: на уровне flow есть `LogicalFlowSummary` (Baseline/Enhanced, Passed, ShouldScaleUp, NextAction) и `EvidenceIds` — пригодно как сигнал/трасса; `AdaptiveLogicalOrchestrator` несёт `DefaultLogicalCapabilityTrainer` (тренировочный хук — доисследовать в 18.CapabilityTraining). `LogicalUnitRegistry` хранит флаг `isLearned` и `TrustState` — точки для пометки обученных юнитов.
- Teacher/provenance: в прочитанных файлах teacher-источники DPO не фигурируют (нужно искать в 42/18/14). По Legal Teacher Gate — проверить отдельно.
- Граф-контекст между ветками уже частично заложен: `LogicalContextPacket`/`LogicalContextFragment` и `PhaseTransitionSignal` (advisory re-entry в оркестрацию) — это зачатки fan-in/перехода рёбер графа.

## Классификация уровней (по trainable-architecture-decision-SKILL)

- `ILogicalFlow<string>` `ProjectWikiOrchestrationFlow` — допустимый уровень FLOW (`ILogicalFlow`). Внутри детерминистическая последовательная композиция 6 Bootstrap-юнитов; сам по себе НЕ trainable, обучение не заявлено. Вердикт по скилу: `OK_INTERNAL_DETAIL`/честный untrained-скелет (не «семантический» по умолчанию).
- `ProjectWikiRequestRouterUnit.Route(...)` (вызывается в `RunAsync`) — детерминистический pre-route к scenario. Это роутер/селектор → внутренний детерминистический хелпер, НЕ trainable-уровень. Допустим только как helper/baseline; если в плане он станет финальной авторитетной маршрутизацией для обучаемого графа — это риск `FORBIDDEN_RUNTIME_SHORTCUT`/`NEEDS_..._WRAPPER`. Доисследовать его реализацию.
- `PromptRoutingInputCompilerUnit`, `DomainClassifierUnit` — зарегистрированы как `ILogicalUnit`, но помечены `isLearned:false`, Trusted → допустимые юниты-уровня, честно помечены как детерминистические (не семантические).
- `InMemoryLogicalFlowRegistry` / `InMemoryLogicalUnitRegistry` / `InMemoryLogicalTopologyRegistry` — инфраструктура реестров, не архитектурный «trainable»-уровень (storage detail). `latest-wins` по `RegistrationOrder` — механизм подмены, не авторитет.
- `AdaptiveLogicalOrchestrator` — уровень ORCHESTRATOR (`ILogicalOrchestrator`); содержит `capabilityTrainer` (потенциально обучаемая часть) — классификацию trainable-ядра делать после чтения самого `AdaptiveLogicalOrchestrator` и `DefaultLogicalCapabilityTrainer` (не читал; вне 3–8 бюджета).
- `LogicalFlowDescriptor` — провенанс/маршрутные метаданные (phase/subPhase/taskType/domainTags/version) — диагностика/роутинг, не авторитет обучения.

## Пробелы / что доисследовать (вне бюджета этого рекона)

1. `AdaptiveLogicalOrchestrator` (где? искать в 15/21/ExperimentsV2.LogicalFlow) — как реально выбирает flow, как применяет `capabilityTrainer`, есть ли скрытый детерминизм в роутинге; `RunWithCapabilityGenerationAsync`.
2. `SubmitUserTaskApi` + 6 Bootstrap-юнитов в `42.ProjectWikiDomain` — фактическая «топология» потока (порядок, контракты A↔B между юнитами) — ключ для графовой реконструкции.
3. `DefaultLogicalCapabilityTrainer` (`18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs`) — есть ли обучение/DPO, метрики, checkpoint/export-import, locator.
4. `ILogicalFlowFactory<TInput>` + `LogicalFlowFactoryRequest` (`21.../LogicalFlowFactoryContracts.cs`) — как фабрики выбирают юниты по флагам (динамическая сборка потока).
5. `InMemoryLogicalTopologyRegistry` + `DevelopmentLifecycleTopologySeeder` (9-нодовые flow, 31 подфаза) — ближайшее к «графу узлов/рёбер»; проверить, есть ли там настоящие edges/контракты.
6. `LogicalUnitTrustState`/`isLearned`, lifecycle обученных юнитов (load user→repo fallback, export/import, continue-training) — не подтверждён в прочитанном.
7. `TeachingSessionBuilder.cs` (в цели) — не прочитан подробно; связь с обучением/DPO.
8. Teacher/provenance DPO (Legal Teacher Gate) — источники не видны в 15.Logical; искать в 42/18/14.
