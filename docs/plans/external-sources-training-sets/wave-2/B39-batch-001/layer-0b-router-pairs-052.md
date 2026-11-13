# Layer 0-B — Router Pairs (052)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-052-001 — TimeToInteractiveTest: routing

A: "Write E2E tests to assert TTI < 3.5s on the product listing page." Predicates [TEST, UI]. Routing?
B: TEST → 5.3. UI → 1.5 (page). AllSubPhaseIds: [5.3, 1.5].

---

## C0B-052-002 — TimeToInteractiveTest: skeleton → 1.5

A: "Create test coverage to verify the loading skeleton disappears before TTI threshold." AllSubPhaseIds?
B: [5.3, 1.5]. Skeleton → 1.5.

---

## C0B-052-003 — TimeToInteractiveTest: "measure TTI" → [UI] only

A: "Measure TTI for the dashboard page." AllSubPhaseIds?
B: [1.5]. measure is gap. UI fires (dashboard page → page, if parsed correctly).

---

## C0B-052-004 — TimeToInteractiveTest: submit button disabled → [TEST, UI]

A: "Assert the submit button is disabled while the page loads." AllSubPhaseIds?
B: [5.3, 1.5]. Button → 1.5. Page → 1.5. TEST → 5.3.

---

## C0B-052-005 — TimeToInteractiveTest: TTI does not change routing

A: Does a TTI assertion route TEST to a different sub-phase than standard E2E?
B: No. TEST always → 5.3 for E2E/browser context. TTI, LCP, FID, CLS, p95 — none change the predicate-to-sub-phase mapping.
