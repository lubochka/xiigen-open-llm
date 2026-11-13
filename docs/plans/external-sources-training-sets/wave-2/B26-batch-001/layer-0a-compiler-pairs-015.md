# Layer 0-A — Compiler Pairs (015)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: compiler

---

## C015-001 — SECURITY+QA: PCI-DSS (Payment Card Industry Data Security Standard) audit

A: "Audit the payment processing module for PAN (Primary Account Number) encryption and QA the tokenization controls."
  "audit" → SECURITY; "PAN encryption" → SECURITY (encrypt trigger); "QA" → QA; "tokenization" → SECURITY (token trigger)
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhase: 6.3/7.3
  Note: "PCI-DSS" alone is gap. "PAN encryption" and "tokenization" provide SECURITY triggers.

---

## C015-002 — Gap: "PCI-DSS" as standalone acronym is gap

A: "Ensure PCI-DSS compliance is maintained for the card data environment."
  "ensure" → gap; "PCI-DSS" → gap; "compliance" → gap; "card data environment" → gap
  CompiledPredicateIds: [] → knowledge_edge
  Gap: PCI-DSS as a compliance framework acronym is gap vocabulary. SECURITY requires technical action terms.

---

## C015-003 — SECURITY: cardholder data encryption review

A: "Review the cardholder data storage for encryption at rest — confirm AES-256 (Advanced Encryption Standard) is applied to all sensitive fields."
  "review" (security context) → SECURITY; "encryption at rest" → SECURITY; "AES-256" → SECURITY (cipher/encrypt trigger)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C015-004 — SECURITY+DOCUMENT: CVV (Card Verification Value) retention audit documented

A: "Audit the CardDataService for CVV retention violations and document all non-compliant storage patterns."
  "audit" → SECURITY; "CVV retention violations" → SECURITY (credential/hash trigger); "document" → DOCUMENT
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"] | SubPhase: 6.3/6.2

---

## C015-005 — SECURITY+QA: TLS certificate audit + QA of card data transit

A: "Audit the TLS certificate configuration for card data transmission channels and QA the cipher suite selections."
  "audit" → SECURITY; "TLS certificate" → SECURITY (tls/certificate trigger); "QA" → QA; "cipher suite" → SECURITY (cipher trigger)
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhase: 6.3/7.3
