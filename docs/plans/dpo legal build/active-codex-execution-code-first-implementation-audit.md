# Code-first аудит: базовые контракты Learning Graph
> Пометка 14.08.26: упоминания ветки `DpoImplementation` в этом файле — historical; ветка-канон = `codex/dpo-implementation` (см. 14.08.26WorkLeft-05-branch-reconciliation-codex-dpo-implementation.md).

## Current Accepted/Pushed Execution Reconciliation Overlay (2026-07-14)

```text
audit_state: SUPERSEDED_BY_ACCEPTED_PUSHED_CODE_EXECUTION
execution_state: ACTIVE_CODE_FIRST
implementation_authorization: YES_CODE_FIRST_ONLY
teacher_dpo_training_authorization: NO_TEACHER_DPO_TRAINING_UNTIL_G_DPO_FINAL
source_root: C:\Projects\llm_mvp_core-DpoImplementation
publication_target: origin/codex/dpo-implementation
worktree_state_at_acceptance: clean
accepted_pushed_tip: e2a6bce7448a1672a90bdec95106da279b077c5b
published_core_slices: [07, 08, 09, 10, 11, 12, 13]
next_slice: HOST_REFERENCE_SEAM_FIRST_IN_MANUAL_CHAIN__BOUNDED_SLICE_NOT_SELECTED
architecture_review_state: A1_A5_RESIDUALS_CURRENT
teacher_stage_state: TP1_LOCAL_NOT_STARTED; TP2_HOSTED_NOT_STARTED
dpo_training_state: NOT_STARTED
```

The audit body below is retained as the pre-implementation baseline. Its “read-only”, absent-contract, missing-dotnet/test-environment and design-only/current-status statements are historical where the following independently accepted and pushed evidence supersedes them. This overlay does not erase still-open product work and does not convert a bounded accepted slice into strict composite Gate-B `done`.

### Accepted/pushed core slice ledger

Rows 1-12, including `615d96...`, `002bd7f...`, `d489507e...` and `16e92b46...`, are historical accepted-slice evidence. Row 13 (`e2a6bce...`) is the current accepted/pushed tip.

| Order | Commit | Accepted slice | Evidence boundary |
|---:|---|---|---|
| 1 | `3528025e` | graph contracts, validation and tests | bounded code-first slice; pushed |
| 2 | `c8b282f3` | edge flow and tests | bounded code-first slice; pushed |
| 3 | `354c4d08` | fan-in behavior and tests | bounded code-first slice; pushed |
| 4 | `8500a1a4` | logical-node wrapper and tests | bounded code-first slice; pushed |
| 5 | `cc359cfe` | model activation internals | bounded code-first slice; pushed |
| 6 | `615d96ea27db` | typed in-memory registry plus single-combination candidate runner and tests | bounded code-first slice; pushed |
| 7 | `408044656911d0b5766d71d9723d661e310156ce` | bounded multi-combination strategy/runner and tests | bounded code-first slice; pushed after reviewer fixes and repeat independent `ACCEPT` |
| 8 | `62c1e996670d8a7786dcf0b4545b5f82e8937254` | deterministic topology builder, contracts and tests | bounded code-first slice; independently accepted and pushed |
| 9 | `382544dd7cfc2a6ff473acdb1b5a6abf82a18064` | revisioned in-memory topology runtime owner, contracts and tests | bounded code-first slice; independently accepted and pushed |
| 10 | `002bd7f1523f8c3802a341438cf914bc1a37d12d` | strict logical-registry to dynamic-topology projection, contracts and tests | bounded code-first slice; independently accepted and pushed; historical accepted tip before slice 11 |
| 11 | `d489507e1e9fb0350c541fc1ac3dd5786e01500d` | production L4 read-only dynamic-topology consumption through Adaptive/Assistant composition plus focused tests | bounded code-first slice; independently accepted and pushed; historical accepted tip before slice 12 |
| 12 | `16e92b46cc4f57c3a73341902a853d86a5c17267` | production bootstrap facade joining registry projection, private runtime ownership, read-only source and assistant | bounded code-first slice; independently accepted and pushed; historical accepted tip before slice 13 |
| 13 | `e2a6bce7448a1672a90bdec95106da279b077c5b` | explicit caller-owned composition profile and first production caller outside tests | bounded code-first slice; independently accepted and pushed; current tip |

