# Layer 0-A — Compiler Pairs (026) — CompoundInterestAlgorithm: compounding frequency and TEST boundaries
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: compiler

---

## P026-001 — TEST+IMPLEMENT: "build and spec" compound interest

A: "Build the CompoundInterestAlgorithm and spec out the test cases for annual, monthly, and daily compounding frequencies."
  "build" → IMPLEMENT | "spec" → TEST | "test cases" → TEST (deduplicates)
  "compounding frequencies" — domain phrase; no predicate.
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition | SubPhaseId: 5.1 + 4.1

---

## P026-002 — TEST: "verify" fires on compound interest precision

A: "Verify that the CompoundInterestAlgorithm produces the correct result for monthly compounding with a £5,000 principal."
  "verify" → TEST. Fires regardless of domain (compound interest, billing, or any other).
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P026-003 — GAP: "run the compound interest algorithm" — zero predicates

A: "Run the CompoundInterestAlgorithm with a 6% annual rate compounded monthly."
  "run" — execution verb; zero predicates. Domain nouns ("compound interest", "annual rate") do not elevate "run".
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## P026-004 — TEST: "write regression tests" for compounding

A: "Write regression tests for the CompoundInterestAlgorithm to guard against floating-point drift."
  "write" → IMPLEMENT | "regression tests" → TEST (compound: "regression" + "tests")
  "floating-point drift" — domain term; no predicate.
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition

---

## P026-005 — Deduplication: multiple TEST triggers → one TEST predicate

A: "Create unit tests, write test cases, and assert the CompoundInterestAlgorithm handles zero principal."
  "unit tests" → TEST | "test cases" → TEST (deduplicates) | "assert" → TEST (deduplicates) | "create" → IMPLEMENT | "write" → IMPLEMENT (deduplicates)
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] — deduplication produces exactly one TEST, one IMPLEMENT.
  TransitionType: feature_addition
