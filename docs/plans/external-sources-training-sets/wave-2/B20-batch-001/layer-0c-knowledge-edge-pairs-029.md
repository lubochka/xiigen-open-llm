# Layer 0-C — Knowledge Edge Pairs (029) — NPVCalculator: "calculate" and "update" gaps
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E029-001 — Gap: "calculate NPV" → knowledge_edge

A: "Calculate the NPV for a 5-year project with a 10% discount rate."
B:
  CompiledPredicateIds: []
  GapVocabulary: ["calculate", "discount rate", "5-year project"]
  Note: "calculate" is a mathematical domain verb. It does not belong to either IMPLEMENT or TEST lexicons.
  Gap class: domain computation verbs — calculate, compute, derive, determine, process, evaluate.

---

## E029-002 — Gap: "update" is NOT in IMPLEMENT lexicon → knowledge_edge

A: "Update the NPV calculator to accept variable discount rates and irregular cash flows."
B:
  CompiledPredicateIds: []
  GapVocabulary: ["update", "variable discount rates", "irregular cash flows"]
  Note: "update" is not listed in the IMPLEMENT predicate lexicon.
  Confirmed non-IMPLEMENT verbs: update, modify, change, fix, patch, adjust, tweak, revise.
  SCHEDULE_CAPABILITY_LEARNING: true

---

## E029-003 — Gap reference: NPV domain vocabulary — all zero-predicate

  "discount rate", "cash flow", "present value", "net present value", "future value",
  "time value of money", "weighted average cost of capital", "WACC (Weighted Average Cost of Capital)",
  "terminal value", "discounted cash flow", "DCF (Discounted Cash Flow)"
  None of these domain terms fire any predicate. Action verbs are the sole predicate source.
