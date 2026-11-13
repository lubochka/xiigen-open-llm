# Layer 0-C — Knowledge-Edge Pairs (017)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## Method note

File 017 covers TaxCalculation knowledge-edge boundaries — three pairs.

---

## B06-0C-017-001 — Tax: "functional" fires DOCUMENT; "audit" does not

```
source_id: xiigen-b06-0c-017-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-017-001-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Cases:
    - request: "Specify the functional behavior for tax rate lookup"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — specify fires DOCUMENT, functional fires DOCUMENT, behavior fires DOCUMENT"
    - request: "Audit tax calculations during the checkout process"
      predicates: []
      result: "KNOWLEDGE_EDGE — audit is not in any predicate lexicon in this context"
      capability_gap_id: "gap-tax-audit-001"
    - request: "Define the tax requirements for EU VAT (European Union Value Added Tax)"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — define fires DOCUMENT, requirements fires DOCUMENT"

---

## B06-0C-017-002 — Tax: "compliance" alone vs "requirements" boundary

```
source_id: xiigen-b06-0c-017-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-017-002-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Cases:
    - request: "Tax compliance at checkout"
      predicates: []
      result: "KNOWLEDGE_EDGE — compliance alone is not a predicate term; no document-authoring verb present"
      capability_gap_id: "gap-tax-compliance-vague-001"
    - request: "Define the tax compliance requirements for checkout"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — define fires DOCUMENT, requirements fires DOCUMENT"
    - request: "Document the tax compliance narrative"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — document fires DOCUMENT, narrative fires DOCUMENT"

---

## B06-0C-017-003 — Tax: "estimate" vs "specify" boundary

```
source_id: xiigen-b06-0c-017-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-017-003-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Cases:
    - request: "Specify the tax calculation spec for checkout"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — specify fires DOCUMENT, spec fires DOCUMENT"
    - request: "Estimate the tax impact of adding new product categories"
      predicates: []
      result: "KNOWLEDGE_EDGE — estimate is not in any predicate lexicon"
      capability_gap_id: "gap-tax-estimate-001"
    - request: "Describe the tax use cases for digital goods"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge"
