# Layer 0-A — Compiler Pairs (049)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-049-001 — PerformanceBudgetTest: predicate compilation

A: "Write E2E performance tests for the checkout page — assert page load completes within 2 seconds, verify the loading skeleton renders during fetch."
B: TEST (write, assert, verify). UI ("checkout page" → page; "loading skeleton" → skeleton — two triggers → one UI). Result: [TEST, UI]. TransitionType: documentation. Note: performance context does not change predicate or routing — still 5.3 for TEST.

---

## C0A-049-002 — PerformanceBudgetTest: "profile page load" is gap for TEST

A: "Profile the checkout page load time." Predicates?
B: "profile" is gap. "checkout page" → page → UI fires. Result: [UI]. TEST does not fire.

---

## C0A-049-003 — PerformanceBudgetTest: performance arbiter at Layer 2

A: "Assert the dashboard page load time is under 2 seconds." Which Node 07 arbiter is selected?
B: Performance arbiter is selected when explicit timing/budget vocabulary is present: "load time", "under X seconds", "p95", "p99", "SLA (Service Level Agreement)", "budget". TEST (assert). UI (dashboard page → page). Result: [TEST, UI] → E2ETestingFlow53 with performance arbiter at Node 07.

---

## C0A-049-004 — PerformanceBudgetTest: skeleton fires UI

A: Does "loading skeleton" fire UI?
B: Yes. "skeleton" is in the UI lexicon (skeleton loader category). "loading skeleton" fires UI via "skeleton".

---

## C0A-049-005 — PerformanceBudgetTest: QA before release with performance context

A: "Write performance budget E2E tests, assert page load time < 3s, and QA sign-off before release." Predicates?
B: TEST (write, assert). UI — need UI vocabulary. "page load time" — "page" may not be present as a standalone UI token here unless "checkout page" or similar is in the request. If no explicit UI vocabulary: [TEST, QA]. If "checkout page" or similar: [TEST, UI, QA]. QA → 7.1 (before release). TransitionType: documentation.
