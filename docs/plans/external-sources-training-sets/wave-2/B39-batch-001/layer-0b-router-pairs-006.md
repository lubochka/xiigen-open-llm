# Layer 0-B — Router Pairs (006)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: router

---

## C0B-006-001 — v6-02 corruption: gap suppressed to TEST

A: [] | "Automate the E2E test suite"
B: [] → GapHandler (correct)
Corrupt: ["TEST"] ← v6-02 ("automate" incorrectly maps to TEST)
Reason: "automate" is gap. TEST requires test-writing verbs (write, create, add, assert, validate in test context).

---

## C0B-006-002 — v6-01: TEST_AUTOMATION invented

A: ["TEST"] | "Write automated E2E tests for the checkout flow"
B: ["TEST"] (correct)
Corrupt: ["TEST", "TEST_AUTOMATION"] ← v6-01 (TEST_AUTOMATION is not a valid predicate)
Reason: TEST_AUTOMATION is not a predicate. "automated" is an adjective modifying "tests". One TEST predicate.

---

## C0B-006-003 — v6-03: TEST+UI+QA collapsed to TEST only

A: ["TEST", "UI", "QA"] | "Write UI tests, fix button styling, QA sign-off"
B: Three flows (5.3 + 1.5 + 7.3) (correct)
Corrupt: [E2ETestingFlow53] only ← v6-03 (UI and QA suppressed)
Reason: No cap on flow count. All three predicates independently instantiate flows.

---

## C0B-006-004 — UI routes to 1.5, not 5.3 (contrast)

A: ["UI"] | "The button animation is jerky on slow connections"
B: Sub-phase 1.5 → UIInteractionFlow15 (correct)
Corrupt: Sub-phase 5.3 → E2ETestingFlow53 ← wrong (UI predicate routes to 1.5 always)
Reason: UI routes to 1.5 regardless of context. TEST routes to 5.3 for UI/E2E tests.

---

## C0B-006-005 — Zero predicates: framework config → GapHandler

A: [] | "Install and configure Playwright 1.40"
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "install and configure" + tool version → operational gap. No predicate fires.
