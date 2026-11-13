# Node 04 — Domain Processor Pairs

```
Contract:
  DomainProcessorUnit : ILogicalUnit<WorkBranch, ProcessedBranchResult>
```

## Common coverage labels (apply to every pair in this file)

```
source_license_state: external_license_checked
contract_signature:   DomainProcessorUnit
node_role:            node_04
pair_kind:            flow_node
coverage_status:      covered
attribution_required: false
```

The complete v8 §"Required Coverage Labels" set per pair (including
`source_id`, `source_title`, `transition_type`, `task_type`, and the
`overlap_ratio` token-overlap-gate value) is recorded in
**`Coverage-Labels-Sweep.md`** under "Node 04 — DomainProcessorUnit".

Token-overlap gate: every rejected satisfies overlap_ratio < 0.60
(measured values 0.36–0.41 for the four pairs).

Node 04 is an array-node — A is **exactly one** WorkBranch. B states what
was derived locally, what constraints were preserved, what cannot be
completed locally, and which evidence ids support the result.

Required B fields:

```
branch_id, derived_artifact, preserved_constraints,
locally_unresolved, evidence_ids, status
```

Four pairs in this file — sampled from chosen branches of Node 03.

---

## Pair[ID] ES-2026-05-08-3.4-N04-01 — ES-S5 branch-1 JWT model derivation

A:
```
WorkBranch:
  branch_id:                          "branch-1"
  domain_packet_ids:                  ["ES-S5-pkt-001"]
  action:                             "Define the JWT authentication model: required claims, signature algorithm, expiry handling, bypass paths. Authority: RFC 7519."
  produces:                           "AuthenticationModel artifact"
  depends_on:                         []
  parallel_group:                     "g1"
  wait_state:                         "none"
  recovery_path:                      "if model has unresolved bypass conflicts, return to the translator with the conflict list"
  boundary_or_knowledge_edge_status:  "edge_present — 3.4 registered, will route to security flow"
```

B:
```
ProcessedBranchResult:
  branch_id:             "branch-1"
  derived_artifact:
    AuthenticationModel:
      required_claims:    ["iss", "aud", "exp", "tenant"]
      forbidden_alg:      ["none"]
      expiry_rule:        "exp <= now ⇒ reject"
      bypass_paths:       ["/health", "/metrics", "/version"]
      tenant_claim_role:  "authorization key"
      signature_authority:"RFC 7519 §4 + RFC 7515 §4.1.1"
  preserved_constraints:
    - "RFC 7519 §4.1.1 iss claim required"
    - "RFC 7519 §4.1.3 aud claim required"
    - "RFC 7519 §4.1.4 exp claim required and validated"
    - "alg=none rejected per RFC 8725 §2.1"
  locally_unresolved:
    - "tenant_claim format (string vs guid) requires API contract decision in branch-2"
  evidence_ids:           ["rfc-7519", "rfc-6749", "rfc-8725"]
  status:                 "derived_with_one_unresolved"
```

Reason:
```
[WRONG: branch-1 must derive the model and explicitly preserve the RFC
authority constraints; the unresolved tenant_claim format is a real
boundary that the contract branch must close. The rejected omits the
locally_unresolved entry and claims status "fully_derived". This hides
a real handoff to branch-2: if Node 04 declares the model fully
resolved, downstream Node 05 will apply it and Node 03's depends_on
edge to branch-2 becomes meaningless. Locally_unresolved is the
mechanism that surfaces real cross-branch handoffs — suppressing it
breaks the orchestration contract.]
```

Phrasing-B (Type A — suppresses locally_unresolved):
```
ProcessedBranchResult:
  branch_id:             "branch-1"
  derived_artifact:
    AuthenticationModel:
      required_claims:    ["iss", "aud", "exp", "tenant"]
      forbidden_alg:      ["none"]
      expiry_rule:        "exp <= now ⇒ reject"
      bypass_paths:       ["/health", "/metrics", "/version"]
      tenant_claim_role:  "authorization key"
  preserved_constraints: ["RFC 7519"]
  locally_unresolved:    []
  evidence_ids:          ["rfc-7519"]
  status:                "fully_derived"
```

Coverage labels:
```
contract_signature: DomainProcessorUnit
phase: 3
sub_phase: 3.4
node_role: node_04
coverage_status: covered
pair_kind: flow_node
rejected_type: A
```

---

## Pair[ID] ES-2026-05-08-5.2-N04-02 — ES-S5 branch-3 integration test plan derivation

