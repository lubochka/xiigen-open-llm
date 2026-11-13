# Layer 0-A — Compiler Pairs (048) — Complete TEST+IMPLEMENT combo reference for algorithm domain
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: compiler

---

## P048-001 — Full reference: algorithm-domain verbs that fire TEST

TEST-firing verbs confirmed for algorithm domain (identical to all B20 domains):
  test, verify, assert, validate, spec, mock (test-adjacent), stub (test-adjacent), test-driven
  Compounds: unit tests, test cases, test suite, test harness, test fixture, test double, test coverage, regression tests
  Domain algorithm names do NOT add or remove predicates: sort, search, traverse, heapify, partition, merge, hash, bloom.

---

## P048-002 — Full reference: algorithm-domain verbs that fire IMPLEMENT

IMPLEMENT-firing verbs confirmed for algorithm domain:
  write, create, build, develop, implement, add, extend, generate
  Domain operation verbs do NOT fire IMPLEMENT: sort, search, traverse, partition, merge, heapify, compute, calculate, apply.

---

## P048-003 — Minimal pair summary for algorithm domain: "build" (IMPLEMENT) vs "run" (zero)

A-build: "Build the MergeSort class." → CompiledPredicateIds: ["IMPLEMENT"]
A-run: "Run the MergeSort on the dataset." → CompiledPredicateIds: []
Same algorithm, same sentence structure, opposite outcomes. "build" constructs; "run" executes.

---

## P048-004 — TEST alone TransitionType reference for algorithm-domain calculators

When only TEST fires (assert/verify/validate/spec on algorithm domain):
  TransitionType: documentation
  All algorithm domain verbs (sort, search, traverse, partition, heapify) do NOT count as IMPLEMENT.

---

## P048-005 — Algorithm abbreviation gap reference

None of these algorithm-domain abbreviations fire predicates:
  BFS (Breadth-First Search), DFS (Depth-First Search), BST (Binary Search Tree),
  AVL (Adelson-Velsky and Landis tree), TSP (Travelling Salesman Problem),
  DP (Dynamic Programming), GFG (Graph Flow Graph)
  All are domain vocabulary. Predicates fire from action verbs and test-infrastructure vocabulary only.
