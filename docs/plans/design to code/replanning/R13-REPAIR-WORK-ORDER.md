# R13-REPAIR-WORK-ORDER — causal experiment compiler for hybrid XIIGEN learning

**Date:** 2026-10-13  
**Status:** GOVERNING R13 PROMPT. This work order supersedes the earlier architecture-neutral R13 protocol wherever they conflict.  
**Execution mode:** analysis + experiment design + planning only. No training, paid/external model calls, product code changes, datasets/weights/index/graph mutation, coverage campaign, Windows/GitHub Actions, build or CI are authorized by this task.

## 0. Why R13 must be repaired

The first R13 produced a useful generic learning-examination protocol, but it is not sufficient as the model task that the replanning program needs.

It did not force a semantic join of the latest R01–R06 results, did not directly reopen the primary WorkLeft/RAG/original-prompt lineage for consequential experiment choices, and left the first experiment too architecture-neutral. As a result a later worker could again narrow the problem to the nearest DPO seam, treat E1–E4 as abstract categories, or postpone domain/vector/kNN/LoRA/Flow/Orchestrator/DynamicGraph questions instead of converting them into executable learning experiments.

This repair changes the role of R13:

> **R13 is the causal experiment compiler.** It receives the recovered whole-program requirements and current mechanism evidence, selects the smallest informative intervention that reaches an ordinary consumer, and emits model-ready implementation/examination packets whose success is judged by learning behavior, time, retention and compactness — not test coverage.

The historical files in `results/R13/` remain evidence. Their “PROTOCOL COMPLETE” status is no longer enough to close current R13. Current status is **R13_REPAIR_REQUIRED / LEARNING_NOT_MEASURED** until this work order is executed and its acceptance conditions are met.

## 1. Role and mandatory skill stack

Act as:
- trainable-systems architect;
- experiment designer;
- planning architect;
- plan reviewer;
- code-trace skeptic;
- prompt engineer;
- human-communication architect.

Read the applicable current sections of these skills from `lubochka/llm_mvp_core@codex/dpo-implementation/docs/skills`:
- `trainable-architecture-decision-SKILL.md`
- `phase-planning-SKILL.md`
- `plan-review-SKILL.md`
- `generated-code-review-SKILL.md`
- `code-examination-SKILL.md`
- `prompt-patch-authoring-SKILL.md`
- `human-readable-architecture-audit-SKILL.md`
- `output-readability-gate-SKILL.md`
- `experiment-design-SKILL.md`
- `improvement-measurement-SKILL.md`
- `authority-requirement-binding-SKILL.md`
- `reference-inventory-and-source-map-SKILL.md`

If a skill was renamed, resolve the current filename from the tree and record the exact replacement. Do not claim a skill was read if it was not.

## 2. Non-negotiable project contract

### 2.1 Hybrid architecture

Preserve these four functional roles and map them to actual current code/contracts rather than inventing duplicate interfaces:

1. **ILearningUnit** — domain + input-vector/input-schema + output-schema -> typed many-to-many candidate/result relation.
2. **ILearningFlow** — relations/corrections between Units; transforms one unit’s outputs into lawful inputs/choices for the next.
3. **ILearningOrchestrator** — asynchronous dependency/path/branch composition; accounts for all launched work, cancellation and merge decisions.
4. **ILearningDynamicGraph** — higher-order relations between patterns/compositions and transfer to unseen lawful combinations.

Training ownership remains with the actual `ILearningMachine`/current training owner where primary contracts say so. Do not create four trainers merely because four runtime roles exist.

Deterministic mechanisms are not legacy noise. They provide laws, admissibility, exact computation, provenance, safety and bounded search. Learned mechanisms choose/rank/adapt within lawful space. A correct deterministic answer is not rejected merely because it is deterministic.

### 2.2 DPO role split

