# E2 hybrid algorithm: precise behavior, learning ownership and bounded implementation

## Typed input and route schema (design target, not claimed existing API)

DomainRequest = {raw_input_ref, subject_domains[], development_intent, stack_language, stack_framework, stack_version, user_task_type, source_family_id, data_rights, subject_laws[], uncertainty, text_preprocessing_id, ordinary_consumer_id}. Subject domains are independent of CODE_Q/BUG/PLAN/ARCH/REFACTOR development intents. UNKNOWN and MIXED are first-class; low confidence abstains or branches lawfully, not a disguised confident choice. StackDetector acts on unaltered input before normalization that destroys C#/TS/PY punctuation.

VersionedSemanticIR = {ir_schema, source_span_refs, typed_operators, negation_scope, quantity_and_units, nullable_or_null3_logic, ordered_dependencies, type_relations, security_constraints, domain_laws, provenance, unknowns}. A semantic token is a typed symbol or relation; BPE id is a text encoding; float feature vector is an encoded geometry; LoRA is a learned low-rank adapter to a verified matrix; they are NEVER synonyms.

VectorSpaceIdentity must include subject/intent coverage, parser/law schema, BPE/tokenizer hash, embedding/feature normalization versions, dimensions, projection/adapter recipient+version/hash, output decoder and index build/version. Two equal-length arrays need not live in the same coordinate system. Update geometry -> either proven compatible transform or rebuild/reindex with simultaneous memory peak/rollback, NEVER mix silent stale cache. The output target is a typed output and association refs, not an untyped hallucinated number.

## Algorithm, in operational order

1. Parse source/scope/permissions and raw stack markers. Deterministic laws, allowlisted tools and authority checks are immutable constraints.
2. Predict (subject, intent, stack/version) with multi-label/unknown support; attach its confusion hypothesis. Keep oracle-domain only for a diagnostic arm: a system receiving true domain in production cannot claim predicted-mapper success.
3. Extract typed IR tokens and semantic features, preserve source span and law binding, produce versioned embedding. A new learnable projector must name actual W0, input/output dims, low-rank A/B if used, objective, update owner and caller; neither changed hash nor parameter size proves law preservation.
4. Eligibility A: rights/provenance, train/dev/sealed split, vector-space compatibility, domain conditioning with fallback/uncertainty, memory role (positive/negative/warning). Action admissibility B: type, preconditions, arbiter prohibition, version constraints. A negative neighbor may be retrievable as evidence but must never be executed as valid action.
5. Retrieve candidates with deterministic exact index and observed query budget. If ANN needed, run HNSW cosine over legal compatible space and measure recall against exact; keep ANN approximation as separate factor. Do not insert a query-dependent asymmetric nonmetric learned score directly as HNSW metric.
6. For each legal candidate n calculate interpretable features φ(q,n) (semantic similarity, typed operator/law agreement, intent/stack compatibility, memory role, recency only when lawful). Candidate domain-conditional ranker s(q,n,d;θ)=sum_j w_j(q,d;θ)*φ_j(q,n). Hard legal checks precede and follow ranker; weights must be well-defined with normalization and calibrated confidence. Fixed weights are a deterministic baseline, not learned.
7. Choose query-conditioned k(q,d;θ), radius, confidence, cost within available candidate count and resource envelope. Learnable only when there is named parameter state θ, training signal, update+provenance, saved/reloaded checkpoint, and measured behavioral effect. Otherwise call it a deterministic adaptive heuristic, not learned dynamic kNN.
8. Typed neighbor fusion: do not only return hits[0]. For each neighbor return {id, role=positive/negative/warning, source_ref, law evidence, score, output_type, confidence, canonical coordinate version}. Resolve typed associations when output type supports them; for single-result caller choose one lawful output using aggregate evidence and show why others influenced decision. Conflicts/ambiguous inputs route to arbiter/clarification or abstain. The authorized final arbiter governs adoption.
9. Emit diagnosis of route/neighbor choice/consumer result, lawful user correction or independent rubric, and candidate learning signal. Store exact provenance, one-time pair journal where relevant. Training owner remains existing ILearningMachine/leg; do not silently add a new trainer to a unit.
10. Only adopted states are used by cold ordinary inference: load exact model, projection, index, ranker version, law schema and parent; mismatch -> explicit incompatible state or rollback. Do not claim learning because a unit can compute a score.

## What can actually learn, separately

| Component | Frozen deterministic contract | Candidate change θ/signal/owner | Outcome that proves usefulness |
|---|---|---|---|
| Domain mapper | Raw stack, allowed domain set, uncertainty/abstain, law invariants | Learned domain/intent mapping under labelled lawful source-family outcomes; actual mutable owner to be found | Lower subject-domain confusion and critical wrong-law routing on UNSEEN families; oracle vs predicted gap transparent |
| IR/representation | Exact typed law tokens, tokenizer/space identity, decoder | Learned low-rank projector/weights only at identified matrix; independent matched objective, not "LoRA means token" | Same solver/index, fewer semantically wrong collisions, correct unseen law transfer and migration cost |
| kNN scorer | eligibility rights/split/space, arbiter authority | Updatable domain/query weights or low-rank reranker with actual feedback and compatible memory | Rank flips improve final answer/law at same legal candidate pool, persisted after reload |
| k/radius policy | k in [0,eligible_count], latency/host budget, no illegal evidence | Updatable query-adaptive neighborhood confidence/budget | Improves final task quality/latency at fixed scorer/corpus, not just recall@k |
| typed fusion | ILogicalUnit contract, OutputRefs/associations, negative vs warning semantics | Possible trained fusion only if actually installed owner, otherwise deterministic lawful combination | More than top-hit has causal value at real final consumer, even when chosen output is one |
| E1 Student / DPO | exact x/y+/y−, CODEX selection/judge, permitted Chinese author and response-only objective | RP01 separate Student LoRA update | Measured P/C0/T gain independent of E2 retrieval; E2 cannot inherit an E1 PASS |
| Flow/Orchestrator/Graph | Typed correction, async legality, higher-order pattern authority | RP03 / RP04 learnable state | Their own isolated independent consumer/retention tests, not E2 success |

## First-class counterexamples

- C# s?.Length means nullable int; s?.Length ?? 0 means int with zero fallback, not an equivalent example. Type/nullability must survive IR and neighbor comparison.
- SQL col = NULL is not IS NULL; SQL NULL is three-valued, C# null is not identical. SQL COUNT(column) ignores NULL while COUNT(*) counts rows. Identical words must not force cross-domain nearest hits.
- "Do not send before approval" cannot match a translation that drops negation; a negative memory can warn but cannot supply a chosen answer.
- Angular version-specific lifecycle order cannot be inferred only from "component"; unknown version is UNKNOWN requiring evidence/clarification.
- Mixed SQL snippet inside C# string needs multi-domain separation; do not silently route based only on keyword count.

## Design decisions that require real measurement

- Fixed vector distance vs learned rerank; change one at a time.
- k=1 first-hit vs candidate-set + actual multi-neighbor evidence; the known KnnLogicalUnit code is a candidate seam but only after tracing callers.
- Learned embedding geometry must re-retrieve exact set in its own geometry; do not lock old nearest IDs and claim changed geometry.
- HNSW cosine and exact cosine have separate candidate sets at same corpus/space; ANN omission is measured explicitly.
- Full matched-resource deterministic-only, optional learned-only, and hybrid arms all retain laws/rights; optional missing arm limits claims, not all E2 progress.
