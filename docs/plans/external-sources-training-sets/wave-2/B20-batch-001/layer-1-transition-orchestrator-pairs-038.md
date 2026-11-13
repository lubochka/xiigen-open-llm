# Layer 1 — Transition Orchestrator Pairs (038) — RegexValidator: TransitionType decisions
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T038-001 — TEST alone → documentation for RegexValidator

Request: "Spec the RegexValidator behaviour for strings at the maximum allowed length."
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation
  Evidence: "spec" → TEST. No construction verb present.

---

## T038-002 — TEST+IMPLEMENT → feature_addition for RegexValidator

Request: "Build the RegexValidator and add test cases for email, phone, and postal code patterns."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition
  Evidence: "build" and "add" → IMPLEMENT construction verbs → feature_addition.

---

## T038-003 — Zero predicates → unknown for RegexValidator

Request: "Apply the regex rules to sanitise user input."
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
  Evidence: "apply" (standalone) → NOT in IMPLEMENT lexicon; zero predicates.
