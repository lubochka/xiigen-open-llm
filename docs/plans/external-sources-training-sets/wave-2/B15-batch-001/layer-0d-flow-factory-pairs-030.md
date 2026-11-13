# Layer 0-D — Flow Factory Selection Pairs (030)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B15-factory-030-001 — ExchangeRateService: IMPLEMENT factory call

A: Predicate: IMPLEMENT | Sub-phase: 4.1
B: Factory selects: universal-nine-node-development-flow-4.1 | deterministic_generic

---

## B15-factory-030-002 — ExchangeRateService: IMPLEMENT+API_CONTRACT two factory calls

A: Predicates: ["IMPLEMENT", "API_CONTRACT"]
B: Two factory calls:
- API_CONTRACT → universal-nine-node-contract-flow-3.3
- IMPLEMENT → universal-nine-node-development-flow-4.1

---

## B15-factory-030-003 — ExchangeRateService: DOCUMENT factory call

A: Predicate: DOCUMENT | Sub-phase: 2.1
B: Factory selects: universal-nine-node-documentation-flow-2.1 | deterministic_generic

---

## B15-factory-030-004 — refactoring TransitionType: factory behavior unchanged

A: Predicate: IMPLEMENT | TransitionType: refactoring
B: Factory selects: universal-nine-node-development-flow-4.1 (same as feature_addition).
TransitionType does not change flow selection. It is metadata passed to the flow after selection.

---

## B15-factory-030-005 — "extend" verb: factory produces feature_addition metadata

A: Predicate: IMPLEMENT | TransitionType: feature_addition (from "extend" verb)
B: Factory selects: universal-nine-node-development-flow-4.1 with TransitionType: feature_addition.
"extend" does not alter flow selection. Factory behavior is identical for all IMPLEMENT verbs.
