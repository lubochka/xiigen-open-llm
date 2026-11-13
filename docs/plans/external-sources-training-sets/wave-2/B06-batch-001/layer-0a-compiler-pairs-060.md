# Layer 0-A — Compiler Pairs (060)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: compiler

---

## Method note

File 060 is the multi-predicate sweep and complete rule reference for files 049-060.
Covers all Round 4 domains (ReturnWindowPolicy, WarrantyTracking, SplitPayment,
InstallmentPlan, StoreCreditManagement, WishlistSharing, ProductBundleCreator,
PriceMatchPolicy, StockAlertSystem, AutoReplenishment, VIPAccess)
with a consolidated rule table.

---

## B06-0A-060-001 — Multi-predicate: all Round 4 domains, DOCUMENT sweep

```
source_id: xiigen-b06-0a-060-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define, describe, and specify the requirements for return window policy, warranty tracking, split payment, installment plans, store credit, wishlist sharing, product bundle creator, price match policy, stock alerts, auto-replenishment, and VIP access."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Define" (→ define), "describe" (→ describe), "specify" (→ specify),
      "requirements" (→ requirement)
  Domain nouns add zero predicates.
  Confidence: 0.92

---

## B06-0A-060-002 — Complete DOCUMENT predicate rule reference for files 049-060

```
source_id: xiigen-b06-0a-060-002
source_license_state: first_party
pair_kind: compiler
```

Extended B06 DOCUMENT term coverage across files 049-060:

  File | Domain                  | DOCUMENT terms demonstrated
  -----|-------------------------|-----------------------------
  049  | ReturnWindowPolicy      | define, requirements, describe, specification
  050  | WarrantyTracking        | describe, requirements, specify, specification
  051  | SplitPayment            | define, requirements, behavior, specification
  052  | InstallmentPlan         | define, requirements, specify, specification
  053  | StoreCreditManagement   | define, requirements, describe, specification
  054  | WishlistSharing         | define, requirements, describe, specification
  055  | ProductBundleCreator    | define, requirements, describe, specification
  056  | PriceMatchPolicy        | define, requirements, describe, specification
  057  | StockAlertSystem        | define, requirements, describe, specification
  058  | AutoReplenishment       | define, requirements, describe, specification
  059  | VIPAccess               | define, requirements, describe, specification
  060  | Multi-domain sweep      | define, describe, specify, requirements

---

## B06-0A-060-003 — Non-terms in Round 4 domains

```
source_id: xiigen-b06-0a-060-003
source_license_state: first_party
pair_kind: compiler
```

Domain words that do NOT fire any predicate (Round 4):
  return window, return eligibility, warranty, manufacturer warranty, extended warranty,
  BNPL, installment, interest-free, missed payment, store credit, credit ledger, credit balance,
  wishlist, shareable link, collaborative wishlist, product bundle, component inventory,
  bundle pricing, price match, competitor price, verification window, stock alert,
  back-in-stock, low-stock threshold, auto-replenishment, recurring order, skip delivery,
  VIP access, early sale window, exclusive product, dedicated support queue

Teaching: domain nouns and operational verbs (check, issue, schedule, notify, reorder,
  split, match, combine, share, grant, track) never fire predicates alone.

---

## B06-0A-060-004 — TransitionType reference for files 049-060

```
source_id: xiigen-b06-0a-060-004
source_license_state: first_party
pair_kind: compiler
```

TransitionType summary for files 049-060:
  DOCUMENT alone → documentation (all single-predicate pairs in 049-059)
  DOCUMENT + IMPLEMENT → feature_addition (pairs -003 in each file)
  DOCUMENT + ARCHITECTURE → documentation (pairs -005 in 049-059)
  DOCUMENT + ARCHITECTURE + IMPLEMENT → feature_addition (if all three present)
  IMPLEMENT alone → feature_addition (pairs -002 in 049-059)

  Rule reminder: IMPLEMENT anywhere in predicate set → feature_addition unconditionally on ALL transitions.

---

## B06-0A-060-005 — Complete factory and predicate reference for B06 batch 001

```
source_id: xiigen-b06-0a-060-005
source_license_state: first_party
pair_kind: compiler
```

Complete predicate coverage for all of B06-batch-001 (files 002-060):

  Total domains covered: 35+
    Original (002-012): checkout, payment, guest, user story, cancellation, refund, notifications, access control, stakeholder, search, feature brief
    Round 1 (013-024): shipping, inventory, loyalty, return/refund, tax, multi-currency, bundle/discount, subscription, fraud detection, data privacy
    Round 2 (025-036): gift card, referral, flash sale, bundle pricing, wholesale, dropshipping, abandoned cart, recommendation, cross-sell, seasonal, digital delivery
    Round 3 (037-048): customer segmentation, dynamic pricing, geofenced pricing, volume discount, membership tiers, coupon stacking, free shipping, minimum order, backorder, preorder
    Round 4 (049-060): return window, warranty, split payment, installment, store credit, wishlist sharing, product bundle creator, price match, stock alerts, auto-replenishment, VIP access

  DomainId: "software-development" confirmed for all 35+ domains.
  RequirementsAuthoringFlow21 is domain-agnostic and applies to all 35+ domains.
