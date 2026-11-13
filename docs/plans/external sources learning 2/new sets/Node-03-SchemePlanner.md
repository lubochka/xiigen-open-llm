# Node 03 — Scheme Planner Pairs (PLAN priority — Type B target)

```
Contract:
  SchemePlannerUnit
      : ILogicalUnit<TranslatedDomainStatement[], WorkBranch[]>
```

## Common coverage labels (apply to every pair in this file)

```
source_license_state: external_license_checked
contract_signature:   SchemePlannerUnit
node_role:            node_03
pair_kind:            flow_node
coverage_status:      covered
attribution_required: false
```

The complete v8 §"Required Coverage Labels" set per pair (including
`source_id`, `source_title`, `transition_type`, `task_type`, and the
`overlap_ratio` token-overlap-gate value) is recorded in
**`Coverage-Labels-Sweep.md`** under "Node 03 — SchemePlannerUnit".
Every pair below is verifiable against that file by Pair[ID].

Token-overlap gate: every rejected satisfies overlap_ratio < 0.60
(measured values 0.30–0.46 for the five pairs).

Per v8 §"Layer 2 Node 03" Node 03 is the **highest-priority training
target** for PLAN accuracy. Every multi-task pair in this file uses a
**Type B rejected** — same branches as the chosen but with reversed or
missing dependency order. Type B carries most of the PLAN signal because
chosen and rejected share sub-phase names and the distinguishing tokens
must come from `depends_on` and the [WRONG: ...] annotation.

Required fields per branch:

```
branch_id, domain_packet_ids, action, produces, depends_on,
parallel_group, wait_state, recovery_path, boundary_or_knowledge_edge_status
```

Five pairs in this file — three Type B (multi-task), two Type A (single
branch where Type B does not apply).

---

## Pair[ID] ES-2026-05-08-3.4-N03-01 — ES-S5 three branches with explicit dependency chain

A:
```
TranslatedDomainStatement[]:
  - packet_id:        "ES-S5-pkt-001"
    control_type:     "authentication_model"
    formal_statement: "All API requests must carry an Authorization: Bearer <jwt> header. JWT signed (alg != none), claims iss/aud/exp/tenant required. Bypass paths: /health, /metrics, /version."
    handoff_marker:   "ARCH/3.4/jwt-authentication"
  - packet_id:        "ES-S5-pkt-002"
    control_type:     "api_contract"
    formal_statement: "Authorization: Bearer <jwt> header required on every protected endpoint. 401 on missing or invalid bearer; 403 on tenant mismatch."
    handoff_marker:   "ARCH/3.3/api-contract"
  - packet_id:        "ES-S5-pkt-003"
    control_type:     "integration_test_plan"
    formal_statement: "Real PostgreSQL container; two seeded tenants; assert tenant_a JWT cannot read tenant_b orders. Mocked database forbidden."
    handoff_marker:   "TEST/5.2/integration-tests"
```

B:
```
WorkBranch[]:
  - branch_id:                          "branch-1"
    domain_packet_ids:                  ["ES-S5-pkt-001"]
    action:                             "Define the JWT authentication model: required claims, signature algorithm, expiry handling, bypass paths. Authority: RFC 7519."
    produces:                           "AuthenticationModel artifact"
    depends_on:                         []
    parallel_group:                     "g1"
    wait_state:                         "none"
    recovery_path:                      "if model has unresolved bypass conflicts, return to the translator with the conflict list"
    boundary_or_knowledge_edge_status:  "edge_present — 3.4 registered, will route to security flow"
  - branch_id:                          "branch-2"
    domain_packet_ids:                  ["ES-S5-pkt-002"]
    action:                             "Define the API contract: header shape, status codes, error responses. Bind the contract to the model from branch-1."
    produces:                           "OpenAPI document fragment for the protected endpoint"
    depends_on:                         ["branch-1"]
    parallel_group:                     "g2"
    wait_state:                         "wait_for_branch_1_artifact"
    recovery_path:                      "if branch-1 changed bypass paths, update the contract before publishing"
    boundary_or_knowledge_edge_status:  "edge_present — 3.3 unregistered, will produce knowledge-edge result"
  - branch_id:                          "branch-3"
    domain_packet_ids:                  ["ES-S5-pkt-003"]
    action:                             "Write integration tests for tenant isolation using the model from branch-1 and the contract from branch-2."
    produces:                           "TestPlan artifact"
    depends_on:                         ["branch-1", "branch-2"]
    parallel_group:                     "g3"
    wait_state:                         "wait_for_branch_1_and_branch_2"
    recovery_path:                      "if model rejects bypass path the test relied on, regenerate the test scenarios"
    boundary_or_knowledge_edge_status:  "edge_present — 5.2 unregistered, will produce knowledge-edge result"
```

