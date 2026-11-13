# Layer 0-C — Knowledge Edge Pairs (039) — GraphTraversal: gap analysis for traversal-domain verbs
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E039-001 — Graph traversal domain verbs that always produce zero predicates

Execution and domain verbs confirmed as zero-predicate for GraphTraversal context:
  traverse, visit, explore, walk, scan, expand — domain traversal verbs; all zero predicates
  run, invoke, execute, trigger, start — execution verbs; all zero predicates
  BFS (Breadth-First Search), DFS (Depth-First Search) as standalone nouns — zero predicates

---

## E039-002 — "apply traversal" does NOT fire IMPLEMENT

"Apply the traversal algorithm to the dependency tree." → CompiledPredicateIds: []
"apply" (standalone) is explicitly NOT in the IMPLEMENT predicate lexicon.
IMPLEMENT fires on: write, create, build, develop, implement, add, extend, generate.
"apply" fails to fire IMPLEMENT even when it describes an algorithmic action.

---

## E039-003 — "BFS" and "DFS" as abbreviations do NOT fire predicates

"Run BFS (Breadth-First Search) on the graph starting from node 0." → CompiledPredicateIds: []
"BFS" and "DFS" are domain algorithm abbreviations. Neither fires TEST or IMPLEMENT.
"run" is an execution verb producing zero predicates regardless of what follows.
Both layers confirm zero predicates: the abbreviation layer and the execution verb layer.

---

## E039-004 — "visit" does NOT fire TEST or IMPLEMENT

"Visit all reachable nodes in the graph using depth-first traversal." → CompiledPredicateIds: []
"visit" is a domain traversal verb. It is not in any predicate lexicon.
Confirmed gap: traversal action verbs universally produce zero predicates.
