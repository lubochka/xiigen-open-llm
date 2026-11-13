# Layer 0-B — Router Pairs (039)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-039-001 — ReportGenerationTest: routing [TEST, UI]

A: "Create E2E tests for the report generation page — assert the generate button fires the export modal." Predicates [TEST, UI]. Routing?
B: TEST → 5.3. UI → 1.5 (page, button, modal). AllSubPhaseIds: [5.3, 1.5]. TransitionType: documentation.

---

## C0B-039-002 — ReportGenerationTest: "schedule report" gap routing

A: "Schedule the weekly report generation." Predicates []. Routing?
B: Zero predicates → knowledge_edge_synthetic. No routing. SCHEDULE_CAPABILITY_LEARNING emitted.

---

## C0B-039-003 — ReportGenerationTest: dropdown routes to 1.5

A: "Assert the date range dropdown for report generation." Predicates [TEST, UI]. AllSubPhaseIds?
B: AllSubPhaseIds: [5.3, 1.5]. Dropdown → 1.5 (always). TEST → 5.3.

---

## C0B-039-004 — ReportGenerationTest: IMPLEMENT → feature_addition

A: [IMPLEMENT, TEST, UI]. AllSubPhaseIds and TransitionType?
B: AllSubPhaseIds: [4.1, 5.3, 1.5]. TransitionType: feature_addition. Order: 4.1 → 5.3 + 1.5.

---

## C0B-039-005 — ReportGenerationTest: export modal routes to 1.5

A: Does an "export modal" route to 5.3 or 1.5?
B: 1.5. All modal vocabulary routes to 1.5 (UIInteractionFlow15). Modal is UI vocabulary — it is never routed to 5.3 regardless of context.
