# R23 — PC resource, durable learning state and feedback lifecycle

**2026-10-13 · R23 documentary design · PRODUCT_CODE_UNCHANGED · LEARNING_NOT_RUN · PC_FIT_NOT_MEASURED · INDEPENDENT_ASTRA_REVIEW_NOT_PERFORMED**

## 0. Assignment and scope

R23 is one of the existing R21–R25 supplementation assignments in the 25-run replanning programme (10 collection + 5 analysis + 5 applied revisions + 5 supplementation), not an extra training run. Its job is to turn R18 first-learning path and R22 causal experiment catalog into a **bounded, recoverable, fully costed on-PC lifecycle** with feedback and domain retention. No teacher, Student, CODEX or evaluator model was invoked; no weight, dataset, product code, CI, Windows, provider or native workflow was changed.

Direct controls: [R23 active prompt](run-prompts/R23.md), [COMMON](run-prompts/COMMON.md), [RUNS-21–25](RUNS-21-25.md), [LEARNING-WORK-ORDERS-21–25](LEARNING-WORK-ORDERS-21-25.md), [R21 curriculum](results/R21/FINDINGS.md), [R22 contrasts](R22-CAUSAL-EXPERIMENT-CATALOG.md), [R18 work order](R18-FIRST-LEARNING-IMPLEMENTATION-WORK-ORDER.md), [R17 risk evidence policy](R17-EVIDENCE-POLICY-2026-10-13.md), [original four-role goal](../original%20task.md), [original DPO design](../Алгоритм%20создания%20ДПО.md), [WorkLeft-02 teaching roles](../09.09.26WorkLeft-02-teaching-chinese-model.md), [training integration](../09.09.26WorkLeft-INTEGRATION-01-training-into-work-program.md), [training prompts](../09.09.26WorkLeft-TASK-PROMPTS-TRAINING.md). Actual read depth and source SHA are in [R23 SOURCES.csv](results/R23/SOURCES.csv). WorkLeft/RAG, non-S04 slices, DTC and IDE remain in global programme; R23 does not claim that it read every RAG/WorkLeft file or completed R01–R20.

### Non-negotiable training roles

1. **CODEX with GPT-6 Astra UltraCode** performs the *human role* of selecting a legal, meaningful lesson; it is not the Student.
2. A separately approved **external open Chinese model authors BOTH final DPO sides**, chosen and rejected, for the frozen exact request/context. A weak Student answer may inform lesson choice; it is not automatically the authored rejected side.
3. **CODEX with GPT-6 Astra UltraCode performs a separate examination of the EXACT Chinese-authored pair** (domain laws, superior chosen, plausible meaningful negative, rights, byte/context integrity). It may return TIE, BOTH_BAD or UNJUDGEABLE. It does not silently rewrite pair bytes as though they were teacher-authored.
4. The existing permitted ILearningMachine/Student DPO trainer updates only the intended mutable recipient, at most once for an admitted new pair. The mathematical DPO reference is not a CODEX judge.
5. An independent post-update evaluator compares lawful held-out behaviour. A final arbiter accepts or rejects candidate state before an ordinary user caller reloads it. CODEX's pair judgment is not the independent post-update evaluation, and neither is the R25 architectural review.

These are current S04 / WorkLeft-02 programme constraints, not evidence of actual model invocation. An ordinary user correction must NOT bypass Chinese dual-side authorship/CODEX selection and distinct exact-pair judgment for S04, even though a separate generic feedback route can store User-authored pairs for other legitimate declared policies.

### Whole-system roles preserved

| Layer | Deterministic law and type boundary | Learnable authority and ordinary consumer | Cost and state which R23 must retain |
|---|---|---|---|
| ILearningUnit | Domain, vector/token/input/output schemas, typed many-to-many result associations and admissible candidates | Installed Unit adapter/ranker/mapping if actually trained; downstream typed Unit consumer | Unit state, embedding/codebook version, many-to-many association IDs, memory/feedback source |
| ILearningFlow | Typed ports, OutputRef identity/resolution, valid correction and fan-in | Learnable relations/correction weights; next Unit consumer | Edge/association version, correction audit, retained intermediate state |
| ILearningOrchestrator | Dependency/order DAG, cancellation, side effects, finite budgets, final lawful path | Learned route/order/branch ranking; real async ordinary caller | All launched/failed/cancelled branch compute, latency and resource concurrency |
| ILearningDynamicGraph | Trusted typed higher-order edges and pattern admission, deterministic arbiter | Versioned learned higher-order relation policy, real L2→L3 resolution and execution | Graph/index/cache/preload/rollback versions, unseen composition cost |
| ILearningMachine and DPO/LoRA | Exact author/judge rights, provenance, one-pair journal, frozen reference, state promotion | Actual installed project-LoRA Student for first CodeDocumentation E1 | Parent/reference/trial weights, adapter, optimizer, activation, RNG, evaluation and reload |
| DTC, IDE, translation, C#/Angular/SQL and DPO Builder | Source rights, user consent, per-domain laws and native host requirements | Other later domain-specific consumers, perhaps local project adapters | Real user path and user-feedback privacy, per-domain retention and broader delivery gates |

