# Layer 0-D — Flow Factory Pairs (040) — TreeTraversal: factory with "test harness" → documentation
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F040-001 — TEST alone: "test harness" for TreeTraversal → one UnitTestingFlow51, documentation

Request: "Set up a test harness for the TreeTraversal class covering null root and single-node trees."
  "test harness" → TEST (compound). "set up" → context verb; does not fire IMPLEMENT.
  FlowFactory.CreateMulti(["TEST"]) → [UnitTestingFlow51]
  TransitionType: documentation.

---

## F040-002 — TEST+IMPLEMENT: "implement TreeTraversal and add test suite" → two instances

Request: "Implement the TreeTraversal class and add a test suite covering in-order, pre-order, and post-order."
  FlowFactory.CreateMulti(["TEST", "IMPLEMENT"]) → [BackendServiceFlow41, UnitTestingFlow51]
  TransitionType: feature_addition.

---

## F040-003 — Zero predicate: "process tree nodes" → empty factory

Request: "Process the tree nodes using in-order traversal."
  FlowFactory.CreateMulti([]) → []
  TransitionType: unknown | SourceStatus: knowledge_edge.
