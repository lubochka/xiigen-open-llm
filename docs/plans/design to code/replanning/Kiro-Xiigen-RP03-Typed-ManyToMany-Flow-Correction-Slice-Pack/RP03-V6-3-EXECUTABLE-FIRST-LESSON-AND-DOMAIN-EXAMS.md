# RP03 V6.3 — FIRST REAL FLOW LESSON, typed many→many, domain examinations

**ACTIVE BINDING CONTRACT · 14 October 2026.** Use with tasks/v6/RP03.* (the original 15 IDs are preserved). Supersedes earlier V1–V6/V6.2 execution overlays when they disagree. Publication is PLANNING ONLY: product source, trained weights, independent exams, external providers and reviewers are NOT_RUN / NOT_MEASURED / NOT_PERFORMED.

## 0. One outcome instead of another planning loop

Produce the smallest real RP03 mechanism that converts a request into many associated typed outputs, ranks F0-lawful corrections with an actual small trainable F1 Flow policy, updates real numeric parameters from observed preference, saves/loads them in a fresh process, calls the next L2 Unit and ordinary Host, and measures user-task improvement. Start the first REAL Flow lesson before optional Student DPO, all three domain CERT panels, RP02 full kNN ablation, RP04 scheduler and Windows CI.

NOT THIS TASK: create a new 25-round process; rewrite all L2 interfaces; treat a GraphNodeCandidate ID, handoff, IsLearned flag or storage-only receipt as training; maximize test count; force S04 Windows Actions for nondeterministic quality; call unapproved models.

## 1. What live interfaces imply — re-read latest code

- K=github.com/lubochka/llm_mvp_core @ codex/dpo-implementation; P=github.com/lubochka/xiigen-planning @ llm_plan3.
- K/00.Core/LogicalContracts.cs: existing ILogicalUnit<TInput,TOutput>.ExecuteAsync returns scalar typed LogicalUnitResult<TOutput>. K/00.Core/DynamicGraphContracts.cs: IDynamicGraphNode's IQueryable<GraphNodeCandidate> is a graph candidate boundary with OutputRefs and evidence SourceIds. *Request input identities are NOT automatically evidence SourceIds.* Use a backward-compatible request→outputs association/typed resolution/real next consumer, not a breaking scalar ABI rewrite.
- K/21.AgentInfrastructure/Logical/LogicalUnitDynamicGraphNode.cs and DeterministicDynamicGraphNodeOutputProjector.cs may produce IDs/projections, not desired TOutput. K/21.AgentInfrastructure/Logical/DynamicGraphEdgeFlow.cs may emit handoff; K/00.Core/DynamicGraphContracts.cs EdgeTargetHandoff.TargetsExecuted=false. Actual next ILogicalUnit.ExecuteAsync and ordinary Host are separate facts.
- K/21.AgentInfrastructure/Logical/RegistryDynamicGraphEdgeAuthorityResolver.cs resolves registered L2 EdgeAuthorityInput→EdgeAuthorityDecision; DynamicGraphEdgeAuthorityExecutionPipeline.cs calls it. Neither proves a mutable scorer parameter, feedback writer, persistence, fresh reader or downstream use. Discover each separately.
- K/00.Core/LearningMachineContracts.cs: TrainBatchAsync is on ILearningMachine; do NOT add Train onto ILogicalFlow or mislabel Student DPO as Flow learning.
- L2 Unit = typed many→many producer/adapter; L3 Flow = relations/corrections; RP04 owns bounded asynchronous L4 Orchestrator and higher-order L5 DynamicGraph; RP02 owns domain law/semantic vectors/tokens/kNN adaptive k; RP18 owns LoRA→semantic transposition; RP01 owns Student DPO; RP05 owns continuing user feedback.

## 2. Five *bounded* passes, parallel without accidental coupling

