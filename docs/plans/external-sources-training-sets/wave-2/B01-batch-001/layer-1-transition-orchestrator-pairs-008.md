# Layer 1 — Transition Orchestrator Pairs (008)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B01-L1-008-001 — Full four-predicate: UI+ARCHITECTURE+IMPLEMENT+DOCUMENT → feature_addition

```
source_id: xiigen-b01-l1-008-001
source_title: XIIGen B01 layer1-008 — four-predicate: feature_addition
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
  run_id: "B01-l1-008-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Design the architecture for the onboarding screen flow, implement the form components, document the user requirements, and map all screen interactions."
  CompiledPredicateIds: ["UI", "ARCHITECTURE", "IMPLEMENT", "DOCUMENT"]
  PredictedSubPhaseId: "1.1"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  TransitionId: "transition-B01-l1-008-001-1.1-feature_addition"
  TransitionType: "feature_addition"
  ScenarioId: "adaptive-logical-orchestration"
  PhaseId: "1"
  SubPhaseId: "1.1"
  DomainId: "software-development"
  UserRequest: "Design the architecture for the onboarding screen flow, implement the form components, document the user requirements, and map all screen interactions."
  PredicateIds: ["UI", "ARCHITECTURE", "IMPLEMENT", "DOCUMENT"]
  ContextCarryForward: true
  EvidenceIds: ["B01-0a-010-001-prompt"]
  Metadata:
    transition_classification_basis: "implement_signal: implement; ui_signal: screen, form, component, interaction; architecture_signal: design, architecture; document_signal: document, requirement"
    primary_intent: "build"
    domain_signal: "full_onboarding_ux_implementation"
    flow_execution_order: ["ScreenInventoryFlow11", "SystemArchitectureFlow31", "BusinessRequirementsFlow21", "ImplementationFlow41"]

Transition rationale:
  IMPLEMENT present → feature_addition. UI primary → SubPhaseId: "1.1". Four flows
  in priority order. All four predicates produce independent routing decisions and
  independent flow invocations.

---

## B01-L1-008-002 — Full four-predicate: ProductListing variant

```
source_id: xiigen-b01-l1-008-002
source_title: XIIGen B01 layer1-008 — four-predicate product listing variant
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
  run_id: "B01-l1-008-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Build the full product listing screen architecture, implement the responsive component library, write the functional specification for each page, and define the interaction model for all UI elements."
  CompiledPredicateIds: ["UI", "ARCHITECTURE", "IMPLEMENT", "DOCUMENT"]
  PredictedSubPhaseId: "1.1"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  TransitionId: "transition-B01-l1-008-002-1.1-feature_addition"
  TransitionType: "feature_addition"
  ScenarioId: "adaptive-logical-orchestration"
  PhaseId: "1"
  SubPhaseId: "1.1"
  DomainId: "software-development"
  UserRequest: "Build the full product listing screen architecture, implement the responsive component library, write the functional specification for each page, and define the interaction model for all UI elements."
  PredicateIds: ["UI", "ARCHITECTURE", "IMPLEMENT", "DOCUMENT"]
  ContextCarryForward: true
  EvidenceIds: ["B01-0a-010-002-prompt"]
  Metadata:
    transition_classification_basis: "implement_signal: build, implement, write; ui_signal: screen, responsive, component, page, interaction, ui; document_signal: functional, specification, define"
    primary_intent: "build"
    domain_signal: "product_listing_full_implementation"
    flow_execution_order: ["ScreenInventoryFlow11", "SystemArchitectureFlow31", "BusinessRequirementsFlow21", "ImplementationFlow41"]

---

## B01-L1-008-003 — Four-predicate SubPhaseId selection rule

```
source_id: xiigen-b01-l1-008-003
source_title: XIIGen B01 layer1-008 — four-predicate primary SubPhaseId selection
source_license_state: first_party
pair_kind: transition_orchestrator
coverage_status: covered
```

SubPhaseId selection for four-predicate transitions:

  When all four predicates fire [UI, ARCHITECTURE, IMPLEMENT, DOCUMENT]:
    SubPhaseId in SelectedLogicalTransition: "1.1" (UI is primary predicate)

  Hierarchy for SubPhaseId selection:
    UI present → 1.1 (overrides all others)
    UI absent, ARCHITECTURE present → 3.1
    UI + ARCHITECTURE absent, IMPLEMENT present → 4.1
    Only DOCUMENT → 2.1

  The SubPhaseId in the transition is the PRIMARY routing target. It determines
  which flow runs first and which flow's output is the primary artifact.

  Secondary routing decisions are tracked separately in the routing array.
  All four flows will ultimately run, but 1.1 is the "home" sub-phase for
  the transition when UI is in the predicate list.

  TransitionId naming convention:
    "transition-[batch]-[run_id]-[SubPhaseId]-[TransitionType]"
    Example: "transition-B01-l1-008-001-1.1-feature_addition"
