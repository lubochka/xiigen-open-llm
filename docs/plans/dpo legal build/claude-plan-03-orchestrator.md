---
artifact_type: ilearningdynamicgraph_plan_block_03_orchestrator
authority_source: latest unquoted Luba work order for DPO legal build library
status: populated_now_planning_only
last_updated: 2026-06-23
write_scope: C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build\claude-*.md
plan_phase_anchor: Ф1.3.2 (primary) + Ф1.1.2 (канон ребра) + Ф1.3.1 (флоу ребра)
canon_counters: 12 фаз / 26 подфаз / 59 подшагов (Ф0…Ф11), нумерация Ф<P>.<SP>.<SS>
benchmark_read_only_source: codex-plan-03-orchestrator.md (НЕ редактируется)
---

# Блок плана 03 — Оркестратор: ILearningOrchestrator ≡ ILogicalOrchestrator (async, L4)

Автор: Claude (Opus 4.8 MAX), роль — автор полностью решённого блока плана в моём
namespace `claude-*`. Это часть **ПЛАНА** проекта **ILearningDynamicGraph**, а не
разрешение на реализацию продукта (R2/R3: PLAN-ONLY; исполнение — только после
свежего явного approve Luba в текущем ходе). Все цитаты кода — только реально
существующие, проверены чтением файлов, с `file:line`. Имена `ILearning*` из
задачи Любы — это **псевдонимы существующих контрактов** (см. CANON-маппинг §5), а
не новые архитектурные уровни (A-LM-12 — закрытый список из 5 уровней).

Псевдоним этого блока: **ILearningOrchestrator ≡ `ILogicalOrchestrator`**
(Level 4, `00.Core/LogicalContracts.cs:202`). Рантайм-реализация —
`AdaptiveLogicalOrchestrator` (`21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs:10`).

> **Якорь фазы (доска исполнения `codex-EXECUTION-CONTROL-BOARD`, evidence-only).**
> Этот блок-03 — источник канона оркестратора для подшага **Ф1.3.2** «Оркестратор
> L4 читает топологию как контракт» (owner `sa-orch`), и его Gate B G1..G8 (§10)
> = exit-условие Ф1.3.2. Он также питает **Ф1.1.2** (канон ребра `IDynamicGraphEdge`/
> `GraphEdgeKind`) и **Ф1.3.1** (флоу ребра + контракт fan-in merge). Канонические
> счётчики плана: **12 фаз (Ф0…Ф11) / 26 подфаз / 59 подшагов**, нумерация
> `Ф<P>.<SP>.<SS>`. Доска и мастер-план — evidence-only, здесь не правятся.

---

## 0. Якорь задачи и где этот claude-блок улучшает бенчмарк

```text
CURRENT TASK: написать МОЙ (claude-namespace) улучшенный блок-03 оркестратора —
              план-решение L4 для ILearningDynamicGraph, человеческим русским,
              Part A-формат, с сохранением структуры и канонических счётчиков
              (12 фаз / 26 подфаз / 59 подшагов, Ф<P>.<SP>.<SS>) и CANON-инвариантов
              (L5-машина без 6-го уровня A-LM-12; легальный teacher-гейт; запрет
              скрытого детерминизма A-LM-09/A-LM-13). Записать ровно в
              claude-plan-03-orchestrator.md.
NOT THE TASK: исполнять/тестировать/билдить код, запускать раннеры, тренировать,
              вызывать внешние AI/API/модели, git; править/создавать любые codex-*
              (только чтение), продолжать чужой codex-план или claude-00..07 каркас.
EVIDENCE-ONLY: codex-plan-03 (бенчмарк), codex-plan-00/02/04/05/12 (канон рёбер),
              codex-EXECUTION-CONTROL-BOARD / codex-ILEARNINGDYNAMICGRAPH-PLAN-v1
              (канон фаз/счётчиков), claude-00..07 (мой прежний каркас, evidence),
              реконы/гайды codex-*, реальный код llm_mvp_core (читается для цитат).
```

**Где этот блок осознанно лучше бенчмарка `codex-plan-03` (delta-нота, не слепая
копия).** Бенчмарк — сильный, но имеет одну реальную канон-ошибку и пару пробелов
читаемости. Я их чиню:

| # | Слабое место бенчмарка `codex-plan-03` | Улучшение в этом claude-блоке |
|---|---|---|
| D1 | **Канон ребра разъехался.** Бенчмарк объявляет `GraphEdgeKind` из **6** членов `{FanOut, FanIn, ManyToMany, MultiOutput, Sequential, Bridge}` и поле `AuthorityUnitRef`. Но мастер §A.4.1 и `codex-plan-04/05/12` фиксируют **5** членов `{FanOut, FanIn, ManyToMany, MultiOutput, ReturnLoop}`, async = флаг `ParallelEligible`, поле = `string AuthorityUnitId` (ссылка, latest-wins). Доска Ф1.1.2 прямо требует выровнять блок-03 на этот канон. Сам бенчмарк (§5 нота) называет такое расхождение «дефектом, а не деталью именования» — и при этом сам его содержит. | Выровнял на **единый канон 5 членов** + `ParallelEligible` (флаг) + `string AuthorityUnitId` (§5). `Sequential` корректно переосмыслен как `ParallelEligible=false` (режим исполнения, не вид ребра); `Bridge` — как **вставка топологии** (мостовые суб-фазы оркестратора), а не вид ребра. Добавил канон-поля доказательства `BaselineUnitId/BaselineRole/ExcludedLeakageSignals/SourceProof/CutoverProof/AblationProofRef` (`codex-plan-04/12`). |
| D2 | **Нет якоря фаз.** Бенчмарк построен как §1-12, но не привязан к каноническим `Ф<P>.<SP>.<SS>` (12/26/59). Читателю плана не видно, какой это подшаг доски. | Добавил явный якорь **Ф1.3.2** (+ Ф1.1.2/Ф1.3.1), счётчики 12/26/59, и связал Gate B G1..G8 с exit-условием Ф1.3.2 (§0, §10). |
| D3 | **Точность пары перегрузок file:line.** Бенчмарк цитирует `HasTransitionDependency`:525 и `OrderByTransitions`:541 как одиночные, хотя в коде у каждого по 2 перегрузки. | Цитирую обе: `HasTransitionDependency` :522 (диспетчер) / :525 (impl); `OrderByTransitions` :541 (диспетчер) / :544 (impl). Остальные ~25 цитат перепроверены чтением и подтверждены. |
| D4 | **Канон-консистентность имён DPO.** Бенчмарк ссылается на `GraphDpoPair`/`TeacherProvenance`, но не фиксирует, что L4 их **не несёт**. | Явно: на L4 этих полей нет; они живут на DPO-строке/боковом манифесте L5 (§8.3), согласовано с `codex-plan-00/04/05` и моими `claude-04/06`. |

CANON-инварианты сохранены: маппинг ролей Luba → реальные контракты; 5 закрытых
уровней (A-LM-12), DynamicGraph = специализация L5 (НЕ 6-й уровень); легальный
teacher-гейт; запрет скрытого детерминизма (A-LM-09/A-LM-13). NEW-типы помечены
`NEW`; существующие — с `file:line`.

---

## 1. Что это и зачем (роль компонента в ILearningDynamicGraph)

**Оркестратор — это L4-«начальник цеха» графа.** Он берёт один пользовательский
запрос (`LogicalOrchestratorRequest`), решает, **какие узлы графа** (суб-фазы →
flow) включить под этот запрос (это **fan-out**, веер наружу), запускает их
**асинхронно**, упорядочивает по зависимостям рёбер, собирает все ветки обратно в
один результат (это **fan-in**, веер внутрь, поле `BranchResults`) и обрабатывает
**петли возврата** по графу (advisory `PhaseTransitionSignal` — это **вид ребра
`ReturnLoop`**, см. §5 канон).

В пятиуровневой иерархии (`LearningMachineContracts.cs:197-207`) оркестратор стоит
**под** обучающей машиной и **над** flow/узлами:

```
ILearningDynamicGraph ≡ ILearningMachine<DynamicGraphDomain>  (L5, обучение)  ← блок 04
  └── GetOrchestratorAsync()  (LearningMachineContracts.cs:232)
        └── ILearningOrchestrator ≡ ILogicalOrchestrator  (L4, ЭТОТ БЛОК)  ← обход графа узлов/рёбер
              └── ILearningFlow ≡ ILogicalFlow<TInput>     (L3)            ← блок 02 (линия/подграф)
                    └── ILearningUnit ≡ ILogicalUnit<TIn,TOut> (L2)        ← блок 01 (узел/станок)
```

Зачем он нужен именно для **динамического** графа: сегодняшняя топология графа —
это **данные** в реестрах (`InMemoryLogicalFlowRegistry`,
`InMemoryLogicalUnitRegistry`, переходы проекта 17), которые редактируются на лету
mutation-юнитами. Оркестратор — **единственное место**, которое читает эту
изменяемую карту и обходит её в рантайме. Поэтому именно здесь живёт «обход
динамического графа»: fan-out, параллелизм веток, порядок по рёбрам, fan-in и
re-entry. **Train-логики тут нет и быть не должно** (правило A-LM-02,
`LearningMachineContracts.cs:8`): оркестратор — чистый inference-координатор;
перенастройка узлов — только на L5-машине.

Граница ответственности (важно для CANON R6 — тонкий агент, простые API): оркестратор
**не** содержит семантику выбора (это узлы-роутеры L2) и **не** содержит обучение
(это L5). Он — координатор обхода: «прочитать карту графа → разложить веер →
запустить → собрать → при необходимости вернуться». Ядро живёт в `llm_mvp_core`
(R5/R6); агент/расширение его только вызывает.

---

## 2. Концепт простыми словами (аналогия, без кода)

