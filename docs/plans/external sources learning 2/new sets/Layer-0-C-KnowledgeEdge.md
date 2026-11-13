# Layer 0-C — Knowledge Edge Detection Pairs

```
Contract:
  AdaptiveLogicalOrchestrator : ILogicalOrchestrator
  pair_kind: knowledge_edge_detection
```

## Common coverage labels (apply to every pair in this file)

```
source_license_state: external_license_checked
contract_signature:   AdaptiveLogicalOrchestrator
node_role:            layer_0_c
pair_kind:            knowledge_edge_detection
coverage_status:      covered
attribution_required: false
```

Per-pair labels carry: `source_id`, `source_title`, `transition_type`,
`phase`, `sub_phase`, `task_type`, `rejected_type`, `overlap_ratio`.

Token-overlap gate: every rejected satisfies overlap_ratio < 0.60.

Layer 0-C is not a separate unit — it is the orchestrator-level pair shape
for outputs where at least one routed sub-phase has no registered flow.

Required gap-branch metrics (all six keys must appear):

```
knowledge_edge        — "True"
missing_sub_phase     — PredictedSubPhaseId
missing_flow          — "True"
routed_phase_id       — PredictedPhaseId
routing_confidence    — Confidence as 2-decimal string
knowledge_edge_reason — "no logical flow registered for routed sub-phase"
```

Per the v8 acceptance checklist, every batch with knowledge-edge pairs must
include at least one **registered-flow comparison pair**. ES-S5 supplies it
in this batch — its 3.4 branch is registered (Wave 1 saturated 3.4) and
produces a real PASS, while 3.3 and 5.2 produce knowledge-edge branches.

---

## Pair[ID] ES-2026-05-08-3.1-L0C-01 — ES-S1 gap pair (3.1 unregistered)

A:
```
run_id:       "ES-S1-orch-001"
scenario_id:  "ES-S1"
user_request: "We need to define the component boundaries between the order service and the inventory service. The order service must own the order lifecycle and the inventory service must own stock state — neither service may read the other's storage directly."
flags:
  UseKnowledgeEdgeDetection: true
```

B:
```
ScenarioId:      "ES-S1"
SelectedFlowId:  "knowledge-edge-missing-flow"
OverallVerdict:  "KNOWLEDGE_EDGE"
BranchResults:
  - FlowId:           "knowledge-edge-missing-flow"
    OverallVerdict:   "KNOWLEDGE_EDGE"
    FinalHumanResult: "Knowledge edge: routed to sub-phase '3.1', but no logical flow registered for routed sub-phase."
    Metrics:
      knowledge_edge:        "True"
      missing_sub_phase:     "3.1"
      missing_flow:          "True"
      routed_phase_id:       "3"
      routing_confidence:    "0.86"
      knowledge_edge_reason: "no logical flow registered for routed sub-phase"
    EvidenceIds: ["ARCHITECTURE"]
    Warnings:    ["knowledge_edge"]
```

Reason:
```
[WRONG: 3.1 has no registered flow. The orchestrator must produce a
KNOWLEDGE_EDGE verdict with all six required metrics. The rejected returns
PASS with a fabricated finding ("boundary correctness verified") — the
system has no flow that could verify boundary correctness for 3.1, so PASS
is invented data. This is the knowledge-edge-suppressed corruption pattern.]
```

Phrasing-B (knowledge-edge-suppressed corruption):
```
ScenarioId:      "ES-S1"
SelectedFlowId:  "fabricated-3.1-pass"
OverallVerdict:  "PASS"
BranchResults:
  - FlowId:           "fabricated-3.1-pass"
    OverallVerdict:   "PASS"
    FinalHumanResult: "Boundary correctness verified for the order/inventory split."
    Metrics: { }
    EvidenceIds: []
    Warnings:    []
```

Coverage labels:
```
source_id:       arc42-template, ADR-template
source_title:    arc42 + ADR (3.1 gap pair)
transition_type: feature_addition
phase:           3
sub_phase:       3.1
task_type:       architecture-topology
rejected_type:   corruption_case
overlap_ratio:   0.29   (knowledge-edge-suppressed: chosen KNOWLEDGE_EDGE w/ six metrics vs corrupted PASS w/ empty metrics)
```

