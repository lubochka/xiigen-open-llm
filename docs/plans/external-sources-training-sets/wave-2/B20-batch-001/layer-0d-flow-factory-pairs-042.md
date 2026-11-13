# Layer 0-D — Flow Factory Pairs (042) — BinarySearch: factory with deduplication pattern
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F042-001 — TEST alone (three verbs deduplicate): "assert, verify, validate" → one instance, documentation

Request: "Assert, verify, and validate that the BinarySearch returns mid-index for a single-element array."
  Three TEST-firing verbs → one ["TEST"] predicate → FlowFactory.CreateMulti(["TEST"]) → [UnitTestingFlow51]
  TransitionType: documentation.
  Invariant: three TEST verbs deduplicate to one predicate → one flow instance → one TransitionType.

---

## F042-002 — TEST+IMPLEMENT: "create BinarySearch and add test coverage" → two instances

Request: "Create the BinarySearch class and add test coverage for sorted and unsorted arrays."
  FlowFactory.CreateMulti(["TEST", "IMPLEMENT"]) → [BackendServiceFlow41, UnitTestingFlow51]
  TransitionType: feature_addition.

---

## F042-003 — Zero predicate: "look up element" → empty factory

Request: "Look up the target element in the sorted array using binary search."
  FlowFactory.CreateMulti([]) → []
  TransitionType: unknown | SourceStatus: knowledge_edge.