Возьми аналогию завода (она же в гайде 04 и в моих `claude-06`). Оркестратор —
это **начальник цеха со сменным планом**.

1. **Приходит заказ** (промпт пользователя). Начальник цеха сначала отдаёт заказ
   в «бюро переводов» (компиляция промпта в признаки) и «сортировку» (определение
   бизнес-домена), чтобы все станки читали одну и ту же расшифровку заказа.
2. **Развилка ленты (fan-out).** Начальник смотрит на заказ и решает, **на какие
   линии** его пустить. Под простой заказ — одну линию; под широкий заказ — сразу
   несколько линий (веер наружу). Это решение принимает **диспетчер-станок**
   (router-юнит), а не сам начальник: начальник только спрашивает диспетчера и
   получает список линий.
3. **Параллель против очереди.** Если линии **независимы**, начальник запускает их
   **одновременно** (несколько бригад параллельно). Если между линиями есть
   зависимость («сначала фундамент, потом стены»), он выстраивает их **в очередь**
   по карте зависимостей и пробрасывает результат предыдущей линии в следующую.
   В терминах графа «параллелить ли это ребро» — это **флаг `ParallelEligible`** на
   ребре, а не отдельный «вид ленты».
4. **Мосты.** Если между двумя выбранными линиями на карте есть обязательный
   промежуточный участок, начальник **добавляет его сам** (вставка мостовых
   суб-фаз). Это **не вид ребра**, а операция вставки в топологию (см. §5: `Bridge`
   у бенчмарка — это операция, а не член `GraphEdgeKind`).
5. **Сбор веера внутрь (fan-in).** Когда все линии отработали, начальник
   **складывает все их результаты в одну папку** (`BranchResults`) и пишет общий
   вердикт (худший из веток — главный).
6. **Петля возврата (ReturnLoop).** Иногда линия в конце говорит: «по дороге я
   понял, что надо ещё заглянуть в соседний цех» (advisory-сигнал перехода).
   Начальник проверяет совет (уверенность, совпадение домена) и, если совет здравый,
   запускает ещё один проход — но **не бесконечно** (лимит глубины 3, чтобы петля
   не стала циклом). Это и есть канонический пятый вид ребра `ReturnLoop`.
7. **Честное «не знаю».** Если диспетчер не уверен, на какую линию пустить заказ,
   или нужной линии вообще нет на заводе — начальник **не выдумывает** линию. Он
   выписывает «карточку пробела» (knowledge edge): «вот сюда хотели, но линии нет /
   уверенность низкая». Это честный незнайка-ответ (`PASS_AS_HONEST_UNTRAINED_SCAFFOLD`),
   а не подделка.

Чего начальник цеха **не делает сам**: не выбирает линию «по смыслу» (это работа
диспетчера-станка) и не перенастраивает станки (это ночная работа директора —
L5-машины). Он — координатор обхода карты.

---

## 3. Алгоритм/решение ПОШАГОВО (человеческим языком, полное решение)

Это **полностью решённый** обход графа в `AdaptiveLogicalOrchestrator.RunAsync`
(`AdaptiveLogicalOrchestrator.cs:85`). Ниже — что происходит, где, в каком
порядке, какие классы/методы и параметры участвуют. NEW-добавления для
динамического графа помечены явно и собраны в §5/§8.

### Существующий обход (как РЕШЕНО в коде сегодня)

Шаг 0. **Защитные проверки.** `RunAsync` (`:85`) проверяет отмену (`:88` →
`Fail("Cancelled.","CANCELLED")`), `null`-запрос (`:92` →
`Fail(...,"REQUEST_REQUIRED")`), пустой `UserRequest` (`:96-97` →
`Fail("User request cannot be empty.","EMPTY_REQUEST")`) и валидность конструкции
`ValidateConstruction()` (`:100`, тело `:1236` — требует topology/compiler/router/
flows/units). Любая ошибка → `OperationResult.Fail` с кодом. Ничего не бросается
(Rule 1).

Шаг 1. **Флаги.** `var flags = request.Flags ?? new LogicalExecutionFlags()`
(`:105`). Эти флаги (`LogicalContracts.cs:7-59`) — рубильники обхода:
`UseLearnedUnitWhenAvailable` (`:43`, использовать обученный узел, если есть),
`UseContextCarryForward` (`:45`, пробрасывать контекст между ветками),
`UseKnowledgeEdgeDetection` (`:51`, ловить пробелы), `UseConfidenceFloor` (`:42`,
порог уверенности), `UseE35DependencyDetection` (`:47`, динамическое определение
зависимостей).

Шаг 2. **Компиляция промпта (общий источник истины).** `CompilePromptAsync`
(вызов `:108`, тело `:244`) резолвит активный compiler-юнит через
`ResolveActiveUnit(...)` (`:269`) и исполняет его `ExecuteAsync`. Результат
(`SubPhaseRouterSourceDocument`) кладётся в metadata (`:114`), чтобы и
классификатор, и роутер читали одну расшифровку.

Шаг 3. **Классификация бизнес-домена.** `ClassifyDomainAsync` (вызов `:117`, тело
`:207`) резолвит активный доменный классификатор (тоже через `ResolveActiveUnit`,
учитывая `UseLearnedUnitWhenAvailable`) и исполняет его. Метка домена кладётся в
metadata (`:121`) для трассируемости результата.

Шаг 4. **Развилка: явный сценарий или адаптивный обход.** Если `ScenarioId` НЕ
адаптивный (`IsAdaptiveScenario`, `:124`, тело `:1263`), идём в
`RunExplicitScenarioAsync` (`:283` — прямой роут к flow по сценарию, для
совместимости/тестов). Иначе — адаптивный обход графа (шаги 5-9, вход через
`RouteAsync`-вызов `:128`).

Шаг 5. **FAN-OUT (веер наружу).** `RouteAsync` (`:316`):
- если есть `_multiRouter` (`:34`) — исполняет его `ExecuteAsync` с
  `MultiSubPhaseRouterUnit.Id` (`:328`), получает массив
  `SubPhaseRoutingDecision[]` и расширяет его `ExpandRoutedDecisions(...)` (`:772`);
- иначе исполняет одиночный `_router` (`:31`) и оборачивает один decision в массив.
**Вставка мостов (это операция, не вид ребра):** `ExpandRoutedDecisions` (`:772`)
при ≥2 ветках ищет «мостовые» суб-фазы `OrchestratorExpander.FindBridgeSubPhases(...)`
(`:778`) по карте переходов `BuildDynamicDomainTransitions()` (`:825`) и добавляет
их как ветки с пометкой `TOPOLOGY_BRIDGE` (`:799`). Затем `CapRoutedDecisions`
(`:808`) ограничивает число веток до `MaxAdaptiveBranchCount = 8` (`:19`), сохраняя
прямые выборы роутера приоритетнее мостов (`:815`).

Шаг 6. **Порядок рёбер: параллель против очереди.** `ExecuteBranchesAsync`
(`:445`):
- `OrderByTransitions(...)` (диспетчер `:541`, impl `:544`) упорядочивает ветки по
  топологии;
- `HasTransitionDependency(...)` (диспетчер `:522`, impl `:525`) решает, есть ли
  зависимость между ветками. Источник зависимости — детерминистическое замыкание
  графа `DomainGraphClosure.HasAnyDependency` (`:531`, проект 17) плюс опционально
  E35 (`UseE35DependencyDetection`) через `DependencyGraph` проекта 22/29;
- **если зависимостей нет** → **параллельный запуск**: ветки собираются в массив
  `tasks` и `await Task.WhenAll(tasks)` (`:465`). Это и есть Task-параллелизм веток
  графа. Первая упавшая ветка возвращается как ошибка;
- **если зависимости есть** → **последовательный запуск** в топологическом порядке
  (`foreach`), причём каждой следующей ветке передаются уже завершённые ветки
  (carry-forward контекста, см. шаг 8).

Шаг 7. **Исполнение одного ребра.** `ExecuteBranchAsync` (`:496`):
- если `decision.KnowledgeEdge` (уверенность ниже порога) → синтетический knowledge
  edge `BuildKnowledgeEdgeFlowResult(...)` (тело `:1112`) — честный «не знаю», без
  выдуманного flow;
- проверяет, что суб-фаза есть в топологии `_topology.ResolveSubPhase`; нет →
  knowledge edge (missing topology);
- резолвит исполняемый flow `_flows.ResolveBindingBySubPhase`; нет → knowledge edge
  (missing flow);
- иначе исполняет flow `ExecuteBindingAsync` (тело `:967`), который строит
  `LogicalFlowRequest<string>` и зовёт `binding.Flow.ExecuteAsync`.

Шаг 8. **Carry-forward контекста (зачаток fan-in между ветками).**
В `ExecuteBindingAsync` (`:967`) при `UseContextCarryForward` и непустых
завершённых ветках строится типизированный `BuildContextPacket(...)` (`:1032`) из
`LogicalContextFragment[]` (`LogicalContracts.cs:146`) и кладётся в metadata
следующей ветки.

Шаг 9. **FAN-IN (сбор веера внутрь) + петля возврата.**
- сначала knowledge-edge события ставятся в обучающую очередь
  `ScheduleKnowledgeEdges(...)` (`:930`);
- затем `FollowTransitionSignalsAsync(...)` (`:361`) собирает advisory
  `TransitionSignals` всех веток, и для каждого — **только если** глубина
  `< MaxTransitionDepth = 3` (`:22`), уверенность классификации достаточна и домен
  совпадает (иначе warning) — запускает рекурсивный `RunAsync`. Это петля возврата
  по графу (вид ребра `ReturnLoop`) с защитой от циклов;
- финал: `BuildAggregateResult(...)` (`:841`) собирает все ветки (прямые +
  транзишн) в `BranchResults` (`LogicalContracts.cs:190`), вычисляет худший вердикт
  через `VerdictRank` (`:913`), склеивает человекочитаемый текст и строит
  `OutputContextPacket` (`LogicalContracts.cs:192`). Это и есть fan-in.

