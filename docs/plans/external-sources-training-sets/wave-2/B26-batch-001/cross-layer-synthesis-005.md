# Cross-Layer Synthesis (005)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: cross_layer_synthesis

---

## S005 — End-to-end trace: SECURITY+QA for API security audit

### Scenario
Request: "Audit all REST API endpoints for authentication bypass vulnerabilities and QA the authorization gate."

### Layer 0-A (Compiler)
Input text: "Audit all REST API endpoints for authentication bypass vulnerabilities and QA the authorization gate."
  "audit" → SECURITY | "authentication bypass" → SECURITY | "vulnerabilities" → SECURITY | "QA" → QA | "authorization" → AUTHORIZATION | "API endpoints" → API_CONTRACT
  CompiledPredicateIds: ["SECURITY", "API_CONTRACT", "QA", "AUTHORIZATION"]

### Layer 0-B (Router)
  SECURITY → SubPhase 6.3 (SecurityComplianceReviewFlow63)
  API_CONTRACT → SubPhase 3.3 (ApiContractDesignFlow33)
  QA → SubPhase 7.3 (QATestingFlow73)
  AUTHORIZATION → SubPhase 6.3 (AuthorizationAuditFlow63)
  PrimarySubPhaseId: 6.3

### Layer 0-D (Flow Factory)
  FlowFactory.CreateMulti(["SECURITY","API_CONTRACT","QA","AUTHORIZATION"])
  → [SecurityComplianceReviewFlow63, ApiContractDesignFlow33, QATestingFlow73, AuthorizationAuditFlow63]

### Layer 1 (Transition Orchestrator)
  TransitionType: feature_addition | Sequential: SECURITY audit → QA sign-off

### Layer 2 (SecurityComplianceReviewFlow63)
  Findings: Missing Bearer validation on /api/admin/users (CRITICAL) | Default-allow posture (CRITICAL) | 3 unauthenticated endpoints (HIGH) | Stale role claims (HIGH)
  Node 8 Arbiter: FAIL — 2 CRITICAL

### Layer 3 (Teaching)
  "audit" verb fires SECURITY to 6.3. "QA" abbreviation fires QA to 7.3. "authorization" fires AUTHORIZATION to 6.3.
  API_CONTRACT fires independently on "API endpoints" → 3.3.
  Four predicates → four independent routing decisions. None suppresses another.

### Cross-layer invariant
  SECURITY and QA are independent predicates routing to different sub-phases (6.3 and 7.3 respectively).
  The QA predicate fires ONLY on the "QA" abbreviation — not on "quality assurance" spelled out.
  In sub-phase 6.3 context, QA does not contribute TransitionType; SECURITY does.
