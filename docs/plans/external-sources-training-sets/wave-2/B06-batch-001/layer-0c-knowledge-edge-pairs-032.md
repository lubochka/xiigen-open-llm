# Layer 0-C — Knowledge-Edge Pairs (032)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B06-0C-032-001 — ProductRecommendation: gap verbs

```
source_id: xiigen-b06-0c-032-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-032-001-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Metrics:
    gap_verbs_in_recommendation_domain:
      "show related products on item page" → KNOWLEDGE_EDGE (show = operational)
      "filter recommendations by category" → KNOWLEDGE_EDGE (filter = operational)
      "rank products by purchase affinity" → KNOWLEDGE_EDGE (rank = operational)
      "cache recommendation results for 15 minutes" → KNOWLEDGE_EDGE (cache = operational)
      "refresh recommendations on cart update" → KNOWLEDGE_EDGE (refresh = operational)

---

## B06-0C-032-002 — ProductRecommendation: gap vs spec vocabulary

```
source_id: xiigen-b06-0c-032-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-032-002-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  gap_vs_spec_table:
    "gap: show related products" vs "spec: define product recommendation requirements" → spec fires DOCUMENT
    "gap: filter recommendations" vs "spec: specify filtering behavior" → spec fires DOCUMENT
    "gap: rank by affinity" vs "spec: document ranking criteria" → spec fires DOCUMENT
    "gap: cache results" vs "spec: describe caching requirements for recommendations" → spec fires DOCUMENT

---

## B06-0C-032-003 — ProductRecommendation: "feature-brief" and "stakeholder" firing

```
source_id: xiigen-b06-0c-032-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-032-003-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  boundary_cases:
    "Cache a feature-brief for the recommendation engine" → NOT knowledge-edge
      Reason: "feature-brief" fires DOCUMENT via Rule 14.
      CompiledPredicateIds: ["DOCUMENT"]; Confidence: 0.78
    "Show stakeholder the recommendation engine behavior" → NOT knowledge-edge
      Reason: "stakeholder" fires DOCUMENT via Rule 8. Also triggers 2.4 routing signal.
      CompiledPredicateIds: ["DOCUMENT"]; Confidence: 0.78
    Teaching: DOCUMENT lexicon terms fire even when embedded in operational sentences.
