# Layer 0-B — Router Pairs (034) — CommissionCalculator routing: "develop" + "regression tests"
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: router

---

## R034-001 — TEST+IMPLEMENT: "develop and write tests" routes to 4.1 + 5.1

A:
  compiled_predicate_ids: ["TEST", "IMPLEMENT"]
  compiled_text: "Develop the CommissionCalculator and write unit tests for base commission, tiered commission, and cap enforcement."

B:
  RoutingDecisions:
    - predicate: IMPLEMENT → sub_phase: 4.1
    - predicate: TEST → sub_phase: 5.1
  TransitionType: feature_addition
  AllSubPhaseIds: [4.1, 5.1]

---

## R034-002 — TEST+IMPLEMENT: "create regression tests" routes to both

A:
  compiled_predicate_ids: ["TEST", "IMPLEMENT"]
  compiled_text: "Create regression tests to ensure the CommissionCalculator cap enforcement has not changed after the tier restructure."

B:
  RoutingDecisions:
    - predicate: IMPLEMENT → sub_phase: 4.1
    - predicate: TEST → sub_phase: 5.1
  TransitionType: feature_addition
  Note: "create" fires IMPLEMENT; "regression tests" fires TEST.

---

## R034-003 — ZERO predicates: "accrue" domain verb — no routing

A:
  compiled_predicate_ids: []
  compiled_text: "Accrue commissions for all Q4 sales representatives using the tiered model."

B:
  RoutingDecisions: []
  TransitionType: unknown
  SubPhaseId: null
  SourceStatus: knowledge_edge
  Note: "accrue" is a financial domain verb with no predicate mapping.
