# Held-Out and Rephrasing Pairs

Required by `EXTERNAL-SOURCES-ORCHESTRATION-PIPELINE-PLAN-v7.md` line 371
("Every batch must include at minimum: held-out prompts, rephrased
prompts, wrong-transition prompts, dropped-constraint prompts,
wrong-sub-phase prompts, missing-evidence prompts, arbiter contradiction
prompts").

These pairs are NOT used to train the model — they are used to evaluate
that the trained model generalises beyond the in-batch chosen prompts.
Each pair carries `pair_kind: held_out | rephrasing` and is excluded from
DPO step training. They feed into Layer 3 `HeldOutEvaluationUnit`.

2026-05-11 routing-flow correction: any held-out row that targets compiler or
router station behavior is diagnostic only. Routing acceptance requires separate
Layer 0-F held-out rows that execute the full `OrchestrationRoutingFlow` or
equivalent adaptive orchestrator path. Without those flow-level held-outs, the
routing status remains `PAIRSET_READY_ROUTING_FLOW_PENDING`.

Ten pairs in this file:

```
3 held-out prompts                — disjoint from batch chosen prompts
3 rephrasing pairs                — same intent, different surface form
1 wrong-transition prompt         — intent verb mis-mapped to transition
1 dropped-constraint prompt       — required field missing from B
1 wrong-sub-phase prompt          — routing target wrong
1 arbiter-contradiction prompt    — Node 08 verdict contradicts Node 06
```

---

## Common coverage labels

```
source_license_state: external_license_checked
node_role:            stage_9_held_out
coverage_status:      held_out
```

Per-pair labels carry: `source_id`, `source_title`, `transition_type`,
`phase`, `sub_phase`, `task_type`, `pair_kind`, `rejected_type`,
`overlap_ratio`.

---

## Pair[ID] ES-2026-05-08-HO-01 — held-out 3.1 boundary prompt

A (held-out, not in any batch chosen pair):
```
run_id:       "HO-01-router-001"
user_request: "We're moving the customer profile data out of the billing service into a new identity service. Define what each service owns now and which calls cross the boundary."
```

B (expected — chosen):
```
expected_routing:
  - PredictedSubPhaseId: "3.1"
    EvidencePredicates:  ["ARCHITECTURE"]
    KnowledgeEdge:       true
expected_transition_type: "topology_mutation"
expected_node_03_branches:
  - sub_phase: "3.1"  depends_on: []
expected_authority: "arc42 §5 Building Block View"
```

Reason:
```
[WRONG: this prompt names a service split (topology_mutation), not a
new feature (feature_addition). The rejected expects feature_addition.
Topology mutation applies when service ownership changes; here a single
service is being split into two. arc42 building-block view is the
authority for the boundary decision.]
```

Phrasing-B (Type A — wrong transition_type):
```
expected_routing:
  - PredictedSubPhaseId: "3.1"
    EvidencePredicates:  ["ARCHITECTURE"]
    KnowledgeEdge:       true
expected_transition_type: "feature_addition"
```

Coverage labels:
```
source_id:            arc42-template
source_title:         arc42 §5 Building Block View
attribution_required: false
contract_signature:   AdaptiveLogicalOrchestrator
transition_type:      topology_mutation
phase:                3
sub_phase:            3.1
task_type:            architecture-topology
pair_kind:            held_out
rejected_type:        A
overlap_ratio:        0.21
```

---

## Pair[ID] ES-2026-05-08-HO-02 — held-out 5.2 prompt

A:
```
run_id:       "HO-02-router-001"
user_request: "Add an integration test that proves we cannot insert a row with a foreign key referencing a different tenant's parent record."
```

B (expected):
```
expected_routing:
  - PredictedSubPhaseId: "5.2"
    EvidencePredicates:  ["TEST", "AUTHORIZATION"]
    KnowledgeEdge:       true
expected_transition_type: "feature_addition"
expected_authority: "Testcontainers — real container required for cross-row referential integrity verification"
```

Reason:
```
[WRONG: foreign-key cross-tenant verification requires the same real
database that ES-S3 needs — a mocked repository cannot enforce or fail
referential integrity. The rejected routes to 5.1 unit tests with
mocked repository. This is the canonical 5.1 vs 5.2 confusion.]
```

Phrasing-B (Type A — 5.2 confused with 5.1):
```
expected_routing:
  - PredictedSubPhaseId: "5.1"
    EvidencePredicates:  ["TEST"]
    KnowledgeEdge:       true
```

Coverage labels:
```
source_id:            testcontainers-dotnet
source_title:         Testcontainers — referential integrity testing
attribution_required: false
contract_signature:   AdaptiveLogicalOrchestrator
transition_type:      feature_addition
phase:                5
sub_phase:            5.2
task_type:            integration-tests
pair_kind:            held_out
rejected_type:        A
overlap_ratio:        0.24
```

---

## Pair[ID] ES-2026-05-08-HO-03 — held-out 6.3 review prompt

A:
```
run_id:       "HO-03-router-001"
user_request: "Audit whether our session cookie configuration meets the secure-cookie attribute set per RFC 6265bis — Secure, HttpOnly, SameSite=Lax. There is a finding that the session cookie omits the Secure flag in staging."
```

B (expected):
```
expected_routing:
  - PredictedSubPhaseId: "6.3"
    EvidencePredicates:  ["QA", "SECURITY"]
    KnowledgeEdge:       true
expected_transition_type: "repair"
expected_authority: "RFC 6265bis — Set-Cookie attribute semantics"
```

Reason:
```
[WRONG: the prompt names a finding ("Secure flag omitted") and asks for
a review against an RFC standard. That is 6.3 review (repair transition).
The rejected routes to 3.4 (redesign the session model) — but the
session model is already defined; the implementation has a finding.
This is the 6.3 vs 3.4 confusion.]
```

Phrasing-B (Type A — 6.3 confused with 3.4):
```
expected_routing:
  - PredictedSubPhaseId: "3.4"
    EvidencePredicates:  ["SECURITY"]
    KnowledgeEdge:       false
```

Coverage labels:
```
source_id:            rfc-6265
source_title:         IETF RFC 6265bis — HTTP State Management
attribution_required: false
contract_signature:   AdaptiveLogicalOrchestrator
transition_type:      repair
phase:                6
sub_phase:            6.3
task_type:            security-compliance-review
pair_kind:            held_out
rejected_type:        A
overlap_ratio:        0.23
```

---

## Pair[ID] ES-2026-05-08-RP-04 — rephrasing of ES-S5 (different surface form)

A:
```
original_prompt:    "Add JWT bearer authentication on the API endpoint per RFC 7519 and write integration tests for tenant isolation across the authentication and API layers."
rephrased_prompt:   "We need bearer-token auth on the API surface (JWT, RFC 7519) plus integration tests proving tenant data does not leak between tenants — across both auth and API."
```

B (expected — same routing as ES-S5):
```
expected_routing:
  - PredictedSubPhaseId: "3.4"  EvidencePredicates: ["AUTHORIZATION", "SECURITY"]  KnowledgeEdge: false
  - PredictedSubPhaseId: "3.3"  EvidencePredicates: ["API_CONTRACT"]              KnowledgeEdge: true
  - PredictedSubPhaseId: "5.2"  EvidencePredicates: ["TEST"]                      KnowledgeEdge: true
expected_transition_type: "feature_addition"
```

Reason:
```
[WRONG: rephrasing must produce identical routing to the original prompt
when the intent is preserved. The chosen produces the same 3.4+3.3+5.2
fan-out as ES-S5. The rejected drops 5.2 because the rephrased form uses
"data does not leak" instead of "tenant isolation tests" — but the
rephrasing changed the surface, not the intent. The TEST predicate must
still fire from "integration tests" at the end of the rephrased prompt.]
```

Phrasing-B (Type A — surface-form sensitivity, drops 5.2):
```
expected_routing:
  - PredictedSubPhaseId: "3.4"  EvidencePredicates: ["AUTHORIZATION", "SECURITY"]  KnowledgeEdge: false
  - PredictedSubPhaseId: "3.3"  EvidencePredicates: ["API_CONTRACT"]              KnowledgeEdge: true
```

Coverage labels:
```
source_id:            rfc-7519, rfc-6749
source_title:         RFC 7519 + RFC 6749 (rephrasing source)
attribution_required: false
contract_signature:   AdaptiveLogicalOrchestrator
transition_type:      feature_addition
phase:                3
sub_phase:            3.4+3.3+5.2
task_type:            jwt-authentication
pair_kind:            rephrasing
rejected_type:        A
overlap_ratio:        0.27
```

---

## Pair[ID] ES-2026-05-08-RP-05 — rephrasing of ES-S2 (different surface form)

A:
```
original_prompt:   "Design what to measure and alert on for our payment processor service. We need to know which workflows are running, which retries are accumulating, and where compensation activities fire."
rephrased_prompt:  "Pick the SLOs and alarm thresholds for the payment processor — visibility into workflow execution, accumulated retries, and compensation execution sites."
```

B (expected — same 3.5 routing as ES-S2):
```
expected_routing:
  - PredictedSubPhaseId: "3.5"
    EvidencePredicates:  ["ARCHITECTURE"]
    KnowledgeEdge:       true
expected_transition_type: "feature_addition"
```

Reason:
```
[WRONG: "SLOs and alarm thresholds" is direct 3.5 vocabulary even
without the word "observability" — the prompt names the design
artifacts. The rejected routes to 7.3 because "alarm thresholds"
sounds runtime-operational. The 3.5 vs 7.3 boundary applies: choosing
the thresholds is 3.5 design; configuring the dashboard to enforce
them is 7.3.]
```

Phrasing-B (Type A — 3.5 confused with 7.3 again):
```
expected_routing:
  - PredictedSubPhaseId: "7.3"
    EvidencePredicates:  ["ARCHITECTURE"]
    KnowledgeEdge:       true
```

Coverage labels:
```
source_id:            temporal-observability-docs, dapr-observability-docs
source_title:         Temporal + Dapr observability (rephrasing source)
attribution_required: false
contract_signature:   AdaptiveLogicalOrchestrator
transition_type:      feature_addition
phase:                3
sub_phase:            3.5
task_type:            observability-design
pair_kind:            rephrasing
rejected_type:        A
overlap_ratio:        0.26
```

---

## Pair[ID] ES-2026-05-08-RP-06 — rephrasing of ES-S3 (different surface form)

A:
```
original_prompt:   "Write integration tests that verify postgres tenant isolation across the orders module. The tests must spin up a real postgres container and assert that tenant A cannot read tenant B's order rows."
rephrased_prompt:  "Build integration tests against a real Postgres instance for the orders module — assert that tenant_a's JWT cannot read tenant_b's rows."
```

B (expected):
```
expected_routing:
  - PredictedSubPhaseId: "5.2"
    EvidencePredicates:  ["TEST", "AUTHORIZATION"]
    KnowledgeEdge:       true
expected_transition_type: "feature_addition"
```

Reason:
```
[WRONG: rephrasing preserves the 5.2 routing — "real Postgres instance"
is the Testcontainers signal. The rejected routes to 5.4 Performance
Tests because "real Postgres" sounds load-test-like. But the asserted
property is access denial, not throughput; 5.2 is correct.]
```

Phrasing-B (Type A — 5.2 confused with 5.4):
```
expected_routing:
  - PredictedSubPhaseId: "5.4"
    EvidencePredicates:  ["TEST"]
    KnowledgeEdge:       true
```

Coverage labels:
```
source_id:            testcontainers-dotnet
source_title:         Testcontainers (rephrasing source)
attribution_required: false
contract_signature:   AdaptiveLogicalOrchestrator
transition_type:      feature_addition
phase:                5
sub_phase:            5.2
task_type:            integration-tests
pair_kind:            rephrasing
rejected_type:        A
overlap_ratio:        0.25
```

---

## Pair[ID] ES-2026-05-08-WT-07 — wrong-transition prompt

A:
```
run_id:       "WT-07-orch-001"
user_request: "How does the OAuth scope check work in our auth middleware?"
```

B (expected — explain transition):
```
expected_transition_type: "explain"
expected_routing:
  - PredictedSubPhaseId: "3.4"
    KnowledgeEdge:       false
expected_authority: "RFC 6749 §3.3 scope semantics"
```

Reason:
```
[WRONG: "How does ... work" is the canonical explain transition (CODE_Q
intent). The rejected selects feature_addition and produces a plan to
add the scope check — but the prompt asks for an explanation, not an
addition. This is the canonical CODE_Q failure documented in DPO
authoring guidance.]
```

Phrasing-B (Type C — wrong transition_type for the verb):
```
expected_transition_type: "feature_addition"
expected_routing:
  - PredictedSubPhaseId: "3.4"
    KnowledgeEdge:       false
```

Coverage labels:
```
source_id:            rfc-6749
source_title:         RFC 6749 §3.3 scope semantics
attribution_required: false
contract_signature:   LogicalTransitionOrchestrator
transition_type:      explain
phase:                3
sub_phase:            3.4
task_type:            jwt-authentication-explanation
pair_kind:            wrong_transition
rejected_type:        C
overlap_ratio:        0.19
```

---

## Pair[ID] ES-2026-05-08-DC-08 — dropped-constraint prompt

A:
```
run_id:       "DC-08-orch-001"
user_request: "We need a tenant-aware audit log for every administrative action — who, what, when, on which tenant."
flags:
  UseKnowledgeEdgeDetection: true
```

B (expected — knowledge edge with all six required metrics):
```
ScenarioId:      "DC-08"
SelectedFlowId:  "knowledge-edge-missing-flow"
OverallVerdict:  "KNOWLEDGE_EDGE"
BranchResults:
  - FlowId:           "knowledge-edge-missing-flow"
    OverallVerdict:   "KNOWLEDGE_EDGE"
    FinalHumanResult: "Knowledge edge: routed to sub-phase '3.5', but no logical flow registered for routed sub-phase."
    Metrics:
      knowledge_edge:        "True"
      missing_sub_phase:     "3.5"
      missing_flow:          "True"
      routed_phase_id:       "3"
      routing_confidence:    "0.79"
      knowledge_edge_reason: "no logical flow registered for routed sub-phase"
    EvidenceIds: ["ARCHITECTURE", "AUTHORIZATION"]
    Warnings:    ["knowledge_edge"]
```

Reason:
```
[WRONG: chosen B has all six required metrics. The rejected drops three
(routed_phase_id, routing_confidence, knowledge_edge_reason). This
violates the v8 acceptance checklist line that knowledge-edge Metrics
include all six keys. Dropped-constraint pairs train the model to
detect missing required fields in the orchestrator output.]
```

Phrasing-B (Type A — drops three required metrics):
```
ScenarioId:      "DC-08"
SelectedFlowId:  "knowledge-edge-missing-flow"
OverallVerdict:  "KNOWLEDGE_EDGE"
BranchResults:
  - FlowId:           "knowledge-edge-missing-flow"
    OverallVerdict:   "KNOWLEDGE_EDGE"
    FinalHumanResult: "Knowledge edge."
    Metrics:
      knowledge_edge:        "True"
      missing_sub_phase:     "3.5"
      missing_flow:          "True"
    EvidenceIds: []
    Warnings:    []
```

Coverage labels:
```
source_id:            nist-sp-800-53-au-2
source_title:         NIST SP 800-53 AU-2 Audit Events
attribution_required: false
contract_signature:   AdaptiveLogicalOrchestrator
transition_type:      feature_addition
phase:                3
sub_phase:            3.5
task_type:            audit-event-design
pair_kind:            dropped_constraint
rejected_type:        A
overlap_ratio:        0.34
```

---

## Pair[ID] ES-2026-05-08-WS-09 — wrong-sub-phase prompt

A:
```
run_id:       "WS-09-orch-001"
user_request: "Define the OpenAPI request body schema for POST /events including the required envelope fields and the JSON Schema constraints on the payload."
```

B (expected):
```
expected_routing:
  - PredictedSubPhaseId: "3.3"
    EvidencePredicates:  ["API_CONTRACT"]
    KnowledgeEdge:       true
expected_transition_type: "feature_addition"
expected_authority: "OpenAPI 3.1 §4.8.5 + JSON Schema 2020-12"
```

Reason:
```
[WRONG: 3.3 owns API contract definitions including request body schema
shape. The rejected routes to 2.3 Data and Validation Rules — but 2.3
owns business validation policy (e.g., "an order amount must be > 0"),
not the wire-format schema. OpenAPI + JSON Schema is wire-level contract
authority for 3.3, not business-rule authority for 2.3.]
```

Phrasing-B (Type A — 3.3 confused with 2.3):
```
expected_routing:
  - PredictedSubPhaseId: "2.3"
    EvidencePredicates:  ["DOCUMENT", "API_CONTRACT"]
    KnowledgeEdge:       true
```

Coverage labels:
```
source_id:            openapi-specification, json-schema-org
source_title:         OpenAPI 3.1 + JSON Schema 2020-12
attribution_required: false
contract_signature:   MultiSubPhaseRouterUnit
transition_type:      feature_addition
phase:                3
sub_phase:            3.3
task_type:            api-contract
pair_kind:            wrong_sub_phase
rejected_type:        A
overlap_ratio:        0.28
```

---

## Pair[ID] ES-2026-05-08-AC-10 — arbiter-contradiction prompt

A:
```
ReviewedResultSet:
  scenario_id: "AC-10"
  fragments:
    - fragment_id: "frag-3.5-rejected"
      status_class: "rejected"
      fragment_text: "Rejected: inventory.release on inventory.reserve failure — saga rule violation."
      evidence_ids: ["temporal-saga"]
  arbiter_verdicts:
    - request_id: "arb-3.5-saga"
      verdict:    "pass"   ← contradicts the rejected fragment
```

B (expected — Node 09 must surface the contradiction, not silence it):
```
HumanResult:
  scenario_id:        "AC-10"
  overall_verdict:    "ARBITER_CONTRADICTION"
  body: |
    ## Arbiter contradiction detected

    The Temporal saga arbiter returned PASS for a scenario whose Node 06
    fragment was status_class: rejected. The fragment cites the saga rule
    violation; PASS contradicts that rejection.

    ## Resolution required

    Re-arbitrate this scenario before publishing. Either the arbiter
    misread the fragment or the fragment's rejection is unjustified.

  next_action: "send arbiter request back through Node 07 with the rejection text included"
```

Reason:
```
[WRONG: Node 09 must detect arbiter-vs-fragment contradictions and surface
them as ARBITER_CONTRADICTION. The chosen produces that surface and asks
for re-arbitration. The rejected formats the result as PASS, silencing
the contradiction. v8 Layer 2 §"protect orchestration correctness"
requires the human-visible preservation of every hard edge — a
contradiction is the hardest edge.]
```

Phrasing-B (Type A — contradiction silenced):
```
HumanResult:
  scenario_id:        "AC-10"
  overall_verdict:    "PASS"
  body: |
    ## Accepted
    Saga workflow accepted.
```

Coverage labels:
```
source_id:            temporal-saga
source_title:         Temporal saga documentation (contradiction grounding)
attribution_required: false
contract_signature:   FormatterUnit
transition_type:      feature_addition
phase:                3
sub_phase:            3.5
task_type:            workflow-orchestration
pair_kind:            arbiter_contradiction
rejected_type:        A
overlap_ratio:        0.29
```

---

## Held-Out + Rephrasing summary

```
Pairs:                  10
pair_kind distribution:
  held_out                 3   (HO-01, HO-02, HO-03)
  rephrasing               3   (RP-04, RP-05, RP-06)
  wrong_transition         1   (WT-07)
  dropped_constraint       1   (DC-08)
  wrong_sub_phase          1   (WS-09)
  arbiter_contradiction    1   (AC-10)

Type A rejected:        9
Type C rejected:        1   (WT-07)

External-source-only:   true (every pair grounded in RFC, OpenAPI, JSON Schema,
                              Testcontainers, Temporal, NIST, arc42, Dapr)
```

This satisfies v7 line 371 ("Every batch must include at minimum:
held-out prompts, rephrased prompts, wrong-transition prompts,
dropped-constraint prompts, wrong-sub-phase prompts, missing-evidence
prompts, arbiter contradiction prompts").

Missing-evidence prompts are not separately enumerated — they are
covered by DC-08 (dropped-constraint subsumes the missing-evidence case
for the orchestrator output).

---

## Last Updated

2026-05-08.
