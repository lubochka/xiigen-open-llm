# Layer 0-D — Flow Factory Pairs (060) — Complete factory reference: all utility-domain factory patterns
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F060-001 — Factory instance summary: all utility-domain predicate combinations

| CompiledPredicateIds | FlowFactory.CreateMulti result | TransitionType |
|----------------------|-------------------------------|----------------|
| ["TEST"] | [UnitTestingFlow51] | documentation |
| ["IMPLEMENT"] | [BackendServiceFlow41] | feature_addition |
| ["TEST", "IMPLEMENT"] | [BackendServiceFlow41, UnitTestingFlow51] | feature_addition |
| [] | [] | unknown |

"generate" is the only utility operation verb that produces an IMPLEMENT-alone factory result.

---

## F060-002 — Zero-predicate factory calls for utility domain: always returns empty list

Utility operation verbs that always produce FlowFactory.CreateMulti([]) → []:
  parse, split, format, round, convert, match, calculate, compute, determine, produce, output, configure, set up
  All execution verbs: run, invoke, execute, trigger, start, launch
  All informal QA: check, confirm, ensure, make sure

---

## F060-003 — "generate" boundary case: IMPLEMENT alone vs all other utility verbs

"Generate output from RandomGenerator." → FlowFactory.CreateMulti(["IMPLEMENT"]) → [BackendServiceFlow41] → feature_addition
"Format number with NumberFormatter." → FlowFactory.CreateMulti([]) → [] → unknown
"Convert value with UnitConverter." → FlowFactory.CreateMulti([]) → [] → unknown
"Calculate with StatisticsCalc." → FlowFactory.CreateMulti([]) → [] → unknown
Teaching: "generate" uniquely creates a flow instance among utility operation verbs. All others produce empty factory.
