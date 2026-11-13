# Layer 0-A — Compiler Pairs (027) — HIPAACompliance
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: compiler

---

## C027-001 — HIPAA (Health Insurance Portability and Accountability Act) label is gap

A: "Ensure the patient portal is HIPAA compliant before go-live."
  "HIPAA" → gap (regulatory label) | "compliant" → gap | "ensure" → gap
  CompiledPredicateIds: [] → knowledge_edge

---

## C027-002 — HIPAA + PHI encryption audit → SECURITY

A: "Audit the encryption of PHI (Protected Health Information) at rest and in transit."
  "audit" (verb) → SECURITY | "encryption" → SECURITY | "PHI" → domain label, not predicate trigger
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C027-003 — HIPAA + authentication audit

A: "Review the MFA configuration and session timeout settings for the HIPAA-covered patient data portal."
  "Review" + "MFA configuration" → SECURITY | "session timeout" → SECURITY (session group)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C027-004 — HIPAA + audit logging review

A: "Audit the access logs for unauthorized PHI access and verify the audit trail integrity."
  "audit" → SECURITY | "unauthorized PHI access" → SECURITY | "audit trail integrity" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C027-005 — HIPAA Business Associate Agreement boundary

A: "Prepare the Business Associate Agreement documentation for our cloud vendor under HIPAA."
  "Business Associate Agreement" → gap (legal/administrative) | "documentation" → DOCUMENT
  "HIPAA" → gap | "cloud vendor" → gap
  CompiledPredicateIds: ["DOCUMENT"] | SubPhase: 6.2
  Teaching: Administrative/legal documentation with regulatory labels → DOCUMENT, not SECURITY.
