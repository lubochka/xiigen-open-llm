# Layer 0-B — Router Pairs (031) — BreakEvenCalculator routing: "scenario" and "extend" routing
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: router

---

## R031-001 — TEST+IMPLEMENT: "extend and write tests" routes to 4.1 + 5.1

A:
  compiled_predicate_ids: ["TEST", "IMPLEMENT"]
  compiled_text: "Extend the BreakEvenCalculator to handle variable cost curves and write unit tests for each cost model."

B:
  RoutingDecisions:
    - predicate: IMPLEMENT → sub_phase: 4.1
    - predicate: TEST → sub_phase: 5.1
  TransitionType: feature_addition
  AllSubPhaseIds: [4.1, 5.1]

---

## R031-002 — TEST alone: "scenario" + "verify" routes to 5.1

A:
  compiled_predicate_ids: ["TEST"]
  compiled_text: "Define a scenario where the BreakEvenCalculator receives a negative fixed cost and verify the result."

B:
  RoutingDecisions:
    - predicate: TEST → sub_phase: 5.1
  TransitionType: documentation
  Note: "scenario" and "verify" both deduplicate to one TEST routing decision.

---

## R031-003 — ZERO predicates: "make sure" and "calculate" do not route

A:
  compiled_predicate_ids: []
  compiled_text: "Make sure the BreakEvenCalculator handles zero contribution margin without dividing by zero."

B:
  RoutingDecisions: []
  TransitionType: unknown
  SubPhaseId: null
  SourceStatus: knowledge_edge
  CapabilityGapSignal:
    gap_id: "gap-make-sure-not-test-031"
    gap_vocabulary: ["make sure", "handles correctly"]
    SCHEDULE_CAPABILITY_LEARNING: true
