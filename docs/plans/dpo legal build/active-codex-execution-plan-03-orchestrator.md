## Current Code-First Reconciliation Overlay (2026-07-14)

current_implementation_authorization: YES_CODE_FIRST_ONLY
teacher_dpo_training_authorization: NO_TEACHER_DPO_TRAINING_UNTIL_G_DPO_FINAL
dpo_status: NOT_STARTED
dpo_priority: LAST
current_counters: 12 phases / 27 subphases / 64 substeps

## Historical Fable Coverage Repair Overlay (2026-07-05)

historical_fable_plan_readiness: PLAN_NOT_READY / FABLE_GAP_REPAIR_QUEUE
historical_fable_implementation_authorization: NO until fresh Luba approval after review
historical_fable_verdict_basis: Newer-at-that-time Fable coverage evidence `claude-fable-plan-check-v1.md` and attachment `98cbe69a` superseded the previous final-readiness overlay. Fable reported 127 findings: 91 COVERED, 30 PARTIAL, 6 MISSING. This is historical plan-repair evidence only, not current execution authority and not product/test/build/runtime/provider/training/checkpoint/git proof.
historical_fable_verdict_classes: DESIGN_BLOCKER_AT_THAT_TIME=FABLE_MISSING_AND_PARTIAL_ITEMS; REPAIR_QUEUE_AT_THAT_TIME=FABLE_RB_RG_AND_PARTIAL_REPAIR; FABLE_EVIDENCE=claude-fable-plan-check-v1.md lines 35-153 and attachment 98cbe69a lines 32-79; IMPLEMENTATION_AUTHORIZATION_GATE_AT_THAT_TIME=NO until fresh Luba approval after review.
historical_fable_classification_rule: Any older line in this file saying PLAN_READY_FOR_LUBA_REVIEW, Final Readiness Review Overlay, no unresolved design/dependency blocker, CURRENT_REPAIR_QUEUE=NONE_IDENTIFIED_BY_FINAL_READINESS_REVIEW, or parent final-readiness acceptance before this overlay is historical/pre-Fable evidence only unless it cites later authority.
historical_fable_gap_repair_addendum: active-codex-execution-42-fable-gap-repair-additions-20260705.md was the historical repair map for R-B/R-G and partial contract/readability/lifecycle additions; it is not current authority.
## Historical Pre-Fable Final Readiness Review Overlay (2026-07-05)
historical_pre_fable_plan_readiness: PLAN_READY_FOR_LUBA_REVIEW
historical_pre_fable_implementation_authorization: NO until fresh Luba approval after review
historical_pre_fable_final_readiness_review_scope: one bounded planning/governance readiness review only; no product implementation, tests, builds, runtime, browser, provider/API, model/training/checkpoint, git, branch, mergeability, PR, or product-readiness proof was run.
historical_pre_fable_current_verdict_basis: This final review read the required governance/skill gate markers, the two unique original-task/reference attachments, the minimum Claude baseline surfaces, the active-codex plan/board/matrix/source-map/legal/gate/ledger surfaces, and current status/search evidence. End-to-end mental simulation from the active plan found no unresolved design/dependency decision that an executor would have to choose before coding/testing. The prior PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE state after 0aa/Laplace/Kepler/Singer was retained for parent final-review acceptance; this overlay supplies that final bounded review. Remaining items are execution or approval gates only: fresh Luba implementation approval, local self-authored seed file creation, code implementation, tests/builds/runtime/training/checkpoint/fresh-load/ablation proof, same-scenario Path A/Path B host proof, mock/replacement-ledger proof, branch/mergeability/git proof if later authorized, final product review, and route-specific hosted TogetherAI/Qwen3 terms/output-use proof before hosted train/common use.
historical_pre_fable_current_verdict_classes: DESIGN_BLOCKER_CURRENT=NONE_IDENTIFIED_BY_FINAL_READINESS_REVIEW; CURRENT_REPAIR_QUEUE=NONE_IDENTIFIED_BY_FINAL_READINESS_REVIEW; CURRENT_LEDGER_ACCEPTANCE_BOUNDED_EVIDENCE=50/50 accepted planning sections after 2026-07-05 Pass37-Pass50/Singer repair, not 50 cycles, not 50 product reviews, not product proof, not test/build/runtime/provider/training/checkpoint/git proof; HOSTED_PROVIDER_PRE_USE_GATE_ROUTE_SPECIFIC=TogetherAI/Qwen3 proof required before hosted train/common use only, not a local self-authored v1 planning blocker; IMPLEMENTATION_AUTHORIZATION_GATE=NO until fresh Luba approval after review.
historical_pre_fable_classification_rule: Any older line in this file saying PLAN_NOT_READY / PLAN_PARTIAL_REPAIR_QUEUE, current verdict remains PLAN_NOT_READY, CURRENT_REPAIR_QUEUE, parent must run final readiness review, or no-go boundaries before this overlay is historical/pre-final-readiness-review evidence only unless it cites later authority.
---
artifact_type: ilearningdynamicgraph_plan_block_03_orchestrator
authority_source: latest unquoted Luba work order for DPO Legal Learning active execution chain
status: populated_now_planning_only
last_updated: 2026-06-29
write_scope: C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build\active-codex-execution-*.md
plan_phase_anchor: Ф1.3.2 (primary) + Ф1.1.2 (канон ребра) + Ф1.3.1 (флоу ребра)
canon_counters: 12 фаз / 27 подфаз / 64 подшага (Ф0…Ф11), нумерация Ф<P>.<SP>.<SS>
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
> счётчики плана: **12 фаз (Ф0…Ф11) / 27 подфаз / 64 подшага**, нумерация
> `Ф<P>.<SP>.<SS>`. Доска и мастер-план — evidence-only, здесь не правятся.

