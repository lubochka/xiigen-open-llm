# Layer 0-A — Compiler Pairs (034) — CommissionCalculator: TEST with tiered commission domain
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: compiler

---

## P034-001 — TEST+IMPLEMENT: "develop and write tests" for CommissionCalculator

A: "Develop the CommissionCalculator and write unit tests for base commission, tiered commission, and cap enforcement."
  "develop" → IMPLEMENT | "write" → IMPLEMENT (deduplicates) | "unit tests" → TEST
  "base commission", "tiered commission", "cap enforcement" — domain phrases; no predicate.
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition | SubPhaseId: 5.1 + 4.1

---

## P034-002 — TEST: "assert" fires on commission tier boundary

A: "Assert that the CommissionCalculator applies the 10% tier for sales between £5,000 and £10,000."
  "assert" → TEST. Commission tier boundaries are domain detail; predicate fires from "assert".
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P034-003 — GAP: "accrue" is domain verb — zero predicates

A: "Accrue commissions for all Q4 (Quarter 4) sales representatives using the tiered model."
  "accrue" — domain financial verb; NOT in any predicate lexicon. Zero predicates.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## P034-004 — TEST: "regression tests" compound fires TEST

A: "Create regression tests to ensure the CommissionCalculator cap enforcement has not changed after the tier restructure."
  "create" → IMPLEMENT | "regression tests" → TEST
  "ensure" — informal QA; does NOT contribute additional predicates.
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition

---

## P034-005 — Boundary: "commission", "tier", "cap" — domain nouns with zero predicates

A: "Calculate the commission for a rep who sold £12,000 in Q4 with a £1,500 cap."
  "calculate" — domain verb; no predicate. All financial terms are domain vocabulary only.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
