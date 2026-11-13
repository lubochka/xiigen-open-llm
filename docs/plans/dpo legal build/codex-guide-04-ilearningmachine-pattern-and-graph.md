# Гайд 04 — Паттерн ILearningMachine и динамический граф (ILearningDynamicGraph)

Автор: Opus 4.8 MAX (роль — автор гайда). Это часть ПЛАНА (документация-гайд),
а не разрешение на реализацию продукта. Все цитаты кода — только реально
существующие, с `file:line`. Цитаты транскриптов/чужих планов — evidence-only.

Скилы, по которым написан гайд (загружены и применены):
`docs/skills/trainable-architecture-decision-SKILL.md` (5 разрешённых уровней,
Graph-Style/DynamicGraph Reconstruction Gate, Legal Teacher Gate, вердикты),
`docs/skills/implementation-doc-hierarchy-SKILL.md` (§5 — уровни Input/Output),
`docs/skills/implementation-doc-uml-SKILL.md` (§0/§1 — `note for` на каждый
класс, симуляции на каждом ребре/ветке), `docs/HOW-TO-USE-SKILLS.md`
(активационные триггеры для graph/DynamicGraph и DPO-провенанса).

---

## 1. Назначение гайда

Этот гайд решает один вопрос плана ILearningDynamicGraph: **как лечь на уже
существующую в коде иерархию обучения** и **где проходит граница «обучаемый
уровень» против «детерминистический хелпер»** — особенно на рёбрах графа, где
сегодня выбор делает детерминистический роутер/скорер.

Конкретно гайд даёт:

1. Маппинг имён из задачи Любы (`ILearningUnit` / `ILearningFlow` /
   `ILearningOrchestrator` / `ILearningDynamicGraph`) на **реально существующие**
   контракты (`ILogicalUnit` / `ILogicalFlow` / `ILogicalOrchestrator` /
   `ILearningMachine<TDomain>`). Имён `ILearning*` из задачи в коде нет — это
   псевдонимы, которые план обязан явно объявить (а не считать новыми уровнями).
2. Правило A-LM-02 (Train* только на `ILearningMachine`) и A-LM-12 (закрытый
   список из 5 разрешённых уровней) — почему граф НЕ может объявить «новый
   обучаемый уровень DynamicGraph», а должен быть `ILearningMachine` поверх графа.
3. Реконструкцию графа по Graph-Style Gate: узлы, рёбра, fan-out, fan-in,
   many-to-many, async, и **владелец финального авторитета на каждом ребре**.
4. Классификацию каждого роутера/скорера на ребре: обучаемый уровень против
   внутреннего детерминистического хелпера, и где сейчас риск
   `FORBIDDEN_RUNTIME_SHORTCUT`.

Что гайд НЕ делает: не реализует код, не запускает раннеры, не строит DPO-наборы
(это другие гайды/фазы), не редактирует чужие `claude-*`/`dcm-*` артефакты.

---

## 2. Концепт простыми словами (аналогия, без кода)

Представь **завод** (это домен — например, перевод или project-wiki).

- **Станок** = маленький рабочий узел. Берёт деталь на входе, выдаёт деталь на
  выходе. Сам станок НЕ учится во время смены — он просто работает по своей
  настройке. Это уровень «юнит» (`ILogicalUnit`).
- **Конвейерная линия** = последовательность станков, соединённых лентами.
  Линия знает порядок и как деталь идёт от станка к станку. Это «поток»
  (`ILogicalFlow`).
- **Начальник цеха** = распределяет заказы по линиям, решает, какую линию
  включить под конкретный заказ, и собирает итог. Это «оркестратор»
  (`ILogicalOrchestrator`).
- **Директор завода с отделом обучения** = единственный, у кого есть право
  останавливать завод на ночь и **перенастраивать станки по результатам**
  (что приняли, что забраковали). Директор оборачивает начальника цеха и
  добавляет цикл обучения. Это «обучающая машина» (`ILearningMachine`).

Жёсткое правило завода (A-LM-02): **перенастраивать станки имеет право только
директор (машина), и только ночью (в режиме обучения).** Станок, линия и
начальник цеха днём — чистая работа без самообучения. Если станок «сам себя
подкручивает» во время смены — это нарушение.

Теперь «динамический граф». Обычный завод — это фиксированный конвейер. **Граф**
— это завод, где **ленты между станками можно перекладывать на лету**: один
станок может отдать деталь сразу нескольким (это **fan-out**, веер наружу),
несколько станков могут сдать свои детали в одну точку сборки (это **fan-in**,
веер внутрь), и есть **петля возврата** (контролёр забраковал — деталь едет
обратно на нужный станок). «Динамический» здесь = карта лент (топология) хранится
как **данные** в реестре и редактируется в рантайме, а директор/начальник цеха
эту карту читают и обходят.

Самое опасное место графа — **развилка ленты**: «кому отдать деталь дальше?».
Если на развилке решает тупой счётчик («раньше чаще отдавали налево, значит и
сейчас налево») или жёсткая таблица ключевых слов — это **детерминистический
shortcut**. Он допустим только как **черновой генератор кандидатов** или
**baseline для сравнения**, но финальное «кому отдать» в обучаемом графе должен
принимать **обученный** узел, а не таблица. Именно это правило — центр гайда.

Легальность «учителя» (Legal Teacher Gate): если завод учится у внешнего эксперта
(модель-учитель для DPO/дистилляции), эксперт должен быть **легальным**
(open-weight/permissive, лицензия проверена). Закрытые проприетарные модели
(Claude/OpenAI/Gemini) **нельзя** использовать как источник «правильных ответов»
для обучения — только как evidence/обсуждение.

---

## 3. Как это сейчас реализовано в нашем коде

Раздел даёт точные `file:line`, что **РАБОТАЕТ**, и что **ЗАГЛУШКА/未-обучено**.
Главный факт-вывод: иерархия 5 уровней **существует как контракты**, граф-субстрат
(узлы/рёбра, обход) **существует как детерминистический код**, но **обучаемого
графа (trained DynamicGraph) пока нет** — есть честный under-trained скелет.

### 3.1. Пять уровней иерархии (контракты — РАБОТАЮТ как контракты)