For a real DPO lesson:
1. **CODEX using GPT-6 Astra UltraCode performs the human lesson/case selection.**
2. **The external open Chinese model authors BOTH final DPO sides: chosen and rejected.**
3. **CODEX using GPT-6 Astra UltraCode separately judges the exact final pair** after authorship.
4. Student output may be context but is not automatically the final rejected side.
5. Pair authorship, preference selection, exact-pair quality, optimizer admission, runtime arbiter and post-update evaluation are different roles.
6. `TIE`, `BOTH_BAD`, `UNJUDGEABLE` are not forced into preference labels.
7. If bytes/meaning passed to the optimizer differ materially from what was assessed, the learning-efficacy evidence is invalid until re-authored/reassessed.

### 2.3 What counts as progress

The progress unit is:

`new lawful lesson/intervention -> bounded state change -> ordinary consumer -> same-panel observation -> retention + time + full PC footprint -> decision`

Green tests, coverage, Windows runs, changed hashes, nonzero gradient, lower loss, larger index, successful serialization or checkpoint creation do **not** prove learning improvement.

Do not create a new test-coverage project. Deterministic checks may still protect a deterministic invariant such as exact-pair identity, state lineage, rollback, one-application/no-repeat, serialization/shape, permissions or actual native boundary. Such checks are mechanism evidence only.

## 3. Mandatory context — no S04-only narrowing

R13 must consume the whole-program evidence needed for its experiment choices.

### 3.1 Current replanning carriers
Read:
- `COMMON-CONTRACT.md`
- `PLAN-25-RUNS.md`
- `START-NEXT-RUN.md`
- `PARALLEL-R01-R03-JOIN.md`
- `PARALLEL9-MODEL-PACKETS.md`
- `previous rounds thinking results.md` — targeted sections relevant to R13, WorkLeft/RAG, DPO, algorithms and prior failed/narrow passes
- `LEARNING-EXAMINATION-CONTRACT.md`
- `LEARNING-EVALUATION-CONTRACT.md`
- `DOMAIN-EXPERIMENT-PLAYBOOK.md`
- `DOMAIN-EXPERIMENT-RECIPES.md`
- `HYBRID-ALGORITHMS-AND-RUNS.md`
- `R12-REPAIR-WORK-ORDER.md`
- `R14-REPAIR-WORK-ORDER.md`

### 3.2 Mandatory latest R01–R06 semantic join

Read the latest observable `SOURCES.csv`, `FINDINGS.md`, `DECISIONS.md`, `CHANGE-MAP.csv`, and relevant `RUN-LOG.md` sections for **R01, R02, R03, R04, R05, R06**.

Do not merely cite them. Build a semantic receipt table:

`upstream finding/decision -> experiment consequence -> changed R13 requirement -> consumer`

At minimum R13 must consume these already-published facts/constraints:

- **R01:** whole-program surfaces exceed one slice; LearningMachine owns training while runtime uses logical orchestrator/flow/unit; “answer changed” is not learning improvement; discovered domains and graph/orchestrator surfaces cannot be dropped.
- **R02:** algorithm-level efficiency, domain-aware kNN, relation/path attribution, 95%-goal denominator, bulk-to-bulk time/accuracy, retention, PC compactness and whole-system cost must remain separate measurable requirements.
- **R03:** domain-scoped positive/negative/warning memory, one-new-pair full-state accept/rollback, whole-PC compactness, domain-law chains and the distinction between judge preference score and actual DPO objective are primary constraints.
- **R04:** domain/vector/token/index coordinate semantics matter; same shape is not semantic compatibility; current `Compact`/truncation can destroy pair meaning; warnings are not automatically abstentions; Unit/Flow/Graph claims need causal separation.
- **R05:** current DPO/update/LoRA path has real code seams but gain is NOT_MEASURED; exact-pair preservation, objective, state mutation and ordinary consumer must be separated; one-pair/no-repeat cannot be replaced by hidden replay; “code ready” and “empirical gain measured” are different states.
- **R06:** dynamic-k must affect an actual neighbor consumer; BM25, vector kNN and local/global fusion are distinct; memory change and parameter change need matched controls; many-to-many associations/semantic ordering must survive Flow/Orchestrator; DynamicGraph transfer requires unseen composition; zero initial DPO margin does not imply zero gradient; full PC cost includes all branches/state; exact assessed pair and response-conditional objective must survive into the real training recipient.

