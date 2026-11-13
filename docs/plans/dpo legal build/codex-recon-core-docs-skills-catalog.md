# РЕКОН: каталог скилов (core/docs) для ILearningDynamicGraph

Сбор evidence для плана авто-построения DPO для
ILearningUnit / ILearningFlow / ILearningOrchestrator / ILearningDynamicGraph.
Только реально найденные пути. Это не разрешение на реализацию.

---

## 1. Что это и где (точные ключевые пути)

Корень каталога скилов: `C:/Projects/llm_mvp_core/docs/skills/` (около 200 файлов).
Точка входа / навигатор: `C:/Projects/llm_mvp_core/docs/HOW-TO-USE-SKILLS.md`
(v27.5; 782 строки). Содержит: порядок загрузки, таблицу «тип сессии -> скилы»,
таблицу «триггер активации -> скилы», и полный «Skill index» (строки ~538-633)
с одной строкой назначения и версией на каждый файл.

Релевантные под вопрос файлы (прочитаны целиком, кроме отмеченных как
index-only):

- `docs/skills/dpo-triple-format-SKILL.md` — формат DPO-тройки, источники полей,
  минимум троек на класс, gate промоушена, секция «Trainable Architecture and
  Set Quarantine».
- `docs/skills/generated-code-review-SKILL.md` — 5-слойное ревью + Layer 5/5A
  + Deterministic-Boundary Gate (классификация уровней).
- `docs/skills/implementation-integrity-SKILL.md` — 13 Check-ов (анти-стаб,
  реальность кода, граничные проверки уровней, sub-agent boundary).
- `docs/skills/phase-completion-packager-SKILL.md` — упаковка фазы, trainable-phase
  gate (анти-стаб), two-PASS gate, review/fix floor (4 и 10 циклов).
- `docs/skills/trainable-architecture-decision-SKILL.md` — главный классификатор
  допустимых уровней; Graph-Style/DynamicGraph Reconstruction Gate; Legal
  Teacher/Provenance Gate; матрица Dictionary->ILogicalUnit; вердикты.
- `docs/skills/capability-measurement-SKILL.md` — 6 классов capability, шкала
  0-100%, evidence через runtime-команды.

Index-only (назначение взято из «Skill index» в HOW-TO-USE-SKILLS.md, файлы не
открывались полностью):
- `docs/skills/improvement-measurement-SKILL.md`, `capability-measurement` уже выше.
- `docs/skills/learning-signal-capture-SKILL.md`, `learning-path-audit-SKILL.md`,
  `learning-loop-closure-SKILL.md` — обучение/learning-signal/DPO-валидность/
  curriculum tier.
- `docs/skills/planning-dpo-authoring-SKILL.md` — DPO-тройки для планирования.
- `docs/skills/verification-before-completion-SKILL.md` — Runtime Evidence
  Firewall (отвергает validation_only/static/contract/UNKNOWN).
- `docs/skills/skill-injection-SKILL.md` — блоки SK-VERIFY/SK-QA/SK-ARCH/SK-REVIEW.

---

## 2. Как устроено / работает (кратко)

Каталог по вопросу делится на шесть кластеров.

DPO / формат данных:
- `dpo-triple-format-SKILL.md`: DPO-тройка = 6 обязательных групп полей (Identity;
  Context с обязательным `PromptSystem`; Rejected; Chosen; TeachingPoint;
  CurriculumTier 1-5). Тройка без context-package = не обучающая запись.
  Триггер захвата: предыдущий ран < 0.85 и текущий >= 0.85. Минимум троек на класс
  capability; gate `DpoPromotionGate.VerifyMinimumTriples` перед ACTIVE.
  Секция «Set Quarantine»: отвергать строки, где разница chosen/rejected = id
  источника, pair id, имя файла/путь, порядок кандидатов, точная память промпта,
  keyword->skill lookup. Вердикты: KEEP_FOR_TRAINING / _BASELINE_ONLY /
  _EVAL_ONLY / REWRITE_TO_DPO / QUARANTINE_DO_NOT_TRAIN /
  WRONG_DOMAIN_REMOVE_FROM_THIS_MACHINE.
- `planning-dpo-authoring-SKILL.md` (index): 3 типа сигналов планирования
  (ROUTING_DECISION / CALIBRATION_COMPARISON / ESCALATION_DECISION),
  GENERATED vs REINFORCEMENT.
- `learning-signal-capture-SKILL.md` (index): 9 типов сигналов, DPO validity gate
  (3 cross-model условия), таблица curriculum tier (ROUTING=1..SCHEDULED=5).
- `learning-path-audit-SKILL.md` (index): аудит, реально ли эмитятся DPO_TRIPLE/
  OUTCOME/ARBITER_VERDICT/MODEL_COMPARISON/DESIGN_REASONING (LEARNING /
  GENERATING-NOT-LEARNING / STRUCTURAL-FAILURE).
- `learning-loop-closure-SKILL.md` (index): цикл из 4 событий decide->observe->
  update->promote; диагностика «сломанного» события.

