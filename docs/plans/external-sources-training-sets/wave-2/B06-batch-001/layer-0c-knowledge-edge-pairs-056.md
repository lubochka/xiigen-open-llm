# Layer 0-C — Knowledge-Edge Pairs (056)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B06-0C-056-001 — PriceMatchPolicy: gap verbs

```
source_id: xiigen-b06-0c-056-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-056-001-kedge"
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  Gap examples for PriceMatchPolicy:
    "Match the competitor price when the customer submits a valid request." → KNOWLEDGE_EDGE (match = operational)
    "Verify the competitor price from an approved source." → KNOWLEDGE_EDGE (verify = operational)
    "Adjust the product price to match the competitor." → KNOWLEDGE_EDGE (adjust = operational)
    "Limit one price match per order." → KNOWLEDGE_EDGE (limit = operational)
  Teaching: "competitor price", "price match request", "product price", "approved source" are domain nouns.

---

## B06-0C-056-002 — PriceMatchPolicy: boundary cases

```
source_id: xiigen-b06-0c-056-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-056-002-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Boundary cases for PriceMatchPolicy:
    "Define the price match policy requirements." → NOT knowledge-edge (define, requirements fire DOCUMENT)
    "Match prices per the specification." → NOT knowledge-edge (specification fires DOCUMENT)
    "Create the price match handler." → NOT knowledge-edge (create fires IMPLEMENT)
    "Match the competitor price on request." → KNOWLEDGE_EDGE (no predicate term present)

---

## B06-0C-056-003 — PriceMatchPolicy: gap vs spec vocabulary

```
source_id: xiigen-b06-0c-056-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-056-003-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Gap vs spec vocabulary for PriceMatchPolicy:
    gap: "match/verify/adjust/limit/approve price match"
    spec: "define/describe/specify/document price match policy requirements"
  Teaching: "price match", "competitor price", "verification window" are domain nouns — never fire predicates alone.