---

## 0. Якорь задачи и где этот claude-блок улучшает бенчмарк

```text
CURRENT TASK: написать МОЙ (claude-namespace) улучшенный блок-03 оркестратора —
              план-решение L4 для ILearningDynamicGraph, человеческим русским,
              Part A-формат, с сохранением структуры и канонических счётчиков
              (12 фаз / 27 подфаз / 64 подшага, Ф<P>.<SP>.<SS>) и CANON-инвариантов
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
| D2 | **Нет якоря фаз.** Бенчмарк построен как §1-12, но не привязан к каноническим `Ф<P>.<SP>.<SS>` (12/26/59). Читателю плана не видно, какой это подшаг доски. | Добавил явный якорь **Ф1.3.2** (+ Ф1.1.2/Ф1.3.1), счётчики 12/26/59 (historical/pre-PASS26 Claude-baseline; текущий канон 12/27/64), и связал Gate B G1..G8 с exit-условием Ф1.3.2 (§0, §10). |
| D3 | **Точность пары перегрузок file:line.** Бенчмарк цитирует `HasTransitionDependency`:525 и `OrderByTransitions`:541 как одиночные, хотя в коде у каждого по 2 перегрузки. | Цитирую обе: `HasTransitionDependency` :522 (диспетчер) / :525 (impl); `OrderByTransitions` :541 (диспетчер) / :544 (impl). Остальные ~25 цитат перепроверены чтением и подтверждены. |
| D4 | **Канон-консистентность имён DPO.** Бенчмарк ссылается на `GraphDpoPair`/`TeacherProvenance`, но не фиксирует, что L4 их **не несёт**. | Явно: на L4 этих полей нет; они живут на DPO-строке/боковом манифесте L5 (§8.3), согласовано с `codex-plan-00/04/05` и моими `claude-04/06`. |

CANON-инварианты сохранены: маппинг ролей Luba → реальные контракты; 5 закрытых
уровней (A-LM-12), DynamicGraph = специализация L5 (НЕ 6-й уровень); легальный
teacher-гейт; запрет скрытого детерминизма (A-LM-09/A-LM-13). NEW-типы помечены
`NEW`; существующие — с `file:line`.

**Current owner boundary (2026-07-14).** Текущие счётчики — **12/27/64**;
12/26/59 в D2 — только historical/pre-PASS26. L4 получает исключительно
immutable/versioned snapshot `DynamicGraphTopology` и имеет **read authority**
для одного `RunAsync`. Он не добавляет, не удаляет, не переписывает и не
публикует узлы/рёбра. Write/mutation authority принадлежит registry/mutation
pipeline вне L4; он атомарно публикует новую версию, которую следующий запуск
L4 может прочитать. Это же отделяет L4 от DPO: актуальная цепь
`IGraphDpoPairBuilderUnit -> GraphDpoPairCandidate -> triad/legal projection ->
GraphDpoPair(Level=unit)` живёт за границей оркестратора, а DPO остаётся
последней стадией со статусом `NOT_STARTED`.

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
`InMemoryLogicalUnitRegistry`, переходы проекта 17), которые mutation-пайплайн
вне L4 может менять между запусками. Оркестратор читает атомарно опубликованный
immutable/versioned snapshot и обходит только эту зафиксированную версию в
текущем `RunAsync`; он не имеет runtime write-authority над топологией. Поэтому
именно здесь живёт «обход динамического графа»: fan-out, параллелизм веток,
порядок по рёбрам, fan-in и re-entry. **Train-логики и mutation-логики тут нет и
быть не должно** (правило A-LM-02, `LearningMachineContracts.cs:8`): оркестратор —
чистый inference-координатор; перенастройка узлов — только на L5-машине, а
публикация новой topology-version — ответственность registry/mutation owner.

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

> **B-16 / F1-merge-carrier canon (owner: active plan-02 §5A.3 / edge).**
> The execution-facing fan-in contract is `IEdgeFanInMerger.Merge(branchResults, runId)`
> returning typed `GraphFanInMergeResult`, not a bare `LogicalFlowResult`.
> Required fields: `Merged`, `Context`, `FanInMergeRole MergeRole` with v1 value
> `FanInMergeRole.DeterministicFanInBaseline`, `bool MergeOrderIsAuthority` with
> required value `false`, `MergedBranchCount`, and preserved conflicts/rejected
> alternatives/provenance/trace/failure reason refs. Plan-03 does not own the merge
> algorithm; its existing `BuildAggregateResult` is the deterministic N-to-1 aggregation
> that the typed carrier makes explicit. A trainable fan-in arbiter is not introduced in v1.

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
    note for AdaptiveLogicalOrchestrator "СУЩ. 21.../AdaptiveLogicalOrchestrator.cs:10. L4 читает один immutable/versioned topology snapshot на RunAsync; topology write/mutation/publish authority отсутствует. Task.WhenAll:465 = параллелизм веток. Статус: РАБОТАЕТ; авторитет fan-out детерминистический (под-обучен, isLearned:false)."

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
    note for DynamicGraphTopology "NEW (в коде НЕТ). Immutable/versioned snapshot карты графа для L4-read. Registry/mutation owner вне L4 строит и атомарно публикует следующую версию через ListTransitions()/BuildDynamicDomainTransitions():825 + BuildDynamicTopology(signal,budget) блока 04. Статус: ПРОЕКТ."

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
| `DynamicGraphTopology` | Immutable/versioned snapshot карты графа; L4 только читает, mutation/publish выполняет внешний owner | NEW (ПРОЕКТ) |
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
| S11 | `o-- IDynamicGraphEdge` (NEW): канон 5 видов + multi-target | ребро `Kind=FanOut`, `ToNodeIds=["5.1","4.4"]`, `AuthorityUnitId="multi-router@learned"`, `ParallelEligible=true`, `DeterministicFinalAuthority=false` | топология отдаёт ОДНО ребро с ДВУМЯ целями (fan-out выражен `ToNodeIds`, а не одиночным `ToNodeId`); оркестратор резолвит обученного владельца через `ResolveActiveUnit` (`:269`) по `AuthorityUnitId`, baseline (`BaselineUnitId`) — fallback |
| S12 | `..> DynamicGraphTopology` (NEW) | внешний mutation-owner добавил ребро и атомарно опубликовал topology version `v+1` между запусками | текущий `RunAsync` завершает чтение своего immutable snapshot `v`; следующий `RunAsync` читает `v+1` через адаптер `BuildDynamicDomainTransitions` (`:825`). L4 ничего не записывает и алгоритм обхода не меняется |
| S13 | `o-- IDynamicGraphEdge` (NEW): носитель A-LM-09 на ребре | ребро `Kind=FanOut`, финальную власть держит детерминистический хелпер → `DeterministicFinalAuthority != false` | вердикт `FORBIDDEN_RUNTIME_SHORTCUT` (R4 / A-LM-09; синхрон с `codex-plan-12` Gate B и `codex-plan-04` Gate B); регистрация ребра/юнита блокируется до возврата `DeterministicFinalAuthority=false` (финал — обученный L2-юнит) |
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
    GraphEdgeKind Kind { get; }                // PASS21 active alignment: КАНОН-имя поля = Kind (владелец claude-plan-02 §5); EdgeKind — устаревший псевдоним поля, НЕ использовать
    bool ParallelEligible { get; }             // async-режим как ФЛАГ (бывш. AsyncParallel); НЕ финальный авторитет
    string AuthorityUnitId { get; }            // ссылка в реестр на обученного владельца ребра (latest-wins A-LM-03)
    string AuthorityUnitLevel { get; }         // уровень владельца (ожидается L2 ILogicalUnit)
    bool DeterministicFinalAuthority { get; }  // ОБЯЗАТЕЛЬНО = false в проде (носитель A-LM-09)
    string? BaselineUnitId { get; }            // baseline-генератор кандидатов (напр. MultiSubPhaseRouterUnit)
    string BaselineRole { get; }               // напр. "candidate_generator" | "ablation_baseline"
IReadOnlyList<string> ExcludedLeakageSignals { get; } // [candidate_index, source_path, source_id, file_order, correlation_id, exact_prompt]
    string SourceProof { get; }                // провенанс происхождения ребра
    string CutoverProof { get; }               // доказательство перехода baseline→trained
    string AblationProofRef { get; }           // ссылка на ablation-доказательство (Ф5/Ф8)
}

// NEW — immutable/versioned snapshot карты графа для одного L4 RunAsync.
// Registry/mutation owner ВНЕ L4 строит и атомарно публикует следующую версию
// поверх ILogicalTopologyRegistry.ListTransitions() и BuildDynamicDomainTransitions()
// (СУЩ. AdaptiveLogicalOrchestrator.cs:825), плюс вход
// BuildDynamicTopology(DynamicGraphBuildSignal, TopologyBuildBudget) (блок 04).
// L4 не получает Add/Remove/Update/Publish API и никогда не мутирует этот snapshot.
public sealed class DynamicGraphTopology
{
    public IReadOnlyList<IDynamicGraphNode> Nodes { get; init; }      // IDynamicGraphNode — блок 01 (NEW).
    //   P-02 (propagation): публичный many→many выход каждого узла — строго
    //   IQueryable<GraphNodeCandidate> (владелец формы — claude-plan-01-node §5/§N2/PASS10).
    //   Оркестратор — ПОТРЕБИТЕЛЬ этой формы: материализация РОВНО один раз на границе
    //   node→runner в IReadOnlyList<GraphNodeCandidate> внутри AsyncCandidateBatch
    //   (владелец материализации — claude-plan-06 §5.2/PASS09 B-11). Сырой object Output и
    //   финальный GraphNodeCandidate[] — НЕ канон (NO_OBJECT_OUTPUT). Здесь только ССЫЛКА
    //   на owner-shape (abridgement note), контракт узла/поля кандидата — в 01 §5.
    public IReadOnlyList<IDynamicGraphEdge> Edges { get; init; }
    // Рёбра, ВЫХОДЯЩИЕ из nodeId (фильтр по FromNodeId). Каждое такое ребро может
    // нести НЕСКОЛЬКО целей (edge.ToNodeIds) — это и есть fan-out/multi-output из одного узла.
    public IDynamicGraphEdge[] ListEdgesFrom(string nodeId) => /* Edges.Where(e => e.FromNodeId == nodeId) */;
}
```

