# Layer 0-C — Knowledge Edge Pairs (013)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E013-001 — GAP: "check cart totals" does not fire TEST

A:
  user_request: "Check that the cart totals are computed correctly after applying discounts."

B:
  CompiledPredicateIds: []
  TransitionType: unknown
  SourceStatus: knowledge_edge
  CapabilityGapSignal:
    gap_id: "gap-check-cart-013-001"
    gap_vocabulary: ["check that", "computed correctly"]
  Predicate evidence:
    "check" — NOT in TEST lexicon; informal verification; gap
    "computed correctly" — informal correctness language; gap
    Result: zero predicates → knowledge_edge
  Correction prompt: Replace "check that" with "verify that" or "assert that" to fire TEST.

---

## E013-002 — EDGE: "make sure" does not fire TEST

A:
  user_request: "Make sure the CartCalculationService handles items with zero quantity without throwing exceptions."

B:
  CompiledPredicateIds: []
  TransitionType: unknown
  SourceStatus: knowledge_edge
  Predicate evidence:
    "make sure" — informal assurance language; NOT in TEST lexicon
    "throwing exceptions" — domain outcome description; gap
    Result: zero predicates
  Correction prompt: "Verify that the CartCalculationService handles items with zero quantity without throwing exceptions."
  → "verify" fires TEST → CompiledPredicateIds: ["TEST"]
