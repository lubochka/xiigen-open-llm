# Layer 1 — Transition Orchestrator Pairs (047) — RandomGenerator: TransitionType with "generate" boundary
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T047-001 — IMPLEMENT alone → feature_addition for RandomGenerator ("generate" boundary)

Request: "Generate a random number between 1 and 100 using the RandomGenerator."
  CompiledPredicateIds: ["IMPLEMENT"] | TransitionType: feature_addition
  Evidence: "generate" → IMPLEMENT. No test-infrastructure vocabulary present.
  TransitionType: feature_addition (IMPLEMENT governs even alone).

---

## T047-002 — TEST+IMPLEMENT → feature_addition for RandomGenerator

Request: "Add the RandomGenerator class and write unit tests for uniform distribution and seeded reproducibility."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition
  Evidence: "add" and "write" → IMPLEMENT construction verbs → feature_addition.

---

## T047-003 — Zero predicates → unknown for RandomGenerator

Request: "Produce random samples from the distribution using the configured RandomGenerator."
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
  Evidence: "produce" → domain operation verb; NOT in IMPLEMENT lexicon; zero predicates.