> **Канон fan-in merge (B-16 + F1-merge-carrier, владелец — блок 02 §5A.3).** Явный
> контракт N→1 слияния `IEdgeFanInMerger.Merge` возвращает **типизированный
> `GraphFanInMergeResult`** (поля-носители `MergeRole =
> FanInMergeRole.DeterministicFanInBaseline` и `MergeOrderIsAuthority = false` —
> машинно-проверяемый свидетель перм-инвариантности merge на уровне ТИПА,
> симметрия `IDynamicGraphEdge.DeterministicFinalAuthority`; раньше жил только в
> прозе). Алгоритм (РЕШЕНО: **детерминированный baseline**, перм-инвариантен)
> проектируется в `claude-plan-02-edge.md §5A.3`. Оркестратор 03 НЕ владеет
> алгоритмом merge — его `BuildAggregateResult` (`:841`) = существующая рабочая
> реализация той же детерминированной агрегации (худший `VerdictRank` `:913` +
> context-packet), которую контракт `IEdgeFanInMerger` лишь делает явной. Источник
> learning-сигнала «ветку приняли вниз» (`DownstreamAccepted`) — арбитр приёмки
> `EdgeDownstreamAcceptanceUnit` (блок 02 §5A.1), коррекция непринятой ветки —
> `EdgeCorrectionFlow` (блок 02 §5A.2); оба питают DPO, не нарушая A-LM-02.

### NEW-надстройка для ДИНАМИЧЕСКОГО графа (что РЕШЕНО добавить в плане)

Сегодня обход «динамический» только по входу (читает изменяемые реестры), но
**контракты графа не выражены явно** и **финальный авторитет на ребре fan-out
детерминистический**. План решает это так (детали — §5):

NEW-1. **Типизированные рёбра графа** (`IDynamicGraphEdge`, `DynamicGraphTopology`)
вместо «голого» `DynamicDomainTransition(FromId, ToId)` (`17.DynamicDomains/
DynamicDomainGraph.cs:3`) — чтобы fan-out/fan-in/many-to-many/multi-output/
return-loop и per-edge-authority были контрактом, а не неявной логикой кода.
**Оркестратор читает топологию через этот контракт** (адаптер поверх
`BuildDynamicDomainTransitions()` `:825` + вход от `BuildDynamicTopology(signal,
budget)`, блок 04), **не меняя сам алгоритм обхода**.

NEW-2. **Обучаемый владелец fan-out.** Финальный выбор «какие суб-фазы» должен
принимать **обученный** `ILogicalUnit` (блок 01), а `MultiSubPhaseRouterUnit`
(фикс-веса `0.55/0.25/0.20/0.02`) становится **baseline-генератором кандидатов**
для ablation. Оркестратор **уже** поддерживает подмену через `ResolveActiveUnit`
(`:269`) + `UseLearnedUnitWhenAvailable` (`:43`) + latest-wins реестра — **новый
код в RunAsync для этого не нужен**, нужен лишь обученный multi-router-юнит,
зарегистрированный с `isLearned:true` (A-LM-03). На ребре хранится **`string
AuthorityUnitId`** (ссылка в реестр, резолв latest-wins), а не прямой объект-юнит.

NEW-3. **Контракт параллелизма веток как часть домена.** Параллелизм уже есть
(`Task.WhenAll` `:465`), но «можно ли параллелить это ребро» сегодня выводит
детерминистическое замыкание. План фиксирует: решение «зависимы ли ветки» либо
остаётся детерминистическим baseline (честно помечено, выражено флагом
`ParallelEligible` на ребре), либо выносится в обучаемый предиктор зависимостей
(Jaccard E49/E50 → baseline). По умолчанию — оставить детерминистическим (порядок
обхода — это инфраструктура, не семантика), если Luba не потребует обучаемый
порядок (Q3; на доске — lock-флаг `traversal_order_trainability_locked`, Ф0.1.2 Q14).

> **Важное разграничение (B-10).** Сказанное выше — про порядок **рёбер одного уровня**
> (параллель/очередь). Это НЕ то же, что **порядок паттернов РАЗНОГО ПОРЯДКА** (order-0..N): кто
> работает на низком порядке, кто собирает высокий, и в каком межуровневом порядке передавать
> задачу. Этот семантический выбор межуровневого порядка — **обучаемый** и решён отдельным узлом
> `GraphOrderResolverUnit` (владелец — блок 04, `claude-plan-04-dynamicgraph-machine.md §3.3-bis`):
> детерминированный Kahn-топосорт = честный ablation-baseline (число 0.02), а итоговый порядок
> топологии берётся из обученного решения (`DeterministicFinalAuthority=false`). Оркестратор 03
> этот узел НЕ владеет и НЕ дублирует — он лишь **читает** уже построенную топологию из
> `BuildDynamicTopology` (блок 04) и обходит её. То есть «порядок обхода = инфраструктура» верно
> только для рёбер одного уровня; межуровневый порядок паттернов — обучаемая семантика.

---

## 4. UML-фрагмент (Mermaid classDiagram)

```mermaid
classDiagram
    class ILogicalOrchestrator {
        <<interface L4 = ILearningOrchestrator>>
        +RunAsync(LogicalOrchestratorRequest, ct) Task~OperationResult~LogicalOrchestratorResult~~
    }
    note for ILogicalOrchestrator "СУЩ. 00.Core/LogicalContracts.cs:202. Псевдоним Любы ILearningOrchestrator. L4-координатор обхода графа. Train-метода НЕТ (A-LM-02). Статус: РАБОТАЕТ как контракт."

    class AdaptiveLogicalOrchestrator {
        <<class СУЩ.>>
        +RunAsync(request, ct)
        -RouteAsync(...) fan-out :316
        -ExecuteBranchesAsync(...) parallel|sequential :445
        -ExecuteBranchAsync(...) one edge :496
        -FollowTransitionSignalsAsync(...) ReturnLoop :361
        -BuildAggregateResult(...) fan-in :841
        -ExpandRoutedDecisions(...) bridge insertion :772
        -BuildDynamicDomainTransitions() read topology :825
    }
    note for AdaptiveLogicalOrchestrator "СУЩ. 21.../AdaptiveLogicalOrchestrator.cs:10. Рантайм-движок L4. Task.WhenAll:465 = параллелизм веток. Статус: РАБОТАЕТ; авторитет fan-out детерминистический (под-обучен, isLearned:false)."

    class LogicalOrchestratorRequest {
        <<record СУЩ.>>
        +RunId string
        +ScenarioId string
        +UserRequest string
        +Flags LogicalExecutionFlags?
        +Metadata IReadOnlyDictionary
    }
    note for LogicalOrchestratorRequest "СУЩ. LogicalContracts.cs:174. ВХОД оркестратора (input графа). Статус: РАБОТАЕТ."

    class LogicalOrchestratorResult {
        <<record СУЩ.>>
        +SelectedFlowId string
        +OverallVerdict string
        +FinalHumanResult string
        +FlowResult LogicalFlowResult
        +BranchResults LogicalFlowResult[]
        +OutputContextPacket LogicalContextPacket?
    }
    note for LogicalOrchestratorResult "СУЩ. LogicalContracts.cs:182. ВЫХОД оркестратора. BranchResults:190 = собранный fan-in. Статус: РАБОТАЕТ."

    class IDynamicGraphEdge {
        <<interface NEW канон master A.4.1/A.4.2>>
        +EdgeId string
        +FromNodeId string
        +ToNodeIds IReadOnlyList~string~
        +Kind GraphEdgeKind
        +ParallelEligible bool
        +AuthorityUnitId string
        +DeterministicFinalAuthority bool
        +BaselineUnitId string
        +ExcludedLeakageSignals IReadOnlyList~string~
    }
    note for IDynamicGraphEdge "NEW (в коде НЕТ). Типизированное ребро графа: вид Kind=GraphEdgeKind (КАНОН master §A.4.1 — РОВНО 5 членов FanOut/FanIn/ManyToMany/MultiOutput/ReturnLoop) + string AuthorityUnitId (ссылка в реестр на обучаемого владельца ребра, резолв latest-wins A-LM-03). Цель МНОЖЕСТВЕННАЯ ToNodeIds (Sequential/OneToOne => 1 элемент). ParallelEligible — async-режим как ФЛАГ, не член enum. DeterministicFinalAuthority — машинно-проверяемый носитель A-LM-09 (в проде = false). Заменяет голый DynamicDomainTransition(FromId,ToId) (17/DynamicDomainGraph.cs:3). Статус: ПРОЕКТ."

    class DynamicGraphTopology {
        <<class NEW>>
        +Nodes IReadOnlyList
        +Edges IReadOnlyList~IDynamicGraphEdge~
        +ListEdgesFrom(nodeId) IDynamicGraphEdge[]
    }
    note for DynamicGraphTopology "NEW (в коде НЕТ). Мутируемая на лету карта графа как контракт. Адаптер поверх ListTransitions()/BuildDynamicDomainTransitions():825 + вход BuildDynamicTopology(signal,budget) блока 04. Статус: ПРОЕКТ."

    class MultiSubPhaseRouterUnit {
        <<class СУЩ. baseline>>
        +ExecuteAsync(...) SubPhaseRoutingDecision[]
        -Score(...) fixed weights 0.55/0.25/0.20/0.02
    }
    note for MultiSubPhaseRouterUnit "СУЩ. 21.../MultiSubPhaseRouterUnit.cs:8. Финальный авторитет fan-out СЕЙЧАС (isLearned:false). Должен стать baseline-генератором кандидатов; обученный L2-юнит — финал. Статус: РАБОТАЕТ как baseline scorer."

    ILogicalOrchestrator <|.. AdaptiveLogicalOrchestrator : implements
    AdaptiveLogicalOrchestrator ..> LogicalOrchestratorRequest : input
    AdaptiveLogicalOrchestrator ..> LogicalOrchestratorResult : output
    AdaptiveLogicalOrchestrator ..> MultiSubPhaseRouterUnit : fan-out (replaceable via ResolveActiveUnit:269)
    AdaptiveLogicalOrchestrator ..> DynamicGraphTopology : NEW reads edges
    DynamicGraphTopology o-- IDynamicGraphEdge : contains
    IDynamicGraphEdge ..> MultiSubPhaseRouterUnit : baseline candidate generator (BaselineUnitId)
```

