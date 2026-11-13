# Layer 0-B — Router Pairs (010)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: router

---

## C0B-010-001 — TEST+UI: "validate form accessibility" → 5.3 + 1.5

A: ["TEST", "UI"] | "Validate form accessibility: label associations, ARIA roles, keyboard navigation"
B: TEST → 5.3; UI → 1.5.
Reason: "validate" in test-assertion context → TEST (5.3). Form accessibility vocabulary → UI (1.5).

---

## C0B-010-002 — IMPLEMENT+TEST+UI: build accessible component + tests

A: ["IMPLEMENT", "TEST", "UI"] | "Build the accessible dropdown, write keyboard navigation tests"
B: IMPLEMENT → 4.1; TEST → 5.3; UI → 1.5. Three flows. TransitionType: feature_addition.

---

## C0B-010-003 — QA 7.1: release pipeline context

A: ["QA"] | "QA the production release before the branch cut"
B: Sub-phase 7.1 → ReleaseGateFlow71
Reason: "production release", "branch cut" → release pipeline vocabulary → 7.1.

---

## C0B-010-004 — TEST 5.1 for pure JavaScript function (no UI)

A: ["TEST"] | "Write unit tests for the date formatting utility"
B: Sub-phase 5.1 → UnitTestingFlow51
Reason: "unit tests" + "date formatting utility" (pure function, no UI/browser) → 5.1.

---

## C0B-010-005 — TEST+UI+QA: three-predicate, three flows (accessibility feature complete)

A: ["TEST", "UI", "QA"] | "Write accessibility tests, fix ARIA labels on all form controls, QA sign-off"
B: TEST → 5.3 (accessibility/E2E context); UI → 1.5; QA → 7.3. Three flows. QA last.
