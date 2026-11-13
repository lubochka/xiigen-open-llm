# R15 REPAIR-1 — fact-checked author/editor audit; independent review still open

**13.10.2026. Current status: GOVERNING PROMPT REPAIRED / PARENT_SOURCE_AND_CODE_AUDIT / INDEPENDENT_ASTRA_NOT_PERFORMED / LEARNING_NOT_MEASURED.**
This new section is **not** the requested separately invoked CODEX GPT-6 Astra UltraCode review and does not replace the historical reviewed candidate record below. It validates the grounds for [R15-REPAIR-WORK-ORDER.md](../../R15-REPAIR-WORK-ORDER.md) and its connection to [run-prompts/R15.md](../../run-prompts/R15.md). New editorial source review took place after the old R15 candidate at `c1a428a9f753bd97bb46544b02e47705d7416128`; the governing plan prompt before this results update was published at `9a274918bbbadefb71215ba33ee25dc118e0dc54`. **Neither SHA is an independent Astra-approved candidate**.

## F-R15R-01 — what was verified anew, and what was not

Directly opened target primary sources: `original task.md` (four roles, typed many→many, asynchronous alternatives), `task prompt.md`, `goal of the round.md`, the current R16 contract at the top of `Алгоритм создания ДПО.md`, `09.09.26WorkLeft-02-teaching-chinese-model.md`, current `RESEARCH-03` and `MECH-01` introductions, `09.09.26WorkLeft-RESEARCH-01-training-quality-speed-accuracy-program.md` (seven real subject families and one-new-pair lifecycle). Read current R01–R06 findings, R12/R13/R14/R16 actual status carriers, R15 historical five carriers, JOIN, historical-round excerpts, and the real code below. Actual skills opened: trainable-architecture-decision, phase-planning, plan-review, reference-inventory-and-source-map, code-examination, generated-code-review, prompt-patch-authoring, human-readable-architecture-audit, experiment-design, improvement-measurement. Sources appended as S21+.

No fresh complete recursion through all WorkLeft/NDJSON/three codebases, no complete graph/ordinary-generation caller trace, no model invocation and no empirical learning test was performed by this repair. S00–S10/DTC/IDE/non-slice coverage remains bound to the dedicated R01–R10 receipts and to R15-A independent spot-check; file discovery is not whole-program semantic closure.

## F-R15R-02 — concrete pair/content break, not abstract “DPO risk”

Core `18.CapabilityTraining/LlmIngredientLogicalUnits.cs` blob `cb8744fcbbcff99821852e789dc92163976c0ad7`, lines **2269–2343**, was reopened directly. After admission of the **raw** `Prompt/Chosen/Rejected`, `BuildBatch` separately applies `Compact` before `tokenizer.Encode`; `Compact` collapses whitespace and truncates to **480 characters per part**. `ToMask` marks the whole non-padding sequence, including prompt positions. If a judged valid chosen/rejected distinction is only in the lost suffix (or meaningful formatting), training on the compacted pair is not training on what CODEX/Astra judged. This is **CODE_TRACED** behavior; frequency of real occurrence and post-patch gain are **NOT_MEASURED**. Reviewer must demand semantically lossless projected bytes/boundaries and exact final-pair reassessment after material transformation, with smallest R18 seam; no new trainer.

Counterexample: a legal/code answer differs only in an obligation/cancellation clause after character 480; the pair passes raw identity admission but the optimizer sees equal/semantically altered sides. A correct deterministic answer is **not** a bad/rejected example merely because it uses a fixed algorithm.

## F-R15R-03 — DPO objective and source→trained-policy boundary

Core `11.DPO/DpoTrainer.cs` blob `9c358978075db8c22fe0125eaea9e89e831d5635`, `StepVector` and `ComputeLogProbs/BackwardLogProbs` approx. **535–765**: current log scoring sums masked next-token likelihoods and **divides by count**; backward uses the same count. Combined with current BuildBatch/ToMask this is mean over prompt+response scored positions, **not automatically** the claimed conditional logπ(response | prompt) preference objective. DPO action/reward, response mask, tokenizer boundary, length policy, reference freeze and actual trained recipient must be explicit and symmetric in forward/backward. A deliberate length-normalized alternative can be investigated, but cannot masquerade as standard DPO; the public `EvaluateAverageNextTokenLogProbability` contract cannot be silently changed. Actual improvement **NOT_MEASURED**.

