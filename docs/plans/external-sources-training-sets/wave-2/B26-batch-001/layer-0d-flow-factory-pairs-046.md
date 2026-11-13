# Layer 0-D — Flow Factory Pairs (046) — BrokenAuthAudit Flow Factory
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F046-001 — Broken auth audit → SecurityComplianceReviewFlow63

"Audit the authentication flow for broken authentication patterns including credential stuffing and MFA bypass."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (audit + broken auth + credential stuffing + MFA)

---

## F046-002 — Session entropy audit → SecurityComplianceReviewFlow63

"Audit session token entropy and expiry to prevent brute-force session hijacking."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (audit + session token + brute-force)

---

## F046-003 — "build login page" → GapHandler

"Build the login page for the customer portal with username, password, and remember-me fields."
FlowFactory.Create(predicateIds: []) → GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)
Teaching: Building a login feature without authentication audit vocabulary is gap.

---

## F046-004 — Broken auth + AUTHORIZATION → dual flows

"Audit broken auth paths that allow bypassing RBAC authorization gates via session tampering."
FlowFactory.CreateMulti(["SECURITY", "AUTHORIZATION"]) → [SecurityComplianceReviewFlow63, AuthorizationAuditFlow63]
TrustState: deterministic_generic

---

## F046-005 — Auth write-tests → SecurityTestingFlow52

"Write security tests asserting the AuthService blocks credential stuffing and MFA bypass attempts."
FlowFactory.Create("SECURITY", subPhase: "5.2") → SecurityTestingFlow52
Teaching: "write security tests asserting" → 5.2.
