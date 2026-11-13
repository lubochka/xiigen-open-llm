# Layer 0-C — Knowledge-Edge Pairs (046)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B06-0C-046-001 — PreorderRules: gap verbs

```
source_id: xiigen-b06-0c-046-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-046-001-kedge"
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  Gap examples for PreorderRules:
    "Charge the remaining balance when the preorder item ships." → KNOWLEDGE_EDGE (charge = operational)
    "Authorize the deposit at preorder placement." → KNOWLEDGE_EDGE (authorize = operational)
    "Cancel the preorder if the release date is missed." → KNOWLEDGE_EDGE (cancel = operational)
    "Schedule the charge for the release date." → KNOWLEDGE_EDGE (schedule = operational)
  Teaching: "remaining balance", "preorder item", "deposit", "release date" are domain nouns.

---

## B06-0C-046-002 — PreorderRules: boundary cases

```
source_id: xiigen-b06-0c-046-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-046-002-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Boundary cases for PreorderRules:
    "Define the preorder requirements." → NOT knowledge-edge (define, requirements fire DOCUMENT)
    "Charge customers per the specification." → NOT knowledge-edge (specification fires DOCUMENT)
    "Build the preorder lifecycle manager." → NOT knowledge-edge (build fires IMPLEMENT)
    "Charge the balance on release day." → KNOWLEDGE_EDGE (no predicate term present)

---

## B06-0C-046-003 — PreorderRules: gap vs spec vocabulary

```
source_id: xiigen-b06-0c-046-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-046-003-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Gap vs spec vocabulary for PreorderRules:
    gap: "charge/authorize/cancel/schedule/collect preorder payment"
    spec: "define/describe/specify/document preorder rules requirements"
  Teaching: "preorder", "deposit capture", "release date charge" are domain nouns — never fire predicates alone.
