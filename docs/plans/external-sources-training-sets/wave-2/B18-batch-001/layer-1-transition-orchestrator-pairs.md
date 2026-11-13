# Layer 1 — Transition Orchestrator Pairs
# Batch: B18-batch-001
# Scenario: B18 — Development — refactoring and data migration
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: transition_orchestrator

---

## Pair B18B1-WDA-T1-001 — refactoring transition type

```
source_id: xiigen-b18-t1-001
transition_type: refactoring
phase: 4
sub_phase: 4.1
pair_kind: transition_orchestrator
```

A:
  user_request: "Refactor the OrderService class to extract the payment logic into a separate PaymentProcessor module."
  CompiledPredicateIds: ["IMPLEMENT"]
  PredictedSubPhaseId: "4.1"

B:
  TransitionId: "transition-B18-t1-001-4.1-refactoring"
  TransitionType: "refactoring"
  PhaseId: "4"
  SubPhaseId: "4.1"
  DomainId: "software-development"
  PredicateIds: ["IMPLEMENT"]
  ContextCarryForward: true
  Metadata:
    transition_classification_basis: "IMPLEMENT (refactor, class, extract, logic, module) — 'refactor' keyword signals refactoring TransitionType (not feature_addition); existing code being restructured"
    primary_intent: "restructure"
    domain_signal: "code_refactoring"

Teaching: TransitionType "refactoring" applies when "refactor", "extract", "rename",
"restructure", "consolidate" are the dominant action words and no new feature is being
added. Compare with "feature_addition" where new functionality is created.

---

## Pair B18B1-WDA-T1-002 — feature_addition: data migration (new migration created)

```
source_id: xiigen-b18-t1-002
transition_type: feature_addition
phase: 4
sub_phase: 4.4
pair_kind: transition_orchestrator
```

A:
  user_request: "Write the database migration to move customer data from the legacy schema to the new normalized structure."
  CompiledPredicateIds: ["IMPLEMENT", "ARCHITECTURE"]
  PredictedSubPhaseId: "4.4"

B:
  TransitionId: "transition-B18-t1-002-4.4-feature_addition"
  TransitionType: "feature_addition"
  PhaseId: "4"
  SubPhaseId: "4.4"
  DomainId: "software-development"
  PredicateIds: ["IMPLEMENT", "ARCHITECTURE"]
  ContextCarryForward: true
  Metadata:
    transition_classification_basis: "IMPLEMENT (write, migration) + ARCHITECTURE (structure) → 4.4; data migration is classified as feature_addition (new migration artifact created, not refactoring existing code)"
    primary_intent: "create"
    domain_signal: "data_migration"
