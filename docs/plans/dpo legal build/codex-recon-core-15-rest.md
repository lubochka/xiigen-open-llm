# Recon: 15.OrchestratorModel — асинхронная обработка input->output, контракты RunAsync

Сбор evidence для плана ILearningDynamicGraph. Только чтение. Корпус: `core`. Slug: `core-15-rest`.

## Вопрос
Оркестратор-модель: асинхронная обработка input->output, контракты RunAsync.

## Что это и где (точные ключевые пути)
В `15.OrchestratorModel` сосуществуют ДВА разных оркестрационных слоя. Это важно для плана: только один из них относится к иерархии ILearning*/RunAsync.

Слой A — обёртки над логической иерархией (релевантно RunAsync):
- `C:/Projects/llm_mvp_core/15.OrchestratorModel/Logical/LogicalOrchestratorAssistant.cs` — runtime-обёртка; метод `RunAsync(string userRequest, ...)` (строка 208) строит `LogicalOrchestratorRequest` и вызывает `_orchestrator.RunAsync(request, ct)` (строка 278). Также `RunTopologyCommandAsync` (299) и `RunWithCapabilityGenerationAsync` (363).
- `C:/Projects/llm_mvp_core/15.OrchestratorModel/Logical/ProjectWikiOrchestrationFlow.cs` — `ILogicalFlow<string>`; `ExecuteAsync(LogicalFlowRequest<string>, ct)` (строка 37). FlowId/ScenarioId = `project-wiki-orchestration-flow`. Делегирует в `SubmitUserTaskApi.SubmitAsync` (42.ProjectWikiDomain, 6 Bootstrap-юнитов).
- `C:/Projects/llm_mvp_core/15.OrchestratorModel/Logical/TeachingSessionBuilder.cs` (не читался детально — см. Пробелы).

Слой B — устаревший детерминированный конвейер (НЕ иерархия ILearning):
- `C:/Projects/llm_mvp_core/15.OrchestratorModel/OrchestratorFacade.cs` — `ProcessAsync(string)` (строка 43): Classify -> Decompose -> StopFilter -> Sort(DependencyGraph) -> Launch -> Merge -> Quality -> Store. Параллельный запуск уровней через `Task.WhenAll` (строка 83).
- `C:/Projects/llm_mvp_core/15.OrchestratorModel/AgentLauncher.cs` — `LaunchAsync(SubTask, context)` (строка 18), вызывает `AgentAlgorithms.AgentOrchestrator.RunAsync(enriched)` (строка 32) — это ДРУГОЙ RunAsync (из 22.AgentAlgorithms), не `ILogicalOrchestrator.RunAsync`.
- `C:/Projects/llm_mvp_core/15.OrchestratorModel/Core/Interfaces.cs` — контракты слоя B: `IEpisodeClassifier`, `IEpisodeDecomposer`, `IStopSignalDetector`, `IAgentLauncher`, `IResultMerger`, `IEpisodeMemory`, `IQualityOrchestrator` (все синхронные кроме `LaunchAsync`).

ВАЖНО: сами контракты RunAsync определены НЕ здесь, а в:
- `C:/Projects/llm_mvp_core/00.Core/LogicalContracts.cs`: `interface ILogicalOrchestrator` (строка 202) с `Task<OperationResult<LogicalOrchestratorResult>> RunAsync(LogicalOrchestratorRequest, ct)` (204); `record LogicalOrchestratorRequest` (174: RunId, ScenarioId, UserRequest, Flags, Metadata); `record LogicalOrchestratorResult` (182: ScenarioId, SelectedFlowId, OverallVerdict, FinalHumanResult, AppliedFlags, FlowResult; + init-поля `BranchResults[]`, `Warnings[]`, `OutputContextPacket`); `interface ILogicalFlow<TInput>` (166) с `ExecuteAsync` (168).
Движок-реализация `AdaptiveLogicalOrchestrator` живёт в `C:/Projects/llm_mvp_core/21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs` (RunAsync на строке 85), НЕ в 15.

