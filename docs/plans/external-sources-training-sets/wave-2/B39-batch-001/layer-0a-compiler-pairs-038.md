# Layer 0-A — Compiler Pairs (038)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-038-001 — DashboardTest: predicate compilation

A: "Write E2E tests for the dashboard page — assert the summary card renders, verify the chart component updates."
B: TEST ("write E2E tests", "assert", "verify"). UI ("dashboard page" → page; "summary card" — "card" is not in the standard UI lexicon for B39; "chart component" → component). Result: TEST (three triggers → one). UI: page + component → two triggers → one UI. [TEST, UI]. TransitionType: documentation.

---

## C0A-038-002 — DashboardTest: "dashboard" alone is gap for UI

A: "Check the dashboard data." Does UI fire?
B: "dashboard" alone is not in the UI lexicon. "check" is gap. Zero predicates → knowledge_edge_synthetic.

---

## C0A-038-003 — DashboardTest: navigation fires UI

A: "Add test coverage for the dashboard navigation bar." Predicates?
B: TEST ("add test coverage"). UI ("navigation bar" → navigation). Result: [TEST, UI].

---

## C0A-038-004 — DashboardTest: [TEST, UI, QA] with gate before sprint

A: "Write dashboard E2E tests, assert the chart component, and QA gate before sprint review." Predicates?
B: TEST (write, assert). UI (component). QA (QA gate). QA → 7.3 ("before sprint review" = acceptance). [TEST, UI, QA]. AllSubPhaseIds: [5.3, 1.5, 7.3]. TransitionType: documentation.

---

## C0A-038-005 — DashboardTest: "describe dashboard" is gap

A: "Describe the dashboard behavior for users." Any predicates?
B: "describe" is gap. "dashboard" is not UI vocabulary. Zero predicates → knowledge_edge_synthetic.
