# Node 06 — Result Translator Pairs

```
Contract:
  ResultTranslatorUnit
      : ILogicalUnit<AppliedBranchResult[], TranslatedResultFragment[]>
```

## Common coverage labels (apply to every pair in this file)

```
source_license_state: external_license_checked
contract_signature:   ResultTranslatorUnit
node_role:            node_06
pair_kind:            flow_node
coverage_status:      covered
attribution_required: false
```

The complete v8 §"Required Coverage Labels" set per pair (including
`source_id`, `source_title`, `transition_type`, `task_type`, and the
`overlap_ratio` token-overlap-gate value) is recorded in
**`Coverage-Labels-Sweep.md`** under "Node 06 — ResultTranslatorUnit".

Token-overlap gate: every rejected satisfies overlap_ratio < 0.60
(measured values 0.43–0.45 for the three pairs).

Node 06 receives the **full set** of applied branch results — not array-node.
B may fold many results into fewer translated fragments but per v8 §"Node 06"
**every status class must remain visible**. Folding cannot collapse
accepted, blocked, rejected, and knowledge_edge into a single fragment.

Three pairs in this file — sampled across the two scenarios with multiple
applied branches (ES-S5 with three branches, ES-S7 with two), plus ES-S6
with one branch but multiple status classes.

---

## Pair[ID] ES-2026-05-08-3.4-N06-01 — ES-S5 three branches → translated fragments

A:
```
AppliedBranchResult[]:
  - branch_id: "branch-1"
    current_state_A: "API has no tenant-aware JWT authentication"
    target_state_B:  "API enforces JWT with iss/aud/exp/tenant; alg=none rejected; bypass list"
    accepted_items:  ["iss claim required", "aud claim required", "exp validated", "alg=none rejected", "bypass list"]
    knowledge_edges: ["tenant_claim format unresolved — handoff to branch-2"]
  - branch_id: "branch-2"
    current_state_A: "no API contract for the JWT model"
    target_state_B:  "OpenAPI fragment publishes Authorization: Bearer header; 401 on missing; 403 on tenant mismatch; tenant_claim format=string"
    accepted_items:  ["Authorization header required", "401 on missing bearer", "403 on tenant mismatch", "tenant_claim format=string (resolves branch-1 handoff)"]
    knowledge_edges: ["3.3 unregistered — fragment is knowledge-edge"]
  - branch_id: "branch-3"
    current_state_A: "no integration tests for tenant isolation"
    target_state_B:  "integration tests cover cross-tenant access denial against real Postgres"
    accepted_items:  ["real container fixture", "tenant_a → /orders/order_b → 403", "tenant_a → /orders/order_a → 200", "no auth → 401"]
    rejected_items:  ["mocked database fixture"]
    knowledge_edges: ["5.2 unregistered — fragment is knowledge-edge"]
```

B:
```
TranslatedResultFragment[]:
  - fragment_id:      "frag-3.4-accepted"
    bound_branches:   ["branch-1"]
    fragment_text:    "JWT bearer authentication model accepted: iss, aud, exp, tenant claims required; alg=none rejected per RFC 8725; /health, /metrics, /version are bypass paths."
    status_class:     "accepted"
    evidence_ids:     ["rfc-7519", "rfc-8725"]
  - fragment_id:      "frag-3.3-knowledge-edge"
    bound_branches:   ["branch-2"]
    fragment_text:    "API contract derived from the JWT model: Authorization: Bearer required; 401 on missing; 403 on tenant mismatch; tenant_claim format=string. Sub-phase 3.3 has no registered flow — fragment carried as knowledge edge."
    status_class:     "knowledge_edge"
    evidence_ids:     ["openapi-specification", "rfc-6749", "branch-1-derived"]
  - fragment_id:      "frag-5.2-knowledge-edge"
    bound_branches:   ["branch-3"]
    fragment_text:    "Integration test plan accepted: real Postgres container, two seeded tenants, three test cases. Mocked database fixture rejected — incompatible with cross-row isolation. Sub-phase 5.2 has no registered flow — fragment carried as knowledge edge."
    status_class:     "knowledge_edge"
    evidence_ids:     ["testcontainers-dotnet", "branch-1-derived", "branch-2-derived"]
```

