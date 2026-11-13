# Layer 0-A — Compiler Pairs (014)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: compiler

---

## C014-001 — SECURITY+QA+DOCUMENT: GDPR compliance review

A: "Audit the user data processing pipeline for GDPR-related encryption gaps, document the findings, and QA the data retention controls."
  "audit" → SECURITY; "encryption gaps" → SECURITY; "document" → DOCUMENT; "QA" → QA; "data retention controls" → borderline (no independent predicate)
  CompiledPredicateIds: ["SECURITY", "DOCUMENT", "QA"] | SubPhase: 6.3/6.2/7.3
  Note: "GDPR" alone is gap. "encryption gaps" provides the SECURITY trigger independently of the regulatory label.

---

## C014-002 — Gap: "GDPR" alone is gap vocabulary

A: "Ensure the platform is GDPR compliant by the deadline."
  "ensure" → gap; "GDPR" → gap (regulatory framework label); "compliant" → gap
  CompiledPredicateIds: [] → knowledge_edge
  Gap ID: gap-security-regulatory-001. Regulatory acronyms are gap. SECURITY fires on technical action vocabulary.

---

## C014-003 — SECURITY: data encryption audit for PII protection

A: "Audit the PII (Personally Identifiable Information) storage layer for encryption — are fields encrypted at rest and in transit using TLS?"
  "audit" → SECURITY; "encryption" → SECURITY; "PII" → no predicate but provides context; "TLS" → SECURITY (tls trigger)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C014-004 — SECURITY+DOCUMENT: data subject rights audit documented

A: "Review the data subject access request flow for security vulnerabilities and document the access control findings."
  "review" (security co-occurrence) → SECURITY; "security vulnerabilities" → SECURITY; "document" → DOCUMENT; "access control" → SECURITY
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"] | SubPhase: 6.3/6.2

---

## C014-005 — SECURITY+QA: consent mechanism and encryption QA

A: "Audit the consent management module for cipher strength and QA the key rotation policy."
  "audit" → SECURITY; "cipher strength" → SECURITY (cipher trigger); "QA" → QA; "key rotation" → SECURITY
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhase: 6.3/7.3
