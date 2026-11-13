# Layer 0-A — Compiler Pairs (010)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: compiler

---

## C010-001 — SECURITY via "token replay" attack

A: "Audit the RefreshTokenService for token replay vulnerabilities — confirm each token is invalidated after use."
  "audit" → SECURITY; "token replay vulnerabilities" → SECURITY; "invalidated" → SECURITY (token management)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C010-002 — Gap: "SOC2" compliance is gap

A: "Prepare the engineering team for the upcoming SOC2 (System and Organization Controls 2) certification audit."
  "prepare" → gap; "SOC2 certification" → gap (compliance framework, not security-action); "audit" here is a noun event → gap
  CompiledPredicateIds: [] | GapHandler: SCHEDULE_CAPABILITY_LEARNING, domainId: "software-development"
  Teaching: Compliance framework names (SOC2, ISO 27001, GDPR) are gap. "audit" as noun event is gap.

---

## C010-003 — SECURITY + API_CONTRACT via "API key rotation audit"

A: "Audit all API key rotation policies on the ExternalIntegrationService — enforce 90-day rotation."
  "audit" → SECURITY; "API key" → SECURITY (credential management); "enforce" → SECURITY; "API key rotation" → API_CONTRACT (API governance)
  CompiledPredicateIds: ["SECURITY", "API_CONTRACT"] | SubPhase: 6.3

---

## C010-004 — SECURITY + QA + DOCUMENT via "security QA report"

A: "QA the security controls of the DataExportService, document all critical findings, and audit the data masking implementation."
  "QA" → QA; "security controls" → SECURITY; "document" → DOCUMENT; "audit" → SECURITY; "data masking" → SECURITY (data protection)
  CompiledPredicateIds: ["QA", "SECURITY", "DOCUMENT"] | SubPhase: 6.3/6.2

---

## C010-005 — SECURITY via "server-side request forgery" (SSRF)

A: "Audit the WebhookService for SSRF (Server-Side Request Forgery) vulnerabilities in the URL validation logic."
  "audit" → SECURITY; "SSRF" → SECURITY; "vulnerabilities" → SECURITY; "URL validation" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3