Reason:
```
[WRONG: chosen produces three fragments — one per status class — and
preserves the knowledge_edge status on the two unregistered sub-phases.
The rejected folds all three branches into one fragment with status_class
"accepted", erasing the two knowledge_edge fragments. v8 §Node 06 is
explicit: every status class must remain visible after folding. Folding
across status classes destroys the operator's ability to see what is
registered vs what is at the knowledge edge.]
```

Phrasing-B (Type A — folds across status classes):
```
TranslatedResultFragment[]:
  - fragment_id:      "frag-all"
    bound_branches:   ["branch-1", "branch-2", "branch-3"]
    fragment_text:    "JWT model, API contract, and integration tests all accepted."
    status_class:     "accepted"
    evidence_ids:     ["rfc-7519", "openapi-specification", "testcontainers-dotnet"]
```

Coverage labels:
```
contract_signature: ResultTranslatorUnit
phase: 3
sub_phase: 3.4+3.3+5.2
node_role: node_06
coverage_status: covered
pair_kind: flow_node
rejected_type: A
```

---

## Pair[ID] ES-2026-05-08-3.5-N06-02 — ES-S7 two branches → fragments preserving rejected items

A:
```
AppliedBranchResult[]:
  - branch_id: "branch-1"
    current_state_A: "no saga; ordering undefined"
    target_state_B:  "saga forward (reserve → capture) + compensation (compensate-payment ← reservation-fail)"
    accepted_items:  ["forward step 1: inventory.reserve", "forward step 2: payment.capture (depends on step 1)", "compensation: inventory.release ONLY when step 2 fails after step 1 success", "no compensation when step 1 fails", "idempotent activities"]
    rejected_items:  ["inventory.release on inventory.reserve failure — Temporal saga rule violation"]
    knowledge_edges: ["compensation_timeout unresolved"]
  - branch_id: "branch-2"
    current_state_A: "activities not implemented"
    target_state_B:  "activities implemented per branch-1 declaration"
    accepted_items:  ["inventory.reserve(orderId, items)", "payment.capture(orderId, amount)", "inventory.release(orderId)", "all activities idempotent"]
    knowledge_edges: ["4.1 unregistered — fragment is knowledge-edge"]
```

B:
```
TranslatedResultFragment[]:
  - fragment_id:      "frag-3.5-accepted"
    bound_branches:   ["branch-1"]
    fragment_text:    "Saga workflow declaration accepted: forward step 1 inventory.reserve before step 2 payment.capture; compensation inventory.release runs only when step 2 fails after step 1 success; no compensation when step 1 fails; activities are idempotent."
    status_class:     "accepted"
    evidence_ids:     ["temporal-saga"]
  - fragment_id:      "frag-3.5-rejected"
    bound_branches:   ["branch-1"]
    fragment_text:    "Rejected: inventory.release on inventory.reserve failure. This violates the Temporal saga rule that compensation runs only when the forward activity took effect — releasing stock that was never reserved."
    status_class:     "rejected"
    evidence_ids:     ["temporal-saga"]
  - fragment_id:      "frag-3.5-knowledge-edge"
    bound_branches:   ["branch-1"]
    fragment_text:    "Knowledge edge: compensation_timeout (how long to attempt inventory.release before alerting) is unresolved and handed off to a future 3.5 observability scenario."
    status_class:     "knowledge_edge"
    evidence_ids:     ["temporal-saga"]
  - fragment_id:      "frag-4.1-knowledge-edge"
    bound_branches:   ["branch-2"]
    fragment_text:    "Activity implementations defined per branch-1 declaration: inventory.reserve, payment.capture, inventory.release. Sub-phase 4.1 has no registered flow — fragment carried as knowledge edge."
    status_class:     "knowledge_edge"
    evidence_ids:     ["temporal-saga"]
```

Reason:
```
[WRONG: branch-1's rejected_items contains the saga anti-pattern; that
rejection is the result of upstream Node 04 and Node 05 protecting the
Temporal authority. Chosen B emits a dedicated rejected fragment that
keeps this rejection visible. The rejected B drops the rejected fragment
and folds the rejected_items into a parenthetical inside the accepted
fragment text — which violates v8 §Node 06's status-class visibility
requirement and removes the operator's ability to see the rejection in
the formatted output.]
```

