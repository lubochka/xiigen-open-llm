# Layer 0-C — Knowledge-Edge Pairs (020)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## Method note

File 020 covers SubscriptionBilling knowledge-edge boundaries — three pairs.

---

## B06-0C-020-001 — Subscription: "epics" fires DOCUMENT; "schedule" does not

```
source_id: xiigen-b06-0c-020-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-020-001-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Cases:
    - request: "Specify the epics for subscription lifecycle management"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — specify fires DOCUMENT, epics fires DOCUMENT"
    - request: "Schedule subscription renewals during the checkout flow"
      predicates: []
      result: "KNOWLEDGE_EDGE — schedule is not in any predicate lexicon"
      capability_gap_id: "gap-subscription-schedule-001"
    - request: "Define the subscription billing requirements"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge"

---

## B06-0C-020-002 — Subscription: "cancel" and "renew" are gaps; "behavior" is not

```
source_id: xiigen-b06-0c-020-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-020-002-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Cases:
    - request: "Document the renewal behavior and cancellation use cases"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — document fires DOCUMENT, behavior fires DOCUMENT, use cases fires DOCUMENT"
    - request: "Cancel subscriptions that fail payment at checkout"
      predicates: []
      result: "KNOWLEDGE_EDGE — cancel alone is not in any predicate lexicon"
      capability_gap_id: "gap-subscription-cancel-001"
    - request: "Renew subscriptions automatically at checkout"
      predicates: []
      result: "KNOWLEDGE_EDGE — renew alone is not in any predicate lexicon"
      capability_gap_id: "gap-subscription-renew-001"

---

## B06-0C-020-003 — Subscription: "plan the roadmap" is gap; "describe the narrative" is not

```
source_id: xiigen-b06-0c-020-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-020-003-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Cases:
    - request: "Describe the narrative for subscription checkout integration"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — describe fires DOCUMENT, narrative fires DOCUMENT"
    - request: "Plan the roadmap for subscription billing improvements"
      predicates: []
      result: "KNOWLEDGE_EDGE — plan is not a predicate; roadmap alone is not a predicate"
      capability_gap_id: "gap-subscription-roadmap-001"
    - request: "Manage the subscription billing sprint backlog"
      predicates: []
      result: "KNOWLEDGE_EDGE — manage is not a predicate term"
      capability_gap_id: "gap-subscription-manage-backlog-001"
