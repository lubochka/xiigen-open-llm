# Layer 0-B — Router Pairs (030) — IRRCalculator routing: test-driven and mock routing
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: router

---

## R030-001 — TEST+IMPLEMENT: "implement and spec" IRR routes to 4.1 + 5.1

A:
  compiled_predicate_ids: ["TEST", "IMPLEMENT"]
  compiled_text: "Implement the IRRCalculator using Newton-Raphson iteration and spec the unit tests for convergence cases."

B:
  RoutingDecisions:
    - predicate: IMPLEMENT → sub_phase: 4.1
    - predicate: TEST → sub_phase: 5.1
  TransitionType: feature_addition
  AllSubPhaseIds: [4.1, 5.1]

---

## R030-002 — TEST+IMPLEMENT: "test-driven develop" routes to both sub-phases

A:
  compiled_predicate_ids: ["TEST", "IMPLEMENT"]
  compiled_text: "Use a test-driven approach to develop the IRRCalculator iteration logic."

B:
  RoutingDecisions:
    - predicate: TEST → sub_phase: 5.1
    - predicate: IMPLEMENT → sub_phase: 4.1
  TransitionType: feature_addition
  Note: "test-driven" fires TEST; "develop" fires IMPLEMENT. Both route independently.

---

## R030-003 — TEST alone: "mock" and "assert" route to 5.1

A:
  compiled_predicate_ids: ["TEST"]
  compiled_text: "Mock the cash flow data provider and assert the IRRCalculator handles a non-convergent series by returning an error."

B:
  RoutingDecisions:
    - predicate: TEST → sub_phase: 5.1
  TransitionType: documentation
  Note: "mock" and "assert" both fire TEST, deduplicated to one decision. No IMPLEMENT → documentation.
