# R07 REPAIR — полный аудит гибридного learning-механизма до решения R12/R13/R18

Дата ремонта: 2026-10-13.
Последний review/repair work-order contract: 2026-10-13, revision 3 — устранены остаточные loopholes parallel join, first-teaching readiness, hybrid credit assignment, domain-law experiment design и anti-coverage acceptance.

Статус прежнего `results/R07`: **HISTORICAL / PARTIAL / SUPERSEDED FOR R07 COMPLETION**. Его code traces сохраняются как evidence, но прежний verdict `COMPLETE FOR ITS BOUNDED SCOPE` не закрывает R07 по активному плану, потому что обязательные primary inputs R07 не были прочитаны: `09.09.26WorkLeft-MECH-01-mechanism-validation.md`, `09.09.26WorkLeft-RESEARCH-03-dpo-algorithm-conformance.md`, `Алгоритм создания ДПО.md`, полная relevant RAG lineage и исходные требования четырёх ролей.

## 0. Зачем ремонт

R07 не должен быть ещё одним узким поиском пары producer→consumer seams. Его задача — проверить, что весь задуманный XIIGEN learning mechanism складывается в **компактный гибрид deterministic + learned algorithms**, реально способен обучаться на ПК, продолжать обучение от user feedback и улучшать решение по доменам.

Обычные unit/coverage/Windows GitHub Actions **не являются доказательством обучения**. Они допустимы только для детерминированных contracts, serialization, state integrity и других hard invariants. Learning efficacy проверяется как ML/learning system: frozen baseline/challenger, независимые task families, DPO pairs, bulk-to-bulk quality/time/resource measurements, domain mapping, causal ablations и Astra semantic examination.

## 1. Неподвижное распределение ролей

Это обязательный архитектурный инвариант для R07/R12/R13/R18/R21:

- **Human role / lesson selection = CODEX с моделью Astra 6 ultracode.** CODEX решает, чему Student надо учиться сейчас, выбирая weakness/domain/task family по evidence.
- **DPO author = внешняя открытая китайская модель.** Только она создаёт финальные `chosen` и `rejected` candidate texts, пригодные для обучения по разрешённой legal policy.
- **Exact pair judge = CODEX с моделью Astra 6 ultracode.** CODEX получает immutable task/context + exact chosen/rejected bytes и выносит ACCEPT / REJECT / TIE / BOTH_BAD / UNJUDGEABLE. CODEX не переписывает стороны и не становится author.
- **Student = XIIGEN.** Только разрешённая exact pair может менять Student state.
- **Post-update evaluator = отдельная evaluation роль.** Она измеряет Student behavior after update; pair validity и Student utility — разные вопросы.

Запрещено вернуть ручного человека в operational loop выбора/оценки DPO. Запрещено выдавать Astra/CODEX answer за chosen/rejected training target.

## 2. Архитектурные роли, которые R07 обязан восстановить

Не переименовывать смысл по текущим interface names. Сначала сопоставить actual code behavior исходным ролям.

- **ILearningUnit** — упрощённая матрица: domain + input-vector structure + output structure → `IQueryable`/typed many→many results requested output type.
- **ILearningFlow** — отношения между Units, correction входа/выхода и transport/association между ними.
- **ILearningOrchestrator** — более глубокая async модель: fan-out/fan-in, dependency/order, cancellation, budget/fallback и преобразование input→output через комбинации.
- **ILearningDynamicGraph** — higher-order relationships между паттернами разных порядков, reusable compositions и обучение выбора/связей между ними.
- **ILearningMachine** остаётся training owner. Четыре runtime-роли не превращать в четыре независимых trainers.

R07 обязан отдельно показать deterministic admissibility и learned authority для каждого edge/choice. Deterministic helper может ограничивать legal/typed space, но не должен незаметно стать trainable authority.

## 3. REQUIRED SKILLS AND GATES

Перед началом прочитать актуальные относящиеся sections на `lubochka/llm_mvp_core@codex/dpo-implementation/docs/skills`.

**SK-SESSION**
- `planning-session-startup-SKILL.md`
- `SESSION-START-PROMPT.md` и `AGENTS.md`, если доступны для этого execution context.

**SK-INTENT**
- `architect-behavior-classifier-SKILL.md`

**SK-PLAN**
- `reference-inventory-and-source-map-SKILL.md`
- `authority-requirement-binding-SKILL.md`
- `phase-planning-SKILL.md`

**SK-REVIEW**
- `plan-review-SKILL.md`
- `goal-delivery-completeness-SKILL.md`
- `generated-code-review-SKILL.md`
- `code-examination-SKILL.md`
- `claim-verification-SKILL.md` / current equivalent if renamed
- `audit-protocol-SKILL.md`
- `specificity-calibration-SKILL.md`
- `output-skepticism-SKILL.md` / current equivalent
- `reconnaissance-gate-SKILL.md`

**SK-ARCH**
- `trainable-architecture-decision-SKILL.md`
- `no-product-decisions-SKILL.md`
- `human-readable-architecture-audit-SKILL.md`
- `output-readability-gate-SKILL.md`

**SK-DOC / CODE TRACE** — обязателен для R07-B/F/G/J, потому что они делают claims о существующем implementation path:
- `implementation-doc-uml-SKILL.md`
- `implementation-doc-algorithms-SKILL.md`
- `implementation-doc-code-refs-SKILL.md`
- `implementation-doc-status-SKILL.md`
- `implementation-doc-hierarchy-SKILL.md`

**SK-PROMPT / HUMAN COMMUNICATION**
- `prompt-patch-authoring-SKILL.md`
- human-readable/output-readability rules: explain result in Russian human architecture language before ledgers/IDs.

**SK-EXPERIMENT**
- `experiment-design-SKILL.md`
- `improvement-measurement-SKILL.md`

Historical fixed pass counts, old replay percentages or old model names inside skills are **not automatically live requirements**. Import the reasoning/evidence gates, not stale constants.

## 4. PRIMARY CONTEXT — MUST READ, NOT HANDOFF-ONLY

### Planning repo `lubochka/xiigen-planning@llm_plan3`

1. `llm mvp/design to code session/original task.md`
   - original four roles, dynamic DPO graph and project/domain roots.
2. `llm mvp/design to code session/Алгоритм создания ДПО.md`
   - original DPO procedure and pair lifecycle.
3. `llm mvp/design to code session/09.09.26WorkLeft-MECH-01-mechanism-validation.md`
   - full mechanism baseline, actual implementation history, known graph gap.
4. `llm mvp/design to code session/09.09.26WorkLeft-RESEARCH-03-dpo-algorithm-conformance.md`
   - DPO one-pair cycle, legal/eval boundary, four-role conformance.
5. `replanning/SOURCE-ROUTES.md`
6. `replanning/COMMON-CONTRACT.md`
7. `replanning/PARALLEL-R01-R03-JOIN.md`
8. current R04/R05/R06 carriers and **their exact handoff rows**, not only summaries.
9. `replanning/previous rounds thinking results.md` only as historical evidence; never as substitute for primary reads.
10. Relevant original prompts/work-left/RAG routes discovered by R01–R03 for:
   - domain/representation/tokenization;
   - DPO generation and pair quality;
   - LoRA/checkpoint/update lifecycle;
   - BM25+kNN / dynamic kNN / learned selector;
   - DynamicGraph relations;
   - user feedback / continued learning;
   - domain experiments and quality-speed-accuracy program.