---

## Pair[ID] ES-2026-05-08-3.5-L0C-02 — ES-S2 gap pair (3.5 unregistered)

A:
```
run_id:       "ES-S2-orch-001"
scenario_id:  "ES-S2"
user_request: "Design what to measure and alert on for our payment processor service. We need to know which workflows are running, which retries are accumulating, and where compensation activities fire."
flags:
  UseKnowledgeEdgeDetection: true
```

B:
```
ScenarioId:      "ES-S2"
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
      routing_confidence:    "0.83"
      knowledge_edge_reason: "no logical flow registered for routed sub-phase"
    EvidenceIds: ["ARCHITECTURE"]
    Warnings:    ["knowledge_edge"]
```

Reason:
```
[WRONG: 3.5 has no registered flow — KNOWLEDGE_EDGE with all six metrics is
the correct shape. The rejected returns the KNOWLEDGE_EDGE verdict but
suppresses three of the six required metrics: routed_phase_id,
routing_confidence, knowledge_edge_reason. Partial metrics fail the v8
acceptance checklist requirement that knowledge-edge Metrics include all
six keys.]
```

Phrasing-B (Type A — partial metrics, knowledge-edge-suppressed variant):
```
ScenarioId:      "ES-S2"
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
source_id:       temporal-observability-docs, dapr-observability-docs
source_title:    Temporal + Dapr observability (3.5 gap pair)
transition_type: feature_addition
phase:           3
sub_phase:       3.5
task_type:       observability-design
rejected_type:   A
overlap_ratio:   0.42   (Type A: chosen six metrics vs rejected three; metric tokens partially shared)
```

---

## Pair[ID] ES-2026-05-08-5.2-L0C-03 — ES-S3 gap pair (5.2 unregistered)

A:
```
run_id:       "ES-S3-orch-001"
scenario_id:  "ES-S3"
user_request: "Write integration tests that verify postgres tenant isolation across the orders module. The tests must spin up a real postgres container and assert that tenant A cannot read tenant B's order rows."
flags:
  UseKnowledgeEdgeDetection: true
```

B:
```
ScenarioId:      "ES-S3"
SelectedFlowId:  "knowledge-edge-missing-flow"
OverallVerdict:  "KNOWLEDGE_EDGE"
BranchResults:
  - FlowId:           "knowledge-edge-missing-flow"
    OverallVerdict:   "KNOWLEDGE_EDGE"
    FinalHumanResult: "Knowledge edge: routed to sub-phase '5.2', but no logical flow registered for routed sub-phase."
    Metrics:
      knowledge_edge:        "True"
      missing_sub_phase:     "5.2"
      missing_flow:          "True"
      routed_phase_id:       "5"
      routing_confidence:    "0.88"
      knowledge_edge_reason: "no logical flow registered for routed sub-phase"
    EvidenceIds: ["TEST"]
    Warnings:    ["knowledge_edge"]
```

Reason:
```
[WRONG: 5.2 is unregistered — KNOWLEDGE_EDGE is the correct verdict. The
rejected uses missing_sub_phase: "5.1" instead of "5.2". This is the
adjacent-sub-phase confusion at the knowledge-edge layer: the gap branch
correctly fires but reports the wrong sub-phase, which would mis-fund the
gap-tracking dashboard. The router decision named 5.2; the gap branch must
echo it exactly.]
```

Phrasing-B (Type A — wrong missing_sub_phase echoed):
```
ScenarioId:      "ES-S3"
SelectedFlowId:  "knowledge-edge-missing-flow"
OverallVerdict:  "KNOWLEDGE_EDGE"
BranchResults:
  - FlowId:           "knowledge-edge-missing-flow"
    OverallVerdict:   "KNOWLEDGE_EDGE"
    FinalHumanResult: "Knowledge edge: routed to sub-phase '5.1', but no logical flow registered for routed sub-phase."
    Metrics:
      knowledge_edge:        "True"
      missing_sub_phase:     "5.1"
      missing_flow:          "True"
      routed_phase_id:       "5"
      routing_confidence:    "0.88"
      knowledge_edge_reason: "no logical flow registered for routed sub-phase"
    EvidenceIds: ["TEST"]
    Warnings:    ["knowledge_edge"]
```

