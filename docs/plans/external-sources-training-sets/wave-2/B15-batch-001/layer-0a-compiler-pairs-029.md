# Layer 0-A — Compiler Pairs (029)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: compiler

---

## B15-compiler-029-001 — CurrencyConversionService: IMPLEMENT alone

A: "Implement a CurrencyConversionService that converts amounts between supported currencies."
B: Predicates: ["IMPLEMENT"] | feature_addition | DecisionCount: 1
"implement" → IMPLEMENT. "currencies" is domain context; no additional predicates fire.

---

## B15-compiler-029-002 — CurrencyConversionService: IMPLEMENT+API_CONTRACT

A: "Implement a CurrencyConversionService and integrate with an external forex provider."
B: Predicates: ["IMPLEMENT", "API_CONTRACT"] | feature_addition | DecisionCount: 2
"implement" → IMPLEMENT. "integrate" → API_CONTRACT. External provider confirms external system context.

---

## B15-compiler-029-003 — CurrencyConversionService: IMPLEMENT+SECURITY

A: "Implement a CurrencyConversionService and protect the conversion rate cache."
B: Predicates: ["IMPLEMENT", "SECURITY"] | feature_addition | DecisionCount: 2
"implement" → IMPLEMENT. "protect" → SECURITY.

---

## B15-compiler-029-004 — CurrencyConversionService: IMPLEMENT+ARCHITECTURE

A: "Implement a CurrencyConversionService and define the currency domain model."
B: Predicates: ["IMPLEMENT", "ARCHITECTURE"] | feature_addition | DecisionCount: 2
"implement" → IMPLEMENT. "currency domain model" → ARCHITECTURE (domain noun + model).

---

## B15-compiler-029-005 — IMPLEMENT+API_CONTRACT+SECURITY three predicates

A: "Implement a CurrencyConversionService, wire up the forex API, and encrypt the API credentials."
B: Predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY"] | feature_addition | DecisionCount: 3
"implement" → IMPLEMENT. "wire up" → API_CONTRACT. "encrypt" → SECURITY. Three independent predicates, three decisions.
