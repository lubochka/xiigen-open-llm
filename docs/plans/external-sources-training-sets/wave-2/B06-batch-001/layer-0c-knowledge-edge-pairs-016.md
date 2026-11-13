# Layer 0-C — Knowledge-Edge Pairs (016)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## Method note

File 016 covers ReturnAndRefund knowledge-edge boundaries — three pairs.

---

## B06-0C-016-001 — Return: "criteria" fires DOCUMENT; "handle" does not

```
source_id: xiigen-b06-0c-016-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-016-001-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Cases:
    - request: "Define the criteria for return eligibility at checkout"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — define fires DOCUMENT, criteria fires DOCUMENT"
    - request: "Handle return requests from the checkout confirmation page"
      predicates: []
      result: "KNOWLEDGE_EDGE — handle is not in any predicate lexicon"
      capability_gap_id: "gap-return-handle-001"
    - request: "Describe the refund use cases for partial returns"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — describe fires DOCUMENT, use cases fires DOCUMENT"

---

## B06-0C-016-002 — Return: "process" vs "document" boundary

```
source_id: xiigen-b06-0c-016-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-016-002-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Cases:
    - request: "Document the return process requirements"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — document fires DOCUMENT, requirements fires DOCUMENT"
    - request: "Process returns through the checkout confirmation screen"
      predicates: []
      result: "KNOWLEDGE_EDGE — process alone is not a predicate term in this context"
      capability_gap_id: "gap-return-process-001"
    - request: "Specify the return processing behavior"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — specify fires DOCUMENT, behavior fires DOCUMENT"

---

## B06-0C-016-003 — Return: "roadmap" vs "epics" boundary

```
source_id: xiigen-b06-0c-016-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-016-003-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Cases:
    - request: "Define the epics for end-to-end refund processing"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — define fires DOCUMENT, epics fires DOCUMENT"
    - request: "Build a roadmap for the refund capability improvements"
      predicates: ["IMPLEMENT"]
      result: "NOT knowledge-edge — build fires IMPLEMENT. roadmap alone does not fire but build does."
    - request: "Roadmap the refund improvements for next quarter"
      predicates: []
      result: "KNOWLEDGE_EDGE — roadmap used as a verb does not fire any predicate"
      capability_gap_id: "gap-return-roadmap-001"
