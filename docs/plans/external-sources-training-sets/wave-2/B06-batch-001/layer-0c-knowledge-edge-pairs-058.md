# Layer 0-C — Knowledge-Edge Pairs (058)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B06-0C-058-001 — AutoReplenishment: gap verbs

```
source_id: xiigen-b06-0c-058-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-058-001-kedge"
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  Gap examples for AutoReplenishment:
    "Automatically reorder the product every 30 days for the subscribed customer." → KNOWLEDGE_EDGE (reorder = operational)
    "Skip the next delivery when the customer requests a pause." → KNOWLEDGE_EDGE (skip = operational)
    "Process the recurring payment on the scheduled date." → KNOWLEDGE_EDGE (process = operational)
    "Cancel the subscription after three failed payment retries." → KNOWLEDGE_EDGE (cancel = operational)
  Teaching: "product", "subscribed customer", "next delivery", "recurring payment" are domain nouns.

---

## B06-0C-058-002 — AutoReplenishment: boundary cases

```
source_id: xiigen-b06-0c-058-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-058-002-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Boundary cases for AutoReplenishment:
    "Define the auto-replenishment requirements." → NOT knowledge-edge (define, requirements fire DOCUMENT)
    "Reorder products per the specification." → NOT knowledge-edge (specification fires DOCUMENT)
    "Create the recurring order scheduler." → NOT knowledge-edge (create fires IMPLEMENT)
    "Reorder products every 30 days automatically." → KNOWLEDGE_EDGE (no predicate term present)

---

## B06-0C-058-003 — AutoReplenishment: gap vs spec vocabulary

```
source_id: xiigen-b06-0c-058-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-058-003-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Gap vs spec vocabulary for AutoReplenishment:
    gap: "reorder/skip/process/cancel/schedule auto-replenishment"
    spec: "define/describe/specify/document auto-replenishment requirements"
  Teaching: "auto-replenishment", "recurring order", "subscription frequency", "skip delivery" are domain nouns — never fire predicates alone.
