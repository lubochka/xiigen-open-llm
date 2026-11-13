# Layer 0-D — Flow Factory Selection Pairs (044)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B15-factory-044-001 — ExportService: IMPLEMENT factory call

Predicate: IMPLEMENT | Sub-phase: 4.1 | Factory: universal-nine-node-development-flow-4.1

---

## B15-factory-044-002 — ExportService: IMPLEMENT+SECURITY factory calls

Predicate: SECURITY → security → universal-security-audit-flow
Predicate: IMPLEMENT → 4.1 → universal-nine-node-development-flow-4.1

---

## B15-factory-044-003 — ExportService: IMPLEMENT+API_CONTRACT factory calls

Predicate: API_CONTRACT → 3.3 → universal-nine-node-contract-flow-3.3
Predicate: IMPLEMENT → 4.1 → universal-nine-node-development-flow-4.1

---

## B15-factory-044-004 — ExportService: IMPLEMENT+API_CONTRACT+DOCUMENT ("define contracts") factory calls

When "define the export data contracts" is present: API_CONTRACT + DOCUMENT both fire.
Three factory calls:
- API_CONTRACT → 3.3 → universal-nine-node-contract-flow-3.3
- IMPLEMENT → 4.1 → universal-nine-node-development-flow-4.1
- DOCUMENT → 2.1 → universal-nine-node-documentation-flow-2.1

---

## B15-factory-044-005 — ExportService: refactoring TransitionType

"Refactor ExportService to support streaming exports" → IMPLEMENT | refactoring.
Factory call: universal-nine-node-development-flow-4.1 with TransitionType: refactoring metadata.
Same factory ID — TransitionType is a metadata field passed to the factory call, not a different factory.