No new four trainers are authorized by naming four interface roles. A deterministic correct solution is not penalized because it uses no stochastic route. Learned-only comparisons never waive domain legality, safety or types. Higher accuracy at worse full cost may be inferior to a correct bounded deterministic path.

## 1. Direct code finding: generic FeedbackRouter is not S04 DPO selection

At current core branch codex/dpo-implementation, source excerpts show:

- ContinuousLearning/Feedback/FeedbackRouter.cs: ACCEPTED adds a positive TrainingExample. EDITED persists a DpoPair where chosen is **UserCorrection** and rejected is **ModelResponse**, with explicit User authorship; REJECTED constructs a DpoPair whose chosen is a deterministic **[no preferred response]** sentinel. Side-authorship validation/governance and file persistence exist in that route.
- ContinuousLearning/Pipeline/ContinuousLearningPipeline.cs: indexes and caches examples, reserves an eligible pending pair transaction, calls a per-domain incremental trainer and publishes an adapter via registry. That is an existing mechanism, but the selected excerpt does not establish which ordinary UI sends the feedback or that its output enters the current S04 controlled one-lesson path.
- ContinuousLearning/Feedback/FileDpoPairStore.Transactions.cs: transactional selection, replay ledger, reserved/committed identities and capacity are present. Transactional persistence is real, not evidence of new Chinese-authored pairs or functional improvement.
- CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.cs explicitly states that baseline.json is **not a complete recovery snapshot**: weights, optimizer and random generators are not serialized in that JSON. Separate optimizer/random/trial/checkpoint manifests exist.
- RepositoryOwnedModelCheckpointManifest.cs carries parent, weight hash and compatibility fingerprints; IncrementalTrainingSession.cs separates saved-parent resume from explicitly authorized cold start.
- KnnLogicalUnit.cs constructs a trained BPE/HNSW memory Unit with k configured at Train. As previously traced in R06, ordinary Execute selects the first-hit output and uses later hits as evidence. A changing k does not prove multi-neighbor decision quality.

**R23-F01:** storing a User-authored feedback pair does not make it a final open-Chinese-authored S04 pair. Existing generic feedback may be separately legal, but it needs its own declared scope; the S04 path must pass a frozen new lesson through CODEX human-selection, approved Chinese both-side authorship, separate CODEX exact-pair quality verdict, correct target, one update, independent examination and ordinary adoption. The integration question is **which host/caller owns feedback, how it is quarantined/validated, and which declared learner receives it**, rather than a demand to rewrite FeedbackRouter or declare all generic feedback invalid.

## 2. Full-PC resource envelope

**No device profile, numeric limits, checkpoint bytes or observed concurrent peaks were supplied to R23. Numeric PC-fit is UNKNOWN/NOT_MEASURED.** Do not infer it from LoRA size, a historical workstation mention or one successful native/Windows CI.

### 2.1 Binding before each side effect

| Mandatory input | Proof required before claiming measured fit |
|---|---|
| Machine identity, OS/arch, total and actually usable RAM, headroom policy | Fresh local device/process snapshot and owner-approved reserve |
| GPU count, VRAM free/usable, dtype/offload, driver, CUDA/CPU fallback, pinned/unified allocations | Runtime readback for chosen Student and actual concurrent phases |
| Filesystem free space, permissions, atomicity, journal paths, snapshot/rollback and transient-copy reserve | Real disk free bytes, canonical artifact directory and possible double residency |
| Student/base/tokenizer, frozen reference, model shape, current accepted and trial checkpoints | Manifest paths and verified hashes, actual loaded key sets, runtime compatibility |
| Trainable LoRA A/B keys, rank, reference freeze, AdamW moments, gradients, activations, context/batch and KV | Exact config and owner enumeration, not declared default alone |
| Domain features, embeddings/BPE/codebook, index, k/radius/metric, graph/caches, feedback/journal and retained examples | Versioned owner and actual persistent/resident sizes |
| Chinese author, CODEX selector/judge, evaluator, grants and data-transfer boundaries | Explicit legal/provider/model permissions, per-role call/token/currency/time caps and privacy |

