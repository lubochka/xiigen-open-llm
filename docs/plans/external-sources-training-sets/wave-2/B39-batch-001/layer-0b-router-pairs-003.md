# Layer 0-B — Router Pairs (003)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: router

---

## C0B-003-001 — TEST 5.1 vs 5.3: unit test → 5.1

A: ["TEST"] | "Write unit tests for the form validation utility function"
B: Sub-phase 5.1 → UnitTestingFlow51
Reason: "unit tests" + "utility function" (pure function, not browser interaction) → 5.1.

---

## C0B-003-002 — TEST 5.3: browser/interaction test → 5.3

A: ["TEST"] | "Write interaction tests for the button click handler in the browser"
B: Sub-phase 5.3 → E2ETestingFlow53
Reason: "browser", "interaction tests", UI elements → 5.3. System-level test type.

---

## C0B-003-003 — TEST 5.3: smoke tests → 5.3

A: ["TEST"] | "Write smoke tests for the checkout flow"
B: Sub-phase 5.3 → E2ETestingFlow53
Reason: "smoke tests" are system-level → 5.3. Smoke tests are never unit tests.

---

## C0B-003-004 — TEST+UI+QA routes three flows

A: ["TEST", "UI", "QA"] | "Write E2E tests for the form, fix form styling, QA sign-off"
B: TEST → E2ETestingFlow53 (5.3); UI → UIInteractionFlow15 (1.5); QA → QAGatingFlow73 (7.3). Three flows.
Reason: Each predicate independently routes to its sub-phase. Three flows instantiated.

---

## C0B-003-005 — Zero predicates → GapHandler

A: [] | "Set up Playwright for the E2E test suite"
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING emitted
Reason: "set up" is operational gap; "Playwright" is framework name → gap. No predicate fires.
