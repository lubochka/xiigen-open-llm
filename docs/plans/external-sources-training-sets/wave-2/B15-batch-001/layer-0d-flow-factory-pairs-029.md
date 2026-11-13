# Layer 0-D — Flow Factory Selection Pairs (029)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B15-factory-029-001 — CurrencyConversionService: IMPLEMENT factory call

A: Predicate: IMPLEMENT | Sub-phase: 4.1
B: Factory selects: universal-nine-node-development-flow-4.1 | deterministic_generic

---

## B15-factory-029-002 — CurrencyConversionService: three-predicate factory calls

A: Predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
B: Three factory calls:
- SECURITY → universal-security-audit-flow
- API_CONTRACT → universal-nine-node-contract-flow-3.3
- IMPLEMENT → universal-nine-node-development-flow-4.1

---

## B15-factory-029-003 — SECURITY factory call always selects universal-security-audit-flow

Predicate: SECURITY always → universal-security-audit-flow regardless of domain.
There is no service-specific security flow for CurrencyConversionService or any Round 2 service.
TrustState for security: deterministic_generic.

---

## B15-factory-029-004 — API_CONTRACT factory call always selects 3.3 flow

Predicate: API_CONTRACT always → universal-nine-node-contract-flow-3.3 in TrustState: deterministic_generic.
No service-specific contract flow registered for Round 2 services.

---

## B15-factory-029-005 — ARCHITECTURE factory call always selects 3.1 flow

Predicate: ARCHITECTURE always → universal-nine-node-architecture-flow-3.1 in TrustState: deterministic_generic.
No service-specific architecture flow registered for Round 2 services.
