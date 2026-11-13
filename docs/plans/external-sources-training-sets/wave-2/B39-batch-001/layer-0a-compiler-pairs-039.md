# Layer 0-A — Compiler Pairs (039)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-039-001 — ReportGenerationTest: predicate compilation

A: "Create E2E test scenarios for the report generation page — assert the generate button fires the export modal."
B: TEST ("create E2E test scenarios", "assert"). UI ("report generation page" → page; "generate button" → button; "export modal" → modal — three triggers → one UI). Result: [TEST, UI]. TransitionType: documentation.

---

## C0A-039-002 — ReportGenerationTest: "download" is not a predicate trigger

A: "Write tests to verify the report download button triggers a file download." Predicates?
B: TEST (write tests, verify). UI ("report download button" → button). Result: [TEST, UI]. "download" is not a UI vocabulary word and not a TEST/IMPLEMENT trigger. Only "button" fires UI.

---

## C0A-039-003 — ReportGenerationTest: dropdown fires UI

A: "Assert the date range dropdown for the report generator renders correctly." Predicates?
B: TEST (assert). UI ("date range dropdown" → dropdown). Result: [TEST, UI].

---

## C0A-039-004 — ReportGenerationTest: IMPLEMENT fires on "build report generator"

A: "Build the report generation feature and write E2E tests for the report page." Predicates?
B: IMPLEMENT ("build the report generation feature"). TEST ("write E2E tests"). UI ("report page" → page). [IMPLEMENT, TEST, UI]. TransitionType: feature_addition.

---

## C0A-039-005 — ReportGenerationTest: "schedule report" gap

A: "Schedule the weekly report generation." Predicates?
B: "schedule" is gap. "report generation" has no UI vocabulary. Zero predicates → knowledge_edge_synthetic.
