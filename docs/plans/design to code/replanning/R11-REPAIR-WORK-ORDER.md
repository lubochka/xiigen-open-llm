# R11 REPAIR — causal first-lesson path before full delivery

**Дата ремонта:** 2026-10-13  
**Назначение:** полноценный work order для R11 после R01–R10. Этот документ заменяет краткую R11-карточку как governing prompt, но не выполняет R11 сам по себе.

## 0. Почему R11 требует отдельного repair

R11 не должен:
- повторять R01–R10;
- снова сводить весь проект к S04;
- превращать отсутствие полного Windows/IDE/release контура в blocker первого learning experiment;
- объявлять tests/coverage доказательством улучшения обучаемой системы;
- выкидывать старые delivery obligations ради «быстрого прототипа»;
- создавать новый trainer, если существующий путь можно исправить минимальным seam;
- ждать глобального DONE всех соседних исследований, если для конкретного causal claim уже есть достаточные primary/code/evidence inputs.

R11 должен разделить два разных графа:
1. **FIRST-LESSON DAG** — минимальные причинно необходимые шаги, чтобы получить допустимый новый урок, изменить разрешённое state и измерить изменение поведения на обычном consumer.
2. **FULL-DELIVERY DAG** — весь обязательный DPO/DynamicGraph + DTC + IDE + остальные domain/curriculum/release obligations.

Ключевой результат R11 — не красивый dependency diagram. Результат — доказанный **minimal cut-set** первого настоящего учебного наблюдения и сохранённый полный backlog с owner/trigger для каждой перенесённой обязанности.

---

## 1. Неподвижные архитектурные и role invariants

Эти решения нельзя переоткрывать в R11 без более нового прямого owner source.

### DPO roles

- **Human role / lesson selection = CODEX на Astra 6 UltraCode.**
  CODEX выбирает weakness/domain/task family и разрешённую учебную задачу.
- **Final DPO author = внешняя открытая китайская модель.**
  Она создаёт обе final стороны: `chosen` и `rejected`.
- **Exact pair judge = CODEX на Astra 6 UltraCode.**
  CODEX отдельно оценивает exact immutable chosen/rejected pair: ACCEPT / REJECT / TIE / BOTH_BAD / UNJUDGEABLE.
- **Student = XIIGEN.**
  Только допущенная exact pair может менять Student state.
- **Post-update evaluation = отдельный шаг.**
  Валидность пары и полезность обновления Student — разные вопросы.

Запрещено возвращать ручного человека в operational loop выбора/оценки. Запрещено выдавать ответ CODEX/Astra за training target китайского автора.

### Four trainable/runtime roles

Сначала сопоставить actual code behavior, затем имена интерфейсов:

- **ILearningUnit** — domain + input-vector structure + output schema → typed many→many results, с сохранённой association каждого результата к input.
- **ILearningFlow** — отношения между Units и correction input→output.
- **ILearningOrchestrator** — async combinations, dependency/order, fan-out/fan-in, cancellation, budget/fallback, выбор допустимого пути.
- **ILearningDynamicGraph** — higher-order relations между patterns разного порядка, reusable compositions и learned relation/path policy.
- **ILearningMachine** остаётся training owner; четыре роли не означают четыре отдельных trainer stack.

### Existing hybrid decisions to preserve until evidence says otherwise

Не терять без прямого решения:
- TensorCore/shared base/project LoRA;
- StackDetector-before-normalization;
- semantic compression;
- BM25 + kNN;
- positive/negative/warning memory;
- final arbiter;
- semantic OrderedPatternIds/order;
- one-pair/no-repeat/full-state accept-or-rollback;
- separated inference state vs mutable training state;
- correct deterministic candidate не становится negative только из-за происхождения;
- deterministic computation над learned weights не означает «нет обучения».

---

## 2. CURRENT TASK

Построить evidence-bound причинную модель, которая отвечает:

> **Какой самый короткий корректный путь позволяет XIIGEN как можно раньше получить один настоящий новый урок, применить допустимое update к существующему learning path, использовать принятое состояние обычным runtime consumer и измерить semantic improvement/retention/cost — не ожидая независимых release/UI/Windows работ и не теряя обязательства всей программы?**

После этого отдельно построить FULL-DELIVERY DAG и показать, какие обязанности действительно deferred, а не удалены.

---

## 3. NOT THE TASK

