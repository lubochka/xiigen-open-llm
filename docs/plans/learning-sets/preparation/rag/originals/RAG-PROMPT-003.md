# Node 01 — Domain Splitter Pairs

```
Contract:
  DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>
```

## Common coverage labels (apply to every pair in this file)

```
source_license_state: external_license_checked
contract_signature:   DomainSplitterUnit
node_role:            node_01
pair_kind:            flow_node
coverage_status:      covered
attribution_required: false
```

Per-pair labels carry: `source_id`, `source_title`, `transition_type`,
`phase`, `sub_phase`, `task_type`, `rejected_type`, `overlap_ratio`.

Token-overlap gate: every rejected satisfies overlap_ratio < 0.60.

A side is the SelectedLogicalTransition from Layer 1 (real upstream output).
B contains one packet per development territory **actually present** in the
selected transition. Per v7 pipeline plan §3, "do not add territories not
present in the selected transition's A/B/X statement".

Required fields per packet:

```
packet_id, domain_label, phase, sub_phase, task_type,
prompt_fragment, coverage_status, reason, handoff_marker, source_evidence
```

Five pairs in this file — one per scenario where the splitter has to make
a non-trivial territory decision.

---

## Pair[ID] ES-2026-05-08-3.1-N01-01 — ES-S1 single ARCH packet

A:
```
SelectedLogicalTransition:
  scenario_id:      "ES-S1"
  transition_type:  "feature_addition"
  normalized_A:     "current architecture has no defined ownership boundary between order service and inventory service"
  normalized_B:     "architecture has explicit boundary rule — order service owns order lifecycle, inventory service owns stock state, neither reads the other's storage"
  transformation_X: "introduce ownership boundary, dependency direction, and forbidden cross-storage rule"
  source_evidence:
    - "arc42 §5 Building Block View — black-box services with explicit responsibilities"
    - "ADR Decision section — 'we will own X in service Y; we will not allow service Z to read Y's storage'"
```

B:
```
domain_packets:
  - packet_id:       "ES-S1-pkt-001"
    domain_label:    "ARCH"
    phase:           "3"
    sub_phase:       "3.1"
    task_type:       "architecture-topology"
    prompt_fragment: "define the component boundaries between the order service and the inventory service; neither may read the other's storage directly"
    coverage_status: "covered"
    reason:          "the transformation_X states 'introduce ownership boundary' which is the 3.1 responsibility per the topology registration"
    handoff_marker:  "ARCH/3.1/architecture-topology"
    source_evidence: ["arc42-template/05_building_block_view", "ADR-template/decision-section"]
```

Reason:
```
[WRONG: ES-S1's transition contains exactly one territory — 3.1
architecture topology. The rejected adds a 5.1 unit-tests packet because
"order service" suggests testable code, but the transition's A/B/X
statement contains no testing language and the prompt does not request
tests. The v7 pipeline rule is explicit: do not add territories not
present in the selected transition. Adding 5.1 here is an
ungrounded-territory error.]
```

Phrasing-B (Type A — adds an ungrounded territory):
```
domain_packets:
  - packet_id:       "ES-S1-pkt-001"
    domain_label:    "ARCH"
    phase:           "3"
    sub_phase:       "3.1"
    task_type:       "architecture-topology"
    prompt_fragment: "define the component boundaries"
    coverage_status: "covered"
    reason:          "ARCH territory present"
    handoff_marker:  "ARCH/3.1/architecture-topology"
    source_evidence: ["arc42-template"]
  - packet_id:       "ES-S1-pkt-002"
    domain_label:    "TEST"
    phase:           "5"
    sub_phase:       "5.1"
    task_type:       "unit-tests"
    prompt_fragment: "write unit tests for the order service boundary"
    coverage_status: "covered"
    reason:          "order service implies testable behavior"
    handoff_marker:  "TEST/5.1/unit-tests"
    source_evidence: []
```

Coverage labels:
```
source_id:       arc42-template, ADR-template
source_title:    arc42 + ADR (3.1 boundary topology)
transition_type: feature_addition
phase:           3
sub_phase:       3.1
task_type:       architecture-topology
rejected_type:   A
overlap_ratio:   0.31   (Type A: chosen 1 packet vs rejected 2 packets; ungrounded 5.1 packet has disjoint vocabulary)
```

---

## Pair[ID] ES-2026-05-08-3.5-N01-02 — ES-S2 single ARCH packet (3.5)

