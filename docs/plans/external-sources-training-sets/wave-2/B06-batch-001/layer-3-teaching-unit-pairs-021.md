# Layer 3 — Teaching Unit Pairs (021)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## Method note

File 021 covers DomainId, chain integrity, and partial-predicate teaching — three pairs.

---

## B06-L3-021-001 — Teaching: DomainId is always software-development (Rule 17)

```
source_id: xiigen-b06-l3-021-001
pair_kind: teaching_unit
```

B:
  TeachingUnit: "B06-DOMAIN-ID-TEACH-013-024"
  Content:
    rule: "Rule 17: DomainId is always software-development"
    new_domain_examples:
      "shipping requirements" → DomainId: software-development (NOT logistics)
      "loyalty spec" → DomainId: software-development (NOT crm)
      "fraud detection requirements" → DomainId: software-development (NOT security)
      "privacy requirements" → DomainId: software-development (NOT legal)
      "tax requirements" → DomainId: software-development (NOT finance)
    anti_patterns_to_avoid:
      - DomainId: logistics
      - DomainId: finance
      - DomainId: security
      - DomainId: compliance
      - DomainId: crm

---

## B06-L3-021-002 — Teaching: chain integrity — SourceDocumentId propagated through all nodes

```
source_id: xiigen-b06-l3-021-002
pair_kind: teaching_unit
```

B:
  TeachingUnit: "B06-CHAIN-INTEGRITY-TEACH"
  Content:
    rule: "SourceDocumentId propagates unchanged through all 9 nodes"
    chain_trace:
      Layer_0A: "DocumentId generated: B06-chain-teach-001"
      Layer_0B: "DocumentId: B06-chain-teach-001 (passed through)"
      Layer_0D: "SourceDocumentId: B06-chain-teach-001 (passed through)"
      Layer_1: "SourceDocumentId: B06-chain-teach-001 (passed through)"
      Layer_2_N01_to_N09: "TransitionId references B06-chain-teach-001"
    v6_corruption: "v6-03: DocumentId dropped at any layer → chain broken"
    fix: "DocumentId is never generated fresh inside a layer. It flows from 0-A to N09 unchanged."

---

## B06-L3-021-003 — Teaching: partial predicate case — one term fires even in gap context

```
source_id: xiigen-b06-l3-021-003
pair_kind: teaching_unit
```

B:
  TeachingUnit: "B06-PARTIAL-PREDICATE-TEACH"
  Content:
    rule: "If ANY predicate term appears in the request, that predicate fires"
    partial_predicate_cases:
      - request: "Plan the sprint for requirements implementation"
        analysis: "plan does not fire; requirements fires DOCUMENT"
        result: ["DOCUMENT"]
        is_knowledge_edge: false
      - request: "Manage privacy compliance requirements"
        analysis: "manage does not fire; requirements fires DOCUMENT"
        result: ["DOCUMENT"]
        is_knowledge_edge: false
      - request: "Monitor inventory levels"
        analysis: "monitor does not fire; no predicate terms in request"
        result: []
        is_knowledge_edge: true
    teaching_note: "Knowledge-edge only when ALL words in the request lack predicate terms.
      Even one predicate term produces a non-empty predicate set."
