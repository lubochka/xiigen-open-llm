# Layer 0-C — Knowledge-Edge Pairs (013)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## Method note

File 013 covers ShippingRequirements knowledge-edge boundaries — three pairs
distinguishing spec vocabulary from gap vocabulary in shipping domain.

---

## B06-0C-013-001 — Shipping: "spec" fires DOCUMENT; "plan" does not

```
source_id: xiigen-b06-0c-013-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-013-001-kedge"
  CompiledText: "Shipping knowledge edge: spec vs plan"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Cases:
    - request: "Write the shipping specification"
      predicates: ["DOCUMENT", "IMPLEMENT"]
      result: "NOT knowledge-edge — specification fires DOCUMENT, write fires IMPLEMENT"
    - request: "Plan the shipping carrier integration"
      predicates: []
      result: "KNOWLEDGE_EDGE — plan is not in any predicate lexicon"
      capability_gap_id: "gap-shipping-planning-001"
    - request: "Define the shipping requirements"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — define fires DOCUMENT"

---

## B06-0C-013-002 — Shipping: "roadmap" and "manage" are gaps; "define" is not

```
source_id: xiigen-b06-0c-013-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-013-002-kedge"
  CompiledText: "Shipping gap: roadmap, manage"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Cases:
    - request: "Build a roadmap for shipping carrier expansion"
      predicates: ["IMPLEMENT"]
      result: "NOT knowledge-edge — build fires IMPLEMENT. roadmap alone does not fire."
      note: "build fires IMPLEMENT; roadmap is not a predicate term"
    - request: "Manage the shipping carrier SLA (Service-Level Agreement) process"
      predicates: []
      result: "KNOWLEDGE_EDGE — manage is not in any predicate lexicon"
      capability_gap_id: "gap-shipping-manage-001"
    - request: "Describe the shipping SLA requirements"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — describe fires DOCUMENT, requirements fires DOCUMENT"

---

## B06-0C-013-003 — Shipping: domain nouns without predicates are gaps

```
source_id: xiigen-b06-0c-013-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-013-003-kedge"
  CompiledText: "Shipping gap: domain nouns without predicate vocabulary"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Cases:
    - request: "Our shipping process needs improvement"
      predicates: []
      result: "KNOWLEDGE_EDGE — no predicate terms; process and improvement are not predicates"
    - request: "Carrier integration issues at checkout"
      predicates: []
      result: "KNOWLEDGE_EDGE — no predicate terms; carrier, integration, issues are not predicates"
    - request: "Specify the shipping functional criteria"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — specify fires DOCUMENT, functional fires DOCUMENT, criteria fires DOCUMENT"
