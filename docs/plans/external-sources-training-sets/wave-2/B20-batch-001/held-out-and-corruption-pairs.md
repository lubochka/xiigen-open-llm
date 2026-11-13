# Held-Out and Corruption Pairs
# Batch: B20-batch-001
# Scenario: B20 — Unit testing, sub-phase 5.1
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05

---

## HO-001 — "verify" fires TEST; "verify" alone → 5.1

A: "Verify the refund calculation logic with edge cases for negative amounts."
Expected: TEST fires on "verify". CompiledPredicateIds: ["TEST"]. Routing: 5.1.
"edge cases", "negative amounts" — gap (testing methodology terms with no predicate).
TransitionType: feature_addition.

---

## HO-002 — "write tests" + "document" → TEST + DOCUMENT

A: "Write unit tests for the API and document the test coverage report."
Expected: TEST fires on "unit tests". DOCUMENT fires on "document". IMPLEMENT fires
on "write". CompiledPredicateIds: ["TEST", "DOCUMENT", "IMPLEMENT"]. Three independent
routing decisions: TEST → 5.1, DOCUMENT → 6.2, IMPLEMENT → 4.1.

---

## HO-003 — "assert" fires TEST

A: "Assert that the discount calculation returns zero for customers with no active membership."
Expected: TEST fires on "assert". CompiledPredicateIds: ["TEST"]. Routing: 5.1.
"discount calculation", "no active membership" — domain terms with no predicate.

---

## COR-001 — v6-01: TEST suppressed for "verify the authentication flow works"

CORRUPT: "Verify the authentication flow works." → CompiledPredicateIds: ["SECURITY"].
Why corrupt: "verify" fires TEST, not SECURITY. "authentication" fires SECURITY. Both
predicates fire: ["TEST", "SECURITY"]. The corrupt model suppressed TEST and reported
only SECURITY. Correct: CompiledPredicateIds: ["TEST", "SECURITY"]. Two routing decisions.

---

## COR-002 — v6-01: IMPLEMENT invented for "mock the payment gateway"

CORRUPT: "Mock the payment gateway for unit testing." → CompiledPredicateIds: ["IMPLEMENT"].
Why corrupt: "mock" is gap vocabulary — no predicate. "unit testing" contains "testing"
which fires TEST. The corrupt model invented IMPLEMENT for "mock" (treating mock as
a construction action). Correct: CompiledPredicateIds: ["TEST"] via "unit testing".

---

## COR-003 — v6-03: QA and TEST collapsed to single routing decision

CORRUPT: "QA and test the checkout flow." → single routing decision to 5.1.
Why corrupt: "QA" fires QA predicate → 7.3. "test" fires TEST predicate → 5.1.
Two predicates → two independent routing decisions. Collapsing both to 5.1 is a
v6-03 multi-branch result collapse. Correct: [QA → 7.3, TEST → 5.1] — two decisions.
