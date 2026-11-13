# Layer 0-C — Knowledge-Edge Pairs (038)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B06-0C-038-001 — DynamicPricing: gap verbs

```
source_id: xiigen-b06-0c-038-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-038-001-kedge"
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  Gap examples for DynamicPricing:
    "Recalculate product prices every hour based on demand signals." → KNOWLEDGE_EDGE (recalculate = operational)
    "Adjust price based on competitor feed." → KNOWLEDGE_EDGE (adjust = operational)
    "Monitor competitor prices and update accordingly." → KNOWLEDGE_EDGE (monitor, update = operational)
    "Apply the new price to all active listings." → KNOWLEDGE_EDGE (apply = operational)
  Teaching: "dynamic pricing", "demand signals", "competitor feed", "price" are domain nouns — none fire predicates.

---

## B06-0C-038-002 — DynamicPricing: boundary cases

```
source_id: xiigen-b06-0c-038-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-038-002-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Boundary cases for DynamicPricing:
    "Describe dynamic pricing." → NOT knowledge-edge (describe fires DOCUMENT)
    "Adjust prices per the specification." → NOT knowledge-edge (specification fires DOCUMENT)
    "Implement the pricing engine." → NOT knowledge-edge (implement fires IMPLEMENT)
    "Recalculate prices hourly." → KNOWLEDGE_EDGE (no predicate term present)

---

## B06-0C-038-003 — DynamicPricing: gap vs spec vocabulary

```
source_id: xiigen-b06-0c-038-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-038-003-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Gap vs spec vocabulary for DynamicPricing:
    gap: "recalculate/adjust/monitor/apply/update price"
    spec: "define/describe/specify/document pricing requirements"
  Gap verb types: calculation, adjustment, monitoring, application, comparison.
  Teaching: operational verbs for pricing work never fire predicates alone.