Current acceptance packets: slice 07 `32/32` new tests, `187/187` graph regression, full build `0` warnings / `0` errors; slice 08 `24/24`, `211/211`, `0/0`; slice 09 `36/36`, `247/247`, `0/0`; slice 10 `18/18`, `265/265`, `0/0`; slice 11 focused `20/20`, graph `285/285`, Adaptive/Assistant `64/64`, legacy `24/24`, full build `0/0`; slice 12 focused `14/14`, required six-class packet `132/132`, seven classes including Adaptive `156/156`, full build `0/0`; slice 13 focused `20/20`, runtime `14/14`, DynamicGraph `271/271`, Assistant+Adaptive `44/44`, full build `0/0`. Slice 07 needed bounded reviewer fixes before repeat independent `ACCEPT`; slices 08-13 each received independent `ACCEPT` before their own commit/push. For every row, local HEAD equalled the pushed remote tip and the worktree was clean before the next slice began. These observations prove the listed slices only. They do not close composite Gate B, complete the full machine or edge-authority execution, run TP1/TP2, produce DPO pairs, invoke a teacher/judge, train a model or prove checkpoint lifecycle.

### Current post-slice-13 state — manual chain known, bounded host slice not selected

Slice 11 supplies the production L4 read-only consumer. Slice 12 adds `DynamicGraphLogicalOrchestratorRuntime`: it constructs the seeded registry, explicit projector, deterministic builder/runtime owner, exact revision `0 -> 1`, read-only source and assistant as one facade while retaining projector/owner mutation authority privately. Slice 13 adds `DynamicGraphLogicalOrchestratorComposition`: the first production caller outside tests snapshots one explicit caller-owned profile, binds descriptor-only nodes and exact edge/budget metadata, delegates bootstrap once, and preserves downstream failures exactly.

The remaining manual-only chain is explicit but no next bounded implementation slice is selected in this document: select a real deployment host/reference seam; make that host deserialize/select the profile and own runtime lifetime; invoke the composed `Runtime.Assistant`; add persistence/restart for selected profile plus revision/CAS state; then explicitly design authority schema/canon/resolver because current `AuthorityUnitId`/level metadata is insufficient for authority execution. A5-R1 is already resolved for code-first by Luba's live instruction (`YES_CODE_FIRST_ONLY`); teacher/DPO/training remains `NO_TEACHER_DPO_TRAINING_UNTIL_G_DPO_FINAL`. The ordered lane is `CODE_FIRST → TP1_LOCAL eval-only → TP2_HOSTED eval-only → G-DPO-FINAL → DPO/training`; TP1/TP2/DPO have not started, and closed-proprietary outputs remain prohibited as DPO authors/training targets.

### Capability boundary after slices 07-13

The accepted code now separates bounded candidate execution; deterministic topology proposal/validation; revisioned runtime ownership; strict registry projection; production L4 read-only consumption; facade bootstrap; and explicit profile-to-runtime composition. The production composition caller exists, but deployment hosting, profile deserialization/selection, runtime lifecycle, persistence/restart and authority resolution remain manual/not implemented. Whole `Ф1.1.3`, `Ф1.3.2`, `Ф6.1.1`, composite Gate B, edge-authority execution, L5/full machine and DPO/training remain open. `ACCEPTED_PUSHED_SLICE != composite gate`.

audit_state: `COMPLETE_WITH_TEST_ENVIRONMENT_LIMITATION`  
product_readiness: `NOT_READY`  
audit_scope: только четыре базовых контракта; DPO и обучение сознательно отложены  
source_root: `C:\Projects\llm_mvp_core-DpoImplementation`  
source_branch: `DpoImplementation`  
source_baseline: `504bcba93f7bcd4db1c93fc094eea3e8133d310d`  
source_mode: read-only; исходный код не менялся  

## Как читать имена

Имена Любы — архитектурные псевдонимы существующей пятиуровневой иерархии, а не разрешение создать шестой уровень:

| Имя задачи | Реальный базовый контракт в коде |
|---|---|
| `ILearningUnit` | `ILogicalUnit<TInput,TOutput>` |
| `ILearningFlow` | `ILogicalFlow<TInput>` |
| `ILearningOrchestrator` | `ILogicalOrchestrator` |
| `ILearningDynamicGraph` | будущая реализация `ILearningMachine<DynamicGraphDomain>`; сам generic-контракт L5 уже существует |