### Core repo `lubochka/llm_mvp_core@codex/dpo-implementation`

Do **repository reconnaissance first**, then code trace. Do not start from S04-only paths.

Build a current tree/source map for at least:
- core LearningMachine contracts and actual logical Unit/Flow/Orchestrator/DynamicGraph implementations;
- DPO trainer/admission/objective/token transport;
- LoRA/adapters/checkpoint publication/load/adoption;
- routing/domain detector/representation/tokenization;
- BM25+kNN/index/search/neighbor consumer;
- graph edge authority / L2 / L3 / association;
- ordinary product consumers for at least the first lesson candidate;
- feedback/diagnostics source that can create the next weakness/lesson.

A file named Vector, Learning, DPO or Graph is not proof of semantic ownership. Trace producer→state→consumer.

## 5. RAG / REFERENCE TRAVERSAL

R07 has a **two-phase dependency rule** because the current execution order runs R01–R06 in parallel.

- **Phase R07-PREFLIGHT may start immediately:** direct repository reconnaissance, primary-source reads, RAG traversal, and a provisional source/algorithm map. It MUST NOT make final recipient, tokenizer/vector, kNN, DPO/LoRA, architecture-removal, or first-lesson decisions.
- **Phase R07-SYNTHESIS does not wait for sibling DONE.** At synthesis start, snapshot every currently observable R01–R06 carrier. For any missing carrier write `RESULT_NOT_OBSERVED@checkpoint` and continue from direct primary/code evidence. Immediately before R07-J publication, perform one bounded refresh of R01–R06 and semantically ingest only the delta relevant to R07 decisions.
- Missing or partial sibling evidence does not permit invention, but it is **claim-local, not a global stop**. It blocks only a decision whose required fact cannot be established from R07 direct primary/code evidence. Every such blocker must name the exact missing fact, affected decision, owner and falsifier.
- R01–R06 handoffs never substitute for direct primary reads by R07.

After the join, R07 must directly follow every RAG branch that carries a **decision needed for R07** until it reaches the primary decision/code/source.

For every consequential claim record:
`route -> primary source -> exact span -> current code owner -> effect on architecture`.

Mandatory RAG subjects:
1. original four-role semantics;
2. DPO legal teacher / pair author / judge separation;
3. tokenization/vector representation;
4. domain mapping and domain laws;
5. BM25+kNN / dynamic neighbor selection / final arbiter;
6. LoRA/checkpoint lifecycle;
7. user-feedback / weakness→lesson loop;
8. DynamicGraph authority and higher-order composition;
9. learning evaluation / domain bulk measurements.

A copied citation from R04–R06 is advisory evidence, not a direct read.

## 6. CURRENT TASK

Produce a **complete causal architecture of the hybrid learning system** and decide which mechanisms are real, duplicated, missing, disconnected, or semantically incompatible.

Required end-to-end chain:

`request`
→ domain / intent / stack detection
→ domain-specific token/vector representation
→ deterministic admissible candidate set
→ BM25/kNN / dynamic neighbor retrieval where applicable
→ learned Unit/Flow/selector authority
→ async Orchestrator execution
→ DynamicGraph higher-order composition where applicable
→ typed many→many result + associations
→ ordinary user-visible result
→ weakness/error/feedback observation
→ CODEX/Astra 6 ultracode selects lesson
→ open Chinese model authors exact chosen/rejected
→ CODEX/Astra 6 ultracode judges exact pair
→ lossless domain-sensitive tokenization
→ DPO/LoRA update of the **correct semantic recipient**
→ accept / rollback
→ publication + ordinary adoption
→ next request through the same changed recipient
→ bulk-to-bulk independent evaluation.

For every arrow document:
- owner;
- input/output type and multiplicity;
- domain/schema/tokenizer/vector/index/model/checkpoint version;
- deterministic hard constraints;
- learned state and update signal;
- actual mutable parameter/state family;
- ordinary caller/consumer;
- fallback/abstention;
- cost/time/memory contributor;
- exact evidence;
- falsifier.

## 7. REQUIRED SUBTASKS

### R07-A — Repository and decision coverage repair

**Goal:** prove R07 has read the whole relevant design surface before narrowing.

Actions:
- verify R01/R03 maps against current trees;
- directly read both assigned WorkLeft files and original DPO algorithm;
- enumerate relevant RAG branches and exact primary sources;
- enumerate current Unit/Flow/Orchestrator/Graph/DPO/LoRA/kNN/token/domain/feedback code owners;
- mark each as DIRECT_READ / HANDOFF_ONLY / NOT_READ / HISTORICAL.

**Pass condition:** no consequential R07 architecture claim is based only on sibling summaries.

### R07-B — Four-role graph reconstruction

Reconstruct:
- nodes, edges, fan-out/fan-in;
- typed many→many payload association;
- async ordering/cancellation;
- Flow correction semantics;
- Orchestrator vs Flow responsibility;
- DynamicGraph higher-order composition;
- deterministic vs learned authority per edge;
- actual training owner.

Produce at least:
1. one single-domain request;
2. one mixed-domain request;
3. one same-input/two-valid-combinations walkthrough.

### R07-C — Domain laws → token/vector representation

For each baseline domain/family found in active plan, produce a **domain representation passport**:
- domain law / invariant that determines correctness;
- tokens/features that preserve that law;
- what must never be normalized away;
- vector meaning and version;
- oracle vs predicted domain conditions;
- expected OOD/unknown behavior;
- good positive and hard-negative representations.

Do not assume one tokenizer/vectorizer is optimal for all domains.

### R07-D — Dynamic kNN and retrieval law

For each domain or domain family determine:
- whether kNN is appropriate at all;
- candidate metric;
- feature weighting;
- `k`, radius or confidence as **dynamic function inputs**, not universal constants;
- exact vs ANN choice;
- index/version/reindex contract;
- interaction with BM25, learned selector and final arbiter;
- first-neighbor evidence vs actual winning solution.

Required output: candidate dynamic function form and what observation would falsify it. No hard-coded magic number may be called “best” without measured domain evidence.

### R07-E — DPO quality and semantic recipient

Trace:
`weakness -> selected lesson -> exact pair -> domain-sensitive tokens -> objective -> mutable state -> accepted checkpoint -> ordinary consumer`.

Check:
- chosen/rejected both authored by open Chinese model;
- CODEX/Astra selects lesson and judges exact pair;
- TIE/BOTH_BAD/UNJUDGEABLE preserved;
- weak Student response is context, not automatically final rejected;
- no paid/reference output enters chosen/rejected;
- no Compact/truncation destroys the material difference;
- objective optimizes response tokens/semantics actually used by the chosen recipient;
- one-pair/no-repeat and full-state accept/rollback;
- pair validity separated from usefulness of the resulting update.

### R07-F — LoRA and vectorized update

