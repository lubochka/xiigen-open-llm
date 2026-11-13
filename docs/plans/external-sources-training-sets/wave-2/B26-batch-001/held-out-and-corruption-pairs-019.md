# Held-Out and Corruption Pairs (019)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## H019-001 — Held-out: "hardcoded secrets" fires SECURITY
Request: "Audit all repositories for hardcoded API keys, database passwords, and JWT signing secrets."
  Correct: ["SECURITY"]
  Evidence: "audit" → SECURITY; "API keys" → SECURITY (credential/secret); "JWT signing secrets" → SECURITY (jwt/signing/secret)

## H019-002 — Corruption v6-01: "configure secrets manager" fires SECURITY (invention)
Corrupt: "Configure the secrets manager to store environment variables." → ["SECURITY"]
Correct: []
  Fix: "configure" is a setup action — gap. No audit/vulnerability vocabulary. Zero predicates.

## H019-003 — Held-out: "signing key rotation" fires SECURITY
Request: "Review the signing key rotation policy — are keys rotated within 30 days and old keys revoked?"
  Correct: ["SECURITY"]
  Evidence: "review" (security context) → SECURITY; "signing key rotation" → SECURITY (signing + key rotation triggers)

## H019-004 — Corruption v6-03: DOCUMENT predicate dropped from secret audit
Corrupt: "Audit repositories for hardcoded secrets and document the remediation plan." → ["SECURITY"]
Correct: ["SECURITY", "DOCUMENT"]
  Fix: "document" fires DOCUMENT independently. Multi-predicate requests must not be collapsed.

## H019-005 — Held-out: "cipher algorithm" fires SECURITY
Request: "Audit the encryption key management service — verify the cipher algorithm selection is AES-256, not DES."
  Correct: ["SECURITY"]
  Evidence: "audit" → SECURITY; "cipher algorithm" → SECURITY (cipher trigger); "AES-256" → SECURITY; "DES" → SECURITY
