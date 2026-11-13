# Layer 1 — Transition Orchestrator Pairs

```
Contract:
  LogicalTransitionOrchestrator : ILogicalOrchestrator
```

## Common coverage labels (apply to every pair in this file)

```
source_license_state: external_license_checked
contract_signature:   LogicalTransitionOrchestrator
node_role:            layer_1
pair_kind:            transition_orchestrator
coverage_status:      covered
attribution_required: false   (overridden to true on ES-S8 pair only)
```

Per-pair labels carry: `source_id`, `source_title`, `transition_type`,
`phase`, `sub_phase`, `task_type`, `rejected_type`, `overlap_ratio`.

Token-overlap gate: every rejected satisfies overlap_ratio < 0.60.

The A side is the real user prompt as the orchestrator receives it. The B
side is the selected transition with the full candidate set, including
rejected candidates with their rejection reasons.

Six pairs in this file — one per scenario where transition selection is
non-trivial. ES-S3 (testing) and ES-S8 (CODE_Q explanation) collapse into
the same `feature_addition` and `explain` candidate selection patterns
covered by ES-S4 and ES-S2 respectively, so they are not repeated here.

---

## Pair[ID] ES-2026-05-08-3.1-L1-01 — ES-S1 feature_addition (boundary topology)

A:
```
run_id:       "ES-S1-training-001"
scenario_id:  "ES-S1"
user_request: "We need to define the component boundaries between the order service and the inventory service. The order service must own the order lifecycle and the inventory service must own stock state — neither service may read the other's storage directly."
flags:
  UseLearnedUnitWhenAvailable: true
  UseKnowledgeEdgeDetection:   true
```

B:
```
selected_flow_id: "transition.feature_addition"
overall_verdict:  "accepted_for_expert_split"
candidate_set:
  - transition_type:  "feature_addition"
    confidence:       0.84
    normalized_A:     "current architecture has no defined ownership boundary between order service and inventory service"
    normalized_B:     "architecture has explicit boundary rule — order service owns order lifecycle, inventory service owns stock state, neither reads the other's storage"
    transformation_X: "introduce ownership boundary, dependency direction, and forbidden cross-storage rule"
    selected:         true
  - transition_type:  "repair"
    confidence:       0.31
    rejection_reason: "the prompt does not name a defect; it adds a missing architectural rule that did not exist."
    selected:         false
  - transition_type:  "external_source_integration"
    confidence:       0.17
    rejection_reason: "no external source is being ingested; arc42 and ADR are authority for the chosen content but the transition does not integrate them as system data."
    selected:         false
```

Reason:
```
[WRONG: ES-S1 introduces a new architectural rule — ownership boundary —
that did not previously exist in the system. arc42 §5 (building-block view)
and ADR Decision sections both describe this kind of work as a structural
addition, not a fix. The rejected selects "repair" because "boundaries"
sounds remedial, but no defect is named. Repair applies when existing
behavior is broken — boundary definition is feature_addition.]
```

Phrasing-B (Type A — wrong transition_type):
```
selected_flow_id: "transition.repair"
overall_verdict:  "accepted_for_expert_split"
candidate_set:
  - transition_type:  "repair"
    confidence:       0.62
    normalized_A:     "broken boundary between services"
    normalized_B:     "repaired boundary between services"
    transformation_X: "fix the missing rule"
    selected:         true
```

Coverage labels:
```
source_id:       arc42-template, ADR-template
source_title:    arc42 + ADR (boundary topology)
transition_type: feature_addition
phase:           3
sub_phase:       3.1
task_type:       architecture-topology
rejected_type:   A
overlap_ratio:   0.39   (Type A: chosen feature_addition vs rejected repair; transformation_X token sets partially shared)
```

---

## Pair[ID] ES-2026-05-08-3.5-L1-02 — ES-S2 feature_addition (observability strategy)

A:
```
run_id:       "ES-S2-training-001"
scenario_id:  "ES-S2"
user_request: "Design what to measure and alert on for our payment processor service. We need to know which workflows are running, which retries are accumulating, and where compensation activities fire."
flags:
  UseLearnedUnitWhenAvailable: true
  UseKnowledgeEdgeDetection:   true
```