If a newer R01–R06 checkpoint changes any of these, consume the delta rather than restarting the full program.

### 3.3 Primary sources and RAG/WorkLeft lineage

R01–R06 summaries do not replace primary meaning for a consequential experiment choice.

Trace the applicable source route through:
- original task / task prompt / goal files;
- all relevant `*WorkLeft*` / `LeftToWork*` families;
- current RAG usage guide and RAG entries that point to the DPO, dynamic-flow, domain, evaluation or learning-mechanism primary sources;
- original DPO algorithm/build plan;
- dynamic-flow decisions covering BM25+kNN, domain/arbiter, semantic compression, LoRA/projection, positive/negative/warning memory, no-repeat/replay and graph relations;
- the slice/queue/human-map entries for the selected capability;
- current core code only where producer -> state mutation -> ordinary consumer identity matters.

A RAG row, filename, README or historical “DONE” does not prove the runtime path. Reopen the exact primary span/code where the experiment depends on it.

## 4. CURRENT TASK

Produce the **first executable causal learning experiment packet** and the **next algorithm-interaction queue**.

R13 must answer all of these:

1. What exact ability are we trying to improve first?
2. Which deterministic and learned algorithms participate in that ability?
3. What is the actual learned owner/state/signal?
4. What ordinary consumer will use the changed state?
5. What single changed factor produces the first informative contrast?
6. What lawful domain/task families expose success and failure?
7. What DPO material, domain representation, dynamic-kNN or relation signal is needed?
8. What is frozen?
9. What outcome would falsify the hypothesis?
10. What resource/retention cost would make an otherwise higher score unacceptable?
11. If the first experiment is negative or inconclusive, what exact next branch follows without restarting planning?
12. Which later experiments establish Unit -> Flow -> Orchestrator -> DynamicGraph learning and compact PC continual learning?

Do not stop at “E1/E2/E3/E4 exist.” Fill at least one real first-experiment card from current evidence and produce bounded cards for the next interactions.

## 5. R13-A — compile the evidence into an experiment dependency map

Create a table with rows for:
- domain mapper;
- canonicalizer/tokenizer/codebook;
- vector/projection/LoRA;
- exact/vector/BM25 retrieval;
- domain-aware metric / learned reranker / dynamic-k policy;
- positive/negative/warning memory;
- DPO pair authoring/assessment/admission;
- DPO objective/reference/update;
- ILearningUnit;
- ILearningFlow;
- ILearningOrchestrator;
- ILearningDynamicGraph;
- runtime arbiter;
- ordinary consumer;
- user-feedback admission;
- persistence/rollback.

Columns:
`source/requirement | current deterministic role | learned owner/state | signal/objective | actual producer | actual consumer | known gap | first falsifiable contrast | resource cost | upstream evidence IDs`.

Mark each row:
- `TRACED`
- `PARTIAL`
- `NOT_TRACED`
- `NOT_MEASURED`

Never turn `NOT_TRACED` into a new implementation by assumption.

### Acceptance R13-A
The map must make it impossible to attribute a gain to “AI” or “DPO” generically. Each planned gain has one changed factor and one named consumer.

## 6. R13-B — select the first learning observation

Rank candidates using:

`information value / implementation + teacher + judge + evaluation + reindex + rollback cost`

subject to:
- causal identifiability;
- reuse of current code;
- lawful data availability;
- ordinary-consumer observability;
- PC feasibility;
- no hidden prerequisite explosion.

Candidate families:

### E1 — exact DPO pair -> update -> ordinary consumer
Use when the first missing seam is exact-pair/objective/adoption and can be bounded.

Required controls:
- frozen parent;
- exactly one new admitted pair;
- no-update control with matched memory/context;
- exact final chosen/rejected identity before and after projection;
- same evaluation panel;
- protected-domain retention;
- ordinary consumer actually reads accepted state.

### E2 — domain -> representation -> dynamic kNN -> solver
Use when retrieval/routing is the highest-value uncertainty.

