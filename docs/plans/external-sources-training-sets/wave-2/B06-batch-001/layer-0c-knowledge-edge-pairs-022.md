# Layer 0-C — Knowledge-Edge Pairs (022)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## Method note

File 022 covers DataPrivacy knowledge-edge boundaries — three pairs.

---

## B06-0C-022-001 — Privacy: "stakeholder" fires DOCUMENT+2.4; "comply" does not fire

```
source_id: xiigen-b06-0c-022-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-022-001-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Cases:
    - request: "Specify the stakeholder requirements for data privacy in checkout"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — specify fires DOCUMENT, stakeholder fires DOCUMENT; routes to 2.1+2.4"
    - request: "Comply with GDPR (General Data Protection Regulation) during checkout data collection"
      predicates: []
      result: "KNOWLEDGE_EDGE — comply is not in any predicate lexicon; GDPR alone is not a predicate term"
      capability_gap_id: "gap-privacy-comply-gdpr-001"
    - request: "Define the GDPR compliance requirements for checkout"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — define fires DOCUMENT, requirements fires DOCUMENT"

---

## B06-0C-022-002 — Privacy: "delete" and "anonymize" are gaps; "document" is not

```
source_id: xiigen-b06-0c-022-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-022-002-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Cases:
    - request: "Document the data retention behavior for checkout events"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — document fires DOCUMENT, behavior fires DOCUMENT"
    - request: "Delete customer PII (Personally Identifiable Information) records after checkout"
      predicates: []
      result: "KNOWLEDGE_EDGE — delete is not in any predicate lexicon"
      capability_gap_id: "gap-privacy-delete-pii-001"
    - request: "Anonymize checkout logs for GDPR compliance"
      predicates: []
      result: "KNOWLEDGE_EDGE — anonymize is not in any predicate lexicon"
      capability_gap_id: "gap-privacy-anonymize-001"

---

## B06-0C-022-003 — Privacy: "plan the sprint" vs "describe use cases" boundary

```
source_id: xiigen-b06-0c-022-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-022-003-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Cases:
    - request: "Describe the use cases for privacy consent collection at checkout"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — describe fires DOCUMENT, use cases fires DOCUMENT"
    - request: "Plan the sprint for privacy consent requirements implementation"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — requirements fires DOCUMENT; plan does not fire but requirements does"
      note: "Partial-predicate case: 'requirements' fires DOCUMENT even when 'plan' is also in the sentence"
    - request: "Manage the privacy compliance review for the checkout release"
      predicates: []
      result: "KNOWLEDGE_EDGE — manage is not a predicate term"
      capability_gap_id: "gap-privacy-manage-review-001"