### 4.1. Легенда классов

| Класс | Что делает | Статус |
|---|---|---|
| `ILogicalOrchestrator` | L4-контракт обхода графа (псевдоним `ILearningOrchestrator`) | РАБОТАЕТ как контракт (`LogicalContracts.cs:202`) |
| `AdaptiveLogicalOrchestrator` | Рантайм-движок L4: fan-out/параллель/fan-in/петля | РАБОТАЕТ; авторитет fan-out детерминистический (`AdaptiveLogicalOrchestrator.cs:10`) |
| `LogicalOrchestratorRequest` | Вход оркестратора (промпт + флаги + метаданные) | РАБОТАЕТ (`LogicalContracts.cs:174`) |
| `LogicalOrchestratorResult` | Выход (+`BranchResults` = fan-in) | РАБОТАЕТ (`LogicalContracts.cs:182/190`) |
| `IDynamicGraphEdge` | Типизированное ребро (канон 5 видов + `AuthorityUnitId`) | NEW (ПРОЕКТ) |
| `DynamicGraphTopology` | Мутируемая карта графа как контракт | NEW (ПРОЕКТ) |
| `MultiSubPhaseRouterUnit` | Детерминистический роутер fan-out (baseline) | РАБОТАЕТ как baseline (`MultiSubPhaseRouterUnit.cs:8`) |

### 4.2. Таблица симуляции (для каждой связи/ветки: вход → ожидаемый результат)

| # | Связь/ветка | Пример входа | Ожидаемый результат |
|---|---|---|---|
| S1 | `AdaptiveLogicalOrchestrator implements ILogicalOrchestrator` | `RunAsync(req)` с пустым `UserRequest` | `Fail("User request cannot be empty.","EMPTY_REQUEST")` (`:96-97`) — никаких throw |
| S2 | `..> LogicalOrchestratorRequest` (вход) | `UserRequest="исправь падающий unit-тест в backend-сервисе оплаты"`, `ScenarioId=""` | `IsAdaptiveScenario=true` (`:1263`) → адаптивный обход (fan-out) |
| S3 | `..> MultiSubPhaseRouterUnit` fan-out | тот же промпт | `SubPhaseRoutingDecision[]` с `5.1 unit-tests` и `4.4 refactor/fix`; затем `ExpandRoutedDecisions` (`:772`) может вставить мост (`TOPOLOGY_BRIDGE` `:799`) |
| S4 | ветка: **параллель** (`Task.WhenAll`) | 2 независимые суб-фазы (`HasTransitionDependency=false`, impl `:525`) | обе ветки стартуют одновременно (`:465`), результаты в route-порядке |
| S5 | ветка: **очередь** (sequential = `ParallelEligible=false`) | 2 зависимые суб-фазы (`PLAN`→`CODE_Q`) | последовательный `foreach`; 2-й ветке передан контекст 1-й (`BuildContextPacket` `:1032`) |
| S6 | ветка: **knowledge edge** (порог) | `decision.KnowledgeEdge=true` (уверенность ниже floor) | `BuildKnowledgeEdgeFlowResult` (`:1112`), вердикт `KNOWLEDGE_EDGE`, без выдуманного flow |
| S7 | ветка: **missing flow** | суб-фаза в топологии есть, flow нет | knowledge edge `missingFlow=true` + событие в очередь обучения (`ScheduleKnowledgeEdges` `:930`) |
| S8 | `..> LogicalOrchestratorResult` fan-in | 3 ветки: `PASS`, `PASS_WITH_GAPS`, `REJECT` | `BranchResults`=3 (`:190`); `OverallVerdict=REJECT` (худший, `VerdictRank` `:913`) |
| S9 | петля возврата (`ReturnLoop`) | ветка эмитит `PhaseTransitionSignal`, conf высокая, домен совпал | рекурсивный `RunAsync` (`FollowTransitionSignalsAsync` `:361`), ветки добавлены в агрегат (`:841`) |
| S10 | петля: защита от цикла | сигнал при глубине = `MaxTransitionDepth=3` (`:22`) | warning «Transition depth limit reached», переход пропущен |
| S11 | `o-- IDynamicGraphEdge` (NEW): канон 5 видов + multi-target | ребро `EdgeKind=FanOut`, `ToNodeIds=["5.1","4.4"]`, `AuthorityUnitId="multi-router@learned"`, `ParallelEligible=true`, `DeterministicFinalAuthority=false` | топология отдаёт ОДНО ребро с ДВУМЯ целями (fan-out выражен `ToNodeIds`, а не одиночным `ToNodeId`); оркестратор резолвит обученного владельца через `ResolveActiveUnit` (`:269`) по `AuthorityUnitId`, baseline (`BaselineUnitId`) — fallback |
| S12 | `..> DynamicGraphTopology` (NEW) | mutation-юнит добавил ребро на лету | следующий `RunAsync` видит новое ребро через адаптер `BuildDynamicDomainTransitions` (`:825`); алгоритм обхода не меняется |
| S13 | `o-- IDynamicGraphEdge` (NEW): носитель A-LM-09 на ребре | ребро `EdgeKind=FanOut`, финальную власть держит детерминистический хелпер → `DeterministicFinalAuthority != false` | вердикт `FORBIDDEN_RUNTIME_SHORTCUT` (R4 / A-LM-09; синхрон с `codex-plan-12` Gate B и `codex-plan-04` Gate B); регистрация ребра/юнита блокируется до возврата `DeterministicFinalAuthority=false` (финал — обученный L2-юнит) |
| S14 | переосмысление бенчмарка: `Sequential`/`Bridge` НЕ виды enum | ребро, которое надо исполнить строго после другого; и пара линий, требующая промежуточного участка | `Sequential` ⇒ ребро с `ParallelEligible=false` (режим); `Bridge` ⇒ вставка мостовой суб-фазы `FindBridgeSubPhases` (`:778`)+`TOPOLOGY_BRIDGE` (`:799`) (операция топологии). Оба — НЕ члены `GraphEdgeKind` (канон 5 видов) |

---

## 5. Контракты/типы (NEW vs существующее, с file:line)

### Существующие (НЕ менять — это контракт-каркас L4)

```csharp
// СУЩ. 00.Core/LogicalContracts.cs:202
public interface ILogicalOrchestrator
{
    Task<OperationResult<LogicalOrchestratorResult>> RunAsync(
        LogicalOrchestratorRequest request, CancellationToken cancellationToken = default); // :204
}

// СУЩ. LogicalContracts.cs:174 — ВХОД
public sealed record LogicalOrchestratorRequest(
    string RunId, string ScenarioId, string UserRequest,
    LogicalExecutionFlags? Flags, IReadOnlyDictionary<string, object> Metadata);

// СУЩ. LogicalContracts.cs:182 — ВЫХОД (init-поле BranchResults:190 = fan-in)
public sealed record LogicalOrchestratorResult(
    string ScenarioId, string SelectedFlowId, string OverallVerdict,
    string FinalHumanResult, LogicalExecutionFlags AppliedFlags, LogicalFlowResult FlowResult)
{
    public LogicalFlowResult[] BranchResults { get; init; } = [];     // :190
    public string[] Warnings { get; init; } = [];                     // :191
    public LogicalContextPacket? OutputContextPacket { get; init; }   // :192
}
```

### NEW — предлагаемые типы графа (в коде их НЕТ; пометка NEW)

Эти типы вводит **домен графа** (`DynamicGraphDomain`, блок 04) и читает
оркестратор через тонкий адаптер. Они **не добавляют Train-методов** на L4
(A-LM-02). Предлагаемые сигнатуры (утвердить у Luba — Q1/Q2):