### 2.2 Phase ledger and simultaneous peaks

Capture P0 preflight, P1 parent baseline, P2 CODEX select + Chinese author + CODEX exact assessment, P3 accepted/trial snapshot, P4 exclusive optimizer update, P5 isolated trial/parent/no-update examination, P6 promotion/rollback, P7 cold ordinary inference, P8 feedback/new bulk. For every phase record actual loaded objects and overlapping processes, CPU seconds, wall seconds, p50/p95 latency where applicable, peak CPU RSS, GPU allocated/reserved VRAM, disk live bytes + temp peak, I/O and network tokens and **all failed, rejected, cancelled and restarted calls**.

| Resource family | Double-count/missing-cost trap | Correct measurement |
|---|---|---|
| Base Student and reference, inference KV, tokenizer | Reference may be a full separate model; shared tensors cannot be assumed or counted twice | Sample simultaneous actual process/device usage by phase and version |
| LoRA, gradients, optimizer, activations and batches | Small adapter, large train-time optimizer/activation peak | Per-parameter bytes and actual P4 peak including batch/context |
| Domain vectors/codebooks, kNN/HNSW/BM25, Graph/preloads, negative/warning examples | Reindex can retain old and new indices, cache masks growth | Resident and persistent index+graph bytes before/after each bulk |
| Parent/trial/checkpoint, RNG/scheduler/counters and no-repeat journal | baseline.json is not weights; rollback reserve can dominate update | Disk snapshots, temporary copies, owner commit markers and fsync/readback |
| Async parallel branches and fallback | Only winning branch time hides other compute and side effects | All launched/aborted/timeouts, queue, fan-out and cost by branch |
| CODEX selector, Chinese author, CODEX judge, independent evaluator | Remote provider memory is not local PC RAM but calls/data/cost matter | Per-role actual calls/tokens/network/wall/currency, including invalid lessons |

Memory formula: peak_RAM = maximum concurrent actual RAM(t); peak_VRAM = maximum concurrent actual VRAM(t), with allocated and reserved reported separately. Reject before unsafe mutation unless each peak plus approved headroom fits the real usable device capacity. Disk preflight reserves **all simultaneously live parent/trial/optimizer/RNG/journal/reindex/temp/rollback** bytes plus headroom; never assume separate measured peaks happen concurrently without timeline evidence. A budget cannot be approved by simply removing rollback or shortening assessed semantic content.

Full effort = teacher/choice/judge/Student update + evaluation + indexing/reindex/reload + recovery + ALL failed/invalid/rejected attempts + normal inference under contention. Count cost-per-accepted-useful-improvement only if nonzero independently proven improvements; otherwise report UNDEFINED. Full time cost is not equivalent to GitHub Actions duration.

### 2.3 First E1 bounded envelope, not a product run permit

| Parameter | Required R23 constraint and binding |
|---|---|
| Domain/lesson | Existing CodeDocumentation C# Student, one new lawfully selected family and newly admissible exact pair; legitimate NO_TRAINING_NEEDED exit |
| Model/LoRA | Existing R18 A–D recipients; actual accepted or authorized clean cold parent, detached frozen reference, exact mutable LoRA keys and tokenizer |
| Update | **One new pair_id, at most one permitted optimizer application** to isolated trial; no invisible replay, no second Step after uncertain outcome |
| Parallelism | One exclusive mutation owner for a parent; ordinary inference may be serialized or concurrently profiled within real memory cap |
| Author/judge | Per-role permission and finite calls/tokens; CODEX Astra UltraCode selection and separate exact check, Chinese model both final sides |
| Exam | Frozen parent, no-update and trial on identical predeclared family panel, one untouched confirmation path and retention; promotion only after result |
| Context/masks | Exact pair preserved through domain-sensitive tokenizer and response-only conditional DPO objective; no semantic Compact 480 truncation |
| kNN/Graph | Freeze representation/ranking/k/index/Graph on E1; later experiments R22:C01–C11 isolate factors |
| Hard budget | Owner-approved finite RAM/VRAM/disk/wall/tokens/calls/lesson and evaluation limits **before** their relevant operation |
| Windows and S05 | No blanket Windows-coverage scientific gate; native/host validation remains where E1 actually crosses that boundary; S05 remote is separate |

