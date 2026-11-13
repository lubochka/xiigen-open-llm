# Layer 0-B — Router Pairs (008)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: router

---

## C0B-008-001 — TEST+UI: "assert UI renders" → 5.3 + 1.5

A: ["TEST", "UI"] | "Assert the product grid renders correctly at all breakpoints"
B: TEST → 5.3; UI → 1.5.
Reason: "assert" in responsive/render context → TEST (5.3). "product grid", "breakpoints" → UI (1.5).

---

## C0B-008-002 — DOCUMENT+UI: documentation about UI component

A: ["UI", "DOCUMENT"] | "Write documentation for the design system button variants"
B: UI → 1.5; DOCUMENT → DocumentationFlow62.
Reason: "write documentation" → DOCUMENT. "button variants" → UI. Two flows.

---

## C0B-008-003 — TEST+UI+QA routing to three flows (5.3 + 1.5 + 7.3)

A: ["TEST", "UI", "QA"] | "Add E2E tests for the search bar, fix placeholder text, QA sign-off"
B: TEST → E2ETestingFlow53 (5.3); UI → UIInteractionFlow15 (1.5); QA → QAGatingFlow73 (7.3). Three flows. QA last.

---

## C0B-008-004 — IMPLEMENT+TEST+UI+QA: four flows (4.1 + 5.3 + 1.5 + 7.3)

A: ["IMPLEMENT", "TEST", "UI", "QA"] | "Build date picker, write interaction tests, fix calendar styling, QA sign-off"
B: IMPLEMENT → 4.1; TEST → 5.3; UI → 1.5; QA → 7.3. Four flows. QA last. TransitionType: feature_addition.

---

## C0B-008-005 — Gap route: "run tests" without writing verb

A: [] | "Run the E2E test suite in headless mode"
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "run" is operational gap. TEST requires test-writing verbs, not execution commands.
