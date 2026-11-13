# Layer 1 — Transition Orchestrator Pairs (036)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B15-t1-036-001 — ExportService: IMPLEMENT single transition

A: "Implement an ExportService that generates CSV and JSON exports."
B: TransitionType: feature_addition | SubPhaseIds: ["4.1"] | ContextCarryForward: true

---

## B15-t1-036-002 — ExportService: IMPLEMENT+API_CONTRACT transition

A: "Implement ExportService and define the export data contracts."
B: TransitionType: feature_addition | SubPhaseIds: ["3.3", "4.1", "2.1"] | ContextCarryForward: true
"define contracts" → API_CONTRACT + DOCUMENT; both "3.3" and "2.1" in SubPhaseIds.

---

## B15-t1-036-003 — ExportService: refactoring transition

A: "Refactor ExportService to support streaming exports."
B: TransitionType: refactoring | SubPhaseIds: ["4.1"] | ContextCarryForward: true

---

## B15-t1-036-004 — "define contracts" dual-predicate SubPhaseIds confirmed

"define the [X] contracts" → API_CONTRACT + DOCUMENT → SubPhaseIds includes both "3.3" and "2.1".
This rule applies to all B15 services where "define ... contracts" pattern is present.

---

## B15-t1-036-005 — ImportService: IMPLEMENT+API_CONTRACT+SECURITY+TEST transition

A: "Create ImportService, wire up the source API, authenticate connections, and write integration tests."
B: TransitionType: feature_addition | SubPhaseIds: ["3.3", "security", "4.1", "5.1"] | ContextCarryForward: true
