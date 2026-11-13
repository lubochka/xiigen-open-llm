# Layer 0-B — Router Pairs (022)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: router

---

## R022-001 — SECURITY+QA → [6.3, 7.3]: RBAC audit + permission matrix QA
Request: "Audit the RBAC configuration for all AdminPanel endpoints and QA the permission matrix."
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhases: [6.3, 7.3]

## R022-002 — SECURITY+AUTHORIZATION → [6.3, 6.3]: privilege escalation review
Request: "Review the AdminService for privilege escalation paths — can a standard user elevate to admin via API parameter manipulation?"
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"] | SubPhases: [6.3, 6.3]
  Teaching: Both SECURITY and AUTHORIZATION route to 6.3 in an audit context. Two routing decisions, same destination.

## R022-003 — SECURITY+AUTHORIZATION+DOCUMENT → [6.3, 6.3, 6.2]: access control matrix documented
Request: "Audit the access control enforcement points across all microservices and document all unenforced authorization boundaries."
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION", "DOCUMENT"] | SubPhases: [6.3, 6.3, 6.2]

## R022-004 — Zero predicates → GapHandler: "user permissions" in product context
Request: "Allow users to manage their own team permissions from the settings page."
  CompiledPredicateIds: [] | GapHandler: SCHEDULE_CAPABILITY_LEARNING
  Gap: Product-feature permission requests without audit vocabulary produce zero predicates.

## R022-005 — SECURITY+AUTHORIZATION+QA → [6.3, 6.3, 7.3]: default-deny posture audit + coverage QA
Request: "Audit the AuthorizationMiddleware for default-deny posture and QA unauthenticated access paths."
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION", "QA"] | SubPhases: [6.3, 6.3, 7.3]
