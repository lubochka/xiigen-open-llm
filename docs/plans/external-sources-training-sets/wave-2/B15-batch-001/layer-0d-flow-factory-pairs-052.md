# Layer 0-D — Flow Factory Selection Pairs (052)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B15-factory-052-001 — CompensationService: IMPLEMENT factory call

Predicate: IMPLEMENT | Sub-phase: 4.1 | Factory: universal-nine-node-development-flow-4.1

---

## B15-factory-052-002 — CompensationService: IMPLEMENT+ARCHITECTURE factory calls

Predicate: ARCHITECTURE → 3.1 → universal-nine-node-architecture-flow-3.1
Predicate: IMPLEMENT → 4.1 → universal-nine-node-development-flow-4.1

---

## B15-factory-052-003 — CompensationService: IMPLEMENT+TEST factory calls

Predicate: IMPLEMENT → 4.1 → universal-nine-node-development-flow-4.1
Predicate: TEST → 5.1 → universal-nine-node-testing-flow-5.1

---

## B15-factory-052-004 — CompensationService: refactoring factory call

Predicate: IMPLEMENT | Factory: universal-nine-node-development-flow-4.1 | TransitionType: refactoring
Same factory ID as feature_addition. TransitionType: refactoring is metadata field.

---

## B15-factory-052-005 — CompensationService: IMPLEMENT+API_CONTRACT factory calls (rare)

Predicate: API_CONTRACT → 3.3 → universal-nine-node-contract-flow-3.3
Predicate: IMPLEMENT → 4.1 → universal-nine-node-development-flow-4.1
API_CONTRACT fires when "wire up" or "integrate" verb explicitly targets an external compensation API.
