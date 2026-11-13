# Layer 0-D — Flow Factory Pairs (057) — CloudSecurityPosture Flow Factory
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F057-001 — Cloud IAM audit → dual flows

"Audit IAM role permissions and S3 bucket policies for public exposure vulnerabilities."
FlowFactory.CreateMulti(["SECURITY", "AUTHORIZATION"]) → [SecurityComplianceReviewFlow63, AuthorizationAuditFlow63]
TrustState: deterministic_generic (IAM + public exposure = dual triggers)

---

## F057-002 — Security group misconfiguration scan → SecurityComplianceReviewFlow63

"Scan the AWS environment for security group misconfigurations allowing unrestricted inbound access."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (scan + security group + misconfiguration)

---

## F057-003 — "review cloud costs" → GapHandler

"Review AWS spend and identify over-provisioned resources for cost reduction."
FlowFactory.Create(predicateIds: []) → GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)
Teaching: Cost review has no security vocabulary → gap.

---

## F057-004 — KMS key rotation + IAM → dual flows

"Audit AWS KMS key rotation schedules and IAM service account key age compliance."
FlowFactory.CreateMulti(["SECURITY", "AUTHORIZATION"]) → [SecurityComplianceReviewFlow63, AuthorizationAuditFlow63]
TrustState: deterministic_generic (key rotation + IAM = dual triggers)

---

## F057-005 — Cloud posture + DOCUMENT → documentation flow

"Audit exposed S3 buckets and vulnerable security groups, document findings in remediation register."
FlowFactory.CreateMulti(["SECURITY", "DOCUMENT"]) → [SecurityComplianceReviewFlow63, DocumentationFlow62]
TransitionType: documentation