A:
```
WorkBranch:
  branch_id:                          "branch-3"
  domain_packet_ids:                  ["ES-S5-pkt-003"]
  action:                             "Write integration tests for tenant isolation using the model from branch-1 and the contract from branch-2."
  produces:                           "TestPlan artifact"
  depends_on:                         ["branch-1", "branch-2"]
  parallel_group:                     "g3"
  wait_state:                         "wait_for_branch_1_and_branch_2"
  boundary_or_knowledge_edge_status:  "edge_present — 5.2 unregistered"
```

B:
```
ProcessedBranchResult:
  branch_id:             "branch-3"
  derived_artifact:
    TestPlan:
      fixture:           "TestcontainersBuilder<PostgreSqlContainer>().Build() per test class, no shared state"
      seeded_tenants:    ["tenant_a", "tenant_b"]
      seeded_orders:     ["order_a (owner=tenant_a)", "order_b (owner=tenant_b)"]
      test_cases:
        - "GIVEN tenant_a JWT, WHEN GET /orders/order_b, THEN 403"
        - "GIVEN tenant_a JWT, WHEN GET /orders/order_a, THEN 200"
        - "GIVEN no Authorization header, WHEN GET /orders/order_a, THEN 401"
      mocks_forbidden:   "database mocking is forbidden — the tenant filter must be exercised by real SQL"
  preserved_constraints:
    - "Testcontainers documentation — real container required for cross-row access verification"
    - "RFC 7519 — JWT must carry iss, aud, exp, tenant"
    - "branch-2 contract — 401 on missing bearer; 403 on tenant mismatch"
  locally_unresolved:    []
  evidence_ids:           ["testcontainers-dotnet", "rfc-7519", "branch-2-derived"]
  status:                 "fully_derived"
```

Reason:
```
[WRONG: branch-3's chosen result preserves the Testcontainers authority
that mocks are forbidden for tenant-isolation tests, and binds the test
cases to the model + contract from upstream branches. The rejected
allows mocks_allowed: "true" with a note that mocks reduce flake — but
mocking the database in a tenant-isolation test means the tenant filter
is never executed against real data, which is the exact 5.1 vs 5.2
boundary that Testcontainers documentation defines. Allowing mocks here
collapses 5.2 into 5.1 and the test cannot prove the property the
prompt asks about.]
```

Phrasing-B (Type A — collapses 5.2 into 5.1 by allowing mocks):
```
ProcessedBranchResult:
  branch_id:             "branch-3"
  derived_artifact:
    TestPlan:
      fixture:           "Mocked IOrderRepository per test"
      mocks_allowed:     "true"
      mocks_note:        "mocks reduce flake; real container can be added later"
      test_cases:
        - "GIVEN tenant_a JWT, WHEN OrdersController.Get(order_b), THEN 403 (mocked repository returns null for cross-tenant lookup)"
  preserved_constraints: []
  locally_unresolved:    []
  evidence_ids:           ["xunit-pattern"]
  status:                 "fully_derived"
```

Coverage labels:
```
contract_signature: DomainProcessorUnit
phase: 5
sub_phase: 5.2
node_role: node_04
coverage_status: covered
pair_kind: flow_node
rejected_type: A
```

---

## Pair[ID] ES-2026-05-08-3.5-N04-03 — ES-S7 branch-1 saga design derivation

A:
```
WorkBranch:
  branch_id:                          "branch-1"
  domain_packet_ids:                  ["ES-S7-pkt-001"]
  action:                             "Design the saga workflow declaration: forward order (reserve → capture) and compensation order (compensate-payment ← reservation-fail). Authority: Temporal saga documentation."
  produces:                           "WorkflowDeclaration artifact"
  depends_on:                         []
  parallel_group:                     "g1"
  wait_state:                         "none"
  boundary_or_knowledge_edge_status:  "edge_present — 3.5 unregistered"
```

B:
```
ProcessedBranchResult:
  branch_id:             "branch-1"
  derived_artifact:
    WorkflowDeclaration:
      forward_steps:
        - "1. inventory.reserve(orderId, items)  — must succeed before step 2"
        - "2. payment.capture(orderId, amount)   — runs only after step 1 success"
      compensation_steps:
        - "If step 2 fails AFTER step 1 succeeded: run inventory.release(orderId)"
        - "If step 1 fails: NO compensation runs (nothing has taken effect yet)"
      authority_quote:   "Temporal saga: 'compensation runs only when the forward activity has taken effect'"
  preserved_constraints:
    - "Temporal saga compensation rule — compensation never runs for an activity that did not take effect"
    - "Activities are idempotent (allows safe retry)"
  locally_unresolved:
    - "compensation timeout (how long to attempt inventory.release before alerting) requires 3.5 observability decision in a future scenario"
  evidence_ids:           ["temporal-saga"]
  status:                 "derived_with_one_unresolved"
```

