# Layer 0-B — Router Pairs (014)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: router

---

## R014-001 — SECURITY+DOCUMENT+QA → [6.3, 6.2, 7.3]: GDPR encryption audit
Request: "Audit the user data processing pipeline for GDPR-related encryption gaps, document findings, and QA data retention controls."
  CompiledPredicateIds: ["SECURITY", "DOCUMENT", "QA"] | SubPhases: [6.3, 6.2, 7.3]
  Routing: SECURITY → 6.3 | DOCUMENT → 6.2 | QA → 7.3

## R014-002 — Zero predicates → GapHandler: "GDPR alone"
Request: "Ensure the platform is GDPR compliant by the deadline."
  CompiledPredicateIds: [] | GapHandler: SCHEDULE_CAPABILITY_LEARNING
  Gap: "GDPR", "compliant", "ensure" are all gap vocabulary.

## R014-003 — SECURITY → 6.3: PII encryption audit
Request: "Audit the PII storage layer for encryption at rest and TLS in transit."
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3
  Routing: SECURITY + audit + encryption + TLS → SecurityComplianceReviewFlow63

## R014-004 — SECURITY+DOCUMENT → [6.3, 6.2]: data subject rights audit documented
Request: "Review the data subject access request flow for security vulnerabilities and document the findings."
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"] | SubPhases: [6.3, 6.2]

## R014-005 — SECURITY+QA → [6.3, 7.3]: cipher strength + key rotation QA
Request: "Audit the consent management module for cipher strength and QA the key rotation policy."
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhases: [6.3, 7.3]