Establish what LoRA/adaptation actually changes:
- adapter topology/parameter group;
- token input and mask;
- projection from domain representation to training tokens;
- publication/version compatibility;
- ordinary consumer adoption;
- whether learned delta can be represented/reused compactly on PC;
- whether a result is mistakenly called vectorized because DTO shape is `float[]`.

Required distinction:
`representation vector` ≠ `LoRA parameter delta` ≠ `retrieval vector` ≠ `graph state`.

### R07-G — Deterministic + learned interaction

Classify every mechanism:
- HARD DETERMINISTIC INVARIANT;
- LEARNED AUTHORITY;
- LEARNED SCORE used by deterministic resolver;
- EVAL-ONLY helper;
- DUPLICATE/DEAD wrapper;
- MISSING CONNECTION.

At minimum cover:
- Stack/domain detection;
- normalization;
- BM25;
- kNN;
- dynamic k/radius/weights;
- Unit mapping;
- Flow correction;
- Orchestrator path/budget;
- Graph edge authority;
- LoRA/DPO;
- final arbiter/fallback.

### R07-H — PC compactness and continual-learning viability

Without running training here, produce the exact measurement plan for:
- peak RAM/VRAM;
- persistent model/index/adapter bytes;
- reindex/reload cost;
- per-pair update time;
- evaluation time;
- ordinary inference latency;
- storage growth per bulk;
- rollback cost;
- continued learning from user feedback without replaying accepted pairs illegally.

A compact system claim is invalid until these are observed on a declared PC profile.

### R07-I — Causal examination matrix for R13

Define ML-style examinations, **not coverage**.

Minimum contrasts:
1. same memory/context, no-update vs one accepted update;
2. fixed candidate pool, changed learned selector;
3. fixed selector, changed domain pool;
4. frozen Units, changed Flow relation;
5. frozen Flow, changed selector/Orchestrator;
6. flat primitive relations vs higher-order DynamicGraph composition;
7. representation A/B with downstream algorithm frozen;
8. kNN function A/B with same representation and corpus;
9. accepted child loaded only in evaluator vs accepted child used by ordinary caller;
10. 2×2 interaction for two mechanisms suspected to work only jointly.

For every experiment:
- baseline;
- enhanced;
- one primary causal factor;
- domain/family;
- independent holdout;
- positive/negative/tie examples;
- accuracy/semantic-quality metric;
- domain mapping accuracy;
- path validity/association;
- time;
- peak memory;
- persistent bytes;
- retention;
- uncertainty/CI or honest INCONCLUSIVE boundary;
- falsifier;
- verdict rule IMPROVE / REGRESS / INCONCLUSIVE / INVALID_EVIDENCE.

## 8. GOOD EXAMPLES

**Good — domain-sensitive representation**
C# call-chain task keeps caller/callee identity, symbol kinds and dependency direction. A code formatter change does not erase semantic edges. kNN metric weights dependency-compatible features more than punctuation similarity.

**Good — DPO hard negative**
Chosen and rejected solve the same task and have the same output contract; rejected contains one plausible but material causal error. Open Chinese model authors both exact sides; CODEX/Astra judges the immutable pair.

**Good — dynamic kNN**
For sparse legal-domain evidence, the function widens radius but requires stronger domain-law compatibility; for dense duplicate-rich documentation examples it lowers effective k or deduplicates families. The function is a hypothesis until bulk evidence supports it.

**Good — hybrid deterministic/learned**
Deterministic filter rejects type-incompatible graph edges. Learned selector chooses among remaining legal candidates. Quality improvement is credited to selector only under fixed candidate pool.

**Good — LoRA adoption**
One accepted pair changes a named adapter/checkpoint. The same ordinary consumer loads the accepted child. On unseen tasks of the same family quality improves without regression on protected families.

## 9. BAD EXAMPLES

- “42 tests pass, therefore learning works.”
- “Windows Action is green, therefore DPO is correct.”
- “Checkpoint exists, therefore ordinary output changed.”
- “First nearest neighbor is correct, therefore kNN algorithm is optimal.”
- “Cosine similarity increased, therefore user answer improved.”
- “Vector has the same length, therefore tokenizer/index/model semantics are compatible.”
- “Chinese model generated a pair, therefore it is accepted.”
- “Astra preferred answer A, therefore use Astra output itself as chosen.”
- “Correct deterministic candidate is rejected because it was deterministic.”
- “Change tokenizer, k, LoRA, domain router and graph at once; reward increased, so all are good.”
- “R04/R05/R06 said it, so R07 does not need the primary source.”

## 10. EDGE CASES

Must explicitly reason about:
- unknown/mixed domain;
- same task has several valid outputs;
- both DPO sides are good;
- both sides are bad;
- insufficient evidence / legal uncertainty;
- shape-compatible but semantically incompatible versions;
- stale child/parent checkpoint;
- evaluator loads child while ordinary caller stays on parent;
- no neighbor above meaningful confidence;
- nearest neighbor belongs to wrong domain-law family;
- dynamic k changes selected consumer and confounds credit;
- async branch completes out of order;
- cancellation after partial fan-in;
- 1→0, 1→N and N→M typed associations;
- higher-order graph path adds cost but no quality;
- adaptation improves one domain and regresses another;
- user feedback is wrong or adversarial;
- repeated feedback corresponds to already accepted pair;
- cold start where model/reference margin is ~0;
- domain representation changes and index must be rebuilt;
- fixed deterministic solution is already optimal.

## 11. CURRENT DOCUMENTARY VERIFICATION

R07 repair is accepted only if:
- all mandatory primary sources appear in `SOURCES.csv` as direct reads with exact spans/limits;
- current repo reconnaissance covers all architecture families above;
- every important finding has source→code→consumer or explicit gap;
- four roles are reconstructed, not merely named;
- domain/token/kNN/DPO/LoRA interaction is represented in the same causal map;
- previous bounded R07 findings are reclassified as preserved evidence, contradicted, or superseded;
- no Windows/coverage result is used as efficacy proof;
- no learning improvement is claimed without an experiment;
- role split CODEX/Astra ↔ Chinese DPO author ↔ Student is explicit in FINDINGS, DECISIONS and RUN-LOG.

## 12. OUTPUT / RETURN PACKET

Update **the same five R07 carriers**, preserving historical evidence but making the current status unambiguous:

1. `results/R07/SOURCES.csv`
   - direct primary reads, RAG routes, code owners, spans, versions, read depth.
2. `results/R07/FINDINGS.md`
   - repaired full architecture map;
   - domain representation passports;
   - kNN/dynamic-function hypotheses;
   - DPO/LoRA semantic-recipient trace;
   - deterministic/learned classification;
   - compact-PC evidence needs.
3. `results/R07/DECISIONS.md`
   - reuse/merge/remove/implement decisions;
   - selected unresolved choices for R12;
   - exact experiment requests for R13;
   - minimal implementation seams for R18.
4. `results/R07/CHANGE-MAP.csv`
   - original requirement → primary evidence → current mechanism → gap → owner → functional examination.
5. `results/R07/RUN-LOG.md`
   - what was actually read and traced;
   - direct vs inherited evidence;
   - remaining bounded gaps;
   - current status.

Required current status until this repair is executed:
`R07_REPAIR_REQUIRED — previous bounded R07 preserved as partial evidence; full hybrid architecture verdict not yet complete.`

