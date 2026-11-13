# Layer 0-C — Knowledge Edge Pairs (015)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E015-001 — GAP: "look at discount logic" fires nothing

A:
  user_request: "Look at the DiscountEngine logic to see if it handles stacked discounts properly."

B:
  CompiledPredicateIds: []
  TransitionType: unknown
  SourceStatus: knowledge_edge
  Predicate evidence:
    "look at" — reading/inspection verb; no predicate
    "to see if" — informal investigation; no predicate
    "handles properly" — informal correctness; gap
    Result: zero predicates
  Correction: "Verify that the DiscountEngine handles stacked discounts correctly." → TEST fires.

---

## E015-002 — EDGE: "discount" keyword does not fire any predicate

A:
  user_request: "The DiscountEngine should reject codes used more than once."

B:
  CompiledPredicateIds: []
  TransitionType: unknown
  SourceStatus: knowledge_edge
  Predicate evidence:
    "should" — requirement modal; no predicate
    "reject" — domain verb; no predicate
    "codes used more than once" — domain rule; gap
    Result: zero predicates → knowledge_edge
  Teaching: Requirements stated as "X should Y" have no predicate. Only action vocabulary fires predicates.
  To fire TEST: "Assert that the DiscountEngine rejects codes used more than once."
