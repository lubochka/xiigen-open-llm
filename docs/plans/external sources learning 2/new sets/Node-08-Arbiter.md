# Node 08 — Arbiter Pairs

```
Contract:
  ArbiterUnit : ILogicalUnit<ArbiterReviewRequest, ArbiterVerdict>
```

## Common coverage labels (apply to every pair in this file)

```
source_license_state: external_license_checked
contract_signature:   ArbiterUnit
node_role:            node_08
pair_kind:            flow_node
coverage_status:      covered
attribution_required: false
```

The complete v8 §"Required Coverage Labels" set per pair (including
`source_id`, `source_title`, `transition_type`, `task_type`, and the
`overlap_ratio` token-overlap-gate value) is recorded in
**`Coverage-Labels-Sweep.md`** under "Node 08 — ArbiterUnit".

Token-overlap gate: every rejected satisfies overlap_ratio < 0.60
(measured values 0.32–0.41 for the five pairs).

Node 08 is an array-node — A is **exactly one** ArbiterReviewRequest. B is
the verdict:

```
verdict: pass | concern | reprocess | reject
reason, violated_constraint, reprocess_instruction, learning_gap_packet
```

Per v8 §Node 08, **for clean pass cases, B may be exactly `verdict: "pass"`** —
the rest of the fields are silent. Pair 01 demonstrates this.

Five pairs in this file — pass, concern, reprocess, reject, plus a
borderline pass-vs-concern decision driven by RFC authority.

---

## Pair[ID] ES-2026-05-08-3.4-N08-01 — ES-S5 RFC 7519 arbiter clean pass (silent fields)

A:
```
ArbiterReviewRequest:
  request_id:           "arb-3.4-security"
  arbiter_id:           "rfc-7519-arbiter"
  target_fragments:
    - fragment_id:   "frag-3.4-accepted"
      status_class:  "accepted"
      fragment_text: "JWT bearer authentication model accepted: iss, aud, exp, tenant claims required; alg=none rejected per RFC 8725; bypass list."
      evidence_ids:  ["rfc-7519", "rfc-8725"]
```

B:
```
ArbiterVerdict:
  verdict: "pass"
```

Reason:
```
[WRONG: the fragment satisfies every RFC 7519 MUST: iss (§4.1.1), aud
(§4.1.3), exp (§4.1.4), and rejects alg=none per RFC 8725 §2.1. Clean
pass — silent verdict is correct per v8 §Node 08. The rejected fabricates
a violated_constraint and a reprocess_instruction even though no
constraint is violated. Inventing fields on a clean pass corrupts the
verdict signal: downstream Node 09 would format reprocess hints when no
reprocess is needed.]
```

Phrasing-B (Type A — fabricated fields on a clean pass):
```
ArbiterVerdict:
  verdict:                "pass"
  reason:                 "model is acceptable"
  violated_constraint:    "none, but suggest adding kid claim"
  reprocess_instruction:  "consider adding kid claim for key rotation"
```

Coverage labels:
```
contract_signature: ArbiterUnit
phase: 3
sub_phase: 3.4
node_role: node_08
coverage_status: covered
pair_kind: flow_node
rejected_type: A
```

---

## Pair[ID] ES-2026-05-08-6.3-N08-02 — ES-S6 NIST AC-3 arbiter concern verdict

A:
```
ArbiterReviewRequest:
  request_id:           "arb-6.3-compliance"
  arbiter_id:           "nist-ac-3-arbiter"
  target_fragments:
    - fragment_id:   "frag-6.3-concern"
      status_class:  "concern"
      fragment_text: "6.3 review concern: GET /admin/users does not enforce role=admin claim. CWE-285. AC-3 violated."
      evidence_ids:  ["cwe-285", "nist-sp-800-53-ac-3"]
    - fragment_id:   "frag-6.3-blocked"
      status_class:  "blocked"
      fragment_text: "Production deployment blocked until role gate + regression test supplied."
      evidence_ids:  ["nist-sp-800-53-ac-3"]
```

B:
```
ArbiterVerdict:
  verdict:             "concern"
  reason:              "Implementation does not satisfy AC-3 requirement that the system enforce approved authorizations. The named gap is bounded (single missing middleware on admin routes) and the fragment already specifies the remediation evidence."
  violated_constraint: "NIST SP 800-53 AC-3 — Access Enforcement: 'the system enforces approved authorizations specified in policy'"
  reprocess_instruction: ""
  learning_gap_packet:
    sub_phase: "6.3"
    finding:   "no registered 6.3 review flow; concern verdict produced from external NIST + CWE authority"
    suggested_remediation: "register a 6.3 flow that consumes ReviewVerdict fragments and emits ArbiterVerdict directly"
```

