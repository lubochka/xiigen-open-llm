# Layer 1 — Transition Orchestrator Pairs (024) — TransitionType decision table for TEST predicate combinations
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T024-001 — TransitionType decision table: TEST predicate combinations

| CompiledPredicateIds | TransitionType | Rationale |
|---------------------|----------------|-----------|
| ["TEST"] | documentation | TEST alone = specifying expected behaviour; no construction action |
| ["TEST", "IMPLEMENT"] | feature_addition | Construction verb present; new test artifacts |
| ["TEST", "SECURITY"] | feature_addition | Two predicates = multi-dimensional action |
| ["TEST", "IMPLEMENT", "SECURITY"] | feature_addition | Construction verb present; three-predicate scope |
| ["TEST", "DOCUMENT"] | documentation | Two documentation-class predicates; no construction |
| [] | unknown | No predicates → no TransitionType |

---

## T024-002 — TransitionType belongs to IMPLEMENT; TEST does not own TransitionType

Rule: TransitionType is assigned by the orchestrator based on the overall request intent.
  - The IMPLEMENT predicate is the primary driver of TransitionType:
    - feature_addition: "add", "write", "create", "build", "develop", "implement", "extend"
    - refactoring: "refactor", "extract", "restructure", "reorganise", "rename"
    - documentation: "document", "spec", "describe" (when no IMPLEMENT construction verbs are present)
  - TEST predicate does NOT own TransitionType. It contributes sub-phase routing (5.1) but
    not the TransitionType classification.
  - A single TransitionType applies to the whole request regardless of how many predicates fire.

---

## T024-003 — Confirming "run" produces no TransitionType (canonical boundary pair)

A-run: "Run unit tests for the BillingService."
  CompiledPredicateIds: [] | TransitionType: unknown | SubPhaseId: null → knowledge_edge

A-write: "Write unit tests for the BillingService."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition | SubPhaseId: 5.1

The difference is entirely in the action verb. Same object ("unit tests for the BillingService"),
opposite outcomes. This is the definitive "run ≠ TEST" TransitionType boundary illustration.
