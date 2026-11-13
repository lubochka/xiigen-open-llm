# RP03 V7 — actual Flow learning, domain examination and authority guard-cost repair
**2026-10-15 · executable planning repair; K/runtime NOT CHANGED; independent Astra NOT PERFORMED.**

**Single live route:** [V7 dispatcher](RP03-V7-LEARNING-FIRST-DISPATCH-2026-10-15.md), `tasks/v7/RP03.<ID>.md` (15 independently executable work orders) and nine child cards under `tasks/v7/children/`. This document augments V7, does NOT launch V8, another 25-round exercise, or a universal CI gate. The editor reviewed actual GitHub source; review is not a training run, benchmark or separate model invocation.

## Verified source finding: 1+5+3 Authority calls

On current `llm_mvp_core@codex/dpo-implementation` inspected source (re-pin before editing):
- `21.AgentInfrastructure/Logical/DynamicGraphEdgeAuthorityExecutionPipeline.cs`, blob `70fcdcfbc824d9c0c71f66773136bb6c79fc580f`, `ExecuteAsync` invokes the resolved L2 `unit.ExecuteAsync` (about line 322) once, then order guard (about 402–411), then identity guard (about 460–483).
- `21.AgentInfrastructure/Logical/DynamicGraphEdgeCandidateOrderLeakageGuardUnit.cs`, blob `bc1a11655760777eddb638b6207f519b589305a8`, has `ProbeCount=5` and `ExecuteAsync` calls the L2 authority inside its loop.
- `21.AgentInfrastructure/Logical/DynamicGraphEdgeIdentityLeakageGuardUnit.cs`, blob `fbfff288a0783bfb4035c728b3baf6448c3e3b2d`, has `ProbeCount=3` and similarly re-invokes the authority.
- Normal complete control flow can therefore call L2 **1 + 5 + 3 = 9 times** for one authority result. This is a **source-level invocation count**, NOT a claim of measured ninefold latency, actual 9 calls on a specific request or state mutation. Both guard types protect against order/identity leakage. **Do not remove or skip either guard solely for speed**.
- **HYPOTHESIS to test:** repeated calls may observe different mutable W/feature versions or stochastic ties; then legitimate nondeterminism could appear as leakage or produce excessive resource use. Code reading alone does not prove it happens. RP03.02A traces/pins W snapshots and exact calls; RP03.02B guarantees a fixed inference-only parameter snapshot across the request; RP03.03C measures host-level latency/CPU/RAM/VRAM and offers any replacement proof that maintains the *same* order/identity invariants. Candidates: semantic canonicalization by construction and version-scoped certification with explicit invalidation on W, φ, code or domain-law changes; accept only with a concrete safety argument and smallest affected invariance/denial check.

## Verified distinct type/consumer boundary

K `21.AgentInfrastructure/Logical/LogicalUnitDynamicGraphNode.cs`, blob `781ba5afe61bd3dfa34049133fd4f334a710daa0`: `ExecuteAsync` calls **one** scalar logical Unit for **one** request, projects `GraphNodeCandidate[]`, then returns `candidates.AsQueryable()`. That API shape alone does **not** imply native q1+q2→{o1,o2,o3} association or a typed next consumer. RP03.00B and 01A–C must construct explicit (q1,o1),(q1,o2),(q2,o2),(q2,o3) associations (o2 shared but rights-scoped); resolve `OutputRef` to real `TOutput`, not `ref.ToString()`; invoke registered **next** typed `ILogicalUnit<TInput,TOutput>.ExecuteAsync`. The K `DynamicGraphRegistryL3SelectedTargetExecutor.ExecuteAsync` invokes L3 `flow.ExecuteAsync` (blob `0de38c17d85ecacacceec8f016c6035645d8917c`); `EdgeTargetHandoff.TargetsExecuted=false`, so an L3 handoff is not that next L2 invocation.

## Parallel shortest path, no hidden global gate

| Start | Tasks | Exact observation/return |
|---|---|---|
| NOW, three independent executors | 00B ∥ 02A ∥ 00C-A | actual typed producer/consumer map; L2 real weight writer/reader and 1+5+3 call structure; independently **observed** consented same-x pair of two F0-legal choices, or NO_SIGNAL |
| NOW, nonblocking sources/exam | 00A ∥ 00C-B | original/WorkLeft/RAG→**opened primary** G/F/V seven-domain crosswalk; three domain TRAIN/OP_DEV/EXAM_SEALED family partitions |
| FIRST LEARNING | 02B then 02D | smallest compatible registered Flow scorer and one F0-lawful independent preference to update numeric **W0→W1** (otherwise BUILD_READY_NO_SIGNAL only); atomic W1/idempotency, fresh-process new-PID ordinary L2 reader, replay, rollback |
| Parallel typed lane | 01A→01B→01C | explicit N:M source associations, lawful scoped TOutput dereference and real next typed Unit→Host; independent of W0→W1 |
| JOIN / cold ordinary use | 02C, 03C | identical corrected code C0(W0) vs T(W1), W1 picks admissible target, next typed Unit actually runs, cold ordinary Host result and total PC resource ledger |
| Research evaluation | 03B and three standalone C#/EN→RU/SQL children | blind independent same-case P/C0/T, separate wiring C0−P and candidate learning T−C0, hard-law failures, calibration, ambiguity, time/bytes/bulk, retention |
| Optional, NEVER first-lesson gates | 01D Unit only; 03A RP01 DPO only; 04A genuinely separate exact-SHA Astra UltraCode | no fake Unit learner, Student/Flow confusion or author-as-independent review |

