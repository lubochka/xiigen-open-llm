# Layer 0-D — Flow Factory Pairs (050) — RedTeamExercise Flow Factory
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F050-001 — Red team findings audit → SecurityComplianceReviewFlow63

"Audit the red team exercise attack paths: credential harvesting, lateral movement, and authentication bypass."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (audit + three attack-vector triggers)

---

## F050-002 — "run red team exercise" → GapHandler

"Run a 2-week red team exercise targeting the authentication and payment services."
FlowFactory.Create(predicateIds: []) → GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)
Teaching: Exercise execution/scheduling is always gap.

---

## F050-003 — Red team + AUTHORIZATION → dual flows

"Audit red team findings: privilege escalation via JWT claim forgery and IDOR in billing endpoints."
FlowFactory.CreateMulti(["SECURITY", "AUTHORIZATION"]) → [SecurityComplianceReviewFlow63, AuthorizationAuditFlow63]
TrustState: deterministic_generic (privilege escalation + IDOR = dual triggers)

---

## F050-004 — Red team + DOCUMENT → documentation flow

"Review red team findings for persistence and exfiltration paths, then document the remediation roadmap."
FlowFactory.CreateMulti(["SECURITY", "DOCUMENT"]) → [SecurityComplianceReviewFlow63, DocumentationFlow62]
TransitionType: documentation

---

## F050-005 — Red team + IMPLEMENT → implementation flow

"Implement network segmentation fixes identified in the red team and audit lateral movement paths."
FlowFactory.CreateMulti(["IMPLEMENT", "SECURITY"]) → [FeatureImplementationFlow41, SecurityComplianceReviewFlow63]
TransitionType: feature_addition
