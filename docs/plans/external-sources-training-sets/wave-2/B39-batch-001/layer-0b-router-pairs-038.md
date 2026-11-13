# Layer 0-B — Router Pairs (038)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-038-001 — DashboardTest: routing [TEST, UI]

A: "Write E2E tests for the dashboard page — assert the chart component." Predicates [TEST, UI]. Routing?
B: TEST → 5.3. UI → 1.5 (page, component). AllSubPhaseIds: [5.3, 1.5]. TransitionType: documentation.

---

## C0B-038-002 — DashboardTest: "dashboard" is not UI vocabulary — routing impact

A: "Check the dashboard data." Predicates []. Routing?
B: Zero predicates → knowledge_edge_synthetic. No routing. "dashboard" alone is gap for UI.

---

## C0B-038-003 — DashboardTest: navigation routes to 1.5

A: "Add test coverage for the dashboard navigation bar." Predicates [TEST, UI]. Routing?
B: TEST → 5.3. UI → 1.5 (navigation). AllSubPhaseIds: [5.3, 1.5].

---

## C0B-038-004 — DashboardTest: [TEST, UI, QA → 7.3]

A: "Write dashboard E2E tests, assert the chart component, and QA gate before sprint review." AllSubPhaseIds?
B: AllSubPhaseIds: [5.3, 1.5, 7.3]. QA → 7.3 ("before sprint review" = acceptance). Order: 5.3 + 1.5 → 7.3.

---

## C0B-038-005 — DashboardTest: 5.3 vs 5.1 disambiguation for dashboard

A: Does a dashboard test route to 5.3 or 5.1?
B: 5.3. Dashboard is a browser UI page — multi-step UI interaction context. 5.1 requires pure function/service logic without UI vocabulary. Component and page vocabulary confirm 5.3.