`C:/Projects/llm_mvp_core/00.Core/LogicalContracts.cs`:
- `interface ILogicalUnit<TInput, TOutput>` — LogicalContracts.cs:97; единственный
  метод `ExecuteAsync(LogicalUnitRequest<TInput>) -> LogicalUnitResult<TOutput>`
  (строки 99-101). Чистый inference. Метода Train НЕТ.
- `interface ILogicalFlow<TInput>` — LogicalContracts.cs:166; `ExecuteAsync(
  LogicalFlowRequest<TInput>) -> LogicalFlowResult` (строки 168-170). Train НЕТ.
- `interface ILogicalOrchestrator` — LogicalContracts.cs:202; `RunAsync(
  LogicalOrchestratorRequest) -> LogicalOrchestratorResult` (строки 204-206).
  Train НЕТ.

`C:/Projects/llm_mvp_core/00.Core/LearningMachineContracts.cs`:
- `interface ILearningMachine<TDomain>` — LearningMachineContracts.cs:208. Несёт
  **именно** методы обучения и жизненного цикла:
  `TrainBatchAsync` (212-213), `EvaluateAsync` (217-218), `ExportAsync` (222-223),
  `ImportAsync` (227-228), `GetOrchestratorAsync` (232-233).
- `interface ILearningMachinePair<TPersonal, TGlobal>` —
  LearningMachineContracts.cs:245; `Personal`/`Global` (248/251),
  `SyncPersonalToGlobalAsync` (256), `PullGlobalToPersonalAsync` (260).

Правило A-LM-02 зафиксировано **комментарием в самом файле**:
`LearningMachineContracts.cs:8` — «Train() is ONLY on ILearningMachine — never on
ILogicalUnit or ILogicalFlow». Проверено чтением: ни в одном из трёх нижних
интерфейсов метода Train нет. Путь обучения описан там же комментарием
(LearningMachineContracts.cs:204-207): `TrainBatchAsync` → внутри вызывает
`ILogicalCapabilityTrainer.TrainAndRegisterAsync()` → существующие
`*DpoGenerator + *TrainingRunner` на каждый юнит.

Статус: **РАБОТАЕТ как контракт-каркас.** Это РОВНО пять разрешённых уровней по
A-LM-12 (`DECISIONS-LOCKED.md:35`). Контракт-only ≠ доказательство обученности.

### 3.2. DPO-формат и метрики (РАБОТАЮТ как контракты данных)

- `record LearningTrainingItem(ItemId, Prompt, ChosenResponse, RejectedResponse)`
  — LearningMachineContracts.cs:30-39. Это и есть **готовая DPO-тройка**
  (preference triple), которую авто-построитель графа должен наполнять.
- `record LearningBatch<TDomain>(BatchId, DomainTag, TrainingItems, SourceIds)`
  — LearningMachineContracts.cs:18-27. `SourceIds` (строка 26) — единственный
  крючок провенанса на уровне контракта.
- `record LearningBatchResult(...)` — LearningMachineContracts.cs:43-62:
  `RowsTrained, AccuracyBefore, AccuracyAfter, RewardMargin, CpuMs, PeakRamMb,
  ElapsedMs, PromotedUnitIds`. Комментарий 41-42: «no placeholder or UNKNOWN in
  production». Это закрывает паттерн completion-evidence (численные метрики).
- `record LearningEvaluationResult(... Accuracy, RewardMargin, PassesGate,
  GateNote)` — LearningMachineContracts.cs:90-103. `PassesGate` (строка 100) —
  это trainable-эквивалент runner-гейта.
- `record LearningMachineExport(... CheckpointRef, ManifestRef, AccuracyAtExport,
  ExportedAtUtc)` — LearningMachineContracts.cs:112-127. `CheckpointRef`/
  `ManifestRef` указывают в 25.ModelServer (комментарий 110).

Статус: **РАБОТАЕТ как контракт.** Реальной обученной машины, которая это
заполняет численно, на уровне 00.Core нет (это контракты).

### 3.3. Граф-субстрат: узлы/рёбра (РАБОТАЕТ, но детерминистический)

`C:/Projects/llm_mvp_core/07.GraphEngine`:
- `record GraphNode { Id; Name; Type; FilePath; Signature; RawText; float[]
  Embedding; double PageRankScore {get;set;} }` — GraphNode.cs:3-13. Единственное
  мутируемое поле — `PageRankScore` (строка 12).
- `record GraphEdge { From; To; RelationType; double Weight = 1.0 }` —
  GraphEdge.cs:3-9. **Вес ребра задаётся вручную (init), значение по умолчанию
  1.0, неизменяем — это НЕ обучаемый вес** (строка 8).
- `PageRankCalculator` (PageRank/Personalised PageRank) и `KnowledgeGraph`
  (контейнер с двусторонней смежностью out/in) — детерминистическая математика.

Статус: **РАБОТАЕТ как структура данных + детерминистический ранкер.** Обучения,
DPO, reward/loss, chosen/rejected здесь НЕТ. Это удобный «скелет графа», но не
обучаемый граф (см. recon `codex-recon-core-07-graph.md`).

### 3.4. Динамика топологии: реестр + mutation-юниты (РАБОТАЕТ, детерминистично)

`C:/Projects/llm_mvp_core/17.DynamicDomains/DynamicDomainGraph.cs`:
- `record DynamicDomainTransition(string FromId, string ToId)` — строка 3 (одно
  ребро графа суб-фаз).
- `static class DomainGraphClosure` (строки 6-63): `HasAnyDependency` (8),
  `BuildPredecessorMap` (15) — транзитивное замыкание DFS.
- `static class OrchestratorExpander` (строки 66-115): `FindBridgeSubPhases` (68)
  — поиск «мостовых» узлов через BFS-кратчайший путь `FindPath` (90).

«Динамика на лету» = реестр топологии редактируется в рантайме mutation-юнитами
(`C:/Projects/llm_mvp_core/21.AgentInfrastructure/Logical/`:
`AddTransitionTopologyMutationUnit.cs`, `AddSubPhaseTopologyMutationUnit.cs`,
`AddPhaseTopologyMutationUnit.cs`, `AddDomainTopologyMutationUnit.cs`), а 17
пересчитывает зависимости/порядок/мосты при каждом вызове.

