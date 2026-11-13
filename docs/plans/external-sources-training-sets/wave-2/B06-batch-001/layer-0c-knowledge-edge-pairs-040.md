# Layer 0-C — Knowledge-Edge Pairs (040)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B06-0C-040-001 — VolumeDiscount: gap verbs

```
source_id: xiigen-b06-0c-040-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-040-001-kedge"
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  Gap examples for VolumeDiscount:
    "Calculate the discount when a customer buys more than 10 units." → KNOWLEDGE_EDGE (calculate = operational)
    "Apply the tiered percentage to the cart total." → KNOWLEDGE_EDGE (apply = operational)
    "Check if the cart quantity crosses a break point." → KNOWLEDGE_EDGE (check = operational)
    "Aggregate volume across multiple SKUs." → KNOWLEDGE_EDGE (aggregate = operational)
  Teaching: "volume discount", "tiered percentage", "cart total", "break point", "SKU" are domain nouns.

---

## B06-0C-040-002 — VolumeDiscount: boundary cases

```
source_id: xiigen-b06-0c-040-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-040-002-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Boundary cases for VolumeDiscount:
    "Describe the volume discount requirements." → NOT knowledge-edge (describe, requirements fire DOCUMENT)
    "Calculate discounts per the specification." → NOT knowledge-edge (specification fires DOCUMENT)
    "Build the discount calculator." → NOT knowledge-edge (build fires IMPLEMENT)
    "Calculate the discount for 10+ units." → KNOWLEDGE_EDGE (no predicate term present)

---

## B06-0C-040-003 — VolumeDiscount: gap vs spec vocabulary

```
source_id: xiigen-b06-0c-040-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-040-003-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Gap vs spec vocabulary for VolumeDiscount:
    gap: "calculate/apply/check/aggregate/evaluate volume discount"
    spec: "describe/define/specify/document volume discount requirements"
  Teaching: "discount", "quantity break", "tiered pricing" are domain nouns — never fire predicates alone.
