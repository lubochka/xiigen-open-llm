# Layer 0-B — Router Pairs (029) — NPVCalculator routing: "build" IMPLEMENT + TEST harness
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: router

---

## R029-001 — TEST+IMPLEMENT: "build and create unit tests" routes to 4.1 + 5.1

A:
  compiled_predicate_ids: ["TEST", "IMPLEMENT"]
  compiled_text: "Build the NPVCalculator and create unit tests covering positive, negative, and zero NPV outcomes."

B:
  RoutingDecisions:
    - predicate: IMPLEMENT → sub_phase: 4.1
    - predicate: TEST → sub_phase: 5.1
  TransitionType: feature_addition
  AllSubPhaseIds: [4.1, 5.1]

---

## R029-002 — TEST+IMPLEMENT: "test harness" routes correctly

A:
  compiled_predicate_ids: ["TEST", "IMPLEMENT"]
  compiled_text: "Write a test harness for the NPVCalculator that exercises all cash flow permutations."

B:
  RoutingDecisions:
    - predicate: IMPLEMENT → sub_phase: 4.1
    - predicate: TEST → sub_phase: 5.1
  TransitionType: feature_addition
  Note: "test harness" contributes TEST; "write" contributes IMPLEMENT.

---

## R029-003 — ZERO predicates: "calculate", "update" do not route

A:
  compiled_predicate_ids: []
  compiled_text: "Calculate the NPV for a 5-year project with a 10% discount rate."

B:
  RoutingDecisions: []
  TransitionType: unknown
  SourceStatus: knowledge_edge
  Note: "calculate" is a domain verb; "update" (from P029-005) is also not in IMPLEMENT lexicon. Both are zero-predicate.