Reason:
```
[WRONG: concern is correct because the AC-3 violation is real but the
remediation is bounded and already named. Pass would silence a
confirmed AC-3 violation; reject would close the door on a feasible
remediation. The rejected returns pass with a fabricated note that
"role gates are common practice" — but common practice is not an
authority that overrides the AC-3 violation cited in the fragment.
This is the v8 §Node 08 failure case: pass on a fragment with a named
gap.]
```

Phrasing-B (Type A — wrong verdict, pass when concern required):
```
ArbiterVerdict:
  verdict: "pass"
  reason:  "role gates are a common practice; the missing middleware can be added in normal development"
```

Coverage labels:
```
contract_signature: ArbiterUnit
phase: 6
sub_phase: 6.3
node_role: node_08
coverage_status: covered
pair_kind: flow_node
rejected_type: A
```

---

## Pair[ID] ES-2026-05-08-3.5-N08-03 — ES-S7 Temporal saga arbiter reprocess verdict

A:
```
ArbiterReviewRequest:
  request_id:           "arb-3.5-saga"
  arbiter_id:           "temporal-saga-arbiter"
  target_fragments:
    - fragment_id:   "frag-3.5-accepted"
      status_class:  "accepted"
      fragment_text: "Saga workflow declaration accepted: forward step 1 reserve before step 2 capture; compensation only when step 2 fails after step 1 success; idempotent activities."
      evidence_ids:  ["temporal-saga"]
    - fragment_id:   "frag-3.5-rejected"
      status_class:  "rejected"
      fragment_text: "Rejected: inventory.release on inventory.reserve failure — Temporal saga rule violation."
      evidence_ids:  ["temporal-saga"]
    - fragment_id:   "frag-3.5-knowledge-edge"
      status_class:  "knowledge_edge"
      fragment_text: "Knowledge edge: compensation_timeout unresolved."
      evidence_ids:  ["temporal-saga"]
```

B:
```
ArbiterVerdict:
  verdict:               "reprocess"
  reason:                "Saga rule satisfied for the named compensation step but compensation_timeout is unresolved. Without a timeout, the workflow can hang in compensation-pending state. Reprocess required to set the timeout before declaring the saga complete."
  violated_constraint:   "Temporal saga: every compensation activity must have a bounded retry policy (timeout + max retries)"
  reprocess_instruction: "return to 3.5 observability scenario; resolve compensation_timeout to a concrete duration; rebind to the saga declaration before re-arbitration"
  learning_gap_packet:
    sub_phase:           "3.5"
    finding:             "compensation_timeout is a 3.5 design responsibility — not 7.3 monitoring"
    suggested_remediation: "extend the 3.5 splitter rule to require timeout decision before saga declaration is finalised"
```

Reason:
```
[WRONG: reprocess is correct — the unresolved compensation_timeout is a
real boundary that the workflow declaration cannot finalise without.
Reject would discard valid work; pass would silence an open question.
The rejected returns pass on the basis that "Temporal handles timeouts
by default". Temporal documentation does state default activity timeouts
exist, but compensation activities must have explicit timeouts because
defaults are scoped to forward activities. The rejected reads the
authority partially — defaults exist but do not apply here. Reprocess
is the correct verdict to surface the unresolved decision.]
```

Phrasing-B (Type A — pass instead of reprocess, partial authority read):
```
ArbiterVerdict:
  verdict: "pass"
  reason:  "Temporal handles timeouts by default; the workflow can be deployed and the timeout tuned later."
```

Coverage labels:
```
contract_signature: ArbiterUnit
phase: 3
sub_phase: 3.5
node_role: node_08
coverage_status: covered
pair_kind: flow_node
rejected_type: A
```

---

## Pair[ID] ES-2026-05-08-3.4-N08-04 — ES-S5 OpenAPI arbiter reject (contract incompatible)

