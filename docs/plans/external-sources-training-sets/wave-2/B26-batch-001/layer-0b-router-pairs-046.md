# Layer 0-B — Router Pairs (046) — BrokenAuthAudit Routing
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: router

---

## R046-001 — Broken auth audit → 6.3

R046-001 test: "Audit the authentication flow for broken authentication patterns."
  SECURITY → 6.3 | CompiledPredicateIds: ["SECURITY"] | TrustState: deterministic_generic

---

## R046-002 — MFA bypass review → 6.3

R046-002 test: "Review the MFA implementation for bypass vulnerabilities via fallback paths."
  SECURITY → 6.3 | CompiledPredicateIds: ["SECURITY"] | TrustState: deterministic_generic

---

## R046-003 — "add login page" → GapHandler

R046-003 test: "Add a login page for the new admin section."
  CompiledPredicateIds: [] | GapHandler: SCHEDULE_CAPABILITY_LEARNING

---

## R046-004 — Broken auth + RBAC bypass → 6.3 (SECURITY+AUTHORIZATION)

R046-004 test: "Audit broken auth paths allowing RBAC authorization gate bypass."
  SECURITY → 6.3 | AUTHORIZATION → 6.3 | Dual predicates, same sub-phase

---

## R046-005 — Broken auth + test-writing → 5.2

R046-005 test: "Write security tests asserting the AuthService blocks credential stuffing."
  SECURITY → 5.2 (write-tests context)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 5.2
