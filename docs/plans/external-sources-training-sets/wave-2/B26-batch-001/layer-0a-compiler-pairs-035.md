# Layer 0-A — Compiler Pairs (035) — XSSMitigation
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: compiler

---

## C035-001 — XSS (Cross-Site Scripting) audit → SECURITY

A: "Audit all user-input rendering paths for stored XSS and reflected XSS vulnerabilities."
  "audit" → SECURITY | "XSS" → SECURITY (attack vector) | "vulnerabilities" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C035-002 — XSS + sanitization vocabulary

A: "Review the output encoding and sanitization logic for DOM-based XSS injection points."
  "Review" + "sanitization" → SECURITY | "XSS injection" → SECURITY | "output encoding" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C035-003 — XSS + CSP mitigation audit

A: "Audit the CSP configuration as a secondary defense layer against XSS in the admin panel."
  "audit" → SECURITY | "CSP" → SECURITY | "XSS" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C035-004 — "protect against XSS" is SECURITY — actionable despite adjective form

A: "Protect all user-generated content fields against stored XSS attacks."
  "protect" → SECURITY (protect is in security-action group) | "XSS" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3
  Teaching: "protect" is a security-action verb — fires SECURITY when paired with attack vocabulary.

---

## C035-005 — XSS + IMPLEMENT for fix implementation

A: "Implement HTML entity encoding for all template output variables to fix the stored XSS vulnerability."
  "Implement" → IMPLEMENT | "stored XSS vulnerability" → SECURITY
  "HTML entity encoding" → SECURITY (sanitization group)
  CompiledPredicateIds: ["IMPLEMENT", "SECURITY"] | TransitionType: feature_addition