R11 не:
- реализует product code;
- запускает trainer;
- запускает Windows/GitHub Actions/coverage campaign;
- переписывает R12 architecture;
- выбирает final experiment thresholds за R13, если они ещё требуют design decision;
- повторно читает всю библиотеку без конкретного unresolved claim;
- объявляет отсутствующим то, что не было найдено после ограниченного поиска;
- превращает proposed dependency в universal prerequisite;
- объявляет S04 всей программой;
- выполняет R14 schedule migration;
- меняет core skills/tests/workflows/weights/datasets.

---

## 4. Required skills and gates

Исполнитель обязан **реально прочитать**, а не только перечислить, применимые части:

### SK-SESSION
- `planning-session-startup-SKILL.md`
- `architect-behavior-classifier-SKILL.md`
- действующие `AGENTS.md` / local replanning instructions

### SK-PLAN
- `authority-requirement-binding-SKILL.md`
- `reference-inventory-and-source-map-SKILL.md`
- `phase-planning-SKILL.md`

### SK-REVIEW
- `plan-review-SKILL.md`
- `goal-delivery-completeness-SKILL.md`
- `claim-verification-SKILL.md`
- `reconnaissance-gate-SKILL.md`
- применимые audit/specificity/output-skepticism gates

### SK-ARCH
- `trainable-architecture-decision-SKILL.md`
- `human-readable-architecture-audit-SKILL.md`
- `output-readability-gate-SKILL.md`

### SK-CODE
Когда claim касается существующего implementation path:
- применимые code-review / implementation-doc skills;
- обязательно trace source→class/method→state→ordinary consumer;
- документация/manifest/status не заменяют code path.

В `RUN-LOG.md` записать actual repo/ref/path/blob/span и `read|partial|unavailable`.

---

## 5. Mandatory context — читать адресно, но реально

### Whole-program authority
Из `llm mvp/design to code session`:
- `original task.md`
- `task prompt.md`
- `task prompt.txt`
- `goal of the round.md`
- `Изменения в плане работы - цель.md`
- `Алгоритм создания ДПО.md`
- `Исполнение кода без внешней модели.md`
- `Обучение создания HTML CSS - 1st level.md`
- `planning of testing.md`
- `Claude session decisions.md`
- current slice queue / human slice map
- all active requirements discovered by R01/R02 that are not superseded

### WorkLeft / RAG
R11 не обязан перечитывать всё WorkLeft, но обязан читать **primary sections за каждым consequential dependency claim**. Базовые маршруты:
- `09.09.26WorkLeft-00-ИТОГ.md`
- `09.09.26WorkLeft-01-code-changes.md`
- `09.09.26WorkLeft-COVERAGE-01-three-projects-requirements.md`
- `09.09.26WorkLeft-MECH-01-mechanism-validation.md`
- `09.09.26WorkLeft-RESEARCH-03-dpo-algorithm-conformance.md`
- `09.09.26WorkLeft-RAG-MASTER-INDEX.md`
- relevant RAG primary lineage for DPO/DynamicGraph/DTC/IDE
- relevant TASK-PROMPTS / DEC / MATRIX / REPORT entries identified by R01–R03/R09

### Replanning inputs
- `PLAN-25-RUNS.md`
- `SOURCE-ROUTES.md`
- `COVERAGE-AND-WORKLOAD-GUARD.md`
- `PARALLEL-R01-R03-JOIN.md`
- `previous rounds thinking results.md`
- `HYBRID-ALGORITHMS-AND-RUNS.md`
- `MODEL-RUN-PACKETS-CP5-11-25.md`
- `PARALLEL9-MODEL-PACKETS.md`
- `ALGORITHM-EXPERIMENT-HANDOFF.md`
- `ALGORITHM-EXAMINATION-RESULTS6.md`
- `LEARNING-ACCEPTANCE-DELTA*` relevant sections if present

### R01–R10 results
R11 consumes current available, versioned evidence:
- R01: inventory/roots/owners
- R02: source requirements + owner decisions
- R03: RAG→primary routes
- R04: representation/many→many semantics
- R05: DPO/update/state/ordinary-consumer findings
- R06: domain/kNN/path/graph findings
- R07: full hybrid audit, not bounded old seam-only evidence
- R08: domain laws/data/DPO pair quality/evaluation families
- R09: process/full resource/cost-to-learning-signal evidence
- R10: nine-way join, conflicts and sufficient first-lesson subset

