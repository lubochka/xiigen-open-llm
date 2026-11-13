# Layer 0-B — Router Pairs (048)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: router

---

## Method note

File 048 is the complete routing rule reference for files 037-048 (Round 3 domains).

---

## B06-0B-048-001 — TransitionType reference for Round 3 routing

```
source_id: xiigen-b06-0b-048-001
source_license_state: first_party
pair_kind: router
```

TransitionType and routing rules for files 037-048:
  DOCUMENT alone → 2.1 → documentation
  DOCUMENT + IMPLEMENT → 2.1 + 4.1 → feature_addition
  DOCUMENT + ARCHITECTURE → 2.1 + 3.1 → documentation
  DOCUMENT + ARCHITECTURE + IMPLEMENT → 2.1 + 3.1 + 4.1 → feature_addition
  IMPLEMENT alone → 4.1 → feature_addition
  No predicates → knowledge-edge → 0 routing decisions

  No 2.4 routing cases in Round 3 (none of these domains carry "stakeholder" signal).

---

## B06-0B-048-002 — 2.1 vs 2.4 disambiguation for Round 3 domains

```
source_id: xiigen-b06-0b-048-002
source_license_state: first_party
pair_kind: router
```

2.4 trigger state for Round 3 domains:

  All Round 3 domains: 2.4 is NOT triggered unless "stakeholder" term explicitly appears.
    CustomerSegmentation, DynamicPricing, GeofencedPricing, VolumeDiscount,
    MembershipTiers, CouponStackingRules, FreeShippingThreshold, MinimumOrderValue,
    BackorderManagement, PreorderRules — none trigger 2.4 by default.

  Hypothetical: "Define the segmentation requirements for all stakeholder groups."
    → Would trigger 2.1 + 2.4 (stakeholder term present).
    → But base domain requests without "stakeholder" → 2.1 only.

  Rule: 2.4 is never the sole routing decision. Always added to 2.1 when triggered.

---

## B06-0B-048-003 — Complete routing summary for all four rounds

```
source_id: xiigen-b06-0b-048-003
source_license_state: first_party
pair_kind: router
```

Routing summary across all four rounds (files 013-048):

  Round 1 (013-024): 10 domains — no 2.4 except LoyaltyProgram, DataPrivacy (stakeholder)
  Round 2 (025-036): 11 domains — no 2.4 except ReferralProgram, ProductRecommendation (stakeholder)
  Round 3 (037-048): 10 domains — no 2.4 in any domain (no stakeholder signals in any)

  Total domains with 2.4 capability: 4 (LoyaltyProgram, DataPrivacy, ReferralProgram, ProductRecommendation)
  All other domains: 2.1 only for DOCUMENT; 2.1+4.1 for DOCUMENT+IMPLEMENT.

---

## B06-0B-048-004 — Routing anti-patterns complete summary (Round 3)

```
source_id: xiigen-b06-0b-048-004
source_license_state: first_party
pair_kind: router
```

Key anti-pattern reminder for all rounds:
  Domain names never determine routing. Only predicate firing determines routing.
  "Customer segmentation" → DOCUMENT → 2.1 (same as every other DOCUMENT request).
  "Dynamic pricing" → DOCUMENT → 2.1 (same sub-phase regardless of business domain).
  The sub-phase map (2.1, 2.4, 3.1, 4.1, 1.1) is fixed. New domains add zero new sub-phases.
  DomainId is always "software-development" regardless of business domain.
