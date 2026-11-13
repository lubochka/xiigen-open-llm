# Layer 3 — Teaching Unit Pairs (004)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU004-001 — TEST+UI: "assert" fires TEST, UI vocabulary fires UI

A: "Assert that the progress bar fills correctly during file upload."
→ ["TEST", "UI"] — "assert" → TEST (5.3); "progress bar" → UI. Two predicates.

---

## TU004-002 — QA last: TEST+UI+QA ordering

A: "Write tests for the toggle, fix toggle styling, QA sign-off."
Predicates: ["TEST", "UI", "QA"] | Execution: TEST+UI → QA (always last)
Teaching: QA gate executes after all other flows complete. QA validates test results AND UI output.

---

## TU004-003 — "validate" in UI/test context fires TEST+UI

A: "Validate that the dropdown renders all options at 320px viewport."
→ ["TEST", "UI"] — "validate" (assertion) → TEST; "dropdown renders" + "320px viewport" → UI.
Teaching: "validate" in test-assertion context with UI vocabulary → TEST+UI.

---

## TU004-004 — IMPLEMENT+TEST+UI: building and testing a new component

A: "Build the stepper component and write tests for step navigation."
→ ["IMPLEMENT", "TEST", "UI"] — "build" → IMPLEMENT; "write tests" → TEST; "stepper component" → UI.
Teaching: Three distinct predicates from one request. No predicate is subsumed by another.

---

## TU004-005 — QA does not drive TransitionType

A: "Write tests for the carousel and QA sign-off."
→ ["TEST", "QA"] | TransitionType: documentation (TEST without IMPLEMENT → documentation)
Teaching: QA does not make TransitionType feature_addition. Only IMPLEMENT does. TEST+QA → documentation.
