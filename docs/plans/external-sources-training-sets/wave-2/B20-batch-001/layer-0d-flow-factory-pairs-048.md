# Layer 0-D — Flow Factory Pairs (048) — Algorithm domain factory summary reference
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F048-001 — Factory instance summary: all algorithm-domain predicate combinations

| CompiledPredicateIds | FlowFactory.CreateMulti result | TransitionType |
|----------------------|-------------------------------|----------------|
| ["TEST"] | [UnitTestingFlow51] | documentation |
| ["IMPLEMENT"] | [BackendServiceFlow41] | feature_addition |
| ["TEST", "IMPLEMENT"] | [BackendServiceFlow41, UnitTestingFlow51] | feature_addition |
| [] | [] | unknown |

---

## F048-002 — Zero-predicate factory calls for algorithm domain

Algorithm domain verbs that always produce empty factory results:
  sort, search, traverse, partition, merge, heapify — core algorithm verbs
  compute, calculate, find, locate, visit — operation verbs
  run, invoke, execute, trigger — execution verbs
  All produce FlowFactory.CreateMulti([]) → [].

---

## F048-003 — TEST alone from test-infrastructure nouns: documentation invariant

When TEST fires from test-infrastructure nouns without a construction verb:
  "test suite", "test harness", "test fixture", "test double", "unit tests", "test cases", "test coverage"
  If no construction verb (write, create, build, develop, implement, add, extend, generate) is present:
  → FlowFactory.CreateMulti(["TEST"]) → [UnitTestingFlow51] → TransitionType: documentation
  This invariant holds regardless of which test-infrastructure noun fires TEST.