R18 G1–G6 remain runtime/design bindings (accepted state or lawful cold start, installed trainable keys, permitted teacher/judge/evaluator, frozen fair exam panel, ordinary generation adopter and actual finite resource profile). No fake numeric cap is supplied by R23. If hardware or grant is missing, only affected mutation is blocked; read-only R21/R22/R24 domain preparation may continue.

## 3. Recoverable lifecycle and transition evidence

Every transition has versioned objective_id, family/domain/stack/project/tenant, training_run_id, pair_id, parent/ref/trial hashes, model/tokenizer/representation/index/Graph/LoRA version, journal effect status, authority, owner and ordinary selected consumer. Do not use a loose filename or UnitId as evidence of trainable recipient.

| State | Permitted operation and owner | Reopen / recovery rule |
|---|---|---|
| ACCEPTED_PARENT → BASELINE_READONLY | R18 owner verifies accepted checkpoint, or explicit lawful cold start **only if genuine absence**, frozen rubric/families/reference and normal response | Corrupt, incompatible or inaccessible checkpoint is NOT evidence that no model exists |
| BASELINE → SUFFICIENT or WEAKNESS | Read-only ordinary Student and independent rubric; store domain-law failure and all resource costs | Sufficient baseline may end without teacher, pair or update |
| WEAKNESS → FEEDBACK_QUARANTINE → LESSON_SELECTED | Validate rights/consent/novelty/domain, then CODEX Astra UltraCode selects case | Raw feedback is evidence, not final chosen side or training grant |
| LESSON_SELECTED → AUTHORED → JUDGED | Approved open Chinese model authors BOTH final chosen/rejected; CODEX Astra UltraCode separately checks exact pair | Changed context/bytes, TIE/BOTH_BAD/UNJUDGEABLE, no rights, or no grant → reject/hold |
| JUDGED → SNAPSHOT_SEALED → RESERVED | Training owner writes and rereads immutable parent, LoRA, optimizer, scheduler/RNG/counters, journal pair IDs, reference, dataset and index/Graph compatibility state | baseline.json alone is not the state; reserve rollback before update |
| RESERVED → MUTATION_ATTEMPTED → TRIAL_SEALED | Exclusive trainer updates only isolated trial with one novel pair, durable effect marker and actual cost receipt | Ambiguous crash/timeout after Step = UNKNOWN_STATE, journal/weights/optimizer/RNG reconciliation **before any next mutation**; never blind replay |
| TRIAL_SEALED → INDEPENDENT_EXAM | Read-only parent/no-update/trial same-panel comparisons and held-out/retention with separate outcome evaluator | If evaluator fails, resume evaluation of same state; do not redo pair/update |
| EXAM → PROMOTED or PARENT_RETAINED | Arbiter applies predeclared law/quality/uncertainty and budget/retention floors, atomic verified acceptance or rejection | Lower loss/new checkpoint/hash never automatically promote |
| PROMOTED → COLD_ORDINARY_ADOPTED | Actual ordinary host reloads selected state with matching tokenizer/encoder/index/graph; response observed | Stale ordinary model = ADOPTION_GAP, not confirmed learning |
| ADOPTED → NEXT_NOVEL_BULK | Valid new source family/feedback may enqueue next case, old-domain retention frozen | No replay of accepted pair, no hidden shared-base overwrite, S06 campaigns have their own limits |

**Crash/failure examination scenarios** (future finite owner observations, not a new Windows coverage campaign):

1. Insufficient VRAM or rollback disk before P4: no mutation; accepted parent intact.
2. Pair persisted, optimizer not attempted: reopen original pair and exact CODEX assessment, no regenerated pair ID.
3. Crash after optimizer step but before durable trial marker: quarantine UNKNOWN_STATE, reconcile journal, pair_id and weight/optimizer/RNG fingerprints; second update forbidden.
4. Trial sealed and evaluator fails: resume read-only evaluation; parent still accepted; do not repeat Student update.
5. Promotion write times out: exact readback accepted pointer before assuming success/failure; never expose unverified trial.
6. Ordinary caller still loads previous checkpoint: report ADOPTION_GAP with versioned evidence; do not claim user improvement.
7. Tokenizer/codebook/vector metric or index/graph version mismatch: fail closed or explicit measured migration with rollback and two-copy peak.
8. Power loss, exhausted budget, teacher/provider unavailable, S05 remote blocked: preserve local state and truthful cost; no hidden paid fallback.