Если конкретный sibling ещё IN_PROGRESS, R11 может продолжать независимые claims по уже опубликованным primary/code evidence. Зависимый вывод маркируется PROVISIONAL с exact missing input; весь R11 не блокируется автоматически.

---

## 6. Required evidence model

Для каждой dependency/edge создать строку:

```
edge_id:
from_state:
to_state:
capability:
exact_input_contract:
exact_output_contract:
state_owner:
ordinary_consumer:
source_ids:
implementation_status: implemented | reusable_partial | missing_seam | unknown
evidence_kind: source_read | code_trace | empirical_observation | hypothesis | decision
first_lesson_required: yes | no | conditional
full_delivery_required: yes | no
why_required:
what_breaks_if_removed:
can_defer_without_invalidating_first_lesson:
deferred_owner:
deferred_trigger:
measurement_needed:
confidence:
unresolved_question:
```

Запрещено использовать `CHECKLIST_PRESENT` как объяснение зависимости.

---

## 7. R11-A — build the FIRST-LESSON causal DAG

### Prompt

Построй причинный путь **от выбора учебной задачи до semantic before/after observation обычного consumer**.

Минимально проверь следующие узлы:

1. **lesson selection**
   - domain/task family/weakness выбран CODEX Astra 6 UltraCode;
   - есть допустимый material/rights/context;
   - выбранная задача относится к реальной capability, а не synthetic fixture.

2. **DPO generation**
   - внешняя открытая китайская модель создаёт обе final sides;
   - chosen/rejected отвечают одному frozen context;
   - negative — hard/useful negative, не garbage и не правильный deterministic ответ, искусственно пониженный по происхождению.

3. **exact pair assessment**
   - CODEX Astra 6 UltraCode оценивает exact bytes/context;
   - changed bytes не наследуют старую оценку;
   - TIE/BOTH_BAD/UNJUDGEABLE не превращаются в forced preference.

4. **lossless/domain-sensitive representation**
   - проверить найденные R05 concerns по whitespace/truncation/input budget;
   - domain laws не должны исчезать при Compact/tokenization;
   - vector/token representation должна иметь version/meaning и actual consumer.

5. **authorized learning objective**
   - response-conditional DPO semantics;
   - policy/reference owner;
   - correct masks/conditioning;
   - m=0 at policy=reference не semantic rejection;
   - LoRA target/parameter owner/frozen base названы, если LoRA реально в path.

6. **state update**
   - bounded update;
   - one-pair/no-repeat;
   - full snapshot/rollback;
   - unknown update outcome не разрешает следующий update.

7. **accepted state publication/adoption**
   - trial evaluator reload ≠ ordinary runtime adoption;
   - назвать exact accepted checkpoint/adapter/state locator;
   - назвать ordinary runtime consumer, который действительно читает принятую версию.

8. **before/after semantic examination**
   - одинаковая panel/rubric/resources;
   - no-update control с сопоставимой memory/context;
   - domain mapping отдельно от solver;
   - retrieval/path separately diagnostic;
   - semantic quality + hard-law violations + retention;
   - wall time + latency + peak RAM/VRAM + persistent footprint.

### Required contrast

Для каждого узла спроси:
- если этот узел удалить, learning result станет **недопустимым**, **необученным**, **неиспользуемым** или только **не release-ready**?
- является ли dependency actual causal requirement или historical process rule?
- можно ли переиспользовать существующий implementation seam вместо нового слоя?

### Output A

- FIRST-LESSON DAG;
- minimal cut-set;
- таблица `KEEP_NOW / REUSE_NOW / FIX_FIRST / DEFER`;
- exact first implementation seams для R12/R18;
- список NOT_MEASURED, который нельзя превращать в defect или success.

---

## 8. R11-B — build the FULL-DELIVERY DAG without blocking R11-A

### Prompt

Построй отдельный граф всей программы, сохранив:
- S00–S10 и найденные extras;
- DPO/DynamicGraph;
- DTC full scenario;
- IDE / Visual Studio / VS Code / Continue integration;
- existing curriculum and domain program;
- Builder;
- PC continual learning / user feedback;
- release/host/Windows/native dependencies;
- eventual multi-domain and higher-order transfer.

Для каждой обязанности, не входящей в first lesson:
- old requirement ID/source;
- capability;
- почему не first-lesson prerequisite;
- owner;
- trigger, после которого она снова становится active;
- какая будущая observation закрывает обязанность.