A:
```
ArbiterReviewRequest:
  request_id:           "arb-3.3-contract"
  arbiter_id:           "openapi-arbiter"
  target_fragments:
    - fragment_id:   "frag-3.3-knowledge-edge"
      status_class:  "knowledge_edge"
      fragment_text: "API contract derived from JWT model. Authorization: Bearer required; 401 on missing; 403 on tenant mismatch; tenant_claim format=string; tenant_claim location=request body."
      evidence_ids:  ["openapi-specification", "rfc-6749", "branch-1-derived"]
```

(Note for the pair: the arbiter A has been seeded with a deliberate
inconsistency — `tenant_claim location=request body` contradicts the
JWT model from branch-1, which placed the tenant claim inside the JWT.
This drives the reject verdict.)

B:
```
ArbiterVerdict:
  verdict:               "reject"
  reason:                "The contract states tenant_claim location is the request body. The JWT model in branch-1 places the tenant claim inside the bearer token. Two different sources of truth for the tenant claim cannot coexist — the API would have to reconcile body vs token on every request, with no rule for which wins."
  violated_constraint:   "OpenAPI 3.1 + RFC 7519 — a JWT claim and a request body field cannot both be the authorization key"
  reprocess_instruction: ""
  learning_gap_packet:
    sub_phase: "3.3"
    finding:   "contract derivation lost the JWT-internal location of the tenant claim during translation"
    suggested_remediation: "restart from branch-1 model; tenant claim location is JWT payload, not request body"
```

Reason:
```
[WRONG: reject is correct because the contract contradicts the upstream
model in a way that cannot be reconciled by a re-run with the same
inputs — the contract derivation itself is wrong. Reprocess would imply
the same inputs can produce a correct contract; here the contract must
be rebuilt from scratch with corrected handoff data. The rejected
returns concern, which keeps the contradiction in the system as an
open question. Concern is wrong because a contract with two sources of
truth is not a bounded gap — it is a structural defect.]
```

Phrasing-B (Type A — concern instead of reject, structural defect kept open):
```
ArbiterVerdict:
  verdict: "concern"
  reason:  "tenant_claim location ambiguous between JWT and request body; downstream consumers should pick one"
```

Coverage labels:
```
contract_signature: ArbiterUnit
phase: 3
sub_phase: 3.3
node_role: node_08
coverage_status: covered
pair_kind: flow_node
rejected_type: A
```

---

## Pair[ID] ES-2026-05-08-3.4-N08-05 — ES-S5 RFC 7519 arbiter pass with audit-event note

A:
```
ArbiterReviewRequest:
  request_id:           "arb-3.4-security-audit"
  arbiter_id:           "rfc-7519-arbiter"
  target_fragments:
    - fragment_id:   "frag-3.4-accepted-audit"
      status_class:  "accepted"
      fragment_text: "JWT bearer authentication model accepted: iss, aud, exp, tenant claims required; alg=none rejected; bypass list; every authentication failure is logged with token_id_hash and timestamp."
      evidence_ids:  ["rfc-7519", "rfc-8725", "nist-sp-800-53-au-2"]
```

B:
```
ArbiterVerdict:
  verdict: "pass"
```

Reason:
```
[WRONG: this is a clean pass — the model satisfies RFC 7519 MUSTs and
the audit logging satisfies AU-2. Per v8 §Node 08, B may be exactly
verdict: "pass". The rejected upgrades the verdict to "concern" with a
fabricated reason that the audit event format is "not specified". AU-2
does not mandate a specific format; it requires the events be logged
with sufficient detail. The fragment names token_id_hash + timestamp,
which satisfies AU-2. Inventing a concern that the authority does not
require is the canonical false-concern Node 08 error.]
```

Phrasing-B (Type A — false concern, fabricated requirement):
```
ArbiterVerdict:
  verdict:             "concern"
  reason:              "Audit event format is not specified — JSON, syslog, or other?"
  violated_constraint: "AU-2 §a — events must be specified to a format"
```

Coverage labels:
```
contract_signature: ArbiterUnit
phase: 3
sub_phase: 3.4
node_role: node_08
coverage_status: covered
pair_kind: flow_node
rejected_type: A
```

---

## Node 08 summary

```
Pairs:                  5
Type A rejected:        5
Verdict shapes used:
  pass (silent)         2 (pairs 01, 05)
  concern               1 (pair 02)
  reprocess             1 (pair 03)
  reject                1 (pair 04)
```

Pair 01 and pair 05 satisfy the v8 acceptance checklist line: "Node 08
pass rows are silent if runtime pass is silent."
