# Layer 0-A — Compiler Pairs (032) — MarginCalculator: TEST with gross/net margin domain
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: compiler

---

## P032-001 — TEST+IMPLEMENT: "create MarginCalculator with test cases"

A: "Create the MarginCalculator service and add test cases for gross margin and net margin calculations."
  "create" → IMPLEMENT | "add" → IMPLEMENT (deduplicates) | "test cases" → TEST
  "gross margin", "net margin" — domain terms; no predicate.
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition | SubPhaseId: 5.1 + 4.1

---

## P032-002 — TEST: "validate margin percentage" fires TEST

A: "Validate that the MarginCalculator returns 40% gross margin for a product with £60 cost and £100 selling price."
  "validate" → TEST. Domain figures (40%, £60, £100) do not affect predicate.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P032-003 — GAP: "trigger" is execution verb

A: "Trigger the MarginCalculator for all products in the quarterly report."
  "trigger" — execution verb; zero predicates. "quarterly report" — domain context; no predicate.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## P032-004 — TEST: "stub" in test-adjacent context fires TEST

A: "Stub the product cost provider and write tests for the MarginCalculator against synthetic pricing data."
  "stub" → TEST (test-adjacent) | "write" → IMPLEMENT | "tests" → TEST (deduplicates)
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition

---

## P032-005 — Boundary: TransitionType documentation when TEST is only predicate via "validate"

A: "Validate, assert, and verify that the MarginCalculator handles a 0% margin without error."
  "validate" → TEST | "assert" → TEST (deduplicates) | "verify" → TEST (deduplicates)
  Three test verbs all deduplicate into one TEST predicate. No IMPLEMENT verb present.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation — TEST alone → documentation.
