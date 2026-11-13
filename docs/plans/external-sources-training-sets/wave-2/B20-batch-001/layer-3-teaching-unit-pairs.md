# Layer 3 — Teaching Unit Pairs
# Batch: B20-batch-001
# Scenario: B20 — Unit testing, sub-phase 5.1
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: teaching_unit

---

## Pair B20B1-WDA-teach-001 — "check" does not fire TEST; "verify" and "assert" do

A: "Check that the payment service returns the correct total." Does TEST fire?
B: No. "check" is not in the TEST predicate lexicon. TEST fires on: test, verify,
assert, validate, spec, unit test, integration test, test suite, write tests.
"Check" is informal QA language with no predicate. Result: zero predicates → knowledge_edge.
To fire TEST: use "verify that the payment service returns the correct total" instead.

---

## Pair B20B1-WDA-teach-002 — "add" fires IMPLEMENT even when the object is "tests"

A: "Add unit tests for the cart service." What predicates fire?
B: Both TEST and IMPLEMENT fire. TEST fires on "unit tests". IMPLEMENT fires on "add"
(add is an IMPLEMENT verb: add/extend/append to a codebase). Two predicates →
two routing decisions: TEST → 5.1, IMPLEMENT → 4.1. This is not a contradiction —
"add" describes the construction action (IMPLEMENT) and "unit tests" describes what
is being constructed (TEST). Both fire independently.

---

## Pair B20B1-WDA-teach-003 — "mock" alone is gap; "mock" + "test" → TEST fires

A: "Mock the database for the UserRepository." vs "Write tests that mock the database."
   Which fires TEST?
B: "Mock the database for the UserRepository" → zero predicates → knowledge_edge.
   "Write tests that mock the database" → TEST fires on "tests". "Mock" alone has no predicate.
   The difference is whether "test/tests" appears as the primary action object.
   "Mock" is test infrastructure vocabulary that is gap in isolation.

---

## Pair B20B1-WDA-teach-004 — QA ≠ TEST; they are separate predicates

A: "QA the checkout flow" vs "Test the checkout flow." What fires in each?
B: "QA the checkout flow" → QA predicate fires on "QA". Routes to 7.3 (QA Test).
   "Test the checkout flow" → TEST predicate fires on "test". Routes to 5.1.
   QA and TEST are distinct predicates. "QA" as a verb (or abbreviation) fires the
   QA predicate. "Test" as a verb fires the TEST predicate. They never substitute for each other.

---

## Pair B20B1-WDA-teach-005 — SECURITY routes to 5.2 in testing context

A: "Write security tests for the JWT token validation." What is the routing?
B: TEST fires on "tests" → routes to 5.1. SECURITY fires on "security" + "JWT" + "token"
→ routes to 5.2 (Security and Compliance Testing), not to Phase 1-3. When SECURITY
appears in a testing-context request (alongside TEST), the SECURITY routing resolves
within the testing phase (5.2) rather than design phases (1.x-3.x). Two routing
decisions: TEST → 5.1, SECURITY → 5.2.
