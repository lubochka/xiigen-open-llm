# Layer 0-C — Knowledge-Edge Pairs (050)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B06-0C-050-001 — WarrantyTracking: gap verbs

```
source_id: xiigen-b06-0c-050-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-050-001-kedge"
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  Gap examples for WarrantyTracking:
    "Track the warranty expiry date for each purchased product." → KNOWLEDGE_EDGE (track = operational)
    "Notify the customer 30 days before warranty expiry." → KNOWLEDGE_EDGE (notify = operational)
    "Register the manufacturer warranty after purchase." → KNOWLEDGE_EDGE (register = operational)
    "Check if the product is still under warranty." → KNOWLEDGE_EDGE (check = operational)
  Teaching: "warranty expiry date", "purchased product", "manufacturer warranty" are domain nouns.

---

## B06-0C-050-002 — WarrantyTracking: boundary cases

```
source_id: xiigen-b06-0c-050-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-050-002-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Boundary cases for WarrantyTracking:
    "Describe the warranty tracking requirements." → NOT knowledge-edge (describe, requirements fire DOCUMENT)
    "Track warranties per the specification." → NOT knowledge-edge (specification fires DOCUMENT)
    "Develop the warranty ledger service." → NOT knowledge-edge (develop fires IMPLEMENT)
    "Track warranty expiry for each product." → KNOWLEDGE_EDGE (no predicate term present)

---

## B06-0C-050-003 — WarrantyTracking: gap vs spec vocabulary

```
source_id: xiigen-b06-0c-050-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-050-003-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Gap vs spec vocabulary for WarrantyTracking:
    gap: "track/notify/register/check/expire warranty"
    spec: "describe/define/specify/document warranty tracking requirements"
  Teaching: "warranty", "manufacturer warranty", "extended warranty", "expiry" are domain nouns — never fire predicates alone.
