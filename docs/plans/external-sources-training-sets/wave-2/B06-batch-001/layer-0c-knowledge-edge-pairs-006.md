# Layer 0-C — Knowledge-Edge Pairs (006)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## Method note

File 006 teaches: "business" is in DOCUMENT lexicon. "business requirements" fires DOCUMENT.

---

## B06-0C-006-001 — "business" fires DOCUMENT: Rule 7

```
source_id: xiigen-b06-0c-006-001
source_title: XIIGen B06 knowledge-edge — business fires DOCUMENT
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-006-001-kedge"
  CompiledText: "Document the business requirements for the checkout flow."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  OverallVerdict: "FLOW_EXECUTING"
  Metrics:
    matched_predicate_count: "1"
    evidence: ["document", "business", "requirements"]
    business_term_teaching:
      term: "business"
      fires_predicate: "DOCUMENT"
      b06_rule: "Rule 7 — business fires DOCUMENT"
      rationale: "'business' signals business-domain specification work.
        'business requirements', 'business rules', 'business logic' all
        contain 'business' which fires DOCUMENT. The term indicates that
        the user is asking for requirements or rules from a business perspective,
        not implementation code."
      note: "business logic also has IMPLEMENT proximity but 'business' itself
        is a DOCUMENT term. 'business logic' as a compound: 'logic' is in
        IMPLEMENT lexicon so 'business logic implementation' → IMPLEMENT fires."

---

## B06-0C-006-002 — "business" context examples: when DOCUMENT fires vs when IMPLEMENT fires

```
source_id: xiigen-b06-0c-006-002
source_title: XIIGen B06 knowledge-edge — business term context analysis
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-006-002-kedge"
  CompiledText: "business term context analysis"
  CompiledPredicateIds: ["META"]
  SourceStatus: "knowledge_edge: meta-teaching pair"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Metrics:
    business_term_context_table:
      - request: "document the business requirements"
        predicates: ["DOCUMENT"]
        reason: "document + business both fire DOCUMENT"
      - request: "define the business rules"
        predicates: ["DOCUMENT"]
        reason: "define + business fire DOCUMENT"
      - request: "implement the business logic"
        predicates: ["IMPLEMENT"]
        reason: "implement fires IMPLEMENT; logic fires IMPLEMENT; business alone yields DOCUMENT but is overridden by IMPLEMENT verbs"
      - request: "write the business requirements"
        predicates: ["DOCUMENT", "IMPLEMENT"]
        reason: "write fires IMPLEMENT; business + requirements fire DOCUMENT"
      - request: "describe the business behavior"
        predicates: ["DOCUMENT"]
        reason: "describe + business + behavior all fire DOCUMENT"
      - request: "build the business module"
        predicates: ["IMPLEMENT"]
        reason: "build fires IMPLEMENT; module is not in any lexicon"

---

## B06-0C-006-003 — "stakeholder" fires DOCUMENT even in status contexts: Rule 8

```
source_id: xiigen-b06-0c-006-003
source_title: XIIGen B06 knowledge-edge — stakeholder fires DOCUMENT Rule 8
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-006-003-kedge"
  CompiledText: "Document the stakeholder expectations and requirements."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"

B:
  OverallVerdict: "FLOW_EXECUTING"
  Metrics:
    evidence: ["document", "stakeholder", "requirements"]
    stakeholder_term_teaching:
      term: "stakeholder"
      fires_predicate: "DOCUMENT"
      b06_rule: "Rule 8 — stakeholder fires DOCUMENT"
      rationale: "'stakeholder' signals that the user is asking about requirements
        from external parties or stakeholders. This is a DOCUMENT-domain term —
        it indicates requirements authoring or documentation work."
      routing_consequence: "DOCUMENT predicate fires; router maps 'stakeholder'
        to sub-phase 2.4 signal alongside 2.1 if requirements signals are also present."
      edge_case: "'stakeholder update' in a status context ('the stakeholder needs
        a status update') also fires DOCUMENT on 'stakeholder' — but this is a
        weaker 2.4 signal. The router may score it lower for 2.4."
