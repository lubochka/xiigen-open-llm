# RP08 architecture — lawful deterministic shell + trainable state in existing engines

## 1. Purpose and four distinct roles
**ILearningUnit**: input domain + vector specification + requested typed output → multiple independently associated results (`IEnumerable/IQueryable` conceptually; actual C# request/result interfaces must be inspected); deterministic port/schema/law admission; learned transformation or ranking owned by **existing ILearningMachine**, not new Unit.Train().
**ILearningFlow**: typed relationships/corrections between Unit outputs and downstream inputs; fixed port/identity validation + separately attributable correction/edge policy, not positional Zip.
**ILearningOrchestrator**: lawful async candidate schedules, branch limits, cancellation, best bounded viable path; deterministic safety and effect reservations + learned admissible route ranking.
**ILearningDynamicGraph**: order-2/order-3 typed, ordered relation patterns and transfer to unseen lawful compositions; deterministic graph schema/authority + versioned learned motif preference. A DPO `Level=unit/UnitId` payload does not magically train all other three roles. Preserve D-15 final arbiter.

## 2. Proposed *integration* envelope — adapter contract, not claimed existing types
```text
DomainSignal {
 subject, intent, stack, language, source_family, project_version,
 law_profile_id, rights_id, confidence, UNKNOWN|MIXED
}
SemanticRepresentation {
 domain_signal_id, law_version, coordinate_schema_id, tokenizer_version,
 source/content identity, typed slots (units, negation, order, AST/DOM/SQL operators),
 projection_version, LoRA_state_id?, embedding/index_compatibility
}
CandidateSet {
 query_id, input_association_ids, output_schema, eligible_positive_hits[],
 warning_or_negative_hits[], law_inadmissible_hits[], scoring_policy_id, adaptive_k,
 scores/provenance, uncertainty, status
}
HybridRoute {
 admitted_typed_units, flow_links, async_branch_plan, graph_patterns_order_2_or_3,
 deterministic_policy_version, per_role_state_refs, arbiter_receipt, total_branch_cost
}
ColdUserResult {
 selected_state_id, real_host_request_id, resulting_html_css_or_typed_answer_ref,
 domain_law_verdict, quality_receipt, latency_cost_receipt, feedback_consent_ref?
}
```

## 3. Algorithm — smallest incremental integration (design pseudocode)
```text
signal = map_subject_intent_stack_language_version(x, law_catalog)
if rights invalid or unknown required law -> SAFE_ABSTAIN / ASK / lawful deterministic fallback
ir = encode_preserving_law_slots(x, signal, domain_coordinate_version)
pool = retrieve(rights_and_split_eligible(ir), same_metric_space_and_law, exact_or_ANN)
negatives = warning_pool_same_context(ir)     # read warning, NEVER execute as good action
candidates = rerank_and_select_k(pool, negatives, per_domain_query_cost_policy)
typed_outputs = learning_unit.many_to_many(x, candidates)     # preserve associations
corrected = learning_flow.correct_links(typed_outputs, typed_versions)
routes = deterministic_admit(orchestrator.async_plans(corrected), permissions, resources)
graph_options = deterministic_admit(graph.order2_order3(routes), schema, law, D15)
chosen_path = learned_rank_only_within_admitted(graph_options, state_versions)
run_and_trace(chosen_path, permitted bounded branches)
result = arbiter_accept_or_reject(real_ordinary_consumer_output)
if authorised_feedback: immutable_feedback_intake(result)  # RP05; NOT auto-DPO
```
**Dynamic kNN:** `k(x, domain, difficulty, confidence, law, latency_budget)` is a hypothesis; compare fixed-k, learned rerank, dynamic-k and multi-hit aggregation under *same* source pool and law/metric space. Hard law is eligibility, not a negative cosine score; warning memory is visible but not a candidate action. Same vector length does not mean comparable coordinate system.

**Vector/token/LoRA:** semantic IR/codebook used for retrieval and typed decisions ≠ actual Student BPE token IDs/masks ≠ LoRA trainable tensor. Record explicit source→IR→vector/index version→Student actual segmented-byte-BPE/masks→parameter adapter→output projection mapping. Reject lossy `Compact` that changes meaning or the exact judged pair in a live DPO recipient, but determine actual caller first. Evaluate negation/quantity/before-after/NULL and responsive viewport as adversarial law distinctions; full coordinate migration requires versioned index rebuild and cold compatibility.

## 4. Evidence ladder and writable state
Record `name; state owner; version; signal; writer; no-update; frozen baseline; changed bytes; cold reader; actual output; evaluation family` separately for domain mapper, vector encoder, k policy, Unit, Flow, E3, E4, Student/LoRA. A deterministic implementation of a *trained policy* is still learned if state truly changes from admitted evidence. Conversely random/exploratory branch selection or additional tests prove nothing.

## 5. Exact DPO role and recipient control
CODEX GPT-6 Astra UltraCode selects a lawful new weakness/task (delegated human choice); an actually authorised external open Chinese model authors BOTH final same-x chosen and rejected; **a separate CODEX Astra UltraCode evaluation** judges frozen exact pair context and bytes (GOOD/BAD/TIE/BOTH_BAD/REJECTED_BETTER/UNJUDGEABLE, rights, recipient, semantic slots). Student under admitted `ILearningMachine` owner applies one unique pair once (parent/ref policy frozen, response-only objective verified, optimizer/RNG snapshot). Separate blind examiner compares corrected no-update N to changed U on source-family heldout. Learned Unit/Flow/route/graph policies have their own named state/credit and controlled interventions; pair author's identity is not a score.