B:
```
selected_flow_id: "transition.feature_addition"
overall_verdict:  "accepted_for_expert_split"
candidate_set:
  - transition_type:  "feature_addition"
    confidence:       0.81
    normalized_A:     "payment processor has ad-hoc telemetry; no defined signal set or alert thresholds"
    normalized_B:     "payment processor has explicit signal catalogue: workflows running, retry counts, compensation fires; alert thresholds bound to each"
    transformation_X: "introduce observability signal catalogue and alert threshold table"
    selected:         true
  - transition_type:  "external_source_integration"
    confidence:       0.34
    rejection_reason: "Temporal/Dapr documentation is consulted as authority but the work itself is internal observability strategy, not a new external integration."
    selected:         false
  - transition_type:  "topology_mutation"
    confidence:       0.22
    rejection_reason: "the topology of services is not changing; only what is measured about them."
    selected:         false
```

Reason:
```
[WRONG: ES-S2 adds new observability strategy — a feature the system did
not previously have. The rejected selects topology_mutation because
"workflows" suggests structural change, but the topology of services is
unchanged. Only the measurement layer is added. Topology mutation applies
when services are added, removed, or have new ownership; here the same
services gain telemetry. feature_addition is correct.]
```

Phrasing-B (Type A — wrong transition_type):
```
selected_flow_id: "transition.topology_mutation"
overall_verdict:  "accepted_for_expert_split"
candidate_set:
  - transition_type:  "topology_mutation"
    confidence:       0.58
    normalized_A:     "service topology without telemetry workflows"
    normalized_B:     "service topology with telemetry workflows"
    transformation_X: "add telemetry as new topological component"
    selected:         true
```

Coverage labels:
```
source_id:       temporal-observability-docs, dapr-observability-docs
source_title:    Temporal + Dapr observability
transition_type: feature_addition
phase:           3
sub_phase:       3.5
task_type:       observability-design
rejected_type:   A
overlap_ratio:   0.41   (Type A: chosen feature_addition vs rejected topology_mutation; transformation_X tokens partially shared)
```

---

## Pair[ID] ES-2026-05-08-3.4-L1-03 — ES-S5 feature_addition (JWT + tests)

A:
```
run_id:       "ES-S5-training-001"
scenario_id:  "ES-S5"
user_request: "Add JWT bearer authentication on the API endpoint per RFC 7519 and write integration tests for tenant isolation across the authentication and API layers."
flags:
  UseLearnedUnitWhenAvailable: true
  UseKnowledgeEdgeDetection:   true
```

B:
```
selected_flow_id: "transition.feature_addition"
overall_verdict:  "accepted_for_expert_split"
candidate_set:
  - transition_type:  "feature_addition"
    confidence:       0.88
    normalized_A:     "current API has no tenant-aware JWT authentication and no integration tests for tenant isolation"
    normalized_B:     "API has tenant-aware JWT bearer authentication per RFC 7519 and tenant-isolation integration tests"
    transformation_X: "add authentication model, API contract constraints, implementation branches, integration tests, release review"
    selected:         true
  - transition_type:  "repair"
    confidence:       0.41
    rejection_reason: "the prompt adds capability, not a fix to broken existing behavior. RFC 7519 is the authority for the new capability."
    selected:         false
  - transition_type:  "external_source_integration"
    confidence:       0.34
    rejection_reason: "the prompt does not ask to ingest a new external source. RFC 7519 is consulted as specification authority, not integrated as a runtime data source."
    selected:         false
```

Reason:
```
[WRONG: ES-S5 is the canonical feature_addition example for the registry —
new capability (JWT auth) that did not exist before. The rejected selects
"repair" because "tenant isolation" suggests something is broken. But the
prompt does not name a broken behavior; it adds the auth and the tests for
the first time. Repair would imply an existing JWT mechanism that fails —
no such mechanism exists yet.]
```

Phrasing-B (Type A — wrong transition_type):
```
selected_flow_id: "transition.repair"
overall_verdict:  "accepted_for_expert_split"
candidate_set:
  - transition_type:  "repair"
    confidence:       0.68
    normalized_A:     "tenant isolation is broken"
    normalized_B:     "tenant isolation is fixed"
    transformation_X: "fix the JWT validation that is failing"
    selected:         true
```

