# Layer 1 — Transition Orchestrator Pairs (023)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T023-001 — SECURITY+QA: feature_addition (AES/bcrypt/HMAC audit + all controls QA)
Request: "Audit DataEncryptionService — AES-256 key size, bcrypt iteration count, HMAC integrity — and QA all controls."
  TransitionType: feature_addition | SECURITY → 6.3 | QA → 7.3

## T023-002 — SECURITY: feature_addition (password hashing: bcrypt/MD5/SHA-1)
Request: "Review all password hashing configurations — confirm bcrypt cost ≥ 12, reject MD5 and SHA-1."
  TransitionType: feature_addition | SECURITY → 6.3

## T023-003 — SECURITY+DOCUMENT: documentation (encryption-at-rest + PII fields documented)
Request: "Audit database encryption-at-rest and document all fields that store PII without encryption."
  TransitionType: documentation | SECURITY → 6.3 | DOCUMENT → 6.2

## T023-004 — SECURITY+IMPLEMENT: feature_addition (CBC vulnerability audit + AES-GCM implementation)
Request: "Audit encryption for CBC mode vulnerabilities and implement AES-GCM as the replacement cipher."
  TransitionType: feature_addition | SECURITY → 6.3 | IMPLEMENT → 4.1
  Teaching: IMPLEMENT changes TransitionType to feature_addition regardless of DOCUMENT co-occurrence.

## T023-005 — SECURITY+QA: feature_addition (TLS 1.3 enforcement + certificate rotation QA)
Request: "Audit TLS configuration — confirm TLS 1.3, reject 1.0/1.1 — and QA certificate rotation automation."
  TransitionType: feature_addition | SECURITY → 6.3 | QA → 7.3
