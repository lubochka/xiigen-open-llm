# RP04 F28–F30: fresh source findings and learning-first repairs

**15 October 2026 · verified bounded code/source audit, PLANNING ONLY.** Extends F17–F27 without creating a new task, Rxx round, model call, engineering-test suite, or code change. Core source branch lubochka/llm_mvp_core@codex/dpo-implementation, inspected HEAD **025c42c74b1d30ffa4660f9824a52cf8fcaf7c88**; refresh exact SHAs on execution. Twelve **existing physical task cards** RP04.00A–05A were repaired IN PLACE, preserving first lesson ASAP.

## F28 · AxisSweep configuration preference is not yet learned arbitrary graph routing

**CODE_VERIFIED_IN_THIS_SOURCE_PATH; other code paths NOT exhaustively audited.**

- K/21.AgentInfrastructure/Logical/AxisSweepCombinationStrategy.cs blob **9720de08246190fe9696115f78d9cba77178c94d**, lines 12–14,45–95: Cartesian product of ONE node, ONE edge key and four fixed flag profiles. Lines 82–87 create NodeSetIds:[nodeId], EdgeOrderKey and Flags. Lines 161–180 create the four profiles.
- K/21.AgentInfrastructure/Logical/BoundedCombinationGraphCandidateRunner.cs blob **8099c2c68d3977f61ddf15ada77b89451f4a499a**, ValidateAndCopyPlans lines 425–472 requires **NodeSetIds.Length == 1**.
- K/15.OrchestratorModel/Logical/DynamicGraphCandidateInvocationOrchestrator.cs blob **b66c9b50df9be24b2619995e8101fcb41283d00d**, CreateAxisSweep lines 99–150 constructs this strategy.

**Decision:** one genuinely observed lawful pair of flag configurations can teach a named preference parameter, giving useful **CONFIG_PREF_STATE_CHANGED** evidence after cold ordinary consumption. It does NOT itself establish **E3_STRUCTURAL_ROUTE_LEARNED** (different multi-step paths), nor **E4_ORDERED_MOTIF_LEARNED** (order-2/3 directed semantic dependencies). The first honest theta update is encouraged; broader path-generation functionality remains explicitly owned follow-up. Do not delay first learner for all domain graph work.

**GOOD:** two FULL_CLEAN_OBSERVED same-input legal results with different flagged configurations; report parameter old→new and typed end-user output, clearly labelled configuration preference. **BAD:** four flag profiles interpreted as four different multi-hop paths, or an unlaunched/failed candidate used as observed rejected outcome. **EDGE:** one lawful candidate or unknown actual caller => NO_VALID_PAIR or SOURCE_GAP, not invented proof; E4 lane independent. **Falsifier:** verify code:method:line and ordinary typed trace proving structurally distinct multi-node routes actually produced by this same code path; narrow finding when proved.

## F29 · Structurally valid batch does not itself prove individual candidate law admission

**CODE_VERIFIED_LOCAL_ABSENCE; downstream security impact UNPROVEN, not a confirmed exploit.**

- K/00.Core/DynamicGraphContracts.cs blob **987c29396fab838db0eecd8060837d5e5e33bce4**, lines 39–64 defines GraphNodeCandidate.LegalStatus, Status, ErrorCodes, PathOrderRef, source/provenance and OutputRefs. Lines 573–600 define GraphCandidateOutput and AsyncCandidateBatch.
- K/15.OrchestratorModel/Logical/DynamicGraphCandidateInvocationOrchestrator.cs blob **b66c9b50df9be24b2619995e8101fcb41283d00d**, lines 440–516 requires complete clean batch Status="OK", wrapper IDs/refs, flattened candidates and candidate UnitId. **No explicit check of each inner LegalStatus or Status in this inspected span.**
- K/21.AgentInfrastructure/Logical/DeterministicDynamicGraphNodeOutputProjector.cs blob **eced7aa0b4b917d603f31905fab2b5f09ef5971e**, lines 34–84 validates unique CandidateIds then SORTS references into an order-free projection; this method does not establish candidate law or order-2/3 execution.

**Decision:** trace actual normal host through inner candidate → domain law/tenant/consent mask → typed materialization and selected target → D-15 final arbiter. Distinguish STRUCTURAL_BATCH_OK, LAW_ADMITTED_CANDIDATE, and LAWFUL_FINAL_EFFECT. Preserve existing PARTIAL_FAILURE fail-closed handling. Local method omission does not prove entire system has a vulnerability.

**GOOD:** high-scoring forbidden inner candidate is actually denied by final authorized guard, with method/line evidence. **BAD:** batch OK/unique ID accepted as sufficient law; guard weakened to pass demo. **EDGE:** downstream guard unlocated ⇒ GUARD_UNPROVEN + no unsafe effect, exact next source method. **Smallest check:** ZERO product tests for source read. If changed deterministic admission/consumer code, one focused accepted lawful typed scenario and one forbidden/foreign-tenant/stale denial; no coverage quota. **Falsifier:** exact normal downstream caller enforces individual candidate rights/laws, proven by code and relevant real result.

## F30 · Fixed-pool theta ranking cannot prove fresh-domain best-path discovery

