# Supplement — Domain Type C (Wrong Domain for Intent Verb)

Six additional Type C pairs across the four domains (CODE_Q, BUG, PLAN,
ARCH) to lift the Type C fraction toward the v7 §"Batch composition
minimums" target of 30 %. Each pair tests an intent-verb→domain
mis-mapping per `DPO-PAIR-AUTHORING-GUIDANCE-v2.md` §"Type C — Wrong
domain for the prompt's intent verb" (line 230).

Intent-verb→domain mapping (from DPO authoring guidance):

```
"explain / how does X work / what is"   → CODE_Q
"X throws / X fails / error in / bug"   → BUG
"implement / add / build / create"      → PLAN or IMPLEMENT
"design / define boundaries / architect" → ARCH
"review / audit / verify compliance"    → QA or ARCH review
```

All scenarios are grounded in external sources only.

---

## Common coverage labels

```
source_license_state: external_license_checked
contract_signature:   AdaptiveLogicalOrchestrator
node_role:            type_c_supplement
pair_kind:            transition_orchestrator
coverage_status:      covered
rejected_type:        C
```

Per-pair labels carry `source_id`, `source_title`, `transition_type`,
`phase`, `sub_phase`, `task_type`, `overlap_ratio`.

---

## Pair[ID] ES-2026-05-08-CODEQ-TC-01 — "How does" routed as CODE_Q (RFC 7519)

A:
```
user_request: "How does the JWT iss claim get validated against the OpenID Connect issuer URL?"
```

B (chosen — explain transition, CODE_Q domain):
```
expected_transition_type: "explain"
expected_domain:          "CODE_Q"
expected_routing:
  - PredictedSubPhaseId: "3.4"
    EvidencePredicates:  ["SECURITY"]
    KnowledgeEdge:       false
expected_response_class: "explanation grounded in RFC 7519 §4.1.1 and OpenID Connect Discovery 1.0"
```

Reason:
```
[WRONG: "How does X work" is the canonical CODE_Q intent verb. The
chosen produces an explanation. The rejected interprets the prompt as
a BUG report ("the validation is broken, fix it") and routes to repair.
This is the most common CODE_Q→BUG misread documented in DPO authoring
guidance §"BUG domain — most common confusion": prompts containing
security tokens get pushed into BUG when they are actually explanatory.]
```

Phrasing-B (Type C — CODE_Q misread as BUG):
```
expected_transition_type: "repair"
expected_domain:          "BUG"
expected_routing:
  - PredictedSubPhaseId: "4.1"
    EvidencePredicates:  ["SECURITY", "IMPLEMENT"]
    KnowledgeEdge:       true
expected_response_class: "fix plan for broken iss validation"
```

Coverage labels:
```
source_id:       rfc-7519, openid-connect-discovery
source_title:    RFC 7519 §4.1.1 + OpenID Connect Discovery 1.0
transition_type: explain
phase:           3
sub_phase:       3.4
task_type:       jwt-authentication-explanation
overlap_ratio:   0.22
```

---

## Pair[ID] ES-2026-05-08-BUG-TC-02 — "throws" routed as BUG (CWE-287 grounding)

A:
```
user_request: "Authentication throws InvalidTokenException intermittently when the request includes the new x-correlation-id header — calls without that header succeed."
```

B (chosen — repair transition, BUG domain):
```
expected_transition_type: "repair"
expected_domain:          "BUG"
expected_routing:
  - PredictedSubPhaseId: "4.1"
    EvidencePredicates:  ["IMPLEMENT", "SECURITY"]
    KnowledgeEdge:       true
expected_response_class: "diagnose and fix the JWT validator's interaction with the new header"
expected_authority:      "CWE-287 — Improper Authentication; defect localised to validator code path triggered by header presence"
```

Reason:
```
[WRONG: "throws / intermittently / when X" is the canonical BUG intent
pattern — there is observable broken behaviour. The chosen routes to
repair / 4.1 backend services. The rejected interprets the prompt as a
PLAN to redesign the auth model and routes to feature_addition / 3.4.
But the model exists; the implementation fails on a specific input
shape. PLAN/3.4 is the wrong domain — fixing a code path is BUG/4.1
work.]
```

Phrasing-B (Type C — BUG misread as PLAN):
```
expected_transition_type: "feature_addition"
expected_domain:          "PLAN"
expected_routing:
  - PredictedSubPhaseId: "3.4"
    EvidencePredicates:  ["SECURITY"]
    KnowledgeEdge:       false
expected_response_class: "redesign auth model to handle correlation header"
```

