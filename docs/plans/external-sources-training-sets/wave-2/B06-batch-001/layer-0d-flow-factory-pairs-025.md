# Layer 0-D — Flow Factory Selection Pairs (025)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B06-0D-025-001 — GiftCardRedemption: DOCUMENT → RequirementsAuthoringFlow21

```
source_id: xiigen-b06-0d-025-001
source_license_state: first_party
pair_kind: factory_selection
```

A:
  RoutingDecisions: [SubPhaseId: "2.1"]
  Context: "GiftCardRedemption DOCUMENT alone"

B:
  SelectedFlow:
    SelectedFlowId: "universal-nine-node-development-flow-2.1"
    FlowAlias: "RequirementsAuthoringFlow21"
    TrustState: "trusted_learned"
    FallbackUsed: false
    Warnings: []

---

## B06-0D-025-002 — GiftCardRedemption: DOCUMENT + IMPLEMENT → two flows

```
source_id: xiigen-b06-0d-025-002
source_license_state: first_party
pair_kind: factory_selection
```

A:
  RoutingDecisions: [SubPhaseId: "2.1", SubPhaseId: "4.1"]
  Context: "GiftCardRedemption DOCUMENT + IMPLEMENT"

B:
  Flow_2_1:
    SelectedFlowId: "universal-nine-node-development-flow-2.1"
    FlowAlias: "RequirementsAuthoringFlow21"
    TrustState: "trusted_learned"
    FallbackUsed: false
    Warnings: []
  Flow_4_1:
    SelectedFlowId: "universal-nine-node-development-flow-4.1"
    FlowAlias: "ImplementationFlow41"
    TrustState: "deterministic_generic"
    FallbackUsed: false
    Warnings: []

---

## B06-0D-025-003 — GiftCardRedemption: factory anti-pattern

```
source_id: xiigen-b06-0d-025-003
source_license_state: first_party
pair_kind: factory_selection
```

Anti-pattern AP-F5: Creating a domain-specific GiftCardFlow21 instead of RequirementsAuthoringFlow21.
  Correct: RequirementsAuthoringFlow21 handles ALL requirement domains.
    The factory is domain-agnostic. Gift card requirements use the same flow as checkout requirements.
  Corrupt: SelectedFlowId: "gift-card-requirements-flow-2.1"
  Teaching: The flow factory does not create new flow types for new domains.
    Only the nine node inputs (N01 A = SelectedLogicalTransition context) differ by domain.
