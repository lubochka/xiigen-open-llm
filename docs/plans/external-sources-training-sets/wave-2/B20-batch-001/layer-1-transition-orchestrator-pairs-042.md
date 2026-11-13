# Layer 1 — Transition Orchestrator Pairs (042) — NumberFormatter: TransitionType decisions
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T042-001 — TEST alone → documentation for NumberFormatter

Request: "Assert that the NumberFormatter displays 0 as '0.00' when precision is set to 2."
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation
  Evidence: "assert" → TEST. No construction verb present.

---

## T042-002 — TEST+IMPLEMENT → feature_addition for NumberFormatter

Request: "Develop the NumberFormatter and add test cases for thousands separators and decimal precision."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition
  Evidence: "develop" and "add" → IMPLEMENT construction verbs → feature_addition.

---

## T042-003 — Zero predicates → unknown for NumberFormatter

Request: "Round the number to two decimal places using the NumberFormatter precision setting."
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
  Evidence: "round" → domain operation verb; zero predicates.
