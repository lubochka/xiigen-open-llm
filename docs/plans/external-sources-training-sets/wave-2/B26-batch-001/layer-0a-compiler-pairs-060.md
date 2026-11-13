# Layer 0-A — Compiler Pairs (060) — Complete Factory Reference
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: compiler

---

## C060-001 — FlowFactory complete predicate-to-flow mapping reference

Predicate → Flow mapping (security domain):
  SECURITY (audit context) → SecurityComplianceReviewFlow63
  SECURITY (test-write context) → SecurityTestingFlow52
  AUTHORIZATION (audit context) → AuthorizationAuditFlow63
  SECURITY + AUTHORIZATION → both flows run concurrently at SubPhase 6.3
  SECURITY + IMPLEMENT → SecurityComplianceReviewFlow63 + FeatureImplementationFlow41
  SECURITY + DOCUMENT → SecurityComplianceReviewFlow63 + DocumentationFlow62
  SECURITY + QA → SecurityComplianceReviewFlow63 + QATestingFlow73

A: Reference-only entry. No A/B pair.

---

## C060-002 — Context disambiguation: audit vs implementation action

A: "Audit the JWT signing algorithm and implement RS256 (RSA Signature with SHA-256) rotation."
  "audit" → SECURITY → 6.3 | "implement" → IMPLEMENT → 4.1 | "JWT signing" → SECURITY | "RS256" → SECURITY (crypto)
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT"] | TransitionType: feature_addition (IMPLEMENT present)

---

## C060-003 — Multi-topic stacking: five security topics in one request

A: "Audit the authentication service for CSRF, SSRF, XXE, SQL injection, and privilege escalation vulnerabilities."
  "audit" → SECURITY | "CSRF" → SECURITY | "SSRF" → SECURITY | "XXE" → SECURITY | "SQL injection" → SECURITY | "privilege escalation" → SECURITY+AUTHORIZATION
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"] | SubPhase: 6.3 | TrustState: deterministic_generic

---

## C060-004 — "security review" as noun phrase (no technical action) → gap boundary

A: "Discuss the results of the security review in the next sprint planning meeting."
  "security review" as noun phrase in discussion context → gap (no technical audit action)
  CompiledPredicateIds: [] → knowledge_edge
  Teaching: "security review" as a noun referencing a past event is gap. The review verb must operate on a technical object.

---

## C060-005 — Six-predicate completeness check

A: "Redesign the authentication architecture, implement bcrypt hashing, audit RBAC for privilege escalation, document findings, and QA the security suite."
  "redesign architecture" → ARCHITECTURE | "implement bcrypt" → IMPLEMENT+SECURITY | "audit RBAC" → SECURITY+AUTHORIZATION | "privilege escalation" → SECURITY+AUTHORIZATION | "document" → DOCUMENT | "QA" → QA
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT", "SECURITY", "AUTHORIZATION", "DOCUMENT", "QA"]
  TrustState: deterministic_generic | TransitionType: feature_addition (IMPLEMENT present)
