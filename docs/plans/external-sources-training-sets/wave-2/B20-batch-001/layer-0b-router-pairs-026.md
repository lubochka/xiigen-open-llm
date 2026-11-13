# Layer 0-B — Router Pairs (026) — CompoundInterestAlgorithm routing: deduplication in routing
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: router

---

## R026-001 — Deduplication: three TEST triggers → one routing decision to 5.1

A:
  compiled_predicate_ids: ["TEST", "IMPLEMENT"]
  compiled_text: "Create unit tests, write test cases, and assert the CompoundInterestAlgorithm handles zero principal."

B:
  RoutingDecisions:
    - predicate: TEST → sub_phase: 5.1
    - predicate: IMPLEMENT → sub_phase: 4.1
  Note: "unit tests", "test cases", and "assert" all fired TEST but deduplicated to one routing decision.
  TransitionType: feature_addition

---

## R026-002 — TEST alone: "spec" routes to 5.1, TransitionType documentation

A:
  compiled_predicate_ids: ["TEST"]
  compiled_text: "Spec out the test cases for annual, monthly, and daily compounding frequencies."

B:
  RoutingDecisions:
    - predicate: TEST → sub_phase: 5.1
  TransitionType: documentation
  PrimarySubPhaseId: 5.1

---

## R026-003 — ZERO predicates: "run the compound interest algorithm"

A:
  compiled_predicate_ids: []
  compiled_text: "Run the CompoundInterestAlgorithm with a 6% annual rate compounded monthly."

B:
  RoutingDecisions: []
  TransitionType: unknown
  SubPhaseId: null
  SourceStatus: knowledge_edge
  Note: "run" is execution. "compounded monthly" is domain context, not a predicate signal.
