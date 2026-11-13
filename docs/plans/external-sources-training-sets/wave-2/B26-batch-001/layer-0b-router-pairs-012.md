# Layer 0-B — Router Pairs (012)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: router

---

## R012-001 — SECURITY → 6.3 via "open redirect audit"
Request: "Audit the PasswordResetService for open redirect vulnerabilities in the redirect URL parameter."
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

## R012-002 — SECURITY + QA → [6.3, 7.3]
Request: "Audit the payment tokenization flow and QA the PCI-DSS (Payment Card Industry Data Security Standard) compliance controls."
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhases: [6.3, 7.3]
  Note: "PCI-DSS compliance" is gap vocabulary but "tokenization" + "payment" co-occurrence with "audit" fires SECURITY

## R012-003 — SECURITY + IMPLEMENT + TEST + DOCUMENT → 4 flows
Request: "Implement HSTS (HTTP Strict Transport Security) headers, write tests asserting HTTPS is enforced, audit the header policy, and document the security configuration."
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT", "TEST", "DOCUMENT"] | SubPhases: [5.2/6.3, 4.1, 5.1, 6.2]

## R012-004 — SECURITY + AUTHORIZATION + QA + DOCUMENT → 4 flows
Request: "QA the RBAC (Role-Based Access Control) implementation, audit privilege escalation paths, verify admin routes are locked down, and document the authorization matrix."
  CompiledPredicateIds: ["QA", "SECURITY", "AUTHORIZATION", "DOCUMENT"] | SubPhases: [7.3, 6.3, 6.3, 6.2]

## R012-005 — Gap: "security policy" document gap
Request: "Write the company information security policy document."
  CompiledPredicateIds: ["IMPLEMENT", "DOCUMENT"]
  Note: "write" → IMPLEMENT; "security policy document" → DOCUMENT; "security policy" without attack/audit vocabulary is gap for SECURITY predicate
  Teaching: SECURITY fires on vulnerability/attack/enforcement actions, not on policy documentation.