Keep the eligible corpus and downstream solver fixed. Change **one** of:
- domain mapping;
- representation/projection;
- metric/reranker;
- k policy.

Do not change tokenizer + corpus + k + metric + reranker + solver in one comparison.

Required diagnostics:
- oracle domain vs predicted domain;
- exact-search reference vs ANN candidate path where applicable;
- first-hit/static current behavior;
- learned/domain-aware candidate use;
- no-update matched control;
- positive/negative/warning memory reason trace.

### E3 — Flow/Orchestrator connection learning
Freeze Unit solver behavior and change only relation/path/branch policy.

Measure:
- valid path/composition;
- end-task quality;
- all launched branch cost, not winner latency only;
- dependency order;
- cancellation/late-result behavior;
- ordinary adoption.

### E4 — DynamicGraph higher-order transfer
Freeze lower-level primitives as far as the hypothesis requires. Train/adapt on permitted lower-order evidence and evaluate:
- known primitive;
- known pairwise relation;
- unseen higher-order composition;
- misleading near neighbor;
- lawful rare path.

A repeated known route does not prove graph transfer.

### Mandatory selection output
Emit:
1. ranked candidates;
2. rejected candidates with reason;
3. selected first experiment;
4. smallest missing engineering seam, if any;
5. what can be prepared in parallel without waiting for that seam.

## 7. R13-C — DPO packet quality

For any DPO-based experiment produce a **pair passport** before update:

- immutable lesson/case ID;
- domain + task family;
- frozen context;
- lawful constraints;
- why this lesson is informative;
- Chinese-model final chosen;
- Chinese-model final rejected;
- Astra UltraCode human-selection receipt;
- Astra UltraCode exact-pair assessment receipt;
- assessment outcome;
- bytes/semantic identity after canonicalization/tokenization/projection;
- optimizer-side chosen/rejected identity;
- reference/policy state identity;
- response boundary/mask/objective identity;
- admission result;
- update/rollback identity.

### Good negative
A plausible answer/path that violates an important domain law or chooses a subtly worse lawful path.

### Bad negative
Nonsense, formatting garbage, or an answer made trivially worse so the pair teaches no useful boundary.

### Invalid evidence
The meaningful distinction exists in the assessed pair but disappears after whitespace compaction, truncation, tokenization, projection or incorrect response mask.

## 8. R13-D — domain laws, tokens/vectors and dynamic kNN

For every domain used in the first two experiment stages create a compact **domain passport**:

- domain / subdomain;
- hard laws/invariants;
- ambiguous or multi-correct cases;
- unknown/abstain case;
- near-miss family;
- meaningful raw distinctions;
- canonicalization losses to avoid;
- tokens/features that must preserve meaning;
- vector/projection hypothesis;
- eligible neighbor universe;
- positive/negative/warning examples;
- fixed baseline metric/k;
- one domain-aware metric/reranker/k hypothesis;
- expected consumer behavior;
- end-task rubric.

Required counterexamples include where applicable:
- negation;
- order/direction;
- significant whitespace or code;
- identifier/case/symbol sensitivity;
- same lexical surface / different meaning;
- different surface / same meaning;
- cross-domain near neighbor;
- OOV/new composition;
- empty compatible pool;
- stale index/coordinate version.

Do **not** claim a universal “best k.” The goal is a dynamic function/policy appropriate to the domain and consumer, learned or adapted from evidence while deterministic law filters remain authoritative.

## 9. R13-E — LoRA/projection examination

Do not equate LoRA with tokenization.

If LoRA or another learned projection is part of the candidate, bind:
- source representation;
- target representation;
- frozen base;
- trainable parameter group/matrix;
- objective;
- gradient/forward path;
- update owner;
- version/rollback unit;
- index/re-encoding effect;
- actual downstream consumer;
- semantic distinction the adaptation should improve.

Compare:
1. no adaptation;
2. faithful fixed representation;
3. one permitted adaptation.

Evaluate downstream domain/path/answer behavior plus full reindex/update cost, not adapter norm or loss alone.

