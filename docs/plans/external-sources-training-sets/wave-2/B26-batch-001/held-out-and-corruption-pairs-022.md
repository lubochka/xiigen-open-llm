# Held-Out and Corruption Pairs (022)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## H022-001 — Held-out: "RBAC wildcard" fires SECURITY+AUTHORIZATION
Request: "Audit the AdminRole — does it use a wildcard '*' permission granting access to all resources?"
  Correct: ["SECURITY", "AUTHORIZATION"]
  Evidence: "audit" → SECURITY; "wildcard '*' permission" → SECURITY+AUTHORIZATION; "all resources" → AUTHORIZATION context

## H022-002 — Corruption v6-01: product-feature "roles" fires AUTHORIZATION (invention)
Corrupt: "Allow admins to assign roles to team members from the settings page." → ["AUTHORIZATION"]
Correct: []
  Fix: Product-feature permission requests without audit vocabulary produce zero predicates. AUTHORIZATION requires audit/enforcement vocabulary.

## H022-003 — Held-out: "IDOR" fires SECURITY+AUTHORIZATION
Request: "Review the /admin/users/{id} endpoint for IDOR — can an admin from tenant A access tenant B's users?"
  Correct: ["SECURITY", "AUTHORIZATION"]
  Evidence: "review" (security context) → SECURITY; "IDOR" → SECURITY+AUTHORIZATION

## H022-004 — Corruption v6-03: AUTHORIZATION dropped from access control audit
Corrupt: "Audit access control enforcement across all microservices and document unenforced authorization boundaries." → ["SECURITY", "DOCUMENT"]
Correct: ["SECURITY", "AUTHORIZATION", "DOCUMENT"]
  Fix: "access control enforcement" + "authorization boundaries" fire AUTHORIZATION independently. Must not drop.

## H022-005 — Held-out: "default-deny" fires SECURITY
Request: "Audit the AuthorizationMiddleware for default-deny posture — are new routes public by default or protected?"
  Correct: ["SECURITY"]
  Evidence: "audit" → SECURITY; "default-deny" → SECURITY; "AuthorizationMiddleware" → AUTHORIZATION (fires AUTHORIZATION also)
  Full correct: ["SECURITY", "AUTHORIZATION"]
