# Layer 0-B — Router Pairs (060) — Complete routing reference for all utility-domain classes
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: router

---

## R060-001 — Routing decision table: all utility-domain predicate combinations

| CompiledPredicateIds | RoutingDecisions | TransitionType | Notes |
|----------------------|-----------------|----------------|-------|
| ["TEST"] | TEST → 5.1 | documentation | TEST alone |
| ["IMPLEMENT"] | IMPLEMENT → 4.1 | feature_addition | "generate" boundary case |
| ["TEST", "IMPLEMENT"] | TEST → 5.1 + IMPLEMENT → 4.1 | feature_addition | Most common |
| [] | none | unknown | knowledge_edge |

---

## R060-002 — Zero-predicate utility vocabulary routing summary

Requests using only these verbs produce zero routing decisions:
  parse, split, match, format, round, convert, transform, determine, evaluate, calculate, compute
  process, produce, apply (standalone) — domain operation verbs
  run, execute, trigger, launch, invoke, start — execution verbs
  make sure, check, confirm, ensure — informal QA

---

## R060-003 — "generate" routing minimal pair: IMPLEMENT vs domain operation verbs

A-generate: "Generate the output sequence from the RandomGenerator." → [IMPLEMENT → 4.1] | feature_addition
A-convert: "Convert the value using the UnitConverter." → [] | unknown | knowledge_edge
A-format: "Format the number using the NumberFormatter." → [] | unknown | knowledge_edge
Teaching: "generate" uniquely routes to 4.1 among utility operation verbs. All others route to nowhere.
