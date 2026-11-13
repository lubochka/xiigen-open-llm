# Layer 1 — Transition Orchestrator Pairs (041) — CurrencyFormatter: TransitionType decisions
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T041-001 — TEST alone → documentation for CurrencyFormatter

Request: "Validate that the CurrencyFormatter correctly displays negative amounts with a leading minus sign."
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation
  Evidence: "validate" → TEST. No construction verb present.

---

## T041-002 — TEST+IMPLEMENT → feature_addition for CurrencyFormatter

Request: "Create the CurrencyFormatter and write a test suite covering GBP (Great British Pound), EUR (Euro), and USD (US Dollar)."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition
  Evidence: "create" and "write" → IMPLEMENT construction verbs → feature_addition.

---

## T041-003 — Zero predicates → unknown for CurrencyFormatter

Request: "Format the currency value to two decimal places."
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
  Evidence: "format" → domain operation verb; zero predicates.