Reason:
```
[WRONG: chosen B preserves the Temporal authority quote and explicitly
encodes the rule that compensation runs only when the forward activity
took effect. The rejected encodes the inverse: inventory.release runs
on inventory.reserve failure. Temporal documentation is explicit that
this is the saga anti-pattern — compensating an activity that never
ran corrupts the inventory state by releasing stock that was never
reserved. The rejected violates the authority quote it claims to follow.]
```

Phrasing-B (Type A — inverts the saga rule):
```
ProcessedBranchResult:
  branch_id:             "branch-1"
  derived_artifact:
    WorkflowDeclaration:
      forward_steps:
        - "1. inventory.reserve(orderId, items)"
        - "2. payment.capture(orderId, amount)"
      compensation_steps:
        - "If step 1 fails: run inventory.release(orderId) for symmetry"
        - "If step 2 fails: alert"
      authority_quote:   "Temporal saga: compensation provides symmetry"
  preserved_constraints: []
  locally_unresolved:    []
  evidence_ids:          ["temporal-saga"]
  status:                "fully_derived"
```

Coverage labels:
```
contract_signature: DomainProcessorUnit
phase: 3
sub_phase: 3.5
node_role: node_04
coverage_status: covered
pair_kind: flow_node
rejected_type: A
```

---

## Pair[ID] ES-2026-05-08-6.3-N04-04 — ES-S6 branch-1 review verdict derivation

A:
```
WorkBranch:
  branch_id:                          "branch-1"
  domain_packet_ids:                  ["ES-S6-pkt-001"]
  action:                             "Issue a 6.3 review verdict: concern, named gap = missing role gate on GET /admin/users. Cite CWE-285 and AC-3."
  produces:                           "ReviewVerdict artifact"
  depends_on:                         []
  parallel_group:                     "g1"
  wait_state:                         "none"
  boundary_or_knowledge_edge_status:  "edge_present — 6.3 unregistered"
```

B:
```
ProcessedBranchResult:
  branch_id:             "branch-1"
  derived_artifact:
    ReviewVerdict:
      verdict_class:           "concern"
      named_gap:               "GET /admin/users does not enforce role=admin claim"
      cwe_mapping:             "CWE-285 — Improper Authorization (authenticated but not authorized for resource)"
      nist_control_violation:  "NIST SP 800-53 AC-3 — system does not enforce approved authorizations for the admin resource"
      remediation_evidence_required:
        - "role gate: middleware that returns 403 when role != admin on /admin/* routes"
        - "regression test that exercises the gate with role != admin and asserts 403"
      reproduction_steps:
        - "1. Obtain JWT for a regular user (role=user)"
        - "2. Call GET /admin/users with that JWT"
        - "3. Observed: 200 OK with admin user list — expected: 403"
  preserved_constraints:
    - "AC-3: enforcement of approved authorizations is a MUST"
    - "CWE-285: authentication != authorization"
  locally_unresolved:    []
  evidence_ids:           ["cwe-285", "nist-sp-800-53-ac-3"]
  status:                 "fully_derived"
```

Reason:
```
[WRONG: 6.3 derives a concern verdict with named gap, control citation,
remediation evidence requirements, and reproduction steps. The rejected
upgrades the verdict to "reject" with no remediation evidence path —
which closes the door on the developer fixing it. A reject verdict at
6.3 is reserved for prompts where the implementation cannot be repaired
within the current architecture; here the gap is a single missing
middleware. Concern with named gap is the correct verdict because the
fix is bounded and the review identifies it.]
```

Phrasing-B (Type A — wrong verdict_class):
```
ProcessedBranchResult:
  branch_id:             "branch-1"
  derived_artifact:
    ReviewVerdict:
      verdict_class:           "reject"
      named_gap:               "RBAC missing"
      cwe_mapping:             "CWE-285"
      nist_control_violation:  "AC-3"
      remediation_evidence_required: []
      reproduction_steps:      []
  preserved_constraints: []
  locally_unresolved:    []
  evidence_ids:           ["cwe-285"]
  status:                 "fully_derived"
```

Coverage labels:
```
contract_signature: DomainProcessorUnit
phase: 6
sub_phase: 6.3
node_role: node_04
coverage_status: covered
pair_kind: flow_node
rejected_type: A
```

---

## Node 04 summary

```
Pairs:                  4
Type A rejected:        4
Sub-phases covered:     3.4, 5.2, 3.5, 6.3
locally_unresolved:     2 of 4 chosen results carry real handoffs (3.4, 3.5)
status values used:     fully_derived, derived_with_one_unresolved
```
