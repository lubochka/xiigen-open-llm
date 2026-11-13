# Layer 0-C — Knowledge-Edge Pairs (049)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B06-0C-049-001 — ReturnWindowPolicy: gap verbs

```
source_id: xiigen-b06-0c-049-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-049-001-kedge"
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  Gap examples for ReturnWindowPolicy:
    "Check if the order is still within the return window." → KNOWLEDGE_EDGE (check = operational)
    "Reject the return request if the window has closed." → KNOWLEDGE_EDGE (reject = operational)
    "Extend the return window for holiday purchases." → KNOWLEDGE_EDGE (extend = operational)
    "Validate the purchase date against the return deadline." → KNOWLEDGE_EDGE (validate = operational)
  Teaching: "order", "return window", "return request", "purchase date", "return deadline" are domain nouns.

---

## B06-0C-049-002 — ReturnWindowPolicy: boundary cases

```
source_id: xiigen-b06-0c-049-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-049-002-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Boundary cases for ReturnWindowPolicy:
    "Define the return window policy requirements." → NOT knowledge-edge (define, requirements fire DOCUMENT)
    "Check returns per the specification." → NOT knowledge-edge (specification fires DOCUMENT)
    "Create the eligibility checker." → NOT knowledge-edge (create fires IMPLEMENT)
    "Check if the return window has expired." → KNOWLEDGE_EDGE (no predicate term present)

---

## B06-0C-049-003 — ReturnWindowPolicy: gap vs spec vocabulary

```
source_id: xiigen-b06-0c-049-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-049-003-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Gap vs spec vocabulary for ReturnWindowPolicy:
    gap: "check/reject/extend/validate/enforce return window"
    spec: "define/describe/specify/document return window policy requirements"
  Teaching: "return window", "30-day window", "holiday extension" are domain nouns — never fire predicates alone.
