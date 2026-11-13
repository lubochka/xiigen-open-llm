# Layer 0-C — Knowledge Edge Pairs (031) — BreakEvenCalculator: "make sure" and cost-model gaps
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E031-001 — Gap: "make sure" → knowledge_edge (informal QA family)

A: "Make sure the BreakEvenCalculator handles zero contribution margin without dividing by zero."
B:
  CompiledPredicateIds: []
  GapVocabulary: ["make sure", "zero contribution margin", "dividing by zero"]
  Note: "make sure" is informal QA language. Same gap family as "check", "confirm", "ensure".
  All four informal QA phrases (check, confirm, ensure, make sure) produce zero predicates.
  SCHEDULE_CAPABILITY_LEARNING: true

---

## E031-002 — Gap: break-even cost domain vocabulary — all zero-predicate

  "contribution margin", "fixed costs", "variable costs", "break-even point",
  "break-even volume", "break-even revenue", "operating leverage",
  "cost structure", "overhead allocation"
  None of these domain terms fire any predicate.

---

## E031-003 — Complete informal QA gap reference (four phrases, one gap family)

All four informal QA verbs/phrases that produce zero predicates:
  1. "check" — e.g. "Check that the BreakEvenCalculator handles zero margin" → []
  2. "confirm" — e.g. "Confirm the break-even point is 500 units" → []
  3. "ensure" — e.g. "Ensure the BreakEvenCalculator has no division errors" → []
  4. "make sure" — e.g. "Make sure the result is positive" → []
These four are canonical informal QA vocabulary. None are in the TEST predicate lexicon.
