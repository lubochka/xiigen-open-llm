# Layer 0-C — Knowledge-Edge Pairs (029)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B06-0C-029-001 — WholesaleDiscount: gap verbs

```
source_id: xiigen-b06-0c-029-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-029-001-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Metrics:
    gap_verbs_in_wholesale_domain:
      "calculate wholesale price for bulk order" → KNOWLEDGE_EDGE (calculate = operational)
      "assign account tier to B2B customer" → KNOWLEDGE_EDGE (assign = operational)
      "negotiate contract pricing agreement" → KNOWLEDGE_EDGE (negotiate = operational)
      "approve price override for wholesale account" → KNOWLEDGE_EDGE (approve = operational)
      "enforce minimum order quantity for wholesale" → KNOWLEDGE_EDGE (enforce = operational)

---

## B06-0C-029-002 — WholesaleDiscount: gap vs spec vocabulary

```
source_id: xiigen-b06-0c-029-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-029-002-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  gap_vs_spec_table:
    "gap: calculate wholesale price" vs "spec: describe wholesale pricing tiers" → spec fires DOCUMENT
    "gap: assign account tier" vs "spec: define account tier assignment criteria" → spec fires DOCUMENT
    "gap: negotiate agreement" vs "spec: document pricing agreement requirements" → spec fires DOCUMENT
    "gap: enforce minimum quantity" vs "spec: specify minimum order quantity requirements" → spec fires DOCUMENT

---

## B06-0C-029-003 — WholesaleDiscount: "epics" fires DOCUMENT even in gap context

```
source_id: xiigen-b06-0c-029-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-029-003-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  boundary_case:
    "Negotiate the wholesale pricing epics for the B2B roadmap" → NOT knowledge-edge
      Reason: "epics" fires DOCUMENT via Rule 15 despite "negotiate" being a gap verb.
      CompiledPredicateIds: ["DOCUMENT"]; Confidence: 0.78
    Teaching: DOCUMENT lexicon terms prevent knowledge-edge classification regardless
      of the surrounding operational verbs. Full-text scan, not position-dependent.
