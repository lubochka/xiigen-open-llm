# Layer 0-A — Compiler Pairs (038) — SearchAlgorithm: TEST fires on "verify", not on search-domain terms
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: compiler

---

## P038-001 — TEST: "verify" fires TEST on search result correctness

A: "Verify that the SearchAlgorithm returns the correct index when the target element is present."
  "verify" → TEST. Fires even when checking index-based results.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P038-002 — TEST+IMPLEMENT: "build and spec SearchAlgorithm"

A: "Build the SearchAlgorithm class and spec out the linear search behaviour for null inputs."
  "build" → IMPLEMENT | "spec" → TEST
  "linear search" — domain name; does not fire any predicate.
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition | SubPhaseId: 5.1 + 4.1

---

## P038-003 — GAP: "invoke search and confirm result" fires zero predicates

A: "Invoke the SearchAlgorithm and confirm that the result is the expected index."
  "invoke" → execution verb; zero predicates. "confirm" → informal QA; zero predicates.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## P038-004 — TEST: "assert" fires TEST on not-found boundary

A: "Assert that the SearchAlgorithm returns -1 when the target is not in the collection."
  "assert" → TEST. Fires for negative/not-found expectations equally.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P038-005 — GAP: "searchable" adjective does NOT fire TEST

A: "Ensure the collection is searchable before calling the SearchAlgorithm."
  "ensure" → informal QA; zero predicates. "searchable" → adjective; does not fire TEST.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
