# R12 DECISIONS — R12-ARCH-2, integrated A–F hybrid candidate (13.10.2026)

**Evidence level: architectural DESIGN + targeted current code traces; NOT an independent CODEX/GPT-6 Astra UltraCode review, an implemented feature or a measured learning result.** Historical R12-CP1 selection is downgraded; R12-REPAIR-1 rules stay effective. Exact path/read depth is in SOURCES.csv.

## Decisions retained from R12-REPAIR-1

- **D-R12-R01:** CP1 DPO findings are bounded first-seam evidence, not whole-program selection.
- **D-R12-R02:** primary-lineage/WorkLeft/RAG/flow decisions must be reconciled directly, sibling summaries alone are insufficient.
- **D-R12-R03:** **CODEX using GPT-6 Astra UltraCode performs delegated human case selection and separately assesses the pair; permitted external open Chinese model authors BOTH final chosen and rejected; Student and post-update evaluator separate.**
- **D-R12-R04:** Unit, Flow, Orchestrator, DynamicGraph require distinct state/signal/consumer/falsifier; no four trainers.
- **D-R12-R05:** existing DPO chain remains E1 candidate, not automatically selected by name.
- **D-R12-R06:** domain/intent-specific vector/token family, dynamic k and learned multi-neighbour consumer are required.
- **D-R12-R07:** deterministic laws + post-generation arbiters have final admissibility authority.
- **D-R12-R08:** historical 70/30 replay conflicts with current no-repeat/new-lesson evidence-policy.
- **D-R12-R09:** LoRA low-rank matrices and semantic ResultProjection are distinct.
- **D-R12-R10:** full PC footprint/growth and retention are primary constraints.
- **D-R12-R11:** test/Windows coverage ≠ learning utility; deterministic safeguards remain.
- **D-R12-R12:** design must integrate six bounded A–F parts before architecture selection.

## New explicit, candidate-bound decisions

**D-R12-A01 — scope of authority.** CURRENT for S04/new-lesson efficacy = one new admitted pair applied once, no hidden replay. DECISIONS-LOCKED D-06 70/30 is SCOPED_CONFLICT/HISTORICAL for this protocol, not globally deleted. Later replay comparison requires a new explicit owner decision and matched compute.

**D-R12-A02 — preserved non-negotiable primary laws.** StackDetector-before-normalization, semantic/order preservation, BM25+kNN design with truthful code consumer, positive/negative/warning history, final arbiter and shared base/TensorCore/LoRA remain constraints subject to versioned evidence. Old 20×/2MB/95% claims are historical targets/measurements, not current observed gains.

**D-R12-B01 — one trainer infrastructure, four target behaviors.** ILearningUnit typed many→many, ILearningFlow relations/corrections, ILearningOrchestrator async admissible path selection, ILearningDynamicGraph higher-order transfer. Each learned policy has a target parameter group/state manifest and ordinary consumer. Solver update does not certify Flow/Graph learning.

**D-R12-B02 — additive typed compatibility.** Keep implemented ILogicalUnit<TInput,TOutput> and LogicalUnitResult<TOutput>; propose versioned many-association adapter with stable ordering and association IDs. Do not rewrite existing interface or claim many→many current implementation by name.

**D-R12-B03 — runtime authority.** Eligibility/law/type/security, branch ordering and arbiter verdicts stay deterministic; learned scores cannot overrule denial. A learned score may rank only lawful candidates; correct deterministic solutions remain positive.

**D-R12-B04 — graph status.** Current DynamicGraphMachine training is explicitly NOT_ACTIVATED; available edge resolver/execution components do not establish trained graph transfer or saved policy adoption. R22 owns minimum update+preload+ordinary consumer.

**D-R12-C01 — immutable judged-to-trained pair.** No Compact/whitespace semantic mutation/truncation or unassessed projection between Chinese-authored judged bytes/context and optimizer. Pair passport binds raw/context/token boundaries and codebook; oversize means lossless compatible scheme or fresh judgment. Current 18.CapabilityTraining BuildBatch violates target; R18 fixes it, not a parallel trainer.

