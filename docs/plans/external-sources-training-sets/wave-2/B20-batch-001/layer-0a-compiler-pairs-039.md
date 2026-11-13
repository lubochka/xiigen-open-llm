# Layer 0-A — Compiler Pairs (039) — GraphTraversal: TEST fires on test-infrastructure vocabulary
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: compiler

---

## P039-001 — TEST: "test cases" compound fires TEST for GraphTraversal

A: "Write test cases covering BFS (Breadth-First Search) and DFS (Depth-First Search) traversal order in the GraphTraversal class."
  "test cases" → TEST (compound). "write" → IMPLEMENT.
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition | SubPhaseId: 5.1 + 4.1

---

## P039-002 — TEST: "assert" fires TEST on traversal order

A: "Assert that the GraphTraversal visits nodes in BFS (Breadth-First Search) order starting from the root."
  "assert" → TEST. BFS / DFS are domain algorithm names; neither fires any predicate.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P039-003 — GAP: "traverse the graph" fires zero predicates

A: "Traverse the graph using depth-first order and collect visited nodes."
  "traverse" → domain operation verb; not in any predicate lexicon.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## P039-004 — TEST+IMPLEMENT: "create and validate GraphTraversal"

A: "Create the GraphTraversal class and validate that DFS (Depth-First Search) detects cycles correctly."
  "create" → IMPLEMENT | "validate" → TEST
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition | SubPhaseId: 5.1 + 4.1

---

## P039-005 — GAP: "apply traversal" fires zero predicates

A: "Apply the graph traversal algorithm to the dependency tree."
  "apply" (standalone) → domain operation verb; not in IMPLEMENT lexicon.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
