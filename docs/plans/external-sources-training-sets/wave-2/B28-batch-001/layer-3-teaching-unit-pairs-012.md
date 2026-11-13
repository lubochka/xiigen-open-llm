# Layer 3 — Teaching Unit Pairs (012)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU012-001 — QA+API_CONTRACT multi-predicate

A: "Validate the API contract changes and get QA sign-off."
B: ["API_CONTRACT", "QA"] — "validate API contract" → API_CONTRACT; "QA sign-off" → QA.
Reason: Two predicates, two flows. API_CONTRACT drives contract validation; QA gates final output.
CompiledPredicateIds: ["API_CONTRACT", "QA"]

---

## TU012-002 — QA+DOCUMENT multi-predicate

A: "Document the release notes and get QA approval."
B: ["DOCUMENT", "QA"] — "document" → DOCUMENT; "QA approval" → QA.
Reason: DOCUMENT fires on documentation verbs. QA fires on "QA approval". TransitionType: documentation (IMPLEMENT absent).
CompiledPredicateIds: ["DOCUMENT", "QA"]

---

## TU012-003 — Full five-predicate with QA

A: "Implement the notification feature, write tests, document the API, validate the contract, and QA sign-off."
B: ["IMPLEMENT", "TEST", "DOCUMENT", "API_CONTRACT", "QA"] — five predicates, five flows, QA last.
Reason: Each clause fires its predicate independently. QA gate executes last and validates the complete work package.
CompiledPredicateIds: ["IMPLEMENT", "TEST", "DOCUMENT", "API_CONTRACT", "QA"]

---

## TU012-004 — QA approval is QA (not AUTHORIZATION)

A: "Get QA approval for the login changes."
B: ["QA"] — "QA approval" → QA predicate. Not AUTHORIZATION.
Reason: "approval" in a QA compound noun fires QA, not AUTHORIZATION. AUTHORIZATION fires on access-control verbs (grant, revoke, authorize, permission).
CompiledPredicateIds: ["QA"]

---

## TU012-005 — Gap: sprint retrospective without QA

A: "Run the sprint retrospective and collect action items."
B: [] — retrospective / sprint ceremony → gap.
Reason: Sprint ceremony vocabulary (retrospective, planning, standup, review meeting) without QA abbreviation → gap.
CompiledPredicateIds: []
