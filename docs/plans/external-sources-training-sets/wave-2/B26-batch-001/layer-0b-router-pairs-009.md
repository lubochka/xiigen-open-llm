# Layer 0-B — Router Pairs (009)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: router

---

## R009-001 — SECURITY → 6.3 via mass assignment audit
Request: "Audit the UserProfileController for mass assignment vulnerabilities."
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

## R009-002 — SECURITY + AUTHORIZATION → [6.3, 6.3]
Request: "Review the permission model for the AdminService — confirm super-admin cannot be granted via API call."
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"] | SubPhases: [6.3, 6.3]
  Note: Both SECURITY and AUTHORIZATION route to 6.3 in audit context

## R009-003 — SECURITY + QA → [6.3, 7.3] (token validation QA)
Request: "QA the OAuth2 token validation flow and audit for implicit grant type vulnerabilities."
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhases: [6.3, 7.3]

## R009-004 — SECURITY + IMPLEMENT (CSP implementation) → [5.2, 4.1]
Request: "Implement Content Security Policy headers to block inline script execution."
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT"] | SubPhases: [5.2, 4.1]
  Routing: SECURITY in implementation context → 5.2 (not audit 6.3) | IMPLEMENT → 4.1

## R009-005 — Gap: "vulnerability disclosure" process is gap
Request: "Set up a responsible vulnerability disclosure process for external researchers."
  CompiledPredicateIds: []
  Routing: GapHandler | Teaching: "vulnerability disclosure process" is organizational/policy gap, not a code security-action
