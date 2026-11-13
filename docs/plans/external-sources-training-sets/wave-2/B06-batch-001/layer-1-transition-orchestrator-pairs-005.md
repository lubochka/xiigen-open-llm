# Layer 1 — Transition Orchestrator Pairs (005)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## Method note

File 005 covers DOCUMENT + UI → feature_addition. UI presence drives feature_addition.

---

## B06-T1-005-001 — DOCUMENT+UI: feature_addition from UI presence

```
source_id: xiigen-b06-t1-005-001
source_title: XIIGen B06 T1 — DOCUMENT+UI feature_addition
source_license_state: first_party
contract_signature: TransitionOrchestratorUnit : ILogicalUnit<LogicalOrchestratorRequest, SelectedLogicalTransition>
transition_type: feature_addition
phase: 2
sub_phase: 2.1
pair_kind: transition_orchestrator
```

A:
  run_id: "B06-t1-005-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Document the checkout form requirements and describe the user interface behavior for each step."
  CompiledPredicateIds: ["DOCUMENT", "UI"]
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"]
    - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"]
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  Transitions:
    - TransitionId: "transition-B06-t1-005-001a-2.1"
      TransitionType: "feature_addition"
      PhaseId: "2"
      SubPhaseId: "2.1"
      DomainId: "software-development"
      PredicateIds: ["DOCUMENT"]
      Metadata:
        transition_classification_basis: "UI_present: UI fires → feature_addition globally; even DOCUMENT transition is feature_addition"
        primary_intent: "document"
        domain_signal: "requirements_authoring_with_ux"

    - TransitionId: "transition-B06-t1-005-001b-1.1"
      TransitionType: "feature_addition"
      PhaseId: "1"
      SubPhaseId: "1.1"
      DomainId: "software-development"
      PredicateIds: ["UI"]
      Metadata:
        transition_classification_basis: "UI_present: new UX work; feature_addition"
        primary_intent: "design_ux"
        domain_signal: "ui_requirements_authoring"

  TransitionCount: 2
  GlobalTransitionType: "feature_addition"

Rationale:
  UI predicate present → GlobalTransitionType: feature_addition.
  UI signals new UX work is being created — this is feature addition.
  Both the 2.1 (DOCUMENT) and 1.1 (UI) transitions are feature_addition.

---

## B06-T1-005-002 — DOCUMENT+UI with stakeholder: 2.1 + 1.1 + 2.4

```
source_id: xiigen-b06-t1-005-002
source_title: XIIGen B06 T1 — DOCUMENT+UI with stakeholder signals
source_license_state: first_party
contract_signature: TransitionOrchestratorUnit : ILogicalUnit<LogicalOrchestratorRequest, SelectedLogicalTransition>
transition_type: feature_addition
phase: 2
sub_phase: 2.1
pair_kind: transition_orchestrator
```

A:
  run_id: "B06-t1-005-002"
  user_request: "Document the form requirements for stakeholder review and describe the UI behavior per step."
  CompiledPredicateIds: ["DOCUMENT", "UI"]
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"]
    - PredictedSubPhaseId: "2.4", EvidencePredicates: ["DOCUMENT"]
    - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"]

B:
  TransitionCount: 3
  GlobalTransitionType: "feature_addition"
  Transitions:
    - SubPhaseId: "2.1", TransitionType: "feature_addition", PredicateIds: ["DOCUMENT"]
    - SubPhaseId: "2.4", TransitionType: "feature_addition", PredicateIds: ["DOCUMENT"]
    - SubPhaseId: "1.1", TransitionType: "feature_addition", PredicateIds: ["UI"]

Teaching: Three transitions from two predicates when DOCUMENT routes to
  both 2.1 and 2.4 (stakeholder signal present) and UI routes to 1.1.

---

## B06-T1-005-003 — UI predicate → feature_addition rationale

```
source_id: xiigen-b06-t1-005-003
source_title: XIIGen B06 T1 — UI feature_addition rationale
source_license_state: first_party
pair_kind: transition_orchestrator
```

Teaching note — UI drives feature_addition:

  The UI predicate fires when UI vocabulary appears: form, screen, page,
  interface, component, layout, ux, ui.

  UI presence signals new UX work: a new screen, form, or interface is being
  described or designed. This is categorically feature addition work — new
  UI surfaces are part of new features.

  TransitionType rule for UI:
    UI in CompiledPredicateIds → GlobalTransitionType: feature_addition

  This rule applies even when DOCUMENT is the only other predicate and no
  IMPLEMENT verb is present. UI alone is sufficient to drive feature_addition.

  Contrast:
    "describe the form requirements" → UI fires (form), DOCUMENT fires (describe)
      → feature_addition (UI present)
    "describe the technical requirements" → DOCUMENT fires (describe, requirements)
      → documentation (no UI, no IMPLEMENT)
