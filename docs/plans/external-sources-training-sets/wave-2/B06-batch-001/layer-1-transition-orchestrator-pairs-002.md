# Layer 1 — Transition Orchestrator Pairs (002)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## Method note

File 002 covers DOCUMENT alone → documentation TransitionType for new requirements.
Feature_addition vs documentation distinction based on verb and predicate set.

---

## B06-T1-002-001 — DOCUMENT alone: documentation TransitionType

```
source_id: xiigen-b06-t1-002-001
source_title: XIIGen B06 T1 — DOCUMENT only documentation TransitionType
source_license_state: first_party
contract_signature: TransitionOrchestratorUnit : ILogicalUnit<LogicalOrchestratorRequest, SelectedLogicalTransition>
transition_type: documentation
phase: 2
sub_phase: 2.1
pair_kind: transition_orchestrator
```

A:
  run_id: "B06-t1-002-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Define the acceptance criteria for the payment processing feature."
  CompiledPredicateIds: ["DOCUMENT"]
  PredictedSubPhaseId: "2.1"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  TransitionId: "transition-B06-t1-002-001-2.1-documentation"
  TransitionType: "documentation"
  ScenarioId: "adaptive-logical-orchestration"
  PhaseId: "2"
  SubPhaseId: "2.1"
  DomainId: "software-development"
  UserRequest: "Define the acceptance criteria for the payment processing feature."
  PredicateIds: ["DOCUMENT"]
  ContextCarryForward: true
  EvidenceIds: ["B06-0a-002-001-prompt"]
  Metadata:
    transition_classification_basis: "documentation_signal: 'define', 'acceptance criteria' — output is a requirements artifact; no implementation verbs"
    primary_intent: "document"
    domain_signal: "requirements_authoring"

---

## B06-T1-002-002 — DOCUMENT alone: feature_addition when new artifact

```
source_id: xiigen-b06-t1-002-002
source_title: XIIGen B06 T1 — DOCUMENT only feature_addition new artifact
source_license_state: first_party
contract_signature: TransitionOrchestratorUnit : ILogicalUnit<LogicalOrchestratorRequest, SelectedLogicalTransition>
transition_type: feature_addition
phase: 2
sub_phase: 2.1
pair_kind: transition_orchestrator
```

A:
  run_id: "B06-t1-002-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Define the acceptance criteria for the new one-click checkout capability — this is a new feature not yet in the requirements."
  CompiledPredicateIds: ["DOCUMENT"]
  PredictedSubPhaseId: "2.1"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  TransitionId: "transition-B06-t1-002-002-2.1-feature_addition"
  TransitionType: "feature_addition"
  ScenarioId: "adaptive-logical-orchestration"
  PhaseId: "2"
  SubPhaseId: "2.1"
  DomainId: "software-development"
  UserRequest: "Define the acceptance criteria for the new one-click checkout capability."
  PredicateIds: ["DOCUMENT"]
  ContextCarryForward: true
  Metadata:
    transition_classification_basis: "add_new_capability_signal: 'new feature', 'not yet in requirements' — new artifact being created"
    primary_intent: "create"
    domain_signal: "new_requirements_artifact"

Note: Even with DOCUMENT only, "new" context (new capability, new feature not in requirements)
  can signal feature_addition. The contextual signal overrides the default documentation
  classification for DOCUMENT-only predicates.

---

## B06-T1-002-003 — TransitionType: default behavior for DOCUMENT alone

```
source_id: xiigen-b06-t1-002-003
source_title: XIIGen B06 T1 — DOCUMENT default TransitionType behavior
source_license_state: first_party
pair_kind: transition_orchestrator
```

Teaching note — TransitionType default for DOCUMENT alone:

  Default (no contextual override): DOCUMENT alone → documentation
  Override to feature_addition when:
    - "new" capability or feature explicitly mentioned
    - "add" or "create" appears in context (even without firing IMPLEMENT)
    - Request states the artifact does not yet exist

  Default behavior in B06:
    "Define the acceptance criteria"   → documentation (existing criteria)
    "Describe the use cases"           → documentation
    "Specify the behavior"             → documentation

  Override behavior in B06:
    "Define new acceptance criteria for a feature we haven't specified yet" → feature_addition
    "Create the requirements for the new checkout flow" → feature_addition (IMPLEMENT fires on create)

  Note: The contextual override for DOCUMENT alone is a secondary rule.
  The primary rule is: if IMPLEMENT or UI fires → feature_addition.