Coverage labels:
```
source_id:       rfc-7519, rfc-6749
source_title:    RFC 7519 JWT + RFC 6749 OAuth
transition_type: feature_addition
phase:           3
sub_phase:       3.4
task_type:       jwt-authentication
rejected_type:   A
overlap_ratio:   0.45   (Type A: chosen feature_addition vs rejected repair; tenant-isolation tokens shared, transformation_X differs)
```

---

## Pair[ID] ES-2026-05-08-6.3-L1-04 — ES-S6 repair (compliance review with finding)

A:
```
run_id:       "ES-S6-training-001"
scenario_id:  "ES-S6"
user_request: "Review whether the admin endpoints enforce role-based access control as required by NIST SP 800-53 AC-3 — there is a finding that an authenticated regular user can call the admin user-list endpoint."
flags:
  UseLearnedUnitWhenAvailable: true
  UseKnowledgeEdgeDetection:   true
```

B:
```
selected_flow_id: "transition.repair"
overall_verdict:  "accepted_for_expert_split"
candidate_set:
  - transition_type:  "repair"
    confidence:       0.83
    normalized_A:     "admin endpoint allows authenticated regular user — AC-3 violated"
    normalized_B:     "admin endpoint enforces RBAC; only role=admin can call admin/users; AC-3 satisfied"
    transformation_X: "identify the missing role gate and produce a review verdict citing AC-3"
    selected:         true
  - transition_type:  "feature_addition"
    confidence:       0.39
    rejection_reason: "the auth model already exists; the gap is enforcement, not a new model. NIST AC-3 is the authority for the existing requirement."
    selected:         false
  - transition_type:  "topology_mutation"
    confidence:       0.18
    rejection_reason: "no service ownership change; the existing endpoint must satisfy the existing model."
    selected:         false
```

Reason:
```
[WRONG: ES-S6 names a defect — AC-3 is violated by the current
implementation. Repair is the correct transition: an existing requirement
fails on existing code. The rejected selects feature_addition because the
prompt mentions "role-based access control" and CWE-285, suggesting new
capability. But the model exists; the implementation is defective.
Feature_addition would imply RBAC has never been required — the prompt
contradicts that with the explicit AC-3 reference.]
```

Phrasing-B (Type A — wrong transition_type):
```
selected_flow_id: "transition.feature_addition"
overall_verdict:  "accepted_for_expert_split"
candidate_set:
  - transition_type:  "feature_addition"
    confidence:       0.62
    normalized_A:     "admin endpoint without RBAC capability"
    normalized_B:     "admin endpoint with new RBAC capability"
    transformation_X: "add new role-based access control"
    selected:         true
```

Coverage labels:
```
source_id:       cwe-285, nist-sp-800-53-ac-3
source_title:    CWE-285 + NIST AC-3
transition_type: repair
phase:           6
sub_phase:       6.3
task_type:       security-compliance-review
rejected_type:   A
overlap_ratio:   0.43   (Type A: chosen repair vs rejected feature_addition; AC-3 / RBAC tokens shared)
```

---

## Pair[ID] ES-2026-05-08-3.5-L1-05 — ES-S7 feature_addition (saga compensation)

A:
```
run_id:       "ES-S7-training-001"
scenario_id:  "ES-S7"
user_request: "Implement the saga compensation logic for the order placement workflow before connecting it to the inventory service. The saga must compensate the payment if inventory reservation fails, and the inventory must be reserved before the payment is captured."
flags:
  UseLearnedUnitWhenAvailable: true
  UseKnowledgeEdgeDetection:   true
```

B:
```
selected_flow_id: "transition.feature_addition"
overall_verdict:  "accepted_for_expert_split"
candidate_set:
  - transition_type:  "feature_addition"
    confidence:       0.86
    normalized_A:     "order placement workflow without saga compensation; inventory and payment ordering undefined"
    normalized_B:     "order placement workflow with saga: inventory reserved before payment captured; payment compensated if inventory reservation fails"
    transformation_X: "introduce saga forward order (reserve → capture) and compensation order (compensate-payment ← reservation-fail)"
    selected:         true
  - transition_type:  "repair"
    confidence:       0.32
    rejection_reason: "no saga exists yet to be repaired; the saga is being introduced."
    selected:         false
```

