# Layer 0-B — Router Pairs (035) — BonusCalculator routing: "test double" and "testing" gerund
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: router

---

## R035-001 — TEST+IMPLEMENT: "add and write test scenarios" routes to 4.1 + 5.1

A:
  compiled_predicate_ids: ["TEST", "IMPLEMENT"]
  compiled_text: "Add the BonusCalculator and write test scenarios for discretionary, target-based, and retention bonuses."

B:
  RoutingDecisions:
    - predicate: IMPLEMENT → sub_phase: 4.1
    - predicate: TEST → sub_phase: 5.1
  TransitionType: feature_addition
  AllSubPhaseIds: [4.1, 5.1]

---

## R035-002 — TEST alone: "test double" + "testing" routes to 5.1

A:
  compiled_predicate_ids: ["TEST"]
  compiled_text: "Use a test double for the PerformanceMetricsRepository to isolate the BonusCalculator during testing."

B:
  RoutingDecisions:
    - predicate: TEST → sub_phase: 5.1
  TransitionType: documentation
  Note: "test double" and "testing" both fire TEST and deduplicate to one decision.

---

## R035-003 — ZERO predicates: "pay" domain verb — no routing

A:
  compiled_predicate_ids: []
  compiled_text: "Pay bonuses to all employees who exceeded their targets by more than 15%."

B:
  RoutingDecisions: []
  TransitionType: unknown
  SubPhaseId: null
  SourceStatus: knowledge_edge
  Note: "pay" is an HR domain verb. No predicate exists for payroll execution operations.
