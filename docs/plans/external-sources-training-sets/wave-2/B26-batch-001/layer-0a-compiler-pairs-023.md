# Layer 0-A — Compiler Pairs (023)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: compiler

---

## C023-001 — SECURITY+QA: data encryption review

A: "Audit the DataEncryptionService — verify AES-256 (Advanced Encryption Standard 256-bit) key size, bcrypt (password hashing algorithm) iteration count, and HMAC (Hash-based Message Authentication Code) integrity — and QA all controls."
  "audit" → SECURITY; "AES-256" → SECURITY (encrypt/cipher); "bcrypt" → SECURITY; "HMAC" → SECURITY; "QA" → QA
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhase: 6.3/7.3

---

## C023-002 — SECURITY: hash algorithm strength review

A: "Review all password hashing configurations — confirm bcrypt cost factor ≥ 12, reject MD5 (Message-Digest Algorithm 5) and SHA-1 (Secure Hash Algorithm 1) for passwords."
  "review" (security context) → SECURITY; "password hashing" → SECURITY (hash trigger); "bcrypt" → SECURITY; "SHA-1" → SECURITY (hash trigger); "MD5" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C023-003 — SECURITY+DOCUMENT: encryption-at-rest audit documented

A: "Audit the database encryption-at-rest configuration and document all fields that store PII without encryption."
  "audit" → SECURITY; "encryption-at-rest" → SECURITY (encrypt trigger); "PII" → context; "without encryption" → SECURITY; "document" → DOCUMENT
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"] | SubPhase: 6.3/6.2

---

## C023-004 — SECURITY+IMPLEMENT: encryption implementation audit (IMPLEMENT present changes TransitionType)

A: "Audit the current encryption implementation for CBC (Cipher Block Chaining) mode vulnerabilities and implement AES-GCM (Galois/Counter Mode) as the replacement cipher."
  "audit" → SECURITY; "encryption implementation" → SECURITY (encrypt trigger); "CBC mode vulnerabilities" → SECURITY (cipher trigger); "implement" → IMPLEMENT; "AES-GCM" → SECURITY
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT"] | SubPhase: 6.3/4.1
  TransitionType: feature_addition (IMPLEMENT is present — per predicate rules, SECURITY+QA+IMPLEMENT → feature_addition)

---

## C023-005 — SECURITY+QA: TLS/encryption end-to-end audit + QA

A: "Audit the TLS configuration across all service-to-service channels — confirm TLS 1.3, reject TLS 1.0/1.1 — and QA the certificate rotation automation."
  "audit" → SECURITY; "TLS" → SECURITY (tls trigger); "TLS 1.3" → SECURITY; "TLS 1.0/1.1" → SECURITY; "certificate rotation" → SECURITY (certificate/signing trigger); "QA" → QA
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhase: 6.3/7.3
