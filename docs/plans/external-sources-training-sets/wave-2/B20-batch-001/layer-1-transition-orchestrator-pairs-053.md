# Layer 1 — Transition Orchestrator Pairs (053) — "test coverage" vs "code coverage" TransitionType boundary
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T053-001 — "test coverage" fires TEST; "code coverage" does not: BinarySearch example

A-test-coverage: "Add test coverage for the BinarySearch edge cases."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition
  Evidence: "add" → IMPLEMENT. "test coverage" → TEST (via "test"). Both predicates → feature_addition.

A-code-coverage: "Improve code coverage for the BinarySearch."
  CompiledPredicateIds: [] | TransitionType: unknown
  Evidence: "improve" → NOT in IMPLEMENT lexicon. "code coverage" → gap; "code" replaces "test" signal.

---

## T053-002 — "improve" does NOT fire IMPLEMENT: universal gap verb

"Improve the StatisticsCalc performance." → CompiledPredicateIds: []
"Improve test coverage for the GeoCalculator." → CompiledPredicateIds: ["TEST"]
  "improve" → NOT in IMPLEMENT lexicon; zero IMPLEMENT predicate.
  "test coverage" → TEST fires (compound via "test").
  TransitionType: documentation (TEST alone; "improve" does not fire IMPLEMENT).

---

## T053-003 — Confirmed: "improve" + "test coverage" → TEST alone → documentation

"Improve test coverage for the UnitConverter by adding boundary cases."
  "improve" → zero predicates. "test coverage" → TEST. "adding" → is "add" in gerund form → IMPLEMENT fires.
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition
  Teaching: "adding" (gerund of "add") fires IMPLEMENT; "improve" does not. Gerund form of IMPLEMENT verbs fires.