| Pass | Runnable work | Concrete evidence | Not prerequisite |
|---|---|---|---|
| P0 parallel NOW | 00B actual typed consumer trace ∥ 02A mutable L2 owner trace ∥ 00C PART A ONE real observed preference; 00A PRIMARY WorkLeft/RAG breadth background | missing named seam, real signal or NO_SIGNAL, owner | RP01 DPO, S04 Windows, 25 rounds |
| P1 typed lane | 01A→01B→01C | q1→o1,o2; q2→o2,o3; real TOutput and next Unit | first Flow lesson |
| P2 FIRST REAL LEARNING | 02B→02D | feedback_id + W0→W1 numeric change, new process reads W1; replay and rollback | multi-domain CERT, typed lane |
| P3 join/science | 02C; 03B; 03C | learned lawful typed next Unit; per-domain P/C0/T; cold ordinary PC | all 7 domains completed |
| P4 review | 04A | real, distinct CODEX Astra UltraCode call on exact final K SHA and raw cases | broad code-coverage gate |

OPTIONAL independent 01D Unit-only parameter ablation and 03A RP01 Student DPO never block first 02B. Use one writer per changed K file. The source-breadth audit 00A must map all seven domains but may finish after first legal 02B.

## 3. F0 legality, F1 learned correction and real observed feedback

F0 = deterministic **verified** eligibility mask and independent applicable domain arbiter verdict for type/nullability, tenant/rights/consent/license, schema/vector-law version, TTL, cancellation, state mutation budget/cycle. F0 is NOT the final learned semantic edge authority. Unknown law, ambiguous text grammar or undefined SQL dialect => CONDITIONAL/UNKNOWN and abstain/clarify as appropriate; do not turn guesses into hard-coded vetoes.

F1 = compact *learned* policy over >=2 F0-LAWFUL typed alternatives for exactly the same input x and compatible versions. It must have an identified actual mutable L2 scorer owner, pre-choice features, feedback writer and reader. Law-ineligible neighbor/candidate is vetoed before scoring; high kNN confidence never overrules arbiter (F/DECISIONS-LOCKED.md D-15; F/ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md). Store negative/warning neighbors as retrieval evidence, not lawful F1 negatives if illegal.

**Observed OP_DEV signal is essential, not a proposed rubric.** From consented, rights-cleared actual user A/B selection, independently observed outcome on real OP_DEV case, or independently performed qualified domain judgment: two alternatives that are BOTH F0-legal for SAME x, exact response bytes, observer/outcome/feedback ID, date, domain-law version, provenance and rights. NEVER derive that vote from sealed CERT, same model self-judge, imaginary human or generated chosen/rejected labels. If absent: 00C returns NO_SIGNAL and exact one-screen consented A/B capture route for 02B to use, no false W change. A real independent scored development example permits initial learning; later heldout families judge generalization.

**Minimal candidate learner algorithm** if the existing L2 scorer doesn't already offer a smaller compatible one (not a mandate to create another framework):

~~~text
legal = filter_with_verified_F0(x, typed_targets, law_version, arbiter)
if legal.count < 2: route_single_or_abstain; NO_UPDATE
require observed_feedback && same_x(A,B) && A,B in legal && rights_valid
feature_delta = phi(x, preferred) - phi(x, rejected) // no post-outcome features
if nonfinite(feature_delta) || norm(feature_delta) <= epsilon: NO_UPDATE
p = stable_sigmoid(dot(W0,feature_delta))
W1 = bounded(W0 + eta*(1-p)*feature_delta - eta*lambda*W0)
store_once(feedback_id, W0_hash, W1_hash, domain, port, law, space, checksum)
recheck_F0_and_arbiter_before_side_effects()
~~~

Phi uses domain-correct semantic/law features available at choice, type/port compatibility, previous permissible arbiter history, cost and fixed RP02 confidence. If scorer non-linear, provide the analogous named parameter/loss/update derivative and justify PC cost. Good illustrative (NOT OBSERVED) example: A phi=(1,0), B phi=(0,1), W0=(0,0), eta=.2, lambda=0, independent OP_DEV person actually prefers B → W1=(-.1,.1), B scores above A. Illegal option C stays masked. BAD: use A with missing nullable annotation as legal t-minus; this is a veto example, not trainable lawful preference. EDGE: tie, one legal option, same features, no independent observer, NaN, duplicate feedback, stale domain law/vector space or revoked rights → NO_UPDATE, old W unchanged.

