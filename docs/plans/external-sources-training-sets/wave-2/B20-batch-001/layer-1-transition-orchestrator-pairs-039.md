# Layer 1 — Transition Orchestrator Pairs (039) — DateTimeCalc: TransitionType decisions
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T039-001 — TEST alone → documentation for DateTimeCalc

Request: "Validate that the DateTimeCalc rejects February 30 as an invalid date."
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation
  Evidence: "validate" → TEST. No construction verb present.

---

## T039-002 — TEST+IMPLEMENT → feature_addition for DateTimeCalc

Request: "Implement the DateTimeCalc class and write tests for leap year boundary dates."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition
  Evidence: "implement" and "write" → IMPLEMENT construction verbs → feature_addition.

---

## T039-003 — Zero predicates → unknown for DateTimeCalc

Request: "Calculate the date difference between the start and end dates."
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
  Evidence: "calculate" → domain computation verb; zero predicates.
