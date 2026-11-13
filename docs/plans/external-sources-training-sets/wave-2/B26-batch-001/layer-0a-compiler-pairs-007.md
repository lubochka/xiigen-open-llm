# Layer 0-A — Compiler Pairs (007)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: compiler

---

## C007-001 — SECURITY via "session fixation" vulnerability

A: "Audit the LoginService for session fixation and session hijacking vulnerabilities."
  "audit" → SECURITY; "session fixation" → SECURITY; "session hijacking" → SECURITY; "vulnerabilities" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C007-002 — SECURITY + QA via "QA security controls"

A: "QA the security controls on the DataExportService — verify encryption at rest and in transit."
  "QA" → QA; "security controls" → SECURITY; "verify" → TEST; "encryption at rest" → SECURITY; "in transit" → SECURITY
  CompiledPredicateIds: ["QA", "TEST", "SECURITY"] | SubPhase: 6.3

---

## C007-003 — SECURITY + DOCUMENT via "security findings report"

A: "Document the security findings from the ReportingService audit, including all critical and high severity issues."
  "document" → DOCUMENT; "security findings" → SECURITY; "audit" → SECURITY; "severity" → gap
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"] | SubPhase: 6.3/6.2

---

## C007-004 — SECURITY + AUTHORIZATION via "default-deny" policy

A: "Review the NotificationService authorization rules — confirm default-deny is applied for all unauthenticated callers."
  "review" (security context) → SECURITY; "authorization rules" → AUTHORIZATION; "default-deny" → AUTHORIZATION+SECURITY; "unauthenticated" → AUTHORIZATION
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"] | SubPhase: 6.3

---

## C007-005 — Gap: "code review" (general) does not fire SECURITY

A: "Do a code review of the PaymentService before merging to main."
  "code review" → gap ("review" without security co-occurrence is gap); "merging" → gap
  CompiledPredicateIds: [] | GapHandler: SCHEDULE_CAPABILITY_LEARNING, domainId: "software-development"
  Teaching: "code review" without security vocabulary is gap. SECURITY fires on "review" only when security-action co-occurrence is present (e.g., "review for vulnerabilities", "security review").
