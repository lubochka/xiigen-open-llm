# Layer 0-C — Knowledge Edge Pairs (006)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## E006-001 — "threat model" fires SECURITY even without "audit" verb

A: "Create a threat model for the new DataIngestionPipeline."
  "create" → IMPLEMENT; "threat model" → SECURITY (threat modeling is a security-action concept)
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT"]
  Teaching: "threat model" fires SECURITY. It is security-action vocabulary regardless of the action verb.

---

## E006-002 — "QA" fires regardless of sentence position

A: "Before deploying, QA the security configuration of the InvoiceService."
  "QA" → QA (sentence-internal abbreviation fires regardless of position)
  "security configuration" → SECURITY; "deploying" → gap
  CompiledPredicateIds: ["QA", "SECURITY"]
  Teaching: QA fires on the abbreviation "QA" regardless of where it appears in the sentence.

---

## E006-003 — "sanitize" fires SECURITY; "sanitization" fires SECURITY

A: "Sanitize all user inputs to the ProductSearchService before passing to the database layer."
  "sanitize" → SECURITY; "user inputs" → domain gap; "database layer" → domain gap
  CompiledPredicateIds: ["SECURITY"]
  Teaching: "sanitize" and "sanitization" fire SECURITY (input sanitization is a security-action).

---

## E006-004 — "CVE" abbreviation fires SECURITY

A: "Check the project for CVE (Common Vulnerabilities and Exposures) advisories and apply patches."
  "check" → gap; "CVE advisories" → SECURITY; "apply patches" → IMPLEMENT
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT"]
  Teaching: "CVE" abbreviation fires SECURITY. The full form "Common Vulnerabilities and Exposures" also fires SECURITY via "vulnerabilities".

---

## E006-005 — "CSRF token" fires SECURITY; missing CSRF token fires SECURITY

A: "Verify all POST endpoints include a CSRF token in the request validation."
  "verify" → TEST; "CSRF token" → SECURITY
  CompiledPredicateIds: ["TEST", "SECURITY"]
  Teaching: "CSRF" abbreviation fires SECURITY. Verification of CSRF token → TEST+SECURITY.
