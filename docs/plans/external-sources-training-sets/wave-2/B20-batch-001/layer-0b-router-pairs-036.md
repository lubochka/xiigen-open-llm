# Layer 0-B — Router Pairs (036) — Complete routing reference for Round 2 financial calculators
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: router

---

## R036-001 — Routing decision table: all Round 2 predicate combinations

| CompiledPredicateIds | RoutingDecisions | TransitionType | Notes |
|----------------------|-----------------|----------------|-------|
| ["TEST"] | TEST → 5.1 | documentation | TEST alone |
| ["IMPLEMENT"] | IMPLEMENT → 4.1 | feature_addition | IMPLEMENT alone (e.g., "generate") |
| ["TEST", "IMPLEMENT"] | TEST → 5.1 + IMPLEMENT → 4.1 | feature_addition | Most common |
| [] | none | unknown | knowledge_edge |

---

## R036-002 — Zero-predicate financial vocabulary routing summary

Requests using only these verbs produce zero routing decisions:
  calculate, compute, accrue, pay, apply (standalone), evaluate, trigger, invoke, run, execute, launch, start
  make sure, check, confirm, ensure
  All route to: RoutingDecisions: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## R036-003 — "validate" vs "confirm" routing minimal pair

A-validate:
  compiled_predicate_ids: ["TEST"]
  compiled_text: "Validate the InterestCalculator returns 5% for a £1,000 principal."
  RoutingDecisions: [TEST → 5.1] | TransitionType: documentation

A-confirm:
  compiled_predicate_ids: []
  compiled_text: "Confirm the InterestCalculator returns 5% for a £1,000 principal."
  RoutingDecisions: [] | TransitionType: unknown | SourceStatus: knowledge_edge

Teaching: "validate" routes to 5.1; "confirm" routes to nowhere. Same sentence structure, opposite outcome.