### Forbidden shortcut

Нельзя:
- удалить IDE строку, потому что first lesson headless;
- удалить DynamicGraph, потому что первый CodeDocumentation lesson его не использует;
- сделать DynamicGraph обязательным перед первым lesson без actual code dependency;
- сделать все domain experiments prerequisite первого update;
- сделать весь Windows matrix prerequisite learning quality;
- сказать «defer» без owner+trigger.

### Output B

- FULL-DELIVERY DAG;
- preserved backlog;
- mapping `old obligation → new milestone → owner → trigger → examination`;
- critical shared seams между first lesson и full delivery;
- warning о тех deferred работах, которые нельзя забыть в R14/R19/R24.

---

## 9. R11-C — four-role hybrid connection audit

Это часть одного R11, не отдельный новый run.

### ILearningUnit
Проверить:
- domain/input vector/output schema;
- typed many→many;
- association каждого output с input;
- law-sensitive token/vector meaning;
- можно ли первый lesson получить без изменения Unit.

### ILearningFlow
Проверить:
- какие relations/corrections fixed;
- какой learned signal реально меняет связь;
- сравнение `fixed Units + changed Flow`;
- обратный contrast `changed Unit + fixed Flow`;
- не скрывается ли Flow learning под ordinary solver gain.

### ILearningOrchestrator
Проверить:
- async branches;
- dependency/order;
- total cost всех branches;
- learned path selection versus deterministic admissibility;
- cancellation/fallback;
- same-budget comparison.

### ILearningDynamicGraph
Проверить:
- primitive vs higher-order pattern relations;
- learned selector/relation recipient;
- new compositions from familiar primitives;
- actual adoption path L2/L3/consumer;
- не выдан ли document-solver gain за graph-policy learning.

### Output C

Таблица:
`role → current implementation → trainable state → deterministic constraint → learned authority → signal → ordinary consumer → first-lesson relevance → later experiment`.

---

## 10. R11-D — domain, dynamic kNN, token/vector, LoRA causal dependencies

Это не требует выбрать final design за R12, но R11 обязан показать, что именно является prerequisite, а что экспериментальным вариантом.

### Domain mapping
Разделить:
- oracle domain;
- predicted domain;
- solver quality.

Плохой domain mapping нельзя скрывать сильным solver. Хороший oracle-domain result не доказывает router.

### Dynamic kNN
Разделить:
- eligibility/law filter;
- feature meaning;
- metric/score family;
- normalization;
- k/radius/stopping;
- exact vs ANN;
- neighbor recall;
- neighbor consumer/aggregation/reranking;
- uncertainty/abstain.

Увеличение k при first-hit consumer не доказывает пользу k. Query-dependent score не автоматически metric-compatible.

### Vector/tokenization
Проверить:
- domain-specific significant whitespace/syntax/order;
- semantic compression;
- versioning;
- codebook/projection/index compatibility;
- shape equality ≠ semantic compatibility.

### LoRA
Проверить:
- adapted matrix/parameter group;
- frozen base;
- gradient/objective owner;
- output consumer;
- TeachingVector weighting ≠ доказанная semantic projection;
- LoRA update ≠ tokenizer/codebook update.

### Output D

Для каждого механизма:
- `first lesson must-have`;
- `candidate experiment`;
- `full delivery later`;
- falsifier;
- required R12/R13 decision.

---

## 11. R11-E — process-cost and anti-coverage gate

R11 обязан использовать R09 process evidence и классифицировать проверки.

### KEEP before first lesson
Только проверки, которые защищают hard deterministic invariants:
- exact pair integrity/provenance;
- state/snapshot/rollback;
- serialization/version compatibility;
- no-repeat;
- rights/admission;
- actual ordinary-consumer wiring;
- другие proven hard contracts.

### DO NOT use as learning efficacy proof
- line/branch coverage;
- green Windows Actions;
- fixture equality;
- build success;
- manifest completeness;
- count of tests;
- count of passed workflows.

### Learning efficacy evidence
- frozen incumbent/challenger;
- independent families;
- hard-law semantic accuracy;
- domain mapping;
- path/retrieval diagnostics;
- before/after;
- no-update control;
- retention;
- time/resources/footprint;
- bulk-to-bulk curves of **new lessons**;
- Astra semantic examination.

### Output E

Матрица:
`change/risk → deterministic guard → learning examination → release-only check → can defer before first lesson? → owner/trigger`.