Coverage labels:
```
source_id:       cwe-287
source_title:    MITRE CWE-287 — Improper Authentication
transition_type: repair
phase:           4
sub_phase:       4.1
task_type:       backend-services-defect
overlap_ratio:   0.25
```

---

## Pair[ID] ES-2026-05-08-PLAN-TC-03 — "implement and then test" routed as PLAN

A:
```
user_request: "Implement the rate-limit middleware against the published 429 response shape per RFC 6585, and after the middleware is in place write the integration tests for the limit-exceeded path."
```

B (chosen — feature_addition, PLAN domain):
```
expected_transition_type: "feature_addition"
expected_domain:          "PLAN"
expected_routing:
  - PredictedSubPhaseId: "4.1"  EvidencePredicates: ["IMPLEMENT"]  KnowledgeEdge: true
  - PredictedSubPhaseId: "5.2"  EvidencePredicates: ["TEST"]       KnowledgeEdge: true
expected_node_03_dependency: "branch-1 (4.1) → branch-2 (5.2)"
expected_authority:          "RFC 6585 §4 (429 Too Many Requests)"
```

Reason:
```
[WRONG: "Implement ... and then ... write the integration tests" is
the canonical multi-task PLAN intent. The chosen routes to PLAN with
two branches and an explicit dependency. The rejected interprets the
prompt as a CODE_Q "how to" question and produces an explanation
without routing the implementation or test branches. Producing an
explanation when the user asked to build is the most common PLAN→CODE_Q
misread.]
```

Phrasing-B (Type C — PLAN misread as CODE_Q):
```
expected_transition_type: "explain"
expected_domain:          "CODE_Q"
expected_routing:
  - PredictedSubPhaseId: "4.1"  EvidencePredicates: ["IMPLEMENT"]  KnowledgeEdge: true
expected_response_class: "explanation of how rate-limit middleware works in general"
```

Coverage labels:
```
source_id:       rfc-6585
source_title:    IETF RFC 6585 — Additional HTTP Status Codes
transition_type: feature_addition
phase:           4
sub_phase:       4.1+5.2
task_type:       multi-task-implement-then-test
overlap_ratio:   0.30
```

---

## Pair[ID] ES-2026-05-08-ARCH-TC-04 — "design boundaries" routed as ARCH

A:
```
user_request: "Design the boundary between the billing service and the new tax-calculation service. Tax service must own jurisdiction rules and tax-rate snapshots; billing must not call tax storage directly."
```

B (chosen — feature_addition, ARCH domain):
```
expected_transition_type: "feature_addition"
expected_domain:          "ARCH"
expected_routing:
  - PredictedSubPhaseId: "3.1"
    EvidencePredicates:  ["ARCHITECTURE"]
    KnowledgeEdge:       true
expected_response_class: "ADR documenting the boundary, ownership, and forbidden cross-storage rule"
expected_authority:      "arc42 §5 Building Block View"
```

Reason:
```
[WRONG: "Design the boundary" + "must own" + "must not call ... directly"
is the canonical ARCH intent pattern. The chosen routes to ARCH/3.1
with an ADR-shaped response. The rejected interprets the prompt as
IMPLEMENT and routes to 4.1 — producing class definitions for
TaxService and BillingService. Class definitions are implementation
work that follows the boundary decision; producing them now skips the
boundary and writes the rule into code without ownership documentation.]
```

Phrasing-B (Type C — ARCH misread as IMPLEMENT):
```
expected_transition_type: "feature_addition"
expected_domain:          "PLAN"
expected_routing:
  - PredictedSubPhaseId: "4.1"
    EvidencePredicates:  ["IMPLEMENT"]
    KnowledgeEdge:       true
expected_response_class: "TaxService class with TaxRateRepository methods"
```

Coverage labels:
```
source_id:       arc42-template, ADR-template
source_title:    arc42 + ADR template (boundary decision)
transition_type: feature_addition
phase:           3
sub_phase:       3.1
task_type:       architecture-topology
overlap_ratio:   0.24
```

---

## Pair[ID] ES-2026-05-08-QA-TC-05 — "audit / verify compliance" routed as QA review

A:
```
user_request: "Audit whether our session token rotation meets the NIST SP 800-63B §4.1.5 requirement for reauthentication interval — verify the configured timeout value matches the requirement."
```

