# Layer 0-B — Router Pairs (038) — SearchAlgorithm: routing decisions for TEST and zero-predicate cases
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: router

---

## R038-001 — TEST alone: "verify" on SearchAlgorithm routes to 5.1, documentation

Request: "Verify that the SearchAlgorithm returns the correct index when the target is present."
  CompiledPredicateIds: ["TEST"]
  RoutingDecisions: [TEST → 5.1] | TransitionType: documentation | SourceStatus: routable

---

## R038-002 — TEST+IMPLEMENT: "build and spec SearchAlgorithm" routes to 5.1 + 4.1

Request: "Build the SearchAlgorithm class and spec the linear search behaviour for null inputs."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"]
  RoutingDecisions: [TEST → 5.1, IMPLEMENT → 4.1] | TransitionType: feature_addition

---

## R038-003 — Zero predicate: "invoke search" routes to nowhere

Request: "Invoke the SearchAlgorithm and confirm that the result is correct."
  "invoke" → execution verb; "confirm" → informal QA. Both produce zero predicates.
  CompiledPredicateIds: []
  RoutingDecisions: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## R038-004 — Routing minimal pair: "assert" (TEST → 5.1) vs "confirm" (nowhere)

A-assert: "Assert the SearchAlgorithm returns -1 for missing elements." → [TEST → 5.1] | documentation
A-confirm: "Confirm the SearchAlgorithm returns -1 for missing elements." → [] | unknown
Teaching: same object, opposite routing. "assert" is in the TEST lexicon; "confirm" is not.
