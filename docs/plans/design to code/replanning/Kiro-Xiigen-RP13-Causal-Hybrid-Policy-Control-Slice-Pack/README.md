> **LIVE ALL-32 SLICE TASK BINDING — RP13 (2026-10-14):** [current executable 32-slice repair / causality](../32-SLICE-LIVE-EXECUTION-REPAIR-2026-10-14.md) + [minimum-check policy](../LEARNING-FIRST-MINIMAL-VERIFICATION-POLICY-2026-10-14.md) bind ALL original tasks in this pack and preserve full prompts/skills/examples. Unique owner Choose train/no-train among k/Flow/Graph/Student policy owners by trace and counterfactual, no duplicate trainer. Measure: Improved correct path on heldout at same PC cost. Changed deterministic safety only: policy recipient identity, consent/replay and rollback. Priority P2; result NOT_MEASURED; neither Windows coverage nor another DPO/Graph engine is a prerequisite.

# XIIGEN RP13 — Causal Hybrid Policy & Credit Assignment (CHPCA)

**13 октября 2026 · Новый RP13 implementation slice.** `DOCUMENTARY EXECUTION PACK / NOT_IMPLEMENTED / NOT_TRAINED / NOT_MEASURED / NOT_INDEPENDENTLY_REVIEWED`. Он НЕ является исследовательским R13, историческим S13 либо переименованием RP08/RP09. Планирование в P=`lubochka/xiigen-planning@llm_plan3`; отдельная разрешённая реализация позднее в K=`lubochka/llm_mvp_core@codex/dpo-implementation` или доказанном consumer-owner.

## Результат для человека

На законном запросе XIIGEN должен выбрать *которую из допустимых комбинаций* детерминированных ограничений, предметного представления, domain-adaptive kNN, типизированных `ILearningUnit` many→many, обучаемых `ILearningFlow`, асинхронного `ILearningOrchestrator`, higher-order `ILearningDynamicGraph` и Student применить; затем обнаружить **какое именно звено ухудшило решение**, направить разрешённый feedback в правильное обучаемое состояние (или не обновлять ничего), и в следующий раз найти **более полезный допустимый путь за меньшую полную цену на обычном ПК**. Не путать коррекцию маршрута и DPO Student: это разные state/recipient/objective.

**Первый вертикальный proof:** один реальный ordinary C#-documentation или EN→RU запрос → документированный список альтернатив и hard-law veto → trace domain/vector/neighbor/Unit/Flow/Orchestrator/Graph/Student → локализация ошибки → один целевой условно-разрешённый treatment, либо `NO_EFFECT` → независимая панель P/C0/T и cold ordinary consumer. Результат `INCONCLUSIVE`/`REGRESS` корректен; график или README никогда не доказывает обучение.

## Почему RP13 отличается от других

- `RP01` владеет первым exact Chinese-both-sides DPO Student update и loss; `RP02` domain/token/vector/kNN; `RP03` many→many Unit/Flow; `RP04` async Orchestrator/DynamicGraph; `RP05` feedback + full-PC persistence; `RP07` Builder policy; `RP08` cross-domain hybrid *integration*; `RP09` независимая *экзаменационная лаборатория*; `RP06` DTC/IDE host. **RP13 владеет только причинной разметкой влияния звеньев и политикой выбора *где обучать/когда остановиться* на обычном запросе**.
- Не создавать второй trainer, tokenizer, vector DB, kNN engine, graph scheduler, evaluator, standalone UI или source-of-truth. Сначала доказать отсутствующий seam; если существующий владелец уже работает — `REUSE_NO_CHANGE`.
- `R01–R25` остаются отдельной распределённой программой 10+5+5+5 с параллельными исследовательскими стартами, а не 25 prerequisites RP13. `results/Rxx/*` не обновляются этим пакетом. Предыдущее моделирование/чужой статус не является собственным запуском.

## Состав и запуск

Начать с [INSTALL-AND-START](INSTALL-AND-START.md) и [START-PROMPT](START-PROMPT.md); затем [SOURCE-AND-OWNERSHIP](SOURCE-AND-OWNERSHIP.md), [ARCHITECTURE-AND-CREDIT](ARCHITECTURE-AND-CREDIT.md), [DOMAIN-LAW-AND-CURRICULUM](DOMAIN-LAW-AND-CURRICULUM.md), [EXPERIMENT-AND-PC](EXPERIMENT-AND-PC.md), [CONTRACT-AND-RECEIPT-SCHEMAS](CONTRACT-AND-RECEIPT-SCHEMAS.md), [EXECUTION-BOARD](EXECUTION-BOARD.md), [GOVERNANCE-AND-REVIEW](GOVERNANCE-AND-REVIEW.md); выполнять **одну полную карточку** из [tasks](tasks/). [PACKAGE-MANIFEST](PACKAGE-MANIFEST.json) даёт инвентарь 16 задач, **не** certification.

**Не требуется waterfall.** `00A/00B/00C` читают независимые части; `01A/01B/01C` проектируют совместимый контракт; `02A` минимальный tracer по реальному consumer и `04A` frozen baseline имеют ранний путь без Student/teacher. `02B/03B` требуют фактических state owner и effect grant. `05B` review только отдельным Astra вызовом с точным SHA.

**Результат публикации этого каталога — только model-ready planning.** Ни один Chinese pair, CODEX judgement, Student/route update, time/accuracy measurement, Windows workflow, independent Astra PASS или PC deployment **этой публикацией не выполняется**.
