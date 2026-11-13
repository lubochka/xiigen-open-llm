# Layer 1 — Transition Orchestrator Pairs (054) — Gerund forms of IMPLEMENT verbs fire IMPLEMENT
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T054-001 — Gerund forms of IMPLEMENT verbs: full reference

IMPLEMENT fires on all morphological forms of each lexicon verb:
  write → writes / writing / written
  create → creates / creating / created
  build → builds / building / built
  develop → develops / developing / developed
  implement → implements / implementing / implemented
  add → adds / adding / added
  extend → extends / extending / extended
  generate → generates / generating / generated

---

## T054-002 — Gerund "writing" fires IMPLEMENT: SortingAlgorithm example

Request: "Writing unit tests for the SortingAlgorithm is the first step in the TDD (Test-Driven Development) process."
  "writing" (gerund of "write") → IMPLEMENT fires. "unit tests" → TEST fires.
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition
  Teaching: gerund forms preserve predicate-firing; TransitionType follows the same rules.

---

## T054-003 — Past participle "implemented" does NOT introduce new IMPLEMENT predicate in passive voice

Request: "The DijkstraPath has been implemented; validate the path output."
  "implemented" (past participle, passive) → does NOT fire IMPLEMENT for a new action.
  "validate" → TEST fires.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation
  Teaching: passive "has been [verb]" describes a completed state, not a new construction action.
  IMPLEMENT fires only when describing a future/current construction action, not a completed state.