Reason:
```
[WRONG: ES-S5 contains three real dependencies. The 3.3 API contract
cannot be defined until the 3.4 model decides which claims must appear
in the bearer token. The 5.2 integration tests cannot be written until
both the model (branch-1) and the contract (branch-2) exist — mock
tokens cannot test real isolation per the DPO authoring guidance Type B
example. The rejected reverses the chain: tests first using mock tokens,
then the contract derived from passing tests, then the model added last.
This is the exact PLAN dependency-ordering anti-pattern in the
v8 reference table: "5.2 first with mock tokens, 3.4 added after". Mock
tokens bypass actual validation logic; integration tests built on mocks
cannot prove tenant isolation, only that the controller passes through
the mock data. The 3.4 implementation must exist before 5.2 tests can
run meaningfully.]
```

Phrasing-B (Type B — wrong dependency order):
```
WorkBranch[]:
  - branch_id:        "branch-1"
    domain_packet_ids:["ES-S5-pkt-003"]
    action:           "Write integration tests for tenant isolation using mock JWT tokens and a mocked database. The tests will define what the auth model must satisfy."
    produces:         "TestPlan artifact"
    depends_on:       []
    parallel_group:   "g1"
    wait_state:       "none"
  - branch_id:        "branch-2"
    domain_packet_ids:["ES-S5-pkt-002"]
    action:           "Define the API contract derived from the passing tests in branch-1."
    produces:         "OpenAPI document fragment"
    depends_on:       ["branch-1"]
    parallel_group:   "g2"
    wait_state:       "wait_for_branch_1"
  - branch_id:        "branch-3"
    domain_packet_ids:["ES-S5-pkt-001"]
    action:           "Define the JWT authentication model, ensuring it satisfies the tests from branch-1 and the contract from branch-2."
    produces:         "AuthenticationModel artifact"
    depends_on:       ["branch-1", "branch-2"]
    parallel_group:   "g3"
    wait_state:       "wait_for_branch_1_and_branch_2"
```

Coverage labels:
```
contract_signature: SchemePlannerUnit
phase: 3
sub_phase: 3.4+3.3+5.2
node_role: node_03
coverage_status: covered
pair_kind: flow_node
rejected_type: B
```

---

## Pair[ID] ES-2026-05-08-3.3-N03-02 — ES-S4 idempotency contract → tests dependency

A:
```
TranslatedDomainStatement[]:
  - packet_id:        "ES-S4-pkt-001"
    control_type:     "api_contract"
    formal_statement: "POST /payments accepts an optional Idempotency-Key request header. When present and previously seen within 24h, the server returns the original response. RFC 8470 / RFC 9110 idempotency semantics. OpenAPI parameter: in=header, name=Idempotency-Key, schema.type=string, schema.format=uuid."
    handoff_marker:   "ARCH/3.3/api-contract"
  - packet_id:        "ES-S4-pkt-002"
    control_type:     "integration_test_plan"
    formal_statement: "Integration test: send POST /payments with Idempotency-Key=K twice. Assert second response equals first response, no duplicate payment row created. Test against real Postgres + real handler."
    handoff_marker:   "TEST/5.2/integration-tests"
```

