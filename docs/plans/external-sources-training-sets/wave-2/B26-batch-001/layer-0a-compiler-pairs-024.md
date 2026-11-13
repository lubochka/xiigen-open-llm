# Layer 0-A — Compiler Pairs (024) — Complete Predicate Reference
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: compiler

---

## C024-001 — Full SECURITY predicate trigger vocabulary reference

SECURITY fires on the following vocabulary groups (per B26 predicate rules):

  Authentication group: jwt, authentication, auth, oauth, token, session, mfa, credential
  Authorization group: rbac, access control (also fires AUTHORIZATION when privilege/IDOR context)
  Security action group: security, audit (verb), enforce, sanitization, scan (security context)
  Attack vector group: encrypt, tls, certificate, signing, secret, cipher, hash, asvs
  Vulnerability group: vulnerability, vulnerabilities, penetration, injection (SQL/XSS/SSRF/XXE)

A: "Audit the service for authentication bypass, TLS misconfigurations, and token signing vulnerabilities."
  All groups represented: "audit" (action) + "authentication bypass" (auth) + "TLS" (tls) + "token signing" (token/signing)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C024-002 — QA predicate trigger vocabulary reference

QA fires ONLY on the literal abbreviation "QA". "quality assurance" spelled out is gap.

A: "QA the security controls and verify the audit checklist."
  "QA" → QA (literal abbreviation fires); "quality assurance" → gap if written in full
  "verify" → gap (not a QA predicate trigger)
  CompiledPredicateIds: ["SECURITY", "QA"] — "audit checklist" (security context) → SECURITY; "QA" → QA

---

## C024-003 — SECURITY+QA sub-phase determination reference

SECURITY+QA → sub-phase 6.3 (Security and Compliance Review) as primary sub-phase.
QA alone → sub-phase 7.3 (QA Testing and Validation).
SECURITY+QA → feature_addition if IMPLEMENT also present; documentation if DOCUMENT only.

A: "Audit the JWT signing configuration and QA the token rotation policy."
  Result: SECURITY → 6.3 | QA → 7.3 | Primary: 6.3 | TransitionType: feature_addition

---

## C024-004 — Gap vocabulary summary: what does NOT fire SECURITY

Gap vocabulary (zero predicates unless paired with security-action verbs):
  GDPR, SOC2, ISO 27001, PCI-DSS (as standalone acronyms)
  "compliance", "compliant", "compliance-ready"
  "audit" (noun event: "the upcoming audit")
  "ensure", "make secure", "more secure"
  "pentest" (abbreviation) when used as event-scheduling noun
  "code review" without security co-occurrence

A: "Ensure GDPR compliance and prepare for the upcoming audit."
  "ensure" → gap; "GDPR" → gap; "upcoming audit" → gap (noun event)
  CompiledPredicateIds: [] → knowledge_edge

---

## C024-005 — SECURITY+QA boundary case: "quality assurance" vs "QA"

A-correct: "Audit the session management and QA the logout flow."
  "QA" → QA fires | CompiledPredicateIds: ["SECURITY", "QA"]

A-gap: "Audit the session management and perform quality assurance on the logout flow."
  "quality assurance" → gap (spelled out — does not fire QA); "perform" → gap
  CompiledPredicateIds: ["SECURITY"] only — QA predicate NOT fired.

Teaching: The QA predicate is strictly abbreviation-gated. "QA" (two capital letters) fires; "quality assurance" does not.
