# Layer 0-B — Router Pairs (052)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: router

---

## B15-router-052-001 — CompensationService: IMPLEMENT routing

A: "Create a CompensationService to execute rollback actions when saga steps fail."
B: Flow: ImplementationFlow41 | DecisionCount: 1

---

## B15-router-052-002 — CompensationService: IMPLEMENT+ARCHITECTURE routing

A: "Create CompensationService with a compensation domain model defining undo action chains."
B: Flow: ArchitectureFlow31 → ImplementationFlow41 | DecisionCount: 2

---

## B15-router-052-003 — CompensationService: IMPLEMENT+TEST routing

A: "Create CompensationService and write tests for all defined undo actions."
B: Flow: ImplementationFlow41 → TestingFlow51 | DecisionCount: 2

---

## B15-router-052-004 — CompensationService: refactoring routing

A: "Refactor CompensationService to use a registry pattern for undo action lookup."
B: Flow: ImplementationFlow41 | DecisionCount: 1 | TransitionType: refactoring

---

## B15-router-052-005 — CompensationService: IMPLEMENT+API_CONTRACT routing (rare case)

A: "Create CompensationService and wire up the saga step rollback API."
B: Flow: APIContractFlow33 → ImplementationFlow41 | DecisionCount: 2
"wire up" → API_CONTRACT even for internal saga compensation API calls.
