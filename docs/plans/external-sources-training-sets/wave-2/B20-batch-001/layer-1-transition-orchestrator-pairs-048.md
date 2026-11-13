# Layer 1 — Transition Orchestrator Pairs (048) — Utility domain TransitionType summary reference
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T048-001 — TransitionType decision table: all utility-domain predicate combinations

| CompiledPredicateIds | TransitionType | Rationale |
|---------------------|----------------|-----------|
| ["TEST"] | documentation | TEST alone = specifying expected behaviour; no construction action |
| ["IMPLEMENT"] | feature_addition | "generate" boundary case; construction verb present |
| ["TEST", "IMPLEMENT"] | feature_addition | Construction verb present; new test artifacts |
| [] | unknown | No predicates → no TransitionType |

---

## T048-002 — TransitionType belongs to IMPLEMENT; TEST alone → documentation (utility domain)

Rule confirmed for all utility-domain classes:
  - IMPLEMENT governs TransitionType when present.
  - TEST alone → documentation.
  - Utility domain verbs (parse, format, convert, calculate, compute) → NOT IMPLEMENT.
  - Only: write, create, build, develop, implement, add, extend, generate → IMPLEMENT.
  - "configure", "update", "set up", "initialise" → NOT IMPLEMENT.

---

## T048-003 — "update" vs "extend" TransitionType boundary: utility domain confirmed

A-update: "Update the TimeZoneConverter to support IANA identifiers." → [] → unknown
A-extend: "Extend the TimeZoneConverter to support IANA identifiers." → ["IMPLEMENT"] → feature_addition
"update" does not fire IMPLEMENT; "extend" does.
TransitionType differs entirely based on this single word choice.