02B can build scorer/write/read when **NO_SIGNAL**; it cannot claim learned W1. Hidden heldout CERT cannot provide training feedback. Domain kNN/tokenization and RP18 semantic projection are separate parallel development streams, not universal barriers to the first isolated Flow lesson.

## Safety contract F0 versus learned F1, precise learner

1. Domain passport fixes `domain_id, law_version, schema_version, semantic/token-coordinate version, feature names/order, port, tenant/rights, allowed effects`; classify **HARD**, **CONDITIONAL**, **UNKNOWN** evidence. Preserve domain mapping and abstention. RP02 maintains law-conditioned kNN, adaptive k and explicit role/AST/neighborhood counter-evidence; never assume `hits[0]` is the universal answer. LoRA parameter, tokenizer ID and semantic vector are **different spaces**.
2. F0 deterministically filters illegal type, tenant/authorization, consent, schema, TTL, effect, cycle or safe cancellation. A known illegal answer is a veto, **not** a negative in F1 preference data.
3. Two **lawful** same-input same-version alternatives (a+,a-) and independent observed `feedback_id`; freeze all other components. Use only pre-outcome features φ. Candidate simple rule (not Student DPO): `g=φ(x,a+)−φ(x,a−)`; `p=σ(W0·g)`; `W1=clip(W0+η[(1−p)g−λW0])`. `η,λ,feature version` set before outcome. Example `φ(A)=(1,0)`, `φ(B)=(0,1)`, `W0=(0,0)`, preference `B>A`, `η=.2`, `λ=0` ⇒ `W1=(-.1,+.1)`. **ILLUSTRATION, NOT EXPERIMENT.**
4. If a single legal option, tie, ambiguous source, leaked CERT, `g=0`, wrong vector coordinate, non-finite update, stale/duplicate feedback ID ⇒ **NO_UPDATE** or ABSTAIN; never adjust F0 or hidden rubric to make experiment pass. Snapshot W consistently for all inference probes. Persist trial atomically and verify a completely **new process** reading W1 through the actual registered unit and, separately, genuine next typed Unit+Host consumption.
5. RP01 Student DPO is **not** this Flow ranker: delegated-human educational case chosen by CODEX Astra 6 UltraCode, an actually permitted external open Chinese Teacher authors **both** final chosen/rejected, *another* CODEX Astra UltraCode judges unchanged exact hashes, response-conditioned `πθ/πref` DPO trains Student. Do not label a user correction as Chinese-authored. RP04 Orchestrator/Graph governs async scheduling/higher-order patterns; RP05 owns ongoing feedback adoption.

## Domain laws: smallest three independent exams

**C# documentation:** freeze C# compiler/runtime, external source family, `Task<int?>` versus `Task<int>`, overloads, async/cancellation and exception contract. GOOD: two lawful documentation layouts preserve the same actual signature; independent reader prefers one and later unseen family improves. BAD: dropping `?` counted as lawful losing F1 candidate. EDGE: undocumented exception ⇒ `UNKNOWN`, not invented hard law. **Falsifier:** trained and exam source families overlap or actual typed next Unit absent.

**EN→RU:** map POS, syntax/dependencies, discourse referents, negation, grammatical versus semantic gender and permitted paraphrases. GOOD: preserves established referent/negation. BAD: `врач` grammatical masculine automatically forces a person's biological gender; wrong nearest word wins against explicit referent. EDGE: `Мария увидела врача. Она улыбнулась.` may have discourse ambiguity; evidence-aware conditional grading/ABSTAIN. **Falsifier:** C# Flow lesson alone reported as trained EN→RU learning, without separate EN→RU observed pair.

**SQL documentation and routing:** pin engine, exact dialect, tenant/schema, NULL semantics, transaction/isolation, result multiset vs incidental row order. GOOD: two F0-lawful tenant-filtered `IS NULL` selects with equivalent authorized results compared by measured cost. BAD: faster query removes tenant predicate or uses `= NULL`; these are hard F0 violations, not lawful negative preferences. EDGE: differing dialects, optimizer churn, missing schema provenance ⇒ UNKNOWN/ABSTAIN. **Falsifier:** two candidate queries don't return same authorized rows or model changed another variable.