## 10. R13-F — learning the connection between deterministic and nondeterministic algorithms

Create causal contrasts for the connection itself.

### Unit
Can the unit produce/rank lawful typed many-to-many results better after a bounded lesson while preserving alternatives?

### Flow
With Units frozen, does learned relation/correction move the right result into the right next input/recipient?

### Orchestrator
With Units and legal dependency graph fixed, does adaptive path/branch choice improve lawful quality per total compute/time?

### DynamicGraph
With lower-order primitives fixed, does learned higher-order relation improve unseen composition rather than memorize known paths?

For each:
`baseline | one changed state/policy | same panel | ordinary consumer | falsifier | retention | full cost`.

A deterministic candidate can win. Learned scoring does not have authority to violate domain law.

## 11. R13-G — bulk-to-bulk continual-learning protocol

A bulk is an observation window of sequential **new admitted lessons**. It is not replay of the same pair, renamed duplicates or repeated paraphrases counted as independent evidence.

For bulk `B_n` report:
- admitted/rejected/quarantined/rolled-back lessons;
- independent source/task families;
- per-domain quality;
- worst-domain quality;
- domain mapping result;
- hard-law violation rate;
- end-task quality;
- path/retrieval diagnostics where relevant;
- retention vs `B_0` and `B_(n-1)`;
- cumulative update time;
- teacher/judge/evaluation time;
- inference p50/p95;
- peak RAM/VRAM;
- persistent model/adapter/index/graph/cache/snapshot growth;
- failed attempt cost;
- ordinary-consumer adoption rate.

Compare incumbent and challenger on the **same panel**. If a fresh panel is introduced, run both on it.

Do not fit thresholds after seeing the challenger.

## 12. R13-H — decision rules

Every empirical card must predeclare:
- primary behavioral metric;
- hard-law constraints;
- retention constraint;
- meaningful-effect or non-inferiority rule;
- resource ceiling;
- uncertainty treatment;
- stop/rollback condition.

Allowed outcomes:
- `IMPROVE`
- `REGRESS`
- `INCONCLUSIVE`
- `INVALID_EVIDENCE`
- `NOT_MEASURED` before execution

### INVALID_EVIDENCE examples
- different panel/rubric/budget for baseline and challenger;
- leaked evaluation family;
- pair changed after assessment;
- hidden replay;
- multiple uncontrolled factors;
- ordinary consumer stayed on old state;
- near-duplicate families counted as independent;
- state/version lineage missing.

### INCONCLUSIVE examples
- effect smaller than resolution;
- too few independent families;
- evaluator disagreement unresolved;
- both states valid but no meaningful difference.

A valid negative experiment is valuable. Do not rewrite the rubric merely because the result was REGRESS or INCONCLUSIVE.

## 13. Model-ready first experiment card — required schema

R13 cannot complete without one populated card using current evidence.

```
experiment_id:
capability:
domain:
independent_families:
user_visible_goal:
source_requirements:
upstream_receipts_R01_R06:
primary_sources_reopened:
actual_code_owner:
ordinary_consumer:
deterministic_guards:
learned_owner:
learned_state:
learning_signal:
baseline_state:
challenger_intervention:
no_update_control:
frozen_factors:
pair_or_signal_lineage:
representation_version:
index_version:
objective_version:
evaluation_panel:
primary_metric:
hard_violations:
retention_panel:
resource_budget:
hypothesis:
falsifier:
IMPROVE_rule:
REGRESS_rule:
INCONCLUSIVE_rule:
INVALID_EVIDENCE_rules:
rollback:
smallest_missing_engineering_seam:
parallel_preparation:
next_if_improve:
next_if_regress:
next_if_inconclusive:
next_if_invalid:
status:
```

If a field is unknown, write the exact missing source/code trace and owner. Do not fill with a guess.

## 14. Model-ready next-experiment queue

