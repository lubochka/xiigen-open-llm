# Layer 0-A — Compiler Pairs (051) — BugBountyTriage
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: compiler

---

## C051-001 — Bug bounty vulnerability triage → SECURITY

A: "Triage the bug bounty submission reporting a stored XSS (Cross-Site Scripting) vulnerability in the comment field."
  "triage" = audit-action vocabulary in security context → SECURITY | "XSS" → SECURITY | "vulnerability" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C051-002 — IDOR bug bounty report → SECURITY + AUTHORIZATION

A: "Review the bug bounty report describing an IDOR (Insecure Direct Object Reference) in the invoice download endpoint."
  "review" → SECURITY | "IDOR" → SECURITY+AUTHORIZATION (dual-predicate)
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"] | SubPhase: 6.3

---

## C051-003 — "respond to bug bounty researcher" → gap

A: "Respond to the bug bounty researcher with a thank-you and reward payout confirmation."
  "respond" → gap | "thank-you" → gap (communication task, no security-action vocabulary)
  CompiledPredicateIds: [] → knowledge_edge

---

## C051-004 — Bug bounty CSRF finding → SECURITY

A: "Audit the CSRF (Cross-Site Request Forgery) vulnerability reported via bug bounty on the password reset endpoint."
  "audit" → SECURITY | "CSRF" → SECURITY (attack vector)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C051-005 — Bug bounty triage + IMPLEMENT fix → IMPLEMENT+SECURITY

A: "Triage the bug bounty SSRF report and implement URL allowlist validation to close the vulnerability."
  "triage" → SECURITY | "SSRF" → SECURITY | "implement" → IMPLEMENT
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT"] | TransitionType: feature_addition
