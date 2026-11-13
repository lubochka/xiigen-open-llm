# Layer 0-B — Router Pairs (044)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: router

---

## B15-router-044-001 — ExportService: IMPLEMENT routing

A: "Implement an ExportService that generates CSV and JSON exports from backend data."
B: Flow: ImplementationFlow41 | DecisionCount: 1

---

## B15-router-044-002 — ExportService: IMPLEMENT+SECURITY routing

A: "Implement ExportService and secure the export download endpoint."
B: Flow: SecurityAuditFlow → ImplementationFlow41 | DecisionCount: 2

---

## B15-router-044-003 — ExportService: IMPLEMENT+API_CONTRACT routing

A: "Implement ExportService and define the export data contracts."
B: Flow: APIContractFlow33 → ImplementationFlow41 | DecisionCount: 2

---

## B15-router-044-004 — ExportService: IMPLEMENT+DOCUMENT routing

A: "Implement ExportService and document the supported export formats."
B: Flow: ImplementationFlow41 ‖ DocumentationFlow21 | DecisionCount: 2

---

## B15-router-044-005 — ExportService refactoring: same routing, different TransitionType

A: "Refactor ExportService to support streaming exports."
B: Flow: ImplementationFlow41 | DecisionCount: 1 | TransitionType: refactoring
Routing is identical to feature_addition. TransitionType: refactoring propagated to ImplementationFlow41 only.
