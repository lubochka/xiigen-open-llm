# Layer 0-C — Knowledge-Edge Pairs (044)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B06-0C-044-001 — MinimumOrderValue: gap verbs

```
source_id: xiigen-b06-0c-044-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-044-001-kedge"
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  Gap examples for MinimumOrderValue:
    "Block checkout when the cart total is below the minimum." → KNOWLEDGE_EDGE (block = operational)
    "Validate the cart total against the minimum order threshold." → KNOWLEDGE_EDGE (validate = operational)
    "Show an error when the order value is insufficient." → KNOWLEDGE_EDGE (show = operational)
    "Reject the order if it falls below the channel minimum." → KNOWLEDGE_EDGE (reject = operational)
  Teaching: "cart total", "minimum order threshold", "channel minimum", "order value" are domain nouns.

---

## B06-0C-044-002 — MinimumOrderValue: boundary cases

```
source_id: xiigen-b06-0c-044-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-044-002-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Boundary cases for MinimumOrderValue:
    "Describe the minimum order value requirements." → NOT knowledge-edge (describe, requirements fire DOCUMENT)
    "Block orders per the specification." → NOT knowledge-edge (specification fires DOCUMENT)
    "Develop the validation service." → NOT knowledge-edge (develop fires IMPLEMENT)
    "Block orders below the minimum." → KNOWLEDGE_EDGE (no predicate term present)

---

## B06-0C-044-003 — MinimumOrderValue: gap vs spec vocabulary

```
source_id: xiigen-b06-0c-044-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-044-003-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Gap vs spec vocabulary for MinimumOrderValue:
    gap: "block/validate/show/reject/enforce minimum order"
    spec: "describe/define/specify/document minimum order value requirements"
  Teaching: "minimum order", "cart total", "order threshold" are domain nouns — never fire predicates alone.
