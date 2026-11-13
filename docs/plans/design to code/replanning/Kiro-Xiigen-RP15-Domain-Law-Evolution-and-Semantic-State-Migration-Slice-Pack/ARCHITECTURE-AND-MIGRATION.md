# RP15 — minimal hybrid algorithm: law evolution → compatibility → smallest safe effect

## The one new mechanism

Existing ordinary request → [domain×intent×stack/dialect×version×rights mapper, RP02] → hard domain-law authority check → versioned semantic IR/codebook/projection → adaptive kNN with actual multiple lawful candidates and labelled negatives/warnings → typed ILearningUnit many→many → ILearningFlow corrections → bounded asynchronous ILearningOrchestrator → higher-order ILearningDynamicGraph pattern scoring → deterministic final arbiter and Student response → ordinary consumer.

**RP15 compatibility check occurs at the real stored-state read/use boundary**, between current verified law + semantic coordinates and previously persisted projection/index/Unit/Flow/Graph/adapter snapshots. Proposed adapter returns one of:
1. **REUSE_PROVEN**: same effective law meaning, domain/tenant rights, semantic vector coordinate system, typed interfaces and consumer, supported by actual evidence.
2. **SELECTIVE_REENCODE_REINDEX**: law/embedding geometry or index changes; rebuild affected scoped corpus and pointers, do not alter Student weights solely because of a version change.
3. **RECOMPUTE_TYPED_OR_GRAPH_RELATIONS**: typed outputs, fan-in/out or higher-order motifs stale under actual new law; handoff to RP03/RP04 owners.
4. **NEW_LESSON_CANDIDATE**: observed Student weakness on independent law-valid tasks after corrected migration; send lawful novel lesson for CODEX human selection, not automatic DPO.
5. **QUARANTINE_ABSTAIN**: contradiction/unknown provenance/rights, invalid score, stale incompatible representation or incomplete evidence. Deterministic safe response takes precedence.

These are **proposal semantics**, not claims that existing K types have these enum names. At 00B inspect real serializers, selected-state store and callers; prefer using actual existing types.

## Algorithm sketch and impact cone

INPUT: current trusted LawSnapshot L1; accepted StateBundle S0; one request q.
(A) Validate law authority, effective time, tenant rights and typed version of q; unresolved → QUARANTINE.
(B) Compare L1 against S0's effective law and semantic-coordinate hashes, not only length or string labels. Classify additive / corrective / breaking / contradictory / unknown.
(C) Compute affected dependency cone with edges: L→domain classifier→IR/token codebook→semantic projection→kNN corpus/index+metric→Unit associations→Flow corrections→Graph motifs→Student adapter references→ordinary consumer.
(D) Estimate bounded PC rebuild cost; choose proven reuse, smallest re-encode/reindex, typed-graph rematerialization or abort.
(E) Prepare changes under RP05/RP10 existing checkpoint/atomic pointer owner; idempotence key = law delta ID + source fingerprint + target consumer; never modify live accepted index in place.
(F) Final hard arbiter validates candidate laws, rights, type and branch cost independently of learned score; cold read verifies accepted version.
(G) Only after independent Student weakness may a separately approved new DPO lesson be considered. Failure, no-change and deterministic-only wins are first-class outcomes.

**Never** treat LoRA tensor weights as vector tokenizer, Qwen-authored string as a local hard-law authority, kNN negative neighbor as executable positive, a valid JSON manifest as a trained state, or Windows PASS as a domain-law exam.

## Deterministic vs learned boundary

| Stage | Deterministic invariant | Learned/empirical candidate | What RP15 examines |
|---|---|---|---|
| Domain mapping | trusted law/version/rights; valid UNKNOWN | domain posterior | changed law vs wrong routing; oracle only diagnostic |
| IR/token/vector | preserve negation/units/types; versioned coordinate identity | learned projection | equivalence/reprojection, not shape equality |
| kNN | positive/negative/warning roles; rights; compatible metric | rerank/adaptive k/radius | real multi-hit use, exact vs ANN, stale index |
| Unit/Flow | requested TInput/TOutput, many→many edges and port compatibility | association and correction | invalidate minimal affected typed links |
| Async/Graph | DAG dependency, cancellation and hard final arbiter | policy/motif utility | lawful alternative routes, order-2/3 pattern compatibility |
| Student/LoRA/DPO | exact role/authorship/teacher/rights/admission & state atomicity | one supervised DPO Student step | actual parameter and held-out outcome distinct |
| Local delivery | versioned selected pointer and rollback | best lawful policy | cold ordinary consumer + full-PC cost |

## Good / bad / adversarial

- **Good:** C# nullable/
