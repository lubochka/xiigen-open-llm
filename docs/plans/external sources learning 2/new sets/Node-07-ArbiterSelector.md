# Node 07 — Arbiter Selector Pairs

```
Contract:
  ArbiterSelectorUnit
      : ILogicalUnit<TranslatedResultFragment[], ArbiterReviewRequest[]>
```

## Common coverage labels (apply to every pair in this file)

```
source_license_state: external_license_checked
contract_signature:   ArbiterSelectorUnit
node_role:            node_07
pair_kind:            flow_node
coverage_status:      covered
attribution_required: false
```

The complete v8 §"Required Coverage Labels" set per pair (including
`source_id`, `source_title`, `transition_type`, `task_type`, and the
`overlap_ratio` token-overlap-gate value) is recorded in
**`Coverage-Labels-Sweep.md`** under "Node 07 — ArbiterSelectorUnit".

Token-overlap gate: every rejected satisfies overlap_ratio < 0.60
(measured values 0.40–0.43 for the three pairs).

B names each selected arbiter request **and** each considered-but-not-selected
arbiter with the reason — per v8 §Node 07. The full set is required so the
operator can see why a given arbiter did or did not fire.

Three pairs in this file — one per scenario where the arbiter selection
decision is non-trivial.

---

## Pair[ID] ES-2026-05-08-3.4-N07-01 — ES-S5 selects security + contract arbiters

A:
```
TranslatedResultFragment[]:
  - fragment_id: "frag-3.4-accepted"
    status_class: "accepted"
    fragment_text: "JWT bearer authentication model accepted: iss, aud, exp, tenant claims required; alg=none rejected per RFC 8725; bypass list."
    evidence_ids: ["rfc-7519", "rfc-8725"]
  - fragment_id: "frag-3.3-knowledge-edge"
    status_class: "knowledge_edge"
    fragment_text: "API contract derived from JWT model. 3.3 unregistered."
    evidence_ids: ["openapi-specification", "rfc-6749", "branch-1-derived"]
  - fragment_id: "frag-5.2-knowledge-edge"
    status_class: "knowledge_edge"
    fragment_text: "Integration test plan accepted; mocked fixtures rejected. 5.2 unregistered."
    evidence_ids: ["testcontainers-dotnet", "branch-1-derived", "branch-2-derived"]
```

B:
```
ArbiterReviewRequest[]:
  - request_id:        "arb-3.4-security"
    arbiter_id:        "rfc-7519-arbiter"
    target_fragments:  ["frag-3.4-accepted"]
    selected:          true
    reason_for_selection: "fragment is the registered 3.4 PASS branch; RFC 7519 arbiter is the authority for required claim set"
  - request_id:        "arb-3.3-contract"
    arbiter_id:        "openapi-arbiter"
    target_fragments:  ["frag-3.3-knowledge-edge"]
    selected:          true
    reason_for_selection: "fragment is a knowledge-edge contract; OpenAPI arbiter validates header parameter shape and tenant_claim format consistency with branch-1"
  - request_id:        "arb-5.2-coverage"
    arbiter_id:        "testcontainers-arbiter"
    target_fragments:  ["frag-5.2-knowledge-edge"]
    selected:          false
    reason_for_rejection: "5.2 is a knowledge-edge fragment with no registered flow; the testcontainers arbiter has no flow output to score against. Selecting it would fabricate an arbiter result."
  - request_id:        "arb-3.4-performance"
    arbiter_id:        "performance-arbiter"
    target_fragments:  ["frag-3.4-accepted"]
    selected:          false
    reason_for_rejection: "3.4 fragment is a model design, not a runtime artifact. No performance signal exists to score."
```

Reason:
```
[WRONG: chosen lists every considered arbiter with its selection or
rejection reason — including the testcontainers arbiter for 5.2 (rejected
because the fragment is knowledge-edge with no flow output) and the
performance arbiter for 3.4 (rejected because design has no performance
signal). The rejected lists only the two selected arbiters and silently
omits the rejected arbiters. Without their visibility, an auditor cannot
confirm the testcontainers arbiter was considered at all — the operator
cannot tell whether it was rejected for a good reason or skipped by
mistake.]
```