**D-R12-C02 — conditional DPO is explicit.** For chosen/rejected responses to shared x, use response target masks and sum response log-probabilities against a frozen parent reference with identical forward/backward reduction, EOS/pad policy. Current 11.DPO masked full-sequence mean is a code-traced mismatch. Do not silently change public average inference scoring API.

**D-R12-C03 — separated admissions/outcomes.** CODEX/Astra human-role selector ≠ Chinese pair author ≠ CODEX/Astra exact-pair judge ≠ trainer permit ≠ post-update evaluator. TIE/BOTH_BAD/UNJUDGEABLE invalidate preference admission; no repeat pair; UNKNOWN mutation requires journal recovery; accepted checkpoint must reach normal consumer.

**D-R12-D01 — domain identity is a tuple.** Domain family, intent, project, stack, language pair, source rights and output role are separate fields. CODE_Q/BUG/PLAN/ARCH/REFACTOR enum is not complete list of product learning domains. Predicted/oracle domain variants + OOD/mixed cases required.

**D-R12-D02 — vector/retrieval policy.** Preserve raw/source, domain-specific law-sensitive IR and ordered tokens, versioned tokenizer/codebook/vector/index. Deterministic eligibility first; BM25/ANN candidate generation; calibrated learned rerank/aggregate and warning-only memories; final arbiter after solution. Domain weights and k are learned only when they receive a valid signal/update and change a real multi-neighbour consumer.

**D-R12-D03 — kNN experiment.** Current KnnLogicalUnit has fixed k and first-hit output. Dynamic k is a budget policy constrained by lawful pool, confidence/diversity/time; exact-search diagnostic separately from HNSW, query-dependent learned scoring in post-ANN reranker unless index assumptions proven.

**D-R12-E01 — semantic projection contract.** LoRA is A/B adaptation, not tokenizer nor semantic projector. Any ResultProjection_d must bind typed source→target domain vector/codebook, objective, frozen base, parameter group, consumer, version migration, full rollback and law-sensitive held-out effects.

**D-R12-E02 — actual PC costs.** Measure model/reference/optimizer/activations/KV, adapters, tokenizers, graph/memories/index/reindex, cache, datasets/snapshots, teacher+CODEX judge+evaluator, latency and growth each new bulk. Tiny checkpoint alone cannot establish compact runnable continual learning.

**D-R12-F01 — first candidate with falsifier and fallback.** **Conditionally select E1** exact DPO→real Student state→ordinary code documentation consumer, because code-traced optimizer and defects create a short falsifiable path. It is NOT_EXECUTION_READY until R18 verifies and wires *ordinary* generation adoption and fixes objective/bytes; R13 must fix the filled experiment card. If ordinary consumer cannot be bounded in R18, select E2 lawful multi-neighbour rerank as pre-declared fallback; do not change winners post-hoc.

**D-R12-F02 — causal claims separate.** E1 cannot prove Flow/Orchestrator/Graph; E2 needs mapping/representation/consumer; E3 freezes Units; E4 freezes lower-level policies on unseen lawful compositions. Best observed route is not global optimum. Preserve all baseline domains and extras.

**D-R12-F03 — disposition.** A–F are integrated as a **candidate architectural design** rather than postponed prompts. R13/R18 own runnable exact experiment binding, R21/R22 other-domain/algorithm exams, R23 PC/retention and R19/R20 plan/effective executor migration. R15 independent CODEX/Astra review is NOT_PERFORMED and cannot be self-awarded.

## Remains intentionally unresolved (named, not hidden)

1. **R18:** current-head selected CodeDocumentation normal caller actually loading promoted Student policy; full durable snapshot/recovery, exact recipient/optimizer state, semantic pair objective code patch.
2. **R13:** current STATUS OVERRIDE REPAIR_REQUIRED and missing filled first frozen experiment card; thresholds/per-domain source splits must be chosen before results.
3. **R22:** actual many→many adapter, law-sensitive per-domain vectorization and multi-neighbour learned state, nonmetric index compatibility, Flow/Orchestrator state and DynamicGraph training+ordinary adoption.
4. **Owner decision only if used:** whether/how legacy 70/30 replay may be reintroduced in later separate continual-learning studies.
5. **Astra reviewer:** separate CODEX/GPT-6 Astra UltraCode invocation on this *new* candidate; no PASS transfer from R15 parent review.
