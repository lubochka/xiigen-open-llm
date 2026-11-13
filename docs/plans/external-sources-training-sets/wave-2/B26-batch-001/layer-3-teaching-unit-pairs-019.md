# Layer 3 — Teaching Unit Pairs (019)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU019-001 — "secret" fires SECURITY; "hardcoded secrets" is a dense SECURITY signal
A: "Audit all repositories for hardcoded secrets." Does SECURITY fire?
B: Yes. "audit" → SECURITY. "secrets" → SECURITY (secret trigger). "hardcoded secrets" is a classic SECURITY finding pattern.
   CompiledPredicateIds: ["SECURITY"]. Deterministic_generic (audit + secret).

## TU019-002 — "key rotation" fires SECURITY; "configuration rotation" does not
A: "Review the key rotation policy for JWT signing secrets." vs "The config rotates every 30 days in the pipeline."
B: "key rotation" + "JWT signing secrets" → SECURITY (key rotation + jwt + signing + secret — 4 triggers).
   "config rotates every 30 days" → gap (deployment vocabulary, no security-action context).
   Rule: "key rotation" in security context fires SECURITY; "rotation" in deployment/config context is gap.

## TU019-003 — "HSM" fires SECURITY via hardware security context
A: "Audit the payment HSM configuration — is the private signing key stored in the HSM only?" Does SECURITY fire?
B: Yes. "audit" → SECURITY. "HSM" → SECURITY (Hardware Security Module = security-domain hardware). "private signing key" → SECURITY (signing trigger).
   Triple signal. CompiledPredicateIds: ["SECURITY"].

## TU019-004 — "secret vault" fires SECURITY via "secret" trigger; "vault" alone is gap
A: "Audit the HashiCorp Vault secret management — confirm all secrets have TTL policies." vs "Configure the Vault deployment for staging."
B: "audit" + "secret management" + "secrets" → SECURITY. CompiledPredicateIds: ["SECURITY"].
   "configure the Vault deployment" → gap. CompiledPredicateIds: [].

## TU019-005 — "certificate expiry" fires SECURITY via certificate trigger
A: "Audit the TLS certificate expiry dates across all production services." Does SECURITY fire?
B: Yes. "audit" → SECURITY. "TLS certificate" → SECURITY (tls + certificate triggers). "expiry" → security lifecycle context.
   CompiledPredicateIds: ["SECURITY"].