## 4. User feedback is a typed observation, not a free training credential

| Event | Evidence classification | Authorized next step |
|---|---|---|
| Thumbs up / Accepted | User preference, not factual proof | Record exact answer, domain/family/model version and consent, verify independently before proposing lesson |
| User correction / Edited | May be right, wrong, changed requirements, ambiguous or out-of-scope | Verify versioned real-world law/source/stack, domain and tenant; retain User provenance; if eligible CODEX selects lesson, Chinese model authors both sides |
| Rejected without correction | A weakness signal, not a known correct alternative | Queue independent source/lesson; deterministic "[no preferred response]" must not masquerade as Chinese final chosen |
| Spec/domain law changed | New target version, not necessarily earlier model error | Version requirement and rubric, avoid retroactive penalization, recreate lawful family split |
| Conflicting tenants/users | Scope and privacy conflict | Isolate private/user/project state and feedback; do not mutate common assets by default |
| Poisoning, closed exam/forbidden data, licensing unknown | Disallowed or unresolved evidence | Reject/quarantine with minimal legally allowed metadata; never leak sealed exam answers to teacher |
| Duplicate pair/repeated complaint | Recurrent signal, not new optimizer entitlement | Coalesce evidence; no renamed duplicate pair to evade journal |
| OOD, teacher absent or budget reached | Unknown/unsupported for current setup | Bounded queue/abstention and safe deterministic fallback; no unauthorized model call |

Feedback admission requires source consent/rights, data scope, independent domain-law plausibility, exact task version, genuinely novel family, correct trainable recipient, permissible approved teacher/CODEX roles and available resource/rollback capacity. Unknown values cause HOLD/QUARANTINE. User feedback must not mutate weights synchronously during a request. The user may give feedback, but for S04 the human instructional choice role is **CODEX GPT-6 Astra UltraCode**, not automatic FeedbackRouter or user-side DPO authorship.

Ordinary user feedback → classified evidence → domain/legal/source and novelty check → CODEX selects one lawful lesson → approved Chinese model authors chosen/rejected → CODEX independently judges exact pair → target-specific ILearningMachine update → independent exam and arbiter → selected checkpoint → cold ordinary caller → next feedback. Existing generic FeedbackRouter, domain cache, pair transactions and current S04 one-lesson journals are distinct integration seams; R24 should trace their real host/consumer and R18 should trace their training ownership. Do not invent a cross-route API by matching names.

## 5. Bounded growth, domain transfer, continuous compactness

- Bound feedback item/byte counts, pair journal/replay ledger, domain index vectors, graph edge/preloads, RAM/VRAM caches, state snapshots, adapter versions and active branches by owner-approved quotas; reserve critical journal and accepted parent from compaction.
- On representation/tokenizer/semantic ResultProjection/metric changes, re-embed and reindex compatible data under a new version and account for dual live index copies and rollback; same dimension is not semantic compatibility. LoRA adapter weights by themselves do not define vector tokenization.
- Separate R22:C01 domain classifier, C02 feature/vectorization, C03 metric/rerank, C04 dynamic k/radius, C05 ANN/exact, C06 first-hit vs aggregation, C07 Unit×Flow, C08 Orchestrator, C09 Graph, C10 LoRA vs ResultProjection, C11 valid DPO vs memory-only. Vary one causal factor and hold lawful candidate pool/domain constraints fixed where designed; dynamic k is not learned merely because it is query-dependent.
- Preserve seven baseline R21 subjects: EN↔RU translation, orchestration, Figma→HTML/CSS, C#, Angular, SQL documentation and DPO Builder. Preserve extra languages, stored procedures, mixed domains, DTC and IDE/VS Code/Continue, S00–S10; these are not prerequisites for the first C# lesson. Freeze old-domain retention and source families before derived variants.
- Each bulk is **new distinct lawful lessons**: parent B0 → trial B1 → if accepted next B2, otherwise parent retained. Do not replay prior pair as a new one. Measure accuracy and lawful path selection on unchanged independent panels, old-domain worst-family retention, unknown-domain abstention, time-to-useful-correction, disk/memory index growth and full attempted cost (including failures).
- At capacity pressure prioritize rights/safety, domain scarcity/negative-warning cases, worst-domain retention and expected information gain per full cost. Stop before unsafe mutation or defer case; do not silently drop rollback or independent exam. Archive/compact only under separately authorized retention policy and verified journal state. R23 deletes nothing.

