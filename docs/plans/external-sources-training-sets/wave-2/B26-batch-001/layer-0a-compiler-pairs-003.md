# Layer 0-A — Compiler Pairs (003)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: compiler

---

## C003-001 — SECURITY via "penetration test"

A: "Run a penetration test against the OrderService API to identify authentication bypass vulnerabilities."
  "penetration test" → SECURITY (pentesting is a security-action term); "authentication bypass" → SECURITY; "API" → API_CONTRACT
  CompiledPredicateIds: ["SECURITY", "API_CONTRACT"] | SubPhase: 6.3

---

## C003-002 — SECURITY + AUTHORIZATION via "access control" audit

A: "Audit the role-based access control implementation in the AdminPanel and verify least-privilege principles are enforced."
  "audit" → SECURITY; "role-based access control" → AUTHORIZATION; "verify" → TEST; "least-privilege" → SECURITY; "enforced" → SECURITY
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION", "TEST"] | SubPhase: 6.3/5.2

---

## C003-003 — SECURITY + QA + DOCUMENT via full security QA audit

A: "QA the security configuration of the ProductCatalog service, audit for IDOR (Insecure Direct Object Reference) vulnerabilities, and document findings."
  "QA" → QA; "security configuration" → SECURITY; "audit" → SECURITY; "IDOR vulnerabilities" → SECURITY; "document" → DOCUMENT
  CompiledPredicateIds: ["QA", "SECURITY", "DOCUMENT"] | SubPhase: 6.3

---

## C003-004 — SECURITY via "CSRF" token validation

A: "Verify that all state-changing endpoints include CSRF (Cross-Site Request Forgery) token validation."
  "verify" → TEST; "CSRF" → SECURITY; "state-changing endpoints" → API_CONTRACT; "token validation" → SECURITY
  CompiledPredicateIds: ["TEST", "SECURITY", "API_CONTRACT"] | SubPhase: 5.2/6.3

---

## C003-005 — Gap: "secure coding" is gap; "security vulnerability" fires SECURITY

A: "Promote secure coding practices across the engineering team."
  "promote" → gap; "secure coding" → gap (practice/culture term, not security-action); "practices" → gap
  CompiledPredicateIds: [] | GapHandler: SCHEDULE_CAPABILITY_LEARNING, domainId: "software-development"
  Teaching: "secure coding" as a cultural/training concept is gap. SECURITY requires security-action vocabulary.