`R05` traced existing training/coordinator/state handling but could not identify the exact ordinary CodeDocumentation generation caller using the *accepted* version. Trial reload or generic loader is not sufficient. Semantic selector recipient is not necessarily the winning solver ID or `LogicalDpoTrainingUnit.Id`. R18 must close the selected source→optimizer→updated state→ordinary consumer seam or mark it untraced, not invent a second trainer.

## F-R15R-04 — kNN sees neighbours but current output uses only first

Core `21.AgentInfrastructure/Logical/KnnLogicalUnit.cs` blob `2957816a88aa9cdcd3020bb81be56b5dfff165e0`, **102–178, 201–256**, reopened directly: training freezes k; inference executes `_index.Search(query, _k)`, then picks `best=hits[0]` and emits `bestExample.Output`. Remaining hits only contribute evidence IDs. Low-confidence returns a **warning plus answer**, not an abstention. Increasing k or changing HNSW efSearch does not prove learned dynamic candidate selection or multi-neighbour aggregation. This is a causality defect in an *unproven plan claim*, not evidence a new learner is impossible.

For an E2 hypothesis bind deterministic lawful pool/domain filters, vector/index versions, a learnable reranker/aggregator/budget state, domain-feature signal, actual multi-hit decision consumer, exact-search diagnostic and frozen baseline. Compare wrong vs oracle-domain, mixed/OOD, SQL NULL/negation, lexical near-misses, lawful retrieval/path and full runtime cost. LoRA parameter update ≠ tokenizer/codebook and ≠ semantically faithful vector space; contrast via unchanged law-sensitive held-out examples and an explicitly named projection consumer. No universal best k/rank/metric is asserted.

## F-R15R-05 — refreshed status correction and scoped dependencies

- **R12** RUN-LOG currently says `R12_REPAIR_PUBLISHED / REEXECUTION_REQUIRED`; repaired **prompt** A–F is not completed architectural selection. Actual state owner, signal, full Unit/Flow/Orchestrator/Graph connection and chosen first seam remain constrained where dependent.
- **R13** top RUN-LOG and FINDINGS explicitly say `REPAIR_REQUIRED`. E1–E4 architecture-neutral protocol is usable as bounded evidence, but it is **not** a populated authoritative first experiment card. Thus R14's earlier “R13 protocol present” can be true only at historical/provisional-method level; “first recipe ready” is too strong.
- **R14** current V2 changes the former M0→M7 linear reading to a capability DAG and accepts E1–E4 ranked by expected information gain/implementation cost, with E2/E3/E4 eligible before E1 if better justified. A winner/fallback cannot be selected post-hoc or on existence of a StepVector method alone.
- **R16** already changed four live planning/requirements documents. Its log saying R15 was absent described its earlier snapshot; do not retroactively declare R15 missing *now*, and do not infer R16 has applied **later** R15 findings. R16's actual changed sections need receiver-specific readback/disposition.
- **Independent R15 Astra Ultra review:** **NOT_PERFORMED**. The historical GPT-5.6 Sol examination is useful parent adversarial evidence, not independent PASS. This author/editor repair also cannot award independent PASS. **R25** independently checks final candidate later, it does not relabel this step retroactively.

These are **local exact-binding blockers**, not a mandate to stop all unrelated domain/kNN diagnostics, preserve all old Windows gates or restart R01–R12. Conversely a repaired prompt or candidate table alone is not implementation readiness or actual trained improvement.

## F-R15R-06 — four independent role falsifiers and first learning observation

