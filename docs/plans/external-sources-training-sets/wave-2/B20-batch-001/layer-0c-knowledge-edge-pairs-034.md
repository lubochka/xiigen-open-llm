# Layer 0-C — Knowledge Edge Pairs (034) — CommissionCalculator: "accrue" and commission domain gaps
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E034-001 — Gap: "accrue" → knowledge_edge (financial domain verb)

A: "Accrue commissions for all Q4 sales representatives using the tiered model."
B:
  CompiledPredicateIds: []
  GapVocabulary: ["accrue commissions", "Q4 (Quarter 4)", "tiered model"]
  Note: "accrue" is a financial domain verb with no predicate mapping.
  Financial domain verb gap class: accrue, amortise, depreciate, capitalise, defer, recognise (revenue).
  SCHEDULE_CAPABILITY_LEARNING: true

---

## E034-002 — Gap: commission domain vocabulary — all zero-predicate

  "commission rate", "tiered commission", "base commission", "cap", "clawback",
  "commission structure", "payout", "accelerator", "quota attainment",
  "sales representative", "territory", "Q4 (Quarter 4)", "annual plan"
  None of these fire any predicate.

---

## E034-003 — "ensure" with "regression tests" — compound noun saves the predicate

A: "Create regression tests to ensure the CommissionCalculator cap enforcement has not changed."
  "create" → IMPLEMENT. "regression tests" → TEST (compound noun in TEST lexicon).
  "ensure" — informal QA; does NOT contribute a second predicate. It is context noise.
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] — "ensure" is ignored; compound noun fires TEST.
  Teaching: TEST-firing compound nouns are scanned independently of the main verb.
  If any TEST compound noun appears in the sentence, TEST fires — even when the main verb is "ensure".
