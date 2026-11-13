# Layer 0-C — Knowledge-Edge Pairs (030)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B06-0C-030-001 — DropshippingRules: gap verbs

```
source_id: xiigen-b06-0c-030-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-030-001-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Metrics:
    gap_verbs_in_dropshipping_domain:
      "route order to nearest supplier" → KNOWLEDGE_EDGE (route = operational)
      "split shipment across two suppliers" → KNOWLEDGE_EDGE (split = operational)
      "fallback to secondary supplier on stockout" → KNOWLEDGE_EDGE (fallback = operational)
      "notify supplier of new order" → KNOWLEDGE_EDGE (notify = operational)
      "track dropship order status" → KNOWLEDGE_EDGE (track = operational)
      "coordinate multi-supplier shipments" → KNOWLEDGE_EDGE (coordinate = operational)

---

## B06-0C-030-002 — DropshippingRules: gap vs spec vocabulary

```
source_id: xiigen-b06-0c-030-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-030-002-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  gap_vs_spec_table:
    "gap: route order" vs "spec: define supplier routing rules" → spec fires DOCUMENT
    "gap: split shipment" vs "spec: specify split shipment policy" → spec fires DOCUMENT
    "gap: fallback to secondary" vs "spec: document failover behavior" → spec fires DOCUMENT
    "gap: coordinate shipments" vs "spec: describe supplier coordination requirements" → spec fires DOCUMENT

---

## B06-0C-030-003 — DropshippingRules: "capability" fires DOCUMENT

```
source_id: xiigen-b06-0c-030-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-030-003-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  boundary_case:
    "Coordinate dropship capability requirements across all suppliers" → NOT knowledge-edge
      Reason: "capability" fires DOCUMENT via Rule 16; "requirements" fires DOCUMENT via Rule 3.
      CompiledPredicateIds: ["DOCUMENT"]; Confidence: 0.78
    Teaching: Gap verbs ("coordinate") do not suppress DOCUMENT from "capability" or "requirements".
