# Рекон: корпус скилов `llm mvp/all-skills` — DPO / обучение / измерение

**Задача:** evidence для плана ILearningDynamicGraph. Обзор того, что в корпусе скилов есть про DPO, обучение, измерение, граф/оркестрацию/flow и provenance учителя. Только чтение, без исполнения.

---

## 1. Что это и где (точные ключевые пути)

Корпус: `C:/Users/lubao/Documents/xiigen/llm mvp/all-skills/` — большой набор `*-SKILL.md` (>100 файлов; Glob по области был обрезан, полный список не считан). Это translated-копии XIIGen/planning скилов, адаптированные под LlmFromScratch. Большинство — про процесс (планирование, ревью, тесты, completion), а не про код.

Прочитано под вопрос (8 самых релевантных файлов):

- `all-skills/dpo-training-data-SKILL.md` — формат training-pair эксперимента (Input/Rejected/Chosen/Delta), минимум 1 пара на эксперимент, правила «испорченной» пары.
- `all-skills/planning-dpo-authoring-SKILL.md` — JSON-формат пары планировочного решения (chosen/rejected/outcome_was_positive), когда решение достойно пары.
- `all-skills/dpo-triples-to-experiment-pairs-SKILL.md` — мост: XIIGen DPO-тройка (prompt, chosen, rejected) ↔ эксперимент-пара (baseline, enhanced); пример кода `ExperimentResult`.
- `all-skills/learning-loop-closure-SKILL.md` — эксперимент без обновления learning-signal = измерение, не обучение; будущая петля `PASSED → DpoPair → 11.DPO fine-tuning` (11.DPO заявлен как уже существующий).
- `all-skills/learning-signal-capture-SKILL.md` — 6 типов сигнала (DELTA, DOMAIN_PAIR, STAGE_LATENCY, COMPRESSION_RATIO, CLASS_DISTRIBUTION, FAILURE_PATTERN); DOMAIN_PAIR → будущий DPO.
- `all-skills/improvement-measurement-SKILL.md` — нельзя заявлять улучшение без наблюдаемой дельты (BEFORE/AFTER/OBSERVABLE DELTA/EVIDENCE COMMAND); список ложных улучшений.
- `all-skills/static-to-graph-refactoring-SKILL.md` — апгрейд хардкод-порогов к графовому обучению; запись решений в event-лог под будущий `07.GraphEngine`; «не удалять статический fallback».
- `all-skills/graph-rag-integration-SKILL.md` — graph/search только через fabric-интерфейсы (IVectorIndex/IGraphEngine/IHybridRetriever/ISearchEngine); Phase 1 = single-hop.
- `all-skills/flow-design-cycle-SKILL.md` — цикл улучшения алгоритма (DESIGN → BASELINE → IMPROVEMENT cycles → PASSED/FAILED), детектор «stall» (3 SCALE_UP без дельты).

Также **бегло (skim)**: `C:/Projects/llm_mvp_core/docs/HOW-TO-USE-SKILLS.md` (присутствует, v27.5) — правила классификации уровней и активационные триггеры.

Найдены, но **не читались** (защита от перегрузки, отметить для дочиследования): `learning-signal-capture` соседи по графу — `graph-entity-schema-SKILL.md`, `skill-graph-sync-SKILL.md`; по flow — `flow-plan-review`, `flow-examination`, `flow-retrospective`, `flow-lifecycle-translated`, `cross-flow-dependency`, `requirement-to-flow`; по оркестрации — `escalation-orchestrator-SKILL.md`; измерение/способности — `capability-measurement-SKILL.md`; данные — `experiment-data-integrity-SKILL.md` (на него ссылаются 3 DPO-скила); `dpo-triples-to-experiment-pairs` упоминает `model-selection-SKILL.md`.

---

## 2. Как устроено / работает (кратко)

