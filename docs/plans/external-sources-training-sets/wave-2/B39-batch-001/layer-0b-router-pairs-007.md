# Layer 0-B — Router Pairs (007)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: router

---

## C0B-007-001 — TEST 5.3 for visual regression context

A: ["TEST"] | "Add visual regression tests for the header component"
B: Sub-phase 5.3 → E2ETestingFlow53
Reason: "visual regression tests" are system-level snapshot tests → 5.3 (E2E context).

---

## C0B-007-002 — IMPLEMENT+UI+QA: build + QA gate

A: ["IMPLEMENT", "UI", "QA"] | "Build the notification banner and QA it"
B: IMPLEMENT → 4.1; UI → 1.5; QA → 7.3. Three flows. TransitionType: feature_addition. QA last.
Reason: "build" → IMPLEMENT (4.1). "notification banner" → UI (1.5). "QA it" → QA (7.3). QA validates complete output.

---

## C0B-007-003 — TEST only (no UI): pure API test

A: ["TEST"] | "Write integration tests for the cart API response format"
B: Sub-phase 5.3 → E2ETestingFlow53 (API integration context)
Reason: API integration tests are system-level → 5.3. No UI predicate (no UI vocabulary).

---

## C0B-007-004 — UI+QA: two flows, QA last

A: ["UI", "QA"] | "Fix button disabled states and QA review"
B: UI → 1.5; QA → 7.3. Two flows. QA executes last. TransitionType: from UI context (not QA).
Reason: UI → 1.5; QA → 7.3; QA always last. No IMPLEMENT → TransitionType not feature_addition.

---

## C0B-007-005 — TEST+UI disambiguation: "write tests for the UI" vs "fix the UI"

A (TEST+UI): "Write tests for the modal UI" → TEST→5.3 + UI→1.5 (two flows)
A (UI only): "Fix the modal focus behavior" → UI→1.5 (one flow)
Reason: Test-writing verb is required to fire TEST. Fixing a UI issue alone fires UI only.
