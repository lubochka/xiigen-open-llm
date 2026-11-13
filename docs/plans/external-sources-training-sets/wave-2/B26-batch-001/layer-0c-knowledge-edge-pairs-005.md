# Layer 0-C — Knowledge Edge Pairs (005)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## E005-001 — "enforce" always fires SECURITY (policy enforcement is a security action)

A: "Enforce input validation on all user-facing fields in the CheckoutForm."
  "enforce" → SECURITY; "input validation" → SECURITY; "user-facing fields" → domain gap
  CompiledPredicateIds: ["SECURITY"]
  Teaching: "enforce" fires SECURITY regardless of context. Enforcement of rules/policies is a security-action verb.

---

## E005-002 — "whitelist" / "allowlist" fires SECURITY

A: "Implement an allowlist for permitted file extensions in the DocumentUploadService."
  "implement" → IMPLEMENT; "allowlist" → SECURITY (access control enforcement); "file extensions" → domain gap
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT"]
  Teaching: "whitelist"/"allowlist"/"blocklist"/"denylist" fire SECURITY as access control vocabulary.

---

## E005-003 — "scan" fires SECURITY in vulnerability context; "scan" is gap in non-security context

A: "Scan the codebase for SQL injection vulnerabilities." → ["SECURITY"]
  "scan" → SECURITY via "vulnerabilities" co-occurrence
B: "Scan the OrderService logs for performance bottlenecks." → []
  "scan" (monitoring action) + "performance bottlenecks" → gap; no SECURITY co-occurrence
  Teaching: "scan" fires SECURITY only when paired with security/vulnerability vocabulary.

---

## E005-004 — "authentication" fires SECURITY; "authorization" fires AUTHORIZATION

A: "Audit the LoginService authentication — verify tokens are bound to the authenticated user."
  "audit" → SECURITY; "authentication" → SECURITY; "verify" → TEST; "tokens bound to user" → AUTHORIZATION
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION", "TEST"]
  Teaching: "authentication" fires SECURITY; "authorization" fires AUTHORIZATION. They are separate predicates.

---

## E005-005 — "default-allow" is gap alone; "default-allow vulnerability" fires SECURITY

A: "The AuthorizationService uses default-allow which is a security vulnerability."
  "default-allow" → gap alone; "security vulnerability" → SECURITY
  CompiledPredicateIds: ["SECURITY"]
  Teaching: "default-allow" as a configuration description is gap. "security vulnerability" fires SECURITY as the compound attack-vector noun.
