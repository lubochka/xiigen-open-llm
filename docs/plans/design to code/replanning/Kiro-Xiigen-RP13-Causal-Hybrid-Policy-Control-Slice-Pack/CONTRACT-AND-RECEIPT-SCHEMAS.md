# RP13 — minimal trace, hypothesis and results contracts (illustrative schemas)

Эти JSON формы — specification, **НЕ** реализованные K DTO/API. Использовать существующие в K структуры и совместимые extension metadata прежде чем создавать новые.

## One decision record (keys, not verified live fields)

`decision_id, parent_request_id, timestamp, P_branch_SHA, K_branch_SHA, subject_ids[], intent, stack_version, raw_input_ref, source_family, law_version, legal_eligibility, rights_scope, tokenizer_version, feature_schema, embedding_projection_version, candidate_pool_hash, candidate_output_refs[], vetoed_candidates_and_reasons[], selected_candidate, k_policy_and_neighbors_used[], unit_flow_orchestrator_graph_links[], student_state_id, route_policy_state_id, frozen_state_ids[], actual_consumer_method, ordinary_result_ref, functional_result_label, cost_total, missing_trace, attributable_component_or_UNKNOWN`.

Store source references/hashed identities, not unauthorized private raw user input or cert ground truth. Historical user case data cannot be silently repurposed for TRAIN.

## Hypothesis card

`hypothesis_id, source_anchor, suspected_component, competing_causes[], one_intervention, frozen_cofactors[], permissible_paths[], target_state_owner, proposed_update_objective, effect_grant_id_or_NONE, intended_candidate_family, expected_lawful_delta, max_resource, stop_and_rollback, independent_falsifier, likely_interactions, evidence_status`.

Potential target classes: deterministic repair `C0_ONLY`, Student exact DPO (owner RP01), mapper/vector/k policy (RP02), Flow (RP03), Graph route (RP04), Builder (RP07), RP13 selector if approved. Never route DPO pair directly to an arbitrary component without actual representable signal and operator.

## Per-task work receipt

`task_id; active_repos+refs+blob_hashes; source_depth(rows); skill_read_receipts(role,sections); original_requirement_ids; actual_call_chain; REUSE/IMPLEMENT/NEEDS_SOURCE decision; permissions/effect ledger; changed_path_diff; good_bad_edge_measured_or_hypothetical; actual command+coworker+exit or NOT_RUN; P/C0/T exact identities and law rubric (if any); proper-domain/path/utility and costs or NOT_MEASURED; raw experiment case refs; falsification attempt; human-readable Russian conclusion; separate reviewer identity/candidate SHA/NOT_PERFORMED; next_owner_trigger`.

## Worked classification (hypothetical only)

- GOOD: Student given valid C# retrieval makes wrong syntax claim although correct domain/Unit/Flow path; policy cannot fix generation error by raising k. Attribution `Student candidate`, current K owner RP01, require independent contrast and grant.
- BAD: All errors blamed on vectorizer because nearest neighbors "look wrong"; changing k and graph simultaneously then claiming route policy learned. This is not causal.
- EDGE: correct result on two different lawful paths; cost differs, law confidence uncertain; choose abstain or cheaper route with uncertainty, not relabel one path incorrect to manufacture preference.

### Falsification

A change that only writes `trace.json`, `chosen_state_sha`, test coverage or log lines without true ordinary behavior and changed admissible state is `MECHANISM_OBSERVED` at best. A learned claim must survive blinded source-family independent end-task comparison, no-update control, full-PC cost, cold-reopen and a separately reviewed exact candidate.