```csharp
// NEW — вид ребра графа (тип веера). КАНОН master §A.4.1: РОВНО 5 членов.
// ВНИМАНИЕ (исправление дрейфа бенчмарка codex-plan-03): здесь НЕТ Sequential и НЕТ Bridge.
//   Sequential ⇒ это режим исполнения, выражается флагом ParallelEligible=false (НЕ член enum);
//   Bridge     ⇒ это операция вставки мостовой суб-фазы оркестратором (FindBridgeSubPhases :778,
//                TOPOLOGY_BRIDGE :799), а НЕ вид ребра.
// Устаревшие локальные псевдонимы (master §A.4.1): EdgeKind, AsyncParallel, BackEdge — НЕ использовать
//   (AsyncParallel -> флаг ParallelEligible; BackEdge -> GraphEdgeKind.ReturnLoop).
public enum GraphEdgeKind { FanOut, FanIn, ManyToMany, MultiOutput, ReturnLoop }

// NEW — типизированное ребро графа. Заменяет голый
// DynamicDomainTransition(FromId,ToId) (СУЩ. 17.DynamicDomains/DynamicDomainGraph.cs:3).
// КАНОН ребра (master §A.4.1/§A.4.2 + codex-plan-04 §Canon + codex-plan-12 §Canon + codex-plan-00 §4 + codex-plan-05 §5.2):
//   1) цель МНОЖЕСТВЕННАЯ — IReadOnlyList<string> ToNodeIds (а не одиночный ToNodeId):
//      одиночный ToNodeId СТРУКТУРНО не может выразить FanOut/MultiOutput/ManyToMany,
//      которые сам же объявляет GraphEdgeKind; Sequential/OneToOne => список ровно из 1 элемента;
//   2) владелец финала ребра — string AuthorityUnitId (ССЫЛКА в реестр, резолв latest-wins A-LM-03),
//      а НЕ прямой объект-юнит и НЕ устаревший AuthorityUnitRef (codex-plan-12: «edge stores a
//      canonical authority id», не object-ссылку);
//   3) DeterministicFinalAuthority — машинно-проверяемый носитель A-LM-09 НА УРОВНЕ ТИПА:
//      в прод-пути обязан быть false; без этого носителя детерминистический хелпер с рантайм-властью
//      fan-out прошёл бы собственный контракт ребра -> вердикт FORBIDDEN_RUNTIME_SHORTCUT (S13);
//   4) поля доказательства (codex-plan-04/12): BaselineUnitId/BaselineRole/ExcludedLeakageSignals/
//      SourceProof/CutoverProof/AblationProofRef — baseline выражается явным id+role, НЕ null-авторитетом.
public interface IDynamicGraphEdge
{
    string EdgeId { get; }
    string FromNodeId { get; }
    IReadOnlyList<string> ToNodeIds { get; }   // канон: множество целей; Sequential/OneToOne => 1 элемент
    GraphEdgeKind Kind { get; }                // КАНОН-имя поля = Kind (владелец claude-plan-02 §5); тип GraphEdgeKind (§A.4.1, 5 видов); EdgeKind — УСТАРЕВШИЙ псевдоним поля, НЕ использовать
    bool ParallelEligible { get; }             // async-режим как ФЛАГ (бывш. AsyncParallel); НЕ финальный авторитет
    string AuthorityUnitId { get; }            // ссылка в реестр на обученного владельца ребра (latest-wins A-LM-03)
    string AuthorityUnitLevel { get; }         // уровень владельца (ожидается L2 ILogicalUnit)
    bool DeterministicFinalAuthority { get; }  // ОБЯЗАТЕЛЬНО = false в проде (носитель A-LM-09)
    string? BaselineUnitId { get; }            // baseline-генератор кандидатов (напр. MultiSubPhaseRouterUnit)
    string BaselineRole { get; }               // напр. "candidate_generator" | "ablation_baseline"
    IReadOnlyList<string> ExcludedLeakageSignals { get; } // [candidate_index, source_path, exact_prompt]
    string SourceProof { get; }                // провенанс происхождения ребра
    string CutoverProof { get; }               // доказательство перехода baseline→trained
    string AblationProofRef { get; }           // ссылка на ablation-доказательство (Ф5/Ф8)
}

// NEW — мутируемая на лету карта графа как контракт (а не неявная логика в коде оркестратора).
// Реализуется адаптером поверх ILogicalTopologyRegistry.ListTransitions()
// и BuildDynamicDomainTransitions() (СУЩ. AdaptiveLogicalOrchestrator.cs:825),
// плюс вход BuildDynamicTopology(DynamicGraphBuildSignal, TopologyBuildBudget) (блок 04).
public sealed class DynamicGraphTopology
{
    public IReadOnlyList<IDynamicGraphNode> Nodes { get; init; }      // IDynamicGraphNode — блок 01 (NEW)
    public IReadOnlyList<IDynamicGraphEdge> Edges { get; init; }
    // Рёбра, ВЫХОДЯЩИЕ из nodeId (фильтр по FromNodeId). Каждое такое ребро может
    // нести НЕСКОЛЬКО целей (edge.ToNodeIds) — это и есть fan-out/multi-output из одного узла.
    public IDynamicGraphEdge[] ListEdgesFrom(string nodeId) => /* Edges.Where(e => e.FromNodeId == nodeId) */;
}
```

### NEW — точки интеграции в оркестраторе (минимальная правка, в плане; НЕ исполнять)

Оркестратор НЕ переписывается. Для динамического графа достаточно:
- адаптер `DynamicGraphTopology` ← `_topology.ListTransitions()` /
  `BuildDynamicDomainTransitions()` (`:825`) — чтение рёбер как контракт;
- per-edge authority резолвится уже существующим `ResolveActiveUnit<TIn,TOut>(...)`
  (`:269`) по `AuthorityUnitId` ребра + флагу `UseLearnedUnitWhenAvailable` (`:43`).
  **Никаких новых решающих веток в RunAsync** — обученный юнит просто заменяет
  baseline через latest-wins реестра (A-LM-03). Это и есть Gate B Ф1.3.2: «подмена
  baseline→trained не требует нового кода в RunAsync».

> **Единый канон ребра (обязательная ссылка, чинит дефект бенчмарка).**
> `IDynamicGraphEdge` в этом блоке — тот же единственный канонический тип, что в
> `codex-plan-00` §4, `codex-plan-02` §5, `codex-plan-04` §Canon, `codex-plan-05`
> §5.2, `codex-plan-12` §Canon и мастер §A.4.1/§A.4.2. Кардинальность цели —
> **МНОЖЕСТВЕННАЯ** (`IReadOnlyList<string> ToNodeIds`; `Sequential/OneToOne` => 1
> элемент); вид — `GraphEdgeKind` **5 членов**; async — флаг `ParallelEligible`;
> владелец — `string AuthorityUnitId` (ссылка, latest-wins); носитель A-LM-09 —
> `bool DeterministicFinalAuthority` (в проде `false`). **Имя поля вида ребра —
> канонически `Kind`** (тип `GraphEdgeKind`), владелец имени — `claude-plan-02 §5`
> (там `GraphEdgeKind Kind { get; }`); `claude-plan-00 §5` несёт то же `GraphEdgeKind Kind`.
> `EdgeKind` как ИМЯ свойства — устаревший псевдоним и НЕ используется (раунд-2
> RR1-contract-names: ранее блок-03 объявлял поле `EdgeKind`, теперь выровнено на
> `Kind`). Расхождение этих полей между блоками — **дефект**, а не «деталь именования».
> Бенчмарк `codex-plan-03` использовал 6-членный enum (`+Sequential,+Bridge`) и
> `AuthorityUnitRef`; здесь это исправлено на канон (доска Ф1.1.2 Gate B прямо
> требует выровнять блок-03).

---

## 6. Цитаты кода «как сейчас» → сейчас / почему / должно стать / как менять

### Цитата A — Task-параллелизм веток графа

`AdaptiveLogicalOrchestrator.cs:465`:
```
var parallelResults = await Task.WhenAll(tasks);
```
- **Сейчас:** независимые ветки fan-out запускаются параллельно через `Task.WhenAll`
  (массив `tasks` собран из `ExecuteBranchAsync` для каждой ветки).
- **Почему так (ок):** это корректный async-параллелизм обхода графа; ветки без
  зависимости не должны ждать друг друга. `MaxAdaptiveBranchCount=8` (`:19`)
  ограничивает веер. Нарушения нет.
- **Должно стать:** оставить как есть. Для NEW-графа подсказку «параллелить ли
  ребро» можно брать из `IDynamicGraphEdge.ParallelEligible`, но **финальное**
  решение по-прежнему — детерминистический планировщик обхода (это инфраструктура,
  не семантика; A-LM-12 разрешает helper внутри уровня).
- **Как менять:** только если Luba потребует обучаемый порядок (Q3 / lock-флаг Q14)
  — тогда предиктор зависимостей становится обученным L2-юнитом, а текущее
  замыкание/E35 — baseline для ablation.

### Цитата B — финальный авторитет fan-out (детерминистический)

`AdaptiveLogicalOrchestrator.cs:328` (внутри `RouteAsync` `:316`):
```
                MultiSubPhaseRouterUnit.Id,
```
(исполняется `_multiRouter.ExecuteAsync(...)`), плюс источник весов —
`MultiSubPhaseRouterUnit.cs:283`:
`(predicateScore * 0.55) + (exactSubPhaseMention * 0.25) + (tokenScore * 0.20) + (hybridScore * 0.02)`.
- **Сейчас:** финал «какие суб-фазы» принимает `MultiSubPhaseRouterUnit` —
  детерминистическая взвешенная формула + словарь якорей (`:23-56`), `isLearned:false`.
- **Почему так:** это **честный baseline** (claim «умного выбора» не делается),
  поэтому сегодня нарушения нет (`OK_INTERNAL_DETAIL`). Но это **финальная власть
  ребра fan-out**.
- **Должно стать:** при любой claim «семантического/обученного выбора» это станет
  `FORBIDDEN_RUNTIME_SHORTCUT` (R4, A-LM-09 `DeterministicFinalAuthority=false`).
  Финал должен принимать **обученный** `ILogicalUnit` (блок 01; вход БЕЗ
  candidate-index и БЕЗ source-path), а словарь/HybridRanker — baseline-генератор
  кандидатов (`BaselineUnitId`/`BaselineRole`) для ablation.
- **Как менять:** **в RunAsync менять НЕ нужно.** `RouteAsync` уже зовёт
  `_multiRouter`; подменить baseline на обученный multi-router можно регистрацией
  обученного юнита с `isLearned:true` (latest-wins, A-LM-03) + резолвом через
  `ResolveActiveUnit` (`:269`) по `AuthorityUnitId` ребра. Изменение — в блоке 01 +
  L5-обучении (блок 04). Это подшаг **Ф8** (cutover), безопасен только после
  доказанного обучаемого пути Ф4-Ф7.

### Цитата C — порядок рёбер через детерминистическое замыкание проекта 17

`AdaptiveLogicalOrchestrator.cs:531`:
```
if (DomainGraphClosure.HasAnyDependency(routedIds, BuildDynamicDomainTransitions()))
```
- **Сейчас:** «есть ли зависимость между ветками» решает DFS-замыкание проекта 17
  поверх топологии переходов (вызвано из `HasTransitionDependency` impl `:525`).