Reason:
```
[WRONG: ES-S7 introduces a new saga — Temporal saga documentation defines
this as a workflow declaration that did not previously exist. The rejected
selects external_source_integration because the prompt mentions inventory
service "connection". But the inventory service is an existing internal
component, not an external source being ingested.
external_source_integration applies when a new external system is wired in
as a runtime data source — not when one internal service depends on
another.]
```

Phrasing-B (Type A — wrong transition_type):
```
selected_flow_id: "transition.external_source_integration"
overall_verdict:  "accepted_for_expert_split"
candidate_set:
  - transition_type:  "external_source_integration"
    confidence:       0.55
    normalized_A:     "no inventory integration"
    normalized_B:     "inventory integrated as external source"
    transformation_X: "ingest inventory service as external source"
    selected:         true
```

Coverage labels:
```
source_id:       temporal-saga, airflow-dag-tutorial
source_title:    Temporal saga + Airflow DAG
transition_type: feature_addition
phase:           3
sub_phase:       3.5
task_type:       workflow-orchestration
rejected_type:   A
overlap_ratio:   0.40   (Type A: chosen feature_addition vs rejected external_source_integration; "inventory service" token shared)
```

---

## Pair[ID] ES-2026-05-08-3.3-L1-06 — ES-S8 explain (CODE_Q transition)

A:
```
run_id:       "ES-S8-training-001"
scenario_id:  "ES-S8"
user_request: "How does breaking change versioning work in OpenAPI 3.1?"
flags:
  UseLearnedUnitWhenAvailable: true
  UseKnowledgeEdgeDetection:   true
```

B:
```
selected_flow_id: "transition.explain"
overall_verdict:  "accepted_for_expert_split"
candidate_set:
  - transition_type:  "explain"
    confidence:       0.79
    normalized_A:     "user does not know the OpenAPI 3.1 breaking-change taxonomy"
    normalized_B:     "user has been told the OpenAPI 3.1 + AIP-180 breaking-change taxonomy"
    transformation_X: "produce explanatory response grounded in OpenAPI 3.1 §4 and AIP-180 — no code change is requested or implied"
    selected:         true
  - transition_type:  "feature_addition"
    confidence:       0.21
    rejection_reason: "no feature is being added; the verb is 'How does'."
    selected:         false
  - transition_type:  "repair"
    confidence:       0.12
    rejection_reason: "no defect is named; the verb is 'How does'."
    selected:         false
```

Reason:
```
[WRONG: "How does X work" is the canonical explain transition — the prompt
asks for understanding, not for any code or contract change. The rejected
selects feature_addition because OpenAPI is in the lexicon, but no addition
is requested. Producing an implementation plan when explanation was asked
is the most common CODE_Q failure mode documented in the DPO authoring
guidance per-domain section.]
```

Phrasing-B (Type C — wrong domain transition):
```
selected_flow_id: "transition.feature_addition"
overall_verdict:  "accepted_for_expert_split"
candidate_set:
  - transition_type:  "feature_addition"
    confidence:       0.51
    normalized_A:     "no breaking-change versioning support"
    normalized_B:     "breaking-change versioning support added"
    transformation_X: "add OpenAPI versioning"
    selected:         true
```

Coverage labels:
```
source_id:            openapi-specification, aip-180
source_title:         OpenAPI 3.1 + AIP-180
attribution_required: true
attribution_text:     "© Google LLC, CC BY 4.0"
transition_type:      explain
phase:                3
sub_phase:            3.3
task_type:            api-contract-explanation
rejected_type:        C
overlap_ratio:        0.27   (Type C: chosen explain vs rejected feature_addition; OpenAPI versioning token shared, transformation_X disjoint)
```

---

## Layer 1 summary

```
Pairs:                  6
Type A rejected:        5  (ES-S1, ES-S2, ES-S5, ES-S6, ES-S7)
Type C rejected:        1  (ES-S8)
transition types:
  feature_addition      4 (ES-S1, ES-S2, ES-S5, ES-S7)
  repair                1 (ES-S6)
  explain               1 (ES-S8)
```

Every B contains both selected and rejected candidates with rejection
reasons, satisfying the v8 acceptance checklist.
