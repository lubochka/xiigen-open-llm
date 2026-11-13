# Layer 0-D — Flow-Factory Pairs (007)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## F007-001 — SECURITY → 6.3 (SSRF audit)
Input: "Audit the WebhookService for SSRF vulnerabilities in the URL validation."
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

## F007-002 — SECURITY+QA+ARCHITECTURE → [6.3, 7.3, 3.5]
Input: "QA the cloud deployment security posture and audit IAM roles for least-privilege."
  CompiledPredicateIds: ["QA", "SECURITY", "ARCHITECTURE"] | SubPhases: [7.3, 6.3, 3.5]

## F007-003 — SECURITY+IMPLEMENT+TEST → [5.2, 4.1, 5.1]
Input: "Implement 2FA for the AdminService and write tests verifying MFA enforcement."
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT", "TEST"] | SubPhases: [5.2, 4.1, 5.1]

## F007-004 — SECURITY+AUTHORIZATION+QA+DOCUMENT → 4 flows
Input: "QA the RBAC model, audit privilege escalation, verify admin routes locked, document findings."
  CompiledPredicateIds: ["QA", "SECURITY", "AUTHORIZATION", "DOCUMENT", "TEST"] | SubPhases: [7.3, 6.3, 6.3, 6.2, 5.1]

## F007-005 — Gap → GapHandler
Input: "Promote a security-first culture in the engineering team."
  CompiledPredicateIds: []
  Factory: GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)
  Teaching: Cultural/training initiatives are gap regardless of "security" being present.