### NEW — точки интеграции в оркестраторе (минимальная правка, в плане; НЕ исполнять)

Оркестратор НЕ переписывается. Для динамического графа достаточно:
- read-only snapshot-адаптер `DynamicGraphTopology` ← `_topology.ListTransitions()` /
  `BuildDynamicDomainTransitions()` (`:825`) — чтение рёбер как контракт. Адаптер
  фиксирует одну topology-version на весь `RunAsync`; публикация следующей версии
  выполняется вне L4 и не расширяет L4 write-authority;
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
> `bool DeterministicFinalAuthority` (в проде `false`). Расхождение этих полей
> между блоками — **дефект**, а не «деталь именования». Бенчмарк `codex-plan-03`
> использовал 6-членный enum (`+Sequential,+Bridge`) и `AuthorityUnitRef`; здесь
> это исправлено на канон (доска Ф1.1.2 Gate B прямо требует выровнять блок-03).

> **Единый канон выхода узла (P-02 propagation — чинит dropped-owner-type).**
> Оркестратор читает узлы через `DynamicGraphTopology.Nodes` (`IDynamicGraphNode`,
> блок 01) и потому является **потребителем** публичной формы выхода узла. Эта форма —
> **строго `IQueryable<GraphNodeCandidate>`** (владелец формы — `claude-plan-01-node`
> §5/§N2/PASS10; record `GraphNodeCandidate` — тоже владелец 01 §5.2). Оркестратор не
> переобъявляет и не сужает этот тип; для fan-out-скоринга baseline-генератор
> (`MultiSubPhaseRouterUnit`, `SubPhaseRoutingDecision[]`) остаётся эквивалент-кандидатом
> ablation, а **обученный** L2-владелец ребра отдаёт кандидатов в canon-форме узла.
> Материализация queryable-набора происходит **РОВНО один раз** на границе node→runner в
> `IReadOnlyList<GraphNodeCandidate>` внутри `AsyncCandidateBatch` (владелец материализации —
> `claude-plan-06` §5.2/PASS09 B-11; точка входа `IGraphCandidateRunner.RunCandidatesAsync`).
> Сырой `object Output`, untyped-массивы и финальный семантический `GraphNodeCandidate[]` —
> **НЕ канон** (`NO_OBJECT_OUTPUT`, 01 §5 / 06 §5.2). Это abridgement note: здесь только
> ссылка на owner-shape; полный контракт узла и поля кандидата — в 01 §5. Расхождение этой
> формы между блоками — **дефект**, а не деталь именования. СТАТУС: графовых типов в коде
> ещё НЕТ (проект); мержабельность формы в оба базовых плана — hard-требование канона.

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
  Финал должен принимать **обученный** `ILogicalUnit` (блок 01; вход исключает
  полный leakage-6 `[candidate_index, source_path, source_id, file_order,
  correlation_id, exact_prompt]`), а словарь/HybridRanker — baseline-генератор
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
- **Должно стать:** NEW-вход обученного fan-out-юнита наследует это правило и
  исключает полный leakage-6.