Coverage labels:
```
source_id:       testcontainers-dotnet, microsoft-testfx
source_title:    Testcontainers + testfx (5.2 gap pair)
transition_type: feature_addition
phase:           5
sub_phase:       5.2
task_type:       integration-tests
rejected_type:   A
overlap_ratio:   0.38   (Type A: missing_sub_phase 5.2 vs 5.1; one digit differs)
```

---

## Pair[ID] ES-2026-05-08-3.3-L0C-04 — ES-S4 gap pair (3.3 unregistered)

A:
```
run_id:       "ES-S4-orch-001"
scenario_id:  "ES-S4"
user_request: "Add an Idempotency-Key header to the POST /payments endpoint per RFC 8470 and write the integration tests for the duplicate-request handling."
flags:
  UseKnowledgeEdgeDetection: true
```

B:
```
ScenarioId:      "ES-S4"
SelectedFlowId:  "knowledge-edge-missing-flow"
OverallVerdict:  "KNOWLEDGE_EDGE"
BranchResults:
  - FlowId:           "knowledge-edge-missing-flow"
    OverallVerdict:   "KNOWLEDGE_EDGE"
    FinalHumanResult: "Knowledge edge: routed to sub-phase '3.3', but no logical flow registered for routed sub-phase."
    Metrics:
      knowledge_edge:        "True"
      missing_sub_phase:     "3.3"
      missing_flow:          "True"
      routed_phase_id:       "3"
      routing_confidence:    "0.85"
      knowledge_edge_reason: "no logical flow registered for routed sub-phase"
    EvidenceIds: ["API_CONTRACT"]
    Warnings:    ["knowledge_edge"]
  - FlowId:           "knowledge-edge-missing-flow"
    OverallVerdict:   "KNOWLEDGE_EDGE"
    FinalHumanResult: "Knowledge edge: routed to sub-phase '5.2', but no logical flow registered for routed sub-phase."
    Metrics:
      knowledge_edge:        "True"
      missing_sub_phase:     "5.2"
      missing_flow:          "True"
      routed_phase_id:       "5"
      routing_confidence:    "0.81"
      knowledge_edge_reason: "no logical flow registered for routed sub-phase"
    EvidenceIds: ["TEST"]
    Warnings:    ["knowledge_edge"]
```

Reason:
```
[WRONG: ES-S4 fans out to 3.3 AND 5.2. Both are unregistered. The chosen
emits two knowledge-edge branches, one per routed sub-phase. The rejected
emits only one knowledge-edge branch and uses the literal text "multiple
sub-phases" in missing_sub_phase. This violates the metric contract —
missing_sub_phase must be a single sub-phase identifier, not a description.
Suppressing one of the two gaps also denies the operator visibility into
the testing work the prompt requested.]
```

Phrasing-B (Type A — collapses two gaps into one):
```
ScenarioId:      "ES-S4"
SelectedFlowId:  "knowledge-edge-missing-flow"
OverallVerdict:  "KNOWLEDGE_EDGE"
BranchResults:
  - FlowId:           "knowledge-edge-missing-flow"
    OverallVerdict:   "KNOWLEDGE_EDGE"
    FinalHumanResult: "Knowledge edge: routed to multiple sub-phases without registered flows."
    Metrics:
      knowledge_edge:        "True"
      missing_sub_phase:     "multiple sub-phases"
      missing_flow:          "True"
      routed_phase_id:       "3"
      routing_confidence:    "0.85"
      knowledge_edge_reason: "no logical flow registered for routed sub-phase"
    EvidenceIds: ["API_CONTRACT", "TEST"]
    Warnings:    ["knowledge_edge"]
```

Coverage labels:
```
source_id:       openapi-specification, rfc-8470, rfc-9110
source_title:    OpenAPI 3.1 + RFC 8470 (3.3+5.2 gap pair)
transition_type: feature_addition
phase:           3
sub_phase:       3.3+5.2
task_type:       multi-route-contract-and-tests
rejected_type:   A
overlap_ratio:   0.40   (Type A: chosen 2 gap branches vs rejected 1 collapsed)
```