Легенда статусов: `IMPLEMENTED_AND_CALLED` — есть реализация и product-source caller; `IMPLEMENTED_UNWIRED` — реализация есть, но нет product-source caller; `PARTIAL` — часть поведения есть, обязательная часть отсутствует; `STUB` — только заглушка; `MISSING` — требуемого продукта нет; `NOT_IN_CORE` — capability принадлежит другому слою.

## Доска четырёх оснований

| Основание | Статус | Цепочка definition → implementation → wiring/caller → tests | Точный пробел | Файлы / тест / worker |
|---|---|---|---|---|
| **1. `ILearningUnit` ≡ `ILogicalUnit<TInput,TOutput>`: domain/vector/output + many-to-many `IQueryable`** | `PARTIAL` | Контракт и одиночный typed output: `00.Core/LogicalContracts.cs:62`, `:72`, `:97`. Реальная unit-реализация: `21.AgentInfrastructure/Logical/UniversalNineNodeDevelopmentFlow.cs:18`, `:34`. Registry/latest selection: `21.AgentInfrastructure/Logical/InMemoryLogicalUnitRegistry.cs:39`, `:107`. Реальный flow вызывает unit: `UniversalNineNodeDevelopmentFlow.cs:162`, `:205`. Contract tests: `Tests/LogicalFlowContractTests.cs:114`, `:146`. | `DomainTag` — строка, `Output` — один `TOutput`; нет graph-node descriptor/vector, `GraphNodeCandidate`, публичного `IQueryable<GraphNodeCandidate>`, output refs и many-to-many node contract. | Изменить: NEW `00.Core/DynamicGraphContracts.cs`; NEW `Tests/DynamicGraphContractsTests.cs`. Тест: exact generic boundary + exact `IQueryable<GraphNodeCandidate>` + отсутствие `Train*` на node. Worker: `core-foundation-contract-builder` (`gpt-5.5/xhigh`). |
| **2. `ILearningFlow` ≡ `ILogicalFlow<TInput>`: связь и correction между units** | `PARTIAL` | Контракт correction carrier: `00.Core/LogicalContracts.cs:89`, flow result: `:131`, flow: `:166`. Flow последовательно вызывает девять units и собирает сигналы: `UniversalNineNodeDevelopmentFlow.cs:162`, `:205`, `:212`, `:262`. Переход хранится как `LogicalTransition`: `21.AgentInfrastructure/Logical/LogicalTopologyRegistry.cs:51`; correction consumer/re-entry: `AdaptiveLogicalOrchestrator.cs:361`. Тесты перехода/порядка: `Tests/AdaptiveLogicalOrchestratorTests.cs:171`, `:191`, `:235`. | Нет `IDynamicGraphEdge`, пяти edge kinds, typed source/target relation, authority reference, multi-output edge и canonical `Merge(GraphFanInMergeInput) -> OperationResult<GraphFanInMergeResult>`. В production source нет ни одного производителя `new PhaseTransitionSignal`; такие сигналы создаются только fixtures в тестах. | Изменить после базового contract slice: `00.Core/DynamicGraphContracts.cs`, NEW `21.AgentInfrastructure/Logical/DynamicGraphEdgeFlow.cs`, NEW `Tests/DynamicGraphEdgeFlowTests.cs`. Тест: unit→unit correction, fan-out/fan-in/many-to-many, authority, negative cycle. Worker: `graph-flow-builder` (`gpt-5.5/xhigh`). |
| **3. `ILearningOrchestrator` ≡ `ILogicalOrchestrator`: async input→output** | `IMPLEMENTED_AND_CALLED` | Async request/result/interface: `00.Core/LogicalContracts.cs:174`, `:182`, `:202`. Реализация: `21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs:10`, `:85`. Manual composition root: `15.OrchestratorModel/Logical/LogicalOrchestratorAssistant.cs:59`, `:181`; product-source caller: `:210`, `:280`. Fan-out/fan-in и order: `AdaptiveLogicalOrchestrator.cs:445`, `:465`, `:544`, `:841`. Public-facade test: `Tests/LogicalOrchestratorAssistantTests.cs:24`; branch tests: `Tests/AdaptiveLogicalOrchestratorTests.cs:93`, `:136`, `:171`. | Базовый async контракт выполнен. Ограничение: Microsoft DI registration не найден — используется ручная композиция; внешний host caller для `LogicalOrchestratorAssistant` не найден. Graph-specific topology/authority/tenant contract ещё не подключён. | Базовый контракт не менять. После появления graph contracts изменить `AdaptiveLogicalOrchestrator.cs` и `LogicalOrchestratorAssistant.cs`; добавить `Tests/DynamicGraphOrchestratorTests.cs`. Worker: `graph-orchestrator-wiring-builder` (`gpt-5.5/xhigh`). |
| **4. `ILearningDynamicGraph` ≡ `ILearningMachine<DynamicGraphDomain>`: deterministic graph execution** | `MISSING` | Generic L5-контракт существует: `00.Core/LearningMachineContracts.cs:208`, но тестирует только форму через private stubs: `Tests/LearningMachineContractsTests.cs:20`, `:65`. Есть лишь deterministic helpers `DynamicDomainTransition`, closure и bridge expansion: `17.DynamicDomains/DynamicDomainGraph.cs:3`, `:6`, `:15`, `:66`, `:68`; их использует текущий orchestrator: `AdaptiveLogicalOrchestrator.cs:522`, `:772`, `:825`. | Нет `DynamicGraphDomain`, `DynamicGraphMachine`, `DynamicGraphTopology`, graph node/edge contracts, `BuildDynamicTopology`, typed fan-in, authority owner/ref, tenant boundary, domain/context/order carriers. `AddOrUpdateTransition` (`LogicalTopologyRegistry.cs:400-477`) проверяет заполненность полей, но не существование endpoints и не DAG/acyclicity. Значит deterministic helpers — substrate, а не требуемый DynamicGraph product. | После contracts изменить/добавить runtime-файлы только отдельным builder slice; DPO-machine не начинать. Будущие tests: reject cycle/dangling edge/cross-tenant authority, stable order, fan-out/fan-in, context carry-forward. Worker: `dynamic-graph-runtime-builder` (`gpt-5.5/xhigh`). |