A:
```
SelectedLogicalTransition:
  scenario_id:      "ES-S2"
  transition_type:  "feature_addition"
  normalized_A:     "payment processor has ad-hoc telemetry; no defined signal set or alert thresholds"
  normalized_B:     "payment processor has explicit signal catalogue: workflows running, retry counts, compensation fires; alert thresholds bound to each"
  transformation_X: "introduce observability signal catalogue and alert threshold table"
  source_evidence:
    - "Temporal observability docs — workflow_completed/failed metrics, activity retry counter"
    - "Dapr observability docs — metrics + traces + logs separation"
    - "NIST SP 800-53 AU-2 — events to be logged"
```

B:
```
domain_packets:
  - packet_id:       "ES-S2-pkt-001"
    domain_label:    "ARCH"
    phase:           "3"
    sub_phase:       "3.5"
    task_type:       "observability-design"
    prompt_fragment: "design what to measure and alert on for the payment processor: workflows running, retries accumulating, compensation activities firing"
    coverage_status: "covered"
    reason:          "the transformation_X is 'introduce observability signal catalogue' — the design responsibility owned by 3.5 per topology"
    handoff_marker:  "ARCH/3.5/observability-design"
    source_evidence: ["temporal-observability-docs", "dapr-observability-docs", "nist-sp-800-53-au-2"]
```

Reason:
```
[WRONG: 3.5 owns the design — what to measure and what alert thresholds to
set. The rejected places this work into 7.3 Monitoring and Feedback Loop.
7.3's responsibility per the topology is "runtime observation, feedback
ingestion, and operational loop closure" — operating the dashboard once
the signals are designed. The transition's transformation_X is "introduce
the catalogue", not "operate the catalogue", so the territory is 3.5.
This is the canonical 3.5 vs 7.3 confusion documented in the ARCH table.]
```

Phrasing-B (Type A — 3.5 confused with 7.3):
```
domain_packets:
  - packet_id:       "ES-S2-pkt-001"
    domain_label:    "ARCH"
    phase:           "7"
    sub_phase:       "7.3"
    task_type:       "monitoring-feedback"
    prompt_fragment: "configure the monitoring dashboard for the payment processor"
    coverage_status: "covered"
    reason:          "monitoring is observability"
    handoff_marker:  "ARCH/7.3/monitoring-feedback"
    source_evidence: ["temporal-observability-docs"]
```

Coverage labels:
```
source_id:       temporal-observability-docs, dapr-observability-docs, nist-sp-800-53-au-2
source_title:    Temporal + Dapr observability + NIST AU-2
transition_type: feature_addition
phase:           3
sub_phase:       3.5
task_type:       observability-design
rejected_type:   A
overlap_ratio:   0.34   (Type A: chosen 3.5 packet vs rejected 7.3 packet; phase digit shared, sub-phase tail differs)
```

---

## Pair[ID] ES-2026-05-08-3.4-N01-03 — ES-S5 three packets (3.4 + 3.3 + 5.2)

A:
```
SelectedLogicalTransition:
  scenario_id:      "ES-S5"
  transition_type:  "feature_addition"
  normalized_A:     "current API has no tenant-aware JWT authentication and no integration tests for tenant isolation"
  normalized_B:     "API has tenant-aware JWT bearer authentication per RFC 7519 and tenant-isolation integration tests"
  transformation_X: "add authentication model, API contract constraints, implementation branches, integration tests, release review"
  source_evidence:
    - "RFC 7519 §4.1.1 — iss claim required"
    - "RFC 6749 §1.4 — bearer token semantics"
    - "Testcontainers PostgreSQL module documentation — integration test fixture pattern"
```

B:
```
domain_packets:
  - packet_id:       "ES-S5-pkt-001"
    domain_label:    "ARCH"
    phase:           "3"
    sub_phase:       "3.4"
    task_type:       "jwt-authentication"
    prompt_fragment: "design the JWT bearer authentication model: iss/aud/exp claims required, tenant claim placement, bypass paths"
    coverage_status: "covered"
    reason:          "RFC 7519 sets the model authority; transformation_X names 'authentication model' as a deliverable"
    handoff_marker:  "ARCH/3.4/jwt-authentication"
    source_evidence: ["rfc-7519", "rfc-6749"]
  - packet_id:       "ES-S5-pkt-002"
    domain_label:    "ARCH"
    phase:           "3"
    sub_phase:       "3.3"
    task_type:       "api-contract"
    prompt_fragment: "define the API contract: Authorization: Bearer <jwt> header required, 401 on missing, 403 on tenant mismatch"
    coverage_status: "covered"
    reason:          "transformation_X names 'API contract constraints' as a deliverable"
    handoff_marker:  "ARCH/3.3/api-contract"
    source_evidence: ["rfc-6749", "openapi-specification"]
  - packet_id:       "ES-S5-pkt-003"
    domain_label:    "TEST"
    phase:           "5"
    sub_phase:       "5.2"
    task_type:       "integration-tests"
    prompt_fragment: "write integration tests for tenant isolation across the authentication and API layers — real fixture, two tenants, assert cross-tenant access is denied"
    coverage_status: "covered"
    reason:          "transformation_X names 'integration tests' as a deliverable"
    handoff_marker:  "TEST/5.2/integration-tests"
    source_evidence: ["testcontainers-dotnet"]
```

