# Layer 0-A — Compiler Pairs (019)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: compiler

---

## C019-001 — SECURITY+QA: secret rotation audit

A: "Audit the SecretStore for API keys past their rotation deadline and QA the automated rotation pipeline."
  "audit" → SECURITY; "SecretStore" → SECURITY (secret trigger); "API keys" → SECURITY (credential/secret context); "QA" → QA; "rotation" → SECURITY (secret rotation trigger)
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhase: 6.3/7.3

---

## C019-002 — SECURITY: signing key audit

A: "Review the signing key lifecycle — confirm key rotation cadence, algorithm strength, and revocation procedures."
  "review" (security context) → SECURITY; "signing key" → SECURITY (signing trigger); "key rotation" → SECURITY; "algorithm strength" → SECURITY; "revocation" → SECURITY (certificate revocation context)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C019-003 — SECURITY+DOCUMENT: secret sprawl audit documented

A: "Audit all repositories for hardcoded secrets and document the remediation plan for each exposure."
  "audit" → SECURITY; "secrets" → SECURITY (secret trigger); "hardcoded secrets" → SECURITY; "document" → DOCUMENT
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"] | SubPhase: 6.3/6.2

---

## C019-004 — Gap: "secret manager setup" (configuration task) is gap without audit vocabulary

A: "Configure the secrets manager to store environment variables for the deployment pipeline."
  "configure" → IMPLEMENT? Only if "implement" or "enforce" is present. "configure" alone is borderline. No SECURITY audit vocabulary present.
  Correct: CompiledPredicateIds: [] — "configure" is a setup task, not a security audit action.
  Gap note: SECURITY fires on audit/review/scan of secrets, not on configuration setup tasks.

---

## C019-005 — SECURITY+QA: cipher key audit + QA of encryption health

A: "Audit the encryption key management service — verify key storage, cipher algorithm selection, and HSM (Hardware Security Module) integration — and QA the key lifecycle controls."
  "audit" → SECURITY; "encryption key" → SECURITY (encrypt/cipher trigger); "cipher algorithm" → SECURITY (cipher trigger); "QA" → QA
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhase: 6.3/7.3
