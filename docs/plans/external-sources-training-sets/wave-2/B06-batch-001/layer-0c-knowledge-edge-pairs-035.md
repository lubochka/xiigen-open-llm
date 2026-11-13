# Layer 0-C — Knowledge-Edge Pairs (035)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B06-0C-035-001 — DigitalProductDelivery: gap verbs

```
source_id: xiigen-b06-0c-035-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-035-001-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Metrics:
    gap_verbs_in_digital_delivery_domain:
      "email download link after payment" → KNOWLEDGE_EDGE (email = operational)
      "generate license key on purchase" → KNOWLEDGE_EDGE (generate = operational)
      "revoke access on refund" → KNOWLEDGE_EDGE (revoke = operational)
      "throttle concurrent downloads per account" → KNOWLEDGE_EDGE (throttle = operational)
      "expire download URL after 48 hours" → KNOWLEDGE_EDGE (expire = operational)
      "reissue license key on account transfer" → KNOWLEDGE_EDGE (reissue = operational)

---

## B06-0C-035-002 — DigitalProductDelivery: gap vs spec vocabulary

```
source_id: xiigen-b06-0c-035-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-035-002-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  gap_vs_spec_table:
    "gap: email download link" vs "spec: define digital delivery requirements" → spec fires DOCUMENT
    "gap: generate license key" vs "spec: specify license key generation behavior" → spec fires DOCUMENT
    "gap: revoke access" vs "spec: document access revocation policy" → spec fires DOCUMENT
    "gap: expire download URL" vs "spec: describe URL expiry criteria" → spec fires DOCUMENT

---

## B06-0C-035-003 — DigitalProductDelivery: "architecture" fires ARCHITECTURE

```
source_id: xiigen-b06-0c-035-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-035-003-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  boundary_case:
    "Generate CDN architecture for digital delivery" → NOT knowledge-edge
      Reason: "architecture" fires ARCHITECTURE via Rule A-02. "Generate" is operational but ARCHITECTURE fires.
      CompiledPredicateIds: ["ARCHITECTURE"]; Confidence: 0.82
    Teaching: ARCHITECTURE lexicon terms ("architecture", "design") also prevent knowledge-edge.
    Knowledge-edge requires ZERO predicate-lexicon terms (DOCUMENT, IMPLEMENT, ARCHITECTURE, UI).
