# Layer 0-C — Knowledge-Edge Pairs (042)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B06-0C-042-001 — CouponStackingRules: gap verbs

```
source_id: xiigen-b06-0c-042-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-042-001-kedge"
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  Gap examples for CouponStackingRules:
    "Apply all active coupons to the cart in sequence." → KNOWLEDGE_EDGE (apply = operational)
    "Check if two coupons can stack together." → KNOWLEDGE_EDGE (check = operational)
    "Resolve the coupon conflict and select the higher-value coupon." → KNOWLEDGE_EDGE (resolve = operational)
    "Calculate the combined discount from stacked coupons." → KNOWLEDGE_EDGE (calculate = operational)
  Teaching: "active coupons", "cart", "coupon conflict", "stacked coupons" are domain nouns.

---

## B06-0C-042-002 — CouponStackingRules: boundary cases

```
source_id: xiigen-b06-0c-042-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-042-002-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Boundary cases for CouponStackingRules:
    "Define the coupon stacking requirements." → NOT knowledge-edge (define, requirements fire DOCUMENT)
    "Apply coupons per the business rules specification." → NOT knowledge-edge (business Rule 7, specification fires DOCUMENT)
    "Build the coupon resolver." → NOT knowledge-edge (build fires IMPLEMENT)
    "Apply all coupons in sequence." → KNOWLEDGE_EDGE (no predicate term present)

---

## B06-0C-042-003 — CouponStackingRules: gap vs spec vocabulary

```
source_id: xiigen-b06-0c-042-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-042-003-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Gap vs spec vocabulary for CouponStackingRules:
    gap: "apply/check/resolve/calculate/combine coupon stacking"
    spec: "define/describe/specify/document coupon stacking requirements"
  Teaching: "coupon stacking", "conflict detection", "discount combination" are domain nouns — never fire predicates alone.
