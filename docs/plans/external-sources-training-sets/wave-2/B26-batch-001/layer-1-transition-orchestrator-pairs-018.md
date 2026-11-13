# Layer 1 — Transition Orchestrator Pairs (018)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T018-001 — SECURITY+QA: feature_addition (penetration test review + critical remediations QA)
Request: "Review the penetration test findings for AccountService and QA all critical vulnerability remediations."
  TransitionType: feature_addition | SECURITY → 6.3 | QA → 7.3

## T018-002 — SECURITY: feature_addition (red team credential harvesting review)
Request: "Review the red team engagement report for lateral movement and credential harvesting findings."
  TransitionType: feature_addition | SECURITY → 6.3

## T018-003 — SECURITY+DOCUMENT: documentation (penetration test remediation + residual risk documented)
Request: "Audit penetration test remediation status for CRITICAL findings and document the residual risk."
  TransitionType: documentation | SECURITY → 6.3 | DOCUMENT → 6.2

## T018-004 — Zero predicates: N/A (pen test scheduling)
Request: "Schedule the pen test for Q3 and coordinate with the vendor."
  TransitionType: N/A | GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)

## T018-005 — SECURITY+QA: feature_addition (brute-force + hash + MFA)
Request: "Audit the LoginService brute-force protection — hash algorithm strength and MFA bypass — and QA controls."
  TransitionType: feature_addition | SECURITY → 6.3 | QA → 7.3