Обучение / измерение качества:
- `capability-measurement-SKILL.md`: 6 классов (C1 Core Algorithm .. C6 UI),
  шкала 0/25/50/75/100%, каждое >=75% требует runtime-команду
  («Do not report a score without the runtime command that proves it»).
- `improvement-measurement-SKILL.md` (index): требует behavioral delta
  (input->output observable), не только рост числа тестов.

Generated-code-review:
- `generated-code-review-SKILL.md`: 5 слоёв — Layer1 семантика, Layer2 DNA-грепы,
  Layer3 silent-failure (MACHINE constant test), Layer4 training-data impact,
  Layer5 Trainable vs Stub (числовые метрики обязательны для trainable). Layer 5A
  — классификация по допустимым уровням. Deterministic-Boundary Code Review Gate —
  обязательный поиск спрятанного детерминизма; поля
  `deterministic_boundary_review`, `allowed_level`, `claim_matches_mechanism`,
  `llm_mvp_core_core_logic_boundary_checked`, `host_adapters_thin_checked`.
  Multi-cycle floor (4 цикла) и Bounded sub-agent evidence boundary.

Implementation-integrity:
- `implementation-integrity-SKILL.md`: «Fix the instance AND add the structural
  guard». 13 проверок. Check 5 — Implementation Reality / Anti-Stub (реальный
  путь кода entrypoint->service/machine->unit->state; не JSON/report; не стаб;
  числовые held-out метрики). Check 9 — Allowed Trainable Architecture Pattern
  (runtime authority обязан быть внутри допустимого уровня). Check 10 — Bounded
  Sub-Agent Scope Integrity. Check 11 — Full Extension Definition Gate.
  Check 13 — Execution/Code Review Completeness.

Phase-completion:
- `phase-completion-packager-SKILL.md`: 3 артефакта (STATE-Pn.json +
  PHASE-COMPLETE-Pn.md + handoff). Trainable-phase gate (анти-стаб) перед
  status=DONE: generated-code-review Layers1-5 PASS + Runtime Evidence Firewall +
  числовые метрики. Two-PASS gate: PASS_AS_HONEST_UNTRAINED_SCAFFOLD vs
  PASS_AS_TRAINED_FEATURE (смешение = масскарад, -1000). Review/fix floor: дефолт
  2, при «минимум 4» = 4, для extension/governance-repaired = 10. Continuous Plan
  Exception (пакет = checkpoint, не stop). Target Outcome Completion Packaging
  Rule.
- `verification-before-completion-SKILL.md` (index): Runtime Evidence Firewall.

Triggers (из HOW-TO-USE-SKILLS.md): любой graph-style/DynamicGraph trainable план
грузит `trainable-architecture-decision-SKILL.md` +
`human-readable-architecture-audit-SKILL.md` + `codex-planning-quality-GUIDE.md`
+ `plan-review-SKILL.md`; любой DPO/distillation teacher грузит provenance-поля.

---

## 3. Релевантность к ILearningDynamicGraph

Прямая и высокая. Авто-построение DPO для уровней Unit/Flow/Orchestrator/Graph
управляется именно этими файлами:

- DPO/граф/оркестрация: `dpo-triple-format-SKILL.md` даёт жёсткий формат строк,
  которые автогенератор должен производить (включая обязательный `PromptSystem`,
  TeachingPoint, CurriculumTier) и список запрещённых shortcut-сигналов —
  критично, чтобы авто-DPO не учил утечки (порядок кандидатов, id источника).
- Граф/оркестрация (классификация уровней): `trainable-architecture-decision-
  SKILL.md` содержит спец-гейт «Graph-Style / DynamicGraph Reconstruction Gate»
  (nodes/edges, input/output contracts, many_to_many, fan_out/fan_in,
  async_graph_execution, deterministic_vs_trainable_authority_on_edges, скрытый
  детерминизм на каждом ребре). Это прямой чек-лист для ILearningDynamicGraph.
- Измерение качества: `capability-measurement-SKILL.md` +
  `improvement-measurement-SKILL.md` — как мерить, что обучение реально подняло
  поведение, а не число тестов; `learning-path-audit-SKILL.md` — реально ли
  эмитятся обучающие сигналы.
- Teacher-provenance: «Legal Teacher / Provenance Gate» в
  `trainable-architecture-decision-SKILL.md` + триггер в HOW-TO-USE-SKILLS.md:
  поля `teacher_access_mode/hosting_provider/model_id/model_license/
  provider_terms_checked/teacher_output_used_for_training/closed_proprietary_model`.
  Claude/OpenAI/Gemini как distillation teacher запрещены; closed proprietary +
  used_for_training => всегда `DATASET_REBUILD_REQUIRED`. Это прямо ограничивает,
  откуда авто-DPO может брать chosen/rejected.