B (chosen — repair, QA review domain):
```
expected_transition_type: "repair"
expected_domain:          "QA"
expected_routing:
  - PredictedSubPhaseId: "6.3"
    EvidencePredicates:  ["QA", "SECURITY"]
    KnowledgeEdge:       true
expected_response_class: "review verdict: pass / concern / reject with citation to NIST SP 800-63B §4.1.5"
expected_authority:      "NIST SP 800-63B §4.1.5 — Reauthentication Requirements (US gov work, public domain)"
```

Reason:
```
[WRONG: "Audit whether X meets Y" is the canonical QA review intent.
The chosen routes to 6.3. The rejected interprets the prompt as a
design request and routes to ARCH/3.4 to redesign session token
rotation. But the requirement and the implementation both already
exist — the prompt asks whether the implementation satisfies the
requirement. Redesigning is design scope; auditing is review scope.]
```

Phrasing-B (Type C — QA review misread as ARCH design):
```
expected_transition_type: "feature_addition"
expected_domain:          "ARCH"
expected_routing:
  - PredictedSubPhaseId: "3.4"
    EvidencePredicates:  ["SECURITY"]
    KnowledgeEdge:       false
expected_response_class: "redesign session rotation policy"
```

Coverage labels:
```
source_id:       nist-sp-800-63b
source_title:    NIST SP 800-63B Digital Identity Guidelines — Authentication and Lifecycle Management (US gov work, public domain)
attribution_required: false
transition_type: repair
phase:           6
sub_phase:       6.3
task_type:       security-compliance-review
overlap_ratio:   0.26
```

---

## Pair[ID] ES-2026-05-08-CODEQ-TC-06 — "what is" routed as CODE_Q (OpenAPI)

A:
```
user_request: "What is the difference between an OpenAPI request body schema and a parameter with location=body?"
```

B (chosen — explain, CODE_Q):
```
expected_transition_type: "explain"
expected_domain:          "CODE_Q"
expected_routing:
  - PredictedSubPhaseId: "3.3"
    EvidencePredicates:  ["API_CONTRACT"]
    KnowledgeEdge:       true
expected_response_class: "explanation citing OpenAPI 2.0 (parameter in=body) vs OpenAPI 3.x (requestBody object)"
expected_authority:      "OpenAPI 3.1 §4.7 + OpenAPI 2.0 §4.7.4"
```

Reason:
```
[WRONG: "What is the difference between" is the canonical CODE_Q
intent. The chosen produces an explanation. The rejected interprets
the prompt as a PLAN to migrate from OpenAPI 2.0 to 3.x and routes to
4.5 / connector implementation — producing a migration plan when an
explanation was asked. This is the same CODE_Q→PLAN failure mode
from CODEQ-TC-03 inverted.]
```

Phrasing-B (Type C — CODE_Q misread as PLAN):
```
expected_transition_type: "feature_addition"
expected_domain:          "PLAN"
expected_routing:
  - PredictedSubPhaseId: "4.5"
    EvidencePredicates:  ["IMPLEMENT", "API_CONTRACT"]
    KnowledgeEdge:       true
expected_response_class: "migration plan from OpenAPI 2.0 to 3.x"
```

Coverage labels:
```
source_id:       openapi-specification
source_title:    OpenAPI 3.1 §4.7 + OpenAPI 2.0 §4.7.4
transition_type: explain
phase:           3
sub_phase:       3.3
task_type:       api-contract-explanation
overlap_ratio:   0.21
```

---

## Supplement summary

```
Pairs:                  6   (all Type C)
Domain coverage:
  CODE_Q                3   (CODEQ-TC-01, PLAN-TC-03, CODEQ-TC-06 — testing CODE_Q←→{BUG, PLAN})
  BUG                   1   (BUG-TC-02 — testing BUG←→PLAN)
  PLAN                  1   (PLAN-TC-03 — testing PLAN←→CODE_Q)
  ARCH                  1   (ARCH-TC-04 — testing ARCH←→IMPLEMENT)
  QA review             1   (QA-TC-05 — testing QA←→ARCH)
```

Lifts the global Type C count from 6 to 12 across the batch. Type C
remains below the 30 % global target (12 / 96 = 12.5 %); see README
§"Honest disclosure" for the recommended follow-up of 12 additional
Type C pairs at Layer 1 / Layer 0-B before training.

---

## Last Updated

2026-05-08.
