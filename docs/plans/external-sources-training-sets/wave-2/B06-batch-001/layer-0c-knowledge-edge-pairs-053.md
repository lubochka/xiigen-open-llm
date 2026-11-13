# Layer 0-C — Knowledge-Edge Pairs (053)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B06-0C-053-001 — StoreCreditManagement: gap verbs

```
source_id: xiigen-b06-0c-053-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-053-001-kedge"
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  Gap examples for StoreCreditManagement:
    "Issue store credit to the customer after a successful return." → KNOWLEDGE_EDGE (issue = operational)
    "Deduct the spent credit from the customer's balance." → KNOWLEDGE_EDGE (deduct = operational)
    "Expire unused store credit after 12 months." → KNOWLEDGE_EDGE (expire = operational)
    "Apply store credit to the cart during checkout." → KNOWLEDGE_EDGE (apply = operational)
  Teaching: "store credit", "customer's balance", "unused store credit", "cart" are domain nouns.

---

## B06-0C-053-002 — StoreCreditManagement: boundary cases

```
source_id: xiigen-b06-0c-053-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-053-002-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Boundary cases for StoreCreditManagement:
    "Define the store credit management requirements." → NOT knowledge-edge (define, requirements fire DOCUMENT)
    "Issue credit per the specification." → NOT knowledge-edge (specification fires DOCUMENT)
    "Build the credit ledger." → NOT knowledge-edge (build fires IMPLEMENT)
    "Issue credit after each return." → KNOWLEDGE_EDGE (no predicate term present)

---

## B06-0C-053-003 — StoreCreditManagement: gap vs spec vocabulary

```
source_id: xiigen-b06-0c-053-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-053-003-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Gap vs spec vocabulary for StoreCreditManagement:
    gap: "issue/deduct/expire/apply/track store credit"
    spec: "define/describe/specify/document store credit management requirements"
  Teaching: "store credit", "credit balance", "partial spend" are domain nouns — never fire predicates alone.
