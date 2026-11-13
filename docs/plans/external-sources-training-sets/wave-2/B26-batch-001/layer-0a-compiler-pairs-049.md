# Layer 0-A — Compiler Pairs (049) — PenTestReport
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: compiler

---

## C049-001 — Penetration test findings audit → SECURITY

A: "Audit the penetration test report findings and triage each vulnerability by severity."
  "audit" → SECURITY | "vulnerability" → SECURITY | "penetration test" = security-action context
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C049-002 — Pen test + document → SECURITY+DOCUMENT

A: "Review the external pen test findings and document the remediation plan for each critical vulnerability."
  "review" → SECURITY | "vulnerability" → SECURITY | "document" → DOCUMENT
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"] | TransitionType: documentation

---

## C049-003 — "schedule a pen test" → gap (event noun)

A: "Schedule the quarterly penetration test with the external security firm."
  "schedule" → gap | "penetration test" as noun event → gap
  CompiledPredicateIds: [] → knowledge_edge
  Teaching: Scheduling an audit is always gap. The event noun "penetration test" without audit verb is gap.

---

## C049-004 — Pen test scope + injection findings → SECURITY

A: "Review the pen test scope covering SQL injection, XSS (Cross-Site Scripting), and authentication bypass vectors."
  "review" → SECURITY | "SQL injection" → SECURITY | "XSS" → SECURITY | "authentication bypass" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3 | TrustState: deterministic_generic

---

## C049-005 — Pen test + IMPLEMENT remediation → IMPLEMENT+SECURITY

A: "Implement remediations from the pen test report: patch the SSRF endpoint and rotate exposed credentials."
  "implement" → IMPLEMENT | "SSRF endpoint" → SECURITY | "rotate exposed credentials" → SECURITY
  CompiledPredicateIds: ["IMPLEMENT", "SECURITY"] | TransitionType: feature_addition
