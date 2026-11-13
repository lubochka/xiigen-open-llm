# Layer 3 — Teaching Unit Pairs (008)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU008-001 — Teach: "integrate" (verb) → API_CONTRACT; "integration" (noun) → gap

Misconception: "write integration tests" fires API_CONTRACT because integration is an API concern.
Correction:
  "integrate" as a verb → API_CONTRACT (e.g., "integrate the PaymentGateway")
  "integration tests" as a noun phrase → TEST via "tests" (sub-phase 5.1 for service-level; 5.3 for E2E)
  "integration" as a standalone noun (relationship concept) → gap
  "integration tests" does NOT fire API_CONTRACT.
Rule: Verb "integrate" → API_CONTRACT. Noun compound "integration tests" → TEST only.

---

## TU008-002 — Teach: "apply" fires IMPLEMENT; context doesn't change it

Misconception: "apply a patch" might be maintenance and not IMPLEMENT; "apply mutation testing" might be TEST only.
Correction: "apply" fires IMPLEMENT in all contexts where it appears.
  "Apply a patch." → ["IMPLEMENT"]
  "Apply mutation testing." → ["IMPLEMENT", "TEST"] (IMPLEMENT from "apply"; TEST from "mutation testing")
  "Apply the discount rule." → ["IMPLEMENT"]
Rule: "apply" is an IMPLEMENT trigger regardless of what follows it.

---

## TU008-003 — Teach: "ensure" is gap; TEST fires on what follows if a TEST noun is present

Misconception: "ensure the tests are passing" fires both TEST and a confirmation predicate.
Correction: "ensure" is gap. No confirmation predicate exists in the predicate set. TEST fires on "tests" as the noun.
  "Ensure the tests are passing." → ["TEST"] (via "tests"; "ensure" contributes nothing)
  "Ensure the service is working." → [] (gap — "working" is not a TEST trigger)
Rule: "ensure" is always gap. Evaluate what follows it independently.

---

## TU008-004 — Teach: "expected behaviour" fires ARCHITECTURE via "define", not TEST

Misconception: Defining expected behaviour is test specification work and fires TEST.
Correction: "define" fires ARCHITECTURE. "expected behaviour" is gap as a standalone phrase — it does not fire TEST without a test action verb.
  "Define the expected behaviour." → ["ARCHITECTURE"]
  "Test the expected behaviour." → ["TEST"]
  "Assert the expected behaviour." → ["TEST"]
Rule: The action verb ("define" vs "test" vs "assert") determines which predicate fires.

---

## TU008-005 — Teach: "parametrize" is gap; TEST fires on object noun

Misconception: "parametrize the tests" fires IMPLEMENT because parametrization is a code change.
Correction: "parametrize" is gap (test framework keyword). TEST fires on "tests" as the object noun. No IMPLEMENT fires from "parametrize".
  "Parametrize the tests." → ["TEST"]
  "Add parametrized test cases." → ["IMPLEMENT", "TEST"] (IMPLEMENT via "add"; TEST via "test cases")
Rule: Test framework keywords ("parametrize", "fixture", "mock out", "stub out") are gap for IMPLEMENT. Use action verbs to fire IMPLEMENT.
