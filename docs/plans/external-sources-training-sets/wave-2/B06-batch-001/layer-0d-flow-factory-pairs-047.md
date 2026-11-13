# Layer 0-D — Flow Factory Selection Pairs (047)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## Method note

File 047 is the complete factory map summary for files 037-048 (Round 3 domains).

---

## B06-0D-047-001 — Complete factory map for files 037-048

```
source_id: xiigen-b06-0d-047-001
source_license_state: first_party
pair_kind: factory_selection
```

Complete flow factory map update (Round 3 — adds to 013-036 baseline):

  Round 3 files 037-048 add 10 new domain contexts to the factory's coverage.
  All 10 new domains route to the same flows as all prior rounds:
    DOCUMENT → 2.1 → RequirementsAuthoringFlow21 (trusted_learned)
    ARCHITECTURE → 3.1 → ArchitectureFlow31 (trusted_learned)
    IMPLEMENT → 4.1 → ImplementationFlow41 (deterministic_generic)

  No new 2.4 cases in Round 3 (no stakeholder signals in any Round 3 domain).
  No new flow aliases introduced.

---

## B06-0D-047-002 — Factory anti-patterns for Round 3 domains

```
source_id: xiigen-b06-0d-047-002
source_license_state: first_party
pair_kind: factory_selection
```

Factory anti-patterns for files 037-048:

  AP-F10: Creating SegmentationFlow21, PricingFlow21, TierFlow21, etc.
    Correct: RequirementsAuthoringFlow21 handles all requirement domains.

  AP-F11: Assigning different TrustState values to RequirementsAuthoringFlow21 per domain.
    Correct: RequirementsAuthoringFlow21 is always "trusted_learned" regardless of domain.

  AP-F12: Producing zero flows when routing decisions exist.
    Correct: Number of flows always equals number of routing decisions (1:1 invariant).

  AP-F13: Using "deterministic_generic" for RequirementsAuthoringFlow21.
    Correct: Only RequirementsAuthoringFlow21 is "trusted_learned"; ImplementationFlow41 is "deterministic_generic".

---

## B06-0D-047-003 — Round 3 DomainId invariant confirmation

```
source_id: xiigen-b06-0d-047-003
source_license_state: first_party
pair_kind: factory_selection
```

DomainId: "software-development" confirmed for all Round 3 domains.

  Round 3 domains (037-048):
    CustomerSegmentation, DynamicPricing, GeofencedPricing, VolumeDiscount,
    MembershipTiers, CouponStackingRules, FreeShippingThreshold, MinimumOrderValue,
    BackorderManagement, PreorderRules

  All produce DomainId: "software-development" — not their business domain names.
  Total domains with confirmed DomainId: "software-development" = 31 (original 10 + 11 Round 1 + 10 Round 2 + 10 Round 3 [partial]).
