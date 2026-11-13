# Layer 0-A — Compiler Pairs (040) — TreeTraversal: TEST+IMPLEMENT combo patterns
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: compiler

---

## P040-001 — TEST+IMPLEMENT: "implement and add test suite" for TreeTraversal

A: "Implement the TreeTraversal class and add a test suite covering in-order, pre-order, and post-order traversal."
  "implement" → IMPLEMENT | "add" → IMPLEMENT (deduplicates) | "test suite" → TEST
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition | SubPhaseId: 5.1 + 4.1

---

## P040-002 — TEST: "verify" fires TEST for tree traversal output

A: "Verify that the TreeTraversal produces the correct in-order sequence for a balanced binary tree."
  "verify" → TEST. "balanced binary tree" — domain term; does not fire any predicate.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P040-003 — GAP: "process tree nodes" fires zero predicates

A: "Process the tree nodes using in-order traversal and store the result."
  "process" → domain computation verb; not in any predicate lexicon.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## P040-004 — TEST: "test harness" fires TEST for TreeTraversal

A: "Set up a test harness for the TreeTraversal class covering null root and single-node trees."
  "test harness" → TEST (compound). "set up" is context verb; "test harness" is the test-infrastructure noun.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P040-005 — GAP: "determine traversal path" fires zero predicates

A: "Determine the traversal path through the binary search tree."
  "determine" → domain computation verb; zero predicates.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
