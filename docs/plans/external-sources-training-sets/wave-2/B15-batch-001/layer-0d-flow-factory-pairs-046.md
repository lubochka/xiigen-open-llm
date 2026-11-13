# Layer 0-D — Flow Factory Selection Pairs (046)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B15-factory-046-001 — MigrationService: IMPLEMENT factory call

Predicate: IMPLEMENT | Sub-phase: 4.1 | Factory: universal-nine-node-development-flow-4.1

---

## B15-factory-046-002 — MigrationService: IMPLEMENT+SECURITY factory calls

Predicate: SECURITY → security → universal-security-audit-flow
Predicate: IMPLEMENT → 4.1 → universal-nine-node-development-flow-4.1

---

## B15-factory-046-003 — MigrationService: IMPLEMENT+ARCHITECTURE factory calls

Predicate: ARCHITECTURE → 3.1 → universal-nine-node-architecture-flow-3.1
Predicate: IMPLEMENT → 4.1 → universal-nine-node-development-flow-4.1

---

## B15-factory-046-004 — MigrationService: IMPLEMENT+TEST+DOCUMENT factory calls

Predicate: IMPLEMENT → 4.1 → universal-nine-node-development-flow-4.1
Predicate: TEST → 5.1 → universal-nine-node-testing-flow-5.1
Predicate: DOCUMENT → 2.1 → universal-nine-node-documentation-flow-2.1

---

## B15-factory-046-005 — MigrationService: refactoring TransitionType factory call

"Refactor MigrationService to use a streaming pipeline" → universal-nine-node-development-flow-4.1 with TransitionType: refactoring.
TransitionType: refactoring triggers backward-compatibility check inside ImplementationFlow41.
No additional factory call created for refactoring mode — same factory ID, additional metadata only.