---

## 12. Model decomposition rules

R11 — один run. A–E — sections, не пять независимых экспертов.

Child/subtask разрешён только если один вопрос не помещается в bounded context. Каждый child prompt обязан содержать:
- CURRENT TASK;
- NOT THE TASK;
- evidence-only sources;
- required skills actually to read;
- exact source subset;
- exact unresolved claim;
- allowed/forbidden actions;
- expected output schema;
- good example;
- bad example;
- edge cases;
- verification;
- return-only rights;
- call count.

### Recommended bounded child questions

**R11-C1 — ordinary adoption seam**
Только accepted state → ordinary consumer. Не повторять trainer audit.

**R11-C2 — first-lesson native boundary**
Только если конкретный consumer имеет реальную Windows/native dependency.

**R11-C3 — four-role dependency claim**
Только конкретный Unit/Flow/Orchestrator/Graph seam, не вся архитектура.

Нет auto-child для «проверить всё ещё раз».

---

## 13. Good examples

### Good 1 — dependency removal
`IDE screenshot capture` нужен full DTC delivery, но не нужен CodeDocumentation DPO lesson. R11 переносит его в full-delivery DAG, сохраняет owner/trigger, а first lesson оставляет headless.

### Good 2 — ordinary consumer
Trainer применил update и evaluator перечитал trial checkpoint. Это **не достаточно**. R11 требует доказать, что обычный runtime caller использует accepted state.

### Good 3 — kNN
Exact candidate set фиксирован. Baseline использует first-hit, challenger — R12-selected reranker. Так можно отличить retrieval от neighbor-consumer improvement.

### Good 4 — DPO
Китайская модель пишет обе стороны. CODEX Astra 6 UltraCode отклоняет пару как BOTH_BAD. Пара не обучает Student, и это хороший результат quality gate, а не «провал pipeline».

### Good 5 — hybrid connection
Units frozen, меняется только Flow relation. Если semantic result улучшился на held-out compositions, есть evidence за Flow contribution; если менялись Units, tokenizer и retrieval одновременно — causal claim недопустим.

---

## 14. Bad examples

### Bad 1
«Windows workflow красный, поэтому обучение невозможно».
Плохо, пока не доказана actual native dependency first lesson.

### Bad 2
«Coverage 95%, значит DPO работает».
Плохо: coverage не измеряет learning efficacy.

### Bad 3
«k=20 лучше k=5».
Плохо, если consumer всё равно берёт first hit.

### Bad 4
«LoRA weights изменились, следовательно vectorization улучшилась».
Плохо: изменение параметров не доказывает semantic representation quality.

### Bad 5
«Graph не активирован, значит первый documentation lesson нельзя запускать».
Плохо, если actual first-lesson path graph не использует.

### Bad 6
«R07 ещё IN_PROGRESS, поэтому R11 полностью ждёт».
Плохо: ждать можно только конкретный claim, который реально зависит от отсутствующего evidence.

---

## 15. Edge cases

Обязательно рассмотреть:
- pair valid, но Student не улучшается;
- chosen/rejected оба плохие;
- correct fixed deterministic answer;
- significant whitespace after 480 chars;
- unequal answer lengths;
- empty/near-empty response mask;
- policy=reference cold start;
- stale assessment after bytes/context changed;
- no-op update;
- unknown update outcome;
- compatible-looking shape with wrong semantic coordinates;
- exact search good, ANN misses;
- predicted domain wrong, solver right under oracle domain;
- k grows but first-hit consumer unchanged;
- fixed Units / changed Flow;
- changed Unit / fixed Flow;
- async branch fails after expensive compute;
- graph-policy unchanged but document solver improves;
- user feedback conflicts with protected domain;
- OOM or PC footprint exceeds envelope;
- release-only native dependency;
- sibling result arrives late and changes one edge only.

---

## 16. Verification of R11 result

R11 считается документно завершённым только если:

1. Есть **два** DAG: FIRST-LESSON и FULL-DELIVERY.
2. У каждой first-lesson edge есть source + contract/state/consumer + explanation `what breaks if removed`.
3. У каждой deferred obligation есть owner + trigger + later examination.
4. Четыре роли Unit/Flow/Orchestrator/DynamicGraph явно сохранены и сопоставлены actual mechanism.
5. DPO roles закреплены без ручного человека: CODEX/Astra select+judge; Chinese model authors both sides.
6. Domain mapping / kNN / representation / LoRA не сведены в один score.
7. Ordinary adoption отделён от trial evaluator reload.
8. Coverage/Windows не используются как learning-quality evidence.
9. First-lesson cut-set не требует unrelated whole-platform polish.
10. Full delivery не потерял original DTC/IDE/Graph/domain obligations.
11. Каждый NOT_MEASURED остаётся NOT_MEASURED.
12. R12 получает concrete design questions, R13 — experiment variables, R14 — backlog ordering, R17 — acceptance migration, R18 — first implementation packet.
13. Output readable по-русски: сначала человеческое объяснение, затем evidence tables.
14. Applied result записан только в `results/R11/{SOURCES.csv,FINDINGS.md,DECISIONS.md,CHANGE-MAP.csv,RUN-LOG.md}`, sibling carriers не переписаны.
15. GitHub readback подтверждает exact published bytes/commit.

---

## 17. Functional expectation test — не coverage

R11 не выполняет обучение, но обязан проверить, что будущие tasks способны измерить ожидаемое поведение.

### Domain capability matrix
Для каждого selected domain:
`law → required features/tokens → hard negative → mapping test → solver/path test → semantic rubric → retained capability`.

### Learning causality matrix
`baseline → one intervention → same panel → observation → alternative explanation → falsifier → next action`.

### DPO quality matrix
`same frozen context → Chinese chosen/rejected → CODEX exact-pair verdict → admitted update? → Student before/after → retention`.

### Hybrid efficiency matrix
`deterministic-only comparator → learned-only comparator if feasible → fixed hybrid → domain-adaptive hybrid`, при сопоставимых quality/compute. Не строить искусственно отсутствующие comparators только ради матрицы.

### PC continual-learning matrix
Полный future footprint:
base/runtime/tokenizer/encoder/codebook/adapters/optimizer/activations/index/graph/cache/snapshots/reindex + external teacher/judge evaluation cost, с отдельным persistent и peak memory.

Итоговый критерий — способность R13/R18 получить **различимый learning result**, а не количество тестов.

---

## 18. Required return packet

```
R11_STATUS:
snapshot_refs:
skill_load_status:
source_inventory_summary:
FIRST_LESSON_DAG:
FIRST_LESSON_MINIMAL_CUT_SET:
FULL_DELIVERY_DAG:
DEFERRED_OBLIGATIONS_WITH_OWNER_TRIGGER:
FOUR_ROLE_DEPENDENCY_TABLE:
DPO_ROLE_INVARIANTS_CHECK:
DOMAIN_KNN_VECTOR_LORA_DEPENDENCY_TABLE:
DETERMINISTIC_GUARD_VS_LEARNING_EXAMINATION_MATRIX:
FIRST_IMPLEMENTATION_SEAMS_FOR_R18:
DESIGN_QUESTIONS_FOR_R12:
EXPERIMENT_VARIABLES_FOR_R13:
BACKLOG_INPUTS_FOR_R14:
ACCEPTANCE_MIGRATION_INPUTS_FOR_R17:
NOT_MEASURED:
PROVISIONAL_CLAIMS_AND_EXACT_MISSING_INPUTS:
GOOD_BAD_EDGE_CASE_COVERAGE:
files_changed:
github_commit_sha:
remote_readback:
claim_scope:
evidence_scope:
count_claimed:
independent_evidence_count:
subagent_id_or_NONE:
downgrade_needed:
```

---

## 19. Stop condition

R11 останавливается, когда:
- first-lesson causal cut-set доказан до bounded unresolved architecture choices;
- full-delivery obligations сохранены;
- каждый unresolved вопрос имеет exact owner/consumer;
- дальнейшее чтение не меняет R11 dependency decision, а только относится к R12/R13/R14/R17/R18.

R11 не расширяется в бесконечный audit всей библиотеки.

---

## 20. Handoff

**R12** получает: exact architecture decisions needed для first seams и hybrid connections.  
**R13** получает: выбранные causal variables/controls и будущий learning examination scaffold.  
**R14** получает: preserved full-delivery obligations + owner/trigger ordering.  
**R17** получает: проверки, которые нужно перенести из first-lesson gate в release-only gate.  
**R18** получает: минимальный implementation packet, открывающий первый настоящий lesson.

**R11 success = путь к первому измеряемому обучению стал короче и причинно яснее, при этом ни одна обязательная capability не исчезла.**
