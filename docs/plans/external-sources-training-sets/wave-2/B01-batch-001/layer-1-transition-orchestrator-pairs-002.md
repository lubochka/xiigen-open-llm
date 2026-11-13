# Layer 1 — Transition Orchestrator Pairs (002)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B01-L1-002-001 — UI alone: feature_addition TransitionType

```
source_id: xiigen-b01-l1-002-001
source_title: XIIGen B01 layer1-002 — UI alone produces feature_addition transition
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
  run_id: "B01-l1-002-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Create an inventory of all pages in the admin interface and list each page's purpose and navigation."
  CompiledPredicateIds: ["UI"]
  PredictedSubPhaseId: "1.1"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  TransitionId: "transition-B01-l1-002-001-1.1-feature_addition"
  TransitionType: "feature_addition"
  ScenarioId: "adaptive-logical-orchestration"
  PhaseId: "1"
  SubPhaseId: "1.1"
  DomainId: "software-development"
  UserRequest: "Create an inventory of all pages in the admin interface and list each page's purpose and navigation."
  PredicateIds: ["UI"]
  ContextCarryForward: true
  EvidenceIds: ["B01-0a-002-001-prompt"]
  Metadata:
    transition_classification_basis: "add_new_capability_signal: page_inventory, admin_interface"
    primary_intent: "create"
    domain_signal: "ux_screen_mapping"

Transition rationale:
  "Create an inventory" signals the addition of a new UX artifact. No existing
  inventory is referenced; this is net-new UX work. TransitionType: feature_addition.
  UI predicate owns Phase 1; sub-phase 1.1 is screen inventory and flow mapping.

---

## B01-L1-002-002 — AdminPanelScreenInventory: feature_addition

```
source_id: xiigen-b01-l1-002-002
source_title: XIIGen B01 layer1-002 — AdminPanelScreenInventory transition
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
  run_id: "B01-l1-002-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Map every page in the admin panel and document each page's interaction model, entry points, and screen layout."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  PredictedSubPhaseId: "1.1"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  TransitionId: "transition-B01-l1-002-002-1.1-feature_addition"
  TransitionType: "feature_addition"
  ScenarioId: "adaptive-logical-orchestration"
  PhaseId: "1"
  SubPhaseId: "1.1"
  DomainId: "software-development"
  UserRequest: "Map every page in the admin panel and document each page's interaction model, entry points, and screen layout."
  PredicateIds: ["UI", "DOCUMENT"]
  ContextCarryForward: true
  EvidenceIds: ["B01-0a-002-002-prompt"]
  Metadata:
    transition_classification_basis: "add_new_capability_signal: page_map, interaction_model"
    primary_intent: "create"
    domain_signal: "ux_screen_mapping"

Transition rationale:
  UI present → feature_addition. DOCUMENT also fires but UI is the primary driver
  and its presence ensures feature_addition (not documentation). Primary sub-phase
  is 1.1; secondary routing to 2.1 follows in Layer 0-B.

---

## B01-L1-002-003 — UI alone: feature_addition TransitionType rule

```
source_id: xiigen-b01-l1-002-003
source_title: XIIGen B01 layer1-002 — UI alone feature_addition rule annotation
source_license_state: first_party
pair_kind: transition_orchestrator
coverage_status: covered
```

Rule annotation — UI alone TransitionType:

  When PredicateIds: ["UI"] and no IMPLEMENT is present:
    TransitionType: feature_addition

  Rationale: Creating a screen inventory, wireframe, or UX flow document is
  adding a new product capability. The product does not have this artifact
  yet; producing it is feature-level work. Therefore feature_addition applies
  even without an IMPLEMENT predicate.

  This contrasts with DOCUMENT alone (documentation) where no new product
  capability is added — only a specification or use-case document.

  Example contrast:
    [UI] → feature_addition (new screen map created)
    [DOCUMENT] → documentation (requirements document produced)
    [UI, DOCUMENT] → feature_addition (UI drives it; new artifacts at multiple sub-phases)
