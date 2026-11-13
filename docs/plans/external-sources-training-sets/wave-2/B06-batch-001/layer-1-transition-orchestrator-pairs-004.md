# Layer 1 — Transition Orchestrator Pairs (004)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## Method note

File 004 covers DOCUMENT + ARCHITECTURE → documentation TransitionType (no IMPLEMENT).

---

## B06-T1-004-001 — DOCUMENT+ARCHITECTURE: documentation TransitionType

```
source_id: xiigen-b06-t1-004-001
source_title: XIIGen B06 T1 — DOCUMENT+ARCHITECTURE documentation
source_license_state: first_party
contract_signature: TransitionOrchestratorUnit : ILogicalUnit<LogicalOrchestratorRequest, SelectedLogicalTransition>
transition_type: documentation
phase: 2
sub_phase: 2.1
pair_kind: transition_orchestrator
```

A:
  run_id: "B06-t1-004-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Design the architecture for the checkout service and document the functional requirements."
  CompiledPredicateIds: ["ARCHITECTURE", "DOCUMENT"]
  RoutingDecisions:
    - PredictedSubPhaseId: "3.1", EvidencePredicates: ["ARCHITECTURE"]
    - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"]
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  Transitions:
    - TransitionId: "transition-B06-t1-004-001a-3.1"
      TransitionType: "documentation"
      PhaseId: "3"
      SubPhaseId: "3.1"
      DomainId: "software-development"
      PredicateIds: ["ARCHITECTURE"]
      Metadata:
        transition_classification_basis: "ARCHITECTURE_present_no_IMPLEMENT: design + architecture verbs; documentation TransitionType"
        primary_intent: "design"
        domain_signal: "architecture_authoring"

    - TransitionId: "transition-B06-t1-004-001b-2.1"
      TransitionType: "documentation"
      PhaseId: "2"
      SubPhaseId: "2.1"
      DomainId: "software-development"
      PredicateIds: ["DOCUMENT"]
      Metadata:
        transition_classification_basis: "DOCUMENT_only_no_IMPLEMENT: 'document the functional requirements' — output is a documentation artifact"
        primary_intent: "document"
        domain_signal: "requirements_authoring"

  TransitionCount: 2
  GlobalTransitionType: "documentation"

Rationale:
  No IMPLEMENT or UI predicates → GlobalTransitionType: documentation.
  Both 3.1 and 2.1 transitions are documentation.
  "design" fires ARCHITECTURE; "document" fires DOCUMENT; neither fires IMPLEMENT.

---

## B06-T1-004-002 — ARCHITECTURE alone → documentation TransitionType

```
source_id: xiigen-b06-t1-004-002
source_title: XIIGen B06 T1 — ARCHITECTURE alone documentation
source_license_state: first_party
contract_signature: TransitionOrchestratorUnit : ILogicalUnit<LogicalOrchestratorRequest, SelectedLogicalTransition>
transition_type: documentation
phase: 3
sub_phase: 3.1
pair_kind: transition_orchestrator
```

A:
  run_id: "B06-t1-004-002"
  user_request: "Design the technical architecture for the checkout microservice."
  CompiledPredicateIds: ["ARCHITECTURE"]
  PredictedSubPhaseId: "3.1"

B:
  TransitionId: "transition-B06-t1-004-002-3.1-documentation"
  TransitionType: "documentation"
  SubPhaseId: "3.1"
  DomainId: "software-development"
  PredicateIds: ["ARCHITECTURE"]
  Metadata:
    transition_classification_basis: "ARCHITECTURE_only: design + architecture → documentation TransitionType"

---

## B06-T1-004-003 — ARCHITECTURE + IMPLEMENT → feature_addition

```
source_id: xiigen-b06-t1-004-003
source_title: XIIGen B06 T1 — ARCHITECTURE+IMPLEMENT feature_addition
source_license_state: first_party
contract_signature: TransitionOrchestratorUnit : ILogicalUnit<LogicalOrchestratorRequest, SelectedLogicalTransition>
transition_type: feature_addition
phase: 3
sub_phase: 3.1
pair_kind: transition_orchestrator
```

A:
  run_id: "B06-t1-004-003"
  user_request: "Design the checkout architecture and implement the checkout service."
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT"]
  RoutingDecisions:
    - PredictedSubPhaseId: "3.1"
    - PredictedSubPhaseId: "4.1"

B:
  GlobalTransitionType: "feature_addition"
  Transitions:
    - SubPhaseId: "3.1", TransitionType: "feature_addition", PredicateIds: ["ARCHITECTURE"]
    - SubPhaseId: "4.1", TransitionType: "feature_addition", PredicateIds: ["IMPLEMENT"]

Rationale:
  IMPLEMENT present → feature_addition for all transitions.
  Even ARCHITECTURE transition is feature_addition because IMPLEMENT fires globally.