---

## Pair[ID] ES-2026-05-08-3.4-L0C-05 — ES-S5 mixed: 3.4 PASS + 3.3/5.2 KNOWLEDGE_EDGE

This is the **registered-flow comparison pair** required by the v8 acceptance
checklist. ES-S5's 3.4 branch is registered (Wave 1 saturated 3.4) so it
produces a real PASS verdict. Its 3.3 and 5.2 branches are unregistered and
produce knowledge-edge branches. The model must see both shapes side by side
in the same orchestrator result to learn the distinction.

A:
```
run_id:       "ES-S5-orch-001"
scenario_id:  "ES-S5"
user_request: "Add JWT bearer authentication on the API endpoint per RFC 7519 and write integration tests for tenant isolation across the authentication and API layers."
flags:
  UseKnowledgeEdgeDetection: true
```

B:
```
ScenarioId:      "ES-S5"
SelectedFlowId:  "security-verification-3.4"
OverallVerdict:  "MIXED"
BranchResults:
  - FlowId:           "security-verification-3.4"
    ScenarioId:       "ES-S5"
    OverallVerdict:   "PASS"
    FinalHumanResult: "JWT bearer authentication design accepted: iss claim required (RFC 7519 §4.1.1), aud claim validated, exp enforced. tenant claim placement defined."
    Metrics:
      knowledge_edge:        "False"
      jwt_iss_required:      "True"
      jwt_aud_validated:     "True"
      jwt_exp_enforced:      "True"
      tenant_claim_resolved: "True"
    EvidenceIds: ["AUTHORIZATION", "SECURITY"]
    Warnings:    []
  - FlowId:           "knowledge-edge-missing-flow"
    OverallVerdict:   "KNOWLEDGE_EDGE"
    FinalHumanResult: "Knowledge edge: routed to sub-phase '3.3', but no logical flow registered for routed sub-phase."
    Metrics:
      knowledge_edge:        "True"
      missing_sub_phase:     "3.3"
      missing_flow:          "True"
      routed_phase_id:       "3"
      routing_confidence:    "0.78"
      knowledge_edge_reason: "no logical flow registered for routed sub-phase"
    EvidenceIds: ["API_CONTRACT"]
    Warnings:    ["knowledge_edge"]
  - FlowId:           "knowledge-edge-missing-flow"
    OverallVerdict:   "KNOWLEDGE_EDGE"
    FinalHumanResult: "Knowledge edge: routed to sub-phase '5.2', but no logical flow registered for routed sub-phase."
    Metrics:
      knowledge_edge:        "True"
      missing_sub_phase:     "5.2"
      missing_flow:          "True"
      routed_phase_id:       "5"
      routing_confidence:    "0.84"
      knowledge_edge_reason: "no logical flow registered for routed sub-phase"
    EvidenceIds: ["TEST"]
    Warnings:    ["knowledge_edge"]
```

Reason:
```
[WRONG: a mixed orchestrator result must preserve every branch — the
registered 3.4 PASS, the 3.3 KNOWLEDGE_EDGE, and the 5.2 KNOWLEDGE_EDGE.
The rejected returns OverallVerdict: "PASS" and only the 3.4 branch,
swallowing both knowledge-edge branches. The downstream operator would
believe the work is complete when 3.3 and 5.2 are unaddressed. This is the
knowledge-edge-suppressed corruption applied at the multi-branch level.]
```

Phrasing-B (knowledge-edge-suppressed at multi-branch level):
```
ScenarioId:      "ES-S5"
SelectedFlowId:  "security-verification-3.4"
OverallVerdict:  "PASS"
BranchResults:
  - FlowId:           "security-verification-3.4"
    ScenarioId:       "ES-S5"
    OverallVerdict:   "PASS"
    FinalHumanResult: "JWT bearer authentication design accepted."
    Metrics:
      knowledge_edge:        "False"
    EvidenceIds: ["AUTHORIZATION", "SECURITY"]
    Warnings:    []
```