Статус: **РАБОТАЕТ как детерминистический граф-калькулятор.** Это рабочий
прецедент «динамической структуры», но без обучаемого выбора рёбер.

### 3.5. Рантайм-оркестратор и рёбра графа (РАБОТАЕТ; авторитет детерминистический)

`C:/Projects/llm_mvp_core/21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs`
(`: ILogicalOrchestrator`, строка 10):
- `RunAsync` — строка 85 (вход `LogicalOrchestratorRequest`).
- Веер наружу (fan-out) делается роутером: `RouteAsync` (строка 316) вызывает
  `_multiRouter.ExecuteAsync(...)` (строка 326) и расширяет решения
  `ExpandRoutedDecisions` (строка 340).
- Исполнение одного ребра: `ExecuteBranchAsync` (строка 496). Confidence-floor
  гейт: `if (decision.KnowledgeEdge)` → синтетический «knowledge edge» вместо
  выдуманного выбора потока (строки 503-505).
- Порядок рёбер по зависимостям: `HasTransitionDependency` (522) и
  `OrderByTransitions` (541) используют детерминистический
  `DomainGraphClosure.HasAnyDependency/BuildPredecessorMap` из проекта 17
  (строки 531, 552), плюс мосты `OrchestratorExpander.FindBridgeSubPhases`
  (строка 778).
- Сбор веера внутрь (fan-in): `BranchResults` в `LogicalOrchestratorResult`
  (LogicalContracts.cs:190; оркестратор пишет туда, напр.
  AdaptiveLogicalOrchestrator.cs:311, 872), плюс контекст между ветками —
  `LogicalContextPacket`/`LogicalContextFragment` (LogicalContracts.cs:146-163) и
  `PhaseTransitionSignal` (LogicalContracts.cs:89) — петля возврата
  (`FollowTransitionSignalsAsync`, строка 361).

КТО СЕЙЧАС РЕШАЕТ НА РЕБРЕ (важно): финальный выбор «какие суб-фазы и в каком
порядке» сегодня принимает **детерминистический** `MultiSubPhaseRouterUnit`
(см. 3.6), а порядок — детерминистический граф-обход из 17. Это **honest
deterministic authority**, не обучаемая семантика.

Статус: **РАБОТАЕТ; авторитет на рёбрах детерминистический** (под-обучен).

### 3.6. Роутер на ребре — главный кандидат на shortcut (РАБОТАЕТ, детерминистично)

`C:/Projects/llm_mvp_core/21.AgentInfrastructure/Logical/MultiSubPhaseRouterUnit.cs`
(`: ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>`,
строка 8):
- Жёсткая таблица якорных слов `SubPhaseAnchorTerms` (строки 23-56) — захардкожен
  словарь `subPhaseId -> string[]` ключевых слов.
- Метод `Score(...)` (строки 249-288) — **ручная взвешенная формула** с
  фиксированными весами, напр. строка 283:
  `(predicateScore * 0.55) + (exactSubPhaseMention * 0.25) + (tokenScore * 0.20)
  + (hybridScore * 0.02)`.
- Внутри — детерминистический `HybridRanker` (BM25+KNN fusion) как маленький
  tie-breaker (строки 340-364) и `Tokenize` (482).

Регистрируется как `isLearned:false`, `Trusted` (см. recon
`codex-recon-core-15-logical.md`): то есть сегодня это **честно помеченный
baseline-роутер**, не «семантический обученный». Но именно он — финальная власть
веера наружу.

Статус: **РАБОТАЕТ как детерминистический selector/scorer.** Семантической
claim'а нет → нарушения сейчас нет. Это **кандидат №1 на замену обучаемым
юнитом** в плане графа (иначе при будущей claim «умного выбора» = shortcut).

### 3.7. Обучаемый юнит-прецедент: KnnLogicalUnit (РАБОТАЕТ; нюанс Train)

`C:/Projects/llm_mvp_core/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs`
(`: ILogicalUnit<TInput, TOutput>`, строка 33):
- Статическая фабрика `Train(...)` — строка 98 (BPE-токенайзер + HNSW-индекс из
  обучающих примеров). ВАЖНО: это **construction-time фабрика**, а НЕ метод
  `TrainBatchAsync` на интерфейсе. Сам `ILogicalUnit.ExecuteAsync` остаётся
  inference-only (строка 197). Это **не нарушает A-LM-02**: интерфейс юнита
  по-прежнему без Train-метода; обучение инкапсулировано в фабрике, лайфцикл
  батч-обучения принадлежит уровню машины.
- Инференс: `ExecuteAsync` → `_index.Search(query, _k)` (218), «ближайший сосед
  как предсказание»: `var best = hits[0]` (строка 223), confidence из score
  соседа (224), warning при пробитии порога `_confidenceFloor` (228).
- Манифест модели `LogicalKnnModelManifest` (строки 18-30) с ингредиентами.

Статус: **РАБОТАЕТ как обучаемый KNN-юнит-компонент.** По скилу KNN —
audit-triggered, **не запрещён**: это нормальный model component/ретривер
**внутри** разрешённого уровня. Запрещено только сделать standalone-KNN
**всей** обучаемой архитектурой или **немаркированной финальной властью** там,
где требуется обученный юнит.

### 3.8. Bootstrap-узлы оркестрации (РАБОТАЮТ как честный under-trained scaffold)

`C:/Projects/llm_mvp_core/42.ProjectWikiDomain`:
- 6 интерфейсов в `OrchestrationUnitContracts.cs:76-107`, каждый `: ILogicalUnit`.
- 6 Bootstrap-классов в `BootstrapOrchestrationUnits.cs`
  (`BootstrapIntentBuilderUnit` и др.), `Confidence=1.0`, делегируют статике.
- Секвенсор 6 шагов: `TaskOrchestrationUnit.OrchestrateAsync` в
  `ProjectTaskOrchestrationPipeline.cs` (линейный DAG 1→2→3→4→5→6,
  аккумулирующий fan-in, fan-out нет).
