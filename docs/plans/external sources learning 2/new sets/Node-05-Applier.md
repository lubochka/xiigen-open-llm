# Node 05 — Applier Pairs

```
Contract:
  ApplierUnit : ILogicalUnit<ProcessedBranchResult, AppliedBranchResult>
```

## Common coverage labels (apply to every pair in this file)

```
source_license_state: external_license_checked
contract_signature:   ApplierUnit
node_role:            node_05
pair_kind:            flow_node
coverage_status:      covered
attribution_required: false
```

The complete v8 §"Required Coverage Labels" set per pair (including
`source_id`, `source_title`, `transition_type`, `task_type`, and the
`overlap_ratio` token-overlap-gate value) is recorded in
**`Coverage-Labels-Sweep.md`** under "Node 05 — ApplierUnit".

Token-overlap gate: every rejected satisfies overlap_ratio < 0.60
(measured values 0.37–0.42 for the four pairs).

Node 05 is an array-node — A is **exactly one** ProcessedBranchResult.
B preserves the functional principle of the orchestrator:

```
current_state_A, target_state_B, transformation_X,
accepted_items, blocked_items, rejected_items, knowledge_edges, next_action
```

Four pairs, sampled from the chosen Node 04 outputs.

---

## Pair[ID] ES-2026-05-08-3.4-N05-01 — ES-S5 branch-1 JWT model applied

A:
```
ProcessedBranchResult:
  branch_id:             "branch-1"
  derived_artifact:
    AuthenticationModel:
      required_claims:    ["iss", "aud", "exp", "tenant"]
      forbidden_alg:      ["none"]
      bypass_paths:       ["/health", "/metrics", "/version"]
  preserved_constraints: ["RFC 7519 §4.1.1", "RFC 7519 §4.1.3", "RFC 7519 §4.1.4", "RFC 8725 §2.1"]
  locally_unresolved:    ["tenant_claim format"]
  status:                "derived_with_one_unresolved"
```

B:
```
AppliedBranchResult:
  branch_id:        "branch-1"
  current_state_A:  "API has no tenant-aware JWT authentication"
  target_state_B:   "API enforces JWT with iss/aud/exp/tenant claims; alg=none rejected; /health, /metrics, /version bypass"
  transformation_X: "introduce JWT validation middleware bound to RFC 7519 + RFC 8725 constraints"
  accepted_items:
    - "iss claim required (RFC 7519 §4.1.1)"
    - "aud claim required (RFC 7519 §4.1.3)"
    - "exp claim required and validated (RFC 7519 §4.1.4)"
    - "alg=none rejected (RFC 8725 §2.1)"
    - "bypass list: /health, /metrics, /version"
  blocked_items: []
  rejected_items: []
  knowledge_edges:
    - "tenant_claim format unresolved — handoff to branch-2 (3.3 contract)"
  next_action: "publish AuthenticationModel artifact; signal branch-2 that tenant_claim format input is required"
```

Reason:
```
[WRONG: applier preserves every status class. The chosen surfaces the
unresolved tenant_claim format as a knowledge_edges entry — that is the
mechanism that tells branch-2 it has work to do. The rejected promotes
the unresolved item into accepted_items as if it were already decided,
silencing the handoff. Once silenced, branch-2's depends_on edge becomes
informational only and the contract may be published with the wrong
tenant_claim format. Suppressing knowledge_edges entries is exactly the
status-class loss that violates the v8 ApplierUnit principle.]
```

Phrasing-B (Type A — promotes unresolved into accepted):
```
AppliedBranchResult:
  branch_id:        "branch-1"
  current_state_A:  "no JWT"
  target_state_B:   "JWT model enforced"
  transformation_X: "add JWT middleware"
  accepted_items:
    - "iss claim"
    - "aud claim"
    - "exp claim"
    - "alg=none rejected"
    - "bypass list"
    - "tenant_claim format = string"
  blocked_items: []
  rejected_items: []
  knowledge_edges: []
  next_action: "publish"
```

Coverage labels:
```
contract_signature: ApplierUnit
phase: 3
sub_phase: 3.4
node_role: node_05
coverage_status: covered
pair_kind: flow_node
rejected_type: A
```

---

## Pair[ID] ES-2026-05-08-5.2-N05-02 — ES-S5 branch-3 test plan applied

