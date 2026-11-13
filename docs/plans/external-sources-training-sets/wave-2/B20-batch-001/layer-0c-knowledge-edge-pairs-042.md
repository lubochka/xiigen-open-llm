# Layer 0-C — Knowledge Edge Pairs (042) — BinarySearch: gap analysis for search-boundary cases
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E042-001 — "look up" does NOT fire TEST or IMPLEMENT

"Look up the target element in the sorted array using binary search." → CompiledPredicateIds: []
"look up" is a domain operation phrase. It is not in any predicate lexicon.
Confirmed gap: "look up", "find", "locate", "retrieve" are all domain lookup verbs producing zero predicates.

---

## E042-002 — "test coverage" fires TEST; "code coverage" does NOT

"Add test coverage for the BinarySearch edge cases." → CompiledPredicateIds: ["TEST", "IMPLEMENT"]
  "test coverage" → TEST (via "test"). "add" → IMPLEMENT.
"Improve code coverage for the BinarySearch." → CompiledPredicateIds: []
  "code coverage" → gap; "code" before "coverage" replaces the TEST-firing "test" signal.
Teaching: "test coverage" fires TEST; "code coverage" does not. Exact word matters.

---

## E042-003 — Three TEST verbs collapse to single predicate; no route multiplication

"Assert, verify, and validate the BinarySearch returns correct mid-index." → CompiledPredicateIds: ["TEST"]
Deduplication rule: multiple TEST-firing verbs produce exactly one ["TEST"] predicate.
One predicate → one routing decision (TEST → 5.1) → one flow instance (UnitTestingFlow51).
Three verbs do not produce three flow instances or three routing decisions.

---

## E042-004 — "regression tests" fires TEST; "regression" alone does NOT

"Write regression tests for the BinarySearch after extending to float arrays." → ["TEST", "IMPLEMENT"]
  "regression tests" → TEST (compound). "write" → IMPLEMENT.
"Regression found in the BinarySearch for float arrays." → CompiledPredicateIds: []
  "regression" (standalone) → domain term describing a defect; does not fire TEST.
Teaching: the TEST signal requires the compound "regression tests", not "regression" alone.
