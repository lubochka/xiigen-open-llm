# Layer 0-B — Router Pairs (012)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: router

---

## C0B-012-001 — IMPLEMENT+TEST+UI+QA: maximum four-predicate UI flow

A: ["IMPLEMENT", "TEST", "UI", "QA"] | "Build carousel component, write interaction tests, fix CSS animation, QA sign-off"
B: IMPLEMENT → 4.1; TEST → 5.3; UI → 1.5; QA → 7.3. Four flows. TransitionType: feature_addition. QA last.

---

## C0B-012-002 — TEST+UI ambiguity resolved: "create test for component"

A: ["TEST", "UI"] | "Create tests for the breadcrumb navigation component"
B: TEST → 5.3; UI → 1.5.
Reason: "create tests" → TEST; "breadcrumb navigation component" → UI. Both fire.

---

## C0B-012-003 — v6-02 gap suppression: "visual check" → UI (wrong)

A: [] | "Do a visual check of the UI before shipping"
B (correct): []
Corrupt: ["UI"] ← v6-02 ("visual check" incorrectly mapped to UI predicate)
Reason: "visual check" is observational gap. UI requires explicit UI vocabulary (component names, interaction states, layout, styling). Generic "visual check" is not a UI trigger.

---

## C0B-012-004 — UI fires on "animation" vocabulary

A: ["UI"] | "The loading spinner animation is jittery on Firefox"
B: UI → 1.5 (UIInteractionFlow15)
Reason: "loading spinner animation" → UI predicate (animation/interaction vocabulary).

---

## C0B-012-005 — TEST+UI+QA: three-predicate for animation fix

A: ["TEST", "UI", "QA"] | "Write cross-browser tests for the spinner animation and QA sign-off"
B: TEST → 5.3 (cross-browser context); UI → 1.5; QA → 7.3. Three flows. QA last.
