# Layer 0-C — Knowledge-Edge Pairs (018)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## Method note

File 018 covers MultiCurrency knowledge-edge boundaries — three pairs.

---

## B06-0C-018-001 — Currency: "spec" fires DOCUMENT; "convert" does not

```
source_id: xiigen-b06-0c-018-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-018-001-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Cases:
    - request: "Specify the multi-currency spec for the payment gateway"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — specify fires DOCUMENT, spec fires DOCUMENT"
    - request: "Convert currency prices during checkout display"
      predicates: []
      result: "KNOWLEDGE_EDGE — convert is not in any predicate lexicon"
      capability_gap_id: "gap-currency-convert-001"
    - request: "Define the currency requirements for checkout"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge"

---

## B06-0C-018-002 — Currency: "display" and "format" are gaps; "behavior" is not

```
source_id: xiigen-b06-0c-018-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-018-002-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Cases:
    - request: "Document the currency display behavior at checkout"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — document fires DOCUMENT, behavior fires DOCUMENT"
    - request: "Display currency conversion rates during checkout"
      predicates: []
      result: "KNOWLEDGE_EDGE — display alone is not a predicate term in this context"
      capability_gap_id: "gap-currency-display-001"
    - request: "Format currency output in the checkout total"
      predicates: []
      result: "KNOWLEDGE_EDGE — format is not in any predicate lexicon"
      capability_gap_id: "gap-currency-format-001"

---

## B06-0C-018-003 — Currency: "criteria" fires DOCUMENT; "round" does not

```
source_id: xiigen-b06-0c-018-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-018-003-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Cases:
    - request: "Describe the functional criteria for currency auto-detection"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — describe fires DOCUMENT, functional fires DOCUMENT, criteria fires DOCUMENT"
    - request: "Round currency values to two decimal places during checkout"
      predicates: []
      result: "KNOWLEDGE_EDGE — round is not in any predicate lexicon"
      capability_gap_id: "gap-currency-round-001"
    - request: "Specify the rounding criteria for currency calculations"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — specify fires DOCUMENT, criteria fires DOCUMENT"
