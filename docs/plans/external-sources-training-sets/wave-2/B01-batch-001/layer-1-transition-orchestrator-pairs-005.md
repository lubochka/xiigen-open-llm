# Layer 1 — Transition Orchestrator Pairs (005)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B01-L1-005-001 — UI + IMPLEMENT: feature_addition

```
source_id: xiigen-b01-l1-005-001
source_title: XIIGen B01 layer1-005 — UI+IMPLEMENT: feature_addition transition
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
  run_id: "B01-l1-005-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Build interactive prototype screens for the onboarding wizard and create the component library."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  PredictedSubPhaseId: "1.1"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  TransitionId: "transition-B01-l1-005-001-1.1-feature_addition"
  TransitionType: "feature_addition"
  ScenarioId: "adaptive-logical-orchestration"
  PhaseId: "1"
  SubPhaseId: "1.1"
  DomainId: "software-development"
  UserRequest: "Build interactive prototype screens for the onboarding wizard and create the component library."
  PredicateIds: ["UI", "IMPLEMENT"]
  ContextCarryForward: true
  EvidenceIds: ["B01-0a-007-002-prompt"]
  Metadata:
    transition_classification_basis: "implement_signal: build, create; ui_signal: prototype, screen, component"
    primary_intent: "build"
    domain_signal: "ux_component_implementation"

Transition rationale:
  IMPLEMENT present → feature_addition. UI present → SubPhaseId: "1.1" (UI is primary
  predicate and owns Phase 1). Routing decisions: 1.1 (UI) + 4.1 (IMPLEMENT).

---

## B01-L1-005-002 — MobileResponsiveScreens: UI+IMPLEMENT feature_addition

```
source_id: xiigen-b01-l1-005-002
source_title: XIIGen B01 layer1-005 — MobileResponsiveScreens transition
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
  run_id: "B01-l1-005-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Create responsive screen designs for the mobile onboarding flow and implement the layout grid."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  PredictedSubPhaseId: "1.1"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  TransitionId: "transition-B01-l1-005-002-1.1-feature_addition"
  TransitionType: "feature_addition"
  ScenarioId: "adaptive-logical-orchestration"
  PhaseId: "1"
  SubPhaseId: "1.1"
  DomainId: "software-development"
  UserRequest: "Create responsive screen designs for the mobile onboarding flow and implement the layout grid."
  PredicateIds: ["UI", "IMPLEMENT"]
  ContextCarryForward: true
  EvidenceIds: ["B01-0a-006-003-prompt"]
  Metadata:
    transition_classification_basis: "implement_signal: create, implement; ui_signal: responsive, screen, layout"
    primary_intent: "create"
    domain_signal: "responsive_screen_design"

---

## B01-L1-005-003 — TransitionType hierarchy rule

```
source_id: xiigen-b01-l1-005-003
source_title: XIIGen B01 layer1-005 — TransitionType hierarchy rule
source_license_state: first_party
pair_kind: transition_orchestrator
coverage_status: covered
```

TransitionType hierarchy rule:

  1. If IMPLEMENT in PredicateIds → TransitionType: feature_addition (always)
  2. If UI in PredicateIds (no IMPLEMENT) → TransitionType: feature_addition
  3. If ARCHITECTURE in PredicateIds (no IMPLEMENT, no UI) → TransitionType: documentation
  4. If DOCUMENT in PredicateIds (no IMPLEMENT, no UI, no ARCHITECTURE) → TransitionType: documentation
  5. If PredicateIds is empty → TransitionType: unknown (knowledge-edge)

  Priority order: IMPLEMENT > UI > ARCHITECTURE > DOCUMENT

  The hierarchy means:
    [IMPLEMENT, DOCUMENT] → feature_addition (IMPLEMENT wins)
    [ARCHITECTURE, UI] → documentation (no IMPLEMENT; UI does not override ARCHITECTURE
      in TransitionType — ARCHITECTURE+UI without IMPLEMENT = documentation)
    Wait — correction: UI alone = feature_addition; ARCHITECTURE+UI = ???

  Revised rule for ARCHITECTURE+UI:
    [UI, ARCHITECTURE] without IMPLEMENT → documentation
    Rationale: When the intent is to "design" an architecture involving UI (e.g.,
    "design the architecture for the screen routing system"), the primary output
    is an architecture diagram/document — not a new product feature. The "UI" signals
    what the architecture is about, not that implementation is happening.

  This is the B01-specific rule for ARCHITECTURE+UI combinations.
