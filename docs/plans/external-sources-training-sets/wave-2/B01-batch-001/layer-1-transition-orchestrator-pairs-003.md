# Layer 1 — Transition Orchestrator Pairs (003)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B01-L1-003-001 — UI + DOCUMENT: feature_addition (UI drives TransitionType)

```
source_id: xiigen-b01-l1-003-001
source_title: XIIGen B01 layer1-003 — UI+DOCUMENT: feature_addition
source_license_state: first_party
contract_signature: TransitionOrchestratorUnit : ILogicalUnit<LogicalOrchestratorRequest, SelectedLogicalTransition>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: transition_orchestrator
```

A:
  run_id: "B01-l1-003-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Document the complete user interface flows for the checkout process and produce a page specification."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  PredictedSubPhaseId: "1.1"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  TransitionId: "transition-B01-l1-003-001-1.1-feature_addition"
  TransitionType: "feature_addition"
  ScenarioId: "adaptive-logical-orchestration"
  PhaseId: "1"
  SubPhaseId: "1.1"
  DomainId: "software-development"
  UserRequest: "Document the complete user interface flows for the checkout process and produce a page specification."
  PredicateIds: ["UI", "DOCUMENT"]
  ContextCarryForward: true
  EvidenceIds: ["B01-0a-004-001-prompt"]
  Metadata:
    transition_classification_basis: "add_new_capability_signal: interface_flow_document"
    primary_intent: "create"
    domain_signal: "ux_flow_documentation"

Transition rationale:
  UI + DOCUMENT → feature_addition. The presence of UI means a new UX flow document
  is being created (a new product artifact). The DOCUMENT predicate does not change
  the TransitionType from feature_addition; only the absence of UI and IMPLEMENT would
  shift it to documentation. Primary sub-phase: 1.1. Secondary: 2.1.

---

## B01-L1-003-002 — CheckoutInterfaceFlow: UI+DOCUMENT feature_addition

```
source_id: xiigen-b01-l1-003-002
source_title: XIIGen B01 layer1-003 — CheckoutInterfaceFlow transition
source_license_state: first_party
contract_signature: TransitionOrchestratorUnit : ILogicalUnit<LogicalOrchestratorRequest, SelectedLogicalTransition>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: transition_orchestrator
```

A:
  run_id: "B01-l1-003-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Map the checkout form interactions and define the requirements for each input field's validation."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  PredictedSubPhaseId: "1.1"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  TransitionId: "transition-B01-l1-003-002-1.1-feature_addition"
  TransitionType: "feature_addition"
  ScenarioId: "adaptive-logical-orchestration"
  PhaseId: "1"
  SubPhaseId: "1.1"
  DomainId: "software-development"
  UserRequest: "Map the checkout form interactions and define the requirements for each input field's validation."
  PredicateIds: ["UI", "DOCUMENT"]
  ContextCarryForward: true
  EvidenceIds: ["B01-0a-003-001-prompt"]
  Metadata:
    transition_classification_basis: "add_new_capability_signal: form_interaction_map"
    primary_intent: "create"
    domain_signal: "ux_form_mapping"

---

## B01-L1-003-003 — When DOCUMENT alone → documentation TransitionType (no UI)

```
source_id: xiigen-b01-l1-003-003
source_title: XIIGen B01 layer1-003 — DOCUMENT alone: documentation TransitionType
source_license_state: first_party
contract_signature: TransitionOrchestratorUnit : ILogicalUnit<LogicalOrchestratorRequest, SelectedLogicalTransition>
transition_type: documentation
phase: 2
sub_phase: 2.1
task_type: business-requirements
coverage_status: covered
pair_kind: transition_orchestrator
```

A:
  run_id: "B01-l1-003-003"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Describe the functional behavior of the onboarding wizard and document the user journey requirements."
  CompiledPredicateIds: ["DOCUMENT"]
  PredictedSubPhaseId: "2.1"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  TransitionId: "transition-B01-l1-003-003-2.1-documentation"
  TransitionType: "documentation"
  ScenarioId: "adaptive-logical-orchestration"
  PhaseId: "2"
  SubPhaseId: "2.1"
  DomainId: "software-development"
  UserRequest: "Describe the functional behavior of the onboarding wizard and document the user journey requirements."
  PredicateIds: ["DOCUMENT"]
  ContextCarryForward: true
  EvidenceIds: ["B01-0a-005-001-prompt"]
  Metadata:
    transition_classification_basis: "documentation_signal: describe, functional, behavior, document, requirements"
    primary_intent: "document"
    domain_signal: "requirements_specification"

Transition rationale:
  DOCUMENT alone, no UI or IMPLEMENT → documentation TransitionType. Primary
  sub-phase is 2.1 (Business Requirements). No 1.1 routing because no UI fired.
  The output is a requirements document, not a screen inventory.