- **DPO в этом корпусе = «эксперимент-пара», не классический preference-DPO.** Ядро: захватить разницу old/new поведения явно, чтобы из неё можно было учиться. Тройка XIIGen `(prompt, chosen, rejected)` переведена в пару эксперимента `(input, baseline=rejected, enhanced=chosen, delta)`. Хранится текстом в `docs/EXPERIMENT-RESULTS-MAP.md`, а не как обучающий JSONL-датасет.
- **Обучение (петля) пока разомкнуто.** `learning-loop-closure` прямо пишет: сегодня петля заканчивается на записи результата; реальный fine-tuning (`10.LoRA`, `02.DatasetPipeline`, `11.DPO`) — это «FUTURE / when 38.ContinuousLearning is built». То есть в корпусе скилов нет описания реального тренера/чекпойнта/экспорта — только формат-заготовка под будущий пайплайн.
- **Измерение — строгий honesty-гейт.** `improvement-measurement` запрещает называть улучшением рост числа тестов / падение grep-счётчиков / правки скилов; требует наблюдаемую пользователем/тестом дельту с командой-доказательством. `learning-signal-capture` требует объявить тип сигнала и проверять CLASS_DISTRIBUTION (анти-коллапс данных).
- **Граф — пока инфраструктурный, не «обученный».** `static-to-graph-refactoring`: хардкод-порог классифицировать по four-tier, перевести сначала в конфиг (со статическим fallback из эксперимента), писать решения в event-лог, и «когда подключат `07.GraphEngine`» — это станет обновлением confidence рёбер. `graph-rag-integration`: всё через fabric; Phase 1 = single-hop, multi-hop запрещён. То есть граф здесь = knowledge-graph фабрика (lineage экспериментов/решений), а не обучаемый граф маршрутизации.
- **Flow/cycle — про итеративное улучшение алгоритма**, не про runtime-оркестрацию узлов. Терминология «node/flow/cycle» в этих файлах относится к процессу разработки эксперимента (DESIGN→BASELINE→IMPROVEMENT), а не к ILogicalFlow/ILearningDynamicGraph рантайму.

---

## 3. Релевантность к ILearningDynamicGraph

(DPO / обучение / граф / оркестрация / измерение / teacher-provenance)

- **DPO-формат (средняя↔высокая):** три DPO-скила дают готовый словарь полей для авто-построения пар — `input/prompt`, `chosen`, `rejected`, `delta`, `outcome_was_positive`, `outcome_evidence`, плюс правила «испорченной пары» (rejected==chosen, отрицательная дельта, синтетический input, baseline против багованного алгоритма). Это прямой вход для авто-построения DPO под ILearningUnit/Flow/Orchestrator/Graph. **НО** это формат для записи в `EXPERIMENT-RESULTS-MAP.md`, а не схема обучающего датасета/тренера — для DynamicGraph план должен будет определить настоящую DPO-схему сам (этот корпус её не задаёт).
- **Обучение/петля (средняя):** `learning-loop-closure` + `learning-signal-capture` дают принцип «измерение ≠ обучение» и 6 типов сигнала — полезно как гейт, чтобы авто-построитель DPO реально замыкал петлю, а не накапливал мёртвые пары. Реального lifecycle (checkpoint/export/import/continue-training/locator) в корпусе скилов нет — он ожидается из CLAUDE.md/`llm_mvp_core`, не отсюда.
- **Измерение (высокая как гейт):** `improvement-measurement` — прямой анти-masquerade гейт для отчётов DynamicGraph: любое заявление «граф научился/улучшил» обязано иметь наблюдаемую дельту и команду-доказательство; запрещено выдавать рост счётчиков за обучение.
- **Граф/оркестрация (низкая↔средняя, с оговоркой):** `graph-rag-integration` и `static-to-graph-refactoring` важны как правила границ (fabric-first, single-hop Phase 1, не удалять статический fallback, писать решения в event-лог), но они описывают **knowledge-graph фабрику и конфиг-пороги**, а НЕ обучаемый граф маршрутизации с per-edge trainable authority. Для ILearningDynamicGraph (узлы/рёбра, many-to-many, fan-out/fan-in, async, trainable-vs-deterministic per edge) этот корпус прямого блюпринта не даёт — нужен `trainable-architecture-decision-SKILL.md` + `human-readable-architecture-audit` из `llm_mvp_core` (см. триггеры HOW-TO-USE-SKILLS строки 327–347).
- **Teacher-provenance (отсутствует здесь):** в этом корпусе скилов полей provenance учителя (`teacher_access_mode`, `hosting_provider`, `model_id`, `model_license`, `teacher_output_used_for_training`, `closed_proprietary_model`) НЕТ. Они заданы только в `llm_mvp_core/docs/HOW-TO-USE-SKILLS.md` (активационный триггер «Any DPO/distillation teacher output», строка 330) и в `trainable-architecture-decision-SKILL.md`. Для DynamicGraph provenance учителя надо брать оттуда, не из `all-skills`.

---

## 4. Для кода — классификация уровня (ILearningMachine vs детерминистический хелпер)

Эти файлы — **скилы/процесс-документы, не production-код.** Классифицируется не «скил», а то, что он описывает, по правилам HOW-TO-USE-SKILLS (триггеры строк 327–347) и паттерну CLAUDE.md «Allowed trainable architecture pattern gate».

