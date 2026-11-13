# Layer 1 — Transition Orchestrator Pairs
# Batch: B01-batch-001
# Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Contract: TransitionOrchestratorUnit : ILogicalUnit<LogicalOrchestratorRequest, SelectedLogicalTransition>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: transition_orchestrator

---

## Method note

Layer 1 pairs teach the transition orchestrator contract. The A side is the
full LogicalOrchestratorRequest (the raw user request plus flags). The B side
is the SelectedLogicalTransition — the classified transition that the nine-node
flow will receive as its Node 01 input.

Transition types used in this batch:
  feature_addition — new capability being added (new screen inventory, new flows)
  documentation    — producing specifications or documentation without implementation

The TransitionOrchestratorUnit classifies the intent, selects the sub-phase,
and produces the SelectedLogicalTransition that drives the nine-node flow.

EM-9 guard: The Layer 2 Node 01 A side MUST be this SelectedLogicalTransition
object — never a raw user_request string.

---

## Pair B01B1-WDA-T1-001 — feature_addition: new screen inventory for SaaS onboarding

```
source_id: xiigen-b01-t1-001
source_title: XIIGen B01 Layer 1 — feature_addition: SaaS onboarding screen inventory
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
  run_id: "B01-t1-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "We need to map all screen flows and create a complete screen inventory for the SaaS onboarding UX, covering signup, email activation, and the first-run experience."
  CompiledPredicateIds: ["UI"]
  PredictedSubPhaseId: "1.1"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  TransitionId: "transition-B01-t1-001-1.1-feature_addition"
  TransitionType: "feature_addition"
  ScenarioId: "adaptive-logical-orchestration"
  PhaseId: "1"
  SubPhaseId: "1.1"
  DomainId: "software-development"
  UserRequest: "We need to map all screen flows and create a complete screen inventory for the SaaS onboarding UX, covering signup, email activation, and the first-run experience."
  PredicateIds: ["UI"]
  ContextCarryForward: true
  EvidenceIds: ["B01-compiler-001-prompt"]
  Metadata:
    transition_classification_basis: "add_new_capability_signal: screen_inventory, user_flows — no existing screen map referenced"
    primary_intent: "create"
    domain_signal: "ux_screen_mapping"

Transition rationale:
  "We need to map" and "create a complete screen inventory" signal the addition of
  a new artifact that does not exist yet. This is not a repair (nothing is broken)
  nor a refactor (no restructuring of existing code). TransitionType: feature_addition.
  PhaseId "1" and SubPhaseId "1.1" are selected because UI predicate owns Phase 1
  and sub-phase 1.1 is the screen inventory and user flow mapping sub-phase.

---

## Pair B01B1-WDA-T1-002 — documentation: checkout flow specification

```
source_id: xiigen-b01-t1-002
source_title: XIIGen B01 Layer 1 — documentation: checkout interface flow and page spec
source_license_state: first_party
contract_signature: TransitionOrchestratorUnit : ILogicalUnit<LogicalOrchestratorRequest, SelectedLogicalTransition>
transition_type: documentation
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: transition_orchestrator
```

A:
  run_id: "B01-t1-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Document the complete user interface flows for the checkout process and produce a page specification with form interaction details at each step."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  PredictedSubPhaseId: "1.1"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  TransitionId: "transition-B01-t1-002-1.1-documentation"
  TransitionType: "documentation"
  ScenarioId: "adaptive-logical-orchestration"
  PhaseId: "1"
  SubPhaseId: "1.1"
  DomainId: "software-development"
  UserRequest: "Document the complete user interface flows for the checkout process and produce a page specification with form interaction details at each step."
  PredicateIds: ["UI", "DOCUMENT"]
  ContextCarryForward: true
  EvidenceIds: ["B01-compiler-003-prompt"]
  Metadata:
    transition_classification_basis: "documentation_signal: 'document', 'specification' — primary intent is to produce a written artifact, not implement code"
    primary_intent: "document"
    domain_signal: "ui_flow_specification"

Transition rationale:
  "Document" and "produce a page specification" are the primary verbs — both
  signal documentation output rather than implementation. TransitionType: documentation.
  No implementation vocabulary is present (implement, code, build, develop, write[code]).
  The primary sub-phase remains 1.1 because the documentation subject is the user
  interface flows and screen structure (UI domain).
