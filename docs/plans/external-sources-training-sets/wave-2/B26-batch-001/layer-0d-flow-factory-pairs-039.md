# Layer 0-D — Flow Factory Pairs (039) — InjectionAudit Flow Factory
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F039-001 — SQL injection audit → SecurityComplianceReviewFlow63

"Audit all SQL query construction methods for injection vulnerabilities using parameterized query verification."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (audit + SQL injection + vulnerabilities)

---

## F039-002 — Multi-injection audit → SecurityComplianceReviewFlow63

"Scan all input processing paths for SQL injection, LDAP injection, and OS command injection."
FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63
TrustState: deterministic_generic (three independent attack-vector triggers)

---

## F039-003 — "refactor queries to ORM" → GapHandler

"Refactor all raw SQL queries to use the Entity Framework ORM."
FlowFactory.Create(predicateIds: []) → GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)
Teaching: ORM migration without audit vocabulary is gap.

---

## F039-004 — Injection + IMPLEMENT → dual flows

"Implement parameterized queries for all database calls and audit remaining string-concatenated queries."
FlowFactory.CreateMulti(["IMPLEMENT", "SECURITY"]) → [FeatureImplementationFlow41, SecurityComplianceReviewFlow63]
TransitionType: feature_addition

---

## F039-005 — Injection write-tests → SecurityTestingFlow52

"Write security tests verifying all search endpoints block SQL injection payloads."
FlowFactory.Create("SECURITY", subPhase: "5.2") → SecurityTestingFlow52
Teaching: "write tests" → 5.2. "audit" → 6.3.
