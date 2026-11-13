# Layer 0-A — Compiler Pairs (033)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: compiler

---

## B15-compiler-033-001 — BillingCycleService: IMPLEMENT alone

A: "Scaffold a BillingCycleService that manages monthly and annual billing periods."
B: Predicates: ["IMPLEMENT"] | feature_addition | DecisionCount: 1
"scaffold" → IMPLEMENT. "billing periods" is domain vocabulary.

---

## B15-compiler-033-002 — BillingCycleService: IMPLEMENT+ARCHITECTURE

A: "Scaffold a BillingCycleService and define the billing cycle domain model."
B: Predicates: ["IMPLEMENT", "ARCHITECTURE"] | feature_addition | DecisionCount: 2
"scaffold" → IMPLEMENT. "billing cycle domain model" → ARCHITECTURE.

---

## B15-compiler-033-003 — BillingCycleService: IMPLEMENT+TEST

A: "Scaffold a BillingCycleService and write tests for cycle renewal logic."
B: Predicates: ["IMPLEMENT", "TEST"] | feature_addition | DecisionCount: 2
"scaffold" → IMPLEMENT. "write tests" → TEST.

---

## B15-compiler-033-004 — BillingCycleService: IMPLEMENT+DOCUMENT

A: "Scaffold a BillingCycleService and spec out the billing cycle rules."
B: Predicates: ["IMPLEMENT", "DOCUMENT"] | feature_addition | DecisionCount: 2
"scaffold" → IMPLEMENT. "spec out" → DOCUMENT.

---

## B15-compiler-033-005 — BillingCycleService: refactor+extend = refactoring

A: "Refactor and extend the BillingCycleService to add quarterly billing."
B: Predicates: ["IMPLEMENT"] | refactoring | DecisionCount: 1
"refactor" + "extend" → both fire IMPLEMENT; deduplicated to one IMPLEMENT. TransitionType: refactoring (refactor verb present overrides feature_addition).