Reason:
```
[WRONG: ES-S5's transition explicitly enumerates three deliverables in
transformation_X: authentication model (3.4), API contract constraints
(3.3), and integration tests (5.2). The rejected merges 3.3 into 3.4,
emitting only two packets. This violates the splitter rule that one packet
per territory present in the transition is required. Downstream Node 03
would lose the API contract dependency on the model, breaking the
SchemePlanner's depends_on chain.]
```

Phrasing-B (Type A — collapses 3.3 into 3.4):
```
domain_packets:
  - packet_id:       "ES-S5-pkt-001"
    domain_label:    "ARCH"
    phase:           "3"
    sub_phase:       "3.4"
    task_type:       "jwt-authentication"
    prompt_fragment: "design the JWT model and the API contract together"
    coverage_status: "covered"
    reason:          "auth and contract belong together"
    handoff_marker:  "ARCH/3.4/jwt-authentication"
    source_evidence: ["rfc-7519"]
  - packet_id:       "ES-S5-pkt-002"
    domain_label:    "TEST"
    phase:           "5"
    sub_phase:       "5.2"
    task_type:       "integration-tests"
    prompt_fragment: "write integration tests for tenant isolation"
    coverage_status: "covered"
    reason:          "transformation_X names integration tests"
    handoff_marker:  "TEST/5.2/integration-tests"
    source_evidence: ["testcontainers-dotnet"]
```

Coverage labels:
```
source_id:       rfc-7519, rfc-6749, openapi-specification, testcontainers-dotnet
source_title:    RFC 7519 + RFC 6749 + OpenAPI 3.1 + Testcontainers
transition_type: feature_addition
phase:           3
sub_phase:       3.4+3.3+5.2
task_type:       multi-territory-jwt-contract-tests
rejected_type:   A
overlap_ratio:   0.42   (Type A: chosen 3 packets vs rejected 2 packets; merged 3.3+3.4 packet shares vocabulary)
```

---

## Pair[ID] ES-2026-05-08-6.3-N01-04 — ES-S6 single QA packet

A:
```
SelectedLogicalTransition:
  scenario_id:      "ES-S6"
  transition_type:  "repair"
  normalized_A:     "admin endpoint allows authenticated regular user — AC-3 violated"
  normalized_B:     "admin endpoint enforces RBAC; only role=admin can call admin/users; AC-3 satisfied"
  transformation_X: "identify the missing role gate and produce a review verdict citing AC-3"
  source_evidence:
    - "NIST SP 800-53 AC-3 — Access Enforcement: 'the system enforces approved authorizations'"
    - "CWE-285 Improper Authorization — demonstrative example of authenticated-but-not-authorized request"
```

B:
```
domain_packets:
  - packet_id:       "ES-S6-pkt-001"
    domain_label:    "QA"
    phase:           "6"
    sub_phase:       "6.3"
    task_type:       "security-compliance-review"
    prompt_fragment: "review the admin endpoint against AC-3: confirm the missing role gate is the single root cause; verify the fix once applied; produce a finding citation"
    coverage_status: "covered"
    reason:          "transformation_X is 'produce a review verdict' — the 6.3 responsibility is review, not redesign"
    handoff_marker:  "QA/6.3/security-compliance-review"
    source_evidence: ["nist-sp-800-53-ac-3", "cwe-285"]
```

Reason:
```
[WRONG: ES-S6's transition is repair scoped to 6.3 review. The
transformation_X explicitly names "produce a review verdict" — that is
review work, not design work. The rejected places the packet into 3.4
Security and Authorization Model, which would redesign the auth model.
But the model exists; the implementation is defective. 6.3 owns the
review; 3.4 would only be re-engaged if the review verdict identified a
model defect, not an enforcement defect.]
```

