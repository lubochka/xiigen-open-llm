# Layer 0-B — Router Pairs (028)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: router

---

## B15-router-028-001 — TaxCalculationService: IMPLEMENT single route

A: Predicates: ["IMPLEMENT"] from "Build TaxCalculationService."
B: Route: ImplementationFlow41 (sub-phase 4.1) | DecisionCount: 1

---

## B15-router-028-002 — TaxCalculationService: IMPLEMENT+ARCHITECTURE routing

A: Predicates: ["IMPLEMENT", "ARCHITECTURE"] from "Build TaxCalculationService + architect jurisdiction model."
B: Routes: ArchitectureFlow31 (3.1) → ImplementationFlow41 (4.1) | Ordering: 3.1 first | DecisionCount: 2

---

## B15-router-028-003 — TaxCalculationService: IMPLEMENT+API_CONTRACT routing

A: Predicates: ["IMPLEMENT", "API_CONTRACT"] from "Build TaxCalculationService + service contracts."
B: Routes: APIContractFlow33 (3.3) → ImplementationFlow41 (4.1) | Ordering: 3.3 before 4.1 | DecisionCount: 2

---

## B15-router-028-004 — TaxCalculationService: IMPLEMENT+TEST routing

A: Predicates: ["IMPLEMENT", "TEST"] from "Build TaxCalculationService + write tests."
B: Routes: ImplementationFlow41 (4.1) → TestingFlow51 (5.1) | implement before test | DecisionCount: 2

---

## B15-router-028-005 — TaxCalculationService: IMPLEMENT+ARCHITECTURE+API_CONTRACT+TEST

A: Predicates: ["IMPLEMENT", "ARCHITECTURE", "API_CONTRACT", "TEST"] from full tax service request.
B: Routes: ArchitectureFlow31 (3.1) → APIContractFlow33 (3.3) → ImplementationFlow41 (4.1) → TestingFlow51 (5.1) | DecisionCount: 4
Canonical ordering: ARCHITECTURE first, then API_CONTRACT, then IMPLEMENT, then TEST.
