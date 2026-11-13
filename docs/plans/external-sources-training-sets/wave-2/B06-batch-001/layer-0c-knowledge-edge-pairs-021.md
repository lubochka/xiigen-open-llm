# Layer 0-C — Knowledge-Edge Pairs (021)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## Method note

File 021 covers FraudDetection knowledge-edge boundaries — three pairs.

---

## B06-0C-021-001 — Fraud: "criteria" fires DOCUMENT; "detect" does not

```
source_id: xiigen-b06-0c-021-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-021-001-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Cases:
    - request: "Document the fraud scoring criteria and behavior"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — document fires DOCUMENT, criteria fires DOCUMENT, behavior fires DOCUMENT"
    - request: "Detect fraudulent checkout attempts in real time"
      predicates: []
      result: "KNOWLEDGE_EDGE — detect is not in any predicate lexicon"
      capability_gap_id: "gap-fraud-detect-001"
    - request: "Define the fraud detection requirements"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge"

---

## B06-0C-021-002 — Fraud: "block" and "alert" are gaps; "specification" is not

```
source_id: xiigen-b06-0c-021-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-021-002-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Cases:
    - request: "Specify the fraud detection specification for chargebacks"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — specify fires DOCUMENT, specification fires DOCUMENT"
    - request: "Block high-risk checkout transactions automatically"
      predicates: []
      result: "KNOWLEDGE_EDGE — block is not in any predicate lexicon"
      capability_gap_id: "gap-fraud-block-001"
    - request: "Alert the fraud team when suspicious checkout activity occurs"
      predicates: []
      result: "KNOWLEDGE_EDGE — alert is not in any predicate lexicon"
      capability_gap_id: "gap-fraud-alert-001"

---

## B06-0C-021-003 — Fraud: "investigate" and "review" are gaps; "describe" is not

```
source_id: xiigen-b06-0c-021-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-021-003-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Cases:
    - request: "Describe the use cases for fraud challenge flows"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — describe fires DOCUMENT, use cases fires DOCUMENT"
    - request: "Investigate fraudulent transactions flagged during checkout"
      predicates: []
      result: "KNOWLEDGE_EDGE — investigate is not in any predicate lexicon"
      capability_gap_id: "gap-fraud-investigate-001"
    - request: "Review fraud reports generated from checkout data"
      predicates: []
      result: "KNOWLEDGE_EDGE — review is not in any predicate lexicon in this context"
      capability_gap_id: "gap-fraud-review-001"
