# Layer 3 — Teaching Unit Pairs (004)
# Batch: B26-batch-001 | Scenario: B26 — Security audit
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU004-001 — Teach: "sanitize"/"sanitization" fire SECURITY
  "Sanitize all user inputs." → ["SECURITY"] | "Implement input sanitization." → ["SECURITY", "IMPLEMENT"]
  Rule: Input/output sanitization is a security-action — always fires SECURITY.

## TU004-002 — Teach: "encrypt"/"encryption" fire SECURITY
  "Encrypt PII fields at rest." → ["SECURITY"] | "Audit encryption at rest." → ["SECURITY"]
  Rule: "encrypt" is a security-action verb in all contexts. "encryption" fires SECURITY as a noun.

## TU004-003 — Teach: "threat model" fires SECURITY even without "audit" verb
  "Create a threat model for the platform." → ["SECURITY", "IMPLEMENT"]
  "Document a threat model." → ["SECURITY", "DOCUMENT"]
  Rule: "threat model" fires SECURITY as security-action vocabulary regardless of action verb.

## TU004-004 — Teach: "pentest" and "penetration test" both fire SECURITY
  "Pentest the checkout API." → ["SECURITY", "API_CONTRACT"]
  "Run a penetration test." → ["SECURITY"]
  Rule: Penetration testing vocabulary fires SECURITY. "commission pentest" → ["SECURITY"] (IMPLEMENT does not fire on "commission").

## TU004-005 — Teach: security-adjacent nouns fire SECURITY without an action verb
  Nouns that fire SECURITY standalone: vulnerability, CVE, injection, XSS, CSRF, SSRF, IDOR, XXE, plaintext (in credential context)
  Nouns that do NOT fire SECURITY standalone: risk, compliance, policy, control (without enforcement verb)
  Rule: Attack-vector nouns fire SECURITY. Risk-management nouns are gap.
