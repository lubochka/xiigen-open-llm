# Layer 0-D — Flow Factory Pairs (058) — ProbabilityEngine: factory selection for probability domain
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F058-001 — TEST+IMPLEMENT: "develop ProbabilityEngine and add test cases" → two instances

Request: "Develop the ProbabilityEngine and add test cases for conditional probability and Bayes theorem."
  FlowFactory.CreateMulti(["TEST", "IMPLEMENT"]) → [BackendServiceFlow41, UnitTestingFlow51]
  TransitionType: feature_addition.

---

## F058-002 — TEST alone: "verify" on probability sum → one UnitTestingFlow51, documentation

Request: "Verify that all probabilities in the ProbabilityEngine distribution sum to exactly 1.0."
  FlowFactory.CreateMulti(["TEST"]) → [UnitTestingFlow51]
  TransitionType: documentation.

---

## F058-003 — Zero predicate: "calculate the probability" → empty factory

Request: "Calculate the probability of rolling a six on a fair die using the ProbabilityEngine."
  FlowFactory.CreateMulti([]) → []
  TransitionType: unknown | SourceStatus: knowledge_edge.
