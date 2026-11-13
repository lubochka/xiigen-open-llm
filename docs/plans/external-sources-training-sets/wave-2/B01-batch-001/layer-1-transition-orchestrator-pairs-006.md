# Layer 1 — Transition Orchestrator Pairs (006)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B01-L1-006-001 — UI + DOCUMENT + IMPLEMENT: feature_addition; flows ordered

```
source_id: xiigen-b01-l1-006-001
source_title: XIIGen B01 layer1-006 — UI+DOCUMENT+IMPLEMENT: feature_addition, ordered flows
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
  run_id: "B01-l1-006-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Write the screen specifications for the user dashboard and implement the responsive layout components."
  CompiledPredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]
  PredictedSubPhaseId: "1.1"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  TransitionId: "transition-B01-l1-006-001-1.1-feature_addition"
  TransitionType: "feature_addition"
  ScenarioId: "adaptive-logical-orchestration"
  PhaseId: "1"
  SubPhaseId: "1.1"
  DomainId: "software-development"
  UserRequest: "Write the screen specifications for the user dashboard and implement the responsive layout components."
  PredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]
  ContextCarryForward: true
  EvidenceIds: ["B01-0a-006-001-prompt"]
  Metadata:
    transition_classification_basis: "implement_signal: write, implement; ui_signal: screen, responsive, layout, component; document_signal: specification"
    primary_intent: "build"
    domain_signal: "dashboard_screen_implementation"
    flow_execution_order: ["ScreenInventoryFlow11", "BusinessRequirementsFlow21", "ImplementationFlow41"]

Transition rationale:
  Three predicates → IMPLEMENT present → feature_addition. UI primary → SubPhaseId 1.1.
  Flow execution order listed in Metadata for downstream orchestrator. Flows are
  ordered: Screen (1.1) → Requirements (2.1) → Implementation (4.1).

---

## B01-L1-006-002 — OnboardingWizardFlow: UI+DOCUMENT feature_addition

```
source_id: xiigen-b01-l1-006-002
source_title: XIIGen B01 layer1-006 — OnboardingWizardFlow transition
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
  run_id: "B01-l1-006-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Map the five-step onboarding wizard screens and define the requirements for each step."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  PredictedSubPhaseId: "1.1"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  TransitionId: "transition-B01-l1-006-002-1.1-feature_addition"
  TransitionType: "feature_addition"
  ScenarioId: "adaptive-logical-orchestration"
  PhaseId: "1"
  SubPhaseId: "1.1"
  DomainId: "software-development"
  UserRequest: "Map the five-step onboarding wizard screens and define the requirements for each step."
  PredicateIds: ["UI", "DOCUMENT"]
  ContextCarryForward: true
  EvidenceIds: ["B01-0a-004-003-prompt"]
  Metadata:
    transition_classification_basis: "ui_signal: screen; document_signal: define, requirement"
    primary_intent: "create"
    domain_signal: "onboarding_wizard_screen_mapping"
    flow_execution_order: ["ScreenInventoryFlow11", "BusinessRequirementsFlow21"]

---

## B01-L1-006-003 — Flow execution order metadata

```
source_id: xiigen-b01-l1-006-003
source_title: XIIGen B01 layer1-006 — flow execution order in Metadata
source_license_state: first_party
pair_kind: transition_orchestrator
coverage_status: covered
```

Flow execution order in SelectedLogicalTransition Metadata:

  The flow_execution_order field in Metadata is optional but recommended for
  multi-predicate transitions. It lists flows in the order they will be invoked:

  Standard ordering:
    1. ScreenInventoryFlow11 (if 1.1 in routing decisions)
    2. SystemArchitectureFlow31 (if 3.1 in routing decisions)
    3. BusinessRequirementsFlow21 (if 2.1 in routing decisions)
    4. ImplementationFlow41 (if 4.1 in routing decisions)

  This ordering is deterministic. Earlier flows produce context (e.g., screen IDs,
  component specs) that later flows can reference. Implementation flows benefit from
  having screen inventory and architecture context available before they run.

  The flow_execution_order field:
    - Is a string array of FlowIds
    - Is ordered by sub-phase priority (1 → 3 → 2 → 4)
    - Contains exactly the flows that will be invoked, no more, no less
    - Should be validated: count(flow_execution_order) == TotalFlowsSelected
