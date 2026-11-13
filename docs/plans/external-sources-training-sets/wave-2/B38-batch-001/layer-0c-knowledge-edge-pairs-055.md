# Layer 0-C — Knowledge-Edge Pairs (055)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-055-001 — "document" vs. "create": both fire DOCUMENT

"Document" (action verb) fires DOCUMENT.
"Create" in document-production context fires DOCUMENT.
"Generate" does not fire DOCUMENT.
"Produce" does not fire DOCUMENT.

| Sentence | PredicateSet |
|---|---|
| "Document the release criteria." | ["DOCUMENT"] |
| "Create the release criteria document." | ["DOCUMENT"] (create in document context) |
| "Generate the release criteria document." | [] → knowledge_edge |
| "Produce the release criteria document." | [] → knowledge_edge |

---

## B38-0C-055-002 — "policy" vs. "criteria": both trigger DOCUMENT with document verb

Both "policy" (policy document artifact) and "criteria" (rule 3 absolute) fire DOCUMENT when the document verb is present.

| Sentence | PredicateSet |
|---|---|
| "Document the attestation policy." | ["DOCUMENT"] |
| "Document the attestation criteria." | ["DOCUMENT"] |
| "Document the attestation policy and criteria." | ["DOCUMENT"] (single DOCUMENT predicate — consolidates) |
| "Review the attestation policy and criteria." | ["QA"] (review fires QA; policy/criteria without document verb = no DOCUMENT) |

---

## B38-0C-055-003 — "requirements" fires DOCUMENT in document context

"Requirements" as a document artifact fires DOCUMENT when "document" verb is present.

| Sentence | PredicateSet |
|---|---|
| "Document the compliance requirements." | ["DOCUMENT"] |
| "Document the regulatory requirements." | ["DOCUMENT"] |
| "Review the compliance requirements." | ["QA"] (no document verb; requirements without document verb = no DOCUMENT) |
| "Ensure the compliance requirements are met." | [] → knowledge_edge |