After the first card, emit bounded cards for:
- second structurally different domain;
- domain mapping vs solver;
- representation/vector semantic change;
- dynamic-kNN/reranker change;
- memory-fixed vs parameter-update;
- LoRA/projection if causally relevant;
- Flow relation;
- Orchestrator path;
- DynamicGraph composition;
- compact PC continual-learning + feedback.

Each card must say what prior observation unlocks it. Do not turn this into a fixed 90-bulk tunnel.

## 15. Subtask division when R13 is too large

Use children only for bounded independent evidence questions. Parent remains responsible for integration.

### R13-A1 — R01–R06 semantic receipts
**Prompt:** consume latest six result sets and produce only consequential receipts for R13.  
**Context:** result carriers + current R13 repair.  
**Output:** receipt table, conflicts, primary spans requiring reopen.  
**Good:** “R04 truncation finding changes pair-integrity invalidation rule.”  
**Bad:** six summaries with no experiment consequence.  
**Edges:** sibling updated while reading; same claim with different snapshot.  
**Return only:** no shared-file mutation.

### R13-A2 — DPO/update/ordinary consumer
Trace exact pair -> preprocessing -> objective/reference -> update -> publication -> ordinary consumer.  
Do not redesign the trainer. Return the first missing causal seam and exact pair-integrity hazards.

### R13-A3 — domain/representation/dynamic-kNN
Compile one selected domain passport and one E2 candidate using real current retrieval components.  
Separate exact/BM25/vector/ANN, k budget, metric, reranker and arbiter.

### R13-A4 — Flow/Orchestrator
Trace one actual many-to-many composition path and design frozen-Unit relation/path contrasts including async cost/cancellation.

### R13-A5 — DynamicGraph transfer
Trace current graph recipient/state and design one unseen-composition experiment. Do not claim graph learning from route-ID lookup or `IsLearned`.

### R13-B — integration
Parent merges A1–A5, chooses the first card by information value/cost, and creates the branching queue. Child agreement is not independent expert review.

Each child packet must contain CURRENT TASK, NOT THE TASK, exact refs/skills, examples, edge cases, output schema, acceptance, and a call/work budget.

## 16. Good examples

### Good: DPO
The SQL rejected answer uses a plausible JOIN that changes cardinality. Exact chosen/rejected meaning survives preprocessing. One update is compared with a no-update control on held-out SQL families and a protected old-domain panel. Ordinary SQL documentation consumer reads the accepted state.

### Good: domain/vector/kNN
The panel contains “must delete” vs “must not delete,” cross-domain lexical near-neighbors and an unknown request. Same solver and corpus are used. First-hit baseline is compared with one domain-aware reranking change; exact-search diagnostic shows whether ANN candidate loss confounds the result.

### Good: Flow
Unit outputs are frozen. Only learned association/correction state changes. The learned Flow chooses the lawful next recipient on unseen family examples while total path cost is measured.

### Good: Orchestrator
All launched branches, cancellations and late results are charged. The learned path wins only if end-task quality stays lawful and total cost/latency improves under the declared budget.

### Good: DynamicGraph
Known primitives and lower-order relations are reused to solve an unseen lawful higher-order composition. The solver itself is frozen enough to attribute the gain to composition/path learning.

## 17. Bad examples

- “R13 protocol exists, so R13 is done.”
- “DPO loss decreased, therefore Student improved.”
- “Weights changed, therefore learning happened.”
- “The nearest code seam is DPO, therefore E1 is automatically first.”
- “k=8 worked once, therefore 8 is the best k for every domain.”
- “Vector widths match, therefore old and new indexes are compatible.”
- “Warning returned with an answer, therefore the system abstained.”
- “We retrained Units and routing together, so Flow improved.”
- “The known graph route repeated correctly, so DynamicGraph transfers.”
- “Adapter is 2 MB, therefore the whole system is PC-compact.”
- “Windows run passed, therefore learning works.”
- “Coverage improved, therefore the nondeterministic system is correct.”
- “R01–R06 are cited by filename, therefore their findings were consumed.”

## 18. Edge cases

