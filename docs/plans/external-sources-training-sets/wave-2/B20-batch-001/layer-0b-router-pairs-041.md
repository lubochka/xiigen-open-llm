# Layer 0-B — Router Pairs (041) — DijkstraPath: routing decisions for TEST and zero-predicate cases
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: router

---

## R041-001 — TEST alone: "assert" on DijkstraPath routes to 5.1, documentation

Request: "Assert that the DijkstraPath algorithm returns the correct shortest path weight between two nodes."
  CompiledPredicateIds: ["TEST"]
  RoutingDecisions: [TEST → 5.1] | TransitionType: documentation | SourceStatus: routable

---

## R041-002 — TEST+IMPLEMENT: "develop DijkstraPath and write tests" routes to 5.1 + 4.1

Request: "Develop the DijkstraPath class and write tests for graphs with disconnected components."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"]
  RoutingDecisions: [TEST → 5.1, IMPLEMENT → 4.1] | TransitionType: feature_addition

---

## R041-003 — Zero predicate: "compute shortest path" routes to nowhere

Request: "Compute the shortest path between node A and node B using Dijkstra's algorithm."
  "compute" → domain computation verb; zero predicates.
  CompiledPredicateIds: []
  RoutingDecisions: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## R041-004 — Routing minimal pair: "validate" (TEST → 5.1) vs "ensure" (nowhere)

A-validate: "Validate that the DijkstraPath handles negative-weight edges correctly." → [TEST → 5.1] | documentation
A-ensure: "Ensure that the DijkstraPath handles negative-weight edges correctly." → [] | unknown
Teaching: "validate" is in the TEST lexicon; "ensure" is informal QA and is not.
