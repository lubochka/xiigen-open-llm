# Layer 0-A — Compiler Pairs (031) — BreakEvenCalculator: TEST firing and domain boundary
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: compiler

---

## P031-001 — TEST+IMPLEMENT: "extend and write tests" for BreakEvenCalculator

A: "Extend the BreakEvenCalculator to handle variable cost curves and write unit tests for each cost model."
  "extend" → IMPLEMENT | "write" → IMPLEMENT (deduplicates) | "unit tests" → TEST
  "variable cost curves", "cost model" — domain phrases; no predicate.
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition | SubPhaseId: 5.1 + 4.1

---

## P031-002 — TEST: "assert" on break-even volume

A: "Assert that the BreakEvenCalculator returns 500 units for fixed costs of £5,000 and a contribution margin of £10."
  "assert" → TEST. "fixed costs", "contribution margin" — domain terms; no predicate.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P031-003 — GAP: "make sure" does NOT fire TEST

A: "Make sure the BreakEvenCalculator handles zero contribution margin without dividing by zero."
  "make sure" — informal QA language; NOT in TEST predicate lexicon.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## P031-004 — TEST: "scenario" in test context fires TEST

A: "Define a scenario where the BreakEvenCalculator receives a negative fixed cost and verify the result."
  "scenario" → TEST (test-context compound) | "verify" → TEST (deduplicates)
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P031-005 — Boundary: "contribution margin", "fixed cost", "variable cost" — none fire predicates

A: "Calculate the break-even point where fixed costs are £20,000 and contribution margin per unit is £8."
  "calculate" — domain verb; no predicate. All financial terms are domain vocabulary.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
