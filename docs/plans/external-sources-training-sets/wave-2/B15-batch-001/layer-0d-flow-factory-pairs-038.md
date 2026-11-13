# Layer 0-D — Flow Factory Selection Pairs (038)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B15-factory-038-001 — AddressValidationService: IMPLEMENT factory call

Predicate: IMPLEMENT | Sub-phase: 4.1 | Factory: universal-nine-node-development-flow-4.1
TrustState: deterministic_generic | TransitionType: feature_addition

---

## B15-factory-038-002 — AddressValidationService: IMPLEMENT+API_CONTRACT factory calls

Predicate: API_CONTRACT → 3.3 → universal-nine-node-contract-flow-3.3 (deterministic_generic)
Predicate: IMPLEMENT → 4.1 → universal-nine-node-development-flow-4.1 (deterministic_generic)

---

## B15-factory-038-003 — AddressValidationService: IMPLEMENT+TEST factory calls

Predicate: IMPLEMENT → 4.1 → universal-nine-node-development-flow-4.1
Predicate: TEST → 5.1 → universal-nine-node-testing-flow-5.1
Execution order: ImplementationFlow41 first, then TestingFlow51.

---

## B15-factory-038-004 — TEST factory selection: 5.1 vs. 5.3

"write unit tests" or "write integration tests" → TestingFlow51 (sub-phase 5.1).
"write end-to-end tests" or "write E2E tests" → E2ETestingFlow53 (sub-phase 5.3).
Default TEST factory selection is sub-phase 5.1 when test type not specified as E2E.

---

## B15-factory-038-005 — QA factory selection: 7.1 vs. 7.3

"QA in staging" or "staging validation" → QAGatingFlow71 (sub-phase 7.1).
"QA before production release" or "production release gate" → ReleaseGateFlow73 (sub-phase 7.3).
Default QA factory selection is sub-phase 7.1 when environment not specified.
