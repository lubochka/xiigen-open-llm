# Layer 0-B — Router Pairs (017)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-017-001 — TEST+UI → 5.3 + 1.5: snapshot tests for component library

A: ["TEST", "UI"] | "Write snapshot tests for button, input, and dropdown components"
B: TEST → 5.3; UI → 1.5. "snapshot tests" → 5.3 (visual regression domain); "button/input/dropdown" → UI → 1.5.

---

## C0B-017-002 — TEST+UI+IMPLEMENT → 5.3 + 1.5 + 4.1: build harness + create tests

A: ["TEST", "UI", "IMPLEMENT"] | "Build the screenshot test harness and create snapshot assertions for every modal variant"
B: TEST → 5.3; UI → 1.5; IMPLEMENT → 4.1. Three independent flows.
TransitionType: feature_addition (IMPLEMENT present).

---

## C0B-017-003 — UI alone → 1.5: update snapshots

A: ["UI"] | "Update snapshots after the redesign to reflect the new layout"
B: UI → 1.5. One routing decision. "layout" fires UI. "update" is gap so IMPLEMENT and TEST do not fire.

---

## C0B-017-004 — TEST+UI → 5.3 + 1.5: assert snapshot matches pagination

A: ["TEST", "UI"] | "Assert that the pagination component snapshot matches the approved baseline"
B: TEST → 5.3; UI → 1.5. "assert" → TEST → 5.3 (snapshot assertion in E2E context); "pagination component" → UI → 1.5.

---

## C0B-017-005 — TEST+UI+QA → 5.3 + 1.5 + 7.3: full snapshot review

A: ["TEST", "UI", "QA"] | "Create visual snapshot tests for each tooltip variant and QA sign-off on all baselines"
B: TEST → 5.3; UI → 1.5; QA → 7.3.
AllSubPhaseIds: [5.3, 1.5, 7.3]
