# Layer 1 — Transition Orchestrator Pairs (003)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## Method note

File 003 covers DOCUMENT + IMPLEMENT → feature_addition for "write user stories".
IMPLEMENT presence drives feature_addition regardless of documentation intent.

---

## B06-T1-003-001 — DOCUMENT+IMPLEMENT: feature_addition from "write user stories"

```
source_id: xiigen-b06-t1-003-001
source_title: XIIGen B06 T1 — DOCUMENT+IMPLEMENT feature_addition user stories
source_license_state: first_party
contract_signature: TransitionOrchestratorUnit : ILogicalUnit<LogicalOrchestratorRequest, SelectedLogicalTransition>
transition_type: feature_addition
phase: 2
sub_phase: 2.1
pair_kind: transition_orchestrator
```

A:
  run_id: "B06-t1-003-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Write the user stories for the order management feature with acceptance criteria for each scenario."
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  PredictedSubPhaseId: "2.1"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  TransitionId: "transition-B06-t1-003-001-2.1-feature_addition"
  TransitionType: "feature_addition"
  ScenarioId: "adaptive-logical-orchestration"
  PhaseId: "2"
  SubPhaseId: "2.1"
  DomainId: "software-development"
  UserRequest: "Write the user stories for the order management feature with acceptance criteria for each scenario."
  PredicateIds: ["DOCUMENT", "IMPLEMENT"]
  ContextCarryForward: true
  EvidenceIds: ["B06-0a-003-001-prompt"]
  Metadata:
    transition_classification_basis: "IMPLEMENT_present: 'write' fired IMPLEMENT; feature_addition when IMPLEMENT is in compiled predicates"
    primary_intent: "create"
    domain_signal: "requirements_authoring_with_write_verb"
    transition_type_reason: "IMPLEMENT fires on 'write' → feature_addition; DOCUMENT fires on story+criteria → 2.1"

---

## B06-T1-003-002 — Two sub-phase transitions from DOCUMENT+IMPLEMENT

```
source_id: xiigen-b06-t1-003-002
source_title: XIIGen B06 T1 — DOCUMENT+IMPLEMENT two transitions
source_license_state: first_party
contract_signature: TransitionOrchestratorUnit : ILogicalUnit<LogicalOrchestratorRequest, SelectedLogicalTransition>
transition_type: feature_addition
phase: 2
sub_phase: 2.1
pair_kind: transition_orchestrator
```

A:
  run_id: "B06-t1-003-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Write the user stories for the order management feature with acceptance criteria."
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"]
    - PredictedSubPhaseId: "4.1", EvidencePredicates: ["IMPLEMENT"]

B:
  Transitions:
    - TransitionId: "transition-B06-t1-003-002a-2.1"
      TransitionType: "feature_addition"
      PhaseId: "2"
      SubPhaseId: "2.1"
      DomainId: "software-development"
      PredicateIds: ["DOCUMENT"]
      Metadata:
        domain_signal: "requirements_authoring"

    - TransitionId: "transition-B06-t1-003-002b-4.1"
      TransitionType: "feature_addition"
      PhaseId: "4"
      SubPhaseId: "4.1"
      DomainId: "software-development"
      PredicateIds: ["IMPLEMENT"]
      Metadata:
        domain_signal: "backend_implementation"

  TransitionCount: 2
  GlobalTransitionType: "feature_addition"

Teaching: Both transitions share the same TransitionType (feature_addition)
  because IMPLEMENT fires in the compiled predicates. The 2.1 transition is also
  feature_addition even though its evidence predicate is DOCUMENT.

---

## B06-T1-003-003 — IMPLEMENT in predicates drives feature_addition for ALL transitions

```
source_id: xiigen-b06-t1-003-003
source_title: XIIGen B06 T1 — IMPLEMENT drives feature_addition for all transitions
source_license_state: first_party
pair_kind: transition_orchestrator
```

Teaching note — IMPLEMENT drives feature_addition globally:

  When IMPLEMENT is in CompiledPredicateIds, ALL transitions produced by the
  TransitionOrchestratorUnit are feature_addition, regardless of which predicate
  owns each individual transition.

  Example:
    CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
    Transition for 2.1 (DOCUMENT): feature_addition (not documentation)
    Transition for 4.1 (IMPLEMENT): feature_addition

  This is because IMPLEMENT signals that new functionality is being added
  to the system. This new-functionality signal elevates the TransitionType
  globally across all transitions in the same orchestration run.

  Exception: if DOCUMENT and IMPLEMENT route to entirely separate orchestration
  contexts (different sessions), each could have its own TransitionType.
  Within a single orchestration run, the global TransitionType applies.
