# Layer 1 — Transition Orchestrator Pairs (043) — UnitConverter: TransitionType decisions
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T043-001 — TEST alone → documentation for UnitConverter

Request: "Validate that the UnitConverter throws UnsupportedConversionException for incompatible unit types."
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation
  Evidence: "validate" → TEST. No construction verb present.

---

## T043-002 — TEST+IMPLEMENT → feature_addition for UnitConverter

Request: "Implement the UnitConverter and write a test harness for metric to imperial length conversions."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition
  Evidence: "implement" and "write" → IMPLEMENT construction verbs → feature_addition.

---

## T043-003 — Zero predicates → unknown for UnitConverter

Request: "Convert the measurement from metres to feet using the UnitConverter."
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
  Evidence: "convert" → domain operation verb; zero predicates.
