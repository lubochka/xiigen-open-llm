# Layer 0-A — Compiler Pairs (041) — DijkstraPath: TEST fires on "assert", not on path-domain terms
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: compiler

---

## P041-001 — TEST: "assert" fires TEST for Dijkstra shortest-path result

A: "Assert that the DijkstraPath algorithm returns the correct shortest path weight between two nodes."
  "assert" → TEST. Dijkstra is a domain algorithm name; it does not fire any predicate.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P041-002 — TEST+IMPLEMENT: "develop and write tests" for DijkstraPath

A: "Develop the DijkstraPath class and write tests for graphs with disconnected components."
  "develop" → IMPLEMENT | "write" → IMPLEMENT (deduplicates) | "tests" → TEST
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition | SubPhaseId: 5.1 + 4.1

---

## P041-003 — GAP: "compute shortest path" fires zero predicates

A: "Compute the shortest path between node A and node B using Dijkstra's algorithm."
  "compute" → domain computation verb; not in any predicate lexicon.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## P041-004 — TEST: "validate" fires TEST for negative-weight edge detection

A: "Validate that the DijkstraPath implementation raises an error for negative-weight edges."
  "validate" → TEST universally.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P041-005 — GAP: "run path-finding" fires zero predicates

A: "Run the path-finding algorithm on the network topology graph."
  "run" → execution verb; zero predicates.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
