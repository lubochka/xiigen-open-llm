# Node 09 — Formatter Pairs

```
Contract:
  FormatterUnit : ILogicalUnit<ReviewedResultSet, HumanResult>
```

## Common coverage labels (apply to every pair in this file)

```
source_license_state: external_license_checked
contract_signature:   FormatterUnit
node_role:            node_09
pair_kind:            flow_node
coverage_status:      covered
attribution_required: false   (overridden to true on ES-S8 pair only)
```

The complete v8 §"Required Coverage Labels" set per pair (including
`source_id`, `source_title`, `transition_type`, `task_type`, and the
`overlap_ratio` token-overlap-gate value) is recorded in
**`Coverage-Labels-Sweep.md`** under "Node 09 — FormatterUnit".

Token-overlap gate: every rejected satisfies overlap_ratio < 0.60
(measured values 0.31–0.42 for the three pairs).

B is the final human-readable result, naming **accepted, blocked, rejected,
knowledge-edge, and next-action sections** wherever those statuses exist.
Per v8 §"Across the full trace: every constraint remains visible at
node-09; nothing disappears."

Three pairs in this file — multi-branch (ES-S5), single-branch with
multiple status classes (ES-S6), and Type C wrong-domain (ES-S8: explain
domain).

---

## Pair[ID] ES-2026-05-08-3.4-N09-01 — ES-S5 multi-branch human result

A:
```
ReviewedResultSet:
  scenario_id:        "ES-S5"
  fragments:
    - fragment_id: "frag-3.4-accepted"
      status_class: "accepted"
      fragment_text: "JWT bearer authentication model accepted: iss, aud, exp, tenant claims required; alg=none rejected per RFC 8725; /health, /metrics, /version are bypass paths."
    - fragment_id: "frag-3.3-knowledge-edge"
      status_class: "knowledge_edge"
      fragment_text: "API contract: Authorization: Bearer required; 401 on missing; 403 on tenant mismatch; tenant_claim format=string. 3.3 has no registered flow."
    - fragment_id: "frag-5.2-knowledge-edge"
      status_class: "knowledge_edge"
      fragment_text: "Integration test plan: real Postgres container, two seeded tenants, three test cases. Mocked database fixture rejected. 5.2 has no registered flow."
  arbiter_verdicts:
    - request_id: "arb-3.4-security"
      verdict:    "pass"
    - request_id: "arb-3.3-contract"
      verdict:    "pass"
```

B:
```
HumanResult:
  scenario_id:        "ES-S5"
  overall_verdict:    "MIXED — 3.4 PASS; 3.3 and 5.2 KNOWLEDGE_EDGE"
  body: |
    ## Accepted

    - 3.4 JWT bearer authentication model: iss, aud, exp, tenant claims required;
      alg=none rejected per RFC 8725; /health, /metrics, /version are bypass paths.
    - 3.3 API contract: Authorization: Bearer required; 401 on missing; 403 on
      tenant mismatch; tenant_claim format=string.

    ## Rejected

    - 5.2 mocked database fixture for tenant-isolation tests — Testcontainers
      documentation makes real-container fixtures the boundary for cross-row
      isolation verification.

    ## Knowledge edges

    - 3.3 API Contract Definition has no registered flow — contract is captured
      as a knowledge-edge fragment for future arbitration.
    - 5.2 Integration Tests has no registered flow — test plan is captured as a
      knowledge-edge fragment.

    ## Next action

    - Register a 3.3 flow (OpenAPI authority) and a 5.2 flow (Testcontainers
      authority) to advance both knowledge edges to PASS or CONCERN.
```

Reason:
```
[WRONG: chosen names every status class with a section heading and
preserves the rejected mocked-fixture decision and both knowledge-edge
notes — the operator can see 3.3 and 5.2 are unregistered. The rejected
emits "All accepted" with no rejected, knowledge-edge, or next-action
sections. The operator believes 3.3 and 5.2 are complete and registered,
which is wrong. v8 §Node 09 is explicit: every status class must remain
visible. Hiding the knowledge edges breaks the orchestration honesty
contract.]
```

Phrasing-B (Type A — drops knowledge-edge and rejected sections):
```
HumanResult:
  scenario_id:        "ES-S5"
  overall_verdict:    "PASS"
  body: |
    ## Accepted

    - JWT model accepted.
    - API contract accepted.
    - Integration test plan accepted.
```

Coverage labels:
```
contract_signature: FormatterUnit
phase: 3
sub_phase: 3.4+3.3+5.2
node_role: node_09
coverage_status: covered
pair_kind: flow_node
rejected_type: A
```

---

## Pair[ID] ES-2026-05-08-6.3-N09-02 — ES-S6 review verdict human result

A:
```
ReviewedResultSet:
  scenario_id:  "ES-S6"
  fragments:
    - fragment_id: "frag-6.3-concern"
      status_class: "concern"
      fragment_text: "6.3 review concern: GET /admin/users does not enforce role=admin claim. CWE-285. AC-3 violated."
    - fragment_id: "frag-6.3-blocked"
      status_class: "blocked"
      fragment_text: "Production deployment blocked until role gate + regression test supplied."
    - fragment_id: "frag-6.3-rejected-alternatives"
      status_class: "rejected"
      fragment_text: "Auto-pass rejected. Reject verdict rejected — gap is bounded; remediation feasible."
    - fragment_id: "frag-6.3-knowledge-edge"
      status_class: "knowledge_edge"
      fragment_text: "6.3 has no registered flow — verdict carried as knowledge-edge fragment."
  arbiter_verdicts:
    - request_id: "arb-6.3-compliance"
      verdict:    "concern"
      reason:     "Implementation does not satisfy AC-3."
    - request_id: "arb-6.3-cwe"
      verdict:    "concern"
```