- **Как менять:** зафиксировать `ExcludedLeakageSignals=[candidate_index,
  source_path, source_id, file_order, correlation_id, exact_prompt]` на ребре
  (`IDynamicGraphEdge`) и в GraphDpoPair (блок 04).

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
| `IDynamicGraphEdge` / immutable-versioned `DynamicGraphTopology` snapshot (канон 5 видов + `AuthorityUnitId`) | — | ОТСУТСТВУЕТ (NEW, проектируется здесь); L4 read-only, mutation/publish owner находится вне L4 |
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

- Запрещённые сигналы на ребре fan-out — ровно полный leakage-6:
  `[candidate_index, source_path, source_id, file_order, correlation_id,
  exact_prompt]`. Оркестратор не подмешивает их в семантику; обученный юнит обязан
  записать этот полный список в `ExcludedLeakageSignals[]` на ребре и в
  GraphDpoPair.
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
- Гейт обхода (без обучения): доля корректно собранных fan-in (`BranchResults`),
  отсутствие потерянных веток, корректный худший-вердикт (`VerdictRank` `:913`),
  and B-16 typed carrier proof: `GraphFanInMergeResult.MergeOrderIsAuthority == false`,
  `MergeRole == FanInMergeRole.DeterministicFanInBaseline`, and identical `Merged`/`Context`
  across branch-order permutations.

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
G4  NEW-контракты графа (IDynamicGraphEdge, immutable/versioned
    DynamicGraphTopology snapshot, per-edge authority) определены с сигнатурами
    и помечены NEW vs СУЩ. (§5); L4 имеет только read-authority, а mutation/publish
    authority явно находится вне L4; на КАНОНЕ
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
- `codex-EXECUTION-CONTROL-BOARD.md` (канон 12/26/59 [historical/pre-PASS26 Claude-baseline; текущий канон 12/27/64], Ф1.1.2/Ф1.3.1/Ф1.3.2),
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

