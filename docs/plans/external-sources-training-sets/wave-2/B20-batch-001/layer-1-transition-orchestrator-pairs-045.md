# Layer 1 — Transition Orchestrator Pairs (045) — StatisticsCalc: TransitionType decisions
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T045-001 — TEST alone → documentation for StatisticsCalc

Request: "Validate that the StatisticsCalc throws InvalidOperationException when passed an empty dataset."
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation
  Evidence: "validate" → TEST. No construction verb present.

---

## T045-002 — TEST+IMPLEMENT → feature_addition for StatisticsCalc

Request: "Create the StatisticsCalc class and write tests for mean, median, and standard deviation."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition
  Evidence: "create" and "write" → IMPLEMENT construction verbs → feature_addition.

---

## T045-003 — Zero predicates → unknown for StatisticsCalc

Request: "Compute the mean of the dataset using StatisticsCalc."
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
  Evidence: "compute" → domain computation verb; zero predicates.
