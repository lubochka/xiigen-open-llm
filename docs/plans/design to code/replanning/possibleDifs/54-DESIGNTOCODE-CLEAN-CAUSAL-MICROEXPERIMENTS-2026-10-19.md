# GDC-A0–A7: малые причинные эксперименты — действительно ли возвращается полезное обучение?

<!-- CODEX-POST128-PLANNING:START -->
## Действующее уточнение после P128 — Рабочий oracle и внешний итог

Проверенный core: `f8784df037b59cf878889eef4d0956f5d0c985e7`; planning: `5df24354aa803f7ed5dd3bac48146466e2feb189`. Это адресное усиление приёмки соответствующего класса, а не новая аттестация всех исторических опытов карточки.

P123 geometry 2/4→4/4 сохраняется; geometry evidence в продукте не создаёт обученный critic. Честный B:e1 проходит; подмена RawOverlay/feature при прежних ID/gold/counts обнаруживается независимой связью raw→features/gold, nonfinite — рабочим GoldOf, а не только вспомогательным AllFinite. P126/P128 показывают дополнительную границу: обязательный false в JSON должен дойти до процессного и внешнего exit. Собственный output finalizer не является его обязательным входом. Обе попытки P123 вместе: 4 Train, 16 предъявлений, 32 чтения и 4 scaffold calls; ремонт свода не добавляет обучение.

**Передать исполнителю вместе с собственным паспортом:** проследить названный actual consumer, сначала принять полный honest пакет, затем отвергнуть одну адресную порчу тем же checker; результат включить в общий verdict и внешний exit. Указать источник/сборку, фактический расход каждой попытки и предел утверждения. Ремонт expected опирается на независимый договор, не на actual сломанного кода. Сохранённый PRIMARY не повторять; gold, исходные бюджеты и прошлые результаты не менять. Активная попытка сохраняет ранее выданный ей pin карточки. Native source/static counterexample и исполненный тест обозначать отдельно.

