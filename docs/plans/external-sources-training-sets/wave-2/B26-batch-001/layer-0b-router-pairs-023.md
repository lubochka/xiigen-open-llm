# Layer 0-B — Router Pairs (023)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: router

---

## R023-001 — SECURITY+QA → [6.3, 7.3]: AES/bcrypt/HMAC encryption audit + all controls QA
Request: "Audit the DataEncryptionService — verify AES-256 key size, bcrypt iteration count, and HMAC integrity — and QA all controls."
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhases: [6.3, 7.3]

## R023-002 — SECURITY → 6.3: hash algorithm strength review
Request: "Review all password hashing configurations — confirm bcrypt cost factor ≥ 12, reject MD5 and SHA-1 for passwords."
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

## R023-003 — SECURITY+DOCUMENT → [6.3, 6.2]: encryption-at-rest audit documented
Request: "Audit the database encryption-at-rest configuration and document all fields that store PII without encryption."
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"] | SubPhases: [6.3, 6.2]

## R023-004 — SECURITY+IMPLEMENT → [6.3, 4.1]: encryption audit + AES-GCM implementation
Request: "Audit the current encryption for CBC mode vulnerabilities and implement AES-GCM as the replacement cipher."
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT"] | SubPhases: [6.3, 4.1]
  TransitionType: feature_addition (IMPLEMENT present)

## R023-005 — SECURITY+QA → [6.3, 7.3]: TLS 1.3 enforcement audit + certificate rotation QA
Request: "Audit the TLS configuration — confirm TLS 1.3, reject TLS 1.0/1.1 — and QA certificate rotation automation."
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhases: [6.3, 7.3]
