# Layer 3 — Teaching Unit Pairs (006)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU006-001 — Teach: "refactor to make testable" fires IMPLEMENT (refactoring) only

Misconception: "refactor to make testable" should fire both IMPLEMENT and TEST because testability is a test concern.
Correction: "testable" is an adjective goal — it describes a desired state, not a test action. TEST fires on test action verbs and test-lexicon nouns, not on the word "testable".
  "Refactor to make it testable." → ["IMPLEMENT"] | TransitionType: refactoring
  "Refactor it and write unit tests after." → ["IMPLEMENT", "TEST"] (TEST from "unit tests")
Rule: Adjective goals ("testable", "maintainable", "readable") are gap. Predicate fires from verbs and nouns only.

---

## TU006-002 — Teach: "in-memory database" is gap; TEST fires from explicit test noun

Misconception: "set up an in-memory database for unit tests" should fire both IMPLEMENT (via "set up") and TEST.
Correction: "set up" is gap for IMPLEMENT. "in-memory database" is infrastructure gap. TEST fires only on "unit tests".
  "Set up an in-memory database for unit tests." → ["TEST"] only
Rule: Infrastructure setup verbs ("set up", "configure", "provision") are gap for IMPLEMENT. TEST fires on the test noun regardless.

---

## TU006-003 — Teach: "acceptance test" fires TEST via "test"

Misconception: "acceptance test" is a UAT (User Acceptance Testing) term and might route differently than unit tests.
Correction: "acceptance test" fires TEST via "test" (the head noun). All sub-types of test — acceptance, regression, mutation, integration, unit — fire the same TEST predicate.
  Sub-phase routing is determined separately: "acceptance test" → 5.1 (not E2E/browser); "E2E test" → 5.3.
Rule: All compound test types fire TEST. Sub-phase routing uses compound modifier, not predicate strength.

---

## TU006-004 — Teach: "edge case" is gap; "test the edge case" fires TEST

Misconception: "edge case" is a testing concept and should fire TEST.
Correction: "edge case" alone is gap — it describes a scenario category, not a test action.
  "Handle the edge case." → [] (gap — "handle" is gap, "edge case" is gap)
  "Test the edge case." → ["TEST"] (TEST from "test")
  "Write tests covering the edge case." → ["IMPLEMENT", "TEST"] (TEST from "tests", IMPLEMENT from "write")
Rule: Scenario vocabulary (edge case, happy path, corner case) is gap unless a TEST predicate trigger is also present.

---

## TU006-005 — Teach: "flaky test" fires TEST; "fix flaky test" does NOT add IMPLEMENT

Misconception: "fix the flaky test" fires both TEST (via "test") and IMPLEMENT (via "fix").
Correction: "fix" does not fire IMPLEMENT. TEST fires via "test". No IMPLEMENT.
  "Fix the flaky test." → ["TEST"] only
  "Rewrite the flaky test to remove the timing dependency." → ["IMPLEMENT", "TEST"] (IMPLEMENT via "rewrite")
Rule: "fix" → gap. Always. This is a hard lexicon boundary.
