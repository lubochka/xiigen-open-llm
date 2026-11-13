# Codex Recon — Эксперименты оркестрации: что мерили

Корпус: `llm mvp` (внешний). Slug: `mvp-orch-exp`. Релевантность к ILearningDynamicGraph: ВЫСОКАЯ.
Это evidence для плана ILearningDynamicGraph, НЕ разрешение на реализацию.

---

## 1. Что это и где (точные ключевые пути)

Область: `C:/Users/lubao/Documents/xiigen/llm mvp/orchestrator experiment/`

Главный эксперимент оркестрации — **E53** («Dynamic Flow Orchestrator Validation»): конвейер из 9 узлов
(node-00 … node-09), который декомпозирует сложную мультидоменную задачу, планирует её с зависимостями,
ловит нарушения ограничений арбитром, честно помечает «knowledge edge» (не выдумывает ответ) и форматирует
результат под разные аудитории.

Ключевые файлы:
- `orchestrator experiment/E53-ORCHESTRATION-PROOF-PLAN-v1.md` — дизайн: ЧТО доказывает эксперимент,
  3 сценария (A конфликт зависимостей, B knowledge edge / PSD2 SCA, C цепочка из 5 зависимостей),
  9 узлов, критерии успеха (§ Success Criteria, строки 361-383).
- `orchestrator experiment/MASTER-EXPERIMENT-PLAN-v1.md` — карта 14 экспериментов; здесь же (строки 64-72)
  список **12 стабильных метрик** E53 и матрица замены фикстур узлов на обученные юниты (Rounds D-K).
- Результаты раундов (формат метрик одинаков), напр.:
  - `orchestrator experiment/E53-ROUND-D/E53-ROUND-D-VALIDATION-RESULTS.md` (node-00 → trained compiler)
  - `orchestrator experiment/E53-ROUND-F/E53-ROUND-F-VALIDATION-RESULTS.md` (node-08 → trained arbiter)
  - `orchestrator experiment/E53-ROUND-G/E53-ROUND-G-VALIDATION-RESULTS.md` (node-09 → trained formatter)
  - аналогично E53-ROUND-E / -H / -I / -J / -K + `BATCH-ROUND-SUMMARY.md` в каждом.
- Контракт юнита (КОД): `orchestrator experiment/round-zips/stage-phase-25-logical-unit-contract-2026-05-04/code/35.ExperimentsV2/LogicalFlow/LogicalContracts.cs`
  — определяет `ILogicalUnit<TInput,TOutput>`, `LogicalUnitRequest<TInput>`, `LogicalUnitResult<TOutput>`,
  `LogicalExecutionFlags`.
- Расширение на фазы (граф фаз): `orchestrator experiment/MULTI-PHASE-ORCHESTRATION-DESIGN-AND-EXPERIMENT-PLAN-v1.md`
  — Phase Context Packet + Phase Transition Gate (PASS / PASS WITH GAPS / BLOCK).
- UML рантайма потока: `orchestrator experiment/dynamic_flow_runtime_uml.svg`,
  `.../dynamic_flow_data_layer_uml.svg`, `.../continuous_learning_integration.svg`.

---

## 2. Как устроено / что именно мерили (кратко)

Замеры делятся на ДВА класса.

