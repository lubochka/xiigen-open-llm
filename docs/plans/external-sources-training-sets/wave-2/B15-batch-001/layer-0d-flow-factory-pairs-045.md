# Layer 0-D — Flow Factory Selection Pairs (045)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B15-factory-045-001 — ImportService: IMPLEMENT factory call

Predicate: IMPLEMENT | Sub-phase: 4.1 | Factory: universal-nine-node-development-flow-4.1

---

## B15-factory-045-002 — ImportService: IMPLEMENT+SECURITY factory calls

Predicate: SECURITY → security → universal-security-audit-flow
Predicate: IMPLEMENT → 4.1 → universal-nine-node-development-flow-4.1

---

## B15-factory-045-003 — ImportService: IMPLEMENT+API_CONTRACT factory calls

Predicate: API_CONTRACT → 3.3 → universal-nine-node-contract-flow-3.3
Predicate: IMPLEMENT → 4.1 → universal-nine-node-development-flow-4.1

---

## B15-factory-045-004 — ImportService: IMPLEMENT+API_CONTRACT+SECURITY+TEST four factory calls

1. API_CONTRACT → 3.3 → universal-nine-node-contract-flow-3.3
2. SECURITY → security → universal-security-audit-flow
3. IMPLEMENT → 4.1 → universal-nine-node-development-flow-4.1
4. TEST → 5.1 → universal-nine-node-testing-flow-5.1

---

## B15-factory-045-005 — TEST factory: 5.1 default confirmed

"write integration tests" → sub-phase 5.1 → universal-nine-node-testing-flow-5.1.
"write unit tests" → sub-phase 5.1 → universal-nine-node-testing-flow-5.1.
E2E (End-to-End) tests are the only case where sub-phase 5.3 is selected.