## Как устроено/работает (кратко)
Слой A (RunAsync-путь): `LogicalOrchestratorAssistant.CreateDefault(storage)` (строка 59) собирает весь рантайм: регистрирует флоу в `InMemoryLogicalFlowRegistry` (T34, CB4 security, project-documentation, project-wiki-training, `ProjectWikiOrchestrationFlow`, `TranslationRoutingFlow`), сидит топологию `DevelopmentLifecycleTopologySeeder.Seed` (7 фаз / 31 суб-фаза), регистрирует юниты (`PromptRoutingInputCompilerUnit`, `DomainClassifierUnit`, оба `isLearned:false`, Trusted), и создаёт `AdaptiveLogicalOrchestrator` (строка 179). Затем `RunAsync`: при отсутствии явного scenarioId вызывает детерминированный `ProjectWikiRequestRouterUnit().Route(userRequest)` (строка 233) для предварительного выбора сценария (training/documentation), формирует metadata (source, scenario_selection_mode, pre_route_*), строит `LogicalOrchestratorRequest` и делегирует в `_orchestrator.RunAsync`. Результат маппится в `LogicalAssistantRunResult` (record на строке 11). Это тонкий слой: вся семантика/исполнение — в `AdaptiveLogicalOrchestrator` (21).

`ProjectWikiOrchestrationFlow.ExecuteAsync`: читает `active_repo_path`/`linked_repo_paths`/`user_facing_entrypoint` из `request.Metadata`, при отсутствии корня возвращает `BLOCKED_MISSING_PROJECT_ROOT`, иначе строит `SubmitUserTaskRequest` и зовёт `SubmitUserTaskApi.SubmitAsync` (6 Bootstrap-юнитов), собирает `LogicalFlowResult` (Metrics, EvidenceIds = `wiki-unit:*`).

Слой B (`OrchestratorFacade.ProcessAsync`): синхронные шаги классификации/декомпозиции + асинхронный запуск агентов по уровням зависимостей через `Task.WhenAll`, контекст прокидывается вперёд (последний результат уровня). Возвращает `OperationResult<string>`.

## Релевантность к ILearningDynamicGraph
Высокая для слоя A как точки интеграции; средняя по содержанию (логика здесь тонкая, мозги в 21).
- Контракт RunAsync (`ILogicalOrchestrator`) — это L4 целевой иерархии (Level 4 ILogicalOrchestrator -> RunAsync). 15 показывает РЕАЛЬНЫЙ потребительский путь input(`string userRequest`/`Metadata`) -> output(`LogicalOrchestratorResult`/`LogicalFlowResult`). Для DynamicGraph-реконструкции (узлы/рёбра/fan-out/fan-in, async graph execution) релевантны: реестры флоу/юнитов/топологии, сидинг 7 фаз/31 суб-фазы, и адаптивный роутинг по сценариям. Сами рёбра графа и их authority определяются в `AdaptiveLogicalOrchestrator` (21) + топология-сидере — их надо аудировать отдельно (это вне 15).
- Provenance/teacher для DPO: в 15 нет teacher-вызовов и нет обучения. Обучаемого состояния (checkpoint/manifest/export/import/locator) в 15 НЕТ — оркестратор берётся готовым через DI. Лайфсайкл обучаемых юнитов отсутствует в этом проекте.
- Измерение: `LogicalFlowSummary`/`Metrics` несут числовые поля (Baseline/Enhanced, Passed, NextAction), но это контрактные поля результата, не метрики обучения.
- `BranchResults[]`, `OutputContextPacket`, `LogicalContextFragment`/`LogicalContextPacket` (00.Core/LogicalContracts 145-163) — встроенная поддержка fan-in/контекста между ветками графа; прямо релевантно many-to-many/fan-in контрактам DynamicGraph.

## Классификация кода (уровень vs детерминированный хелпер)
Правило: только `ILearningMachine`/`ILogicalOrchestrator`/`ILogicalFlow`/`ILogicalUnit`/`ILearningMachinePair` — допустимые обучаемые уровни (trainable-architecture-decision-SKILL.md §"only allowed trainable architecture levels"). Всё прочее — internal detail/baseline и должно быть честно помечено.

