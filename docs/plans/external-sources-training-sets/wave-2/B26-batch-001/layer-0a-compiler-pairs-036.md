# Layer 0-A — Compiler Pairs (036) — Complete SECURITY Reference
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: compiler

---

## C036-001 — Complete SECURITY trigger vocabulary table

SECURITY fires on ALL of the following (any single term is sufficient with an action verb):
  Auth group: jwt, authentication, auth, oauth, token, session, mfa, credential, password, passphrase
  Authorization group: rbac, access control, privilege, permission boundary, role claim
  Cryptographic group: encrypt, tls, certificate, cipher, hash, asvs, hmac, sha, rsa, bcrypt, key rotation
  Attack vector group: vulnerability, injection, xss, csrf, ssrf, xxe, idor, mitm, sqli, path traversal
  Security action group: audit (verb), harden, protect, sanitize, scan (security context), enforce

A: Reference-only entry. No A/B pair.

---

## C036-002 — SECURITY predicate: solo-noun triggers (no verb needed)

A: "Check whether JWT is properly configured."
  "JWT" → SECURITY fires as solo noun; "check" is a weak action but JWT carries the trigger
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C036-003 — SECURITY multi-group vocabulary stacking

A: "Audit the bcrypt password hashing, TLS certificate pinning, and JWT token signing."
  "audit" → SECURITY | "bcrypt" → SECURITY (crypto) | "TLS" → SECURITY | "JWT" → SECURITY | "signing" → SECURITY
  Five independent SECURITY triggers — all in one request → deterministic_generic (high confidence)
  CompiledPredicateIds: ["SECURITY"] | TrustState: deterministic_generic

---

## C036-004 — "ensure quality of security" → SECURITY, NOT QA

A: "Ensure the quality of security controls across the authentication layer."
  "ensure quality of security controls" → SECURITY (security action vocabulary: "security controls", "authentication")
  "quality of security" does NOT fire QA — QA only fires on literal "QA" abbreviation
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3
  Teaching: "quality" without the literal "QA" abbreviation never fires QA predicate.

---

## C036-005 — Gap summary: what never fires SECURITY regardless of context

Never fires SECURITY:
  - Regulatory labels alone: GDPR, SOC2, ISO 27001, PCI-DSS, HIPAA, NIST CSF (as standalone acronyms)
  - "compliance", "compliant", "certification", "compliance-ready"
  - "make more secure", "more secure", "ensure it's secure"
  - "risk assessment", "risk management", "risk register"
  - "code review" (alone, without security-specific vocabulary)
  - "review security posture" (conceptually, without technical action scope)
  CompiledPredicateIds: [] for all the above → knowledge_edge
