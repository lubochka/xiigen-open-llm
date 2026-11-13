# Layer 1 — Transition Orchestrator Pairs (052) — IMPLEMENT alone from "generate": single-predicate feature_addition
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T052-001 — IMPLEMENT alone → feature_addition: unique to "generate" among utility verbs

Request: "Generate the output sequence from the RandomGenerator."
  CompiledPredicateIds: ["IMPLEMENT"] | TransitionType: feature_addition
  Evidence: "generate" → IMPLEMENT. No TEST predicate.
  TransitionType: feature_addition (IMPLEMENT alone → feature_addition; not documentation).

---

## T052-002 — IMPLEMENT alone → feature_addition across all B20 domains (generate examples)

"Generate the HeapSort class." → ["IMPLEMENT"] → feature_addition
"Generate the SortingAlgorithm lookup table." → ["IMPLEMENT"] → feature_addition
"Generate stub configuration files for the UnitConverter." → ["IMPLEMENT"] → feature_addition
All "generate" requests across any B20 domain → TransitionType: feature_addition.

---

## T052-003 — Confirmed: TransitionType feature_addition ≠ documentation distinction rule

TEST alone → documentation (specifying expected behaviour; no construction).
IMPLEMENT alone → feature_addition (constructing; no test specification).
TEST + IMPLEMENT → feature_addition (construction present; governs TransitionType).
[] → unknown (no predicates; no TransitionType assigned).
The four cases are mutually exclusive and exhaustive for all B20 scenarios.