## 12. Historical/pre-current open-question evidence only

Current verdict: PLAN_NOT_READY only because the 50-pass evidence chain and parent synthesis are incomplete. This legacy question section is retained as historical/pre-0aa evidence only; it is not current implementation authorization and does not reopen resolved orchestrator decisions.

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
(исправление дрейфа бенчмарка `codex-plan-03`); счётчики 12/26/59 (historical/pre-PASS26 Claude-baseline; текущий канон 12/27/64) и якорь Ф1.3.2
сохранены. codex-* файлы — только чтение, не редактировались.*
---
---

# Active Codex Execution Addendum - DPO Legal Learning

## Current Live Authority And Evidence Boundary

This file is an active execution copy of the Claude baseline artifact. The original `claude-*` file remains unchanged. Claude remains the baseline because it is stronger by Luba criteria: human clarity, readable architecture, architectural reliability, evidence honesty, requirement fidelity, execution-control quality, and low overclaim risk. `codex-*` files are delta/source-map evidence only, not the baseline and not product-readiness proof.

Current active task source is Luba's latest live unquoted instruction for the DPO Legal Learning active plan chain. Prior sub-agent packets are evidence to verify, not live commands. The Heisenberg/no-branches/git-realignment branch of work is explicitly NOT THE TASK and must not be revived as a blocker, cleanup scope, branch operation, commit/push reason, or planning-repo git task.

