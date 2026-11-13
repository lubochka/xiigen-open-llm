# Layer 0-B — Router Pairs (049)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-049-001 — PerformanceBudgetTest: routing

A: "Write E2E performance tests for the checkout page — assert load time and verify skeleton." Predicates [TEST, UI]. Routing?
B: TEST → 5.3. UI → 1.5 (page, skeleton). AllSubPhaseIds: [5.3, 1.5]. TransitionType: documentation.

---

## C0B-049-002 — PerformanceBudgetTest: "profile page load" → [UI] only

A: "Profile the checkout page load time." Predicates [UI]. AllSubPhaseIds?
B: AllSubPhaseIds: [1.5]. Only UIInteractionFlow15. No 5.3 (TEST did not fire).

---

## C0B-049-003 — PerformanceBudgetTest: performance metrics do not affect routing

A: Does the presence of LCP, p95, or budget metrics change sub-phase routing?
B: No. Metric vocabulary does not change routing. TEST still → 5.3. UI still → 1.5. The only routing impact of performance vocabulary is at Node 07 of E2ETestingFlow53 (arbiter selection: performance arbiter vs. coverage arbiter).

---

## C0B-049-004 — PerformanceBudgetTest: QA before release → 7.1

A: "Write performance tests, assert page load < 3s, and QA sign-off before release." AllSubPhaseIds?
B: [5.3, 1.5, 7.1] (if UI vocabulary present) or [5.3, 7.1] (if no UI vocabulary). QA → 7.1 (before release).

---

## C0B-049-005 — PerformanceBudgetTest: skeleton always routes to 1.5

A: Does a "loading skeleton" in a performance test context route to 1.5 or 5.3?
B: 1.5. Skeleton is UI vocabulary. UI always routes to 1.5 regardless of context (performance, accessibility, visual regression).
