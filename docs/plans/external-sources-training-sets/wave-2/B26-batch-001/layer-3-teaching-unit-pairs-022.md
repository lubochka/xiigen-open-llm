# Layer 3 — Teaching Unit Pairs (022)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU022-001 — RBAC fires SECURITY; "roles" in product-feature context is gap
A: "Audit the RBAC configuration for all AdminPanel endpoints." vs "Allow admins to assign roles to team members."
B: "audit" + "RBAC" → SECURITY. CompiledPredicateIds: ["SECURITY"]. SubPhase: 6.3.
   "allow admins to assign roles" → gap (product feature). CompiledPredicateIds: [].

## TU022-002 — IDOR fires BOTH SECURITY and AUTHORIZATION
A: "Review the OrderService for IDOR vulnerabilities — can user A access user B's orders?" What predicates fire?
B: Two: SECURITY and AUTHORIZATION. "IDOR" is an explicit dual-predicate trigger per predicate rules.
   "review" (security context) → SECURITY. CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"].
   Both route to 6.3 in audit context. Two routing decisions, same sub-phase destination.

## TU022-003 — "wildcard permission" fires both SECURITY and AUTHORIZATION
A: "Audit the AdminRole for wildcard (*) permissions." What predicates fire?
B: Two: SECURITY and AUTHORIZATION. "wildcard permissions" → SECURITY+AUTHORIZATION (access control audit).
   "audit" → SECURITY. "wildcard" in permission context → AUTHORIZATION.
   CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"].

## TU022-004 — "default-deny" fires SECURITY; product "default value" is gap
A: "Audit the AuthorizationMiddleware for default-deny posture." vs "Set the default pagination to 20 items."
B: "default-deny" → SECURITY (access control security pattern). CompiledPredicateIds: ["SECURITY"].
   "default pagination" → gap (UI feature). CompiledPredicateIds: [].

## TU022-005 — "unauthenticated" endpoint audit fires SECURITY; anonymous endpoint in passing is gap
A: "Audit all unauthenticated endpoints — verify public exposure is intentional." vs "The /health endpoint is intentionally anonymous."
B: "audit" + "unauthenticated endpoints" → SECURITY. CompiledPredicateIds: ["SECURITY"].
   "/health endpoint is... anonymous" → gap (passing observation, no audit vocabulary). CompiledPredicateIds: [].
