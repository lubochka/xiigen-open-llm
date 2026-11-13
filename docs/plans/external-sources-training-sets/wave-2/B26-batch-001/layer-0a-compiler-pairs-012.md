# Layer 0-A — Compiler Pairs (012)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: compiler

---

## C012-001 — SECURITY via "broken authentication" audit

A: "Audit the SessionService for broken authentication — verify sessions expire after inactivity and are invalidated on logout."
  "audit" → SECURITY; "broken authentication" → SECURITY; "sessions expire" → SECURITY; "invalidated on logout" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C012-002 — SECURITY + QA + AUTHORIZATION via access control QA

A: "QA the access control matrix for the CustomerPortal — audit that super-admin routes are not accessible to regular users."
  "QA" → QA; "access control" → AUTHORIZATION; "audit" → SECURITY; "super-admin routes" → AUTHORIZATION; "not accessible" → AUTHORIZATION
  CompiledPredicateIds: ["QA", "SECURITY", "AUTHORIZATION"] | SubPhase: 6.3

---

## C012-003 — SECURITY + DOCUMENT + TEST via "security test report"

A: "Write security test cases for the PaymentGatewayClient, document all findings, and audit the TLS certificate validation."
  "write" → IMPLEMENT; "security test cases" → TEST+SECURITY; "document" → DOCUMENT; "audit" → SECURITY; "TLS certificate" → SECURITY
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT", "TEST", "DOCUMENT"] | SubPhase: 5.2/6.3

---

## C012-004 — SECURITY via "XML injection" (XXE — XML External Entity)

A: "Scan the DataImportService for XXE (XML External Entity) injection vulnerabilities in the XML parser."
  "scan" → SECURITY; "XXE injection vulnerabilities" → SECURITY; "XML parser" → gap (implementation detail)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C012-005 — SECURITY + IMPLEMENT via "implement security controls"

A: "Implement Content Security Policy (CSP) headers in the CustomerFrontend to prevent XSS attacks."
  "implement" → IMPLEMENT; "Content Security Policy" → SECURITY; "CSP headers" → SECURITY; "XSS attacks" → SECURITY
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT"] | SubPhase: 4.1
  Note: Implementation action → SECURITY routes to 5.2/4.1, not audit context 6.3