B:
```
WorkBranch[]:
  - branch_id:        "branch-1"
    domain_packet_ids:["ES-S4-pkt-001"]
    action:           "Define the Idempotency-Key contract per RFC 8470: header name, value format, retention window, replay semantics."
    produces:         "OpenAPI parameter definition + replay-window decision"
    depends_on:       []
    parallel_group:   "g1"
    wait_state:       "none"
    recovery_path:    "if replay window decision is undecidable, escalate to a contract review"
    boundary_or_knowledge_edge_status: "edge_present — 3.3 unregistered"
  - branch_id:        "branch-2"
    domain_packet_ids:["ES-S4-pkt-002"]
    action:           "Write the duplicate-request integration test driven by the contract from branch-1 — same Idempotency-Key produces the same response, no duplicate persistence."
    produces:         "TestPlan artifact"
    depends_on:       ["branch-1"]
    parallel_group:   "g2"
    wait_state:       "wait_for_branch_1"
    recovery_path:    "if branch-1 changes replay window, regenerate the test scenarios"
    boundary_or_knowledge_edge_status: "edge_present — 5.2 unregistered"
```

Reason:
```
[WRONG: the contract decides what idempotent means in this system —
which header name, which retention window, which response semantics.
Tests cannot meaningfully verify duplicate-request handling until those
decisions exist. The rejected reverses the order: tests first against an
imagined contract, then the contract written to match passing tests.
This is the v8 PLAN reference table row "3.3 API Contract → 4.5
Integration Connectors" / "4.5 first, contract written to match
implementation" applied to test plans instead of connectors. The
functional failure: the contract becomes a description of whatever the
test author chose to assert, not an authority that the implementation
must satisfy.]
```

Phrasing-B (Type B — tests before contract):
```
WorkBranch[]:
  - branch_id:        "branch-1"
    domain_packet_ids:["ES-S4-pkt-002"]
    action:           "Write the duplicate-request integration test against an assumed header name and replay window."
    produces:         "TestPlan artifact"
    depends_on:       []
    parallel_group:   "g1"
    wait_state:       "none"
  - branch_id:        "branch-2"
    domain_packet_ids:["ES-S4-pkt-001"]
    action:           "Document the idempotency contract to match what the tests in branch-1 already assert."
    produces:         "OpenAPI parameter definition"
    depends_on:       ["branch-1"]
    parallel_group:   "g2"
    wait_state:       "wait_for_branch_1"
```

Coverage labels:
```
contract_signature: SchemePlannerUnit
phase: 3
sub_phase: 3.3+5.2
node_role: node_03
coverage_status: covered
pair_kind: flow_node
rejected_type: B
```

---

## Pair[ID] ES-2026-05-08-3.5-N03-03 — ES-S7 saga forward order before compensation

A:
```
TranslatedDomainStatement[]:
  - packet_id:        "ES-S7-pkt-001"
    control_type:     "workflow_orchestration_design"
    formal_statement: "Saga forward order: inventory.reserve before payment.capture. Compensation order: when payment.capture fails after a successful inventory.reserve, run inventory.release. When inventory.reserve fails, do not capture payment (no compensation needed). Authority: Temporal saga documentation — compensation runs only when the forward activity has taken effect."
    handoff_marker:   "ARCH/3.5/workflow-orchestration"
  - packet_id:        "ES-S7-pkt-002"
    control_type:     "backend_implementation"
    formal_statement: "Implement activities: inventory.reserve(orderId, items), payment.capture(orderId, amount), inventory.release(orderId). Each activity is idempotent."
    handoff_marker:   "IMPLEMENT/4.1/backend-services"
```

