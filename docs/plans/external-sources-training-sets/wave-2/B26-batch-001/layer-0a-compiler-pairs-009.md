# Layer 0-A — Compiler Pairs (009)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: compiler

---

## C009-001 — SECURITY via "mass assignment" vulnerability

A: "Audit the UserProfileService for mass assignment vulnerabilities — ensure only whitelisted fields are bindable."
  "audit" → SECURITY; "mass assignment vulnerabilities" → SECURITY; "whitelisted" → SECURITY (allowlist enforcement)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C009-002 — SECURITY + AUTHORIZATION via "broken access control"

A: "Review the ProductService for broken access control — verify customers cannot access other customers' order data."
  "review" (security context) → SECURITY; "broken access control" → AUTHORIZATION+SECURITY; "verify" → TEST; "customers cannot access" → AUTHORIZATION
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION", "TEST"] | SubPhase: 6.3

---

## C009-003 — SECURITY + QA via "QA security hardening"

A: "QA the security hardening checklist for the PaymentService before the hotfix deployment."
  "QA" → QA; "security hardening" → SECURITY; "checklist" → gap; "deployment" → gap
  CompiledPredicateIds: ["QA", "SECURITY"] | SubPhase: 6.3

---

## C009-004 — SECURITY + DOCUMENT via "threat model"

A: "Document the threat model for the new DataIngestionPipeline, including attack vectors and mitigations."
  "document" → DOCUMENT; "threat model" → SECURITY (threat modeling is a security-action concept); "attack vectors" → SECURITY; "mitigations" → gap
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"] | SubPhase: 6.3/6.2

---

## C009-005 — SECURITY + IMPLEMENT + TEST via "security hardening implementation"

A: "Implement the OWASP (Open Web Application Security Project) Top 10 mitigations for the OrderAPI and write security regression tests."
  "implement" → IMPLEMENT; "OWASP Top 10" → SECURITY; "mitigations" → SECURITY (security hardening); "write tests" → IMPLEMENT+TEST; "security regression tests" → TEST+SECURITY
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT", "TEST"] | SubPhase: 6.3/4.1/5.2