- **DPO-пара (`dpo-training-data`, `dpo-triples-to-experiment-pairs`, `planning-dpo-authoring`):** это **данные/артефакт обучения**, не уровень иерархии. Пример кода `ExperimentResult.Run()` с `MeasureBaseline/MeasureEnhanced` — детерминистический харнесс измерения (eval-only). По правилу CLAUDE.md «scorer/selector… не являются паттерном архитектуры» — генератор пар сам по себе не уровень; он должен быть деталью внутри разрешённого уровня (например, тренирующего `ILearningMachine` или его runner), а не финальной обучаемой сущностью.
- **`static-to-graph-refactoring` (порог `BugDomainVoteThreshold`):** прямой пример **детерминистического хелпера** — константа-порог + event-лог. Скил честно классифицирует его four-tier как TIER 2 CONFIGURABLE и требует сохранить статический fallback. По правилу R4 (CLAUDE.md «NO HIDDEN DETERMINISM») такой порог НЕ является обучаемым уровнем; чтобы стать частью ILearningDynamicGraph как trainable-ребро, он должен быть обёрнут в `ILogicalUnit`/`ILearningMachine` с типизированным trained-state, чекпойнтом, fresh-load/continue-training и ablation — чего скил не описывает (он лишь готовит данные «на будущее»).
- **`graph-rag-integration` (IVectorIndex/IGraphEngine/IHybridRetriever/BM25):** описывает **fabric-интерфейсы поиска/графа** — это инфраструктура (детерминистический retrieval), не trainable-уровень. `IHybridRetriever = BM25 + VectorSearch` — детерминистический хелпер; по R4 не может быть «trainable grounding/router». Допустим только как helper/eval внутри разрешённого уровня.
- **`flow-design-cycle` / `learning-loop-closure` / `learning-signal-capture` / `improvement-measurement`:** **процесс/гейты**, не код-уровни. Не относятся ни к ILearningMachine/Orchestrator/Flow/Unit/Pair, ни к runtime-хелперу — это методология эксперимента и измерения.

Вывод по уровню: в этом корпусе **нет** описания кода, которое само по себе квалифицируется как `ILearningMachine`/`ILogicalOrchestrator`/`ILogicalFlow`/`ILogicalUnit`/`ILearningMachinePair`. Всё кодоподобное здесь — либо eval-харнесс измерения, либо детерминистический хелпер (порог/BM25/индекс), либо формат данных DPO. Иерархию уровней для ILearningDynamicGraph надо брать из `llm_mvp_core` (CLAUDE.md «ILearningMachine\<TDomain\> pattern» + `00.Core/LearningMachineContracts.cs`), а из этого корпуса — только формат DPO-пар, 6 сигналов и гейты измерения/петли.

---

## 5. Пробелы / что доисследовать

- **Полный список `all-skills` не считан** (Glob обрезан). Не подтверждено отсутствие отдельного скила про checkpoint/export/import/locator/continue-training или про trainable-graph edges. Доисследовать Glob по `*checkpoint*`, `*export*`, `*locator*`, `*provenance*`, `*teacher*`, `*node*`, `*edge*`, `*dynamic*`, `*reward*`, `*margin*`.
- **`experiment-data-integrity-SKILL.md` не прочитан** — на него ссылаются все 3 DPO-скила (контаминация baseline). Важен для правил «испорченной пары» при авто-построении DPO. Прочитать.
- **`capability-measurement-SKILL.md`, `coverage-completeness-gate`, `verification-before-completion`** не читались под этот вопрос — относятся к измерению/доказательству завершения, могут содержать численные гейты для «граф обучился». Доисследовать при написании раздела измерения плана.
- **Оркестрация:** прочитан только `flow-design-cycle` (это про цикл улучшения, не runtime). `escalation-orchestrator-SKILL.md` и flow-семейство (`flow-plan-review`, `cross-flow-dependency`, `requirement-to-flow`, `flow-lifecycle-translated`) не читались — проверить, есть ли там runtime-оркестрация узлов/рёбер, релевантная ILearningOrchestrator/DynamicGraph.
- **Teacher-provenance отсутствует в `all-skills`** — подтверждено по прочитанным файлам, но не по всему корпусу. Канонический источник полей provenance — `llm_mvp_core/docs/HOW-TO-USE-SKILLS.md` (строка 330) и `trainable-architecture-decision-SKILL.md`; их и использовать в плане, а не этот корпус.
- **Граница «universal vs project-specific».** Эти скилы — translated XIIGen, общие. План DynamicGraph должен решить, какие правила (формат DPO, 6 сигналов, гейты измерения) переносятся в `llm_mvp_core` как общие, а что остаётся примером; этот корпус сам границу не задаёт.