B:
```
WorkBranch[]:
  - branch_id:        "branch-1"
    domain_packet_ids:["ES-S7-pkt-001"]
    action:           "Design the saga workflow declaration: forward order (reserve → capture) and compensation order (compensate-payment ← reservation-fail). Authority: Temporal saga documentation."
    produces:         "WorkflowDeclaration artifact"
    depends_on:       []
    parallel_group:   "g1"
    wait_state:       "none"
    recovery_path:    "if reserve and capture cannot be sequenced atomically, escalate to a saga-design review"
    boundary_or_knowledge_edge_status: "edge_present — 3.5 unregistered"
  - branch_id:        "branch-2"
    domain_packet_ids:["ES-S7-pkt-002"]
    action:           "Implement the activities in the order required by branch-1's declaration: reserve, capture, release. Activities are idempotent."
    produces:         "ActivityImplementations artifact"
    depends_on:       ["branch-1"]
    parallel_group:   "g2"
    wait_state:       "wait_for_branch_1_declaration"
    recovery_path:    "if implementation reveals impossible sequencing, return to branch-1 with the constraint"
    boundary_or_knowledge_edge_status: "edge_present — 4.1 unregistered (but routed)"
```

Reason:
```
[WRONG: Temporal saga documentation is explicit — the workflow
declaration owns the forward and compensation order; activities only
implement the work. The rejected reverses this: implement activities
first, then derive the workflow declaration from "what the activities
do". Compensation activities only run when the forward activity has
taken effect; if the activity implementer chooses an order that does not
satisfy this rule, the workflow declaration cannot retroactively repair
it. The rejected also models inventory.release as running on
inventory.reserve failure, which is the exact anti-pattern Temporal docs
flag: never compensate an activity that did not take effect.]
```

Phrasing-B (Type B — implementation before saga design):
```
WorkBranch[]:
  - branch_id:        "branch-1"
    domain_packet_ids:["ES-S7-pkt-002"]
    action:           "Implement the activities in any working order; observe the runtime behavior."
    produces:         "ActivityImplementations artifact"
    depends_on:       []
    parallel_group:   "g1"
    wait_state:       "none"
  - branch_id:        "branch-2"
    domain_packet_ids:["ES-S7-pkt-001"]
    action:           "Document the saga declaration to match the order the activities ended up running. inventory.release runs on inventory.reserve failure for symmetry."
    produces:         "WorkflowDeclaration artifact"
    depends_on:       ["branch-1"]
    parallel_group:   "g2"
    wait_state:       "wait_for_branch_1"
```

Coverage labels:
```
contract_signature: SchemePlannerUnit
phase: 3
sub_phase: 3.5+4.1
node_role: node_03
coverage_status: covered
pair_kind: flow_node
rejected_type: B
```

---

## Pair[ID] ES-2026-05-08-3.1-N03-04 — ES-S1 single 3.1 branch (Type A territory swap)

A:
```
TranslatedDomainStatement[]:
  - packet_id:        "ES-S1-pkt-001"
    control_type:     "ownership_boundary_rule"
    formal_statement: "Order service owns OrderLifecycleState. Inventory service owns StockState. Neither service reads the other's storage directly. Cross-service state changes go through the published service contracts only."
    handoff_marker:   "ARCH/3.1/architecture-topology"
```

B:
```
WorkBranch[]:
  - branch_id:        "branch-1"
    domain_packet_ids:["ES-S1-pkt-001"]
    action:           "Document the ownership boundary as an ADR: Decision (order owns OrderLifecycle, inventory owns Stock), Forbidden (cross-storage reads), Allowed (published service contracts). Authority: arc42 building-block view."
    produces:         "ADR record"
    depends_on:       []
    parallel_group:   "g1"
    wait_state:       "none"
    recovery_path:    "if a stakeholder challenges the boundary, return to ARCH translator with the dispute"
    boundary_or_knowledge_edge_status: "edge_present — 3.1 unregistered"
```

Reason:
```
[WRONG: a single ARCH/3.1 boundary statement produces a single ADR
branch with no dependencies. The rejected fabricates a depends_on of
"branch-0" that does not exist in this plan, and inserts a refactoring
action (4.4) that the splitter never produced. Inventing depends_on
edges to non-existent branches and inventing branches for territories
not present is the exact anti-pattern §3 of the v7 pipeline plan
forbids: do not add territories not in the selected transition.]
```