## 13. STOP CONDITION

R07 may become COMPLETE only when it can answer, with evidence:

1. What exact mechanisms learn?
2. What exact deterministic mechanisms remain hard constraints?
3. How are Unit, Flow, Orchestrator and DynamicGraph connected?
4. How is domain identified and represented?
5. How should domain-specific kNN behavior be varied and measured?
6. How are valid positive/negative/tie DPO examples constructed and judged?
7. What tokenization reaches the objective without losing the domain distinction?
8. What LoRA/state is updated, and which ordinary caller consumes it?
9. How does the accepted change return into the next user request?
10. What experiment can prove improvement in accuracy/time/PC cost from bulk to bulk?
11. What evidence would show the architecture is going in the wrong direction?
12. What is the **smallest implemented dynamic-algorithm seam** needed to start real teaching immediately?

If any answer is unknown, R07 may still publish the exact gap and owner, but it must not call the whole R07 complete.


---

## 14. R07 EXECUTION SHAPE FOR A MODEL — TEN BOUNDED SUBTASK PACKETS

This section is authoritative when the shorter R07-A…I descriptions above are insufficient for execution. These are **subtasks inside R07**, not new top-level replanning runs and not R26+. They exist to prevent one model pass from narrowing the problem to a convenient slice.

### Common execution envelope for every R07 subtask

This block is **binding for every R07-A…R07-J packet**. A child prompt that omits any field below is invalid and must be repaired before its output can close a R07 subtask.

**CURRENT TASK:** execute only the named R07 subtask and return evidence into the five existing R07 carriers.

**NOT THE TASK:** product implementation; Windows GitHub Actions; line/branch coverage campaigns; broad “make tests green” work; training a model; changing sibling result folders; inventing architecture because a source was hard to locate.

**EVIDENCE-ONLY:** until a named experiment has actually run, learning gain, best tokenizer/vectorizer, best dynamic-k function, LoRA efficacy, PC fit and cross-domain superiority remain NOT_MEASURED/HYPOTHESIS. Documentary or code evidence may establish topology, ownership, compatibility, missing seams and experiment design only.

**Model role:** architecture/research worker. In the future learning loop the **human decision role is performed by CODEX using Astra 6 ultracode** for (1) lesson/weakness selection and (2) exact immutable DPO-pair quality judgment. The **external open Chinese model authors both chosen and rejected DPO sides only**. XIIGEN is Student. CODEX/Astra output itself must never be inserted as chosen/rejected.

**required_skills_and_gates:** SK-SESSION + SK-INTENT + SK-PLAN + SK-REVIEW + SK-ARCH + SK-PROMPT/HUMAN. Add SK-DOC/CODE TRACE for B/F/G/J and whenever an implementation-path claim is made. Add SK-EXPERIMENT for C/D/E/F/H/I/J. Record exact skill files actually read; filename lists without reads are not load evidence.

**context_files_to_read:** the packet-specific context/references below plus section 4 PRIMARY CONTEXT, section 5 RAG subjects, the latest relevant R01–R06 carriers after join where required, and every direct primary/code source needed to falsify the packet's claims.

**implementation_or_artifact_details:** for any claim that a mechanism exists or is missing, name repo/ref/path/class-or-symbol/call-chain/state family/ordinary consumer. A planning document, status ledger or checkpoint filename is not implementation evidence.

**original_task_requirements_to_cover:** preserve the four behavioral roles; combined deterministic+learned architecture; domain-sensitive mapping/token/vector law; DPO positive/negative/tie provenance; dynamic kNN/retrieval; LoRA/state recipient; PC continual learning; ordinary adoption; behavioral bulk-to-bulk examination.

**mental_simulation_required:** execute the packet-specific simulations plus at least one happy path, one mixed/unknown-domain path, one failure/rollback or abstention path, and one “hidden deterministic shortcut” check where applicable.

**good_examples / bad_examples:** use the packet-specific examples plus sections 8–9. An executor may add examples but may not delete the governing ones.

**allowed_files:** read any file in the two named repositories needed by the packet; write only the five `results/R07/*` carriers. During repair of this work order itself, the parent may edit `R07-REPAIR-WORK-ORDER.md` and navigation docs explicitly named by the user.

**forbidden_files:** sibling `results/Rxx` writes; product code; tests; workflows; weights; datasets; core skills; generated model artifacts. Sibling carriers are read-only evidence.

**allowed_commands/actions:** repository/file/tree/search/read operations; documentary writes limited to allowed files; semantic comparison; code tracing; source/RAG traversal; readback/diff/commit ancestry verification. No product/runtime execution is implied by “verification”.

**forbidden_commands/actions:** Windows/GitHub Actions runs as evidence of learning; coverage campaigns; product builds/tests used as ML efficacy proof; trainer execution; paid/reference generation inserted into training sides; external model calls outside the explicitly authorized future DPO roles; force-push; silent sibling rewrites.

**RAG rule:** follow references until the decision-bearing primary source or code owner is reached. A sibling summary, RAG index, status file or WorkLeft cross-reference is navigation/evidence, not final authority by itself.

**Evidence types:** DIRECT_PRIMARY_READ, DIRECT_CODE_TRACE, SIBLING_HANDOFF, OWNER_DECISION, HYPOTHESIS, NOT_MEASURED. Never promote HYPOTHESIS to result.

**Functional verification rule:** the acceptance question is “does this evidence cover the functional expectation and can a later experiment falsify it?”, not “how many tests pass?”. Deterministic tests may prove type/serialization/state-integrity invariants; they do not prove learning efficacy.

**verification_required:** before marking a packet complete, re-open every consequential primary/code span supporting its decisions, check ordinary-consumer adoption, check deterministic-vs-learned ownership, identify at least one falsifier, and verify no NOT_MEASURED claim was promoted. For A and J also verify the R01–R06 receipt table is complete.

**return_packet_schema:**
- task_id and checkpoint;
- exact sources/refs/blobs/spans actually read;
- requirement/mechanism IDs covered;
- findings with confidence and falsifier;
- deterministic-vs-learned classification;
- accepted/rejected sibling handoffs;
- unresolved gaps + owner;
- functional examination that would validate the finding later;
- changes to FINDINGS/DECISIONS/CHANGE-MAP/SOURCES/RUN-LOG;
- claims that remain NOT_MEASURED;
- `claim_scope`, `evidence_scope`, `count_claimed`, `independent_evidence_count`, `subagent_id_or_NONE`, `downgrade_needed`.

A subtask is not accepted if its evidence scope is narrower than its claim scope.
### R07-A PACKET — Whole-repository reconnaissance and R01–R06 semantic join

**Prompt.** Build a repository-wide map before making any algorithm decision. Prove that R07 is not another S04-only pass. Inventory **the whole design-to-code directory first**: every slice/extras family, every `WorkLeft*`/`workLeft*` file, every general task/goal/prompt file, every RAG root/index and every code-family route. Only after inventory may you classify entries as relevant/deferred/historical, with a written reason. Then map relevant code families and ordinary consumers. Then ingest the latest observable R01–R06 packets and state exactly what each contributes or fails to contribute to R07.

