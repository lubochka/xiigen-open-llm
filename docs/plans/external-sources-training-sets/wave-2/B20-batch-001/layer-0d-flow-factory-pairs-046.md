# Layer 0-D — Flow Factory Pairs (046) — HashMapImpl: factory with "test double" → documentation
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F046-001 — TEST alone: "test double" for HashMapImpl → one UnitTestingFlow51, documentation

Request: "Use a test double to simulate hash collision scenarios in the HashMapImpl unit tests."
  "test double" → TEST (compound). "use" → context verb; does not fire predicates.
  FlowFactory.CreateMulti(["TEST"]) → [UnitTestingFlow51]
  TransitionType: documentation.

---

## F046-002 — TEST+IMPLEMENT: "add HashMapImpl and write unit tests" → two instances

Request: "Add the HashMapImpl class and write unit tests for collision resolution using chaining."
  FlowFactory.CreateMulti(["TEST", "IMPLEMENT"]) → [BackendServiceFlow41, UnitTestingFlow51]
  TransitionType: feature_addition.

---

## F046-003 — Zero predicate: "compute the hash" → empty factory

Request: "Compute the hash of the key to locate the correct bucket in the HashMap."
  FlowFactory.CreateMulti([]) → []
  TransitionType: unknown | SourceStatus: knowledge_edge.
