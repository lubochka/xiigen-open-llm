# Layer 3 — Teaching Unit Pairs (012)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU012-001 — Teach: "test data builder" fires TEST via "test data"; IMPLEMENT fires via "build"

Misconception: "build a test data builder" fires IMPLEMENT only because it's a utility build task.
Correction: "build" → IMPLEMENT; "test data builder" → TEST fires on "test data" (canonical TEST trigger). Both fire.
  "Build a test data builder for the OrderService." → ["IMPLEMENT", "TEST"]
Rule: "test data" is a canonical TEST trigger. Compound utility names containing "test data" fire TEST.

---

## TU012-002 — Teach: "TDD" abbreviation alone is gap; IMPLEMENT fires via embedded verb

Misconception: "use TDD (Test-Driven Development)" fires TEST because TDD is about tests.
Correction: "TDD" abbreviation is gap for TEST (abbreviations require explicit expansion to fire predicates).
  "Use TDD." → [] (gap — "use" is gap; "TDD" is gap)
  "Develop using TDD." → ["IMPLEMENT"] (IMPLEMENT via "develop"; TDD still gap)
  "Write failing tests first using TDD." → ["IMPLEMENT", "TEST"] (IMPLEMENT via "write"; TEST via "tests")
Rule: TDD is a gap abbreviation. Use "test" or "tests" explicitly to fire TEST.

---

## TU012-003 — Teach: "green" and "red" (TDD states) are gap; TEST fires on "tests"

Misconception: "keep tests green" fires TEST with an added affirmative-state qualifier.
Correction: "green" and "red" as TDD state descriptors are gap. TEST fires on "tests" independently.
  "Keep all tests green." → ["TEST"] (TEST via "tests"; "green" contributes nothing; "keep" is gap)
  "The tests are red after the refactor." → ["TEST", "IMPLEMENT"] (TEST via "tests"; IMPLEMENT via "refactor")
Rule: Metaphorical state vocabulary (green, red, broken, healthy, stable) is gap. Evaluate the noun for predicate.

---

## TU012-004 — Teach: "parametrize" is gap; "add parametrized test cases" fires IMPLEMENT+TEST

Misconception: "parametrize the tests" fires IMPLEMENT because changing test structure is code work.
Correction: "parametrize" is gap. IMPLEMENT fires only from explicit action verbs.
  "Parametrize the tests." → ["TEST"] only (TEST via "tests"; "parametrize" is gap)
  "Add parametrized test cases." → ["IMPLEMENT", "TEST"] (IMPLEMENT via "add"; TEST via "test cases")
Rule: Framework keywords are gap for IMPLEMENT. "add", "write", "create", "build" fire IMPLEMENT.

---

## TU012-005 — Teach: "test fixture" is a canonical TEST trigger

Misconception: "test fixture" might be gap because it's infrastructure, not an assertion.
Correction: "test fixture" fires TEST via "test". It is a canonical TEST predicate trigger (same as "test data", "test double", "test plan").
  "Set up a test fixture." → ["TEST"] ("set up" is gap; TEST via "test fixture")
  "Implement a test fixture." → ["IMPLEMENT", "TEST"] (IMPLEMENT via "implement"; TEST via "test fixture")
Rule: All "test X" noun compounds fire TEST via the "test" component: test fixture, test data, test plan, test double, test suite, test case, test coverage.
