# Layer 0-B — Router Pairs (018)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: router

---

## R018-001 — SECURITY+QA → [6.3, 7.3]: penetration test findings review + critical remediations QA
Request: "Review the penetration test findings for the AccountService and QA all critical vulnerability remediations."
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhases: [6.3, 7.3]

## R018-002 — SECURITY → 6.3: red team report review
Request: "Review the red team engagement report for lateral movement and credential harvesting findings."
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

## R018-003 — SECURITY+DOCUMENT → [6.3, 6.2]: penetration test remediation documented
Request: "Audit the penetration test remediation status for all CRITICAL findings and document the residual risk."
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"] | SubPhases: [6.3, 6.2]

## R018-004 — Zero predicates → GapHandler: "pen test scheduling"
Request: "Schedule the pen test for Q3 and coordinate with the vendor."
  CompiledPredicateIds: [] | GapHandler: SCHEDULE_CAPABILITY_LEARNING
  Gap: "schedule [event]" is logistics, not a security audit action.

## R018-005 — SECURITY+QA → [6.3, 7.3]: brute-force + hash + MFA audit + QA
Request: "Audit the LoginService brute-force protection — hash algorithm strength and MFA bypass paths — and QA the controls."
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhases: [6.3, 7.3]
  Note: "brute-force", "hash", "MFA" are all explicit SECURITY triggers.
