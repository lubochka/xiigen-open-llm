# Layer 0-A — Compiler Pairs (018)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: compiler

---

## C018-001 — SECURITY+QA: penetration test review

A: "Review the penetration test findings for the AccountService and QA all critical vulnerability remediations."
  "review" (security context) → SECURITY; "penetration test" → SECURITY (penetration trigger); "vulnerability remediations" → SECURITY; "QA" → QA
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhase: 6.3/7.3
  Note: "penetration" is an explicit SECURITY trigger per predicate rules.

---

## C018-002 — SECURITY: red team report review

A: "Review the red team engagement report for lateral movement and credential harvesting findings."
  "review" (security context) → SECURITY; "credential harvesting" → SECURITY (credential trigger); "lateral movement" → SECURITY (attack technique)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C018-003 — SECURITY+DOCUMENT: penetration test remediation documented

A: "Audit the penetration test remediation status for all CRITICAL findings and document the residual risk."
  "audit" → SECURITY; "penetration test" → SECURITY; "CRITICAL findings" → SECURITY context; "document" → DOCUMENT
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"] | SubPhase: 6.3/6.2

---

## C018-004 — Gap: "pen test scheduling" is gap

A: "Schedule the pen test for Q3 and coordinate with the vendor."
  "schedule" → gap; "pen test" → SECURITY? No: "schedule the pen test" is a logistics noun phrase, not a security-action verb. Context is event-scheduling, not audit execution.
  CompiledPredicateIds: [] → knowledge_edge
  Gap: "schedule [event]" is gap. SECURITY fires on "audit/review/scan [service]" patterns.

---

## C018-005 — SECURITY+QA: brute-force protection audit + QA

A: "Audit the LoginService brute-force protection — review lockout thresholds, hash algorithm strength, and MFA (Multi-Factor Authentication) bypass paths — and QA the controls."
  "audit" → SECURITY; "brute-force" → SECURITY; "hash algorithm" → SECURITY (hash trigger); "MFA" → SECURITY (mfa trigger); "QA" → QA
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhase: 6.3/7.3
