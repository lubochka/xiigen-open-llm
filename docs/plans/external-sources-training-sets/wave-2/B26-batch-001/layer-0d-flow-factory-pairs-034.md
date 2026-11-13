# Layer 0-D — Flow Factory Pairs (034) — Complete SECURITY Reference Flow Factory
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F034-001 — FlowFactory complete mapping reference

FlowFactory SecurityDomain mappings:
  SECURITY (audit/review/scan) → SecurityComplianceReviewFlow63 at SubPhase 6.3
  SECURITY (write/implement/assert tests) → SecurityTestingFlow52 at SubPhase 5.2
  AUTHORIZATION → AuthorizationAuditFlow63 at SubPhase 6.3
  SECURITY + AUTHORIZATION → [SecurityComplianceReviewFlow63, AuthorizationAuditFlow63]
  No security predicates → GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)

A: Reference-only entry.

---

## F034-002 — SECURITY+AUTHORIZATION factory invocation

"Audit the RBAC permission model for privilege escalation and IDOR vulnerabilities."
FlowFactory.CreateMulti(["SECURITY", "AUTHORIZATION"]) → [SecurityComplianceReviewFlow63, AuthorizationAuditFlow63]
Both flows run concurrently in SubPhase 6.3.
TrustState: deterministic_generic

---

## F034-003 — "ensure compliance" → GapHandler

"Ensure the application is SOC2 and PCI-DSS compliant before the audit."
FlowFactory.Create(predicateIds: []) → GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)
Teaching: Compliance labels without audit verb are always gap.

---

## F034-004 — Six-predicate FlowFactory invocation

"Redesign auth, implement bcrypt, audit RBAC, document findings, QA security suite, update architecture."
FlowFactory.CreateMulti(["ARCHITECTURE","IMPLEMENT","SECURITY","AUTHORIZATION","DOCUMENT","QA"])
→ Six concurrent flows, one per predicate
TransitionType: feature_addition (IMPLEMENT present)

---

## F034-005 — Test-writing vs audit factory disambiguation

"Write security tests for XSS and then audit the CSP headers."
FlowFactory.CreateMulti(["SECURITY(5.2)", "SECURITY(6.3)"]) → [SecurityTestingFlow52, SecurityComplianceReviewFlow63]
Both flows from the same SECURITY predicate, different sub-phases based on action verbs.
