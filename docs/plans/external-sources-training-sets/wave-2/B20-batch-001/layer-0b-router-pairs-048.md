# Layer 0-B — Router Pairs (048) — Algorithm domain routing summary reference
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: router

---

## R048-001 — Routing decision table: all algorithm-domain predicate combinations

| CompiledPredicateIds | RoutingDecisions | TransitionType | Notes |
|----------------------|-----------------|----------------|-------|
| ["TEST"] | TEST → 5.1 | documentation | TEST alone |
| ["IMPLEMENT"] | IMPLEMENT → 4.1 | feature_addition | IMPLEMENT alone (e.g., "generate") |
| ["TEST", "IMPLEMENT"] | TEST → 5.1 + IMPLEMENT → 4.1 | feature_addition | Most common |
| [] | none | unknown | knowledge_edge |

---

## R048-002 — Zero-predicate algorithm vocabulary routing summary

Requests using only these verbs produce zero routing decisions:
  sort, search, traverse, partition, merge, heapify, compute, calculate, determine, evaluate, process
  run, execute, trigger, launch, invoke, start — execution verbs (same gap as all other B20 domains)
  make sure, check, confirm, ensure — informal QA (same gap as all other B20 domains)

---

## R048-003 — Routing minimal pair for algorithm domain: "assert" vs "check"

A-assert: "Assert that the MergeSort output is sorted." → [TEST → 5.1] | documentation
A-check: "Check that the MergeSort output is sorted." → [] | unknown | knowledge_edge
Teaching: same sentence structure, opposite routing. "assert" routes; "check" does not.