- Анти-стаб приёмка: `generated-code-review-SKILL.md` Layer5 +
  `implementation-integrity-SKILL.md` Check5 + `phase-completion-packager-SKILL.md`
  trainable-phase gate — не дают принять «обученный граф», если это json/стаб без
  числовых метрик и смены решения после обучения.

---

## 4. Для кода — классификация уровня vs детерминистический хелпер

Правило источника: `trainable-architecture-decision-SKILL.md` (строки ~95-117,
Decision Procedure ~120-162) и зеркально `generated-code-review-SKILL.md`
Layer 5A (~158-220) и `implementation-integrity-SKILL.md` Check 9 (~294-321).

Допустимые trainable-уровни (ровно пять):
- `ILearningMachine<TDomain>` (Level 5) — Train/Evaluate/Export/Import/Orchestrator.
- `ILogicalOrchestrator` (Level 4).
- `ILogicalFlow<TInput>` (Level 3).
- `ILogicalUnit<TInput,TOutput>` (Level 2).
- `ILearningMachinePair<TPersonal,TGlobal>` (Level 1) — common + user-adaptive.

Внутренний детерминистический хелпер (НЕ архитектурный уровень; допустим только
как internal detail/baseline внутри допустимого уровня, с честной меткой):
- scorer, selector, classifier, router, retriever, detector;
- KNN / Jaccard / BM25 store, dictionary / counter state;
- regex / keyword / pattern matching;
- private-методы `TryDecide*`, `ChooseBestPattern`, `SelectBestPattern`,
  `CountMatchingSignals`.
Правило (цитата смысла, trainable-architecture-decision-SKILL.md): «Everything
else is an implementation detail unless it is wrapped by one of those levels and
has the required lifecycle evidence». KNN не запрещён сам по себе
(`OK_MODEL_COMPONENT`) — запрещено выдавать standalone KNN/retrieval за всю
trainable-архитектуру или за немаркированную финальную authority.

Вердикты классификатора (trainable-architecture-decision-SKILL.md ~194-209):
`OK_ALLOWED_LEVEL`, `OK_MODEL_COMPONENT`, `OK_INTERNAL_DETAIL`,
`BASELINE_MUST_BE_LABELLED`, `NEEDS_ILOGICALUNIT_WRAPPER`,
`NEEDS_ILOGICALFLOW_OR_ORCHESTRATOR_WRAPPER`, `FORBIDDEN_RUNTIME_SHORTCUT`,
`REPORT_CLAIM_TOO_STRONG`, `DATASET_REBUILD_REQUIRED`.

Для ILearningDynamicGraph: на каждом ребре графа нужно назвать владельца финальной
authority (детерминистический хелпер vs trainable unit/flow/orchestrator/machine/
pair). Скрытый детерминизм на ребре = `FORBIDDEN_RUNTIME_SHORTCUT`, если не помечен
явно как eval-only/helper/baseline с ablation/no-shortcut тестами
(Graph-Style Reconstruction Gate).

Замена словаря/счётчика: матрица Dictionary->ILogicalUnit
(trainable-architecture-decision-SKILL.md ~242-384) — одна заполненная строка на
поле/метод (typed state, training update algorithm, KNN role, DPO
positive/rejected/quarantine с конкретными значениями, lifecycle
load->fallback->backup/export/import/share->continue-training). Schema-only строка
«не является решением».

---

## 5. Пробелы / что доисследовать

- HOW-TO-USE-SKILLS.md прочитан строки 1-633 из 782; хвост (~634-782, прочие
  активационные триггеры и разделы) не прочитан — отметить как доисследовать.
- Не открыты целиком (только index-строки): `learning-signal-capture-SKILL.md`,
  `learning-path-audit-SKILL.md`, `learning-loop-closure-SKILL.md`,
  `planning-dpo-authoring-SKILL.md`, `improvement-measurement-SKILL.md`,
  `verification-before-completion-SKILL.md`, `skill-injection-SKILL.md`. Для
  плана авто-DPO их стоит прочитать целиком (особенно DPO validity gate и
  curriculum tier в learning-signal-capture; блок SK-VERIFY/SK-QA в
  skill-injection).
- В каталоге нет файла со словом «ILearningDynamicGraph» в имени; «graph»-скилы:
  `graph-backed-routing-SKILL.md`, `graph-entity-schema-SKILL.md`,
  `graph-extension-protocol-SKILL.md` (есть в Glob, не открывались) — проверить,
  относятся ли они к DomainModelRegistry-графу или к runtime DynamicGraph.
- Упомянутые в триггерах `codex-planning-quality-GUIDE.md`,
  `human-readable-architecture-audit-SKILL.md`, `plan-review-SKILL.md`,
  `reference-inventory-and-source-map-SKILL.md` не входят в эту узкую под-задачу,
  но обязательны при написании самого плана графа (High-Risk Architecture Recon
  Library Gate). Они — предмет смежного рекона.
- DPO teacher provenance: формат полей задан, но конкретный реестр одобренных
  hosted open-weight источников в этом каталоге не найден — доисследовать, где он
  ведётся (вероятно вне docs/skills).
