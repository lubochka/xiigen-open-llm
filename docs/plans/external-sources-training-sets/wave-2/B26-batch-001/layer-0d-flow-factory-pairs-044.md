# Layer 0-D — Flow Factory Pairs (044) — InsecureDeserializationAudit Flow Factory
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F044-001 — Insecure deserialization audit → SecurityComplianceReviewFlow63

"Audit all deserialization endpoints for insecure deserialization vulnerabilities enabling RCE."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (audit + insecure deserialization + RCE)

---

## F044-002 — Gadget chain + privilege escalation → dual flows

"Scan the application for deserialization gadget chains enabling privilege escalation via crafted payloads."
FlowFactory.CreateMulti(["SECURITY", "AUTHORIZATION"]) → [SecurityComplianceReviewFlow63, AuthorizationAuditFlow63]
TrustState: deterministic_generic (privilege escalation = dual trigger)

---

## F044-003 — "migrate to System.Text.Json" → GapHandler

"Migrate all object serialization from Newtonsoft.Json to System.Text.Json."
FlowFactory.Create(predicateIds: []) → GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)
Teaching: Serializer migration without vulnerability/audit vocabulary is gap.

---

## F044-004 — TypeNameHandling review → SecurityComplianceReviewFlow63

"Review all JSON deserializer configurations for TypeNameHandling.All exploitation vectors."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (review + exploitation vectors)

---

## F044-005 — Deserialization + IMPLEMENT+SECURITY → dual flows

"Implement type allowlist deserialization and audit all existing endpoints for gadget chain exposure."
FlowFactory.CreateMulti(["IMPLEMENT", "SECURITY"]) → [FeatureImplementationFlow41, SecurityComplianceReviewFlow63]
TransitionType: feature_addition
