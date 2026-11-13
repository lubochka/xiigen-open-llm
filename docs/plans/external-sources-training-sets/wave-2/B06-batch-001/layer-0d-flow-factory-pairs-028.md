# Layer 0-D — Flow Factory Selection Pairs (028)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B06-0D-028-001 — BundlePricing: DOCUMENT → RequirementsAuthoringFlow21

```
source_id: xiigen-b06-0d-028-001
source_license_state: first_party
pair_kind: factory_selection
```

A:
  RoutingDecisions: [SubPhaseId: "2.1"]

B:
  Flow_2_1:
    SelectedFlowId: "universal-nine-node-development-flow-2.1"
    FlowAlias: "RequirementsAuthoringFlow21"
    TrustState: "trusted_learned"
    FallbackUsed: false
    Warnings: []

---

## B06-0D-028-002 — BundlePricing: DOCUMENT + IMPLEMENT → two flows

```
source_id: xiigen-b06-0d-028-002
source_license_state: first_party
pair_kind: factory_selection
```

A:
  RoutingDecisions: [SubPhaseId: "2.1", SubPhaseId: "4.1"]

B:
  Flow_2_1:
    SelectedFlowId: "universal-nine-node-development-flow-2.1"
    FlowAlias: "RequirementsAuthoringFlow21"
    TrustState: "trusted_learned"
  Flow_4_1:
    SelectedFlowId: "universal-nine-node-development-flow-4.1"
    FlowAlias: "ImplementationFlow41"
    TrustState: "deterministic_generic"

---

## B06-0D-028-003 — BundlePricing: domain-agnostic factory invariant

```
source_id: xiigen-b06-0d-028-003
source_license_state: first_party
pair_kind: factory_selection
```

Factory domain-agnostic invariant (Round 2 confirmation):
  The factory does NOT inspect domain context (bundle pricing, gift card, seasonal, etc.)
  when selecting flows. Flow selection is based ONLY on routing decisions (sub-phase IDs).

  SubPhaseId: "2.1" → RequirementsAuthoringFlow21 (always)
  SubPhaseId: "3.1" → ArchitectureFlow31 (always)
  SubPhaseId: "4.1" → ImplementationFlow41 (always)
  SubPhaseId: "2.4" → StakeholderRequirementsFlow24 (always)
  SubPhaseId: "1.1" → ScreenInventoryFlow11 (always)

  Domain context travels through SelectedLogicalTransition (N01 input), not through FlowId.
  This invariant holds for all Round 2 domains: GiftCard, Referral, FlashSale, Bundle,
  Wholesale, Dropship, AbandonedCart, Recommendation, CrossSell, Seasonal, DigitalDelivery.