Persistence 02D: atomically bind version, domain/port/tenant, schema/space/law, feedback_id, trial hash, parent, checksum and rollback. Terminate warm instance; fresh PID normal resolver LOADS W1 and makes decision; replay once-only; cold rollback to W0 reproduces old selection. Warm dictionary is NOT cold proof.

## 4. Three pilot exams and all seven WorkLeft requirements

WorkLeft-RESEARCH-01 defines **seven** domain tracks: EN↔RU, orchestration, Figma→HTML+CSS, C# docs, Angular docs, SQL docs, and DPO creation across the six. 00A must map all to PRIMARY and owner. Initial CERT pilots: C#, EN→RU and dialect-pinned SQL. Three pilots are NOT proof that seven domains were covered. Preserve existing 95% aspirational WorkLeft threshold but NEVER call it achieved before measurement.

| Domain | Grounded constraints to validate | Example lawful preference | Counterexample / edge |
|---|---|---|---|
| C# docs | real declared signature, nullable Task<int?> vs Task<int>, overload, async/exception contract | two accurate compact descriptions ranked by real independent outcome | dropped ? is hard veto; missing source overload => UNKNOWN |
| EN→RU | POS, referent/coreference/gender, morphology, negation, meaning | two legal translations, observed better referent-preserving one | flipped negation is illegal; врач masculine grammar ≠ known human gender |
| SQL | pinned dialect, schema, NULL semantics, GROUP BY, tenant and parameterization | two legal query strategies, accuracy+cost observed | missing tenant predicate is illegal; unknown dialect => abstain |
| Remaining 4 tracks | orchestration dependency law, Figma DOM/CSS fidelity, Angular API semantics, six-domain DPO correctness | define dedicated later independent OP_DEV/CERT panels | NOT_MEASURED until examined, not secretly attributed from C# |

kNN is not just Search(x,k) then hits[0] (K/KnnLogicalUnit.cs currently follows that pattern). Domain-appropriate token vector/distance/neighbor *eligibility* and adaptive k belong to RP02; some historically observed language choices depend on syntactic/referent/domain-law constraints, not mere cosine similarity. Freeze RP02 during RP03 W0/W1, then separately ablate exact-vs-ANN, k=1/fixed/adaptive, fixed/learned law features, optional RP18 LoRA projection and RP01 Student weights. Do not change all variables together.

## 5. Scientific evaluation instead of nondeterministic software tests

Freeze TRAIN and observed OP_DEV and sealed source-family-disjoint CERT (family/project/template/time) plus independent grader/rubric BEFORE training. CERT never used for teaching, case choosing, prompt tuning or hyperparameter selection. P = original pipeline; C0 = fixed serving implementation, W0, no feedback update; T = SAME repaired implementation, W1 after one actual admissible update. Fix Unit U0, Student LoRA, RP02 tokenizer/vector/k/distance/index, RP04 graph/scheduler, candidate pool, arbiter, domain, schema, budgets. C0−P = engineering improvement; **T−C0** = Flow learning delta (which may be zero or negative). A C# lesson is RU/SQL OOD/retention only; to claim *training* on RU or SQL, collect their OWN lawful same-x observed OP_DEV lesson and compare their separately frozen C0/T.

For each domain and learning bulk save: raw blind cases+families; n correct/N, law violations, abstains, association and path correctness, typed next Unit/Host end-task correctness, ranking margin/uncertainty, time-to-gain, p50/p95 latency, CPU/GPU/RAM/VRAM, tokenizer/vector/index/adapter/optimizer/checkpoint bytes and learning/evaluation energy when available; retention on earlier domains; P/C0/T per-case disagreement and uncertainty/CI; evaluator ID and judge disagreement. Repeated evaluation on the SAME CERT must not train the model; fresh blind families for bulk-after-bulk causal learning curve. Report GAIN/NO_GAIN/REGRESSION/INCONCLUSIVE/INVALID_EVIDENCE. Serious hard-law/rights regression => quarantine/rollback.