A:
```
ProcessedBranchResult:
  branch_id:             "branch-3"
  derived_artifact:
    TestPlan:
      fixture:           "TestcontainersBuilder<PostgreSqlContainer>"
      test_cases:
        - "tenant_a → /orders/order_b → 403"
        - "tenant_a → /orders/order_a → 200"
        - "no auth → 401"
      mocks_forbidden:   "true"
  preserved_constraints: ["Testcontainers", "RFC 7519", "branch-2 contract"]
  status:                "fully_derived"
```

B:
```
AppliedBranchResult:
  branch_id:        "branch-3"
  current_state_A:  "no integration tests for tenant isolation"
  target_state_B:   "integration tests cover tenant_a vs tenant_b cross-row access denial against a real Postgres container"
  transformation_X: "introduce tenant-isolation test suite with real-container fixture"
  accepted_items:
    - "real PostgreSQL container per test class (Testcontainers)"
    - "test case: tenant_a → /orders/order_b → 403"
    - "test case: tenant_a → /orders/order_a → 200"
    - "test case: no auth → 401"
  blocked_items: []
  rejected_items:
    - "mocked database fixture — incompatible with cross-row isolation verification"
  knowledge_edges:
    - "5.2 has no registered flow — applier output reaches Node 06 as knowledge-edge branch result"
  next_action: "register TestPlan artifact; downstream knowledge-edge branch"
```

Reason:
```
[WRONG: rejected_items is the mechanism that records what the applier
refused. The chosen explicitly rejects mocked fixtures with the reason
that they cannot verify cross-row isolation. The rejected silences this —
no rejected_items, no knowledge_edges. Downstream Node 06 cannot
distinguish "5.2 fully applied" from "5.2 reached knowledge-edge", and
Node 09 will format the result as if the testing work is registered and
complete. This breaks the orchestration honesty contract.]
```

Phrasing-B (Type A — drops rejected_items and knowledge_edges):
```
AppliedBranchResult:
  branch_id:        "branch-3"
  current_state_A:  "no integration tests"
  target_state_B:   "tests added"
  transformation_X: "add integration tests"
  accepted_items:   ["real container", "tenant_a vs tenant_b cases", "401 case"]
  blocked_items:    []
  rejected_items:   []
  knowledge_edges:  []
  next_action:      "publish"
```

Coverage labels:
```
contract_signature: ApplierUnit
phase: 5
sub_phase: 5.2
node_role: node_05
coverage_status: covered
pair_kind: flow_node
rejected_type: A
```

---

## Pair[ID] ES-2026-05-08-3.5-N05-03 — ES-S7 branch-1 saga applied

A:
```
ProcessedBranchResult:
  branch_id:             "branch-1"
  derived_artifact:
    WorkflowDeclaration:
      forward_steps:        ["1. inventory.reserve", "2. payment.capture"]
      compensation_steps:   ["If step 2 fails AFTER step 1: run inventory.release", "If step 1 fails: NO compensation"]
      authority_quote:      "Temporal saga compensation runs only when forward activity took effect"
  preserved_constraints: ["Temporal saga rule", "idempotent activities"]
  locally_unresolved:    ["compensation timeout"]
  status:                "derived_with_one_unresolved"
```

B:
```
AppliedBranchResult:
  branch_id:        "branch-1"
  current_state_A:  "order placement workflow without saga; ordering between inventory and payment undefined"
  target_state_B:   "order placement workflow with saga forward order (reserve → capture) and compensation order (compensate-payment ← reservation-fail)"
  transformation_X: "introduce workflow declaration with explicit forward and compensation order per Temporal saga rule"
  accepted_items:
    - "forward step 1: inventory.reserve"
    - "forward step 2: payment.capture (depends on step 1 success)"
    - "compensation: inventory.release ONLY when step 2 fails after step 1 success"
    - "no compensation when step 1 fails (forward activity did not take effect)"
    - "activities are idempotent"
  blocked_items: []
  rejected_items:
    - "inventory.release on inventory.reserve failure — Temporal saga rule violation; would release stock that was never reserved"
  knowledge_edges:
    - "compensation_timeout unresolved — handoff to a future 3.5 observability scenario"
  next_action: "publish WorkflowDeclaration; raise observability scenario for compensation_timeout"
```