For all, separate `TRAIN`, `OP_DEV`, `EXAM_SEALED` by **source family**, not duplicate case rows. Raw per-family `P, C0, T`: P legacy; C0 corrected code W0 no lesson; T identical corrected code W1. Only `T−C0` is candidate Flow learning effect; `C0−P` is wiring. Include raw outputs, case hash, law validity, accuracy correct/N and paired disagreement, independent judge and uncertainty, calibrated abstain, next L2/Host, time-to-first-benefit, per-bulk learning curve, prior-domain retention; total wall p50/p95, CPU/GPU, RAM/VRAM and disk for weights/tokenizer/index/optimizer/Flow journal/checkpoints/rollback. Include per-request diagnostic 1+5+3 call overhead separately and do **not** infer 9× slowdown. N too small ⇒ INCONCLUSIVE, no invented 95% pass.

## Minimal deterministic testing, never stochastic code coverage

| Work | Required check | Forbidden substitute |
|---|---|---|
| Read-only source/review/plan/primary mapping | **ZERO new product software tests**; direct method/caller/consumer and references | Windows CI/test count presented as learning proof |
| F0/typed rights state changes | Only **changed risk** accepted result and changed denial/invariant (same tenant,type,TTL,version,effect,cancel as actually touched) | Broad test coverage target or new full workflow |
| Numeric Flow writer/checkpoint changes | Smallest changed sign/non-finite/no-op/idempotency, atomic save/load/replay/rollback and safety probes | A W hash/green build called quality gain |
| Non-deterministic learning | Independent law-aware source-family-blind per-domain **P/C0/T examination** and resource/retention evidence | Unit tests for exact sampled words / after-the-fact tuning of CERT |

Never bypass genuine security/legal/native release requirements when reached. Do not run GitHub Windows jobs merely to confirm stochastic learning. F0 guards are deterministic protection and **remain required** where actually modified.

## Required weak-model task receipt

Each task keeps its own real context, narrow skills, WorkLeft/RAG→opened primary section, scoped code owner and specific GOOD/BAD/EDGE/FALSIFIER, changed-risk probe and exact return schema in `tasks/v7/`. Return: `P_SHA,K_SHA,source+primary+skills actually read,method producer→caller→reader→ordinary Host, rights/domain law, observed independent feedback or NO_SIGNAL, W0/W1 hash+numeric or NOT_RUN, new PID and actual next Unit or NOT_ADOPTED, independent P/C0/T raw cases or NOT_MEASURED, total PC ledger, GOOD/BAD/EDGE/falsifier, failure class→smallest repair owner, next task, commit`. Separately mark SOURCE_TRACED/CODE_CHANGED/PARAMETER_CHANGED/COLD_CONSUMED/CAUSAL_MEASURED/USER_ADOPTED/INDEPENDENT_ASTRA_REVIEWED (TRUE/FALSE/NOT_RUN). Never conflate statuses or invent model/runtime invocation IDs.

Source navigation: [Original role contract](../../original%20task.md); [WorkLeft PROMPTS](../../09.09.26WorkLeft-PROMPTS.md); [WorkLeft mechanism](../../09.09.26WorkLeft-MECH-01-mechanism-validation.md); [R04 findings](../results/R04/FINDINGS.md); [R22 causal experiments](../R22-CAUSAL-EXPERIMENT-CATALOG.md); [previous rounds](../previous%20rounds%20thinking%20results.md); [parallel join](../PARALLEL-R01-R03-JOIN.md); [Repair Atlas](../RP01-RP05-REPAIR-ATLAS-2026-10-15/). Executor should open appropriate primary G/F/V via WorkLeft RAG leaf (not merely cite an index), read only task-relevant sections of real K skills (architect, phase-planning, plan-review, code-examination, trainable-architecture-decision, prompt-patch-authoring, learning-path-audit, experiment-design, claim-verification, human-readable-architecture-audit) and avoid recursively loading all skills.

**Publication boundary: PLANNING_EDIT_ONLY. `llm_mvp_core` / Flow or Student weights UNCHANGED_BY_THIS_DOCUMENT; runtime learning/domain exam/PC performance/independent Astra NOT_RUN, NOT_MEASURED or NOT_PERFORMED.**

## Latest source-backed N26–N31 delta
[Detailed evidence, GOOD/BAD/EDGE/falsifier and 7-domain owner split](RP03-V7-SOURCE-REPAIRS-AND-7-DOMAIN-EXAMS-2026-10-15.md). **New hard distinction**: early W1 is allowed in an isolated experimental recipient, but serving resolver rejects untrained/trust<Probationary; trial must not be mislabelled Trusted. **New registry hazard**: newest exact ID selected before trust filtering, so rollback/promotion must cover the serving tuple as well as weights. **New typed data proof**: the node projector moves CandidateId, not TOutput; typed next Unit requires explicit real dereference. 7 WorkLeft domain exam children = C#/ENRU/SQL pilot + ORCH/DESIGN/ANGULAR/DPO follow-up. Unchanged: original V7 15 parents, zero source-only software tests, smallest changed deterministic risk probes, raw P/C0/T for learned outcome, no Windows coverage gate.
