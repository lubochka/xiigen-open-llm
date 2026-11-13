# Layer 1 — Transition Orchestrator Pairs (049) — "should" modal boundary: universal TransitionType rule
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T049-001 — "should" + domain description → unknown: StatisticsCalc example

Request: "The StatisticsCalc should throw InvalidOperationException for an empty dataset."
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
  Evidence: "should throw" → modal + domain behaviour description; does not fire any predicate.
  No test-lexicon verb present → zero predicates → TransitionType: unknown.

---

## T049-002 — "assert that [service] should" combines TEST verb with modal: TEST fires

Request: "Assert that the StatisticsCalc should throw InvalidOperationException for an empty dataset."
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation
  Evidence: "assert" → TEST fires from the action verb, not from "should throw".
  "should throw" → modal description; does not fire independently.
  TransitionType: documentation (TEST alone; no IMPLEMENT construction verb).

---

## T049-003 — Universal rule: modals ("should", "must", "shall", "would") never fire predicates

"The BinarySearch must return -1 for missing elements." → CompiledPredicateIds: [] → unknown
"The UnitConverter shall handle incompatible types." → CompiledPredicateIds: [] → unknown
"The CurrencyFormatter would display zeros as 0.00." → CompiledPredicateIds: [] → unknown
All three → zero predicates → TransitionType: unknown.
Modal verbs expressing design intent do not fire TEST, IMPLEMENT, or any predicate.
Predicates fire only from explicit action verbs in the predicate lexicons.
