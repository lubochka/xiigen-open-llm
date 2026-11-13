# Layer 0-C — Knowledge-Edge Pairs (043)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B06-0C-043-001 — FreeShippingThreshold: gap verbs

```
source_id: xiigen-b06-0c-043-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-043-001-kedge"
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  Gap examples for FreeShippingThreshold:
    "Waive shipping costs when the cart reaches the threshold." → KNOWLEDGE_EDGE (waive = operational)
    "Check if the cart total meets the free shipping minimum." → KNOWLEDGE_EDGE (check = operational)
    "Remove the shipping fee from the order summary." → KNOWLEDGE_EDGE (remove = operational)
    "Notify the customer they qualify for free shipping." → KNOWLEDGE_EDGE (notify = operational)
  Teaching: "shipping costs", "cart total", "free shipping minimum", "shipping fee" are domain nouns.

---

## B06-0C-043-002 — FreeShippingThreshold: boundary cases

```
source_id: xiigen-b06-0c-043-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-043-002-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Boundary cases for FreeShippingThreshold:
    "Define the free shipping threshold requirements." → NOT knowledge-edge (define, requirements fire DOCUMENT)
    "Waive shipping per the specification." → NOT knowledge-edge (specification fires DOCUMENT)
    "Build the eligibility checker." → NOT knowledge-edge (build fires IMPLEMENT)
    "Waive shipping when threshold is met." → KNOWLEDGE_EDGE (no predicate term present)

---

## B06-0C-043-003 — FreeShippingThreshold: gap vs spec vocabulary

```
source_id: xiigen-b06-0c-043-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-043-003-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Gap vs spec vocabulary for FreeShippingThreshold:
    gap: "waive/check/remove/notify/evaluate shipping threshold"
    spec: "define/describe/specify/document free shipping threshold requirements"
  Teaching: "free shipping", "threshold", "cart minimum" are domain nouns — never fire predicates alone.
