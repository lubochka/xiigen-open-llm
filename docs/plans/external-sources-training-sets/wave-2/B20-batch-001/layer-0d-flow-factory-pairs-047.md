# Layer 0-D — Flow Factory Pairs (047) — BloomFilter: factory selection for probabilistic domain
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F047-001 — TEST+IMPLEMENT: "develop BloomFilter and add test cases" → two instances

Request: "Develop the BloomFilter class and add test cases for false-positive rate verification."
  FlowFactory.CreateMulti(["TEST", "IMPLEMENT"]) → [BackendServiceFlow41, UnitTestingFlow51]
  TransitionType: feature_addition.

---

## F047-002 — TEST alone: "assert" on BloomFilter definitely-absent → one UnitTestingFlow51, documentation

Request: "Assert that the BloomFilter never produces a false-negative for elements that were inserted."
  FlowFactory.CreateMulti(["TEST"]) → [UnitTestingFlow51]
  TransitionType: documentation.

---

## F047-003 — Zero predicate: "calculate false-positive rate" → empty factory

Request: "Calculate the expected false-positive rate for a BloomFilter with 1000 elements."
  FlowFactory.CreateMulti([]) → []
  TransitionType: unknown | SourceStatus: knowledge_edge.
