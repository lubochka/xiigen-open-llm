# Layer 0-C — Knowledge Edge Pairs (014)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E014-001 — GAP: "confirm inventory allocation" does not fire TEST

A:
  user_request: "Confirm that inventory allocation reserves the right quantity for multi-item orders."

B:
  CompiledPredicateIds: []
  TransitionType: unknown
  SourceStatus: knowledge_edge
  Predicate evidence:
    "confirm" — NOT in TEST lexicon; informal validation language; gap
    "reserves the right quantity" — domain phrase; gap
    Result: zero predicates
  Correction: "Assert that inventory allocation reserves the right quantity..." → TEST fires on "assert".

---

## E014-002 — EDGE: "ensure" does not fire TEST

A:
  user_request: "Ensure the InventoryAllocationService does not over-allocate stock when concurrent requests arrive."

B:
  CompiledPredicateIds: []
  TransitionType: unknown
  SourceStatus: knowledge_edge
  Predicate evidence:
    "ensure" — NOT in TEST lexicon; imperative requirement language; gap
    "does not over-allocate" — domain correctness requirement; gap
    Result: zero predicates
  Teaching: "ensure", "confirm", "check", "make sure" are all informal correctness language.
  None fire TEST. The TEST predicate requires: test, verify, assert, validate, spec, mock, stub, fixture, scenario, regression (with test), harness, coverage (with test).