- Flow-адаптеры `ProjectDocumentationPipelineFlowAdapter.cs` /
  `ProjectWikiTrainingFlowAdapter.cs` (`: ILogicalFlow<string>`) честно отдают
  `DOCUMENTATION_GRAPH_READY_TRAINING_RUNNER_MISSING` / `training_status=
  runner_missing` и метрики `UNKNOWN` в registry-режиме (антимаскарад).

Регистрация уровней — `15.OrchestratorModel/Logical/LogicalOrchestratorAssistant.cs`
(`CreateDefault`): `InMemoryLogicalFlowRegistry.Register(...)`, сидинг топологии
`DevelopmentLifecycleTopologySeeder.Seed` (7 фаз/31 суб-фаза), конструирование
`AdaptiveLogicalOrchestrator`. Реестр выбирает **новейший** дескриптор
(`latest-wins` по `RegistrationOrder`) — так trained-юнит может заменить Bootstrap
(A-LM-03).

Статус (двух-PASS из CLAUDE.md): сейчас это `PASS_AS_HONEST_UNTRAINED_SCAFFOLD`,
**не** `PASS_AS_TRAINED_FEATURE`. Скрытого детерминизма «под видом trained» нет:
нет checkpoint/manifest и заявленных метрик в самих юнитах.

### 3.9. Сводка статуса (что есть / чего нет)

| Слой | Файл (ключевой) | Статус |
|---|---|---|
| 5 уровней (контракты) | LogicalContracts.cs:97/166/202; LearningMachineContracts.cs:208/245 | РАБОТАЕТ как контракт |
| DPO-формат + метрики | LearningMachineContracts.cs:30,43,90,112 | РАБОТАЕТ как контракт |
| Граф узлов/рёбер | 07.GraphEngine/Models/GraphNode.cs:3, GraphEdge.cs:3 | РАБОТАЕТ, детерминистический; вес не обучаем |
| Динамика топологии | 17.DynamicDomains/DynamicDomainGraph.cs:3,6,66 | РАБОТАЕТ, детерминистический калькулятор |
| Оркестратор/рёбра | AdaptiveLogicalOrchestrator.cs:85,316,496 | РАБОТАЕТ; авторитет рёбер детерминистический |
| Роутер веера | MultiSubPhaseRouterUnit.cs:8,249,283 | РАБОТАЕТ; selector/scorer baseline (кандидат на замену) |
| Обучаемый KNN-юнит | KnnLogicalUnit.cs:33,98,223 | РАБОТАЕТ как model component (KNN ок внутри уровня) |
| Bootstrap-узлы 42 | BootstrapOrchestrationUnits.cs; TaskOrchestrationUnit | РАБОТАЕТ как honest untrained scaffold |
| `ILearning*` из задачи | — | ОТСУТСТВУЕТ (имён нет в коде → псевдонимы плана) |
| Контракты РЁБЕР графа (fan-out/fan-in/many-to-many/per-edge authority/async) | — | ЧАСТИЧНО/ОТСУТСТВУЕТ как явные контракты (см. §7) |
| Teacher/provenance поля в DPO-строке | — | ОТСУТСТВУЕТ (есть только `SourceIds`) |
| Trained DynamicGraph (checkpoint/ablation/fresh-load) | — | ОТСУТСТВУЕТ |

---

## 4. Пошаговый рецепт / алгоритм

Рецепт «как лечь на иерархию и реконструировать граф», по шагам. Это **план
действий для плана**, не исполнение.

### Шаг 0. Зафиксировать маппинг имён (обязательно первым)

Имён `ILearningUnit/ILearningFlow/ILearningOrchestrator/ILearningDynamicGraph` в
коде НЕТ. План обязан объявить их **псевдонимами существующих уровней**, а не
новыми разрешёнными уровнями (A-LM-12 — закрытый список).

| Имя из задачи Любы | Реальный контракт | Уровень | Файл:line |
|---|---|---|---|
| `ILearningUnit` | `ILogicalUnit<TInput,TOutput>` | L2 (узел) | LogicalContracts.cs:97 |
| `ILearningFlow` | `ILogicalFlow<TInput>` | L3 (подграф/линия) | LogicalContracts.cs:166 |
| `ILearningOrchestrator` | `ILogicalOrchestrator` | L4 (обход графа) | LogicalContracts.cs:202 |
| `ILearningDynamicGraph` | `ILearningMachine<DynamicGraphDomain>` | L5 (обучение поверх графа) | LearningMachineContracts.cs:208 |

Обоснование L5 для графа: по A-LM-01 (`DECISIONS-LOCKED.md` A-LM-01)
`ILearningMachine` стоит НАД оркестратором и добавляет цикл обучения; по A-LM-02
Train* живёт только на машине. Значит «обучаемый граф» = `ILearningMachine`,
который через `GetOrchestratorAsync()` (LearningMachineContracts.cs:232) отдаёт
`ILogicalOrchestrator`, обходящий узлы/рёбра. **DynamicGraph НЕ становится новым
обучаемым уровнем** — он становится доменом `TDomain` поверх существующего L5.

Если план хочет именно НОВЫЙ архитектурный уровень «DynamicGraph» — это
decision-reopening (A-LM-12 закрыт), и требуется явное решение Любы. По умолчанию
— псевдоним поверх L5.

### Шаг 1. Реконструировать граф по Graph-Style Gate

Заполнить (это требование скила, раздел «Graph-Style / DynamicGraph
Reconstruction Gate»):

```
roles_and_hierarchy:        L5 ILearningMachine<DynamicGraphDomain> -> L4 orchestrator -> L3 flow -> L2 units
DynamicGraph nodes:         L2 ILogicalUnit (узлы-паттерны/станки)
DynamicGraph edges:         переходы (DynamicDomainTransition / topology registry transitions)
input_contracts:           LogicalUnitRequest<TInput>  (LogicalContracts.cs:62)
output_contracts:          LogicalUnitResult<TOutput>  (LogicalContracts.cs:72)
many_to_many_outputs:      сейчас НЕТ явного контракта на ребре (создать, см. §7)
fan_out:                   RouteAsync -> _multiRouter (AdaptiveLogicalOrchestrator.cs:316,326)
fan_in:                    BranchResults[] + LogicalContextPacket (LogicalContracts.cs:190,158)
multi_output_edge_contracts: ОТСУТСТВУЮТ (создать typed edge record, §7)
async_graph_execution:     ExecuteAsync/RunAsync уже async; параллельный обход рёбер НЕ закреплён контрактом
deterministic_vs_trainable_authority_on_edges: см. §6 (таблица per-edge)
hidden_deterministic_shortcut_check_tied_to_each_edge: см. §6 (для каждого ребра)
```

