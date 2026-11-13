# Layer 0-C — Knowledge Edge Pairs (008)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## E008-001 — "broken access control" fires AUTHORIZATION+SECURITY

A: "Audit the OrderService for broken access control — customers should not see other customers' orders."
  "audit" → SECURITY; "broken access control" → AUTHORIZATION+SECURITY
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"]
  Teaching: "broken access control" fires both SECURITY (it's a vulnerability class) and AUTHORIZATION (it's an access control failure).

---

## E008-002 — "privilege escalation" fires AUTHORIZATION+SECURITY

A: "Scan the AdminService for privilege escalation paths — verify no regular user can gain super-admin rights."
  "scan" → SECURITY; "privilege escalation" → AUTHORIZATION+SECURITY; "verify" → TEST; "super-admin rights" → AUTHORIZATION
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION", "TEST"]
  Teaching: "privilege escalation" fires both SECURITY and AUTHORIZATION as it is simultaneously a security vulnerability and an authorization failure.

---

## E008-003 — "security posture" fires SECURITY

A: "Review the security posture of the microservices platform before the public launch."
  "review" (security co-occurrence) → SECURITY; "security posture" → SECURITY; "public launch" → gap
  CompiledPredicateIds: ["SECURITY"]
  Teaching: "security posture" fires SECURITY via "security" co-occurrence. "posture" alone is gap.

---

## E008-004 — "IDOR" abbreviation fires SECURITY+AUTHORIZATION

A: "Audit the ProductService for IDOR (Insecure Direct Object Reference) vulnerabilities."
  "audit" → SECURITY; "IDOR" → SECURITY+AUTHORIZATION; "vulnerabilities" → SECURITY
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"]
  Teaching: IDOR fires both SECURITY (it's a vulnerability class) and AUTHORIZATION (it's a broken authorization pattern).

---

## E008-005 — "session management" fires SECURITY in audit context

A: "Audit the UserService session management — verify session tokens are invalidated on logout."
  "audit" → SECURITY; "session management" → SECURITY (session security); "verify" → TEST; "tokens invalidated" → SECURITY
  CompiledPredicateIds: ["SECURITY", "TEST"]
  Teaching: "session management" fires SECURITY when audited/reviewed. "verify" fires TEST independently.
