# Layer 0-D — Flow Factory Selection Pairs (028)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B15-factory-028-001 — TaxCalculationService: IMPLEMENT factory call

A: Predicate: IMPLEMENT | Sub-phase: 4.1
B: Factory selects: universal-nine-node-development-flow-4.1 | deterministic_generic

---

## B15-factory-028-002 — TaxCalculationService: ARCHITECTURE factory call

A: Predicate: ARCHITECTURE | Sub-phase: 3.1
B: Factory selects: universal-nine-node-architecture-flow-3.1 | deterministic_generic

---

## B15-factory-028-003 — TaxCalculationService: four-predicate factory calls

A: Predicates: ["IMPLEMENT", "ARCHITECTURE", "API_CONTRACT", "TEST"]
B: Four factory calls:
- ARCHITECTURE → universal-nine-node-architecture-flow-3.1
- API_CONTRACT → universal-nine-node-contract-flow-3.3
- IMPLEMENT → universal-nine-node-development-flow-4.1
- TEST → universal-nine-node-testing-flow-5.1

---

## B15-factory-028-004 — Gate B criteria for promotion (reference)

Gate B passes when ALL of: all 9 nodes covered, min_confidence ≥ 0.89, ≥25 production evidence pairs.
Gate B fails when ANY of: missing node, confidence below 0.89, insufficient evidence.
TrustState promotion: probationary_accepted → trusted_learned after Gate B pass + threshold.

---

## B15-factory-028-005 — TransitionType: refactoring scope is 4.1 only

TransitionType: refactoring is passed as metadata only to the factory call for sub-phase 4.1.
The factory calls for 3.1, 3.3, security, 5.1, 2.1, 7.1 do not receive TransitionType metadata.
ImplementationFlow41 reads TransitionType from SelectedLogicalTransition to determine execution mode.
