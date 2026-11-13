# Layer 0-D — Flow Factory Pairs (041) — DijkstraPath: factory selection for path-finding domain
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F041-001 — TEST+IMPLEMENT: "develop DijkstraPath and write tests" → two instances

Request: "Develop the DijkstraPath class and write tests for graphs with disconnected components."
  FlowFactory.CreateMulti(["TEST", "IMPLEMENT"]) → [BackendServiceFlow41, UnitTestingFlow51]
  TransitionType: feature_addition.

---

## F041-002 — TEST alone: "validate" on DijkstraPath → one UnitTestingFlow51, documentation

Request: "Validate that the DijkstraPath handles negative-weight edges by raising an error."
  FlowFactory.CreateMulti(["TEST"]) → [UnitTestingFlow51]
  TransitionType: documentation.

---

## F041-003 — Zero predicate: "compute shortest path" → empty factory

Request: "Compute the shortest path between node A and node B using Dijkstra's algorithm."
  FlowFactory.CreateMulti([]) → []
  TransitionType: unknown | SourceStatus: knowledge_edge.