### Шаг 2. Для КАЖДОГО ребра назвать владельца финального авторитета

По скилу: каждое ребро обязано назвать, кто принимает финал — детерминистический
helper, trained unit, flow, orchestrator, machine или pair. Скрытый
детерминистический shortcut на ребре = `FORBIDDEN_RUNTIME_SHORTCUT`, если он не
помечен явно как eval-only/helper/baseline с ablation. См. готовую таблицу в §6.

### Шаг 3. Развернуть L5-обучение поверх графа (не на узлах)

- Узлы/рёбра/flow/orchestrator — чистый inference (A-LM-02).
- Обучение — `ILearningMachine.TrainBatchAsync` (LearningMachineContracts.cs:212),
  внутри → `ILogicalCapabilityTrainer.TrainAndRegisterAsync`
  (`21.AgentInfrastructure/Logical/LogicalCapabilityTrainingContracts.cs`).
- После публикации валидированных весов trained-юнит заменяет Bootstrap/baseline
  через `latest-wins` реестра (A-LM-03).

### Шаг 4. Лайфцикл обучаемого узла/графа (обязателен в плане)

По скилу `ILogicalUnit` lifecycle checklist:
1. загрузить user/cache checkpoint первым;
2. загрузить repo/common checkpoint вторым (fallback);
3. честный untrained/scaffold-статус, если нет ни одного
   (как сейчас `runner_missing`);
4. backup/export/import/share без потери провенанса
   (`ExportAsync`/`ImportAsync`, LearningMachineContracts.cs:222/227);
5. continue-training **от существующего** checkpoint, не с нуля;
6. common и user-adaptive ноги раздельно (`ILearningMachinePair`,
   LearningMachineContracts.cs:245) — если домен этого требует.

### Шаг 5. Гейты измерения и анти-leakage (см. §6)

Применить D-17 (exactPromptLeakageRate=0.0), ablation/no-shortcut, fresh-load,
continue-training; численные метрики из `LearningBatchResult`/
`LearningEvaluationResult` (без UNKNOWN в production).

### Шаг 6. Легальный teacher-гейт для DPO (см. §6)

Заполнить provenance-поля **до** того, как строка станет chosen/rejected. Закрытые
проприетарные выводы → `DATASET_REBUILD_REQUIRED`.

---

## 5. Конкретные примеры с реальными значениями

### 5.1. Пример: выбор владельца авторитета на ребре fan-out

Сценарий: пользователь пишет «исправь падающий тест в backend-сервисе». Веер
наружу должен выбрать суб-фазы.

Сейчас (детерминистически): `MultiSubPhaseRouterUnit.Score` (строка 283) считает
по якорным словам `SubPhaseAnchorTerms` (строки 23-56). Для «test/fix/bug/error»
сработают якоря `4.4` (`refactor/fix/bug/error/log`) и `5.1`
(`unit/test/fix/bug/error/log`). Финал — детерминистическая сумма весов.

В обучаемом графе: финал на этом ребре должен принимать обученный
`ILogicalUnit` (вход — семантические признаки промпта БЕЗ индекса кандидата и
БЕЗ source-path; выход — список рёбер с уверенностью). Якорный словарь и
`HybridRanker` остаются **генератором кандидатов/baseline для ablation**, не
финальной властью.

### 5.2. Пример DPO-тройки для обучения роутера ребра (формат `LearningTrainingItem`)

Тип: `LearningTrainingItem(ItemId, Prompt, ChosenResponse, RejectedResponse)`
(LearningMachineContracts.cs:30-39). Значения — иллюстративные (НЕ из закрытого
учителя):

```
ItemId:          edge-route-0007
Prompt:          "исправь падающий unit-тест в backend-сервисе оплаты"
ChosenResponse:  "route -> [5.1 unit-tests (conf 0.86), 4.4 refactor/fix (conf 0.74)]"
RejectedResponse:"route -> [4.2 frontend (conf 0.81)]   [WRONG: выбрано по слову
                  'сервис', проигнорирован сигнал 'unit-тест/падающий' → утечка
                  поверхностного токена вместо смысла задачи]"
```

Чему учит: на ребре fan-out выбирать суб-фазу по **смыслу задачи** (тест/баг), а
не по случайному совпадению токена («сервис» → frontend). `[WRONG:...]` явно
называет ошибочный сигнал — это и есть rejected-плечо.

### 5.3. Пример rejected-плеча против leakage индекса кандидата

```
ItemId:          edge-route-0042-leak
Prompt:          "добавь интеграционный тест на хранилище"
ChosenResponse:  "route -> [5.2 integration-tests]"
RejectedResponse:"route -> [первый кандидат в списке]   [WRONG: выбран по позиции
                  в списке кандидатов (candidate index), а не по смыслу; нарушает
                  permutation-invariance — при перестановке кандидатов выбор
                  меняется]"
```

Чему учит: запрет shortcut «бери первого/по позиции». Это прямой аналог
`CandidateOrderLeakageGuard`-логики из скила (раздел про `_candidatePositionWeights`).

### 5.4. Пример quarantine-строки (нелегальный teacher)

```
row_source:      rollout-2026-05-03T...jsonl  (provider=openai, "GPT-5", Codex Desktop)
leakage/legal:   closed_proprietary_model=true; teacher_output_used_for_training=true
verdict:         DATASET_REBUILD_REQUIRED  (QUARANTINE_DO_NOT_TRAIN)
rebuild_action:  не использовать как chosen/rejected; перестроить пары из
                 человеческих промптов Любы или из легального open-weight/permissive
                 учителя; закрытый вывод допустим только как evidence/обсуждение.
```

(Источник факта о rollout-логе — recon `codex-recon-mvp-local-wiki.md` §5.)

### 5.5. Пример честного untrained-ответа узла (как сейчас)

