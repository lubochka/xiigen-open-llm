# Layer 0-C — Knowledge Edge Pairs (017)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E017-001 — GAP: "check the shipping rates" fires nothing

A:
  user_request: "Check the shipping rates returned by ShippingCostCalculator for a 2 kg parcel to Germany."

B:
  CompiledPredicateIds: []
  TransitionType: unknown
  SourceStatus: knowledge_edge
  Predicate evidence:
    "check" — NOT TEST; informal inspection; gap
    "returned by" — domain phrase; gap
    "2 kg parcel" — domain measurement; gap
    Result: zero predicates
  Correction: "Verify that the ShippingCostCalculator returns the correct rate for a 2 kg parcel to Germany." → TEST.

---

## E017-002 — EDGE: "run unit tests" — the canonical "run ≠ TEST" boundary case for ShippingCost

A:
  user_request: "Run unit tests for the ShippingCostCalculator after the weight-tier refactor."

B:
  CompiledPredicateIds: []
  TransitionType: unknown
  SourceStatus: knowledge_edge
  Predicate evidence:
    "run" — execution verb; NOT in TEST predicate lexicon
    "unit tests" — object of "run"; does not promote "run" to TEST
    "after the weight-tier refactor" — temporal qualifier; gap
    Result: zero predicates → knowledge_edge
  Critical boundary: "run unit tests" is the most frequently confused case. "run" = execute; never = write/verify/assert/spec.
  To fire TEST: remove "run", replace with "Write unit tests for the ShippingCostCalculator..."
