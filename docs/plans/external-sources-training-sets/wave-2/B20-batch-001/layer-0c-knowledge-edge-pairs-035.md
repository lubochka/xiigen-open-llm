# Layer 0-C — Knowledge Edge Pairs (035) — BonusCalculator: "pay" and HR domain gaps
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E035-001 — Gap: "pay" → knowledge_edge (HR domain verb)

A: "Pay bonuses to all employees who exceeded their targets by more than 15%."
B:
  CompiledPredicateIds: []
  GapVocabulary: ["pay bonuses", "exceeded their targets", "15%"]
  Note: "pay" is an HR (Human Resources) payroll domain verb. No predicate mapping.
  HR domain verb gap class: pay, compensate, disburse, process payroll, award, grant, distribute.

---

## E035-002 — Gap: "evaluate" → knowledge_edge (assessment domain verb)

A: "Evaluate performance reviews and apply the bonus formula for all eligible employees."
B:
  CompiledPredicateIds: []
  GapVocabulary: ["evaluate", "performance reviews", "apply the bonus formula"]
  Note: "evaluate" is an assessment domain verb; NOT in any predicate lexicon.
  "apply" is also not in IMPLEMENT lexicon. Both contribute zero predicates.

---

## E035-003 — HR/bonus domain vocabulary — all zero-predicate

  "performance threshold", "target attainment", "bonus pool", "discretionary bonus",
  "retention bonus", "sign-on bonus", "vesting period", "performance review",
  "eligibility criteria", "HR (Human Resources)", "payroll cycle"
  None of these fire any predicate. Predicate lexicons are action-verb-driven, not domain-noun-driven.
