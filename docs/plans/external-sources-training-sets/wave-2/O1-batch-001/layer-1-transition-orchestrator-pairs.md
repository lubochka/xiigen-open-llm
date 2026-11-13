# Layer 1 — Transition Orchestrator Pairs
# Batch: O1-batch-001
# Scenario: O1 — multi-tenant JWT authentication
# Contract: TransitionOrchestratorUnit : ILogicalUnit<LogicalOrchestratorRequest, SelectedLogicalTransition>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: transition_orchestrator

---

## Method note

Layer 1 pairs teach the transition orchestrator to classify and select the
correct transition type from a routed orchestrator request. The transition
type shapes which nine-node flow template is invoked and what semantic intent
the Node 01 domain-splitter receives.

Two transition type outcomes are covered:

  Pair 001: feature_addition — a new capability is being added. The transition
    orchestrator classifies the request as feature_addition and produces a
    SelectedLogicalTransition routed to sub-phase 3.4. This is the primary
    path for the O1 scenario.

  Pair 002: repair — a defect correction is requested. The transition orchestrator
    classifies the request as repair. Because no repair-specific flow is registered
    for the matched sub-phase, the transition is marked as "unroutable_transition_type"
    and the orchestrator records a capability gap. The feature_addition nine-node
    flow is not invoked for repair requests — using it would be a EM-9 class failure.

The SelectedLogicalTransition fields required by Node 01 (DomainSplitterUnit):
  TransitionId, TransitionType, ScenarioId, PhaseId, SubPhaseId, DomainId,
  UserRequest, PredicateIds, ContextCarryForward, EvidenceIds, Metadata.

---

## Coverage map

| Pair ID | User request type | TransitionType | Outcome |
|---|---|---|---|
| O1B1-WDA-T1-001 | Add JWT auth (new capability) | feature_addition | Accepted → 3.4 |
| O1B1-WDA-T1-002 | Fix token expiry bug (defect) | repair | Unroutable; capability gap recorded |

---

## Pair O1B1-WDA-T1-001 — feature_addition transition selected for JWT auth

```
source_id: xiigen-o1-t1-001
source_title: XIIGen O1 scenario transition orchestrator — feature_addition selected for 3.4
source_license_state: first_party
contract_signature: TransitionOrchestratorUnit : ILogicalUnit<LogicalOrchestratorRequest, SelectedLogicalTransition>
transition_type: feature_addition
phase: 3
sub_phase: 3.4
task_type: transition_orchestration
node_role: transition-orchestrator
coverage_status: covered
pair_kind: transition_orchestrator
```

A:
  run_id: "O1-t1-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "We need to add multi-tenant JWT authentication to the API and make sure tenant isolation is tested."
  selected_sub_phase_id: "3.4"
  routing_confidence: 0.92
  evidence_predicates: ["SECURITY", "AUTHORIZATION"]
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true
    UseContextCarryForward: true
  metadata:
    source: "logical_orchestrator_assistant"
    scenario_selection_mode: "adaptive"

B:
  TransitionId: "transition-O1-t1-001-3.4-feature_addition"
  TransitionType: "feature_addition"
  ScenarioId: "adaptive-logical-orchestration"
  PhaseId: "3"
  SubPhaseId: "3.4"
  DomainId: "software-development"
  UserRequest: "We need to add multi-tenant JWT authentication to the API and make sure tenant isolation is tested."
  PredicateIds: ["SECURITY", "AUTHORIZATION"]
  ContextCarryForward: true
  EvidenceIds: ["O1-router-001-prompt"]
  Metadata:
    transition_classification_basis: "add_new_capability_signal: jwt, authentication, multi-tenant"
    routing_confidence: "0.92"
    transition_type_confidence: "0.91"
  [unit result envelope]
    UnitId: "unit-transition-orchestrator-v1"
    UnitRole: "transition-orchestrator"
    SubPhaseId: "3.4"
    Confidence: 0.91
    SelectionOutcome: "ACCEPTED_FEATURE_ADDITION"
    Warnings: []

Transition classification evidence:
  "add" signals new-capability intent (feature_addition pattern).
  "multi-tenant" is a scope qualifier indicating capability breadth, not a defect.
  "make sure ... is tested" is an acceptance-criteria qualifier, not a repair signal.
  No defect vocabulary present (fix, broken, regression, bug, error, broken, issue,
    incident, revert). Transition type is feature_addition at confidence 0.91.

---

## Pair O1B1-WDA-T1-002 — repair transition unroutable, capability gap recorded

```
source_id: xiigen-o1-t1-002
source_title: XIIGen O1 scenario transition orchestrator — repair type unroutable
source_license_state: first_party
contract_signature: TransitionOrchestratorUnit : ILogicalUnit<LogicalOrchestratorRequest, SelectedLogicalTransition>
transition_type: repair
phase: 3
sub_phase: 3.4
task_type: transition_orchestration
node_role: transition-orchestrator
coverage_status: covered
pair_kind: transition_orchestrator
```

A:
  run_id: "O1-t1-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "The authentication token expiry time is too long and users are being logged out unexpectedly — we need to fix the expiry logic."
  selected_sub_phase_id: "3.4"
  routing_confidence: 0.92
  evidence_predicates: ["SECURITY", "AUTHORIZATION"]
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true
  metadata:
    source: "logical_orchestrator_assistant"
    scenario_selection_mode: "adaptive"

B:
  TransitionId: "transition-O1-t1-002-3.4-repair"
  TransitionType: "repair"
  ScenarioId: "adaptive-logical-orchestration"
  PhaseId: "3"
  SubPhaseId: "3.4"
  DomainId: "software-development"
  UserRequest: "The authentication token expiry time is too long and users are being logged out unexpectedly — we need to fix the expiry logic."
  PredicateIds: ["SECURITY", "AUTHORIZATION"]
  ContextCarryForward: false
  EvidenceIds: []
  Metadata:
    transition_classification_basis: "defect_signal: fix, logged out unexpectedly, too long"
    routing_confidence: "0.92"
    transition_type_confidence: "0.88"
    unroutable_reason: "no_repair_flow_registered_for_3.4"
  [unit result envelope]
    UnitId: "unit-transition-orchestrator-v1"
    UnitRole: "transition-orchestrator"
    SubPhaseId: "3.4"
    Confidence: 0.88
    SelectionOutcome: "UNROUTABLE_TRANSITION_TYPE"
    Warnings: ["repair_flow_not_registered:3.4", "capability_gap_recorded:repair-security-verification-3.4"]

Transition classification evidence:
  "fix" is a strong defect-correction signal → repair transition type.
  "logged out unexpectedly" describes an existing defect in production behavior.
  "too long" describes a misconfiguration of an existing capability.
  No new-capability vocabulary present (add, implement, introduce, create, enable).
  Transition type is repair at confidence 0.88.

Unroutable rationale:
  The flow registry for sub-phase 3.4 contains only SecurityVerification34FlowAdapter,
  which is registered for the feature_addition transition type. No repair-specific flow
  is registered for 3.4. The transition orchestrator records a capability gap rather
  than routing the repair request through a feature_addition flow — doing so would
  misclassify the task and produce incorrect node outputs. The capability gap entry
  (repair-security-verification-3.4) is scheduled for learning-loop resolution.
