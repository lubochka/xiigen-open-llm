# Layer 0-C — Knowledge Edge Pairs (019)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E019-001 — "secret" is an explicit SECURITY trigger; "configuration secret" fires SECURITY

A-fires: "Audit the ConfigService for hardcoded secrets — SMTP (Simple Mail Transfer Protocol) password, Stripe API key, database connection string."
  "audit" → SECURITY; "hardcoded secrets" → SECURITY (secret trigger); "SMTP password" → SECURITY (credential); "Stripe API key" → SECURITY (secret/credential)
  CompiledPredicateIds: ["SECURITY"]

A-gap: "The ConfigService reads configuration from appsettings.json."
  "reads configuration" → gap; "appsettings.json" → gap
  CompiledPredicateIds: []

---

## E019-002 — "key rotation" fires SECURITY; "configuration rotation" is gap

A-fires: "Review the key rotation policy for the JWT signing secret — is rotation triggered automatically on expiry?"
  "review" (security context) → SECURITY; "key rotation" → SECURITY; "JWT signing secret" → SECURITY (jwt/signing/secret triggers)
  CompiledPredicateIds: ["SECURITY"]

A-gap: "The configuration rotation happens every 30 days via the deployment pipeline."
  "configuration rotation" → gap (deployment vocabulary, no security-action context)
  CompiledPredicateIds: []

---

## E019-003 — "HSM" (Hardware Security Module) fires SECURITY via hardware security context

A: "Audit the payment processing HSM configuration — is the private signing key stored in the HSM and never exported to software?"
  "audit" → SECURITY; "HSM" → SECURITY (hardware security = security domain); "private signing key" → SECURITY (signing trigger); "never exported" → SECURITY context
  CompiledPredicateIds: ["SECURITY"]

---

## E019-004 — "vault" alone is gap; "secret vault" fires SECURITY via "secret"

A-gap: "Configure the HashiCorp Vault deployment for the staging environment."
  "configure" → gap; "Vault" → gap (infrastructure noun, no audit vocabulary)
  CompiledPredicateIds: []

A-fires: "Audit the HashiCorp Vault secret management — confirm all secrets have TTL (Time-To-Live) policies and audit logging enabled."
  "audit" → SECURITY; "secret management" → SECURITY (secret trigger); "secrets" → SECURITY; "TTL policies" → SECURITY context
  CompiledPredicateIds: ["SECURITY"]

---

## E019-005 — "certificate" is an explicit SECURITY trigger; "certificate" for non-security use cases requires audit vocabulary

A-fires: "Audit the TLS certificate expiry dates across all production services — flag any certificate expiring within 30 days."
  "audit" → SECURITY; "TLS certificate" → SECURITY (tls/certificate triggers)
  CompiledPredicateIds: ["SECURITY"]

A-borderline: "The deployment pipeline uses a certificate to sign build artifacts."
  "certificate to sign" → SECURITY (certificate/signing triggers); action is deployment (not audit)
  CompiledPredicateIds: ["SECURITY"] — weak signal; "certificate" + "sign" fire independently of audit context.
