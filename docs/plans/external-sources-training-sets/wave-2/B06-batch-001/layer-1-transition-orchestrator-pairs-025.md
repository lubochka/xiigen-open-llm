# Layer 1 — Transition Orchestrator Pairs (025)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B06-T1-025-001 — GiftCardRedemption: DOCUMENT → documentation

```
source_id: xiigen-b06-t1-025-001
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  UserRequest: "Define the requirements and behavior for gift card redemption at checkout."
  PredicateIds: ["DOCUMENT"]

B:
  Transitions:
    1. SubPhaseId: "2.1"; TransitionType: "documentation"; DomainId: "software-development"
       SelectedFlowId: "universal-nine-node-development-flow-2.1"
  TransitionCount: 1
  TransitionType: "documentation"

---

## B06-T1-025-002 — GiftCardRedemption: DOCUMENT + IMPLEMENT → feature_addition

```
source_id: xiigen-b06-t1-025-002
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  UserRequest: "Write the specification for gift card redemption and build the balance validation service."
  PredicateIds: ["DOCUMENT", "IMPLEMENT"]

B:
  Transitions:
    1. SubPhaseId: "2.1"; TransitionType: "feature_addition"; DomainId: "software-development"
    2. SubPhaseId: "4.1"; TransitionType: "feature_addition"; DomainId: "software-development"
  TransitionCount: 2
  TransitionType: "feature_addition" (IMPLEMENT present — all transitions become feature_addition)
  CanonicalOrdering: 2.1 → 4.1

---

## B06-T1-025-003 — GiftCardRedemption: DOCUMENT + ARCHITECTURE → documentation

```
source_id: xiigen-b06-t1-025-003
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  UserRequest: "Document the gift card requirements and design the service architecture."
  PredicateIds: ["DOCUMENT", "ARCHITECTURE"]

B:
  Transitions:
    1. SubPhaseId: "2.1"; TransitionType: "documentation"; DomainId: "software-development"
    2. SubPhaseId: "3.1"; TransitionType: "documentation"; DomainId: "software-development"
  TransitionCount: 2
  TransitionType: "documentation" (ARCHITECTURE alone does not upgrade to feature_addition)
  CanonicalOrdering: 2.1 → 3.1

---

## B06-T1-025-004 — GiftCardRedemption: DOCUMENT + ARCHITECTURE + IMPLEMENT → feature_addition

```
source_id: xiigen-b06-t1-025-004
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  UserRequest: "Write the gift card requirements, design the service architecture, and build the redemption engine."
  PredicateIds: ["DOCUMENT", "ARCHITECTURE", "IMPLEMENT"]

B:
  Transitions:
    1. SubPhaseId: "2.1"; TransitionType: "feature_addition"; DomainId: "software-development"
    2. SubPhaseId: "3.1"; TransitionType: "feature_addition"; DomainId: "software-development"
    3. SubPhaseId: "4.1"; TransitionType: "feature_addition"; DomainId: "software-development"
  TransitionCount: 3
  TransitionType: "feature_addition" (IMPLEMENT present — overrides all transitions)
  CanonicalOrdering: 2.1 → 3.1 → 4.1
