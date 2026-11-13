# Layer 0-D — Flow Factory Selection Pairs (030)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B06-0D-030-001 — DropshippingRules: DOCUMENT → RequirementsAuthoringFlow21

```
source_id: xiigen-b06-0d-030-001
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

## B06-0D-030-002 — DropshippingRules: DOCUMENT + IMPLEMENT → two flows

```
source_id: xiigen-b06-0d-030-002
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

## B06-0D-030-003 — DropshippingRules: factory map update for Round 2

```
source_id: xiigen-b06-0d-030-003
source_license_state: first_party
pair_kind: factory_selection
```

Complete flow factory map update (adds to 025-036 baseline):

  Round 2 adds 11 new domain contexts to the factory's coverage:
  GiftCardRedemption, ReferralProgram, FlashSaleEngine, BundlePricing, WholesaleDiscount,
  DropshippingRules, AbandonedCartRecovery, ProductRecommendation, CrossSellUpsell,
  SeasonalPromotion, DigitalProductDelivery.

  All 11 new domains route to the same flows as the original domains:
    DOCUMENT → 2.1 → RequirementsAuthoringFlow21 (trusted_learned)
    DOCUMENT + stakeholder → 2.4 → StakeholderRequirementsFlow24 (in addition to 2.1)
    ARCHITECTURE → 3.1 → ArchitectureFlow31 (deterministic_generic)
    IMPLEMENT → 4.1 → ImplementationFlow41 (deterministic_generic)

  New stakeholder-triggered 2.4 cases in Round 2:
    ReferralProgram (file 026, pair 1)
    ProductRecommendation (file 032, pair 2)