`ProjectDocumentationPipelineFlowAdapter` при пройденном coverage возвращает
`OverallVerdict = DOCUMENTATION_GRAPH_READY_TRAINING_RUNNER_MISSING`,
`Metrics["training_status"] = "runner_missing"`. Это `PASS_AS_HONEST_UNTRAINED_
SCAFFOLD`: узел честно говорит «я ещё не обучен», не выдумывая accuracy/loss.

---

## 6. Гейты и правила (применимые к теме)

### 6.1. Per-edge authority + hidden-shortcut (классификация рёбер графа)

Таблица: для каждого ребра — кто решает сегодня, и вердикт по скилу.

| Ребро (что делает) | Файл:line | Механизм сейчас | Владелец авторитета сейчас | Вердикт по скилу | Что должно стать в графе |
|---|---|---|---|---|---|
| Fan-out: prompt → суб-фазы | AdaptiveLogicalOrchestrator.cs:316,326; MultiSubPhaseRouterUnit.cs:249,283 | Ручная взвешенная формула + словарь якорей | Детерминистический `MultiSubPhaseRouterUnit` (`isLearned:false`) | `OK_INTERNAL_DETAIL` сейчас (claim'а нет); при claim «умного выбора» → `FORBIDDEN_RUNTIME_SHORTCUT` | Обученный `ILogicalUnit` (вход без candidate-index/source-path); словарь+`HybridRanker` = baseline/ablation |
| Single-route fallback | AdaptiveLogicalOrchestrator.cs:344; SubPhaseRouterUnit.cs:33 | Скоринг/ближайший-неверный | Детерминистический `SubPhaseRouterUnit` | `OK_INTERNAL_DETAIL` / `BASELINE_MUST_BE_LABELLED` | то же — обученный выбор, детерминистика как baseline |
| Порядок рёбер (зависимости) | AdaptiveLogicalOrchestrator.cs:531,552; DynamicDomainGraph.cs:8,15 | Граф-замыкание DFS над топологией | Детерминистический `DomainGraphClosure` (проект 17) | `OK_INTERNAL_DETAIL` (обход без claim) | допустимо детерминистично, ЕСЛИ выбор «есть ли зависимость» не объявлен обучаемым; иначе обученный предиктор зависимости + Jaccard как baseline |
| Вставка мостов | AdaptiveLogicalOrchestrator.cs:778; DynamicDomainGraph.cs:68 | BFS-кратчайший путь | Детерминистический `OrchestratorExpander` | `OK_INTERNAL_DETAIL` | то же |
| Confidence-floor → knowledge edge | AdaptiveLogicalOrchestrator.cs:503-505 | Порог уверенности | Детерминистический гейт (честный «не знаю») | `OK_INTERNAL_DETAIL` (антимаскарад) | оставить как honest-degraded |
| Ранжирование кандидатов (память) | KnnLogicalUnit.cs:218,223 | HNSW KNN, ближайший сосед | KNN-компонент внутри юнита | `OK_MODEL_COMPONENT` (KNN ок внутри уровня) | оставить как model component/ретривер; не делать standalone-финалом |
| Latest-wins выбор версии узла | InMemoryLogicalUnitRegistry (`ResolveLatest`) | `OrderByDescending(RegistrationOrder)` | Детерминистический tie-break | `OK_INTERNAL_DETAIL` как выбор версии | НЕ делать «latest by registration order» финальной властью ребра/выбора узла (иначе shortcut) |

Правило (A-LM-09, `DECISIONS-LOCKED.md:32`): финальную семантику принимает
обучаемый уровень (`DeterministicFinalAuthority=false`); BM25/regex/if-else —
только bootstrap-генераторы кандидатов, не продакшн-финал. D-15: priority решает
«что пробовать», KNN — память, арбитр — «что принято»; ничто из priority/KNN/
budget/external-AI не переопределяет отклонение арбитра.

### 6.2. Анти-leakage (обязательно для DPO/eval графа)

- D-17: `exactPromptLeakageRate` обязан быть 0.0 во всех eval-гейтах; eval-промпт,
  точно совпадающий с train-промптом → `PassesGate=false`; exclusion-without-
  failure запрещён.
- Запрещённые сигналы на ребре: candidate-index/позиция в списке (см. 5.3),
  source-path как scoring-фича (в `MultiSubPhaseRouterUnit` путь уже исключён из
  скоринга — `BuildPathFreeScoringText`, строка 291, и держится только как
  provenance-warning, строки 367-408 — это правильный образец), source-id/exact-
  prompt replay.
- Ablation/no-shortcut: при наличии детерминистического хелпера нужен прогон без
  него, доказывающий, что обученный юнит, а не shortcut, держит качество.

### 6.3. Легальный teacher-гейт (Legal Teacher / Provenance Gate)

Обязательные поля до использования строки в обучении:

```
teacher_access_mode:
hosting_provider:
model_id:
model_license:
provider_terms_checked:
teacher_output_used_for_training:
closed_proprietary_model:
request_id/model_version (если есть):
```

Если `closed_proprietary_model=true` и `teacher_output_used_for_training=true` →
вердикт всегда `DATASET_REBUILD_REQUIRED`. Claude/OpenAI/Gemini/Gemini Grounding
**запрещены** как distillation-учителя. Разрешены только размещённые
open-weight/permissive после проверки лицензии/условий (предпочтительно MIT/
Apache-2.0). Приватные промпты/чаты/локальные файлы Любы тренируют **user-
adaptive** ногу, если не редактированы и не одобрены для common (A-LM-08, R5).

### 6.4. Запрет скрытого детерминизма (R4 / A-LM-12 / A-LM-13)

- A-LM-12 (`DECISIONS-LOCKED.md:35`): scorer/selector/classifier/router/retriever/
  detector, KNN/Jaccard/BM25, dictionary/counter, regex/keyword/pattern matcher,
  `TryDecide*`/`ChooseBestPattern`/`SelectBestPattern`/`CountMatchingSignals` —
  это **детали реализации**, не новые уровни. Helper допустим только **внутри**
  разрешённого уровня и помечен честно (internal/baseline).
- A-LM-13 (`DECISIONS-LOCKED.md:36`): если код/доки называют «семантические юниты»,
  реализованные лишь как секции чекпойнта/словари/счётчики/scoring-методы —
  план/ревью обязаны классифицировать каждый элемент: настоящий `ILogicalUnit` |
  секция чекпойнта | baseline | запрещённый shortcut | слишком сильная claim.
- Удаление детерминистического хелпера ≠ покрытие обучаемой замены: план обязан
  назвать trainable unit, typed trained state, checkpoint/manifest, export/import,
  locator, fresh-load probe, continue-training probe, ablation/leakage gate.

### 6.5. Гейты измерения (completion-evidence)

- Численные метрики из `LearningBatchResult` (LearningMachineContracts.cs:43) и
  `LearningEvaluationResult` (строка 90) — без UNKNOWN/static/validation-only для
  claim «trained feature».
- Две легитимные PASS-точки (CLAUDE.md): `PASS_AS_HONEST_UNTRAINED_SCAFFOLD`
  (контракт честно говорит not-ready, без скрытого детерминизма; доказано
  тестами) и `PASS_AS_TRAINED_FEATURE` (checkpoint + held-out метрики + ablation
  + fresh-load + continue-training + e2e через реальный хост). Репорт scaffold'а
  как trained-feature = masquerade (самый опасный false-positive).

---

## 7. Пробелы / что нужно построить в плане

Перечень того, чего НЕТ в коде и что план обязан спроектировать (это НЕ
исполнение, а список дыр):

1. **Имена `ILearning*` из задачи отсутствуют.** План обязан зафиксировать
   маппинг §4 Шаг 0 как псевдонимы, а не новые уровни (A-LM-12 закрыт). Введение
   `DynamicGraph` как НОВОГО архитектурного уровня = decision-reopening, требует
   явного решения Любы.

2. **Контракты РЁБЕР графа отсутствуют как явные типы.** Сегодня ребро — это
   `DynamicDomainTransition(FromId, ToId)` (DynamicDomainGraph.cs:3) или
   topology-transition в реестре; нет typed-контракта на:
   - `multi_output_edge` (одно ребро → несколько типизированных выходов),
   - `fan_in` слияние (контракт агрегации; сейчас есть только `BranchResults[]`
     и `LogicalContextPacket`, но без правил many-to-many merge),
   - `per_edge_authority` (поле «кто финальный решатель этого ребра»),
   - `async_graph_execution` (контракт параллельного обхода рёбер; методы async,
     но граф-параллелизм не закреплён).
   План должен ввести типизированную запись ребра (typed trained state, а не
   сырой `Dictionary`).

3. **Финальный авторитет на ребре fan-out сегодня детерминистический.**
   `MultiSubPhaseRouterUnit.Score` (строки 249-288) с фиксированными весами и
   словарём `SubPhaseAnchorTerms` (23-56). План обязан назвать обученный
   `ILogicalUnit`-замену (вход без candidate-index/source-path), оставив словарь/
   `HybridRanker` как baseline/ablation. Иначе при любой claim «умного выбора» —
   `FORBIDDEN_RUNTIME_SHORTCUT`.

4. **Teacher/provenance поля в DPO-строке отсутствуют.** `LearningTrainingItem`
   (LearningMachineContracts.cs:30) несёт только `Prompt/Chosen/Rejected`;
   `LearningBatch` — только `SourceIds` (строка 26). Для «dpo legal build» план
   должен решить, где хранить поля Legal Teacher Gate (на строке, батче или
   манифесте) — иначе авто-построенные DPO не пройдут шлюз.

5. **Лайфцикл trained-замены не реализован для 6 оркестрационных Bootstrap-узлов.**
   A-LM-03 описывает только слот подмены; load user→repo fallback→export/import→
   continue-training в коде для этих узлов не виден. Нужны DPO-генераторы/раннеры
   именно для оркестрационных узлов (в 42 есть генераторы для wiki-юнитов, но для
   6 Bootstrap-оркестрационных отдельных не обнаружено — проверить/построить).

6. **Обучаемый предиктор зависимостей/рёбер vs детерминистический Jaccard.**
   E49/E50 (`35.ExperimentsV2`) выводят рёбра по Jaccard-порогу 0.35, но это
   эксперимент-симуляция, не подключённая к рантайм-графу 17. Если план объявит
   вывод рёбер обучаемым — Jaccard становится baseline для ablation, а финал —
   обученный `ILogicalUnit`.

7. **Размещение common vs adaptive ассетов графа не зафиксировано.** Прецедент —
   A-LM-10 (`.xiigen/smart-skills/` для user-leg; `models/`+`training-data/` для
   common). Для DynamicGraph конкретная директория/манифест/локатор не определены
   (`25.ModelServer`, `IModelRepository`, `IRemoteUnitLocator`).

8. **Тесты untrained-честности и no-shortcut.** План должен задать тесты, что
   узлы честно `not_ready` и не прячут детерминизм (permutation-invariance,
   no-position-authority, no-source-id-leakage, fresh-load, continue-training,
   D-17 exact-prompt leakage = 0.0).

9. **Статус «новый уровень DynamicGraph» в DECISIONS-LOCKED.md не закреплён.**
   В реестре locked-решений DynamicGraph как уровень отсутствует — это открытый
   архитектурный вопрос для Любы (см. recon `codex-recon-core-docs-decisions.md`).

---

## 8. Ссылки

### 8.1. Реальные файлы кода (file:line, на которые опирается гайд)

- `C:/Projects/llm_mvp_core/00.Core/LogicalContracts.cs` — `LogicalUnitRequest`
  :62; `LogicalUnitResult`:72; `PhaseTransitionSignal`:89; `ILogicalUnit`:97;
  `LogicalFlowRequest`:105; `LogicalFlowResult`:131; `LogicalContextFragment`:146;
  `LogicalContextPacket`:158; `ILogicalFlow`:166; `LogicalOrchestratorRequest`
  :174; `LogicalOrchestratorResult`:182 (+`BranchResults`:190);
  `ILogicalOrchestrator`:202; `ITrustScorer`:226.
- `C:/Projects/llm_mvp_core/00.Core/LearningMachineContracts.cs` — A-LM-02
  комментарий:8; `LearningBatch`:18; `LearningTrainingItem`:30;
  `LearningBatchResult`:43; `LearningEvaluationResult`:90; `LearningMachineExport`
  :112; иерархия+training-path комментарий:197-207; `ILearningMachine<TDomain>`
  :208 (Train:212/Eval:217/Export:222/Import:227/GetOrchestrator:232);
  `ILearningMachinePair`:245.
- `C:/Projects/llm_mvp_core/07.GraphEngine/Models/GraphNode.cs` — `GraphNode`:3
  (`PageRankScore` set:12).
- `C:/Projects/llm_mvp_core/07.GraphEngine/Models/GraphEdge.cs` — `GraphEdge`:3
  (`Weight=1.0` init:8).
- `C:/Projects/llm_mvp_core/17.DynamicDomains/DynamicDomainGraph.cs` —
  `DynamicDomainTransition`:3; `DomainGraphClosure.HasAnyDependency`:8;
  `BuildPredecessorMap`:15; `OrchestratorExpander.FindBridgeSubPhases`:68;
  `FindPath`:90.
- `C:/Projects/llm_mvp_core/21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs`
  — `: ILogicalOrchestrator`:10; `MaxAdaptiveBranchCount`:19; `RunAsync`:85;
  `BranchResults=[flowResult]`:311; `RouteAsync`:316; `_multiRouter.ExecuteAsync`
  :326; `ExpandRoutedDecisions`:340; single-router:344;
  `FollowTransitionSignalsAsync`:361; `ExecuteBranchAsync`:496;
  `decision.KnowledgeEdge`:504; `HasTransitionDependency`:522;
  `OrderByTransitions`:541; `DomainGraphClosure.HasAnyDependency`:531;
  `BuildPredecessorMap`:552; `FindBridgeSubPhases`:778; `BuildDynamicDomainTransitions`
  :825; fan-in BranchResults:872.
- `C:/Projects/llm_mvp_core/21.AgentInfrastructure/Logical/MultiSubPhaseRouterUnit.cs`
  — `: ILogicalUnit<...SubPhaseRoutingDecision[]>`:8; `SubPhaseAnchorTerms`:23-56;
  `ExecuteAsync`:77; `Score`:249; взвешенная формула:283; `BuildPathFreeScoringText`
  :291; `BuildHybridScores`(HybridRanker):340; `BuildSourcePathWarnings`:367;
  `Tokenize`:482.
- `C:/Projects/llm_mvp_core/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs` —
  `LogicalKnnModelManifest`:18; `: ILogicalUnit<TInput,TOutput>`:33; `Train`:98;
  `ExecuteAsync`:197; `_index.Search`:218; `best = hits[0]`:223.
- `C:/Projects/llm_mvp_core/21.AgentInfrastructure/Logical/SubPhaseRouterUnit.cs`
  — `: ILogicalUnit<...SubPhaseRoutingDecision>`:33; `CalculateWrongDomainScore`:331.
- `C:/Projects/llm_mvp_core/42.ProjectWikiDomain/OrchestrationUnitContracts.cs` —
  6 unit-интерфейсов:76-107.
- `C:/Projects/llm_mvp_core/42.ProjectWikiDomain/BootstrapOrchestrationUnits.cs` —
  6 Bootstrap-классов.
- `C:/Projects/llm_mvp_core/42.ProjectWikiDomain/ProjectTaskOrchestrationPipeline.cs`
  — `TaskOrchestrationUnit.OrchestrateAsync`, `SubmitUserTaskApi`.
- `C:/Projects/llm_mvp_core/15.OrchestratorModel/Logical/LogicalOrchestratorAssistant.cs`
  — `CreateDefault` (регистрация flow/units/topology, latest-wins).
- `C:/Projects/llm_mvp_core/21.AgentInfrastructure/Logical/LogicalCapabilityTrainingContracts.cs`
  — `ILogicalCapabilityTrainer.TrainAndRegisterAsync` (мост к обучению).
- `C:/Projects/llm_mvp_core/docs/DECISIONS-LOCKED.md` — A-LM-09:32; A-LM-10:33;
  A-LM-11:34; A-LM-12:35; A-LM-13:36 (+A-LM-01..A-LM-08, D-14..D-17 по тексту).

### 8.2. Скилы

- `C:/Projects/llm_mvp_core/docs/skills/trainable-architecture-decision-SKILL.md`
- `C:/Projects/llm_mvp_core/docs/skills/implementation-doc-hierarchy-SKILL.md`
- `C:/Projects/llm_mvp_core/docs/skills/implementation-doc-uml-SKILL.md`
- `C:/Projects/llm_mvp_core/docs/skills/dpo-triple-format-SKILL.md`
- `C:/Projects/llm_mvp_core/docs/HOW-TO-USE-SKILLS.md`
- `C:/Projects/llm_mvp_core/CLAUDE.md` (двух-PASS, Graph-Style, Part A Contract)

### 8.3. Мои рекон-сводки (evidence-only)

База: `C:/Users/lubao/Documents/xiigen/llm mvp/dpo legal build/`
- `codex-recon-core-00-contracts.md`, `codex-recon-core-17-dynamic.md`,
  `codex-recon-core-07-graph.md`, `codex-recon-core-08-graphrag.md`,
  `codex-recon-core-15-logical.md`, `codex-recon-core-15-rest.md`,
  `codex-recon-core-21-infra.md`, `codex-recon-core-42-wiki.md`,
  `codex-recon-mvp-dyn-flow.md`, `codex-recon-mvp-local-wiki.md`,
  `codex-recon-core-docs-decisions.md`.

### 8.4. Evidence-only артефакты (НЕ редактировать, не продолжать)

- `C:/Projects/llm_mvp_core/docs/plans/claude-ILEARNINGDYNAMICGRAPH-DPO-PLAN-v1.md`
  — чужой `claude-*` план по теме; читать как evidence, не дублировать/не править.
- Концепт-документы `C:/Users/lubao/Documents/xiigen/llm mvp/llm dynamic flow/`
  (DYNAMIC-FLOW-*, D-13/ARBITER-KNN amendments) — концептуальный предшественник
  графа; имена «Orchestrator/flow/unit» там концептуальные, не C#-интерфейсы.