**Context / required references.**
- Planning: original task.md, Алгоритм создания ДПО.md, all workLeft/WorkLeft relevant to learning, mechanism, DPO, domains, quality/speed/accuracy, integration; task/goal/prompt files in the design-to-code directory; SOURCE-ROUTES.md; COMMON-CONTRACT.md; PARALLEL-R01-R03-JOIN.md; latest R01–R06 five-carrier packets.
- Core: current branch tree, not a preselected slice list.
- Previous-round thinking: historical comparison only.

**RAG traversal.** Create a visited-set keyed by repo/ref/path/span and follow every route touching the nine mandatory RAG subjects from section 5.

**Required output.** A coverage matrix:
source family -> primary files -> code family -> functional expectation -> R01..R06 handoff -> R07 direct-read status -> remaining gap.

**Good example.** “R05 says pair truncation breaks the meaningful delta; R07 re-reads the primary DPO algorithm and current projector code, then accepts only the part supported by both.”

**Bad example.** “R05 already checked DPO, so mark DPO complete.”

**Edge cases.** renamed file; stale RAG target; same decision copied into several WorkLeft files; current code moved; R01 carrier partial; sibling statuses from different SHAs.

**Verification.** No R07-B…J synthesis may claim completeness until every required source family is DIRECT_READ or explicitly GAP with owner. Repository tree listing alone does not satisfy this.

### R07-B PACKET — Reconstruct ILearningUnit → ILearningFlow → ILearningOrchestrator → ILearningDynamicGraph

**Prompt.** Reconstruct the intended four behavioral levels from original requirements and actual code behavior. Do not infer semantics from interface names. Show what each level consumes, emits, learns, constrains, and delegates.

**Context / references.**
- original four-role requirement;
- MECH-01;
- current LogicalContracts, LearningMachine contracts, implementations and ordinary callers;
- R04/R06 handoffs for typed relations and graph authority, revalidated against code.

**Required artifact.** Human-readable Russian architecture plus Mermaid/PlantUML and a table:
level -> domain -> input -> output multiplicity -> associations -> deterministic constraints -> learned state -> update signal -> async behavior -> consumer.

**Mental simulations.**
1. one domain, one valid path;
2. one domain, two valid combinations;
3. mixed domain with fan-out/fan-in;
4. cancellation after one branch returns;
5. N→M outputs with associations preserved.

**Good example.** Deterministic type compatibility produces legal edges; learned authority selects among them; Orchestrator schedules selected work; Graph stores higher-order relation evidence.

**Bad example.** “Everything implementing ILogical* is a LearningUnit.”

**Edge cases.** Flow and Orchestrator both appear to route; Graph wrapper contains no state; one level exists only as adapter; same output has multiple provenance associations.

**Verification.** Every edge has an owner, versioned payload, fallback, hidden-deterministic-shortcut check and an ordinary consumer or explicit missing-connection finding.

### R07-C PACKET — Domain mapping and domain-law-sensitive representation/tokenization

**Prompt.** For every baseline domain/family in the active plan, determine which facts must survive normalization/tokenization/vectorization for correctness. Build representation passports and explicitly separate domain detection, semantic representation, training tokens and retrieval vectors.

**Context / references.**
- R02/R04/R08 domain requirements when available;
- domain WorkLeft/RAG primaries;
- current StackDetector/domain detector/normalizer/tokenizer/vectorizer/projector code;
- subject-matter sources that establish real-world domain laws.

**Required passport fields.**
domain_id, law/constraint_id, authority source, raw signals, protected distinctions, canonical tokens, training-token mapping, retrieval-vector meaning, units/types, version, OOD behavior, index compatibility, positive representation, hard negative, falsifier.

**Good example.** SQL domain preserves NULL/cardinality semantics even if surface formatting normalizes away; code domain preserves caller/callee direction.

**Bad example.** One global embedding scheme declared optimal because dimensions match.

**Edge cases.** mixed domain; unknown domain; lexical collision across domains; same token text different semantics; representation migration while old index is live.

**Functional examination.** Freeze downstream learner and compare representation A/B on domain mapping, path validity and semantic quality. A representation is not “better” because cosine distance looks nicer.

### R07-D PACKET — Dynamic kNN/BM25/retrieval law per domain

**Prompt.** Determine where retrieval helps, what is deterministic, what should be learned, and how k/radius/weights should vary by domain conditions. Do not select magic constants from intuition.

**Context / references.**
- R06 retrieval/arbiter evidence;
- BM25, kNN, vector-search, index/version code;
- domain representation passports from R07-C;
- current corpus/index ownership and final consumer.

**Required candidate function form.**
Describe inputs such as density, confidence, domain purity, query entropy, duplicate-family ratio, distance distribution, latency budget and memory budget; describe outputs k, radius, weight vector, exact/ANN mode or abstain. Mark each term HYPOTHESIS until measured.

**Good example.** Sparse domain widens radius only when law-compatible evidence remains high-confidence; dense duplicate-heavy domain deduplicates families before selecting neighbors.

**Bad example.** “k=5 is standard, therefore k=5.”

**Edge cases.** no meaningful neighbor; nearest item from wrong domain-law family; ANN returns unstable order; dynamic k changes downstream consumer and creates a credit confound; index version mismatches representation.

**Functional examination.** Same corpus + same representation + same downstream selector, vary one retrieval law. Measure task success, domain-law violations, latency, memory and abstention. Preserve first-neighbor evidence separately from final winning solution.

### R07-E PACKET — DPO pair construction, provenance and exact-pair judgment

**Prompt.** Reconstruct the one-lesson DPO path and prove role separation. Define what constitutes a useful chosen/rejected/tie example for each domain without using coverage proxies.

**Binding roles.**
- CODEX / Astra 6 ultracode selects the lesson/weakness;
- external open Chinese model authors **both exact sides**;
- CODEX / Astra 6 ultracode judges immutable chosen/rejected plus task/context;
- XIIGEN is Student;
- post-update evaluation is separate.

**Context / references.**
Алгоритм создания ДПО.md, RESEARCH-03, legal-policy primaries, current pair builder/admission/objective code, R05 handoff, domain passports.

**Required pair record.**
lesson_id, domain, immutable context hash, author provenance, chosen bytes/hash, rejected bytes/hash, material semantic difference, law/constraint affected, judge verdict, judge evidence, TIE/BOTH_BAD/UNJUDGEABLE, admission state, pair_id/no-repeat state.

**Good example.** Both answers are plausible; rejected has one material causal/domain-law mistake while matching task/output contract.

**Bad example.** Weak Student output is automatically used as rejected; CODEX-generated ideal answer is inserted as chosen; stylistic preference masquerades as factual correctness.

**Edge cases.** both sides good; both bad; legal uncertainty; tiny meaningless difference; truncation deletes the distinguishing span; repeated pair; adversarial user feedback; pair valid but useless for the selected recipient.

**Functional examination.** First validate pair quality/provenance; only then measure Student update utility. Never merge those verdicts.

### R07-F PACKET — LoRA / learner-state recipient / token-to-parameter path

**Prompt.** Trace exact training tokens to the mutable parameter/state family and from accepted state into the ordinary consumer. Distinguish LoRA delta, retrieval vector, representation vector and graph state.