Must explicitly cover as applicable:
- both DPO sides invalid;
- TIE/BOTH_BAD/UNJUDGEABLE;
- exact pair becomes semantically identical after compaction/truncation;
- significant whitespace/code;
- response mask empty or includes prompt incorrectly;
- initial DPO margin = 0;
- deterministic lawful answer conflicts with learned rank;
- mixed/new/unknown domain;
- multi-correct answer;
- no compatible neighbors;
- ANN set differs from exact search;
- dynamic k collapses to minimum/maximum;
- warning memory points to superficially similar but wrong domain;
- stale vector/index coordinate version;
- LoRA update requires re-embedding/reindex;
- ordinary consumer remains on parent state;
- update outcome unknown after interruption;
- rollback partially published;
- one domain improves while another collapses;
- user feedback is wrong/contradictory;
- async branch returns after cancellation;
- two lawful paths have equal quality but different total cost;
- graph growth or index growth destroys PC compactness;
- all first lessons are rejected: low latency must not be called learning success.

## 19. Functional examination — no coverage-first acceptance

R13 is a planning/design task, so its current acceptance is documentary/semantic:
- every consequential claim has source/evidence lineage;
- current R01–R06 receipts are explicit;
- selected first experiment has an actual owner/state/consumer or a single exact missing seam;
- deterministic and learned responsibilities are explicit;
- one changed factor is named;
- DPO pair validity is separated from Student utility;
- domain mapping is separated from solver quality;
- representation is separated from LoRA and from index identity;
- dynamic-kNN is tied to actual consumer behavior;
- Unit/Flow/Orchestrator/DynamicGraph each has its own falsifiable future experiment;
- bulk means new lessons;
- full PC cost and retention are mandatory;
- outcomes include honest negative/inconclusive/invalid states;
- no test coverage/Windows/CI is used as learning-efficacy evidence.

Future empirical validation is performed by executing the resulting experiment packet, not by adding more software tests.

## 20. Required R13 outputs

Use the existing five-carrier result format under `results/R13/`:

- `SOURCES.csv` — exact current sources/blobs/spans and R01–R06 receipts;
- `FINDINGS.md` — repaired dependency map, selected first experiment, next queue and limits;
- `DECISIONS.md` — binding experiment decisions and rejected alternatives;
- `CHANGE-MAP.csv` — downstream receipts to R14/R15/R17/R18/R21/R22/R23;
- `RUN-LOG.md` — what was read, exact checkpoints, children/calls, unresolved inputs, and publication readback.

Preserve historical useful material but correct the status. Do not erase prior evidence.

## 21. Downstream contract

### To R14
Provide the ranked first capability and branch conditions, not merely E1–E4 labels.

### To R15
Provide the exact candidate packet for genuinely independent Astra Ultra review. Self-review is not R15.

### To R17
Provide the learning-efficacy clauses that replace coverage/Windows proxies while preserving deterministic mechanism guards.

### To R18
Provide:
- exact smallest implementation seam;
- repo/ref/file/class/method where known;
- current -> target behavior;
- allowed scope;
- paired functional examination prompt from section 13.

### To R21
Provide domain-passport gaps, lawful examples/negatives and protected families.

### To R22
Provide executable follow-up experiment cards; R22 expands and runs the interaction matrix, it does not invent the first causal protocol from zero.

### To R23
Provide resource instrumentation fields and full-system PC budget components.

## 22. Stop condition

R13 repair is complete only when:
1. latest R01–R06 findings are semantically consumed;
2. applicable primary RAG/WorkLeft/original sources are reopened for consequential choices;
3. one first experiment card is populated from actual evidence;
4. a smallest engineering seam is named if execution is not yet possible;
5. the next interaction queue covers domain mapping, representation, dynamic-kNN, DPO, LoRA/projection, Flow, Orchestrator and DynamicGraph without making all of them prerequisites to the first lesson;
6. every experiment has a lawful behavioral metric, falsifier, retention and full-cost boundary;
7. no learning claim relies on test coverage, Windows, CI, loss or hash alone;
8. outputs are published and read back from `llm_plan3`.

Until then, status remains **R13_REPAIR_REQUIRED**.
