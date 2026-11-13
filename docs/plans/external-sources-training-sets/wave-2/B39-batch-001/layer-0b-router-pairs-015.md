# Layer 0-B — Router Pairs (015)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-015-001 — TEST+UI+QA: visual regression → 5.3 + 1.5 + 7.3

A: ["TEST", "UI", "QA"] | "Write visual regression tests for buttons, cards, modals and QA sign-off on baselines"
B: TEST → 5.3 (E2ETestingFlow53); UI → 1.5 (UIInteractionFlow15); QA → 7.3 (QAGatingFlow73).
Reason: "visual regression tests" → 5.3; "buttons/cards/modals" → UI → 1.5; "QA sign-off" → 7.3 (feature acceptance).

---

## C0B-015-002 — TEST+UI → 5.3 + 1.5: snapshot baseline for product card

A: ["TEST", "UI"] | "Create snapshot tests for the product card component across light and dark themes"
B: TEST → 5.3; UI → 1.5. Two flows.
Reason: "snapshot tests" → TEST → 5.3 (visual regression/snapshot is E2E domain); "product card component" → UI → 1.5.

---

## C0B-015-003 — UI alone → 1.5: layout update without test verb

A: ["UI"] | "Update snapshots after the redesign to reflect the new layout"
B: UI → 1.5 (UIInteractionFlow15). One flow.
Reason: "update" → gap (no IMPLEMENT trigger); "layout" → UI → 1.5. "update" is not a test-writing verb so TEST does not fire.

---

## C0B-015-004 — TEST+QA → 5.3 + 7.3: regression assert + QA

A: ["TEST", "QA"] | "Assert that all visual regression test baselines pass QA review"
B: TEST → 5.3; QA → 7.3. No UI predicate (no UI component vocabulary in this prompt).
Reason: "assert" → TEST → 5.3; "QA review" → QA → 7.3. Visual regression context confirms 5.3 not 5.1.

---

## C0B-015-005 — TEST+UI+QA → 5.3 + 1.5 + 7.3: full visual regression pipeline

A: ["TEST", "UI", "QA"] | "Write visual regression specs for each screen breakpoint, validate responsive layout components, and QA the suite"
B: TEST → 5.3; UI → 1.5; QA → 7.3. Three independent routing decisions.
AllSubPhaseIds: [5.3, 1.5, 7.3]
