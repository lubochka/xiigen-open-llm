# Layer 0-C — Knowledge-Edge Pairs (031)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B06-0C-031-001 — AbandonedCartRecovery: gap verbs

```
source_id: xiigen-b06-0c-031-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-031-001-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Metrics:
    gap_verbs_in_abandoned_cart_domain:
      "send recovery email after two hours" → KNOWLEDGE_EDGE (send = operational)
      "trigger notification on cart abandonment" → KNOWLEDGE_EDGE (trigger = operational)
      "schedule follow-up email at 24 hours" → KNOWLEDGE_EDGE (schedule = operational)
      "personalize recovery message with cart items" → KNOWLEDGE_EDGE (personalize = operational)
      "suppress recovery email if order completed" → KNOWLEDGE_EDGE (suppress = operational)

---

## B06-0C-031-002 — AbandonedCartRecovery: gap vs spec vocabulary

```
source_id: xiigen-b06-0c-031-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-031-002-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  gap_vs_spec_table:
    "gap: send recovery email" vs "spec: specify email timing window requirements" → spec fires DOCUMENT
    "gap: trigger notification" vs "spec: define abandonment notification behavior" → spec fires DOCUMENT
    "gap: schedule follow-up" vs "spec: document follow-up schedule criteria" → spec fires DOCUMENT
    "gap: suppress if ordered" vs "spec: describe suppression conditions" → spec fires DOCUMENT

---

## B06-0C-031-003 — AbandonedCartRecovery: "story" fires DOCUMENT

```
source_id: xiigen-b06-0c-031-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-031-003-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  boundary_case:
    "Trigger a recovery story for returning cart customers" → NOT knowledge-edge
      Reason: "story" fires DOCUMENT via Rule 8 (story). "Trigger" is operational but DOCUMENT fires.
      CompiledPredicateIds: ["DOCUMENT"]; Confidence: 0.78
    Teaching: "story" is a DOCUMENT lexicon term. It fires regardless of surrounding verbs.