- **Почему так (ок):** это обход карты графа (инфраструктура порядка), а не
  семантическая claim. `OK_INTERNAL_DETAIL`.
- **Должно стать:** допустимо оставить детерминистическим, **если** «есть ли
  зависимость» не объявляется обучаемым. Если объявляется (E49/E50 Jaccard) —
  Jaccard становится baseline, финал — обученный предиктор зависимости; результат
  выражается флагом `ParallelEligible` на ребре.
- **Как менять:** по умолчанию не менять (Q3). Карта рёбер читается через NEW
  `DynamicGraphTopology` (адаптер поверх `BuildDynamicDomainTransitions` `:825`).

### Цитата D — честный «не знаю» (knowledge edge)

`AdaptiveLogicalOrchestrator.cs:1112` (тело `BuildKnowledgeEdgeFlowResult`, вызвано
из `ExecuteBranchAsync` `:496`):
```
    private static LogicalFlowResult BuildKnowledgeEdgeFlowResult(
```
- **Сейчас:** при низкой уверенности/отсутствии flow оркестратор выдаёт
  синтетический knowledge edge (`KnowledgeEdgeFlowId` `:16`), а не выдуманный выбор.
- **Почему так (ок):** это антимаскарад — честный degraded-ответ
  (`PASS_AS_HONEST_UNTRAINED_SCAFFOLD`). Правильный образец.
- **Должно стать:** оставить. Это и есть требуемое «честное untrained-поведение» на
  уровне обхода графа.
- **Как менять:** не менять; покрыть тестом untrained-честности (§9).

### Цитата E — образец анти-leakage (источник пути исключён из скоринга)

`21.AgentInfrastructure/Logical/MultiSubPhaseRouterUnit.cs:291` (`BuildPathFreeScoringText`):
- **Сейчас:** SourcePath намеренно исключён из текста, влияющего на скоринг
  (путь держится только как provenance-warning).
- **Почему так (ок):** это запрет source-path как scoring-фичи — образец для
  будущего входа обученного владельца ребра.
- **Должно стать:** NEW-вход обученного fan-out-юнита наследует это правило
  (path-free) и дополнительно исключает candidate-index.
- **Как менять:** зафиксировать `ExcludedLeakageSignals=[candidate_index,
  source_path, exact_prompt]` на ребре (`IDynamicGraphEdge`) и в GraphDpoPair (блок 04).

---

## 7. Статус (честно: что работает, что заглушка)

| Элемент обхода | Файл:line | Статус |
|---|---|---|
| Контракт L4 `ILogicalOrchestrator` | `LogicalContracts.cs:202` | РАБОТАЕТ как контракт (Train-метода нет, A-LM-02) |
| `RunAsync` (полный обход) | `AdaptiveLogicalOrchestrator.cs:85` | РАБОТАЕТ |
| FAN-OUT (`RouteAsync` + multi-router) | `:316`,`:328` | РАБОТАЕТ; финальный авторитет детерминистический (baseline, под-обучен) |
| Вставка мостов (операция, не вид ребра) | `:772`,`:778`,`:799` | РАБОТАЕТ (детерминистический BFS, проект 17) |
| Параллелизм веток (`Task.WhenAll`) | `:465` | РАБОТАЕТ |
| Очередь по зависимостям | `:445`,`:525`,`:544` | РАБОТАЕТ (детерминистическое замыкание `:531`) |
| Carry-forward контекста | `:1032`,`:967` | РАБОТАЕТ (зачаток fan-in между ветками) |
| FAN-IN агрегат (`BranchResults`) | `:841`, `LogicalContracts.cs:190` | РАБОТАЕТ |
| Петля возврата `ReturnLoop` (transition signals) | `:361` (лимит `:22`) | РАБОТАЕТ (с защитой глубины=3) |
| Knowledge edge (честный «не знаю») | `:1112`, `:16` | РАБОТАЕТ (антимаскарад) |
| Подмена обученного юнита (`ResolveActiveUnit`) | `:269` | РАБОТАЕТ как слот; обученного multi-router-юнита ПОКА НЕТ |
| `IDynamicGraphEdge` / `DynamicGraphTopology` (канон 5 видов + `AuthorityUnitId`) | — | ОТСУТСТВУЕТ (NEW, проектируется здесь) |
| Per-edge authority как контракт | — | ОТСУТСТВУЕТ (NEW; сегодня неявно = baseline router) |
| Обучаемый владелец fan-out | — | ОТСУТСТВУЕТ (нужен блок 01 + L5 блок 04) |
| Teacher/provenance на уровне обхода | — | N/A на L4 (teacher живёт на DPO-строке/L5; см. §8.3) |

Двух-PASS вывод для L4: сейчас **`PASS_AS_HONEST_UNTRAINED_SCAFFOLD`** (обход
честный, скрытого «trained» детерминизма нет: baseline-роутер помечен
`isLearned:false`). `PASS_AS_TRAINED_FEATURE` для fan-out наступит только после
обученного multi-router-юнита + checkpoint/ablation/fresh-load/continue-training
(блоки 01/04, фазы Ф4-Ф5-Ф8).

---

## 8. Гейты (применимые к L4-оркестратору)

### 8.1. Запрет скрытого детерминизма (R4 / A-LM-09 / A-LM-12 / A-LM-13)

- Финальную семантику выбора ребра принимает **обучаемый уровень**
  (`DeterministicFinalAuthority=false`, A-LM-09). Этот запрет — **не только проза**:
  он несётся **на уровне типа** полем `IDynamicGraphEdge.DeterministicFinalAuthority`
  (§5; синхрон с `codex-plan-12` Gate B `DeterministicFinalAuthority=false` и
  `codex-plan-04` Gate B). Ветка-нарушитель (детерминистический хелпер как финал
  ребра → `DeterministicFinalAuthority != false`) симулируется в §4.2 строкой S13 и
  даёт вердикт `FORBIDDEN_RUNTIME_SHORTCUT`.
- `MultiSubPhaseRouterUnit` (фикс-веса `0.55/0.25/0.20/0.02`, `:283`),
  `SubPhaseRouterUnit`, `DomainGraphClosure`/E35-ordering, `HybridRanker`,
  `CapRoutedDecisions` tie-break (`:808`/`:815`), latest-wins резолв — это
  **детали реализации/baseline** (A-LM-12). Они допустимы **только** честно
  помеченными (`BaselineRole`=baseline/eval-only) и **никогда** не как заявленный
  «умный» финал.
- Удаление baseline ≠ покрытие обучаемой замены: план обязан назвать обученный юнит,
  typed trained state, checkpoint/manifest, export/import, locator, fresh-load
  probe, continue-training probe, ablation/leakage gate (это блоки 01/04, фазы
  Ф4-Ф5; оркестратор только вызывает их через `ResolveActiveUnit`). Baseline
  выражается явным `BaselineUnitId`/`BaselineRole`, а не null-авторитетом
  (`codex-plan-04`).

### 8.2. Анти-leakage (для будущего обученного fan-out-юнита; оркестратор передаёт вход)

- Запрещённые сигналы на ребре fan-out: **candidate-index/позиция** в списке
  (permutation-variant выбор), **source-path** как scoring-фича, source-id/
  exact-prompt replay. Оркестратор сам индекс кандидата не подмешивает в семантику;
  обученный юнит обязан исключить эти сигналы (`ExcludedLeakageSignals[]` на ребре и
  в GraphDpoPair).
- D-17: `exactPromptLeakageRate = 0.0` во всех eval-гейтах обучения роутера ребра;
  exclusion-without-failure запрещён.
- Образец-«как надо»: `MultiSubPhaseRouterUnit` уже исключает путь из скоринга
  (`BuildPathFreeScoringText`, `MultiSubPhaseRouterUnit.cs:291`) и держит путь
  только как provenance-warning — этот паттерн сохранить.

### 8.3. Легальный teacher-гейт (Legal Teacher / Provenance Gate)

- На самом L4 teacher-вызовов **нет** (`RunAsync` не зовёт внешних учителей). Гейт
  применяется к **DPO-строкам**, на которых учится владелец ребра (блок 04):
  teacher для дистилляции — **только** open-weight permissive (Qwen3 Apache-2.0,
  DeepSeek MIT, Kimi K2 modified-MIT, GLM, Mistral) через нейтральных провайдеров
  (Together/Fireworks/OpenRouter/Novita/DashScope/DeepSeek/Moonshot/Z.ai).
- **Никогда** Claude/OpenAI/Gemini как teacher для дистилляции. Контаминированные
  строки (`source_ref sonnet-*` / `rollout openai`) → `DATASET_REBUILD_REQUIRED`.
- Judges (`judge_model`) могут быть лучшими моделями (включая closed), но judge ≠
  цель обучения.
- Поля `TeacherProvenance` и канон `GraphDpoPair` (CANON, имена синхронны с
  `codex-plan-00/04/05` и моими `claude-04/06`) живут на DPO-строке / боковом
  DPO-манифесте L5, а **не** на L4-контракте оркестратора.

### 8.4. Измерение (completion-evidence)

- Численные метрики обхода берутся из L3/L5: `LogicalFlowSummary`
  (`Baseline/Enhanced/Passed/NextAction`, `LogicalContracts.cs:114`) на ветку и
  `LearningBatchResult`/`LearningEvaluationResult` (`LearningMachineContracts.cs:43,
  90`) на обучение владельца ребра. Без UNKNOWN/static/validation-only для claim
  «trained feature».
  > **Носитель `BranchConfidence` (кросс-блок-консистентность с claude-plan-02 §5A.1).**
  > `LogicalFlowSummary:114` НЕ несёт поля confidence — его поля ровно
  > `Baseline/Enhanced/Passed/NextAction`. Поэтому `BranchConfidence` арбитра приёмки
  > (`EdgeDownstreamAcceptanceUnit`, claude-plan-02 §5A.1) берётся из
  > `EdgeAuthorityDecision.OrderedTargets[].Confidence` (обученный авторитет, claude-plan-02
  > §5 :620), а для baseline-ветки — из `SubPhaseRoutingDecision.Confidence`
  > (`SubPhaseRouterUnit.cs:21`). Стоп-число приёмки `AcceptanceFloor=0.80` (claude-plan-02
  > §5A.1) = тот же confidence-floor, что у `UseConfidenceFloor` (:42) и
  > `WeaknessTriggerConfig.confidenceFloor=0.80` (claude-plan-09 §5.3).