**(A) Количественные метрики стабильности (главный измеритель E53).**
Идея: 9 детерминированных фикстур-узлов по одному заменяют на обученные юниты (Flow-01 adapters),
а 12 метрик оркестрации и набор тестов должны остаться БЕЗ ИЗМЕНЕНИЙ. «Дрейф» любой метрики = находка.
12 метрик (из MASTER + результатов раундов):
- `domain packet count` = 11 (11 доменных пакетов);
- `plan item count` = 11;
- `arbiter verdict count` = 7;
- `lost_edge_count` = 0 (ни одно ребро зависимости не потеряно);
- `dependency_violation_count` = 0 (порядок зависимостей не нарушен);
- `invented_biometric_plan_items` = 0 (узлы не выдумали покрытие knowledge edge);
- `duplicate_creation_accepted` = false;
- `forbidden_stack_term_count` = 0 (результат stack-independent, без C#/кодовой лексики);
- `human_result_sections_present` = 5/5;
- `live_external_calls` = 0 (никаких внешних AI-вызовов);
- `learning_gap_packet_count` = 4 (4 пакета «недостающей компетенции»);
- `e2e_orchestration_success` = true.
Плюс тестовые гейты: фокус-набор E53 `12/12 passed`, полный набор `997/997 passed`, build zero errors.
Сравнение раунда с раундом — поузловая таблица «Signal name | prior | this | changed»
(см. ROUND-D BATCH-ROUND-SUMMARY § Section 3).

**(B) Качественная оркестрационная способность (через оракул-контракты, ручной trace).**
Мерили НЕ числами, а сверкой с ground-truth контрактом сценария:
- правильный доменный split (node-01);
- правильный граф зависимостей и поимка инвертированной зависимости арбитром (node-03 + node-07/08);
- честная идентификация knowledge edge без выдумывания (node-06/09), напр. PSD2 SCA;
- разные форматы под разные аудитории на одних и тех же результатах (node-09: architect vs product/CTO/eng).
Каждый узел оценивается по рубрике; различают «orchestration failure» (узел принял неверное решение)
и «training gap» (узла не обучали на паттерне). Это формат E51A/E51B-evidence.

Раунды E53 (D→K) подтверждают: при замене каждого узла на обученный — ноль дрейфа всех 12 метрик
и сохранение типов арбитражных вердиктов (rejection / boundary / learning-gap / pass).

---

## 3. Релевантность к ILearningDynamicGraph

Очень высокая — это фактически готовый эталон цели проекта.
- **Граф/оркестрация:** 9-узловой конвейер = многоузловой граф с fan-out/fan-in (тесты
  `E53_Run_PreservesArrayFanOutAndFanInCounts`, `E53_Run_PreservesAllHardEdges`,
  `E53_Run_EnforcesDependencyOrder`) — прямой материал для Graph-Style/DynamicGraph Reconstruction Gate
  (nodes/edges/fan-out/fan-in/authority-per-edge).
- **DPO/обучение:** E53-ORCHESTRATION-PROOF-PLAN перечисляет per-node обучающие пары (27 новых пар,
  6 файлов узлов) и явно называет «эти пары — стартовая точка DPO». Узлы node-01/03/07/08/09 — кандидаты
  на обучаемые юниты; node-04..06 допускают human-proxy.
- **Измерение:** готовый набор из 12 метрик + двух-классовая классификация (orchestration failure vs
  training gap) + «metric drift = находка» — прямой шаблон для acceptance/ablation-гейтов ILearningDynamicGraph.
- **Граф фаз:** MULTI-PHASE-плана Phase Context Packet (decisions/constraints/knowledge edges/boundaries/
  rejected) и Phase Transition Gate (constraint/decision/knowledge-edge/rejection preservation) — модель
  передачи состояния по рёбрам между фазами-подграфами.
- **teacher-provenance:** в самих E53-артефактах teacher/distillation-источник DPO-пар НЕ задокументирован
  (см. Пробелы) — это зона, которую план обязан закрыть провенанс-полями скила.

---

## 4. Классификация (по trainable-architecture-decision-SKILL.md)

Замеренный артефакт — это эксперимент/контракт + код-контракт юнита, поэтому классифицирую найденный код:

- `ILogicalUnit<TInput,TOutput>` (LogicalContracts.cs, строки 108-112) — **разрешённый уровень
  ILogicalUnit**. Совпадает с иерархией CLAUDE.md (Level 2, async-first `ExecuteAsync`). Узлы node-00..09,
  обёрнутые как `Flow01NodeXX...` адаптеры (напр. `Flow01Node00TrainedLogicalCompiler`,
  `Flow01Node03TrainedSchemePlanner`), реализуют этот уровень. Verdict: `OK_ALLOWED_LEVEL` (как контракт).
- 9-узловой конвейер E53 как целое — кандидат на **ILogicalOrchestrator / ILogicalFlow** (Level 4/3):
  последовательность + fan-out/fan-in + арбитраж. В коде это `E53HighLevelOrchestrationProof.cs`
  (вне области рекона, в `llm_mvp_core`); здесь — только описание, не сам класс-оркестратор.
- `LogicalExecutionFlags` (строки 6-83): флаги `UseDomainScopedRetrieval`/`UseConfidenceFloor`/
  `UseLearnedUnitWhenAvailable`/`UseKnowledgeEdgeDetection` и т.д. — **внутренние детерминированные
  настройки/политика исполнения**, не отдельный обучаемый уровень. По скилу: внутренняя деталь внутри
  разрешённого уровня; KNN/retrieval здесь — компонент модели/память (audit-triggered, не запрещён),
  а НЕ самостоятельная архитектура. Verdict: `OK_INTERNAL_DETAIL`.
- Метрики/детерминированные фикстуры узлов (baseline до замены) — **eval/baseline only**: они служат
  оракулом и базой сравнения, не выдаются за обученную семантику. По скилу — `KEEP_FOR_BASELINE_ONLY` /
  `BASELINE_MUST_BE_LABELLED` (в раундах помечены как deterministic fixture, что корректно).
- Внимание (потенциальный `FORBIDDEN_RUNTIME_SHORTCUT` при переносе в продакшн): арбитры/селектор
  не должны решать по доменным меткам вместо формальных результатов — это прямо отмечено как риск в
  T4 failure modes (MASTER строки 360-363). Для ILearningDynamicGraph это leakage-сигнал, требующий
  ablation/permutation-теста на ребре.

Оговорка: финального кода узлов-оркестраторов я в области рекона не видел (только контракт ILogicalUnit
и описания в планах/результатах), поэтому уровневые выводы по конвейеру — из документации, не из кода класса.

---

## 5. Пробелы / что доисследовать

- **Teacher/provenance DPO-пар не задокументирован** в E53-артефактах: нет полей `teacher_access_mode`,
  `hosting_provider`, `model_id`, `model_license`, `closed_proprietary_model`. Критично для Legal Teacher Gate —
  доисследовать происхождение пар в `clean-batch-001/002/003` (откуда взяты CB1-*/CB2-*/CB3-*).
- **Сам алгоритм обучения юнитов** (как именно `Flow01NodeXX*Trained...` учится из пар, какое типизированное
  состояние, checkpoint/manifest/export-import/locator/continue-training) в области рекона НЕ виден —
  результаты лишь утверждают «trained adapter uses learned signals». Нет evidence жизненного цикла
  (fresh-load / continue-training / ablation). Доисследовать в `llm_mvp_core` (15.OrchestratorModel,
  25.ModelServer, 35.ExperimentsV2) — вне данной области.
- **Per-edge authority и async-исполнение графа** (Graph Reconstruction Gate) не описаны явно как контракты
  рёбер; есть только агрегатные метрики lost_edge/dependency_violation. Нужны input/output-контракты на ребро,
  many-to-many, multi-output edge contracts.
- **Числовые метрики качества обучения** (accuracy/loss/reward margin, CPU/RAM/время, row counts) в E53
  отсутствуют — измерялась стабильность (drift=0) и сверка с оракулом, а не numeric learned-метрики.
  Для completion-evidence ILearningDynamicGraph этого недостаточно — план должен добавить numeric-гейты.
- Не читались целиком: Category 2/3/4 эксперименты (G1-G6, T1-T5, M1) и T34/P3-раунды фаз-переходов —
  при углублении в граф фаз начать с `MULTI-PHASE-...` + `T34-PHASE-TRANSITION-ROUND-A-*` оракулов.
