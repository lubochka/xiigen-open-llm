# Layer 0-C — Knowledge Edge Pairs (022)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E022-001 — GAP: "price display looks correct" fires nothing

A:
  user_request: "The price display looks correct in the UK locale but not in the German locale."

B:
  CompiledPredicateIds: []
  TransitionType: unknown
  SourceStatus: knowledge_edge
  Predicate evidence:
    "looks correct" — observation/informal description; no predicate
    "UK locale" / "German locale" — domain terms; no predicate
    Result: zero predicates → knowledge_edge
  Teaching: Observation statements ("X looks correct", "X seems wrong", "X is not working") produce no predicates. They describe a state, not an action from any predicate lexicon.

---

## E022-002 — EDGE: "run unit tests" boundary — PriceFormatter

A:
  user_request: "Run unit tests for the PriceFormatter to check the German locale is now fixed."

B:
  CompiledPredicateIds: []
  TransitionType: unknown
  SourceStatus: knowledge_edge
  Predicate evidence:
    "run" — NOT TEST; execution verb
    "unit tests" — object of "run"; does not elevate "run" to TEST
    "check" — NOT TEST; informal inspection; gap
    Result: zero predicates
  Correction prompt: "Write unit tests for the PriceFormatter verifying the German locale output." → ["TEST", "IMPLEMENT"]
  Teaching: This file is the PriceFormatter-specific instance of the "run ≠ TEST" boundary teaching.
  The pattern holds regardless of domain: run + any-test-noun = zero predicates.
