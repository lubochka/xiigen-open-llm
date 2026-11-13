# Layer 0-D — Flow Factory Selection Pairs (031)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B06-0D-031-001 — AbandonedCartRecovery: DOCUMENT → RequirementsAuthoringFlow21

```
source_id: xiigen-b06-0d-031-001
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

## B06-0D-031-002 — AbandonedCartRecovery: DOCUMENT + IMPLEMENT → two flows

```
source_id: xiigen-b06-0d-031-002
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

## B06-0D-031-003 — AbandonedCartRecovery: DomainId invariant

```
source_id: xiigen-b06-0d-031-003
source_license_state: first_party
pair_kind: factory_selection
```

DomainId invariant for Round 2 domains (Rule 17 extension):
  All Round 2 domains produce DomainId: "software-development".

  Domain                | DomainId produced       | Incorrect alternatives
  ----------------------|-------------------------|-----------------------
  GiftCardRedemption    | software-development    | payments, ecommerce
  ReferralProgram       | software-development    | marketing, growth
  FlashSaleEngine       | software-development    | promotions, campaigns
  BundlePricing         | software-development    | pricing, merchandise
  WholesaleDiscount     | software-development    | b2b, wholesale
  DropshippingRules     | software-development    | logistics, fulfillment
  AbandonedCartRecovery | software-development    | marketing, crm
  ProductRecommendation | software-development    | ml, personalization
  CrossSellUpsell       | software-development    | revenue, merchandising
  SeasonalPromotion     | software-development    | marketing, retail
  DigitalProductDelivery| software-development    | digital, drm

  Rule 17 is absolute: DomainId reflects the WORK domain (software development),
  never the BUSINESS domain.
