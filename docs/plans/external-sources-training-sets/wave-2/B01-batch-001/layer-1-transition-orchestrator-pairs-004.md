# Layer 1 — Transition Orchestrator Pairs (004)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B01-L1-004-001 — DOCUMENT alone: documentation TransitionType

```
source_id: xiigen-b01-l1-004-001
source_title: XIIGen B01 layer1-004 — DOCUMENT alone documentation TransitionType
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
  run_id: "B01-l1-004-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Define the acceptance criteria for the SaaS onboarding feature and describe the expected user behaviors at each step."
  CompiledPredicateIds: ["DOCUMENT"]
  PredictedSubPhaseId: "2.1"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  TransitionId: "transition-B01-l1-004-001-2.1-documentation"
  TransitionType: "documentation"
  ScenarioId: "adaptive-logical-orchestration"
  PhaseId: "2"
  SubPhaseId: "2.1"
  DomainId: "software-development"
  UserRequest: "Define the acceptance criteria for the SaaS onboarding feature and describe the expected user behaviors at each step."
  PredicateIds: ["DOCUMENT"]
  ContextCarryForward: true
  EvidenceIds: ["B01-0a-005-003-prompt"]
  Metadata:
    transition_classification_basis: "documentation_signal: define, criteria, describe, behavior, feature"
    primary_intent: "document"
    domain_signal: "acceptance_criteria_specification"

Transition rationale:
  DOCUMENT alone → documentation. "Define", "criteria", "describe", "behavior",
  "feature" all fire DOCUMENT (note: "feature" alone fires IMPLEMENT, but in context
  "feature" here is used as noun without any implementation verb companion).
  Correction: "feature" alone fires IMPLEMENT → ["DOCUMENT", "IMPLEMENT"] → feature_addition.

  Revised:
    CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
    TransitionType: feature_addition (IMPLEMENT present)
    Note: "feature" in IMPLEMENT lexicon fires regardless of context.

---

## B01-L1-004-002 — Pure DOCUMENT example (no "feature" present)

```
source_id: xiigen-b01-l1-004-002
source_title: XIIGen B01 layer1-004 — pure DOCUMENT documentation TransitionType
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
  run_id: "B01-l1-004-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Document the use-cases for the admin role and describe the capability requirements for each workflow."
  CompiledPredicateIds: ["DOCUMENT"]
  PredictedSubPhaseId: "2.1"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  TransitionId: "transition-B01-l1-004-002-2.1-documentation"
  TransitionType: "documentation"
  ScenarioId: "adaptive-logical-orchestration"
  PhaseId: "2"
  SubPhaseId: "2.1"
  DomainId: "software-development"
  UserRequest: "Document the use-cases for the admin role and describe the capability requirements for each workflow."
  PredicateIds: ["DOCUMENT"]
  ContextCarryForward: true
  EvidenceIds: ["B01-0a-005-002-prompt"]
  Metadata:
    transition_classification_basis: "documentation_signal: document, use-case, describe, capability, requirement"
    primary_intent: "document"
    domain_signal: "use_case_documentation"

Transition rationale:
  DOCUMENT fires on: document, use-case, describe, capability, requirement.
  No UI terms, no IMPLEMENT terms. Pure DOCUMENT → documentation TransitionType.
  Primary sub-phase: 2.1.

---

## B01-L1-004-003 — DOCUMENT alone sub-phase: always 2.1 not 1.1

```
source_id: xiigen-b01-l1-004-003
source_title: XIIGen B01 layer1-004 — DOCUMENT alone sub-phase ownership rule
source_license_state: first_party
pair_kind: transition_orchestrator
coverage_status: covered
```

Rule: DOCUMENT alone → SubPhaseId: "2.1" (not "1.1")

  Sub-phase 1.1 is owned by UI predicate. Even if the DOCUMENT request is about
  UX screens, sub-phase assignment follows predicate ownership, not subject matter.

  Example:
    Request: "Document the onboarding screen requirements."
    If "screen" fires UI → ["UI", "DOCUMENT"] → two routing decisions: 1.1 + 2.1
    If "screen" is absent → ["DOCUMENT"] → single routing decision: 2.1 only

  The TransitionOrchestratorUnit selects SubPhaseId based on the primary predicate:
    Primary predicate hierarchy: UI > ARCHITECTURE > IMPLEMENT > DOCUMENT
    When UI is present, SubPhaseId is always "1.1" (Phase 1 primary).
    When UI absent, ARCHITECTURE present: SubPhaseId is "3.1".
    When UI and ARCHITECTURE absent, IMPLEMENT present: SubPhaseId is "4.1".
    When only DOCUMENT: SubPhaseId is "2.1".

  The SubPhaseId in the SelectedLogicalTransition is the PRIMARY sub-phase.
  Secondary routing decisions from Layer 0-B are separate from this primary assignment.
