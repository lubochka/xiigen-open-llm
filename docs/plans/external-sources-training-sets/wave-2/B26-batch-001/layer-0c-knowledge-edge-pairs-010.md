# Layer 0-C — Knowledge Edge Pairs (010)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## E010-001 — "OWASP" fires SECURITY

A: "Review the BillingService against the OWASP (Open Web Application Security Project) API Security Top 10."
  "review" (security context) → SECURITY; "OWASP" → SECURITY (security framework co-occurrence); "API Security" → SECURITY
  CompiledPredicateIds: ["SECURITY"]
  Teaching: "OWASP" fires SECURITY as a security framework reference. Combined with "review" in security context → SECURITY.

---

## E010-002 — "zero-trust" fires SECURITY

A: "Audit the network segmentation model for zero-trust compliance."
  "audit" → SECURITY; "zero-trust" → SECURITY (security architecture concept); "network segmentation" → ARCHITECTURE
  CompiledPredicateIds: ["SECURITY", "ARCHITECTURE"]
  Teaching: "zero-trust" fires SECURITY as a security architecture concept.

---

## E010-003 — "sensitive data" fires SECURITY in audit context

A: "Audit the UserService for sensitive data exposure in error messages and logs."
  "audit" → SECURITY; "sensitive data exposure" → SECURITY; "error messages" → domain gap
  CompiledPredicateIds: ["SECURITY"]
  Teaching: "sensitive data exposure" fires SECURITY as an OWASP vulnerability class.

---

## E010-004 — "code injection" fires SECURITY

A: "Test the ReportingEngine for code injection vulnerabilities in the template processor."
  "test" → TEST; "code injection vulnerabilities" → SECURITY
  CompiledPredicateIds: ["TEST", "SECURITY"]
  Teaching: "code injection" fires SECURITY. All injection types (SQL, code, LDAP, XML) fire SECURITY.

---

## E010-005 — "password policy" is borderline; fires SECURITY only with action verb

A: "Define a password policy requiring 12 characters, uppercase, and special characters."
  "define" → ARCHITECTURE; "password policy" → SECURITY (authentication policy = security domain)
  CompiledPredicateIds: ["SECURITY", "ARCHITECTURE"]
  vs "Check our current password policy." → [] (gap — no action verb fires SECURITY or ARCHITECTURE)
  Teaching: "password policy" fires SECURITY when defined/enforced/implemented. "check" alone is gap.
