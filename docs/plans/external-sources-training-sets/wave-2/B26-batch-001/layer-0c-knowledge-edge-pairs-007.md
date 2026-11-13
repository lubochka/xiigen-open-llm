# Layer 0-C — Knowledge Edge Pairs (007)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## E007-001 — "API key" fires SECURITY when in credential-management context

A: "Rotate the API keys for all third-party integrations in the PaymentService."
  "rotate" → gap for IMPLEMENT; "API keys" → SECURITY (credential management); API_CONTRACT context also present
  CompiledPredicateIds: ["SECURITY", "API_CONTRACT"]
  Teaching: "API key" in credential/rotation context fires SECURITY. "API" in endpoint context fires API_CONTRACT.

---

## E007-002 — "replay attack" fires SECURITY

A: "Confirm the AuthTokenService prevents replay attacks by validating token nonces."
  "confirm" → gap; "replay attacks" → SECURITY; "token nonces" → SECURITY (nonce = cryptographic protection)
  CompiledPredicateIds: ["SECURITY"]
  Teaching: "replay attack" fires SECURITY as an attack vector noun. "confirm" is gap (TEST requires "verify"/"assert"/"validate").

---

## E007-003 — "data masking" fires SECURITY

A: "Audit the DataExportService to ensure PII fields are masked in all export formats."
  "audit" → SECURITY; "PII fields masked" → SECURITY (data masking = data protection action); "export formats" → domain gap
  CompiledPredicateIds: ["SECURITY"]
  Teaching: "data masking" fires SECURITY. It is a data protection/security-action term.

---

## E007-004 — "XSS" abbreviation fires SECURITY even without "vulnerability" noun

A: "Implement output encoding in all templates to prevent XSS."
  "implement" → IMPLEMENT; "output encoding" → SECURITY; "prevent XSS" → SECURITY
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT"]
  Teaching: "XSS" fires SECURITY as a standalone abbreviation. The full form "Cross-Site Scripting" also fires SECURITY.

---

## E007-005 — "code review" without security co-occurrence is gap

A: "Schedule a code review for the UserManagement module before the sprint ends."
  "schedule" → gap; "code review" → gap (no security vocabulary co-occurrence); "sprint" → gap
  CompiledPredicateIds: [] → knowledge_edge
  Teaching: "code review" is gap unless paired with security vocabulary ("review for vulnerabilities", "security review", "security audit"). "code review" alone → gap.
