# Layer 0-A — Compiler Pairs (035) — BonusCalculator: TEST boundary with performance metrics domain
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: compiler

---

## P035-001 — TEST+IMPLEMENT: "add BonusCalculator with test scenarios"

A: "Add the BonusCalculator and write test scenarios for discretionary bonus, target-based bonus, and retention bonus."
  "add" → IMPLEMENT | "write" → IMPLEMENT (deduplicates) | "test scenarios" → TEST (compound: "test" + "scenarios")
  Wait — "scenario" fires TEST in test context; "test scenarios" is a compound in the TEST lexicon.
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition | SubPhaseId: 5.1 + 4.1

---

## P035-002 — TEST: "verify" on BonusCalculator edge case

A: "Verify that the BonusCalculator returns £0 for an employee who did not meet the minimum performance threshold."
  "verify" → TEST. Domain context (performance threshold, £0 bonus) does not suppress.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P035-003 — GAP: "pay" is domain verb — zero predicates

A: "Pay bonuses to all employees who exceeded their targets by more than 15%."
  "pay" — domain HR (Human Resources) verb; NOT in any predicate lexicon.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## P035-004 — TEST: "test double" fires TEST

A: "Use a test double for the PerformanceMetricsRepository to isolate the BonusCalculator during testing."
  "test double" → TEST (compound noun in TEST lexicon) | "testing" → TEST (gerund, deduplicates)
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P035-005 — Boundary: "performance review", "target", "threshold" — never fire predicates

A: "Evaluate performance reviews and apply the bonus formula for all eligible employees."
  "evaluate" — domain verb; no predicate. "apply" — NOT in IMPLEMENT lexicon.
  "performance reviews", "bonus formula" — domain terms; no predicate.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