## 6. Learning examination and truthful verdicts

For the existing R18/R22 E1 experiment pre-register one primary domain-law-aware output metric, number of independent **source families** (not paraphrases), parent/no-update/trial conditions, held-out/retention split, stopping/look rule, variance/uncertainty, maximum tolerated hard-law errors, latency/resources and owner-approved acceptance threshold BEFORE outcomes. Compare same family panel with frozen source, tokenizer, candidate pool and evaluator, with the trial examined before promotion. Exact P0–P8 resource receipt and ordinary-state identity accompany every scored answer.

For later R22 contrasts: map oracle vs predicted domain and UNKNOWN, law-sensitive vector integrity (negation, number/SQL dialect/HTML semantics), lawful neighbor utility and fast-but-wrong failures, Unit many-to-many association, Flow correction, Orchestrator valid path and all branch cost, Graph unseen higher-order composition, LoRA actual recipient and old-domain transfer. In a small finite lawful graph enumerate feasible paths for best comparator; for larger graphs report best-observed fixed-budget comparator, never a claimed global optimum. Compare deterministic-only, learned-only *within laws*, and hybrid, then report quality per full PC cost.

Required per-bulk data: experiment_id, bulk_id, source-family split/hash, raw case IDs and independent-family denominators, law and domain/intent/stack/version, parent/no-update/trial/adopted model hashes, CODEX selection and distinct exact verdict IDs, both final Chinese author hashes and legal profile, pair_id/no-repeat state, tokenizer/vector/metric/index/k/Graph/LoRA versions, per-domain quality and worst-domain retention, path validity/cost, p50/p95, all token/model call and branch costs, RAM/VRAM simultaneous peak, temp and steady disk, reload/reindex/recovery, consent and final arbiter decision.

Verdicts: **IMPROVE / REGRESS / INCONCLUSIVE / INVALID_EVIDENCE / NOT_MEASURED**. Neither green CI, new hash, loss reduction nor adapter size is a measured improvement. A valid negative is a completed investigation, not permission to fit the metric to the result. Historical 95% is a long-term domain rubric target, not first-lesson threshold, router confidence or test coverage.

## 7. Model-ready bounded subtask prompts (document only; ZERO workers run)

**COMMON child packet:** Read active R23 and COMMON plus exact task-specific skills from core docs/skills; follow relevant WorkLeft/RAG locator to source PRIMARY span, pin repo/branch/blob/read depth and actual current state. Return only assigned result to R23 parent, with input→target owner, lawful good/bad examples, edge cases, falsifier, resource and authority, next owner. Do not push sibling shards, invoke models, train, delete data or run Windows Actions. A missing actual hardware/provider/consumer is recorded as UNKNOWN with precise next read, not improvised. Skill citation by title alone is not evidence that content was loaded.

### R23-A — Full PC footprint, queue and finite caps

**PROMPT:** Trace actual phase P0–P8 live object graph and cost across Student/reference, project-LoRA/AdamW/gradients/activations, tokenizer/codebook/embeddings, BM25+kNN/HNSW and graph, journal/snapshots, teacher/CODEX assessor/evaluator. Bind actual device, dtype, context/batch, concurrency, free disk/headroom. Produce per-phase concurrent allocation timeline, peak RAM/VRAM and bytes on disk, p50/p95 and full attempted costs including failed/cancelled work. Set stop-before-mutate constraints and enumerate unknown bindings; do not choose numerical caps from an unavailable machine.

**INPUT:** R22 E1, R18 G1–G6, R09 corrected CI evidence, current optimizer/checkpoint/journal code, owner-provided device profile. **SKILLS:** trainable-architecture-decision; phase-planning; code-examination; experiment-design; output-readability-gate. **GOOD:** trial+reference simultaneous memory and snapshot reserve UNKNOWN, no PC-fit claim; no update until actual preflight. **BAD:** "adapter 2 MB, so fit on 8 GB" or omitting cancelled branches. **EDGES:** GPU/CPU offload, shared tensors, disk full, evaluator in another process, teacher remote, concurrent ordinary serving. **OUTPUT:** P0–P8 footprint and stop ledger with actual/estimated/unbound tags. **ACCEPT:** all resource owners, headroom assumptions, sample method and all cost accounted; no invented measurements.