**Context / references.**
- current trainer, tokenizer/projector, objective/loss, adapter/LoRA implementation, checkpoint save/load/publication/adoption;
- R05 checkpoint findings;
- actual ordinary caller of the chosen capability.

**Required trace.**
pair bytes -> token ids/mask -> model input -> objective -> trainable parameters -> optimizer/update -> child state -> acceptance/rollback -> version publication -> ordinary load -> changed inference path.

For LoRA name base matrix dimensions, rank/adapters, trainable tensors, frozen base, checkpoint identity, compatibility and consumer.

**Good example.** A named adapter changes; accepted child version is loaded by the same user-facing consumer; parent remains rollback target.

**Bad example.** “float[] changed, therefore LoRA learned”; “checkpoint sealed, therefore user output changed.”

**Edge cases.** same tensor shape but incompatible semantics; partial snapshot; parent/child mismatch; evaluator loads child but product caller uses parent; tokenizer version changes without index rebuild.

**Functional examination.** Same frozen request panel, parent vs accepted child through the ordinary caller; record quality and resource delta. Publication alone is not evidence.

### R07-G PACKET — Deterministic/learned interaction and credit assignment

**Prompt.** Create the combined-algorithm map the previous plan was missing. For every mechanism, decide whether it is hard deterministic, learned authority, learned score inside deterministic resolution, eval-only, duplicate/dead, or missing connection.

**Must cover.** Stack/domain detection; normalization; BM25; kNN; dynamic k/radius/weights; Unit mapping; Flow correction; Orchestrator selection/budget; Graph edge authority; final arbiter; DPO/LoRA; rollback/version/adoption.

**Required output.**
mechanism -> class -> input -> state -> owner -> allowed decision power -> downstream consumer -> causal ablation -> falsifier.

Also identify **manipulable connections**: which component can be frozen while another changes, and what observation would attribute gain/regression.

**Good example.** Deterministic admissibility freezes legal candidates; learned selector alone changes target, allowing causal attribution.

**Bad example.** Add randomness and call the component “learning”.

**Edge cases.** deterministic heuristic encodes all decisions leaving learned model cosmetic; learned score exists but resolver ignores it; two learners update same semantic decision; learned selector changes candidate pool indirectly.

**Verification.** At least one isolating ablation exists for every claimed learned authority. If not, mark credit assignment unresolved.

### R07-H PACKET — PC compactness and continual learning from user input

**Prompt.** Decide whether the architecture can plausibly fit the intended PC workflow and continue learning incrementally without illegal replay or uncontrolled state growth. This subtask designs measurements; it does not invent successful numbers.

**Context / references.**
- actual state/checkpoint/index/cache formats;
- user-feedback path;
- one-pair/no-repeat/rollback rules;
- R09 cost evidence when available.

**Measurement ledger.**
Per accepted lesson and per bulk: update wall time, inference latency, peak RAM/VRAM, temporary bytes, persistent adapter/model/index/graph bytes, reindex/reload time, rollback bytes/time, external model calls, pair-author/judge cost, storage growth, retention delta.

**Good example.** Report adapter bytes together with index, graph, snapshots and peak coexistence during safe swap.

**Bad example.** “LoRA file is 2MB so the system fits on PC.”

**Edge cases.** old+new index overlap; rollback requires full snapshot; feedback storm; already accepted pair repeated; user feedback wrong; no GPU; cold start; domain expansion causes unbounded graph/index growth.

**Functional examination.** A compactness claim requires an explicit PC profile and observed measurements later; until then verdict is PLAUSIBLE/UNPLAUSIBLE/UNKNOWN, never PASS.

### R07-I PACKET — Learning-system evaluation design, bulk-to-bulk

**Prompt.** Convert functional expectations into ML-style evaluation rather than code coverage. Define domain panels, DPO batches and causal comparisons that can reveal whether the combined deterministic+learned architecture is improving.

**Inputs.** R07-B…H are mandatory. R08 domain-law examples and R09 cost evidence are opportunistic inputs **only if observable at the current snapshot**; R07-I must not wait for R08/R09 completion and must record missing evidence as bounded handoff needs. The only hard sibling join for R07 synthesis is R01–R06 as defined in section 15.

**Required panels.**
- operational development panel;
- untouched certification/holdout panel;
- OOD/compositional panel;
- protected retention families;
- prequential/user-feedback panel.

**Required metrics.** domain mapping accuracy; task/semantic accuracy; path validity; association preservation; chosen/rejected discrimination; tie calibration; retrieval recall/precision only where meaningful; ordinary-consumer quality; latency; update time; peak memory; persistent bytes; retention/forgetting; abstention/unknown quality.

**Bulk comparison.** Compare A0 frozen reference, Aprev last accepted state and C candidate on the same compatible panel/version. Record one primary causal factor per comparison and 2×2 interaction only when a joint effect is the hypothesis.

**Good example.** Representation changes while retrieval/selector frozen; later retrieval law changes with representation frozen.

**Bad example.** Different panel and different tokenizer in every bulk, then call higher score learning.

**Edge cases.** confidence intervals overlap; evaluator drift; domain distribution shifts; one domain improves while protected domain regresses; candidate faster but below minimum quality.

**Verdicts.** IMPROVE / REGRESS / INCONCLUSIVE / INVALID_EVIDENCE. Astra semantic examination reviews evidence quality and architecture direction, but does not replace measured outcomes.

### R07-J PACKET — Integration, falsification, first-teaching seam and Astra review

**Prompt.** Integrate R07-A…I after the R01–R06 join. Choose the smallest causal path that can reach one real lesson quickly, while preserving the long-term four-level architecture. Do not optimize for documentation completeness; optimize for earliest falsifiable teaching loop.

**Required synthesis.**
1. one human-readable causal diagram from request to next request after accepted update;
2. one selected semantic recipient and explicit rejected alternatives;
3. one minimal missing implementation seam for R18;
4. one first domain/family and why it gives high information value;
5. one DPO pair construction recipe;
6. one domain representation/token route;
7. one retrieval/dynamic-k hypothesis if retrieval is on that path;
8. one ordinary consumer adoption proof requirement;
9. one R13 experiment with baseline/challenger;
10. one rollback/failure path;
11. one PC resource measurement plan;
12. one list of evidence that would make us abandon or redesign the path.

**Astra review requirement.** CODEX using Astra 6 ultracode performs a separate architecture-quality examination of the assembled evidence: wrong recipient, hidden deterministic shortcut, broken pair provenance, lossy tokenization, invalid kNN credit, non-adopted checkpoint, non-PC state growth, or experiment confounding. This is a semantic review of the evidence/plan, not a substitute for training outcomes.

**Good example.** Choose one domain where current code already has most of pair→update→ordinary-consumer chain; implement only the missing state-adoption seam, run one controlled lesson, and compare parent/child on independent tasks.

**Bad example.** Require all domains, all graph levels, Windows CI and perfect coverage before the first lesson.

**Stop rule.** R07-J may publish READY_FOR_R12_R13_R18 only if all twelve section-13 questions have evidence-backed answers or named bounded gaps that do not block the first lesson. Otherwise remain R07_REPAIR_REQUIRED.

