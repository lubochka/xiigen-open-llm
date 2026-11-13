# Layer 0-C — Knowledge-Edge Pairs (023)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## Method note

File 023 covers multi-domain knowledge-edge sweep — three pairs testing gap
boundaries across the new domains introduced in files 013-022.

---

## B06-0C-023-001 — Multi-domain gap sweep: plan/manage/monitor

```
source_id: xiigen-b06-0c-023-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-023-001-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Cases:
    - request: "Plan the roadmap for shipping, loyalty, and subscription billing improvements"
      predicates: []
      result: "KNOWLEDGE_EDGE — plan and roadmap are not predicate terms"
      capability_gap_id: "gap-multi-domain-roadmap-plan-001"
    - request: "Manage the delivery of fraud detection and privacy requirements"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — requirements fires DOCUMENT; manage does not fire but requirements does"
      note: "Partial predicate: requirements is a DOCUMENT term even when manage appears"
    - request: "Monitor tax and currency calculation performance during checkout"
      predicates: []
      result: "KNOWLEDGE_EDGE — monitor and performance (without SLA framing) are not predicate terms"
      capability_gap_id: "gap-multi-domain-monitor-001"

---

## B06-0C-023-002 — DOCUMENT verbs with non-predicate objects do not change outcome

```
source_id: xiigen-b06-0c-023-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-023-002-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Cases:
    - request: "Describe how the shipping carrier system works"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — describe fires DOCUMENT even when the object is not a requirement noun"
    - request: "Define the quarterly shipping goals"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — define fires DOCUMENT even for non-requirement objects"
      note: "Predicate fires on verb presence alone; object does not suppress firing"
    - request: "Specify what fraud protection the team needs"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — specify fires DOCUMENT"

---

## B06-0C-023-003 — Combined gap vocabulary: all non-firing terms in new domains

```
source_id: xiigen-b06-0c-023-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-023-003-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Metrics:
    gap_vocabulary_new_domains:
      "shipping domain gaps": ["plan", "manage", "track", "route", "ship", "deliver"]
      "inventory domain gaps": ["track", "monitor", "reserve", "release", "optimize"]
      "loyalty domain gaps": ["promote", "accrue", "redeem", "tier"]
      "return domain gaps": ["handle", "process", "return", "refund"]
      "tax domain gaps": ["audit", "comply", "calculate", "estimate"]
      "currency domain gaps": ["convert", "display", "format", "round"]
      "bundle domain gaps": ["configure", "apply", "calculate", "stack"]
      "subscription domain gaps": ["schedule", "cancel", "renew", "charge"]
      "fraud domain gaps": ["detect", "block", "alert", "investigate", "review"]
      "privacy domain gaps": ["comply", "delete", "anonymize", "mask"]
    teaching_note: "None of these domain-specific verbs fire any predicate.
      They represent operational actions, not requirements-authoring actions.
      A request must contain a DOCUMENT, IMPLEMENT, ARCHITECTURE, or UI
      lexicon term to produce a non-empty predicate set."