### R23-B — Full state, unknown-effect recovery and cold adoption

**PROMPT:** Follow one specific permitted pair through CodeDocumentationOneLessonEndpoint/session/grant → exact DPO material → baseline attempt journal → optimizer/RNG/trial manifests → publication coordinator → accepted checkpoint loader → actual normal generator caller. Differentiate generic FeedbackRouter/FileDpoPairStore transaction from S04. For each write/transition list owner, durable bytes/hash, permit, effect status, source-state version, rollback and readback. Simulate OOM before update, post-step crash, evaluator crash, promotion timeout, incompatible index/tokenizer, old ordinary cache, lawful cold start vs corrupt accepted checkpoint. Name unknown actual consumer as a gap, not assumed complete.

**INPUT:** WorkLeft training prompts, R18 A–D, current feedback/training code and R05/R07 attributed seams. **SKILLS:** trainable-architecture-decision; code-examination; plan-review; learning-loop-closure; output-readability-gate. **GOOD:** crash after Step → UNKNOWN_STATE reconcile journal+weights+optimizer+RNG → read-only exam if trial verified. **BAD:** re-run same pair after HTTP 500 or call baseline.json full snapshot. **EDGES:** partial journal fsync, incompatible index, stale consumer, local S04 accepted but remote S05 unavailable. **OUTPUT:** transition/recovery map and specific missing owner patch proposal. **ACCEPT:** no possible blind re-application of pair, no orphan unaccounted model state, ordinary adopter traced or openly unbound.

### R23-C — Feedback classification and law-aware DPO handoff

**PROMPT:** Pin real ordinary feedback producer→FeedbackRouter/cache/pairs→transaction→actual selected policy owner; classify Accepted/Edited/Rejected, factual user correction, changed requirement, poison, tenant conflict, consent, duplicate/OOD and missing teacher. Distinguish User-authored generic DPO from S04 Chinese both final sides, CODEX human lesson choice plus separate exact-pair quality judgment. Establish lawful domain and typed Unit/Flow/Orchestrator/Graph recipient version before training.

**INPUT:** FeedbackRouter.cs, ContinuousLearningPipeline.cs, source rights/WorkLeft-02, original roles and R21 domain laws; no invented UI API. **SKILLS:** client-adaptive-learning-loop; learning-loop-closure; code-examination; trainable-architecture-decision; prompt-patch-authoring; output-readability-gate. **GOOD:** SQL dialect correction independently verified and only then a CODEX-selected lesson; raw user edit remains attributed to user. **BAD:** thumbs-up means ground truth or "no preferred response" treated as a Chinese chosen side. **EDGES:** TIE/BOTH_BAD/UNJUDGEABLE, malicious correction, changed domain law, offline provider, different tenants. **OUTPUT:** admission table, target and actual caller/missing handoff. **ACCEPT:** no unauthorized feedback-to-optimizer shortcut, all domains and user privacy retained.

### R23-D — Controlled growth, old-domain retention and causal efficiency

**PROMPT:** Plan distinct-new-family B0/B1/B2 bulks, independent holdout and old-domain regression after each accepted update. Freeze factors for R22 C01–C11, compare law-preserving vector/token schemes and appropriate domain k/radius, fair fixed k/metric, actual consumer influence, and Unit/Flow/Orchestrator/Graph independent policies. Count all memory/index/graph/snapshot growth, reindex overlap, async losers and actual time-to-useful-feedback. Hand exact adoption and independent-review questions to R24/R25.

**INPUT:** R21 passports, R22 contrasts, R13/R17 experimental rubric, R18 first ordinary adopter; selected index/Graph code. **SKILLS:** phase-planning; plan-review; experiment-design; learning-loop-closure; output-readability-gate. **GOOD:** a fast wrong C# solution fails a law; valid EN↔RU negation regression prevents promotion regardless of tiny adapter. **BAD:** changing model, embedding, k and index together then crediting k; deleting rollback state for fit. **EDGES:** ANN omitted right neighbor, first-hit ignores k, teacher outage, Graph fan-out, silent old-domain forgetting. **OUTPUT:** predeclared bulk schema and finite growth/retention rules. **ACCEPT:** meaningful future scientific evaluation and no invented numerical outcome.

## 8. Exact handoff and open blockers