Phrasing-B (Type A — drops considered-but-not-selected arbiters):
```
ArbiterReviewRequest[]:
  - request_id:        "arb-3.4-security"
    arbiter_id:        "rfc-7519-arbiter"
    target_fragments:  ["frag-3.4-accepted"]
    selected:          true
    reason_for_selection: "RFC 7519 authority"
  - request_id:        "arb-3.3-contract"
    arbiter_id:        "openapi-arbiter"
    target_fragments:  ["frag-3.3-knowledge-edge"]
    selected:          true
    reason_for_selection: "OpenAPI authority"
```

Coverage labels:
```
contract_signature: ArbiterSelectorUnit
phase: 3
sub_phase: 3.4+3.3+5.2
node_role: node_07
coverage_status: covered
pair_kind: flow_node
rejected_type: A
```

---

## Pair[ID] ES-2026-05-08-6.3-N07-02 — ES-S6 selects compliance arbiter, rejects design arbiter

A:
```
TranslatedResultFragment[]:
  - fragment_id: "frag-6.3-concern"
    status_class: "concern"
    fragment_text: "6.3 review concern: GET /admin/users does not enforce role=admin claim. CWE-285. AC-3 violated."
    evidence_ids: ["cwe-285", "nist-sp-800-53-ac-3"]
  - fragment_id: "frag-6.3-blocked"
    status_class: "blocked"
    fragment_text: "Production deployment blocked until role gate + regression test supplied."
    evidence_ids: ["nist-sp-800-53-ac-3"]
  - fragment_id: "frag-6.3-rejected-alternatives"
    status_class: "rejected"
    fragment_text: "Auto-pass rejected. Reject verdict rejected — gap is bounded; remediation is feasible."
    evidence_ids: ["nist-sp-800-53-ac-3"]
  - fragment_id: "frag-6.3-knowledge-edge"
    status_class: "knowledge_edge"
    fragment_text: "6.3 has no registered flow — verdict carried as knowledge-edge fragment."
    evidence_ids: []
```

B:
```
ArbiterReviewRequest[]:
  - request_id:        "arb-6.3-compliance"
    arbiter_id:        "nist-ac-3-arbiter"
    target_fragments:  ["frag-6.3-concern", "frag-6.3-blocked"]
    selected:          true
    reason_for_selection: "fragments cite AC-3 directly; the NIST AC-3 arbiter is the authority for 'system enforces approved authorizations'"
  - request_id:        "arb-6.3-cwe"
    arbiter_id:        "cwe-285-arbiter"
    target_fragments:  ["frag-6.3-concern"]
    selected:          true
    reason_for_selection: "fragment maps the finding to CWE-285; the CWE-285 arbiter validates that authentication-vs-authorization distinction is correctly applied"
  - request_id:        "arb-3.4-design"
    arbiter_id:        "rfc-7519-arbiter"
    target_fragments:  ["frag-6.3-concern"]
    selected:          false
    reason_for_rejection: "the fragment is a 6.3 review verdict, not a 3.4 design output. RFC 7519 arbiter has no design artifact to score and would fabricate a redesign."
  - request_id:        "arb-6.3-performance"
    arbiter_id:        "performance-arbiter"
    target_fragments:  ["frag-6.3-concern"]
    selected:          false
    reason_for_rejection: "review verdict is not a runtime artifact; performance signal is absent."
```

Reason:
```
[WRONG: chosen explicitly considers and rejects the RFC 7519 design
arbiter — that rejection is load-bearing because it documents that the
selector did not slip from review (6.3) to design (3.4). The rejected
includes the RFC 7519 arbiter as selected, which would route the review
verdict to a design arbiter and produce a redesign verdict that the
operator did not request. This is the 6.3 vs 3.4 selection error —
review fragments must not be sent to design arbiters.]
```

Phrasing-B (Type A — selects 3.4 design arbiter for 6.3 fragment):
```
ArbiterReviewRequest[]:
  - request_id:        "arb-6.3-compliance"
    arbiter_id:        "nist-ac-3-arbiter"
    target_fragments:  ["frag-6.3-concern", "frag-6.3-blocked"]
    selected:          true
    reason_for_selection: "NIST AC-3 authority"
  - request_id:        "arb-3.4-design"
    arbiter_id:        "rfc-7519-arbiter"
    target_fragments:  ["frag-6.3-concern"]
    selected:          true
    reason_for_selection: "the finding mentions JWT roles; RFC 7519 arbiter is the JWT authority"
```

