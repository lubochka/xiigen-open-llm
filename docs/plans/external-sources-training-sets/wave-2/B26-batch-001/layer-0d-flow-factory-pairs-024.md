# Layer 0-D — Flow Factory Pairs (024) — Flow Factory Reference
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F024-001 — SecurityComplianceReviewFlow63 instantiation rules

SecurityComplianceReviewFlow63 is instantiated when:
  1. SECURITY fires AND sub-phase is 6.3 (audit/review context)
  2. AUTHORIZATION fires in audit context (also → 6.3, creates AuthorizationAuditFlow63)
  Node count: 9 | TrustState: deterministic_generic (dense signal) or probationary_accepted (weak signal)

Example:
  "Audit the JWT configuration for algorithm vulnerabilities."
  FlowFactory.Create("SECURITY", subPhase: "6.3") → SecurityComplianceReviewFlow63

---

## F024-002 — Multi-predicate flow instantiation rules

FlowFactory.CreateMulti(predicateIds) → one FlowInstance per predicate.
Predicates are NEVER collapsed. Each predicate instantiates its own flow independently.

Example with 3 predicates:
  ["SECURITY", "DOCUMENT", "QA"] → [SecurityComplianceReviewFlow63, DocumentationFlow62, QATestingFlow73]
  All three flows run; none suppresses another.

---

## F024-003 — TransitionType determination for 6.3 flows

SECURITY alone → TransitionType: feature_addition (new audit artifact)
SECURITY+DOCUMENT → TransitionType: documentation
SECURITY+QA → TransitionType: feature_addition
SECURITY+IMPLEMENT → TransitionType: feature_addition (IMPLEMENT is the decisive predicate)
Zero predicates → TransitionType: unknown (GapHandler path, no flow)

---

## F024-004 — probationary_accepted vs deterministic_generic threshold

deterministic_generic: 3+ SECURITY triggers in request OR audit verb + 2+ security nouns
probationary_accepted: 1 security noun + gap action verb (single signal, no audit verb)
knowledge_edge (no flow): all-gap vocabulary (GDPR, compliance, ensure, schedule)

Examples:
  "Audit authentication vulnerabilities in JWT" → deterministic_generic (3 triggers)
  "Look into the authentication flow" → probationary_accepted (1 trigger, gap verb)
  "Ensure GDPR compliance" → knowledge_edge (zero predicates)

---

## F024-005 — QA alone does NOT contribute TransitionType in 6.3 context

Per predicate rules: "QA alone → no contribution TransitionType"
QA contributes TransitionType only when paired with SECURITY or another active predicate.

Example:
  "QA the security findings." → QA → 7.3 | SECURITY → 6.3 → TransitionType: feature_addition (SECURITY contributes)
  "QA the release candidate." → QA → 7.3 only → TransitionType: feature_addition (from QA alone in 7.3 context)
  Rule: In sub-phase 6.3 context specifically, QA does not contribute TransitionType; SECURITY does.
