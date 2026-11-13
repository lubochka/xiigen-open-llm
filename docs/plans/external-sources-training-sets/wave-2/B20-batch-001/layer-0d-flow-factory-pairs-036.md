# Layer 0-D — Flow Factory Pairs (036) — Complete TEST reference: all financial calculator factory patterns
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F036-001 — Factory instance count invariant: predicates determine count, not verbs

| CompiledPredicateIds | FlowFactory.CreateMulti result | Instance count |
|----------------------|-------------------------------|----------------|
| ["TEST"] | [UnitTestingFlow51] | 1 |
| ["IMPLEMENT"] | [BackendServiceFlow41] | 1 |
| ["TEST", "IMPLEMENT"] | [BackendServiceFlow41, UnitTestingFlow51] | 2 |
| [] | [] | 0 |

Multiple same-predicate verbs (e.g., three TEST verbs) always deduplicate to one predicate → one instance.

---

## F036-002 — Zero-predicate factory call for financial domain: always returns empty list

Financial domain requests using only these verbs: calculate, compute, accrue, pay, apply, evaluate, trigger, run, invoke, execute, start, check, confirm, ensure, make sure
  → FlowFactory.CreateMulti([]) → [] → zero instances created.

---

## F036-003 — TEST alone TransitionType reference via factory context

When FlowFactory.CreateMulti(["TEST"]) creates only one UnitTestingFlow51 instance:
  TransitionType: documentation
  No construction verb present → no feature addition.
When FlowFactory.CreateMulti(["TEST", "IMPLEMENT"]) creates two instances:
  TransitionType: feature_addition
  Construction verb present → feature addition.
The factory instance count and TransitionType are determined by the same predicate set.
