# Layer 0-B — Router Pairs (038)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: router

---

## B15-router-038-001 — AddressValidationService: IMPLEMENT routing

A: "Implement an AddressValidationService to normalize and verify customer addresses."
B: Flow: ImplementationFlow41 | DecisionCount: 1 | NearestWrong: 4.2 Integration (0.40)

---

## B15-router-038-002 — AddressValidationService: IMPLEMENT+API_CONTRACT routing

A: "Implement AddressValidationService and integrate with the postal verification API."
B: Flow: APIContractFlow33 → ImplementationFlow41 | DecisionCount: 2

---

## B15-router-038-003 — AddressValidationService: IMPLEMENT+ARCHITECTURE routing

A: "Implement AddressValidationService with an address domain model."
B: Flow: ArchitectureFlow31 → ImplementationFlow41 | DecisionCount: 2

---

## B15-router-038-004 — AddressValidationService: IMPLEMENT+TEST routing

A: "Implement AddressValidationService and write unit tests for all validation rules."
B: Flow: ImplementationFlow41 → TestingFlow51 | DecisionCount: 2 | Ordering: IMPLEMENT before TEST

---

## B15-router-038-005 — TEST routing position confirmed

TEST (5.1) always routes AFTER IMPLEMENT (4.1). Anti-pattern: routing TEST before IMPLEMENT or in place of IMPLEMENT.
Correct: ImplementationFlow41 → TestingFlow51 for IMPLEMENT+TEST combination.