[Сводка нового аудита](codex-2026-11-03-POST128-TRANSFER-AND-EXPERIMENT-AUDIT.md) · [действующая core-карточка](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/cards/P123-design-conversion-critic-evidence.md#current-executor-task). Историческое содержание ниже сохранено.
<!-- CODEX-POST128-PLANNING:END -->

**19 октября 2026 · DESIGN ONLY.** Ни один нижеописанный native C# / Student learning / browser QA запуск **не осуществлён** в этом документировании. Выполненная source-equivalent микро-проба в отдельных [Python](54-DESIGNTOCODE-CLEAN-PROXY.py) и [JSON](54-DESIGNTOCODE-CLEAN-PROXY-RESULT.json) доказывает лишь условную логику C# веток, не качество нейронного обучения.

**Цель:** восстановить полезный **обычный ответ после обучения**, а не удовлетворить число тестов. Метод из `experiment-design-SKILL.md`: `ExperimentId / Baseline / Enhanced / Metric / Tier`; один фактор на пробу, отрицательный контроль, одинаковый знаменатель, недоступный при тренировке heldout. Реальная DPO-пара допускается только после существующей проверки легальности, независимой подготовки chosen/rejected, осмысленности и source-disjoint. Не перемешивать внешние корпусы с внутренними. Не ослаблять frozen RP01 benchmarks.

## GDC-A0 — P0: восстановить честный сигнал качества после изменения DPO projection

- **Тема/профиль:** Design-to-Code, 4 HTML/CSS случая: успешное визуальное исправление, провал, инвертированная ошибка preference, одинаковые стороны. **Получатель:** `DesignConversionMachine` local learned feature weights; нейронная policy только отдельным контролем.
- **Исходная пара:** фиксированные server-admitted `Chosen.Html/Css` и `Rejected.Html/Css` + *отдельная* подтверждённая оценка reviewer для каждой стороны. Закрепить `UnitId`, `RecordId`, source_hash, model reference, checkpoint.
- **Baseline A:** фактический [current `ToCanonicalTrainingItem`](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionTrainingPipeline.cs#L2712-L2727) → approved Machine; trace `preferenceMargin`.
- **Enhanced B:** тот же HTML/CSS input, с теми же frozen policy tokens, но `Machine` получает из authority-bound **typed sidecar** `chosenScore/rejectedScore` (не включать ни в текст source prompt, ни в target HTML/CSS); передать только через допущенный train route.
- **C:** swap trusted scores при неизменных HTML/CSS → знак локального margin должен поменяться; **D:** equal score / equal response и неправомочный источник → не объявлять meaningful gradient.
- **Метрики:** sign и magnitude `trusted pair margin`; число и ключи реально обновлённых `LearnedWeightCheckpoint`; exact cold-restore hash; после обычного `CreateRuntimeRoutingFlow` blind defect-resolution score + inappropriate changes count; wall/CPU/RAM. Preference metric Student отдельно.
- **Успех:** B различает реальные `+/-` сигналы на том же корпусе, A fallback `1.0`, и **после cold restore** хотя бы в одном pre-registered heldout обнаружена полезная содержательная разница без ухудшения остальных; иначе результат `mechanism_fixed_quality_not_proven`.
- **Фальсификация:** для фактического caller уже существует другая надёжная typed оценка, не затронутая текущим builder; тогда найденный source mismatch не причинен данному caller.
- **Объём:** Tier1: 2 training source families + 4 independent holdout; затем Tier2 12+ источников только если сигнал полезный. **Не запускать Windows full-suite** вместо одного native producer/consumer.
- **Адресный след:** gold [Pipeline:L1487–1520](https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/44.DesignConversionDomain/DesignConversionTrainingPipeline.cs#L1487-L1520), current [Pipeline:L2712–2727](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionTrainingPipeline.cs#L2712-L2727), [Machine:L2944–3003](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L2944-L3003).

## GDC-A1 — P0: восстановить `correction_category` до фактического обученного Improver

- **Тема:** Design-to-Code, выбор визуального исправления, а не генерирование текста по одной константе.
- **Получатель:** learned weights именно Unit `design_conversion_runtime_scaffold_improver`, затем вызов [LearnedWeightImproverUnit](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L4232-L4254).
- **Baseline A:** current canonical response `{html,css}` — на этом пути [парсер correction categories](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionMachine.cs#L3008-L3091) увидит ноль `correction_category` полей.
- **Enhanced B:** безопасный server-owned training sidecar отдельно маркирует семантическую категорию **именно chosen correction** и rejected correction по approved visual QA; модель HTML/CSS по-прежнему видит только source и нужные outputs. Два типа defect (contrast, overflow).
- **C (wrong-label):** intentionally invert human-selected correction IDs while keeping source/HTML fixed — learned policy должна либо отказать, либо изменить выбор, но не сохранять автоматический PASS. **D (consumer fallback):** no learned categories.
- **Измерить:** доступные `correction_category:contrast` и `:overflow` в свежем checkpoint, их observation count/weight и signed preference, actual chosen rule ID в обычном API, полноту исправлений и unintended CSS; источник изменений lexical vs neural vs hard-coded.
- **Stop:** если нельзя доказать законный typed teacher label, не подменять его regex из HTML. **Tier1:** 4 пары + 4 unseen layouts.

## GDC-A2 — P0: немонотонность при нескольких одновременных дефектах

- **Тема:** Design-to-Code, HTML/CSS **обязательно** и контраст, и переполнение на одном layout; контроль single-defect. **Получатель:** runtime `ImproverUnit`; источник веса — authorised checkpoint, не ручная подмена production состояния.
- **A:** checkpoint без learned correction preference. **B:** один положительный `contrast`. **C:** положительные `contrast` и `overflow`. **D:** нулевые/отрицательные веса. **E:** только один найденный дефект.
- **Прогноз source-only:** A = два selected rules; B = только один; C = только один; D = два; E = один. [Код](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionTrainedUnits.cs#L298-L353).
- **Метрика:** `required_defects_fixed / initially_identified_defects`, `new_defects_introduced`, diff HTML/CSS, measured visual/browser QA, p95 unit latency, **отдельно** `trained-state load succeeded`. Без внешнего AI можно локально верифицировать наличие обоих свойств CSS и DOM overflow, но это лишь *частичный surrogate*, не visual quality.
- **Decision:** если learned B/C теряет исправление при одинаковом input — подтвердить regressibility алгоритма. Только после A/B менять policy на сохраняющую независимые safety/critical corrections; **не** просто применять все правила при конфликтующих CSS.

## GDC-A3 — P1: размерность source vector и обучение различимых задач

- **Тема:** Design-to-Code: требования layout, где релевантный параметр за первыми 16 координатами.
- **Baseline:** current [`internal_vector.Take(16)`](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionDpoDatasetBuilder.cs#L1231-L1260); **B:** authoritatively labelled, length-aware selection of **feature-named relevant dimensions**, без IDs/outcome leakage; **C:** rerandomized tail / inert-tail control.
- **Input:** две approved 18D vectors с совпадающими первыми 16, разной 17-й и разным действительно правильным output, совпадающей *другой* безопасной структурой. Для heldout взять новые layout families, не дубликаты.
- **Метрики:** уникальность **полнейшего** prompt/feature fingerprint; changed learned weight, source-disjoint accuracy и resource budget. Недопустимо заранее приписывать +N процентов.
- **Stop:** если vector dimension 17 не используется настоящим feature encoder или повторён в другом поле — source-only alias **неактивен**, зафиксировать NOT_APPLICABLE.

## GDC-A4 — P1: безопасные отношения в AST/DOM после sanitizer

- **Тема:** Design-to-Code DOM parent-child, иногда C# AST parent-role; **не перевод естественного языка без релевантности признака**.
- **A:** current [`SanitizeSemanticSourceStructurePayload`](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/44.DesignConversionDomain/DesignConversionDpoDatasetBuilder.cs#L1308-L1380) удаляет `parentRef` полностью. **B:** *privacy-safe derived* `same_parent` / `parent_role` / relative sibling order, computed by source encoder without exposing raw IDs, only if approved by legal/safety review. **C:** swapped parent relation negative control.
- **Input:** 4 дизайна, где верная вложенность зависит от parent relationship, но роли/размеры одинаковы.
- **Метрика:** distinct legal vectorization; actual nested DOM correctness on 8 heldout independent trees; provenance admission outcome and privacy scan. Bad input JSON must be rejected/diagnosed, not reclassified as high-quality identical `{}`.
- **Stop:** если полная source encoder feature map уже сохраняет relation in another field, отметить N54-02 как неактивную для данного потребителя.

## GDC-A5 — P0 audit: старый публичный train и новый legally admitted train

- **Тема:** Design-to-Code deployment of learning, receiver selection, lifecycle, generalizable to other Unit only by explicit caller trace.
- **A:** старый caller вызывает `Machine.TrainBatchAsync`; current code закономерно возвращает legal authority required. **B:** тот же разрешённый источник через `RunAsync(ServerOwnedLegalTrainingTeacherAdmissionAuthority,...)` или `RunValidatedAsync` с точным sealed binding — не обход guard.
- **Метрики:** received vs rejected counts by `UnitId`, error propagation, training state/checkpoint version and hash (до/после), trained service consumer acquisition after restart. **Никакое** `RunStarted=true` не засчитывается как learned effect.
- **Фальсификация:** все production callers уже используют admitted path и failures surfaced — онда это нормально обеспеченная защита, НЕ дефект.

## GDC-A6 — восстановление возможностей из соседних успешных веток (НЕ общий эксперимент)

Эти доменные пробы существуют отдельно и должны использовать исходные собственные gold receipts / methods. Их нельзя объединять в одну «среднюю точность».

| Subject / task | Positive donor / prior preparation | A/B и именно что измерить |
|---|---|---|
| **Перевод:** EN→RU, RU→EN (два профиля, **одна** предметная тема) | [language_translate prior](00-LANGUAGE-TRANSLATE-RECOVERY-FIRST-2026-10-19.md); E25 POS-bigrams обучаются на примерах; E26 контекстный словарь предзаполнен | exact/lexical-only vs trained POS-order kNN vs contextual kNN; 8 неоднозначных слов + 8 отрицаний, из независимых предложений; genuine train state, cold restore and ordinary translation, failures by POS/order |
| **Языки программирования:** C# AST/SQL (подпрофили предмета) | [E09 & matrix→KNN](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md) / [external_domains P0](00-EXTERNAL-DOMAINS-SUCCESS-RESTORATION-P0-2026-10-19.md) | baseline AST signature vs kNN with parent+ordinal, same validation files/denominator; count correct structural next step and latency, no data leakage by file family |
| **Оркестрация:** dependency sequencing | [orchestrator-extensions P0](00-ORCHESTRATOR-EXTENSIONS-GOLD-RECOVERY-FIRST-2026-10-19.md) | gold trained v1 checkpoint vs current empty after failed v1 load vs provenance-rebuilt v2; 6–12 new dependency queries; cold API response and missing/extra dependencies |
| **Продолжение обучения:** удержание старых тем | [E43 archived](00-EXTERNAL-DOMAINS-SUCCESS-RESTORATION-P0-2026-10-19.md) | same *recipient* and same new lesson batch ± replay, old/new heldout, restore and ordinary calls; не переносить bigram E43 retention число на neural q/v |

## GDC-A7 — RP01 Student sanity отдельно от golden DesignConversion

**Тема:** C# документация и перевод (отдельные панели); **получатель:** настоящий C#/EN→RU `B0 + q/v-LoRA` Student.

- A: исходный frozen random B0 → DPO-only; B: legal supervised chosen-only warmup → exactly same DPO after warmup, matched compute/conditions; C: голый B0/zero-step negative; D: independent learned Unit/kNN path, без смешивания в training Student.
- **Разнести измерения**: (1) objective preference, (2) factual free-answer accuracy, (3) `chosen` law/negation preserved, (4) hallucination/abstain, (5) old-domain retention, (6) tokens/time/RAM.
- **Гейт**: если baseline random model не способен связно декодировать, DPO-only на 500 мини-парах **не объявлять генеративным learner regression от DesignToCode-clean**: сравнивались несопоставимые модели.
- Frozen RP01 evidence [16 Oct results](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md): C# 0/3→0/3; ENRU 0/4→0/4; не переоценивать результаты после правки evaluator.

## Единая карточка отчёта, которую должен вернуть исполнитель

```yaml
experiment_id: GDC-A0
topic: design-to-code
task_profile: html-css-contrast-and-overflow
recipient: "DesignConversionMachine:unit_id (exact)"
donor_branch_sha: e346b88a3cc1fcce48aefa626bfc7b00509eefd6
current_branch_sha: "<actual checked execution SHA>"
dataset_origin_approval: "<source proof, no raw private data in report>"
corpus_sha256: "<digest>"
heldout_group_sha256: "<digest>"
changed_factor: "<exactly one>"
baseline_and_enhanced: ["A/current", "B/authorized typed signal"]
negative_controls: ["swapped", "equal", "untrained", "blocked-source"]
train_record_count: 0
heldout_count: 0
trained_state_changed: false
cold_restore_works: false
ordinary_consumer_called: false
score_preference_delta: null
visual_quality_after_vs_before: null
unseen_success_numerator_denominator: null
latency_ms: null
ram_mb: null
evidence_files: []
result: "NOT_RUN" # SOURCE_CONFIRMED / MECHANISM_CONFIRMED / CAUSAL_GAIN / FAILED / NOT_APPLICABLE
falsifier_checked: false
independent_reviewer: null
```

**Два независимых этапа контроля:** Architect/Code Examiner проверяет `caller -> unit -> state -> consumer` до запуска; независимый QA/Planning Reviewer после запуска сравнивает frozen case ids, source-disjoint и фактически прочитанный checkpoint. Образцовый GOOD: `two trained effects survive cold restore, 6/8 unseen correct vs 3/8 baseline` с отдельным signed receipt; BAD: `PASSED 100 tests; score вырос` без heldout. Пример GOOD — только шаблон, **не результат XIIGEN**.

**Кому поручить**: исполнитель CODEX — адресные source edits и native harness; независимый reviewer — модель данных, source identity, transcript обычного API. Запрет: защищённый `llm_mvp_core/docs/plans/replanning-possible-diffs/**` не менять; self-contained local C# harness/temporary sandbox без Windows full GitHub CI и без изменения frozen RP01 facts. Результаты опыта возвращать в `xiigen-planning/.../possibleDifs` с чётким `NOT_RUN/PASS/FAIL`.

**Скиллы при исполнении:** `architect-behavior-classifier-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `code-examination-SKILL.md`, `experiment-design-SKILL.md`, `claim-verification-SKILL.md`, `human-communication-GLOSSARY.md`, `prompt-patch-authoring-SKILL.md`. Читателю объяснять предмет, задачу, получатель обучения, механизм и измерение раздельно.
