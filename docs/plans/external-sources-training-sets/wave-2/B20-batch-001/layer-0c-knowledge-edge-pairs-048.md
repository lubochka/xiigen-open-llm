# Layer 0-C — Knowledge Edge Pairs (048) — Complete algorithm-domain gap reference (Round 3 confirmed)
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E048-001 — Complete algorithm operation verb gap list (Round 3 confirmed)

Algorithm operation verbs that always produce zero predicates:
  sort, search, traverse, partition, merge, heapify, pivot — core algorithm verbs
  find, locate, look up, retrieve, query, fetch — search/lookup verbs
  compute, calculate, determine, evaluate, process — computation verbs
  visit, explore, walk, scan, expand — traversal verbs
  apply (standalone), handle, resolve, distribute — operation verbs

---

## E048-002 — "apply" boundary: standalone vs compound

"apply" (standalone) → always zero predicates. NOT in IMPLEMENT lexicon.
"apply changes" → zero predicates (same rule).
"apply security patch" → zero predicates (same rule; "patch" is not in IMPLEMENT lexicon either).
Contrast: "implement, create, build, add, write, develop, extend, generate" → IMPLEMENT fires.
"apply" is the canonical example of an operation verb that looks like IMPLEMENT but is not.

---

## E048-003 — Complete -able adjective gap list for algorithm domain (Round 3 confirmed)

-able adjectives that never fire TEST or any predicate:
  sortable, searchable, traversable, hashable, iterable, comparable, serialisable, parseable
  These all follow the "testable" adjective rule: adjective form never fires TEST.
  Only noun/verb/gerund forms of test-related words fire TEST.

---

## E048-004 — Algorithm abbreviation gap list (Round 3 confirmed)

Algorithm abbreviations that never fire predicates:
  BFS (Breadth-First Search), DFS (Depth-First Search), BST (Binary Search Tree)
  AVL (Adelson-Velsky and Landis tree), TSP (Travelling Salesman Problem)
  DP (Dynamic Programming), O(n) (Order notation) — all domain vocabulary
  Predicates fire from action verbs and test-infrastructure vocabulary only.
