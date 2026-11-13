# Layer 1 — Transition Orchestrator Pairs (040) — TimeZoneConverter: TransitionType decisions
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T040-001 — TEST alone → documentation for TimeZoneConverter

Request: "Assert that the TimeZoneConverter handles DST (Daylight Saving Time) transitions without a 1-hour offset error."
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation
  Evidence: "assert" → TEST. No construction verb. DST (Daylight Saving Time) → domain term; does not fire predicates.

---

## T040-002 — TEST+IMPLEMENT → feature_addition for TimeZoneConverter

Request: "Extend the TimeZoneConverter to support IANA (Internet Assigned Numbers Authority) timezone IDs and add regression tests."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition
  Evidence: "extend" and "add" → IMPLEMENT construction verbs → feature_addition.

---

## T040-003 — Zero predicates → unknown for TimeZoneConverter

Request: "Convert the meeting timestamp from UTC (Coordinated Universal Time) to Tokyo Standard Time."
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
  Evidence: "convert" → domain operation verb; zero predicates.
