# Layer 0-A — Compiler Pairs (030)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: compiler

---

## B15-compiler-030-001 — ExchangeRateService: IMPLEMENT alone

A: "Create an ExchangeRateService that fetches live exchange rates."
B: Predicates: ["IMPLEMENT"] | feature_addition | DecisionCount: 1
"create" → IMPLEMENT. "exchange rates" is domain vocabulary, not a predicate trigger.

---

## B15-compiler-030-002 — ExchangeRateService: IMPLEMENT+API_CONTRACT

A: "Create an ExchangeRateService and connect to the OpenExchangeRates API."
B: Predicates: ["IMPLEMENT", "API_CONTRACT"] | feature_addition | DecisionCount: 2
"create" → IMPLEMENT. "connect" (external system) → API_CONTRACT.

---

## B15-compiler-030-003 — ExchangeRateService: IMPLEMENT+DOCUMENT

A: "Create an ExchangeRateService and document the rate-caching strategy."
B: Predicates: ["IMPLEMENT", "DOCUMENT"] | feature_addition | DecisionCount: 2
"create" → IMPLEMENT. "document" → DOCUMENT.

---

## B15-compiler-030-004 — ExchangeRateService: refactoring

A: "Refactor the ExchangeRateService to use a distributed cache instead of in-memory."
B: Predicates: ["IMPLEMENT"] | refactoring | DecisionCount: 1
"refactor" → IMPLEMENT. TransitionType: refactoring (overrides feature_addition).

---

## B15-compiler-030-005 — "extend" fires IMPLEMENT as feature_addition

A: "Extend the ExchangeRateService to support historical rate lookups."
B: Predicates: ["IMPLEMENT"] | feature_addition | DecisionCount: 1
"extend" → IMPLEMENT fires. TransitionType: feature_addition (not refactoring — "extend" does not override to refactoring).