## 15. R01–R06 JOIN GATE FOR R07

Before R07-J, record one receipt row per R01–R06:

Rxx -> checkpoint/blob -> accepted findings -> rejected/contradicted findings -> primary spans reopened by R07 -> effect on R07 decision -> unresolved owner.

Minimum expected contributions:
- R01: full source/slice/WorkLeft/RAG inventory and missed roots;
- R02: original requirements, authority, conflicts and DPO lifecycle semantics;
- R03: primary RAG lineage and decision-bearing sources;
- R04: domain/vector/schema/token/association behavior;
- R05: exact pair→objective→state→publication/consumer path;
- R06: BM25/kNN/dynamic retrieval, selector/solver and graph authority.

R07 must not simply concatenate these outputs. It must reconcile contradictions against primary sources and current code.

## 16. FUNCTIONAL EXPECTATION COVERAGE GATE — REPLACES COVERAGE-FIRST THINKING

A final R07 packet must contain a matrix with one row per functional expectation:

expectation -> domain(s) -> deterministic contract -> learned behavior -> DPO signal -> representation/token route -> dynamic algorithm -> ordinary consumer -> measurement -> falsifier -> owner/run.

Mandatory expectations:
1. correct domain mapping, including unknown/mixed domain;
2. typed many→many Unit results and association preservation;
3. Flow correction improves a relation without corrupting Unit semantics;
4. Orchestrator handles async combinations/budget/fallback;
5. DynamicGraph transfers/reuses higher-order relations where useful;
6. valid positive/negative/tie DPO construction and provenance;
7. domain-sensitive tokenization does not erase the learning distinction;
8. LoRA/learned state mutates the intended semantic recipient;
9. dynamic kNN/retrieval varies appropriately by domain evidence;
10. combined deterministic + learned path is more compact/effective than compared alternatives under declared constraints;
11. accepted state is used by the next ordinary user request;
12. continual user-feedback learning is bounded, versioned and rollback-safe on PC.

No row is accepted by test count. Each row needs a future or observed **behavioral examination** and a falsifier.

## 17. ANTI-NARROWING / ANTI-OVERTESTING RULES

R07 automatically fails and must be repaired again if an executor:
- looks only at S04 or only at one slice family;
- reads only sibling findings and not assigned primary WorkLeft/RAG sources;
- omits general prompts/task/goal files in the design-to-code directory;
- omits WorkLeft files because their names look historical;
- treats Windows Actions, green build, line coverage or test count as learning evidence;
- proposes tests for stochastic efficacy instead of task panels/bulk comparisons;
- changes several learning mechanisms in one experiment without a factorial/ablation design;
- calls k/tokenizer/vector/LoRA “best” without domain-specific measurements;
- loses the CODEX/Astra lesson-selector + judge role separation from the open Chinese pair-author role;
- declares checkpoint publication equivalent to ordinary adoption;
- requires finishing every architectural level before starting the first controlled teaching lesson.

## 18. PUBLICATION AND READBACK

After editing the five R07 carriers:
1. re-read the committed files from llm_plan3;
2. verify markers R07_REPAIR_REQUIRED, Astra 6 ultracode, open Chinese model, R01–R06 JOIN, domain representation passport, dynamic kNN, ordinary consumer, bulk-to-bulk, and NOT_MEASURED;
3. verify no statement claims learning gain, best algorithm, PC fit or complete R07 without evidence;
4. verify the selected first-lesson seam is smaller than a whole-slice rewrite;
5. record the publication commit SHA in RUN-LOG.


---

## 19. REVISION 3 — BINDING REPAIR ADDENDUM FOR PARALLEL EXECUTION AND EARLIEST TEACHING

This section is binding and overrides any older R07 wording that creates a global wait cycle, treats test/coverage completion as learning acceptance, or allows the hybrid architecture to be credited without causal comparison.

### 19.1 Snapshot-refresh protocol for R01–R20 simultaneous execution

R07 is executed while all R01–R20 may start independently and run simultaneously under PARALLEL20; R01–R06 remain the specific semantic sources for this R07 refresh. Therefore:

1. **START SNAPSHOT:** R07-A records the latest observable identity of R01–R06. Missing result = `RESULT_NOT_OBSERVED`, not WAIT.
2. **DIRECT WORK CONTINUES:** R07-A…I continue from direct planning/core primaries and current code. A missing sibling cannot substitute for a missing primary read and cannot globally pause R07.
3. **PRE-J REFRESH:** immediately before R07-J, refresh the latest observable R01–R06 identities once. Ingest only semantic deltas that can change a R07 decision.
4. **CLAIM-LOCAL BLOCKING:** if an unavailable sibling contains a fact that R07 cannot independently establish and that fact is essential to one decision, mark only that decision `DEPENDENCY_OPEN`.
5. **NO STATUS INFLATION:** a sibling checkpoint is not DONE evidence; a copied conclusion is not an independent read.
6. **LATE CORRECTION:** if R01–R06 later publishes evidence that contradicts R07, reopen only the affected decision and its descendants; do not restart A–J.

This protocol is intended to preserve parallel throughput and remove the old “everyone waits for everyone” failure mode.

### 19.2 Whole-program inventory gate — proof against S04-only narrowing

R07-A must create an inventory **before relevance filtering**. The inventory must include, by concrete path/name or explicit unresolved pattern:

- every S00–S10 slice/package/version family and extras;
- every `WorkLeft*`, `workLeft*`, `LeftToWork*` family discovered in the design-to-code root and referenced roots;
- general `task*`, `goal*`, `original*`, `prompt*`, decision and algorithm files;
- every RAG root/index/NDJSON and every referenced repo/root;
- external product/code roots named by the original program;
- current core code families for Unit / Flow / Orchestrator / DynamicGraph / DPO / LoRA / tokenization / vectors / retrieval / domain / feedback / ordinary consumers.

Only after inventory may a row become RELEVANT / DEFERRED / HISTORICAL / DUPLICATE. Every non-relevant classification requires a reason and the requirement/decision it cannot affect. “Did not look relevant from filename” is invalid.

The coverage receipt must expose:
`inventory_count -> direct_read_count -> handoff_only_count -> deferred_count -> unresolved_count`.
Counts are navigation evidence only; consequential claims still require semantic reads.

### 19.3 Baseline domain-law matrix

R07-C and R07-I must explicitly account for the currently planned baseline families:

- EN↔RU / language transformation;
- orchestration / multi-step coordination;
- Figma/design → HTML/CSS;
- C#;
- Angular;
- SQL;
- DPO Builder / preference construction;
- every additional domain discovered by R01/R03/R08.

For every family, the work order must identify a **real-world or formal correctness law** before proposing tokens, vectors, kNN or DPO negatives. Examples:
- SQL NULL/cardinality/order semantics;
- code caller→callee and type/dependency direction;
- UI layout hierarchy, spatial constraints and visual correspondence;
- orchestration dependency/order/cancellation/resource constraints;
- translation semantic preservation rather than lexical similarity alone.

If no authoritative law/source is available, the passport remains `DOMAIN_LAW_UNRESOLVED`; the executor may design the experiment but may not declare the representation correct.

