# S20 — hybrid compound-request algorithm; design, NOT newly deployed code

## Diagram of responsibilities
~~~text
ordinary raw user request + rights + typed target
 -> StackDetector BEFORE normalization (F D-07)
 -> RP02 map subject × developer intent × compiler/dialect/locale × law version, UNKNOWN supported
 -> S20 simultaneous domain-law and cross-tenant/type dependency envelope
 -> HARD source-authority / law / rights / arbiter admissibility (F D-15)
 -> RP02 per-subject semantic IR / codebook / vector (no incompatible-space cosine)
 -> RP02 lawful+warning+negative multi-neighbor selection, per-subject dynamic k under PC
 -> RP03 ILearningUnit true typed many→many OutputRefs, RP03 ILearningFlow corrective edges
 -> RP04 ILearningOrchestrator bounded lawful async DAG, RP04 ILearningDynamicGraph higher-order motifs
 -> RP11/RP13 select among enumerated *lawful* feasible routes by measured utility/full cost
 -> S20 typed inter-domain fan-in JOIN; final HARD arbiter -> existing Student where needed
 -> cold ordinary core/DTC/IDE output with exact state/provenance and full PC ledger
 -> feedback -> RP12 smallest causal repair or explicitly no update
 -> optional CODEX human case selection -> admitted open Chinese Teacher BOTH DPO sides
 -> separate CODEX exact-pair quality -> existing RP01 Student/LoRA DPO once
 -> RP09 independent P/C0/T -> RP10 acceptance/rollback -> cold ordinary consumer
~~~

## Core roles (names vs actually observed APIs)
**ILearningUnit:** conceptual domain/vector/input/output contract and many-to-many requested-type output; current K ILogicalUnit<TInput,TOutput> ExecuteAsync and GraphNodeCandidate OutputRefs are anchors, NOT proof of trainable many→many implementation.
**ILearningFlow:** typed correction relation between units, prove learned delta under frozen Units, not just changed metadata.
**ILearningOrchestrator:** lawful async scheduling of fan-out/fan-in, dependencies, cancellation, all attempted/cancelled branch costs.
**ILearningDynamicGraph:** learned higher-order pattern relationships (order two or greater) and actual ordinary route; never hard law authority.
**ILearningMachine/Student:** separate higher-level actual trainer/LoRA recipient. LoRA does not produce vector tokenizations nor silently train Unit/Flow/Graph.

## Multi-law and representations
Each scoped segment needs original span, subject, intent, target language/version/dialect/register, law authority/fingerprint, tenant rights, typed input/output, token/codebook/schema, OutputRefs, state and UNKNOWN/conflict. Shape is *conceptual*; use existing K DTO after 00C inspection. Joint candidate admissible only when every part and cross-boundary dependency is valid under deterministic arbiter and rights. Abstain/ASK or safe partial where facts are missing; never turn learned nearest neighbor into higher law.

K/PromptDomainClassifier D-04 is *developer-intent labels* not subject dialect. K/KnnLogicalUnit Search(_k) then hits[0] is not multi-hit output fusion. E51B 60-slot development vector and BPE/predicate kNN must be versioned and never compared raw without compatible basis; current projection must have identifiable trainable owner/weights/quality if called learned.

**Per-domain candidate dynamic k:** k_d(x) = clip(kmin,kmax,ceil(kmin + a_d*uncertainty_d + b_d*neighbor_disagreement_d − c_d*cost_pressure)). Coefficients are only an experimentable proposal until actual RP02 learned policy/state exists. Hard-law eligibility before scoring; negative/warning can caution/veto but must not be chosen as positive. Compare k=1, fixed k>1, dynamic k, exact/ANN **on matched eligible pool and whole-PC budget**, record contributors not just retrieved hit IDs.

**Route compactness:** enumerate feasible legal typed paths within finite PC budget; compare deterministic cheapest, existing learned policy and best *observed enumerated* path. No claim of unknown global optimum. Full wall/RAM/CPU/VRAM/disk, tokenizer/index, snapshots, failed/cancelled branches and externally authorized model/evaluator usage.

## Causal split
P=unchanged accepted parent pipeline; C0=same corrected deterministic architecture without Student update; T=**same C0 pipeline** plus exactly one actually authorized novel admitted Chinese both-side Student update. A better C0 is engineering benefit; only T−C0 with correct state mutation and blinded holdout can support Student learning. Later feedback bulks must retain prior domain knowledge. A partial/negative result is valid.

GOOD: C#9 API + SQL Server query both satisfy law; typed join preserves names/nullable and SQL parameters at ordinary consumer.
BAD: Plan developer intent label used as subject and k=5 hits listed while first-hit alone predicts; accept wrong SQL dialect due to Graph score.
EDGE: mixed version, unknown dialect, stale codebook, contradictory rights, zero legal neighbor, async cancellation, cross-domain negative transfer → bounded lawful abstention or owner handoff.