Phrasing-B (Type A — folds rejected_items into accepted fragment):
```
TranslatedResultFragment[]:
  - fragment_id:      "frag-3.5-accepted"
    bound_branches:   ["branch-1"]
    fragment_text:    "Saga workflow accepted (release-on-reserve-failure was considered but not used)."
    status_class:     "accepted"
    evidence_ids:     ["temporal-saga"]
  - fragment_id:      "frag-4.1-accepted"
    bound_branches:   ["branch-2"]
    fragment_text:    "Activity implementations accepted."
    status_class:     "accepted"
    evidence_ids:     ["temporal-saga"]
```

Coverage labels:
```
contract_signature: ResultTranslatorUnit
phase: 3
sub_phase: 3.5+4.1
node_role: node_06
coverage_status: covered
pair_kind: flow_node
rejected_type: A
```

---

## Pair[ID] ES-2026-05-08-6.3-N06-03 — ES-S6 single branch → fragments preserving block and rejection

A:
```
AppliedBranchResult[]:
  - branch_id: "branch-1"
    current_state_A: "GET /admin/users accepts authenticated regular user — AC-3 violated"
    target_state_B:  "verdict published with named gap and remediation evidence requirements"
    accepted_items:  ["concern verdict", "named gap", "CWE-285 mapping", "AC-3 violation cited", "required evidence: role gate", "required evidence: regression test"]
    blocked_items:   ["deployment to production blocked until both remediation evidence items are supplied"]
    rejected_items:  ["auto-pass without remediation evidence", "reject verdict (gap is bounded; remediation feasible)"]
    knowledge_edges: ["6.3 unregistered — fragment is knowledge-edge"]
```

B:
```
TranslatedResultFragment[]:
  - fragment_id:      "frag-6.3-concern"
    bound_branches:   ["branch-1"]
    fragment_text:    "6.3 review concern: GET /admin/users does not enforce role=admin claim. CWE-285 (Improper Authorization). NIST AC-3 violated. Remediation evidence required: a role gate returning 403 when role != admin AND a regression test that exercises the gate."
    status_class:     "concern"
    evidence_ids:     ["cwe-285", "nist-sp-800-53-ac-3"]
  - fragment_id:      "frag-6.3-blocked"
    bound_branches:   ["branch-1"]
    fragment_text:    "Production deployment is blocked until both remediation evidence items (role gate + regression test) are supplied."
    status_class:     "blocked"
    evidence_ids:     ["nist-sp-800-53-ac-3"]
  - fragment_id:      "frag-6.3-rejected-alternatives"
    bound_branches:   ["branch-1"]
    fragment_text:    "Auto-pass rejected: a documented AC-3 violation cannot be silently accepted. Reject verdict rejected: the gap is bounded and remediation is feasible within the current architecture, so concern (not reject) is correct."
    status_class:     "rejected"
    evidence_ids:     ["nist-sp-800-53-ac-3"]
  - fragment_id:      "frag-6.3-knowledge-edge"
    bound_branches:   ["branch-1"]
    fragment_text:    "6.3 has no registered flow — review verdict carried as knowledge-edge fragment for downstream arbitration."
    status_class:     "knowledge_edge"
    evidence_ids:     []
```

Reason:
```
[WRONG: chosen B emits four fragments, one per status class
(concern/accepted, blocked, rejected, knowledge_edge). The rejected
emits one "concern" fragment and discards the deployment block, the
rejected alternatives, and the knowledge-edge marker. Once flattened
this way Node 09 has no input to format the deployment block — and the
operator may merge to production without seeing the block. v8 §Node 06
explicitly requires every status class to remain visible.]
```

Phrasing-B (Type A — flattens to a single fragment):
```
TranslatedResultFragment[]:
  - fragment_id:      "frag-6.3-concern"
    bound_branches:   ["branch-1"]
    fragment_text:    "6.3 review verdict: concern; missing role gate on /admin/users; CWE-285 + AC-3."
    status_class:     "concern"
    evidence_ids:     ["cwe-285", "nist-sp-800-53-ac-3"]
```

Coverage labels:
```
contract_signature: ResultTranslatorUnit
phase: 6
sub_phase: 6.3
node_role: node_06
coverage_status: covered
pair_kind: flow_node
rejected_type: A
```

---

## Node 06 summary

```
Pairs:                  3
Type A rejected:        3 (each rejects a different folding error)
Status classes used:    accepted, knowledge_edge, rejected, blocked, concern
Fragment counts:        ES-S5 → 3 fragments; ES-S7 → 4; ES-S6 → 4
```

Every chosen B preserves the status classes from the upstream
AppliedBranchResult set. No fold collapses two status classes into one.
