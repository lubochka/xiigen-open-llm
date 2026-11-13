# Layer 0-B — Router Pairs (029)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: router

---

## B15-router-029-001 — CurrencyConversionService: IMPLEMENT single route

A: Predicates: ["IMPLEMENT"] from "Implement CurrencyConversionService."
B: Route: ImplementationFlow41 (sub-phase 4.1) | DecisionCount: 1

---

## B15-router-029-002 — CurrencyConversionService: IMPLEMENT+API_CONTRACT routing

A: Predicates: ["IMPLEMENT", "API_CONTRACT"] from "Implement CurrencyConversionService + integrate forex provider."
B: Routes: APIContractFlow33 (3.3) → ImplementationFlow41 (4.1) | DecisionCount: 2

---

## B15-router-029-003 — CurrencyConversionService: IMPLEMENT+SECURITY routing

A: Predicates: ["IMPLEMENT", "SECURITY"] from "Implement CurrencyConversionService + protect cache."
B: Routes: SecurityAuditFlow → ImplementationFlow41 (4.1) | DecisionCount: 2

---

## B15-router-029-004 — CurrencyConversionService: IMPLEMENT+ARCHITECTURE routing

A: Predicates: ["IMPLEMENT", "ARCHITECTURE"] from "Implement CurrencyConversionService + currency domain model."
B: Routes: ArchitectureFlow31 (3.1) → ImplementationFlow41 (4.1) | DecisionCount: 2

---

## B15-router-029-005 — CurrencyConversionService: IMPLEMENT+API_CONTRACT+SECURITY three-route ordering

A: Predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY"] from "Implement + wire up forex API + encrypt credentials."
B: Routes: SecurityAuditFlow → APIContractFlow33 (3.3) → ImplementationFlow41 (4.1) | DecisionCount: 3
Ordering rule: SECURITY before API_CONTRACT before IMPLEMENT.
