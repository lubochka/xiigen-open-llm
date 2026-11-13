# Layer 0-D — Flow Factory Selection Pairs (060)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## Method note

File 060 is the complete factory reference for all of B06-batch-001 (files 002-060).

---

## B06-0D-060-001 — Complete factory map for files 049-060

```
source_id: xiigen-b06-0d-060-001
source_license_state: first_party
pair_kind: factory_selection
```

Round 4 files 049-060 add 11 new domain contexts to the factory's coverage.
All 11 new domains route to the same flows as all prior rounds:
  DOCUMENT → 2.1 → RequirementsAuthoringFlow21 (trusted_learned)
  ARCHITECTURE → 3.1 → ArchitectureFlow31 (trusted_learned)
  IMPLEMENT → 4.1 → ImplementationFlow41 (deterministic_generic)

No new flow aliases introduced in Round 4.
No new 2.4 cases in Round 4 (no stakeholder signals in any Round 4 domain).

---

## B06-0D-060-002 — Factory anti-patterns for Round 4 domains

```
source_id: xiigen-b06-0d-060-002
source_license_state: first_party
pair_kind: factory_selection
```

Factory anti-patterns for files 049-060:

  AP-F14: Creating ReturnFlow21, WarrantyFlow21, PaymentFlow21, CreditFlow21, etc.
    Correct: RequirementsAuthoringFlow21 handles all requirement domains — no exceptions.

  AP-F15: Assigning "payment" TrustState to ImplementationFlow41 for payment domains.
    Correct: TrustState assignment is flow-specific, not domain-specific. ImplementationFlow41 = "deterministic_generic" always.

  AP-F16: Creating VIPFlow41 or WishlistFlow41 for specialized implementation.
    Correct: ImplementationFlow41 is the sole 4.1 flow regardless of implementation domain.

  AP-F17: Producing null or empty flows for Round 4 domains.
    Correct: ILogicalFlowFactory never produces null. Every routing decision maps to a valid flow.

---

## B06-0D-060-003 — Complete DomainId invariant for all of B06-batch-001

```
source_id: xiigen-b06-0d-060-003
source_license_state: first_party
pair_kind: factory_selection
```

DomainId: "software-development" confirmed for all 42+ domains in B06-batch-001.

  Confirmed domains (42 total):
    Original batch (002-012): checkout, payment, user story, cancellation, refund, notifications, access control, search, feature brief, stakeholder (10 domains)
    Round 1 (013-024): shipping, inventory, loyalty, return/refund, tax, multi-currency, bundle/discount, subscription, fraud detection, data privacy (10 domains)
    Round 2 (025-036): gift card, referral, flash sale, bundle pricing, wholesale, dropshipping, abandoned cart, recommendation, cross-sell, seasonal, digital delivery (11 domains)
    Round 3 (037-048): customer segmentation, dynamic pricing, geofenced pricing, volume discount, membership tiers, coupon stacking, free shipping, minimum order, backorder, preorder (10 domains)
    Round 4 (049-060): return window, warranty, split payment, installment, store credit, wishlist, product bundle creator, price match, stock alerts, auto-replenishment, VIP access (11 domains)

  Rule 17 invariant: DomainId = "software-development" for all 42+ domains, unconditionally.
  The factory never uses business domain names as DomainId values.
