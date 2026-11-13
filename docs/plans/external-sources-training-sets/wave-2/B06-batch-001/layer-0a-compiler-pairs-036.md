# Layer 0-A — Compiler Pairs (036)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: compiler

---

## Method note

File 036 is the multi-predicate sweep and complete rule reference for files 025-036.
Covers all Round 2 domains (GiftCardRedemption, ReferralProgram, FlashSaleEngine,
BundlePricing, WholesaleDiscount, DropshippingRules, AbandonedCartRecovery,
ProductRecommendation, CrossSellUpsell, SeasonalPromotion, DigitalProductDelivery)
with a consolidated rule table.

---

## B06-0A-036-001 — Multi-predicate: all Round 2 domains, DOCUMENT sweep

```
source_id: xiigen-b06-0a-036-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define, describe, and specify the requirements and behavior for gift card redemption, referral programs, flash sales, bundle pricing, wholesale discounts, dropshipping, abandoned cart recovery, product recommendations, cross-sell, seasonal promotions, and digital delivery."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Define" (→ define), "describe" (→ describe), "specify" (→ specify),
      "requirements" (→ requirement), "behavior" (→ behavior)
  Domain nouns add zero predicates.
  Confidence: 0.92

---

## B06-0A-036-002 — Multi-predicate: DOCUMENT + IMPLEMENT across Round 2 domains

```
source_id: xiigen-b06-0a-036-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Write the specifications for gift card, referral, flash sale, bundle pricing, and abandoned cart recovery — and build the corresponding services."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "specifications" (→ specification)
    IMPLEMENT fires on: "Write" (→ write), "build" (→ build)
  TransitionType: feature_addition
  Confidence: 0.91

---

## B06-0A-036-003 — Complete DOCUMENT predicate rule reference for files 025-036

```
source_id: xiigen-b06-0a-036-003
source_license_state: first_party
pair_kind: compiler
```

Extended B06 DOCUMENT term coverage across files 025-036:

  File | Domain                  | DOCUMENT terms demonstrated
  -----|-------------------------|-----------------------------
  025  | GiftCardRedemption      | define, requirements, behavior, specification
  026  | ReferralProgram         | define, business, use cases, specify, criteria
  027  | FlashSaleEngine         | describe, requirements, specify, functional, specification
  028  | BundlePricing           | define, document, specification, narrative
  029  | WholesaleDiscount       | describe, specify, requirements, document, epics
  030  | DropshippingRules       | define, specification, document, capability, requirements
  031  | AbandonedCartRecovery   | specify, requirements, functional, story
  032  | ProductRecommendation   | define, requirements, describe, feature-brief, stakeholder
  033  | CrossSellUpsell         | define, specify, requirements, specification
  034  | SeasonalPromotion       | describe, define, requirements — all 17 confirmed
  035  | DigitalProductDelivery  | define, requirements, specify, specification
  036  | Multi-domain sweep      | define, describe, specify, requirements, behavior, specifications

---

## B06-0A-036-004 — Predicate non-terms in Round 2 domains

```
source_id: xiigen-b06-0a-036-004
source_license_state: first_party
pair_kind: compiler
```

Domain words that do NOT fire any predicate (Round 2):
  gift card, redemption, balance, partial redemption, expiry,
  referral, invite, accrual, reward, flash sale, countdown, inventory hold,
  bundle, SKU, pricing, wholesale, B2B, tiered, dropship, supplier, routing,
  abandoned cart, recovery, email trigger, product recommendation, collaborative filtering,
  personalization, cross-sell, upsell, affinity, seasonal, campaign, blackout,
  digital delivery, download, license key, CDN, token

Teaching: domain nouns and operational verbs (handle, track, calculate, run, activate,
show, route, send, email, apply) never fire predicates alone.

---

## B06-0A-036-005 — TransitionType reference for files 025-036

```
source_id: xiigen-b06-0a-036-005
source_license_state: first_party
pair_kind: compiler
```

TransitionType summary for files 025-036:
  DOCUMENT alone → documentation (all single-predicate pairs in 025-035)
  DOCUMENT + IMPLEMENT → feature_addition (pairs -002 and -003 in each file)
  DOCUMENT + ARCHITECTURE → documentation (file 029-003, 032-003)
  DOCUMENT + ARCHITECTURE + IMPLEMENT → feature_addition (files 027-003, 033-005, 035-005)
  IMPLEMENT alone → feature_addition (files 025-003, 026-002, 027-002, 028-002, 029-002, 030-002, 031-002, 032-002, 033-002, 034-002, 035-002)

  Rule reminder: IMPLEMENT anywhere in predicate set → feature_addition unconditionally on ALL transitions.
