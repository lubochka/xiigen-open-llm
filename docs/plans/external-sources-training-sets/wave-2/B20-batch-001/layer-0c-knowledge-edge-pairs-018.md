# Layer 0-C — Knowledge Edge Pairs (018)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E018-001 — GAP: "check loyalty points balance" fires nothing

A:
  user_request: "Check that a customer's loyalty points balance is correct after a £100 purchase."

B:
  CompiledPredicateIds: []
  TransitionType: unknown
  SourceStatus: knowledge_edge
  Predicate evidence:
    "check" — NOT TEST; gap
    "is correct" — informal correctness; gap
    "£100 purchase" — domain value; gap
    Result: zero predicates

---

## E018-002 — EDGE: "QA loyalty points accrual" fires QA, not TEST

A:
  user_request: "QA the loyalty points accrual flow for gold-tier customers."

B:
  CompiledPredicateIds: ["QA"]
  TransitionType: feature_addition
  PhaseId: 7
  SubPhaseId: 7.3
  SourceStatus: ready
  Predicate evidence:
    "QA" — QA predicate fires on the literal abbreviation "QA"
    "loyalty points accrual" — domain phrase; no predicate
    "gold-tier customers" — domain classification; no predicate
    "test" does NOT appear — TEST does not fire
    Result: ["QA"] only; TEST is absent because "test" is absent from the request
  Teaching: "QA" and "TEST" are distinct predicates. QA routes to 7.3; TEST routes to 5.1.
  The absence of "test" from "QA the loyalty flow" means TEST is never fired.
