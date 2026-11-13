# Layer 0-B — Router Pairs (037) — CSRFProtection Routing
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: router

---

## R037-001 — CSRF audit → 6.3

R037-001 test: "Audit all state-changing endpoints for missing CSRF token validation."
  SECURITY → 6.3 | CompiledPredicateIds: ["SECURITY"] | TrustState: deterministic_generic

---

## R037-002 — CSRF + test-writing → 5.2

R037-002 test: "Write CSRF protection tests for the checkout POST endpoint."
  SECURITY → 5.2 (test-writing context: "write tests")
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 5.2
  Teaching: "write CSRF tests" → 5.2. "audit CSRF exposure" → 6.3.

---

## R037-003 — CSRF + IMPLEMENT → 4.1 + 6.3

R037-003 test: "Implement CSRF double-submit cookie protection and audit all existing state-changing endpoints."
  IMPLEMENT → 4.1 | SECURITY → 6.3
  PrimarySubPhaseId: 6.3 | AllSubPhaseIds: [4.1, 6.3]

---

## R037-004 — "configure CSRF middleware" → GapHandler

R037-004 test: "Configure the CSRF middleware in the ASP.NET pipeline."
  CompiledPredicateIds: [] | GapHandler: SCHEDULE_CAPABILITY_LEARNING

---

## R037-005 — CSRF + DOCUMENT → 6.3 + 6.2

R037-005 test: "Audit CSRF exposure and document findings for the security review board."
  SECURITY → 6.3 | DOCUMENT → 6.2
  AllSubPhaseIds: [6.3, 6.2] | TransitionType: documentation
