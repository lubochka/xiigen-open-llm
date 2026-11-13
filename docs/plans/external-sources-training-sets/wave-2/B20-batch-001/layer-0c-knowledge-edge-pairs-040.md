# Layer 0-C — Knowledge Edge Pairs (040) — TreeTraversal: gap analysis for tree-domain verbs
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E040-001 — Tree traversal domain verbs that always produce zero predicates

Execution and domain verbs confirmed as zero-predicate for TreeTraversal context:
  traverse, visit, walk, in-order, pre-order, post-order (when used as operation verbs) — all zero predicates
  run, invoke, execute, trigger, start — execution verbs; all zero predicates
  "in-order traversal", "pre-order traversal", "post-order traversal" as noun phrases — zero predicates

---

## E040-002 — "process tree nodes" does NOT fire IMPLEMENT

"Process the tree nodes using in-order traversal." → CompiledPredicateIds: []
"process" is a domain computation verb. It is NOT in the IMPLEMENT predicate lexicon.
Confirmed gap: operation verbs on tree data structures universally produce zero predicates.

---

## E040-003 — "determine traversal path" does NOT fire TEST or IMPLEMENT

"Determine the traversal path through the binary search tree." → CompiledPredicateIds: []
"determine" is a domain computation verb. Zero predicates.
BST (Binary Search Tree) — domain abbreviation; does not fire predicates.

---

## E040-004 — "test harness" provides TEST signal; "set up" is neutral context verb

"Set up a test harness for the TreeTraversal class." → CompiledPredicateIds: ["TEST"]
Note: this is NOT a zero-predicate case. "test harness" is a test-infrastructure compound.
"set up" is a context verb; it does not add or suppress predicates.
The TEST signal comes from "test harness" independently of the context verb.
