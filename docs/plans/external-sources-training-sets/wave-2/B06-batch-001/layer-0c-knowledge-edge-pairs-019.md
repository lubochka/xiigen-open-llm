# Layer 0-C — Knowledge-Edge Pairs (019)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## Method note

File 019 covers BundleAndDiscount knowledge-edge boundaries — three pairs.

---

## B06-0C-019-001 — Bundle: "feature-brief" fires DOCUMENT; "configure" does not

```
source_id: xiigen-b06-0c-019-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-019-001-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Cases:
    - request: "Specify the feature-brief for product bundle checkout"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — specify fires DOCUMENT, feature-brief fires DOCUMENT"
    - request: "Configure bundle pricing rules in the discount engine"
      predicates: []
      result: "KNOWLEDGE_EDGE — configure is not in any predicate lexicon"
      capability_gap_id: "gap-bundle-configure-001"
    - request: "Define the bundle discount requirements"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge"

---

## B06-0C-019-002 — Discount: "apply" and "calculate" are gaps; "behavior" is not

```
source_id: xiigen-b06-0c-019-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-019-002-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Cases:
    - request: "Document the discount application behavior"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — document fires DOCUMENT, behavior fires DOCUMENT"
    - request: "Apply discount codes at the checkout total step"
      predicates: []
      result: "KNOWLEDGE_EDGE — apply is not in any predicate lexicon"
      capability_gap_id: "gap-discount-apply-001"
    - request: "Calculate discounts during checkout for bundle items"
      predicates: []
      result: "KNOWLEDGE_EDGE — calculate is not in any predicate lexicon"
      capability_gap_id: "gap-discount-calculate-001"

---

## B06-0C-019-003 — Discount: "plan the sprint" is a project-planning gap

```
source_id: xiigen-b06-0c-019-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-019-003-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Cases:
    - request: "Plan the sprint for discount engine requirements"
      predicates: []
      result: "KNOWLEDGE_EDGE — plan is not a predicate term; sprint planning is a project management gap"
      capability_gap_id: "gap-discount-sprint-plan-001"
      note: "requirements appears in request but plan cancels — no verb in DOCUMENT or IMPLEMENT lexicon fires first"
      correction: "ACTUALLY: 'requirements' fires DOCUMENT. However 'plan' does not fire any predicate.
        Final result: ['DOCUMENT'] because requirements IS a DOCUMENT term even in this sentence.
        This is a partial-predicate case — DOCUMENT fires, plan is ignored."
    - request: "Define the discount requirements for the sprint"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — define fires DOCUMENT, requirements fires DOCUMENT"
    - request: "Manage the discount engine delivery timeline"
      predicates: []
      result: "KNOWLEDGE_EDGE — manage and timeline are not predicate terms"
      capability_gap_id: "gap-discount-manage-timeline-001"