| Role/chain | Deterministic protection | Learned cause that must change | Independent falsifier |
|---|---|---|---|
| ILearningUnit many→many | typed domain/input/output associations, lawful schema and provenance | selected Unit's learnable state, domain-specific representation/action preference | candidate weights change but typed output or ordinary Unit decision does not |
| ILearningFlow relations | compatible Unit ports and transition laws | relation/correction state with Units held fixed | same Unit outputs, learned Flow route/repair has no quality effect or no ordinary adopter |
| ILearningOrchestrator async | allowed dependency order, cancellation and bounded costs | learned branch/path policy under same solvers/resources | better solver alone explains gain, branch ordering/cost unchanged, or no consumer |
| ILearningDynamicGraph higher order | admissible compositions and pattern order | higher-order graph relation state | no lift on *unseen lawful compositions* with lower levels held fixed |

Compare E1/E2/E3/E4 as alternatives, freeze one affected mechanism, lawful held-out family, no-update/memory-only controls, pre-bound measure, full PC resource envelope, retention, actual ordinary consumer, one-pair/no-repeat, domain accuracy/path quality and per-bulk cost/quality. `IMPROVE/REGRESS/INCONCLUSIVE/INVALID_EVIDENCE` are future **empirical** verdicts; no learning run happened. 95% is a source-stated target, not a measured current score. A domain-law-perfect simple deterministic baseline may win; the point is to discover that early.

## F-R15R-07 — disposition of prior ten findings

| Old issue | Refreshed condition | Exact dependency/receiver |
|---|---|---|
| O1 R12 architecture | OPEN: R12 A–F not yet executed | R12 then R18 for only affected state/recipient/consumer; independent E2 may continue |
| O2 R13 provenance | OPEN: R13 correction says REPAIR_REQUIRED | R13 and exact R18 first recipe; R16/R19 readback |
| O3 E1–E4 selection | OPEN: no locked *candidate-specific* winner/fallback | R18 compare causal information/cost/panel/ordinary adoption |
| O4 exact DPO pair | CONFIRMED CODE RISK, not measured gain | R18 lossless pair/objective/recipient, R20/21/22 consumer |
| O5 dynamic k | CONFIRMED first-hit current consumer | R12-D/R22 learned multi-hit state and contrast |
| O6 LoRA semantics | OPEN HYPOTHESIS | R22/R23 semantic downstream + full PC cost |
| O7 four roles | OPEN until owner→learned state→ordinary consumer traced | R12-B, R18, R22 |
| O8 independent Astra | **NOT_PERFORMED**, not paper PASS | actual separately invoked R15 reviewer and later R25 |
| O9 test/Windows burden | KEEP REPAIR; no new gate | R17 risk-based evidence/readback |
| O10 PC compactness | **NOT_MEASURED** | R23 full observed footprint and retention |

**This document's acceptance:** detailed governing prompt published, genuine primary/code observations logged and explicit receiving actions named; **independent review and learned-system gain NOT completed**. No product test/run/code/dataset/weight was modified here.

---

## HISTORICAL FIRST R15 REVIEW — retained without representing it as the current independent verdict

# R15 — adversarial candidate review / independent Astra review boundary

**Date:** 2026-10-13  
**Candidate reviewed:** `c1a428a9f753bd97bb46544b02e47705d7416128` on `llm_plan3` before R15 publication  
**Requested independent reviewer:** CODEX using GPT-6 Astra UltraCode  
**Actual independent invocation:** **NOT_PERFORMED** — no separate Astra/Codex reviewer invocation is available in this session.  
**Parent examination actually performed:** GPT-5.6 Sol adversarial review of the exact published R11–R14 planning candidate.  
**Independent-review status:** **INSUFFICIENT_EVIDENCE / NOT_PERFORMED**.  
**Parent review verdict:** **NEEDS_FIX before exact R18 implementation binding.**  
**Learning gain:** **NOT_MEASURED**.

This file does not pretend that parent self-review is independent review. It records what can be established from the exact candidate and routes concrete repairs into R16–R20.

## 1. Strongest version of the current direction

The current candidate fixes the original failure mode substantially:

