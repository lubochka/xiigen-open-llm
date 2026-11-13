# Layer 0-C — Knowledge-Edge Pairs (028)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B06-0C-028-001 — BundlePricing: gap verbs

```
source_id: xiigen-b06-0c-028-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-028-001-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Metrics:
    gap_verbs_in_bundle_pricing_domain:
      "calculate bundle discount at checkout" → KNOWLEDGE_EDGE (calculate = operational)
      "apply bundle price to cart" → KNOWLEDGE_EDGE (apply = operational)
      "match SKU combinations for bundle eligibility" → KNOWLEDGE_EDGE (match = operational)
      "compute nested discount sequence" → KNOWLEDGE_EDGE (compute = operational)
      "display bundle savings on product page" → KNOWLEDGE_EDGE (display = operational)

---

## B06-0C-028-002 — BundlePricing: gap vs spec vocabulary

```
source_id: xiigen-b06-0c-028-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-028-002-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  gap_vs_spec_table:
    "gap: calculate bundle pricing" vs "spec: define bundle pricing rules" → spec fires DOCUMENT
    "gap: apply discounts to cart" vs "spec: specify discount application behavior" → spec fires DOCUMENT
    "gap: display savings" vs "spec: document bundle savings display requirements" → spec fires DOCUMENT

---

## B06-0C-028-003 — BundlePricing: "narrative" fires DOCUMENT even in gap context

```
source_id: xiigen-b06-0c-028-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-028-003-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  boundary_case:
    "Calculate the bundle pricing narrative for the product team" → NOT knowledge-edge
      Reason: "narrative" fires DOCUMENT via Rule 13 despite "Calculate" being a gap verb.
      CompiledPredicateIds: ["DOCUMENT"]; Confidence: 0.78
    Teaching: Rule 13 ("narrative" fires DOCUMENT) applies anywhere in sentence.
    The lexicon scan finds "narrative" even when surrounded by operational verbs and domain nouns.
