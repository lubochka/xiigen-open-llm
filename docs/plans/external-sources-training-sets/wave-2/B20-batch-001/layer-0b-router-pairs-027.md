# Layer 0-B — Router Pairs (027) — AmortizationSchedule routing: IMPLEMENT alone vs TEST+IMPLEMENT
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: router

---

## R027-001 — IMPLEMENT alone: "generate" routes to 4.1 only

A:
  compiled_predicate_ids: ["IMPLEMENT"]
  compiled_text: "Generate the amortization schedule for a 30-year mortgage at 4.5% interest."

B:
  RoutingDecisions:
    - predicate: IMPLEMENT → sub_phase: 4.1
  TransitionType: feature_addition
  PrimarySubPhaseId: 4.1
  Note: "generate" fires IMPLEMENT; no TEST predicate present. Routes to 4.1 only.

---

## R027-002 — TEST alone: "validate" routes to 5.1

A:
  compiled_predicate_ids: ["TEST"]
  compiled_text: "Validate that the AmortizationSchedule handles early payoff scenarios correctly."

B:
  RoutingDecisions:
    - predicate: TEST → sub_phase: 5.1
  TransitionType: documentation
  PrimarySubPhaseId: 5.1

---

## R027-003 — TEST+IMPLEMENT: "develop and write tests" routes to both

A:
  compiled_predicate_ids: ["TEST", "IMPLEMENT"]
  compiled_text: "Develop the AmortizationSchedule service and write unit tests covering principal reduction and interest payments."

B:
  RoutingDecisions:
    - predicate: IMPLEMENT → sub_phase: 4.1
    - predicate: TEST → sub_phase: 5.1
  TransitionType: feature_addition
  AllSubPhaseIds: [4.1, 5.1]