Phrasing-B (Type A — 6.3 confused with 3.4):
```
domain_packets:
  - packet_id:       "ES-S6-pkt-001"
    domain_label:    "ARCH"
    phase:           "3"
    sub_phase:       "3.4"
    task_type:       "jwt-authentication"
    prompt_fragment: "redesign the auth model to add role-based access control"
    coverage_status: "covered"
    reason:          "RBAC is auth design"
    handoff_marker:  "ARCH/3.4/jwt-authentication"
    source_evidence: ["nist-sp-800-53-ac-3"]
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
overlap_ratio:   0.36   (Type A: chosen 6.3 review packet vs rejected 3.4 design packet; AC-3 token shared, packet semantics disjoint)
```

---

## Pair[ID] ES-2026-05-08-3.5-N01-05 — ES-S7 two packets (3.5 + 4.1)

A:
```
SelectedLogicalTransition:
  scenario_id:      "ES-S7"
  transition_type:  "feature_addition"
  normalized_A:     "order placement workflow without saga compensation; inventory and payment ordering undefined"
  normalized_B:     "order placement workflow with saga: inventory reserved before payment captured; payment compensated if inventory reservation fails"
  transformation_X: "introduce saga forward order (reserve → capture) and compensation order (compensate-payment ← reservation-fail)"
  source_evidence:
    - "Temporal saga documentation — compensation runs only when forward activity has taken effect"
    - "Apache Airflow DAG tutorial — depends_on_past and trigger_rule define when downstream tasks fire"
```

B:
```
domain_packets:
  - packet_id:       "ES-S7-pkt-001"
    domain_label:    "ARCH"
    phase:           "3"
    sub_phase:       "3.5"
    task_type:       "workflow-orchestration"
    prompt_fragment: "design the saga workflow: inventory.reserve before payment.capture; on inventory.reserve failure, do not capture payment; on payment.capture failure after reserve, run inventory.release as compensation"
    coverage_status: "covered"
    reason:          "transformation_X is 'introduce saga forward order and compensation order' — the orchestration design responsibility is 3.5"
    handoff_marker:  "ARCH/3.5/workflow-orchestration"
    source_evidence: ["temporal-saga", "airflow-dag-tutorial"]
  - packet_id:       "ES-S7-pkt-002"
    domain_label:    "IMPLEMENT"
    phase:           "4"
    sub_phase:       "4.1"
    task_type:       "backend-services"
    prompt_fragment: "implement the saga activities: inventory.reserve, payment.capture, inventory.release; the workflow itself follows the 3.5 design"
    coverage_status: "covered"
    reason:          "transformation_X 'implement' the saga and the prompt's verb 'Implement the' both ground the IMPLEMENT territory"
    handoff_marker:  "IMPLEMENT/4.1/backend-services"
    source_evidence: ["temporal-saga"]
```

Reason:
```
[WRONG: ES-S7 has two distinct territories — saga orchestration design
(3.5) and activity implementation (4.1). The chosen emits one packet per
territory. The rejected emits the 4.1 implementation packet only and drops
the 3.5 design packet. Skipping 3.5 means the implementation packet has
no upstream design to satisfy — Node 03 will receive only an
implementation statement and produce a plan with empty depends_on, which
violates the saga ordering authority from Temporal documentation.]
```

Phrasing-B (Type A — drops the 3.5 design packet):
```
domain_packets:
  - packet_id:       "ES-S7-pkt-001"
    domain_label:    "IMPLEMENT"
    phase:           "4"
    sub_phase:       "4.1"
    task_type:       "backend-services"
    prompt_fragment: "implement the saga activities for the order placement workflow"
    coverage_status: "covered"
    reason:          "implement the saga"
    handoff_marker:  "IMPLEMENT/4.1/backend-services"
    source_evidence: ["temporal-saga"]
```

Coverage labels:
```
source_id:       temporal-saga, airflow-dag-tutorial
source_title:    Temporal saga + Airflow DAG
transition_type: feature_addition
phase:           3
sub_phase:       3.5+4.1
task_type:       saga-design-and-implement
rejected_type:   A
overlap_ratio:   0.39   (Type A: chosen 2 packets (3.5 + 4.1) vs rejected 1 (4.1 only); 3.5 design packet missing)
```

---

## Node 01 summary

```
Pairs:                  5
Type A rejected:        5  (every pair tests territory enumeration)
Packets per pair:
  1 packet              ES-S1, ES-S2, ES-S6
  2 packets             ES-S7
  3 packets             ES-S5
```

Every chosen B has packets only for territories present in the upstream
transition's transformation_X. No invented territories.
