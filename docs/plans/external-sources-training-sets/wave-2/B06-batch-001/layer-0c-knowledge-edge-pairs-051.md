# Layer 0-C — Knowledge-Edge Pairs (051)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B06-0C-051-001 — SplitPayment: gap verbs

```
source_id: xiigen-b06-0c-051-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-051-001-kedge"
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  Gap examples for SplitPayment:
    "Split the order total across two payment methods at checkout." → KNOWLEDGE_EDGE (split = operational)
    "Authorize the partial amount on each payment method." → KNOWLEDGE_EDGE (authorize = operational)
    "Capture the remaining balance from the secondary method." → KNOWLEDGE_EDGE (capture = operational)
    "Fallback to full payment on one method if the other fails." → KNOWLEDGE_EDGE (fallback = operational)
  Teaching: "order total", "payment methods", "partial amount", "remaining balance" are domain nouns.

---

## B06-0C-051-002 — SplitPayment: boundary cases

```
source_id: xiigen-b06-0c-051-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-051-002-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Boundary cases for SplitPayment:
    "Define the split payment requirements." → NOT knowledge-edge (define, requirements fire DOCUMENT)
    "Split payments per the specification." → NOT knowledge-edge (specification fires DOCUMENT)
    "Build the payment coordinator." → NOT knowledge-edge (build fires IMPLEMENT)
    "Split the payment across two methods." → KNOWLEDGE_EDGE (no predicate term present)

---

## B06-0C-051-003 — SplitPayment: gap vs spec vocabulary

```
source_id: xiigen-b06-0c-051-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-051-003-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Gap vs spec vocabulary for SplitPayment:
    gap: "split/authorize/capture/fallback/process payment methods"
    spec: "define/describe/specify/document split payment requirements"
  Teaching: "split payment", "payment method", "partial capture" are domain nouns — never fire predicates alone.