- progress is organized around learning questions/capabilities rather than slice number or Windows/coverage throughput;
- deterministic correctness is separated from learning efficacy;
- the four behavioral levels remain distinct: ILearningUnit, ILearningFlow, ILearningOrchestrator and ILearningDynamicGraph;
- domain mapping, semantic representation/vectorization, dynamic kNN, DPO, LoRA/projection, ordinary-consumer adoption, retention and full-PC cost are explicit;
- DPO role policy is preserved: CODEX/Astra performs the human lesson choice and exact-pair assessment; the external open Chinese model authors both final chosen and final rejected; Student training and post-update examination are separate;
- E1–E4 are treated as falsifiable experiment families and the first experiment is intended to maximize information gain per implementation cost instead of defaulting to the nearest code seam;
- DTC/IDE/native obligations are preserved without making all of them universal prerequisites for the first learning observation.

That is a materially better direction than “finish S04 plus tests/Windows first”.

## 2. Blocking objection O1 — R12 is still not a selected hybrid architecture

**Source:** current `results/R12` explicitly says `REEXECUTION_REQUIRED`.  
**Counterexample:** R18 cannot bind an exact learner recipient/state/ordinary consumer when R12-B..F have not yet produced the final causal architecture.  
**Impact:** an implementation packet can accidentally optimize the nearest DPO seam and recreate the same narrowing failure that R12 repair was created to prevent.  
**Smallest repair:** execute R12-A..F against the already identified primary sources and produce one candidate-bound architecture result; do not launch new repository-wide research.  
**Receiver:** R16 for requirement wording; R18 for exact seam; R19 for roadmap migration.  
**Disposition:** **BLOCKS exact implementation binding, does not invalidate capability-first replanning.**

## 3. Blocking objection O2 — R13 status conflict is real and must be resolved

**Source A:** R14 treats R13 E1–E4 protocol as current/observed input.  
**Source B:** `results/R13/RUN-LOG.md` begins with a STATUS OVERRIDE stating that R13 is **REPAIR_REQUIRED** under `R13-REPAIR-WORK-ORDER.md`, requiring latest R01–R06 semantics, consequential primary WorkLeft/RAG reads, a populated first experiment card and fresh readback.  
**Counterexample:** a downstream run may read R13 FINDINGS/DECISIONS as complete while the authoritative run log says the current completion receipt does not exist.  
**Impact:** candidate provenance is ambiguous; R14’s “R13 is no longer missing” statement is too strong unless the repaired R13 outputs are separately present and authoritative.  
**Smallest repair:** perform the already-scoped R13 repair and make the current status single-valued across FINDINGS/DECISIONS/RUN-LOG/CHANGE-MAP.  
**Receiver:** R16, R18, R19, R20.  
**Disposition:** **BLOCKS claiming exact experiment readiness.**

## 4. Objection O3 — “information gain / implementation cost” is a ranking principle, not yet an executable selection

R14 correctly removes the false E1-first rule, but the candidate does not yet contain a frozen comparison table with actual candidate-specific:

- exact missing seam;
- ordinary consumer;
- expected falsifying observation;
- implementation effort category;
- confounders;
- PC cost envelope;
- prerequisite count;
- evidence-risk score or qualitative ordering;
- reason the selected candidate beats the others now.

**Counterexample:** E2 can always be said to be “more informative” after E1 looks hard, or vice versa, producing a moving target.  
**Smallest repair:** after R12/R13 repair, freeze one R18 selection record with an explicit E1/E2/E3/E4 comparison and one winner plus one fallback. No new framework is required.  
**Receiver:** R18.  
**Disposition:** **NEEDS_FIX before execution.**

## 5. Objection O4 — DPO validity must bind exact judged bytes to optimized bytes

The candidate already names the right risk: suffix/whitespace/truncation, response-conditional objective, recipient and ordinary consumer. R15 confirms these are not cosmetic.

A valid DPO lesson requires:

1. the external open Chinese model authors both final sides;
2. CODEX/Astra human-role selection authorizes the lesson/case;
3. CODEX/Astra separately assesses the exact final projected pair;
4. the optimizer receives the same semantically relevant bytes/context that were assessed;
5. the changed state is the intended Student/selector owner;
6. the ordinary consumer reads the accepted state;
7. a no-update control has comparable context/memory/compute.

If any of these are absent, weight movement or loss change is not a valid learning claim.

**Receiver:** R18/R20/R21/R22.  
**Disposition:** **keep as hard evidence contract.**

