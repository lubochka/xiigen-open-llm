# Layer 1 — Transition Orchestrator Pairs (013)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## Method note

File 013 covers ShippingRequirements transition orchestration — four pairs
demonstrating TransitionType determination and transition ordering.

---

## B06-T1-013-001 — ShippingRequirements: DOCUMENT alone → documentation

```
source_id: xiigen-b06-t1-013-001
source_license_state: first_party
contract_signature: TransitionOrchestratorUnit : ILogicalUnit<LogicalOrchestratorRequest, SelectedLogicalTransition>
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1"

B:
  Transitions:
    - TransitionId: "transition-shipping-013-001"
      SubPhaseId: "2.1"
      TransitionType: "documentation"
      DomainId: "software-development"
      SelectedFlowId: "universal-nine-node-development-flow-2.1"
  TransitionCount: 1

TransitionType rule: DOCUMENT alone, no IMPLEMENT or UI present → documentation.

---

## B06-T1-013-002 — ShippingRequirements: DOCUMENT+IMPLEMENT → feature_addition

```
source_id: xiigen-b06-t1-013-002
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1"
    - PredictedSubPhaseId: "4.1"

B:
  Transitions:
    - TransitionId: "transition-shipping-013-002a"
      SubPhaseId: "2.1"; TransitionType: "feature_addition"; DomainId: "software-development"
    - TransitionId: "transition-shipping-013-002b"
      SubPhaseId: "4.1"; TransitionType: "feature_addition"; DomainId: "software-development"
  TransitionCount: 2
  TransitionOrdering: 2.1 before 4.1

TransitionType rule: IMPLEMENT present → feature_addition on ALL transitions.

---

## B06-T1-013-003 — ShippingRequirements: DomainId always software-development

```
source_id: xiigen-b06-t1-013-003
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  Context: "ShippingRequirements — domain: shipping/logistics"

B:
  Transition:
    SubPhaseId: "2.1"; TransitionType: "documentation"
    DomainId: "software-development"

Rule 17: DomainId is always "software-development" for B06.
Anti-pattern: DomainId: "logistics" or DomainId: "shipping" — wrong.
The shipping DOMAIN is the subject matter; the work DOMAIN is always software-development.

---

## B06-T1-013-004 — ShippingRequirements: transition ordering 2.1 before 4.1

```
source_id: xiigen-b06-t1-013-004
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  RoutingDecisions: [2.1, 4.1]

B-correct (canonical order):
  Transitions:
    1. SubPhaseId: "2.1" (requirements first)
    2. SubPhaseId: "4.1" (implementation follows)

B-corrupt (ordering violation):
  Transitions:
    1. SubPhaseId: "4.1" (implementation before requirements — wrong)
    2. SubPhaseId: "2.1"

Why harmful: Shipping implementation before shipping requirements are written
means the implementation has no specification to validate against.
