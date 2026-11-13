# Layer 0-B — Router Pairs (036)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: router

---

## Method note

File 036 is the complete routing rule reference for files 025-036,
covering all Round 2 domains and routing scenarios.

---

## B06-0B-036-001 — Multi-domain DOCUMENT sweep → 2.1

```
source_id: xiigen-b06-0b-036-001
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
pair_kind: router
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledText: "Define and specify all requirements across gift card redemption, referral programs, flash sales, bundle pricing, wholesale discounts, dropshipping, abandoned cart recovery, product recommendations, cross-sell, seasonal promotions, and digital delivery."

B:
  RoutingDecisions:
    - PredictedPhaseId: "2"; PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]; Confidence: 0.92; KnowledgeEdge: false
  DecisionCount: 1
  Routing rationale: Domain breadth does not change sub-phase. DOCUMENT → 2.1 regardless of domain count.

---

## B06-0B-036-002 — Routing decision count table for files 025-036

```
source_id: xiigen-b06-0b-036-002
source_license_state: first_party
pair_kind: router
```

Routing decision count by file and scenario:

  File | Scenario                    | DecisionCount | Sub-phases
  -----|-----------------------------|--------------|-----------
  025  | GiftCardRedemption          | 1, 2, or 2   | 2.1 / 2.1+3.1 / 2.1+4.1
  026  | ReferralProgram             | 1, 2, or 2   | 2.1 / 2.1+2.4 / 2.1+4.1
  027  | FlashSaleEngine             | 1 or 3       | 2.1 / 2.1+3.1+4.1
  028  | BundlePricing               | 1 or 2       | 2.1 / 2.1+4.1
  029  | WholesaleDiscount           | 1, 2, or 1   | 2.1 / 2.1+4.1 / 3.1
  030  | DropshippingRules           | 1 or 2       | 2.1 / 2.1+4.1
  031  | AbandonedCartRecovery       | 1 or 2       | 2.1 / 2.1+4.1
  032  | ProductRecommendation       | 1, 2, or 2   | 2.1 / 2.1+2.4 / 2.1+3.1
  033  | CrossSellUpsell             | 1, 2, or 3   | 2.1 / 2.1+4.1 / 2.1+3.1+4.1
  034  | SeasonalPromotion           | 1 or 2       | 2.1 / 2.1+4.1
  035  | DigitalProductDelivery      | 1, 2, or 3   | 2.1 / 2.1+4.1 / 2.1+3.1+4.1
  036  | Multi-domain sweep          | 1             | 2.1

  New stakeholder-triggered 2.4 cases: ReferralProgram (026), ProductRecommendation (032).

---

## B06-0B-036-003 — Routing anti-patterns for Round 2 domains

```
source_id: xiigen-b06-0b-036-003
source_license_state: first_party
pair_kind: router
```

Routing anti-patterns for Round 2 domains:

  AP-R8: Routing gift card redemption to a separate "payment sub-phase."
    Correct: No payment sub-phase exists. DOCUMENT → 2.1. IMPLEMENT → 4.1.

  AP-R9: Routing flash sale to "campaign sub-phase."
    Correct: No campaign sub-phase. DOCUMENT → 2.1. IMPLEMENT → 4.1.

  AP-R10: Routing seasonal promotion to "marketing sub-phase."
    Correct: No marketing sub-phase. DomainId is always software-development.
    DOCUMENT → 2.1 regardless of business domain.

  AP-R11: Routing digital delivery to "fulfillment sub-phase."
    Correct: No fulfillment sub-phase. DOCUMENT → 2.1. IMPLEMENT → 4.1.

  AP-R12: Omitting 2.4 when "stakeholder" is present in referral or recommendation context.
    Correct: "stakeholder" always adds 2.4 in addition to 2.1.

---

## B06-0B-036-004 — 2.1 vs 2.4 disambiguation for Round 2 domains

```
source_id: xiigen-b06-0b-036-004
source_license_state: first_party
pair_kind: router
```

2.4 trigger cases added by Round 2:
  ReferralProgram + "stakeholder" → 2.1 + 2.4
  ProductRecommendation + "stakeholder" → 2.1 + 2.4

  Domains that do NOT trigger 2.4 (no stakeholder term):
    GiftCardRedemption, FlashSaleEngine, BundlePricing, WholesaleDiscount,
    DropshippingRules, AbandonedCartRecovery, CrossSellUpsell,
    SeasonalPromotion, DigitalProductDelivery (unless "stakeholder" explicitly present)

  Rule: 2.4 is never the sole routing decision. Always added to 2.1.