Reason:
```
[WRONG: chosen records the explicit rejection of the saga anti-pattern
and surfaces compensation_timeout as a real knowledge-edge handoff. The
rejected merges the anti-pattern into accepted_items ("inventory.release
on reserve failure for symmetry"). This is the same Temporal saga
violation Node 04 already rejected — the applier's role is to preserve
that rejection, not undo it. If accepted, the workflow declaration
encodes a behavior that releases stock never reserved, which the
authority explicitly forbids.]
```

Phrasing-B (Type A — promotes anti-pattern into accepted):
```
AppliedBranchResult:
  branch_id:        "branch-1"
  current_state_A:  "no saga"
  target_state_B:   "saga in place"
  transformation_X: "add saga"
  accepted_items:
    - "forward steps 1, 2"
    - "compensation: inventory.release on any failure"
  blocked_items:    []
  rejected_items:   []
  knowledge_edges:  []
  next_action:      "publish"
```

Coverage labels:
```
contract_signature: ApplierUnit
phase: 3
sub_phase: 3.5
node_role: node_05
coverage_status: covered
pair_kind: flow_node
rejected_type: A
```

---

## Pair[ID] ES-2026-05-08-6.3-N05-04 — ES-S6 review verdict applied

A:
```
ProcessedBranchResult:
  branch_id:             "branch-1"
  derived_artifact:
    ReviewVerdict:
      verdict_class:                "concern"
      named_gap:                    "GET /admin/users does not enforce role=admin claim"
      cwe_mapping:                  "CWE-285"
      nist_control_violation:       "AC-3"
      remediation_evidence_required:["role gate", "regression test"]
  preserved_constraints: ["AC-3 MUST", "CWE-285"]
  status:                "fully_derived"
```

B:
```
AppliedBranchResult:
  branch_id:        "branch-1"
  current_state_A:  "GET /admin/users accepts authenticated regular user — AC-3 violated"
  target_state_B:   "verdict published with named gap and remediation evidence requirements"
  transformation_X: "issue concern verdict citing CWE-285 + AC-3 with bounded remediation list"
  accepted_items:
    - "concern verdict (not pass, not reject)"
    - "named gap: missing role=admin enforcement on /admin/users"
    - "CWE-285 mapping (Improper Authorization)"
    - "AC-3 control violation cited"
    - "required evidence: role gate that returns 403"
    - "required evidence: regression test exercising the gate"
  blocked_items:
    - "deployment to production blocked until both remediation evidence items are supplied"
  rejected_items:
    - "auto-pass without remediation evidence"
    - "reject verdict (gap is bounded; remediation is feasible within current architecture)"
  knowledge_edges:
    - "6.3 has no registered flow — applier output reaches Node 06 as knowledge-edge branch result"
  next_action: "publish ReviewVerdict; block deployment; await remediation evidence"
```

Reason:
```
[WRONG: chosen preserves blocked_items (deployment block) and
rejected_items (auto-pass; reject) — both are status classes that the
applier must surface for downstream visibility. The rejected drops both
status classes entirely. Without blocked_items, Node 09 cannot include
the deployment block in the formatted human result; without
rejected_items, the operator cannot see why concern (not reject, not
pass) is the correct verdict. This collapses three status classes into
one ("accepted") and loses the orchestration distinctions.]
```

Phrasing-B (Type A — drops blocked_items and rejected_items):
```
AppliedBranchResult:
  branch_id:        "branch-1"
  current_state_A:  "RBAC missing"
  target_state_B:   "verdict issued"
  transformation_X: "issue concern verdict"
  accepted_items:
    - "concern verdict"
    - "named gap"
    - "CWE-285 + AC-3"
    - "remediation evidence list"
  blocked_items:    []
  rejected_items:   []
  knowledge_edges:  []
  next_action:      "publish"
```

Coverage labels:
```
contract_signature: ApplierUnit
phase: 6
sub_phase: 6.3
node_role: node_05
coverage_status: covered
pair_kind: flow_node
rejected_type: A
```

---

## Node 05 summary

```
Pairs:                  4
Type A rejected:        4
Status classes preserved across chosen B sides:
  accepted_items        4 of 4
  blocked_items         1 (ES-S6)
  rejected_items        3 (ES-S5 branch-3, ES-S7, ES-S6)
  knowledge_edges       3 (ES-S5 branch-1, ES-S5 branch-3, ES-S6)
```