Phrasing-B (Type A — invented dependency and territory):
```
WorkBranch[]:
  - branch_id:        "branch-0"
    domain_packet_ids:[]
    action:           "Refactor the existing repository code to introduce a service boundary."
    produces:         "RefactorPlan artifact"
    depends_on:       []
    parallel_group:   "g0"
    wait_state:       "none"
  - branch_id:        "branch-1"
    domain_packet_ids:["ES-S1-pkt-001"]
    action:           "Document the ownership boundary as an ADR."
    produces:         "ADR record"
    depends_on:       ["branch-0"]
    parallel_group:   "g1"
    wait_state:       "wait_for_branch_0"
```

Coverage labels:
```
contract_signature: SchemePlannerUnit
phase: 3
sub_phase: 3.1
node_role: node_03
coverage_status: covered
pair_kind: flow_node
rejected_type: A
```

---

## Pair[ID] ES-2026-05-08-6.3-N03-05 — ES-S6 single 6.3 branch (Type A scope swap)

A:
```
TranslatedDomainStatement[]:
  - packet_id:        "ES-S6-pkt-001"
    control_type:     "compliance_review_finding"
    formal_statement: "Finding: GET /admin/users does not enforce role=admin claim. CWE-285 + AC-3. Required remediation evidence: a role gate AND a regression test."
    handoff_marker:   "QA/6.3/security-compliance-review"
```

B:
```
WorkBranch[]:
  - branch_id:        "branch-1"
    domain_packet_ids:["ES-S6-pkt-001"]
    action:           "Issue a 6.3 review verdict: concern, named gap = missing role gate on GET /admin/users. Cite CWE-285 and AC-3. Required remediation evidence: role gate that returns 403 when role != admin AND a regression test that exercises the gate."
    produces:         "ReviewVerdict artifact"
    depends_on:       []
    parallel_group:   "g1"
    wait_state:       "none"
    recovery_path:    "if remediation evidence is supplied, re-evaluate; if disputed, escalate to compliance"
    boundary_or_knowledge_edge_status: "edge_present — 6.3 unregistered"
```

Reason:
```
[WRONG: 6.3 produces a review verdict with required remediation
evidence — that is the planner's only branch for this scenario. The
rejected splits the work into a 6.3 verdict branch followed by a 4.1
implementation branch ("implement the role gate") that the splitter
never produced. The 6.3 review is allowed to demand remediation evidence
but it cannot author the remediation — that work is owned by 4.1 in a
separate transition once a remediation request enters the pipeline. The
planner must not invent the implementation branch here.]
```

Phrasing-B (Type A — invents an implementation branch):
```
WorkBranch[]:
  - branch_id:        "branch-1"
    domain_packet_ids:["ES-S6-pkt-001"]
    action:           "Issue the review verdict."
    produces:         "ReviewVerdict artifact"
    depends_on:       []
    parallel_group:   "g1"
    wait_state:       "none"
  - branch_id:        "branch-2"
    domain_packet_ids:[]
    action:           "Implement the role gate to satisfy the verdict from branch-1."
    produces:         "ImplementedRoleGate artifact"
    depends_on:       ["branch-1"]
    parallel_group:   "g2"
    wait_state:       "wait_for_branch_1"
```

Coverage labels:
```
contract_signature: SchemePlannerUnit
phase: 6
sub_phase: 6.3
node_role: node_03
coverage_status: covered
pair_kind: flow_node
rejected_type: A
```

---

## Node 03 summary

```
Pairs:                  5
Type B rejected:        3   (ES-S5, ES-S4, ES-S7) — multi-task wrong order
Type A rejected:        2   (ES-S1, ES-S6) — single-branch territory invention
Multi-task pairs:       3   (all three use Type B per v8 §"highest priority for PLAN training")
```

Each Type B annotation explains *why* the wrong order fails, not just
that the order is wrong (per DPO authoring guidance Error 5):

```
ES-S5: "mock tokens bypass actual validation logic and cannot test real isolation"
ES-S4: "contract becomes a description of whatever the test author chose to assert"
ES-S7: "compensation runs only when forward activity has taken effect"
```
