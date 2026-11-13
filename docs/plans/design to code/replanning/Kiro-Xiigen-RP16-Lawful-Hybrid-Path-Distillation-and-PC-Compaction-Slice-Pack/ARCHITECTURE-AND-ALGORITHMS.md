# RP16 — deterministic/learned path distillation architecture

```text
ordinary user → RP02 [domain | intent | lang | dialect | version | rights]
 → RP02 [semantic IR/token/vector, lawful BM25+kNN dynamic multi-hit]
 → RP03 ILearningUnit<TInput,TOutput> typed many-to-many association
 → RP03 ILearningFlow typed correlation/input→output correction
 → RP04 ILearningOrchestrator bounded async alternatives/cancel/fan-in
 → RP04 ILearningDynamicGraph order-2/order-3 patterns + existing hard arbiter
      │                      │
      │ F: full lawful path  └── RP16 proposed mθ compact typed subpath
      │                            ↳ RP02 hard eligibility/versions/negative warning
      │                            ↳ RP04 existing runtime route executor, else F
      └─────────────── user answer via real ordinary consumer
                        ↓ RP09 blinded exam + full-PC cost
                      RP05 admitted trace/feedback → RP16 macro candidate state B1→B2
                        ↓ RP10 compatible cold promotion/rollback (if measured)
        optional INDEPENDENT RP01 Student chosen/rejected DPO→LoRA trial, not macro
```

ILearningUnit = simplified domain/vector/output-shape unit producing many→many typed results. ILearningFlow = correct associations between these results (not arbitrary zip). ILearningOrchestrator = async composition of typed flows with legal dependencies, cancellation and budgets. ILearningDynamicGraph = higher-order patterns and learned routing, under Machine infrastructure. **Training belongs to existing ILearningMachine**; no four new trainers. RP16 does not replace any of these, and RP11's causal credit ranks feasible alternatives, not a new RP16 policy learner.

**Proposed, not existing K API:** `LawfulTraceEvidenceV1` = source_family, rights/tenant, domain law passport + version, typed input/output and OutputRefs, ordered edges/node IDs, higher-order dependencies, graph/codec/kNN/Student version, arbiter verdict and all-branch cost. `CompactPathCandidateV1` = parent, admitted independent trace families, typed pre/postconditions, parameterized guarded subpath, forbidden shortcuts, version compatibility, positive/negative/warning counterexamples, full-path fallback ref, bounded resource, status DRAFT/SHADOW/CANDIDATE/ACCEPTED/REJECTED. Existing registry/cached mechanism must be sought first; a new class is justified only by a missing consumed seam.

`mθ(d,typed_x,law_v)` is an **admitted parameterized graph subpath**. New nonreplayed source-family observations must change θ/guards/conditions and alter lawful generalization on unseen cases relative to static S exact cache and corrected no-update C0. Storing an old response, increasing kNN count or a changed JSON hash alone is NOT learning. General semantic equivalence of arbitrary programs is not decidable; permit only bounded verified subpaths, preserve all graph dependencies and use F fallback on uncertainty.

Law/rights/type/version/side-effect arbiter filters candidates **before** any fuzzy cosine/kNN/credit score. Hard law cannot become a soft preference. Dynamic kNN similarity and LoRA matrix transpose are NOT domain tokenization; versioned learned projection into semantic IR requires RP02 owner + heldout meaningful examination. Macro cannot silently replace higher-order graph relations nor speed through non-idempotent parallel commands.

For eligible real candidate set `A(x)`, choose among those with `Law && Rights && Type && Dependencies && CostBound`; measure regret against best **enumerated lawful route within same budget** (not unknowable global optimum). A correct deterministic F may remain best and is a valid result.