## Отрицательные проверки

Поиск по `00.Core`, `17.DynamicDomains`, `21.AgentInfrastructure`, `15.OrchestratorModel`, `Tests` дал **0** определений/упоминаний каждого из: `ILearningUnit`, `ILearningFlow`, `ILearningOrchestrator`, `ILearningDynamicGraph`, `IDynamicGraphNode`, `IDynamicGraphEdge`, `DynamicGraphTopology`, `DynamicGraphMachine`, `DynamicGraphDomain`, `GraphNodeCandidate`, `GraphFanInMergeInput`, `GraphFanInMergeResult`, `AuthorityUnitId`.

Marker scan (`TODO|FIXME|NotImplemented|stub|placeholder|scaffold|fake|mock`) не нашёл production stub в прочитанных foundation-файлах. Он нашёл test-only stubs в `LearningMachineContractsTests.cs`; поэтому эти тесты доказывают implementability generic L5-контракта, но не реализацию DynamicGraph.

## Минимальная проверка build/test

Попытка:

```text
dotnet test Tests\LlmFromScratch.Tests.csproj --no-restore --filter "FullyQualifiedName~LogicalFlowContractTests|FullyQualifiedName~AdaptiveLogicalOrchestratorTests|FullyQualifiedName~DependencyGraphWiringTests|FullyQualifiedName~LearningMachineContractsTests"
```

Результат: `NOT_RUN_ENVIRONMENT` — команда `dotnet` отсутствует в `PATH`, стандартные `dotnet.exe`, `MSBuild.exe` и `vstest.console` не найдены. Это не test pass и не test failure продукта. Статически прочитаны релевантные test bodies; runtime-верификация остаётся обязательной у builder worker с .NET 9 SDK.

## Implementation Evidence Map