- Гейт обхода (без обучения): доля корректно собранных fan-in (`BranchResults`),
  отсутствие потерянных веток, корректный худший-вердикт (`VerdictRank` `:913`).

---

## 9. Тест-стратегия (FC-26: по слоям, что применимо к L4)

| Слой | Что проверяем на оркестраторе | Пример теста |
|---|---|---|
| **unit** | защитные проверки, флаги, маппинг decision→branch | пустой `UserRequest` → `EMPTY_REQUEST` (`:96-97`); `null` request → `REQUEST_REQUIRED` (`:92-93`) |
| **business-logic** | fan-out → fan-in согласованность; худший вердикт | 3 ветки `PASS/PASS_WITH_GAPS/REJECT` → `OverallVerdict=REJECT` (`VerdictRank` `:913`); `BranchResults.Length==3` (`:190`) |
| **integration** | реальные реестры flow/unit/topology + multi-router | адаптивный промпт → ≥1 ветка из топологии 7 фаз/31 суб-фаза (seeder) |
| **e2e-host** | путь через `LogicalOrchestratorAssistant.RunAsync` (15) | пользовательский промпт → `LogicalAssistantRunResult` с непустым `BranchResults` |
| **QA-automation** | человекочитаемость `FinalHumanResult`/warnings | warnings петли/домена читаемы, без internal-жаргона |
| **negative** | missing flow / missing topology / depth-limit | суб-фаза без flow → `KNOWLEDGE_EDGE` `missingFlow=true`; глубина=3 → warning (`:22`) |
| **untrained-honesty** | оркестратор не выдумывает выбор, когда не уверен | низкая уверенность → knowledge edge (`:1112`), НЕ случайный flow; baseline-роутер `isLearned:false` виден |
| **trained-acceptance** | (после блока 01/04) обученный multi-router держит fan-out; ablation | с обученным юнитом fan-out лучше baseline на held-out; permutation-invariance кандидатов; `CutoverProof`/`AblationProofRef` непусты |
| **regression** | существующие тесты L4 зелёные; baseline ≥1488 | поведение `RunAsync` не изменилось от введения `DynamicGraphTopology`-адаптера |
| **ux-automation** | (когда есть user-facing surface) человеческие метки/ошибки | warnings и `FinalHumanResult` без internal-жаргона на основном пути |

NEW-замечание: тесты untrained-честности и no-shortcut (permutation-invariance,
no-position-authority, no-source-id-leakage, fresh-load, continue-training,
D-17=0.0) — это **обязательная** часть приёмки владельца ребра (блоки 01/04, фазы
Ф4-Ф5-Ф8); оркестратор покрывается тем, что он корректно **делегирует** в обученный
юнит и честно деградирует.

---

## 10. Gate B (измеримое условие готовности блока) — exit-условие Ф1.3.2

Блок 03 считается готовым (как **план-решение L4**, exit-gate подшага доски
**Ф1.3.2**), когда **все** строки истинны:

```
G1  Псевдоним зафиксирован: ILearningOrchestrator == ILogicalOrchestrator (L4,
    LogicalContracts.cs:202); НОВЫЙ уровень не вводится (A-LM-12). = true
G2  Обход графа описан пошагово с file:line для: fan-out (:316), параллелизм
    (:465), очередь (:445/:525/:544), fan-in (:841), петля ReturnLoop (:361),
    knowledge edge (:1112). = 6/6
G3  Каждое ребро/решение имеет названного владельца авторитета и вердикт по скилу
    (таблица §6 + §8.1): fan-out=baseline→обучаемый; порядок=детермин. helper;
    knowledge-edge=honest. Скрытых «trained»-shortcut = 0.
G4  NEW-контракты графа (IDynamicGraphEdge, DynamicGraphTopology, per-edge
    authority) определены с сигнатурами и помечены NEW vs СУЩ. (§5), на КАНОНЕ
    master §A.4.1 (GraphEdgeKind = 5 членов; ParallelEligible = флаг; AuthorityUnitId
    = ссылка). Дрейф бенчмарка (6 членов / AuthorityUnitRef) исправлен. = true
G5  Доказано, что для подмены baseline обученным юнитом НЕ нужен новый решающий код
    в RunAsync (используется ResolveActiveUnit:269 + latest-wins, A-LM-03). = true
G6  Тест-стратегия покрывает слои FC-26, включая untrained-honesty и
    trained-acceptance (§9). = 10/10
G7  Гейты teacher/leakage/измерение привязаны к правильному уровню (teacher/
    leakage → DPO-строка владельца ребра, не L4; §8). = true
G8  Открытые архитектурные вопросы (уровень DynamicGraph, контракт ребра,
    обучаемость порядка) вынесены в §12 для Luba, не решены молча. = true
```

Измеримый порог: **G1..G8 = 8/8 истинно**, число скрытых детерминистических
финальных авторитетов на L4 без честной пометки = **0**, число введённых новых
архитектурных уровней = **0**, число членов `GraphEdgeKind` = **5** (канон).

---

## 11. Референсы (Stage 2: затрагиваемые проекты/доки/реконы/гайды)

### Реальный код (file:line, перепроверено чтением)
- `C:/Projects/llm_mvp_core/00.Core/LogicalContracts.cs` — `ILogicalOrchestrator`:202
  (`RunAsync`:204); `LogicalOrchestratorRequest`:174; `LogicalOrchestratorResult`(+
  `BranchResults`):182,190; `LogicalFlowResult`:131; `LogicalFlowSummary`:114;
  `LogicalContextPacket`:158; `LogicalContextFragment`:146; `PhaseTransitionSignal`:89;
  `ILogicalUnit`:97; `ILogicalFlow`:166; `LogicalExecutionFlags`:7 (`UseConfidenceFloor`:42,
  `UseLearnedUnitWhenAvailable`:43, `UseContextCarryForward`:45, `UseE35DependencyDetection`:47,
  `UseKnowledgeEdgeDetection`:51).
- `C:/Projects/llm_mvp_core/00.Core/LearningMachineContracts.cs` — иерархия+training-path:197-207;
  A-LM-02 коммент:8; `ILearningMachine`:208 (`GetOrchestratorAsync`:232); `LearningBatchResult`:43;
  `LearningEvaluationResult`:90; `LearningMachineExport`:112.
- `C:/Projects/llm_mvp_core/21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs` —
  `:10` класс; `MaxAdaptiveBranchCount`:19; `MaxTransitionDepth`:22; `RunAsync`:85;
  `RunWithCapabilityGenerationAsync`:158; `ClassifyDomainAsync`:207; `CompilePromptAsync`:244;
  `ResolveActiveUnit`:269; `RunExplicitScenarioAsync`:283; `RouteAsync`:316;
  `MultiSubPhaseRouterUnit.Id`:328; `FollowTransitionSignalsAsync`:361; `ExecuteBranchesAsync`:445;
  `Task.WhenAll`:465; `ExecuteBranchAsync`:496; `HasTransitionDependency`:522/:525;
  `OrderByTransitions`:541/:544; `DomainGraphClosure.HasAnyDependency`:531; `ExpandRoutedDecisions`:772;
  `FindBridgeSubPhases`:778; `TOPOLOGY_BRIDGE`:799; `CapRoutedDecisions`:808/:815;
  `BuildDynamicDomainTransitions`:825; `BuildAggregateResult`:841; `VerdictRank`:913;
  `ScheduleKnowledgeEdges`:930; `ExecuteBindingAsync`:967; `BuildContextPacket`:1032;
  `BuildKnowledgeEdgeFlowResult`:1112; `ValidateConstruction`:1236; `IsAdaptiveScenario`:1263.
- `C:/Projects/llm_mvp_core/21.AgentInfrastructure/Logical/MultiSubPhaseRouterUnit.cs` —
  `:8` класс; `SubPhaseAnchorTerms`:23-56; `Score`:249; веса:283; `BuildPathFreeScoringText`:291.
- `C:/Projects/llm_mvp_core/17.DynamicDomains/DynamicDomainGraph.cs` — `DynamicDomainTransition`:3;
  `DomainGraphClosure.HasAnyDependency`:8.
- `C:/Projects/llm_mvp_core/15.OrchestratorModel/Logical/LogicalOrchestratorAssistant.cs` —
  `CreateDefault` (регистрация flow/units/topology, latest-wins).
- `C:/Projects/llm_mvp_core/docs/DECISIONS-LOCKED.md` — A-LM-02, A-LM-03, A-LM-09, A-LM-12, A-LM-13, D-17.

### Затрагиваемые проекты
- `00.Core` (контракты L4 — не менять), `21.AgentInfrastructure` (движок + реестры +
  NEW-адаптер топологии), `15.OrchestratorModel` (потребитель/e2e-host),
  `17.DynamicDomains` (источник рёбер для `DynamicGraphTopology`-адаптера),
  `25.ModelServer`/`IModelRepository`/`IRemoteUnitLocator` (checkpoint/locator
  владельца ребра — блоки 01/04), `18.CapabilityTraining`
  (`DefaultLogicalCapabilityTrainer` / `ILogicalCapabilityTrainer`).