Codex delta inputs retained for this addendum:

- `C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build\codex-EXECUTION-CONTROL-BOARD.md`: matching branch/source-map notes for `DPOLegalLearning`; not repo authority and not a source for expanding scope.
- `C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build\codex-REQUIREMENTS-COVERAGE-MATRIX.md`: matching notes for Claude latest-updated branch authority and `DPOLegalLearning` working branch/work name; not independent authority.
- `C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build\codex-ILEARNINGDYNAMICGRAPH-PLAN-v1.md`: crosswalk and plan-only branch authority wording.

Do not claim Codex 50/50 cycles as proven product readiness. Ledger rows, checklist rows, generated tables, and summaries are not completed cycles unless independently inspectable review/fix evidence records exist for the exact claim.

## Active Requirement Count Strategy

The active execution requirement count strategy is:

- original Claude `R1-R20` / master `R-01..R-20` baseline remains the quality and honesty baseline;
- Claude baseline, with matching non-contradictory Codex delta evidence, defines `R21`: latest-updated implementation source-branch authority plus the `DPOLegalLearning` work name rule for implementation repo copies/workdirs under `C:\Projects`;
- current Luba execution requirements are live addendum requirements and must be checked by pre-implementation, pre-commit, pre-push, and before-stop gates;
- the prior no-branch planning-repo policy packet is quarantined evidence only and is not an active task, stop gate, branch-cleanup requirement, or permission to touch planning-repo git state.

This strategy is an execution-control strategy, not product-readiness proof.

## R21 - Source Authority, Working Directory, And DPOLegalLearning

For each participating repository/source role before implementation:

