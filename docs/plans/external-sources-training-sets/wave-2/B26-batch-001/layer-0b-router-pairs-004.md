# Layer 0-B — Router Pairs (004)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: router

---

## R004-001 — SECURITY → 6.3 (dependency scan)
Request: "Scan project dependencies for known CVE vulnerabilities."
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3
  Routing: SECURITY → SecurityAuditFlow63

## R004-002 — SECURITY + AUTHORIZATION → [6.3, 6.3]
Request: "Audit the RoleService for privilege escalation vulnerabilities — confirm default-deny for all unauthenticated routes."
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"] | SubPhases: [6.3, 6.3]
  Routing: Both SECURITY and AUTHORIZATION route to 6.3 in audit context (same flow tier, separate sub-flows)

## R004-003 — SECURITY + QA → [6.3, 7.3] (pre-release security gate)
Request: "QA the security hardening changes before the v2.1 release and audit all third-party integrations."
  CompiledPredicateIds: ["QA", "SECURITY"] | SubPhases: [7.3, 6.3]
  Routing: QA → QAGatingFlow73 | SECURITY → SecurityAuditFlow63

## R004-004 — SECURITY + TEST → [5.2, 5.1]
Request: "Assert that the FileUploadService rejects executable files and verify the MIME type whitelist."
  CompiledPredicateIds: ["TEST", "SECURITY"] | SubPhases: [5.1, 5.2]
  Routing: TEST → 5.1 | SECURITY in assert/verify context → 5.2

## R004-005 — Zero predicates → GapHandler
Request: "Make sure the codebase is secure before the customer demo."
  CompiledPredicateIds: []
  Routing: GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)
  Teaching: "make sure" is gap; "secure" as adjective is gap (SECURITY fires on security-action verbs/nouns, not adjective "secure"); "customer demo" is gap