Coverage labels:
```
contract_signature: ArbiterSelectorUnit
phase: 6
sub_phase: 6.3
node_role: node_07
coverage_status: covered
pair_kind: flow_node
rejected_type: A
```

---

## Pair[ID] ES-2026-05-08-3.5-N07-03 — ES-S7 selects saga + idempotency arbiters

A:
```
TranslatedResultFragment[]:
  - fragment_id: "frag-3.5-accepted"
    status_class: "accepted"
    fragment_text: "Saga workflow declaration accepted: forward step 1 reserve before step 2 capture; compensation only when step 2 fails after step 1 success; idempotent activities."
    evidence_ids: ["temporal-saga"]
  - fragment_id: "frag-3.5-rejected"
    status_class: "rejected"
    fragment_text: "Rejected: inventory.release on inventory.reserve failure — Temporal saga rule violation."
    evidence_ids: ["temporal-saga"]
  - fragment_id: "frag-3.5-knowledge-edge"
    status_class: "knowledge_edge"
    fragment_text: "Knowledge edge: compensation_timeout unresolved."
    evidence_ids: ["temporal-saga"]
  - fragment_id: "frag-4.1-knowledge-edge"
    status_class: "knowledge_edge"
    fragment_text: "Activity implementations defined per branch-1. 4.1 unregistered."
    evidence_ids: ["temporal-saga"]
```

B:
```
ArbiterReviewRequest[]:
  - request_id:        "arb-3.5-saga"
    arbiter_id:        "temporal-saga-arbiter"
    target_fragments:  ["frag-3.5-accepted", "frag-3.5-rejected"]
    selected:          true
    reason_for_selection: "Temporal saga arbiter validates compensation_only_when_forward_took_effect rule against both accepted and rejected fragments"
  - request_id:        "arb-3.5-idempotency"
    arbiter_id:        "idempotency-arbiter"
    target_fragments:  ["frag-3.5-accepted", "frag-4.1-knowledge-edge"]
    selected:          true
    reason_for_selection: "idempotency claim ('all activities idempotent') must be validated; arbiter checks that each named activity has an idempotency key strategy"
  - request_id:        "arb-3.5-observability"
    arbiter_id:        "audit-au-2-arbiter"
    target_fragments:  ["frag-3.5-knowledge-edge"]
    selected:          false
    reason_for_rejection: "compensation_timeout is unresolved and handed off; arbitrating the missing timeout would require fabricating the value the system has not decided yet"
```

Reason:
```
[WRONG: chosen selects exactly the arbiters that have authority over the
fragments — saga rule, idempotency — and rejects the AU-2 audit arbiter
because it has no resolved value to score. The rejected adds an extra
"reject-the-anti-pattern" arbiter that is not in the registry; arbiters
must come from a fixed registry, and inventing one to "double-check the
saga rejection" duplicates work the saga arbiter already does. Inventing
arbiter ids is the canonical Node 07 selection error.]
```

Phrasing-B (Type A — invents an arbiter id not in the registry):
```
ArbiterReviewRequest[]:
  - request_id:        "arb-3.5-saga"
    arbiter_id:        "temporal-saga-arbiter"
    target_fragments:  ["frag-3.5-accepted"]
    selected:          true
    reason_for_selection: "saga authority"
  - request_id:        "arb-3.5-anti-pattern-double-check"
    arbiter_id:        "anti-pattern-detector"
    target_fragments:  ["frag-3.5-rejected"]
    selected:          true
    reason_for_selection: "double-check the rejected anti-pattern"
```

Coverage labels:
```
contract_signature: ArbiterSelectorUnit
phase: 3
sub_phase: 3.5
node_role: node_07
coverage_status: covered
pair_kind: flow_node
rejected_type: A
```

---

## Node 07 summary

```
Pairs:                  3
Type A rejected:        3
Considered-but-not-selected arbiters preserved: 5 across the three chosen Bs
Errors flagged:         dropping rejected arbiters (pair 01),
                        wrong-arbiter selection (pair 02),
                        invented arbiter id (pair 03)
```
