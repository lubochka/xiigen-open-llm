# Layer 0-C — Knowledge Edge Pairs (038) — SearchAlgorithm: gap analysis for search-domain verbs
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E038-001 — Search domain verbs that always produce zero predicates

Execution and domain verbs confirmed as zero-predicate for SearchAlgorithm context:
  search, find, look up, locate, retrieve, query, fetch — domain operation verbs; all zero predicates
  run, invoke, execute, trigger — execution verbs; all zero predicates
  "searchable" (adjective) — zero predicates

---

## E038-002 — "invoke search and confirm" double-gap pattern

"Invoke the SearchAlgorithm and confirm the result is the expected index." → CompiledPredicateIds: []
Two zero-predicate verbs compound: "invoke" (execution) + "confirm" (informal QA).
Neither adds predicates. The result is zero predicates, not a partial match.
SCHEDULE_CAPABILITY_LEARNING: true — this is a gap request.

---

## E038-003 — "find" does NOT fire TEST or IMPLEMENT

"Find the target element in the sorted array using binary search." → CompiledPredicateIds: []
"find" is a domain operation verb. It is NOT in any predicate lexicon.
This is distinct from "locate" and "retrieve" — all three are domain-only vocabulary.

---

## E038-004 — "searchable" adjective gap: parallel to "testable" rule

"Ensure the data structure is searchable before applying the SearchAlgorithm." → CompiledPredicateIds: []
"ensure" → informal QA. "searchable" → adjective; does not fire TEST.
Confirmed: -able adjectives universally produce zero predicates in B20.