Coverage labels:
```
source_id:                    rfc-7519, rfc-6749, rfc-8725
source_title:                 RFC 7519 + RFC 6749 + RFC 8725 (registered comparison ES-S5)
transition_type:              feature_addition
phase:                        3
sub_phase:                    3.4+3.3+5.2
task_type:                    multi-route-jwt-mixed-verdict
rejected_type:                corruption_case
overlap_ratio:                0.35   (mixed: chosen 3 branches w/ split verdicts vs rejected 1 PASS)
registered_flow_comparison:   true
```

---

## Pair[ID] ES-2026-05-08-6.3-L0C-06 — ES-S6 gap pair (6.3 unregistered)

A:
```
run_id:       "ES-S6-orch-001"
scenario_id:  "ES-S6"
user_request: "Review whether the admin endpoints enforce role-based access control as required by NIST SP 800-53 AC-3 — there is a finding that an authenticated regular user can call the admin user-list endpoint."
flags:
  UseKnowledgeEdgeDetection: true
```

B:
```
ScenarioId:      "ES-S6"
SelectedFlowId:  "knowledge-edge-missing-flow"
OverallVerdict:  "KNOWLEDGE_EDGE"
BranchResults:
  - FlowId:           "knowledge-edge-missing-flow"
    OverallVerdict:   "KNOWLEDGE_EDGE"
    FinalHumanResult: "Knowledge edge: routed to sub-phase '6.3', but no logical flow registered for routed sub-phase."
    Metrics:
      knowledge_edge:        "True"
      missing_sub_phase:     "6.3"
      missing_flow:          "True"
      routed_phase_id:       "6"
      routing_confidence:    "0.87"
      knowledge_edge_reason: "no logical flow registered for routed sub-phase"
    EvidenceIds: ["QA", "AUTHORIZATION"]
    Warnings:    ["knowledge_edge"]
```

Reason:
```
[WRONG: 6.3 has no registered flow — KNOWLEDGE_EDGE is required. The
rejected returns OverallVerdict: "REJECT" with the AC-3 finding text in the
human result. REJECT requires an arbiter that examined the implementation
and confirmed a control violation. Without a registered 6.3 flow there is
no arbiter result to ground a REJECT — the verdict is fabricated. The
correct response is KNOWLEDGE_EDGE: the orchestrator cannot produce a 6.3
review verdict yet, and the finding must be surfaced to a human reviewer.]
```

Phrasing-B (Type A — fabricated REJECT instead of KNOWLEDGE_EDGE):
```
ScenarioId:      "ES-S6"
SelectedFlowId:  "fabricated-6.3-review"
OverallVerdict:  "REJECT"
BranchResults:
  - FlowId:           "fabricated-6.3-review"
    OverallVerdict:   "REJECT"
    FinalHumanResult: "AC-3 violated: regular user can call admin endpoint. Reject deployment until role gate is added."
    Metrics:
      knowledge_edge:        "False"
      ac3_violation:         "True"
    EvidenceIds: ["AUTHORIZATION"]
    Warnings:    []
```

Coverage labels:
```
source_id:       cwe-285, nist-sp-800-53-ac-3
source_title:    CWE-285 + NIST AC-3 (6.3 gap pair)
transition_type: repair
phase:           6
sub_phase:       6.3
task_type:       security-compliance-review
rejected_type:   A
overlap_ratio:   0.36   (Type A: chosen KNOWLEDGE_EDGE vs rejected fabricated REJECT; verdict tokens disjoint, evidence shared)
```

---

## Layer 0-C summary

```
Pairs:                              6
Gap branches with all six metrics:  9 (across 6 pairs)
Registered comparison pair:         1 (ES-S5)
Type A rejected:                    3 (ES-S2, ES-S3, ES-S4, ES-S6)
corruption_case rejected:           2 (ES-S1, ES-S5)
```

The ES-S5 mixed pair satisfies the v8 requirement: every batch with
knowledge-edge pairs must include at least one registered-flow comparison.
3.4 produces PASS; 3.3 and 5.2 produce knowledge-edge branches.