## 6. Objection O5 — dynamic kNN cannot be proven by changing k

Current research identifies existing exact/BM25/HNSW/domain-classifier seams and also a first-hit/fixed-k style limitation in at least one path. Therefore “dynamic k” is not itself the learnable algorithm.

The future experiment must name:

- deterministic domain/law filter;
- representation/vector identity;
- candidate pool/index identity;
- metric/features;
- learned reranker/aggregator or learned budget state;
- lower/upper k;
- how multiple neighbours influence the decision;
- exact-search diagnostic;
- ANN assumptions;
- ordinary consumer and final arbiter.

**Counterexample:** changing k from 4 to 8 while the consumer only uses the first accepted neighbor cannot demonstrate learned dynamic-k benefit.  
**Smallest repair:** R12-D/R22 bind one actual multi-neighbor consumer or explicitly add the smallest aggregation/rerank seam.  
**Disposition:** **candidate architecture direction accepted; executable mechanism still unbound.**

## 7. Objection O6 — LoRA is not semantic token/vector correctness

The current direction correctly separates LoRA from tokenization, but downstream packets must keep that separation operational.

Every LoRA/projection claim must bind source representation, target representation, trainable parameter group, frozen base, objective, version/rollback and actual consumer.

**Counterexample:** a smaller adapter or lower adapter loss can coexist with semantic collapse of negation, order or domain law.  
**Smallest repair:** use law-sensitive downstream contrasts, not adapter shape/loss, as the acceptance observation.  
**Receiver:** R18/R22/R23.

## 8. Objection O7 — four-role learning can still collapse if state ownership stays abstract

The four names are preserved, but each executable packet must provide owner/state/signal/mutation/storage/consumer/falsifier:

- **ILearningUnit:** typed many→many domain/input/output behavior and unit-local learned state where applicable;
- **ILearningFlow:** learned relation/correction between Units with Unit behavior frozen for causal examination;
- **ILearningOrchestrator:** asynchronous path/dependency/branch policy with branch cost and cancellation;
- **ILearningDynamicGraph:** higher-order relation state demonstrated on unseen lawful compositions.

**Counterexample:** better Student solver output followed by the same fixed route is not evidence of Flow/Orchestrator/Graph learning.  
**Receiver:** R12-B/R18/R22.

## 9. Invalid-evidence scenarios R15 requires future runs to distinguish

The following must produce INVALID_EVIDENCE rather than IMPROVE/REGRESS:

- assessed DPO pair differs from optimized pair after projection/truncation;
- prompt+answer scoring is used where response-conditioned preference is required;
- solver updates while the claim is selector/Flow/Graph learning;
- challenger gets extra memory/context compared with no-update control;
- ordinary caller remains on parent checkpoint;
- held-out family leaked into training/admission;
- paraphrases of one family are counted as independent domains;
- rubric/threshold changes after answers are observed;
- hidden replay or repeated accepted pair is counted as new lessons;
- update outcome is unknown but a second update is attempted;
- representation/index version changes together with k/metric/solver and gain is attributed to one factor.

Correct deterministic answers are not rejected merely because they are deterministic.

## 10. First-experiment readiness versus whole-program completeness

**First-experiment readiness:** **NOT READY YET** because exact R12 architecture and repaired R13 first card are not authoritative.  
**Whole-program direction:** **PROVISIONALLY SOUND** as a capability DAG preserving hybrid deterministic+learned mechanisms and delivery obligations.  
**Broad-transfer claim:** **NOT ALLOWED** until at least one valid positive observation is replicated on a structurally different domain/source family.  
**PC compactness:** **NOT MEASURED** until full runtime/index/graph/cache/snapshot/evaluator cost is observed.

## 11. R15 candidate-bound verdict

- exact R11–R14 implementation candidate: **NEEDS_FIX**;
- exact independent Astra review: **NOT_PERFORMED / INSUFFICIENT_EVIDENCE**;
- capability-first architecture direction: **retain**;
- coverage/Windows as learning proof: **reject**;
- next action: repair R12 and R13 semantics, then bind a single exact R18 experiment packet and submit that exact candidate to the required CODEX GPT-6 Astra UltraCode independent review.