- `LogicalOrchestratorAssistant` (15.../Logical/LogicalOrchestratorAssistant.cs) — НЕ уровень иерархии; это `BaseService`-обёртка/фасад над `ILogicalOrchestrator`. Классификация: внутренняя runtime-обёртка (OK_INTERNAL_DETAIL как транспорт). Семантических claim'ов не делает. Маршрутизация сценария внутри неё — детерминированная (`ProjectWikiRequestRouterUnit`, см. ниже).
- `ProjectWikiOrchestrationFlow` (15.../Logical/ProjectWikiOrchestrationFlow.cs) — это `ILogicalFlow<string>` (допустимый L3-уровень как контракт-сигнатура). Но содержательно — детерминированный orchestrator-fan-out по 6 Bootstrap-юнитам через `SubmitUserTaskApi`; обучаемого состояния нет. Классификация: OK_ALLOWED_LEVEL по контракту, но текущая реализация = baseline/Bootstrap (honest untrained scaffold), НЕ trained feature. Должна честно репортить runner_missing (соответствует проектному `DOCUMENTATION_GRAPH_READY_TRAINING_RUNNER_MISSING`).
- `ProjectWikiRequestRouterUnit().Route(...)` (вызов в LogicalOrchestratorAssistant.cs:233) — детерминированный селектор/роутер сценария (router/selector в терминах скила). Классификация: internal detail / FORBIDDEN_RUNTIME_SHORTCUT-РИСК, если в плане ILearningDynamicGraph выбор маршрута должен быть обучаемым (R4 «No hidden determinism»). Сейчас это рантайм-authority выбора сценария до оркестратора — пометить как deterministic helper и решить, заменяется ли обучаемым unit'ом. (Сам класс — в 42.ProjectWikiDomain, не в 15; здесь только потребление.)
- `PromptRoutingInputCompilerUnit`, `DomainClassifierUnit` — регистрируются с `isLearned:false`, `LogicalUnitTrustState.Trusted` (LogicalOrchestratorAssistant.cs:156-176). Честно помечены как необучаемые baseline-юниты. Классификация: OK_INTERNAL_DETAIL / BASELINE_MUST_BE_LABELLED (помечено корректно).
- `OrchestratorFacade` + `AgentLauncher` + контракты в `Core/Interfaces.cs` (`IEpisodeClassifier`/`Decomposer`/`StopSignalDetector`/`ResultMerger`/`QualityOrchestrator`/`EpisodeMemory`) — устаревший детерминированный конвейер. НЕ уровни ILearning-иерархии. `IEpisodeClassifier` через ISemanticCompiler29, decomposer по фикс-правилам (E32), `InMemoryEpisodeMemory` — заявлен как stub до 14.ContinuousLearning. Классификация: internal detail / baseline pipeline; для DynamicGraph это, вероятно, legacy и НЕ часть целевой иерархии — подтвердить у архитектора.
- `RunAsync` в `AgentLauncher.cs:32` — это `AgentAlgorithms.AgentOrchestrator.RunAsync`, НЕ `ILogicalOrchestrator.RunAsync`. Коллизия имён; не путать в плане.

## Пробелы / что доисследовать
- `AdaptiveLogicalOrchestrator` (`21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs`, RunAsync:85) — НАСТОЯЩИЙ движок RunAsync, рёбра графа, fan-out/fan-in, authority на рёбрах. Вне 15, но критичен для DynamicGraph — отдельный recon обязателен.
- `TeachingSessionBuilder.cs` (15.../Logical/) — не прочитан; относится к teaching/training-сессиям, потенциально релевантно DPO/обучению. Доисследовать.
- `DevelopmentLifecycleTopologySeeder`, `InMemoryLogicalTopologyRegistry`, `SubPhaseRouterUnit`/`MultiSubPhaseRouterUnit`, `DefaultLogicalCapabilityTrainer`, `AddDomainTopologyMutationUnit` и пр. — определены вне 15 (`ExperimentsV2.LogicalFlow`/21). Узлы/рёбра/мутации графа и `capabilityTrainer` — где живёт обучение и authority — доисследовать там.
- `ProjectWikiRequestRouterUnit` (42.ProjectWikiDomain) — реализация детерминированного роутера; проверить, не является ли он скрытым shortcut'ом там, где нужен обучаемый выбор.
- Provenance/teacher-gate: в 15 не применимо (нет teacher-вызовов) — отметить N/A, но проверить 21/42, если граф будет строить DPO.
- Не подтверждён статус `OrchestratorFacade`/слой B относительно целевой иерархии (legacy vs нужно мигрировать) — продуктовое решение Любы/архитектора.
