# Layer 0-B — Router Pairs (030)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: router

---

## B15-router-030-001 — ExchangeRateService: IMPLEMENT single route

A: Predicates: ["IMPLEMENT"] from "Create ExchangeRateService."
B: Route: ImplementationFlow41 (sub-phase 4.1) | DecisionCount: 1

---

## B15-router-030-002 — ExchangeRateService: IMPLEMENT+API_CONTRACT

A: Predicates: ["IMPLEMENT", "API_CONTRACT"] from "Create ExchangeRateService + connect to OpenExchangeRates."
B: Routes: APIContractFlow33 (3.3) → ImplementationFlow41 (4.1) | DecisionCount: 2

---

## B15-router-030-003 — ExchangeRateService: IMPLEMENT+DOCUMENT

A: Predicates: ["IMPLEMENT", "DOCUMENT"] from "Create ExchangeRateService + document caching strategy."
B: Routes: ImplementationFlow41 (4.1) ‖ DocumentationFlow21 (2.1) | DOCUMENT parallel with IMPLEMENT | DecisionCount: 2

---

## B15-router-030-004 — ExchangeRateService: refactoring — route unchanged

A: Predicates: ["IMPLEMENT"] | TransitionType: refactoring from "Refactor ExchangeRateService."
B: Route: ImplementationFlow41 (4.1) | TransitionType: refactoring | DecisionCount: 1
Route does not change for refactoring — same ImplementationFlow41 is used; TransitionType metadata controls flow behavior.

---

## B15-router-030-005 — NearestWrongSubPhase scoring for 4.1

NearestWrongSubPhase for ImplementationFlow41: sub-phase 4.2 (Integration) | Score: 0.40.
NearestWrongSubPhase for APIContractFlow33: sub-phase 3.1 (Architecture) | Score: 0.36.
Router confidence for 4.1: typically 0.91–0.95. Score above 0.89 threshold = correct routing.
