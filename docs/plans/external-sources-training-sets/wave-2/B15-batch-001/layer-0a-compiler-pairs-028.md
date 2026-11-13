# Layer 0-A — Compiler Pairs (028)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: compiler

---

## B15-compiler-028-001 — TaxCalculationService: IMPLEMENT alone

A: "Build a TaxCalculationService that computes sales tax by jurisdiction."
B: Predicates: ["IMPLEMENT"] | feature_addition | DecisionCount: 1
"build" → IMPLEMENT. "jurisdiction" is a domain noun, not a predicate trigger.

---

## B15-compiler-028-002 — TaxCalculationService: IMPLEMENT+ARCHITECTURE

A: "Build a TaxCalculationService and architect the tax jurisdiction model."
B: Predicates: ["IMPLEMENT", "ARCHITECTURE"] | feature_addition | DecisionCount: 2
"build" → IMPLEMENT. "architect" verb → ARCHITECTURE. "jurisdiction model" also → ARCHITECTURE (deduplicated to one ARCHITECTURE predicate).

---

## B15-compiler-028-003 — TaxCalculationService: IMPLEMENT+API_CONTRACT

A: "Build a TaxCalculationService and define the tax service contracts."
B: Predicates: ["IMPLEMENT", "API_CONTRACT"] | feature_addition | DecisionCount: 2
"build" → IMPLEMENT. "service contracts" → API_CONTRACT.

---

## B15-compiler-028-004 — TaxCalculationService: IMPLEMENT+TEST

A: "Build a TaxCalculationService and write tests for the rounding logic."
B: Predicates: ["IMPLEMENT", "TEST"] | feature_addition | DecisionCount: 2
"build" → IMPLEMENT. "write tests" → TEST (unit/integration → sub-phase 5.1).

---

## B15-compiler-028-005 — "tax model" fires ARCHITECTURE

A: "Build a TaxCalculationService and define the tax model."
B: Predicates: ["IMPLEMENT", "ARCHITECTURE"] | feature_addition | DecisionCount: 2
"tax model" → ARCHITECTURE fires (domain noun + model phrase fires ARCHITECTURE). Teaching: "model" paired with any domain noun fires ARCHITECTURE.
