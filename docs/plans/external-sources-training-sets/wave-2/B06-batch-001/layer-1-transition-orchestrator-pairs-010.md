# Layer 1 — Transition Orchestrator Pairs (010)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## Method note

File 010 covers canonical B06 request patterns and their full transition outputs.

---

## B06-T1-010-001 — Canonical: "define the requirements" → documentation 2.1

```
source_id: xiigen-b06-t1-010-001
source_title: XIIGen B06 T1 — canonical define requirements documentation
source_license_state: first_party
contract_signature: TransitionOrchestratorUnit : ILogicalUnit<LogicalOrchestratorRequest, SelectedLogicalTransition>
transition_type: documentation
phase: 2
sub_phase: 2.1
pair_kind: transition_orchestrator
```

A:
  run_id: "B06-t1-010-001"
  user_request: "Define the business requirements and use cases for the checkout flow."
  CompiledPredicateIds: ["DOCUMENT"]
  PredictedSubPhaseId: "2.1"

B:
  TransitionId: "transition-B06-t1-010-001-2.1-documentation"
  TransitionType: "documentation"
  SubPhaseId: "2.1"
  DomainId: "software-development"
  PredicateIds: ["DOCUMENT"]
  ContextCarryForward: true
  Metadata:
    transition_classification_basis: "define→DOCUMENT, business→DOCUMENT, requirements→DOCUMENT, use cases→DOCUMENT; no IMPLEMENT or UI"
    primary_intent: "document"
    domain_signal: "business_requirements"

---

## B06-T1-010-002 — Canonical: "write user stories" → feature_addition 2.1+4.1

```
source_id: xiigen-b06-t1-010-002
source_title: XIIGen B06 T1 — canonical write user stories feature_addition
source_license_state: first_party
contract_signature: TransitionOrchestratorUnit : ILogicalUnit<LogicalOrchestratorRequest, SelectedLogicalTransition>
transition_type: feature_addition
phase: 2
sub_phase: 2.1
pair_kind: transition_orchestrator
```

A:
  run_id: "B06-t1-010-002"
  user_request: "Write the user stories and acceptance criteria for the checkout flow."
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1"
    - PredictedSubPhaseId: "4.1"

B:
  GlobalTransitionType: "feature_addition"
  Transitions:
    - SubPhaseId: "2.1", TransitionType: "feature_addition",
      PredicateIds: ["DOCUMENT"],
      Metadata: { primary_intent: "create", domain_signal: "requirements_authoring_write" }
    - SubPhaseId: "4.1", TransitionType: "feature_addition",
      PredicateIds: ["IMPLEMENT"],
      Metadata: { primary_intent: "implement", domain_signal: "backend_implementation" }

---

## B06-T1-010-003 — Canonical: "stakeholder requirements" → documentation 2.1+2.4

```
source_id: xiigen-b06-t1-010-003
source_title: XIIGen B06 T1 — canonical stakeholder requirements documentation
source_license_state: first_party
contract_signature: TransitionOrchestratorUnit : ILogicalUnit<LogicalOrchestratorRequest, SelectedLogicalTransition>
transition_type: documentation
phase: 2
sub_phase: 2.1
pair_kind: transition_orchestrator
```

A:
  run_id: "B06-t1-010-003"
  user_request: "Document the checkout requirements and stakeholder expectations."
  CompiledPredicateIds: ["DOCUMENT"]
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1"
    - PredictedSubPhaseId: "2.4"

B:
  GlobalTransitionType: "documentation"
  Transitions:
    - SubPhaseId: "2.1", TransitionType: "documentation",
      PredicateIds: ["DOCUMENT"],
      Metadata: { primary_intent: "document", domain_signal: "business_requirements" }
    - SubPhaseId: "2.4", TransitionType: "documentation",
      PredicateIds: ["DOCUMENT"],
      Metadata: { primary_intent: "document", domain_signal: "stakeholder_requirements" }
  TransitionCount: 2