**R18 existing A–D:** add this P0–P8 envelope, verified snapshot/rollback identities, feedback ownership separation and G1–G6 numeric/runtime bindings; keep one existing CodeDocumentation learner and correct exact-pair→token→response objective→ordinary consumer instead of recreating a trainer. **R22:** add true full PC/teacher/CODEX/judge/evaluator and failed-branch cost to E1 and C01–C11, freeze one changed factor. **R24:** trace DTC/IDE/translation/feedback ordinary host consent and actually selected state, determine effect on real user result without declaring deployment from adapter files. **R25:** request a separately invoked independent GPT-6 Astra UltraCode review of exact candidate/ref and code for user/Chinese authorship confusion, crash recovery, budget, first study fairness, no-repeat and old-domain retention; R23 did not execute that review.

Open and not fabricated: target PC hardware/VRAM/disk and numeric caps, actual admissible teacher/CODEX/evaluator profile and grants, full accepted/checkpoint+reference identity, final mutable trainable keys, actual user-feedback host and CodeDocumentation ordinary generator, predeclared evaluation thresholds/family IDs and resource observations. These are **specific first-experiment bindings** rather than grounds to restart all 25 replanning tasks.

**R23 exit verdict: DOCUMENT_PREPARED / PRODUCT_NOT_EXECUTED.** Publication of this contract and own result carriers can complete R23 documentary task, but cannot establish PC_FIT, user-visible learning, correct dynamic kNN, domain vectorization improvement or successful continuous adaptation. All those require future authorized measured observations.



<!-- R23-REPAIR-2026-10-13 -->
## 9. Repair after work-order completeness review

The previous §7 A-D packets are retained as historical design summaries, **not as valid executor prompts**. A current skill review found that they did not contain the complete mandatory sub-agent schema and bundled independent learning questions too aggressively.

The repaired executable specification is [R23-REPAIR-WORK-ORDERS.md](R23-REPAIR-WORK-ORDERS.md). It splits R23 into eight bounded work orders:
A whole-PC resources; B durable state/recovery; C feedback admission; D DPO-pair quality; E domain mapping/laws; F vector tokenization/domain representation/LoRA; G dynamic kNN; H Unit→Flow→Orchestrator→DynamicGraph interaction and bulk-to-bulk learning.

**Current rule:** deterministic tests/checks may establish state safety, contracts, replay prevention and integration boundaries. They are not evidence that a learning algorithm improved. Learning acceptance comes from matched before/after bulk experiments on independent source families with domain-aware quality, retention, time and resource measurements.

This repair also makes the WorkLeft/RAG inventory mandatory instead of optional background discovery. Start from RAG-MASTER-INDEX/RAG-00 and trace the relevant RAG/WorkLeft prompts/research/mechanism files before narrowing the task.


<!-- R23-V2-LIFECYCLE-CORRECTION-2026-10-13 -->
## 10. V2 current-executor correction — no research barrier before E1

The A–D in §7 and A–H initial inventory are historical documentary steps, **not** the currently executable model instructions. The single effective full prompt is [R23-REPAIR-WORK-ORDERS.md §V2](R23-REPAIR-WORK-ORDERS.md), which supplies actual source-routing/skills and A0/A1, B0/B1, C0/C1, D0/D1, E0/E01–E07, F1/F2, G1/G2, H1–H5 bounded subtask prompts. It repairs both the old missing allowed/forbidden boundaries and the undifferentiated four-role H experiment.

**Do not delay the first R18 E1 learning attempt until F/G/H or all seven R21 domains finish.** The only mandatory pre-mutation controls are the actual E1 G1–G6 source/legal/hardware/rollback/immutable pair/no-repeat/heldout panel/ordinary-adopter bindings. A0/B0/C0/D0/E0 collect their minimal evidence; other R23 investigations proceed independently as they become source-ready. Training and paid teacher/CODEX calls remain separate authorized R18 operations, not part of the R23 documentary result.

The model can test deterministic state/type/replay protection without substituting that result for learning quality. Actual quality needs paired no-update/parent/trial ordinary-consumer evaluation across independent source families and protected older domains, plus full PC/call/time/failed-attempt cost. DPO author and exact-pair judge provenance, learned authority in each Unit/Flow/Orchestrator/DynamicGraph layer, domain law/representation/dynamic kNN hypotheses and false-positive outcomes remain distinct. **Current R23 V2 status = EXECUTOR_PROMPT_REPAIRED, PRODUCT_TRAINING=0, MEASURED_PC_FIT=0, INDEPENDENT_ASTRA_CODE_REVIEW=NOT_PERFORMED.**
