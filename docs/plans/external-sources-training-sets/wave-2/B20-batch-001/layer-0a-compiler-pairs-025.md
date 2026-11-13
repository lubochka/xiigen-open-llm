# Layer 0-A — Compiler Pairs (025) — InterestCalculator: TEST fires on "validate", not on domain terms
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: compiler

---

## P025-001 — TEST: "validate interest calculation" fires TEST universally

A: "Validate that the InterestCalculator returns the correct simple interest for a £1,000 principal at 5% over 12 months."
  "validate" → TEST. Universal rule: "validate" fires TEST in all contexts, including business-rule validation.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SourceStatus: routable

---

## P025-002 — TEST+IMPLEMENT: "write tests and implement InterestCalculator"

A: "Write unit tests for the InterestCalculator and implement the simple interest formula."
  "write" → IMPLEMENT | "unit tests" → TEST | "implement" → IMPLEMENT (deduplicates)
  "simple interest formula" — domain phrase; does not fire any predicate.
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition | SubPhaseId: 5.1 + 4.1

---

## P025-003 — GAP: "check interest is correct" does NOT fire TEST

A: "Check that the InterestCalculator returns correct interest for edge cases."
  "check" → NOT in TEST predicate lexicon. Informal QA language; zero predicates.
  "correct interest" — domain phrase; does not elevate "check".
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## P025-004 — TEST: "assert" fires TEST on interest boundary conditions

A: "Assert that the InterestCalculator throws on a negative interest rate."
  "assert" → TEST. Fires even when the expected outcome is an exception.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P025-005 — GAP: "testable" adjective alone does NOT fire TEST

A: "Ensure the InterestCalculator is testable with dependency-injected rate providers."
  "ensure" — informal QA; zero predicates. "testable" — adjective; does NOT fire TEST.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
  Boundary: "testable" (adjective) ≠ "test" (verb/noun/gerund). Only verb/noun/gerund forms fire TEST.