**SCIENTIFIC_DESIGN_RISK, NOT a verified product bug.** One observed θ preference may improve selection among already available candidates, even when no correct lawful solution is ever proposed for a new domain. Distinguish two examination questions without adding Windows tests:

**A. FIXED_POOL_C0_U (isolated policy score):** C0 and U use SAME corrected software, SAME real cold user caller/flags/laws, SAME candidate_pool_hash, RP02 domain/role token-vector/kNN/LoRA projection, RP03 typed Unit/Flow, frozen opposite lane and PC budget. C0 ZERO update, U ONE named θ update. Capture true selected path/class, lawful typed TOutput, raw valid/N, critical error veto, paired uncertainty, all launched/failed/lost/cancelled branch costs. If independently verified best lawful path exists in pool, record ORACLE_IN_POOL and optional regret. If absent ORACLE_UNREACHABLE. If oracle unknown ORACLE_UNKNOWN—do not invent ranking regret. Fixed pool ranks; it cannot discover a missing path.

**B. FRESH_POOL_E2E (domain mapping/generation):** unseen source-family-disjoint heldout cases, new lawful domain/pattern mapping, candidate generation and safe typed end-user output with matched whole-PC budget. If independent oracle known, measure legal_best_path_present/N, else lawful final answer/N and warranted abstention. Report macro and worst-domain, uncertainty across stochastic paired repetitions, total time-to-valid answer; include index/graph/checkpoints, CPU/GPU/RAM/VRAM and cancelled/losing branches with real host sensors, missing values NA. End-to-end changing pools is NOT the same isolation as A. No heldout tuning. Post-first-lesson ablations compare D deterministic-only, L learned-only when lawful, H hybrid; RP02 law-adaptive kNN/role tokenization, RP03 Unit/Flow, RP18 LoRA→semantic projection remain separately owned.

**GOOD / BAD / EDGE domain examples:**
- CODE C# nullable/SQL: GOOD NullGuard→ParameterizedBind→safe typed proposal; BAD Bind before Guard/interpolated SQL; EDGE absent dialect ⇒ abstain rather than invent.
- LANG EN→RU: GOOD syntactic role+grammatical agreement+referent ID with legitimate masculine antecedent switch; BAD nearest-neighbor female→ban any masculine pronoun; EDGE ambiguous coreference ⇒ multiple lawful hypotheses/clarify.
- ID synthetic: GOOD async identity+consent and expiry check before single effect; BAD high-score expired authorization; EDGE consent revoked after fan-in ⇒ zero effect, retain branch costs (never claim real statutory proof).
- DTC: GOOD accessible requested HTML/CSS output and independent designs; BAD visual-only inaccessible result/wrong stack; EDGE unlicensed design ⇒ no training.
- MIXED/UNKNOWN: GOOD composition of compatible source-backed law passports or abstention; BAD raw cosine neighbor or flag count bypasses rule conflicts; EDGE incompatible laws ⇒ safe fallback.

**Mandatory raw exam record:** experiment_id, source_family/split, domain/law/source-version/rights, input and typed output expectation, K code/θ/checkpoint identity, role/vector/token/kNN/LoRA, RP03 Unit/Flow, graph/order/axis class, candidate/pool hash, oracle state, inner LegalStatus, actual ordinary result and arbiter, numerator/N and severe violations, uncertainty/abstentions, all PC branch costs or NA. B0→B1→B2 retention only after real authorized learning bulks; a two-domain first pilot is STATE_CHANGED/ORDINARY_USED at most, not general gain.

## Task ownership and fastest route — NOT new rounds

Existing cards 00A/00B inspect source/choose writer; 01A/01B/01C obtain full observed legal alternatives→one real numeric θ update→cold user config or actual E3 path, labelled honestly. 02A/02B/02C separately establish ordered typed motif and learned θmotif, not flag sweep. 03A prepares legally permitted response DPO through CODEX HUMAN selector → approved open Chinese BOTH final responses → a DIFFERENT CODEX exact-pair judge; response DPO is distinct from observed path/motif pairwise gradient. 03B owns A/B exams. 04A binds consented feedback to executed not imagined path, persists once and cold rollback. 05A needs actually invoked independent exact-SHA CODEX Astra UltraCode reviewer; writing this prompt is NOT invoking it.

**Critical path unchanged:** 00A→00B→(01A→01B→01C OR 02A→02B→02C)→03B, 03A parallel, 04A/05A after first lesson. **Engineering minimum:** docs/ADR/science/NO_CHANGE => ZERO new product tests; modified deterministic security/type/cancellation/idempotence/rollback only smallest relevant lawful+forbidden probe and touched build if needed. No Windows Actions/coverage percentage as learner-quality criterion; real separate native release/security rules remain.

**Status:** F28 SOURCE_VERIFIED_LOCAL, F29 SOURCE_VERIFIED_LOCAL/END_TO_END_UNPROVEN, F30 SCIENTIFIC_RISK; RP04 12 planning cards updated. PRODUCT_CODE_UNCHANGED_BY_THIS_REPAIR; LEARNING_NOT_EXECUTED; DOMAIN_EXAMS_NOT_RUN; PC_NOT_MEASURED; INDEPENDENT_ASTRA_NOT_PERFORMED.
