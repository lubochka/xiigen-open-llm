# Layer 0-C — Knowledge-Edge Pairs (033)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B06-0C-033-001 — CrossSellUpsell: gap verbs

```
source_id: xiigen-b06-0c-033-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-033-001-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Metrics:
    gap_verbs_in_cross_sell_domain:
      "show upsell widget on product page" → KNOWLEDGE_EDGE (show = operational)
      "display cross-sell in checkout sidebar" → KNOWLEDGE_EDGE (display = operational)
      "trigger upsell prompt at cart review step" → KNOWLEDGE_EDGE (trigger = operational)
      "calculate price delta for upgrade option" → KNOWLEDGE_EDGE (calculate = operational)
      "rank cross-sell products by affinity score" → KNOWLEDGE_EDGE (rank = operational)

---

## B06-0C-033-002 — CrossSellUpsell: gap vs spec vocabulary

```
source_id: xiigen-b06-0c-033-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-033-002-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  gap_vs_spec_table:
    "gap: show upsell widget" vs "spec: define upsell display requirements" → spec fires DOCUMENT
    "gap: trigger prompt" vs "spec: specify trigger conditions" → spec fires DOCUMENT
    "gap: calculate price delta" vs "spec: document price delta calculation behavior" → spec fires DOCUMENT
    "gap: rank by affinity" vs "spec: describe affinity ranking criteria" → spec fires DOCUMENT

---

## B06-0C-033-003 — CrossSellUpsell: "specification" fires DOCUMENT

```
source_id: xiigen-b06-0c-033-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-033-003-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  boundary_case:
    "Display the upsell specification to the development team" → NOT knowledge-edge
      Reason: "specification" fires DOCUMENT via Rule 2. "Display" is operational but DOCUMENT fires.
      CompiledPredicateIds: ["DOCUMENT"]; Confidence: 0.78
    Teaching: "specification" (or "spec") fires DOCUMENT anywhere in the sentence.
    Operational verb context does not suppress DOCUMENT lexicon firing.