1. Identify the source authority as the most advanced/latest-updated branch in that repository at the time work starts, using read-only git commands.
2. Treat the source authority branch as read-only authority, not as the implementation work branch.
3. Because `C:\Project` does not exist and `C:\Projects` exists in this environment, use `C:\Projects` as the implementation root. Create or use a separate working directory for the work name `DPOLegalLearning`, with repo copies under `C:\Projects\DPOLegalLearning\<repo-name>` unless an existing active plan row names a more specific safe `C:\Projects` path.
4. The controlling repo-by-repo authority/source-to-plan table is `active-codex-execution-02-source-inventory-and-reference-map.md` R21.1. During future implementation, record the concrete repo path or URL, branch-list source, selected latest-updated branch, selected branch timestamp/ref evidence, divergence note, separate `C:\Projects` working directory, and work name `DPOLegalLearning`.
5. Participating Claude-baseline route/source identifiers are `llm_mvp_core` (core implementation), `xiigen-platform-dot-net-angular` (public API facade for R16/R17), `xiigen-flows-dot-net-angular` (FLOW-31/FLOW-38 internal seam), and client/caller labels `FigmaToCode-design-gen` + `CanvaToCode` (R16 html/css clients). FigmaToCode/CanvaToCode are not implementation-repository authority for the platform host unless a later exact source proves that. Codex may be used only as matching delta/source-map evidence. This list must not be expanded from discovered/copied workdirs unless Luba explicitly expands repo scope.
6. Do not switch, create, delete, rebase, merge, cherry-pick, push, or open PRs in `C:\Users\lubao\Documents\xiigen`. That planning location may be read only as an evidence source when needed; no git realignment or branch cleanup is part of this chain.

## Execution Actor And Stop Gate

Future implementation, if later authorized, must run through bounded sub-agent work packets, requested model/mode `gpt-5.5` / `xhigh` (user wording: gpt 5.5 extra high). Parent orchestrates and no unbounded executor is allowed. Each packet must name exact scope, allowed files/repos, max time, required report packet, accepted evidence, and forbidden actions. A spawned bounded sub-agent is already the delegated execution actor for its allowed scope and must not invoke nested Codex/Claude/CLI agents unless the work order explicitly asks for nested delegation. Ledger rows, tables, summaries, and self-review are not fake cycles and cannot be reported as sub-agent passes.

Stop is allowed only when the product is fully ready, or when a true external blocker remains after mitigation/discovery and decision-prep. Before any stop, the worker must check whether the active plan is complete. If incomplete, the worker must identify the loophole that would cause premature stop, repair the governing plan/gate/skill text inside the allowed scope, and continue the nearest safe authorized work. NOT READY, NEEDS_FIX, review findings, partial evidence, and missing verification are bounded work queues, not completion claims.

## Pre-Commit And Pre-Push Merge/PR Conflict Gate

Before every commit and before every push during implementation:

1. Re-read the current source authority branch for each participating implementation repo because the source branch may advance in parallel.
2. Validate that merge/PR into the original source authority branch copied from has no conflicts.
3. Record the exact command or tool evidence used for the no-conflict result.
4. If conflicts exist, stop the commit/push path, repair or prepare a decision packet, then re-check against the current source authority.
5. Do not create PRs and do not merge unless Luba gives a separate future instruction.

This gate does not authorize any branch operation, commit, push, merge, PR, or cleanup in `C:\Users\lubao\Documents\xiigen`.

## External APIs And Mock-Only Technical Debt Gate

Future implementation, if later authorized, must handle every external API/resource path through an explicit seam/interface/adapter boundary, mock the expected external response instead of making the real external call, and add a unit test that asserts exactly the expected mock response/result. Record replacement of the mock with the real external resource call only in:

`C:\Users\lubao\Documents\xiigen\llm mvp\DPO LEARNING\codex-external-calls-mock.md`

This mock replacement record is the only allowed technical debt. No other technical debt may be introduced under this active plan. Mock-only evidence must not be reported as real external integration readiness.

## Current Worker Execution Boundary

Current 2026-06-29 planning-repair boundary: Luba's latest unquoted instruction says `План не готов к выполнению - нужно починить`. This worker is authorized only to repair active plan files in this folder. Product implementation, implementation workdir setup/copying, repo inventory, tests/builds, git actions, commits, pushes, PRs, merges, and mock-debt file edits are NOT AUTHORIZED in this task. Any earlier wording that implied starting implementation after coherence is superseded until a later plan-readiness gate truly passes and Luba gives fresh implementation authority.

Source `claude-*` and `codex-*` files remain read-only. In this planning-repair task, PRs, merges, commits, pushes, branch switches, workdir copying, and implementation repo edits are forbidden. Future implementation commits/pushes may be considered only in separate implementation workdirs after plan readiness, a bounded sub-agent work order, and the mergeability/no-conflict gate pass; PRs and merges still require separate future Luba instruction.