| Требование | Доказанная capability | Реализация / exact refs | Production call chain | Changed code | Наблюдаемое поведение / falsification |
|---|---|---|---|---|---|
| Unit async typed input/output | Один unit исполняет typed request и возвращает typed result | `LogicalContracts.cs:62-101`; `UniversalNineNodeDevelopmentFlow.cs:18-85` | `UniversalNineNodeDevelopmentFlow.ExecuteAsync` → `unit.ExecuteAsync` (`:162-205`) | none | Falsify: interface shape/test `LogicalFlowContractTests.cs:114-159` не компилируется или unit не возвращает output. |
| Flow relation/correction substrate | Units выполняются по порядку; optional correction signal может вернуть orchestration в новый проход | `LogicalContracts.cs:89-169`; `UniversalNineNodeDevelopmentFlow.cs:162-262`; `AdaptiveLogicalOrchestrator.cs:361-443` | flow → unit loop → `TransitionSignals` → `FollowTransitionSignalsAsync` → `RunAsync` | none | Falsify: тесты `AdaptiveLogicalOrchestratorTests.cs:171`, `:235` не проходят; production producer всё ещё отсутствует. |
| Orchestrator async input/output | Реальные branches, parallel independent work, ordered dependent work, aggregate result | `LogicalContracts.cs:174-206`; `AdaptiveLogicalOrchestrator.cs:85-154`, `:445-493`, `:841-875` | `LogicalOrchestratorAssistant.RunAsync` → `_orchestrator.RunAsync` → route → branches → aggregate (`LogicalOrchestratorAssistant.cs:210-280`) | none | Falsify: facade/branch tests at `LogicalOrchestratorAssistantTests.cs:24` and `AdaptiveLogicalOrchestratorTests.cs:93/:136/:171` fail. |
| Dynamic graph product | Не доказан; только deterministic closure/bridge substrate | `DynamicDomainGraph.cs:3-105`; adapter calls `AdaptiveLogicalOrchestrator.cs:522-570`, `:772-837` | Нет definition→implementation→composition→caller chain для DynamicGraph | none | Falsify нынешний `MISSING`: найти production types + DI/manual composition + caller + focused tests для всех обязательных graph invariants; текущий scan дал 0. |

## Явно отложено

| Отложенная область | Статус | Причина |
|---|---|---|
| DPO intake, legal admission, provenance, pair builder | `DEFERRED_NOT_AUDITED` | Не входит в этот foundational audit. |
| Training, checkpoint, fresh-load, continue-training, lifecycle, measurement | `DEFERRED_NOT_AUDITED` | DPO teaching/training должно идти последним, после graph contracts/runtime. |
| Пять worktrees и принятые 20/20 RAG циклы | `ACCEPTED_BASELINE_NOT_REDONE` | Использованы только для маршрутизации owner boundaries; не выданы за code proof. |

## Единственный следующий продуктовый срез

`next_implementation_slice: CORE-DYNAMIC-GRAPH-CONTRACTS-01`

- Цель: создать живой non-training фундамент графа без шестого уровня и без DPO.
- Изменить только: NEW `00.Core/DynamicGraphContracts.cs`, NEW `00.Core/DynamicGraphValidation.cs`, NEW `Tests/DynamicGraphContractsTests.cs`.
- Обязательное поведение: typed node с domain/input/output; публичный candidate output ровно `IQueryable<GraphNodeCandidate>`; edge kinds ровно `FanOut`, `FanIn`, `ManyToMany`, `MultiOutput`, `ReturnLoop`; authority хранится как non-empty id/ref; topology несёт tenant/domain/context/order; validator отклоняет duplicate nodes, dangling endpoints, missing authority, cross-tenant/cross-domain mismatch и cycles, но принимает legal fan-out/fan-in/many-to-many DAG.
- Не делать: `DynamicGraphMachine`, Train/Evaluate/Export/Import, DPO/legal/provider/checkpoint, DI/host wiring.
- Тест: `dotnet test Tests\LlmFromScratch.Tests.csproj --filter FullyQualifiedName~DynamicGraphContractsTests` на машине с .NET 9 SDK.
- Ожидаемое доказательство: code diff в трёх файлах + passing focused test + повторный zero-scan для `TODO|NotImplemented|stub|placeholder` в новых production-файлах.
- Owner: bounded builder sub-agent `core-foundation-contract-builder`, requested model/mode `gpt-5.5` / `xhigh`.
- Почему это продуктовый шаг: он создаёт исполняемые invariant checks и compile-time contracts, от которых зависят flow, orchestrator и DynamicGraph runtime; это не новая audit/status запись.
