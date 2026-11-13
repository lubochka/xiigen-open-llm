# Layer 0-A — Compiler Pairs (005)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: compiler

---

## C005-001 — SECURITY via "SQL injection" audit

A: "Audit all database query points in the OrderService for SQL injection risks."
  "audit" → SECURITY; "SQL injection" → SECURITY; "risks" → gap (risk assessment vocabulary)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C005-002 — SECURITY + TEST via "assert no injection"

A: "Assert that the SearchService input parser blocks SQL injection attempts."
  "assert" → TEST; "SQL injection" → SECURITY; "blocks" → SECURITY (blocking/preventing attack)
  CompiledPredicateIds: ["TEST", "SECURITY"] | SubPhase: 5.2
  Note: Test-assertion context → SECURITY routes to 5.2 (not 6.3)

---

## C005-003 — SECURITY + AUTHORIZATION + QA via privilege audit

A: "Audit the PermissionService for privilege escalation vulnerabilities, QA the admin role assignments."
  "audit" → SECURITY; "privilege escalation" → AUTHORIZATION+SECURITY; "vulnerabilities" → SECURITY; "QA" → QA; "admin role" → AUTHORIZATION
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION", "QA"] | SubPhase: 6.3

---

## C005-004 — SECURITY + IMPLEMENT via "patch"

A: "Patch the reflected XSS vulnerability in the CustomerFeedback form and add output encoding."
  "patch" → IMPLEMENT (patch as a code fix/implementation); "reflected XSS" → SECURITY; "add" → IMPLEMENT; "output encoding" → SECURITY
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT"] | SubPhase: 6.3/4.1

---

## C005-005 — Gap: "risk assessment" vocabulary is gap

A: "Perform a risk assessment of the data processing pipeline."
  "perform" → gap; "risk assessment" → gap (risk management vocabulary, not security-action); "data processing" → gap
  CompiledPredicateIds: [] | GapHandler: SCHEDULE_CAPABILITY_LEARNING, domainId: "software-development"
  Teaching: "risk assessment" is risk-management vocabulary — gap. SECURITY requires attack/vulnerability/enforcement vocabulary.
