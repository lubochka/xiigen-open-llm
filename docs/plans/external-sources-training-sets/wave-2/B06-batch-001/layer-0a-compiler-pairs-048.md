# Layer 0-A — Compiler Pairs (048)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: compiler

---

## Method note

File 048 is the TransitionType reference for files 037-048.
Covers all Round 3 domains (CustomerSegmentation, DynamicPricing, GeofencedPricing,
VolumeDiscount, MembershipTiers, CouponStackingRules, FreeShippingThreshold,
MinimumOrderValue, BackorderManagement, PreorderRules).

---

## B06-0A-048-001 — TransitionType reference table for files 037-048

```
source_id: xiigen-b06-0a-048-001
source_license_state: first_party
pair_kind: compiler
```

TransitionType summary for files 037-048:
  DOCUMENT alone → documentation (all single-predicate pairs in 037-046)
  DOCUMENT + IMPLEMENT → feature_addition (pairs -002/-003 in most files)
  DOCUMENT + ARCHITECTURE → documentation (pairs -005 in 037-046)
  DOCUMENT + ARCHITECTURE + IMPLEMENT → feature_addition (pairs -005 in 038, 040)
  IMPLEMENT alone → feature_addition (pairs -002 in 037, 039, 041-046)

  Rule reminder: IMPLEMENT anywhere in predicate set → feature_addition unconditionally.

---

## B06-0A-048-002 — DOCUMENT terms demonstrated across files 037-048

```
source_id: xiigen-b06-0a-048-002
source_license_state: first_party
pair_kind: compiler
```

Extended B06 DOCUMENT term coverage across files 037-048:

  File | Domain                  | DOCUMENT terms demonstrated
  -----|-------------------------|-----------------------------
  037  | CustomerSegmentation    | define, requirements, behavior, specification
  038  | DynamicPricing          | describe, requirements, specify, specification
  039  | GeofencedPricing        | define, requirements, document, specification
  040  | VolumeDiscount          | describe, requirements, specification
  041  | MembershipTiers         | define, requirements, specify, specification
  042  | CouponStackingRules     | define, requirements, business, specification
  043  | FreeShippingThreshold   | define, requirements, specify, specification
  044  | MinimumOrderValue       | describe, requirements, behavior, specification
  045  | BackorderManagement     | define, requirements, specify, specification
  046  | PreorderRules           | define, requirements, describe, specification
  047  | Multi-predicate sweep   | requirements, document, business, functional, specifications
  048  | TransitionType reference| (reference file — no new terms)

---

## B06-0A-048-003 — Non-terms in Round 3 domains

```
source_id: xiigen-b06-0a-048-003
source_license_state: first_party
pair_kind: compiler
```

Domain words that do NOT fire any predicate (Round 3):
  customer segmentation, RFM, cohort, segment, dynamic pricing, demand-based,
  competitor price, price floor, price ceiling, geofenced, region, geolocation,
  volume discount, quantity break, tiered percentage, SKU eligibility,
  membership tier, Bronze, Silver, Gold, Platinum, point accumulation,
  coupon stacking, coupon types, conflict detection, free shipping,
  minimum order, cart total, backorder, restock, preorder, deposit capture,
  release date, scheduled charge

Teaching: domain nouns and operational verbs (assign, recalculate, waive, block,
  queue, charge, upgrade, apply, calculate) never fire predicates alone.

---

## B06-0A-048-004 — DOCUMENT + ARCHITECTURE combo examples (Round 3)

```
source_id: xiigen-b06-0a-048-004
source_license_state: first_party
pair_kind: compiler
```

DOCUMENT + ARCHITECTURE combinations in Round 3:
  037-005: CustomerSegmentation — data pipeline architecture
  038-005: DynamicPricing — pricing engine architecture
  039-005: GeofencedPricing — geolocation service architecture
  040-005: VolumeDiscount — pricing rule engine architecture
  041-005: MembershipTiers — tier state machine architecture
  042-005: CouponStackingRules — promotion engine architecture
  043-005: FreeShippingThreshold — shipping eligibility service architecture
  044-005: MinimumOrderValue — cart validation service architecture
  045-005: BackorderManagement — fulfillment queue architecture
  046-005: PreorderRules — payment scheduling architecture

  All 10 cases: TransitionType = documentation (ARCHITECTURE alone does not upgrade).

---

## B06-0A-048-005 — Multi-predicate: all Round 3 domains, DOCUMENT sweep

```
source_id: xiigen-b06-0a-048-005
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define, describe, and specify the requirements for customer segmentation, dynamic pricing, geofenced pricing, volume discounts, membership tiers, coupon stacking, free shipping, minimum order, backorder management, and preorder rules."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Define" (→ define), "describe" (→ describe), "specify" (→ specify),
      "requirements" (→ requirement)
  Domain nouns add zero predicates.
  TransitionType: documentation
  Confidence: 0.92
