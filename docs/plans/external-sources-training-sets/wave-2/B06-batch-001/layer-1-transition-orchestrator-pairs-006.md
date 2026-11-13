# Layer 1 — Transition Orchestrator Pairs (006)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## Method note

File 006 covers the full four-predicate orchestration: ARCHITECTURE+IMPLEMENT+DOCUMENT+UI
→ feature_addition with four transitions.

---

## B06-T1-006-001 — Four-predicate orchestration: four transitions, feature_addition

```
source_id: xiigen-b06-t1-006-001
source_title: XIIGen B06 T1 — four predicate four transitions
source_license_state: first_party
contract_signature: TransitionOrchestratorUnit : ILogicalUnit<LogicalOrchestratorRequest, SelectedLogicalTransition>
transition_type: feature_addition
phase: 2
sub_phase: 2.1
pair_kind: transition_orchestrator
```

A:
  run_id: "B06-t1-006-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Design the architecture, write the requirements spec, implement the checkout service, and document the UI flow."
  CompiledPredicateIds: ["ARCHITECTURE", "DOCUMENT", "IMPLEMENT", "UI"]
  RoutingDecisions:
    - PredictedSubPhaseId: "3.1", EvidencePredicates: ["ARCHITECTURE"]
    - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"]
    - PredictedSubPhaseId: "4.1", EvidencePredicates: ["IMPLEMENT"]
    - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"]

B:
  Transitions:
    - TransitionId: "transition-B06-t1-006-001a-3.1"
      TransitionType: "feature_addition"
      SubPhaseId: "3.1"
      DomainId: "software-development"
      PredicateIds: ["ARCHITECTURE"]
      Metadata:
        primary_intent: "design"
        domain_signal: "architecture_design"

    - TransitionId: "transition-B06-t1-006-001b-2.1"
      TransitionType: "feature_addition"
      SubPhaseId: "2.1"
      DomainId: "software-development"
      PredicateIds: ["DOCUMENT"]
      Metadata:
        primary_intent: "document"
        domain_signal: "requirements_authoring"

    - TransitionId: "transition-B06-t1-006-001c-4.1"
      TransitionType: "feature_addition"
      SubPhaseId: "4.1"
      DomainId: "software-development"
      PredicateIds: ["IMPLEMENT"]
      Metadata:
        primary_intent: "implement"
        domain_signal: "backend_implementation"

    - TransitionId: "transition-B06-t1-006-001d-1.1"
      TransitionType: "feature_addition"
      SubPhaseId: "1.1"
      DomainId: "software-development"
      PredicateIds: ["UI"]
      Metadata:
        primary_intent: "design_ux"
        domain_signal: "ui_requirements"

  TransitionCount: 4
  GlobalTransitionType: "feature_addition"

Rationale:
  IMPLEMENT and UI both present → feature_addition globally.
  All four transitions carry feature_addition TransitionType.
  DomainId: software-development on all transitions (Rule 17).

---

## B06-T1-006-002 — DomainId is always software-development: Rule 17

```
source_id: xiigen-b06-t1-006-002
source_title: XIIGen B06 T1 — DomainId always software-development
source_license_state: first_party
pair_kind: transition_orchestrator
```

Teaching note — DomainId Rule 17:

  B06 Core Rule 17: DomainId: software-development always.
  All B06 transitions carry DomainId: "software-development".

  DomainId is not derived from predicate analysis — it is a fixed property
  of the B06 scenario. The checkout flow business requirements scenario
  is always in the software-development domain.

  Regardless of how many predicates fire and how many transitions are produced,
  every transition in every B06 orchestration run carries:
    DomainId: "software-development"

  Other domains (e.g., healthcare, finance) would have different DomainId values
  but that is outside B06 scope.
