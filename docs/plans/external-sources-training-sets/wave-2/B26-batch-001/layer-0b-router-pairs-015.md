# Layer 0-B — Router Pairs (015)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: router

---

## R015-001 — SECURITY+QA → [6.3, 7.3]: PCI-DSS tokenization audit
Request: "Audit the payment processing module for PAN encryption and QA the tokenization controls."
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhases: [6.3, 7.3]
  Note: "PCI-DSS" alone is gap; "PAN encryption" and "tokenization" fire SECURITY.

## R015-002 — Zero predicates → GapHandler: "PCI-DSS standalone"
Request: "Ensure PCI-DSS compliance is maintained for the card data environment."
  CompiledPredicateIds: [] | GapHandler: SCHEDULE_CAPABILITY_LEARNING
  Gap: PCI-DSS as compliance acronym is gap. No technical security-action vocabulary.

## R015-003 — SECURITY → 6.3: AES-256 cardholder data audit
Request: "Review cardholder data storage for encryption at rest — confirm AES-256 applied to all sensitive fields."
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

## R015-004 — SECURITY+DOCUMENT → [6.3, 6.2]: CVV retention audit documented
Request: "Audit the CardDataService for CVV retention violations and document all non-compliant storage patterns."
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"] | SubPhases: [6.3, 6.2]

## R015-005 — SECURITY+QA → [6.3, 7.3]: TLS certificate + cipher suite audit
Request: "Audit the TLS certificate configuration for card data channels and QA the cipher suite selections."
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhases: [6.3, 7.3]
  Routing: SECURITY → SecurityComplianceReviewFlow63 | QA → QATestingFlow73