B:
```
HumanResult:
  scenario_id:        "ES-S6"
  overall_verdict:    "CONCERN — AC-3 violated; bounded remediation required"
  body: |
    ## Concern

    GET /admin/users does not enforce the role=admin claim. CWE-285 (Improper
    Authorization) applies — the request is authenticated but unauthorized for
    the resource. NIST SP 800-53 AC-3 requires the system to enforce approved
    authorizations; the current implementation does not.

    ## Blocked

    Production deployment is blocked until both remediation evidence items are
    supplied.

    ## Rejected (alternative verdicts)

    - Auto-pass: rejected — a documented AC-3 violation cannot be silently accepted.
    - Reject: rejected — the gap is bounded and the remediation is feasible
      within the current architecture, so concern is the correct verdict.

    ## Knowledge edges

    - 6.3 Security and Compliance Review has no registered flow — this verdict
      is produced from external NIST + CWE authority and carried as a
      knowledge-edge fragment.

    ## Next action

    Supply both remediation evidence items:
      1. A role gate that returns 403 when role != admin on /admin/* routes.
      2. A regression test that exercises the gate with role != admin and
         asserts 403.
    Re-arbitrate when both are present.
```

Reason:
```
[WRONG: chosen preserves all five status classes — concern, blocked,
rejected alternatives, knowledge edge, next action. The rejected drops
the blocked section ("deployment blocked") and the rejected-alternatives
section. Without the blocked section, the operator may merge to
production. Without the rejected-alternatives section, an auditor cannot
see why concern (not pass, not reject) was chosen. Both omissions are
v8 §Node 09 violations: every constraint remains visible.]
```

Phrasing-B (Type A — drops blocked and rejected-alternatives sections):
```
HumanResult:
  scenario_id:        "ES-S6"
  overall_verdict:    "CONCERN"
  body: |
    ## Concern

    GET /admin/users does not enforce role=admin. Add a role gate.

    ## Knowledge edges

    - 6.3 has no registered flow.

    ## Next action

    Add the role gate.
```

Coverage labels:
```
contract_signature: FormatterUnit
phase: 6
sub_phase: 6.3
node_role: node_09
coverage_status: covered
pair_kind: flow_node
rejected_type: A
```

---

## Pair[ID] ES-2026-05-08-3.3-N09-03 — ES-S8 explain transition human result (Type C target)

A:
```
ReviewedResultSet:
  scenario_id:  "ES-S8"
  fragments:
    - fragment_id: "frag-3.3-explain"
      status_class: "accepted"
      fragment_text: "OpenAPI 3.1 breaking-change taxonomy: removing a required property is breaking; adding a required property is breaking; widening an enum is non-breaking; narrowing an enum is breaking. AIP-180 (CC BY 4.0) defines the same taxonomy."
  arbiter_verdicts:
    - request_id: "arb-3.3-openapi"
      verdict:    "pass"
```

B:
```
HumanResult:
  scenario_id:        "ES-S8"
  overall_verdict:    "EXPLAIN — passed"
  body: |
    ## Explanation

    OpenAPI 3.1 considers a contract change "breaking" when an existing client
    cannot consume the new version without modification.

    Breaking changes:
      - Removing a required property from a request or response schema.
      - Adding a required request property without a default.
      - Narrowing an enum (removing an allowed value).
      - Tightening a string format or maximum length.

    Non-breaking changes:
      - Adding an optional property.
      - Widening an enum (adding an allowed value, with consumer guidance).
      - Adding a new endpoint.

    Authority: OpenAPI 3.1 specification §4 (schema object), Google AIP-180
    "Compatibility" (CC BY 4.0 — © Google LLC).

    ## Next action

    None requested — the prompt asked "How does X work" and the explanation
    is the deliverable.

  attribution: "© Google LLC, CC BY 4.0 (AIP-180)"
```

Reason:
```
[WRONG: ES-S8 is an explain transition (CODE_Q intent). The chosen
formats the explanation, names the authority, and explicitly states no
next action because none was requested. The rejected formats the result
as if it were a feature_addition — listing implementation steps for
adding versioning support. This is the canonical CODE_Q failure mode
documented in the DPO authoring guidance: providing an action response
when an explanation was asked. Token overlap with the chosen would be
moderate (both mention OpenAPI versioning), but the wrong domain
(implementation vs explanation) makes this Type C.]
```

Phrasing-B (Type C — wrong domain, action when explanation asked):
```
HumanResult:
  scenario_id:        "ES-S8"
  overall_verdict:    "PLAN"
  body: |
    ## Implementation steps

    1. Add a version field to the OpenAPI document.
    2. Implement a request router that selects the schema by Accept-Version header.
    3. Document the breaking-change rules in the README.

    ## Next action

    Open a feature ticket and start with step 1.
```

Coverage labels:
```
contract_signature: FormatterUnit
phase: 3
sub_phase: 3.3
node_role: node_09
coverage_status: covered
pair_kind: flow_node
rejected_type: C
```

---

## Node 09 summary

```
Pairs:                  3
Type A rejected:        2 (ES-S5, ES-S6 — drop status sections)
Type C rejected:        1 (ES-S8 — wrong domain)
Status sections preserved across chosen Bs:
  Accepted              3
  Blocked               1 (ES-S6)
  Rejected              2 (ES-S5, ES-S6)
  Knowledge edges       2 (ES-S5, ES-S6)
  Next action           3 (with explicit "none requested" for ES-S8)
  Attribution           1 (ES-S8 — CC BY 4.0)
```

Every chosen B preserves every status class present in the upstream
ReviewedResultSet.