### Канон рёбер/фаз (evidence-only, НЕ редактировать)
- `codex-plan-00-partA-uml-status.md` §4 (канон `IDynamicGraphEdge`/`GraphEdgeKind` 5 членов),
  `codex-plan-02-edge.md` §5, `codex-plan-04-dynamicgraph-machine.md` §Canon/Gate B,
  `codex-plan-05-partA-hierarchy.md` §5.2, `codex-plan-12-no-hidden-determinism.md` §Canon/Gate B.
- `codex-EXECUTION-CONTROL-BOARD.md` (канон 12/26/59, Ф1.1.2/Ф1.3.1/Ф1.3.2),
  `codex-ILEARNINGDYNAMICGRAPH-PLAN-v1.md` (мастер §A.4.1/§A.4.2).
- `codex-guide-04-ilearningmachine-pattern-and-graph.md` (маппинг уровней, Graph-Style Gate),
  `codex-recon-core-15-logical.md`, `codex-recon-core-21-infra.md`, `codex-recon-core-17-dynamic.md`,
  `codex-recon-core-07-graph.md`.
- Бенчмарк этого блока: `codex-plan-03-orchestrator.md` (только чтение; дельта — §0).

### Связанные claude-блоки (мой namespace, согласованы)
- `claude-03-ilearningmachine-pattern-notes.md` (иерархия уровней),
  `claude-04-dpo-legal-model-policy.md` (легальный teacher / provenance),
  `claude-06-ilearningdynamicgraph-architecture-brief.md` (компоненты/данные/маршрутизация),
  `claude-01-authority-requirements-ledger.md` (binding requirements).

### Evidence-only (НЕ редактировать, не продолжать)
- `C:/Projects/llm_mvp_core/docs/plans/claude-ILEARNINGDYNAMICGRAPH-DPO-PLAN-v1.md` — чужой `claude-*` план.

---

## 12. Открытые вопросы для Luba (в финальную секцию плана)

- **Q1 (CANON, подтверждение уровня).** Подтверждаешь, что `ILearningDynamicGraph`
  = `ILearningMachine<DynamicGraphDomain>` (L5), а оркестратор остаётся
  `ILogicalOrchestrator` (L4) **без** введения нового 6-го уровня (A-LM-12
  закрыт)? Это ведущий вариант CANON; нужен явный «да». (Доска: Ф0.1.1.)
- **Q2 (контракт ребра, после исправления дрейфа).** Утверждаешь **канон** ребра
  `IDynamicGraphEdge` = `GraphEdgeKind` **5 членов** {FanOut, FanIn, ManyToMany,
  MultiOutput, ReturnLoop} + флаг `ParallelEligible` + `string AuthorityUnitId`
  (ссылка, latest-wins) + `DeterministicFinalAuthority=false`, как в
  `codex-plan-04/12` и мастере §A.4.1 (а **не** 6-членный enum с `Sequential/Bridge`
  и не `AuthorityUnitRef`, как было в `codex-plan-03`-бенчмарке)? Имена/поля можно
  поправить. (Доска: Ф1.1.2.)
- **Q3 (обучаемость порядка обхода).** Порядок рёбер (параллель/очередь) сейчас
  детерминистический (`DomainGraphClosure` `:531` + E35). Оставляем его
  baseline-инфраструктурой обхода (флаг `ParallelEligible`), **или** ты хочешь
  обучаемый предиктор зависимостей (тогда Jaccard E49/E50 → baseline для ablation,
  финал — обученный L2-юнит)? По умолчанию — оставить детерминистическим. (Доска:
  Ф0.1.2 Q14 `traversal_order_trainability_locked`.)
- **Q4 (per-edge authority storage).** Где хранить `AuthorityUnitId`/`BaselineUnitId`
  ребра: в топологии графа (как поле ребра, моё предложение) или в отдельном реестре
  владельцев? Влияет на блок 04 (export/import графа) и на резолв latest-wins.

---

*Конец claude-блока 03 (оркестратор L4). PLAN-ONLY: документация-решение внутри
плана ILearningDynamicGraph, мой namespace `claude-*`. Не разрешает реализацию,
тренировку, билд, запуск раннеров, git или вызов внешних моделей. Цитаты кода —
только реально существующие, перепроверены чтением, с `file:line`. NEW-типы
помечены; существующие — с `file:line`. Канон ребра выровнен на master §A.4.1
(исправление дрейфа бенчмарка `codex-plan-03`); счётчики 12/26/59 и якорь Ф1.3.2
сохранены. codex-* файлы — только чтение, не редактировались.*

---

## §ADD-03 — Additive blueprint: single→multi edge конвертер, depth-carrier, contract-on-execution-path

> **Additive**: инстанцирует мост одноцелевых transition → многоцелевое ребро, носитель
> глубины ReturnLoop, и честную классификацию контракта ребра (read-only view vs исполнимый).
> Прежние решения §1–§5 не меняются.

### §ADD-03.1 — Representation-Bridge: `DynamicDomainTransition(From,To)` → `IDynamicGraphEdge.ToNodeIds` (закрывает HIGH: fan-out канон описателен; `ListEdgesFrom` = заглушка-комментарий)

**Правило (LOCKED-add, Representation-Bridge):** источник на диске = старая одноцелевая форма;
NEW-тип требует инстанцированного конвертера старая→новая (правило группировки + присвоение Kind),
не заглушку-комментарий тела метода.

```text
// АГРЕГАЦИЯ N одноцелевых transition(From,To) в ОДНО многоцелевое ребро (LOCKED-add):
IReadOnlyList<IDynamicGraphEdge> ListEdgesFrom(string nodeId) {
  var outs = ListTransitions().Where(t => t.FromId == nodeId).ToList();   // все переходы из nodeId
  if (outs.Count == 0) return Array.Empty<IDynamicGraphEdge>();
  // ГРУППИРОВКА: все переходы из ОДНОГО FromId с ОДИНАКОВЫМ signal-kind → одно ребро с ToNodeIds[].
  var toNodeIds = outs.Select(t => t.ToId).Distinct().ToArray();
  // ПРИСВОЕНИЕ Kind (детерминированное правило):
  //   toNodeIds.Length > 1 && parallelSignal → FanOut;
  //   toNodeIds.Length > 1 && !parallel      → MultiOutput;
  //   toNodeIds.Length == 1 && backEdge(To предок From) → ReturnLoop;
  //   toNodeIds.Length == 1                   → Sequential (ToNodeIds = 1 элемент);
  //   (FanIn — со стороны целевого узла: >1 incoming в один To.)
  var kind = ResolveKind(toNodeIds, outs);
  return new[]{ new DynamicGraphEdge {
     ToNodeIds = toNodeIds, Kind = kind,
     AuthorityUnitId = ResolveAuthorityUnitId(nodeId, kind),   // резолв обученного владельца
     ParallelEligible = kind == FanOut, DeterministicFinalAuthority = false } };
}
```
Тело метода больше не `=> /* Edges.Where(...) */`; исполнитель имеет строку «как из
одноцелевых переходов собрать многоцелевое fan-out-ребро».

### §ADD-03.2 — Contract-On-Execution-Path: честная классификация (закрывает HIGH: NEW-контракт read-only, не на пути RunAsync)

**Правило (LOCKED-add, Contract-On-Execution-Path):** NEW-типизированный контракт либо реально
потребляется путём исполнения (traversal/RunAsync), либо честно помечен «read-only topology view в v1».

```text
// ЧЕСТНАЯ ПОМЕТКА v1 (LOCKED-add): IDynamicGraphEdge/DynamicGraphTopology в v1 =
//   READ-ONLY topology view. Реальный fan-out RunAsync (RouteAsync:316) продолжает
//   производить SubPhaseRoutingDecision[] и обходить decision-массив, НЕ Edges.
//   Gate B G4 (5 членов enum, канон ребра) проверяет ФОРМУ типа = read-only view — ЭТО и есть его scope.
//   E2E-исход «оркестратор обходит типизированный граф рёбер» в v1 достигается на уровне
//     SubPhaseRoutingDecision (реальный путь); типизированное ребро = описательный/провенанс-слой.
// ПРЕДУСЛОВИЕ активации «contract consumed by execution path» (deferred):
//   RouteAsync начинает читать IDynamicGraphEdge.ToNodeIds напрямую (не decision-массив) —
//   отдельная фаза, отдельный Gate B «contract consumed by execution path». В v1 НЕ обещано.
```
Так убрана иллюзия «оркестратор обходит типизированный граф» — v1 честно = read-only view +
реальный SubPhaseRoutingDecision-путь.

### §ADD-03.3 — Recursion-State-Carrier: носитель глубины ReturnLoop (закрывает HIGH: depth-носитель не определён; guard недоказан)

**Правило (LOCKED-add, Recursion-State-Carrier):** рекурсивный повторный вход требует явно
названного НОСИТЕЛЯ счётчика через границу вызова + глобального бюджета повторных входов.

```text
// Носитель глубины (LOCKED-add): ключ в Metadata запроса RunAsync.
//   const string DEPTH_KEY = "__transition_depth__";
//   int currentDepth = req.Metadata.TryGetValue(DEPTH_KEY, out var d) ? (int)d : 0;
// FollowTransitionSignalsAsync перед рекурсивным RunAsync:
//   if (currentDepth >= MaxTransitionDepth /*=3*/) → warning "Transition depth limit reached", переход пропущен;
//   иначе: var childMeta = new Dictionary<..>(req.Metadata){ [DEPTH_KEY] = currentDepth + 1 };
//          await RunAsync(req with { Metadata = childMeta });   // инкремент передан через границу
// ГЛОБАЛЬНЫЙ бюджет веера (LOCKED-add): total_reentries := branches × depth;
//   const int MAX_TOTAL_REENTRIES = 12 (=4 branches × 3 depth). При превышении → стоп веера
//   (не только per-chain лимит). Симуляция S10 «глубина=3 → warning» теперь имеет носитель:
//   depth растёт по Metadata[DEPTH_KEY], не всегда 0 (нет вечной петли), согласован между ветками.
```