### 19.4 DPO construction algorithm, not merely pair storage

R07-E must produce a model-ready **pair-generation and admission recipe** for each selected domain:

`weakness evidence -> CODEX/Astra lesson specification -> pair-generation prompt to external open Chinese model -> two independently authored candidate sides -> immutable pair seal -> CODEX/Astra exact-pair judgment -> admission/quarantine -> Student update -> independent post-update evaluation`.

Binding role separation:
- **CODEX + Astra 6 ultracode performs the human role twice:** (1) chooses what XIIGEN should learn next; (2) evaluates the quality/validity of the exact immutable pair.
- **The external open Chinese model authors BOTH chosen and rejected candidate texts.**
- CODEX/Astra must not author or rewrite either training side.
- XIIGEN Student output may be evidence/context but is not automatically rejected.
- Pair judge verdict and Student improvement verdict are separate.

Each pair recipe must define:
- task/context boundary and immutable hashes;
- material difference target;
- forbidden trivial negatives;
- domain-law violation or solution-path distinction being taught;
- TIE / BOTH_BAD / UNJUDGEABLE;
- duplicate/near-duplicate detection;
- no-repeat identity;
- pair quality rubric;
- post-update usefulness measurement.

### 19.5 Dynamic kNN must be a domain-conditioned function family

R07-D may not stop at “try dynamic k”. It must specify a **candidate function family** and a fitting/examination protocol.

At minimum evaluate whether the domain-conditioned policy should depend on:
`domain confidence, local density, neighbor distance distribution, law-compatibility, query entropy, duplicate-family ratio, OOD score, latency budget, memory budget`.

Outputs may include:
`k, radius, feature-weight vector, BM25/vector blend, exact-vs-ANN mode, abstain`.

The task must compare at least:
- fixed-k baseline;
- simple deterministic adaptive rule;
- learned/adaptive rule if a real trainable recipient exists.

The “best” policy is selected only from domain bulk evidence with the same corpus, representation and downstream consumer frozen. First-neighbor correctness is not the objective; the objective is end-to-end valid solution/path quality under cost.

### 19.6 Vectorization / LoRA crosswalk

R07-F must create an explicit crosswalk:

`domain law -> semantic features -> canonical representation -> retrieval vector -> training token IDs/mask -> model hidden state -> LoRA/parameter delta -> checkpoint/version -> ordinary consumer behavior`.

The work order must detect and reject category errors:
- retrieval vector ≠ training token;
- training token ≠ LoRA delta;
- LoRA delta ≠ graph relation state;
- equal tensor/vector length ≠ semantic compatibility.

If a proposed “transpose to vectorized token” has no defined reversible/meaning-preserving mapping to the actual objective and consumer, it is a design gap for R12/R18, not an implementation detail to guess later.

### 19.7 Hybrid algorithm credit — deterministic-only vs learned-only vs hybrid

R07-G/I/J must explicitly test the user's central hypothesis that the combined deterministic + learned system is **more compact and effective**.

For the selected first domain/path, define comparable arms where technically meaningful:

- **D:** deterministic-only admissibility/rules with learned authority disabled;
- **L:** learned-only or minimally constrained learned path, only where safe/meaningful;
- **H:** hybrid deterministic admissibility + learned selection/correction;
- optionally **H+G:** hybrid plus higher-order DynamicGraph composition when Graph is actually on the path.

Use the same panel, corpus, representation version, ordinary consumer and resource profile. Measure:
- task/semantic accuracy;
- domain mapping;
- valid-path/association rate;
- abstention/OOD behavior;
- inference latency;
- update time;
- peak RAM/VRAM;
- persistent bytes;
- retention/forgetting.

R07 may recommend H only if evidence later shows a meaningful quality/cost frontier. Before measurement the verdict is `HYPOTHESIS`, never “hybrid is better”.

### 19.8 Learning acceptance — no coverage-first tasks

For R07 and its downstream learning experiment:
- **no line/branch/test-coverage target;**
- **no Windows GitHub Action prerequisite;**
- **no “N tests green” acceptance criterion for learning efficacy.**

Deterministic invariants such as serialization/type/state-version integrity may be checked by the smallest local deterministic assertion/check **only when needed to trust the experiment apparatus**. These checks are infrastructure evidence and must never be counted as learning improvement or become a multi-run gate.

The principal acceptance unit is:
`functional expectation -> behavioral panel -> DPO/bulk intervention -> ordinary-consumer outcome -> time/resource measurement -> falsifier`.

### 19.9 FIRST-TEACHING-READY gate

R07-J must optimize for starting real teaching as early as safely possible.

The first teaching loop is READY only when these minimum facts are known:
1. one domain/family and weakness;
2. one actual semantic recipient/state owner;
3. one ordinary consumer of the accepted state;
4. one lossless-enough representation/token route for the taught distinction;
5. one exact DPO pair recipe with correct role separation;
6. one accept/rollback/version path;
7. one parent-vs-child behavioral panel with independent unseen examples;
8. one PC resource measurement envelope;
9. no unresolved gap that can silently redirect the update to the wrong state or consumer.

The following are explicitly **not prerequisites** for the first lesson:
- completion of every slice;
- completion of every domain;
- full DynamicGraph integration when not on the selected path;
- Windows CI;
- broad coverage;
- perfect documentation;
- final choice of the best tokenizer/kNN function for all domains.

If the minimum facts exist, R07-J must hand off:
`FIRST_TEACHING_READY -> R18 minimal seam -> R13 controlled lesson`.
If not, it must name the **single smallest missing blocker** and not expand it into a whole-slice rewrite.

### 19.10 Model-complete packet enforcement for A–J

In addition to the common envelope, every child packet must visibly repeat or inherit by exact section reference all of:
`CURRENT TASK, NOT THE TASK, EVIDENCE-ONLY, required_skills_and_gates, context_files_to_read, implementation_or_artifact_details, original_task_requirements_to_cover, mental_simulation_required, good_examples, bad_examples, edge_cases, allowed_files, forbidden_files, allowed_commands, forbidden_commands, verification_required, return_packet_schema, claim_scope/evidence_scope/independent_evidence_count/downgrade_needed`.

A child cannot close its packet with a prose summary alone. It must return the requested evidence rows and explicitly mark:
`DIRECT_PRIMARY_READ / DIRECT_CODE_TRACE / SIBLING_HANDOFF / HYPOTHESIS / NOT_MEASURED`.

### 19.11 Revised R07 final statuses

Use only:
- `R07_REPAIR_REQUIRED` — governing packet still incomplete or a blocker prevents a valid first-teaching decision;
- `R07_RESEARCH_COMPLETE_FIRST_TEACHING_BLOCKED` — architecture research is sufficiently complete but one named implementation/recipient blocker prevents the first lesson;
- `R07_READY_FOR_R12_R13_R18_FIRST_TEACHING` — evidence is sufficient to select recipient, minimal seam and controlled experiment;
- `R07_LEARNING_GAIN_MEASURED` is **forbidden inside R07** unless an actual authorized learning experiment has run and its raw evidence is linked.

Revision 3 itself is a planning repair, not evidence that A–J have executed.
