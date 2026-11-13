# Layer 0-C — Knowledge-Edge Pairs (055)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B06-0C-055-001 — ProductBundleCreator: gap verbs

```
source_id: xiigen-b06-0c-055-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-055-001-kedge"
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  Gap examples for ProductBundleCreator:
    "Combine three products into a bundle at a discounted price." → KNOWLEDGE_EDGE (combine = operational)
    "Reserve inventory for each bundle component." → KNOWLEDGE_EDGE (reserve = operational)
    "Compute the bundle price from its component prices." → KNOWLEDGE_EDGE (compute = operational)
    "Restrict individual sale of bundle-only items." → KNOWLEDGE_EDGE (restrict = operational)
  Teaching: "products", "bundle", "discounted price", "bundle component", "component prices" are domain nouns.

---

## B06-0C-055-002 — ProductBundleCreator: boundary cases

```
source_id: xiigen-b06-0c-055-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-055-002-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Boundary cases for ProductBundleCreator:
    "Define the product bundle requirements." → NOT knowledge-edge (define, requirements fire DOCUMENT)
    "Combine products per the specification." → NOT knowledge-edge (specification fires DOCUMENT)
    "Build the bundle pricing engine." → NOT knowledge-edge (build fires IMPLEMENT)
    "Combine products into a bundle." → KNOWLEDGE_EDGE (no predicate term present)

---

## B06-0C-055-003 — ProductBundleCreator: gap vs spec vocabulary

```
source_id: xiigen-b06-0c-055-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-055-003-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Gap vs spec vocabulary for ProductBundleCreator:
    gap: "combine/reserve/compute/restrict/price product bundles"
    spec: "define/describe/specify/document product bundle requirements"
  Teaching: "bundle", "component selection", "bundle pricing override" are domain nouns — never fire predicates alone.