## 6. The minimum deterministic software verification budget

- 00A, 00B, 00C, 02A, 03B, 04A: **ZERO** new product tests; source receipts or independent blinded learning examination. No software coverage target.
- 01A/B/C and 02C code edits: reuse the closest existing narrow proof for shared association/typed source and one touched denial/tenant/version/cancel risk; do not regenerate broad test matrix.
- 02B changed actual learning writer: smallest touched update-sign/finiteness/applied-once/F0-law veto checks. 02D changed checkpoint: one interrupted atomic write/rollback and one duplicate/replay/compatibility check as concretely needed, reusing existing focused checks.
- 03C serving changes and optional 01D/03A: only touched rights/version/checkpoint/optimizer guards. Read-only work means zero new tests.

These examples are NOT quotas to add tests when risks are unchanged. No Windows GitHub Action or percentage coverage can establish learning accuracy. Keep genuine release/security/native/licensing gates at their impacted stage; do not disable safety tests, manipulate assertions or equate green tests to scientific gains.

## 7. Exact weak-model subtask prompt contract

For every task the executor reads only the task-relevant K/docs/skills with receipts: architect-behavior-classifier, reference-inventory-and-source-map, phase-planning, plan-review, plan-self-validation; code-examination/data-flow-integrity/generated-code-review when touching code; prompt-patch-authoring and conversation-intent-and-self-reflection for readable instructions; experiment-design and trainable-architecture-decision for learning decisions. No claim that all 194 skills were loaded just because named.

One card = one work order: **CURRENT TASK; NOT THE TASK; P/K branch and exact HEAD; needed primary sources (not merely RAG index), code producer→caller→consumer; owner, allowed edits and forbidden unrelated files; minimum permissions and feedback; steps; GOOD/BAD/EDGE with WHY; falsifier/repair; only touched deterministic safety check; deliverable and next owner.** If task too large, split only within the named card into short producer/learner/consumer handoffs, never create an S04-style dependency waterfall.

~~~text
TASK_ID; P_HEAD; K_HEAD_BEFORE; K_COMMIT_AFTER_OR_NONE
SKILLS_ACTUALLY_READ; ORIGINAL_WORKLEFT_PRIMARY_PATH_SECTION; RAG_LEAF_FOLLOWED; K_METHOD_CALLER_CONSUMER
INPUT_X; DOMAIN_LAW_VERSION; F0_LAWFUL_OR_CONDITIONAL; SIGNAL_ID_AND_OBSERVER_OR_NO_SIGNAL
W0_W1_NUMERIC_HASHES_OR_NONE; FEEDBACK_APPLIED_ONCE; COLD_NEW_PID_OR_NOT_RUN
REQUEST_SOURCE_IDS; OUTPUTREF_TO_REAL_TOUTPUT; NEXT_UNIT_EXECUTED_OR_NOT_INVOKED
GOOD_BAD_EDGE_WHY; FALSIFIER; FAILURE_REPAIR_OWNER; EDITED_FILES
SMALLEST_TOUCHED_CODE_CHECK_OR_ZERO; RAW_P_C0_T_CASES_OR_NOT_MEASURED
PC_METRICS_OR_NOT_MEASURED; ASTRA_SEPARATE_INVOCATION_ID_OR_NOT_PERFORMED
~~~

00A uses D/original task.md, task prompt.md, Алгоритм создания ДПО.md, WorkLeft-02, WorkLeft-RESEARCH-01/03, WorkLeft-MECH-01, WorkLeft-RAG-00/Master and actual PRIMARY leaves in G plan-01/02/07/08, F DECISIONS-LOCKED, R/previous rounds thinking results, R01–R25 findings; it must traverse them and preserve all seven domains. RAG headings are navigation, not proof. Source audits DO NOT gate immediate first 02B if actual signal/scorer are available. CODEX Astra UltraCode independent examination is NOT_PERFORMED without a real separate reviewer invocation with exact K SHA, raw data and review receipt. Authorized GitHub write ≠ automatic external model/data-use license.
