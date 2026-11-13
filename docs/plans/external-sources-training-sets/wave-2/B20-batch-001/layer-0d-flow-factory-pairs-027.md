# Layer 0-D — Flow Factory Pairs (027) — AmortizationSchedule: IMPLEMENT-only factory output
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F027-001 — IMPLEMENT alone: factory produces BackendServiceFlow41 only

Request: "Generate the amortization schedule for a 30-year mortgage at 4.5% interest."
  CompiledPredicateIds: ["IMPLEMENT"] (via "generate")
  FlowFactory.CreateMulti(["IMPLEMENT"]) → [BackendServiceFlow41]
  One flow instance (4.1 only). No UnitTestingFlow51 — TEST was not fired.
  TransitionType: feature_addition

---

## F027-002 — TEST alone: factory produces UnitTestingFlow51 only

Request: "Validate that the AmortizationSchedule handles early payoff scenarios correctly."
  FlowFactory.CreateMulti(["TEST"]) → [UnitTestingFlow51]
  TransitionType: documentation.

---

## F027-003 — TEST+IMPLEMENT: factory produces both flows, execution order 4.1 → 5.1

Request: "Develop the AmortizationSchedule service and write unit tests covering principal reduction."
  FlowFactory.CreateMulti(["TEST", "IMPLEMENT"]) → [BackendServiceFlow41, UnitTestingFlow51]
  Execution order: BackendServiceFlow41 (4.1) first, UnitTestingFlow51 (5.1) second.
  Rationale: implementation must exist before tests can be written against it.
  TransitionType: feature_addition
