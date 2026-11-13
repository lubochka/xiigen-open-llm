# Held-Out and Corruption Pairs (004)
# Batch: B26-batch-001 | Scenario: B26 — Security audit
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## H004-001 — Held-out: "sanitize" fires SECURITY
Request: "Sanitize all user inputs to the OrderService before database storage."
  Correct: ["SECURITY"]
  Evidence: "sanitize" → SECURITY (input sanitization is a security action)

## H004-002 — Corruption v6-01: "risk assessment" fires SECURITY (invention)
Corrupt: "Perform a risk assessment of the payment processing pipeline." → ["SECURITY"]
Correct: []
  Fix: "risk assessment" is risk-management vocabulary — gap.

## H004-003 — Held-out: "scan" fires SECURITY in vulnerability context
Request: "Scan the project dependencies for CVE vulnerabilities before the release."
  Correct: ["SECURITY"]
  Evidence: "scan" + "CVE vulnerabilities" → SECURITY

## H004-004 — Corruption v6-02: SECURITY suppressed in "enforce" context
Corrupt: "Enforce HTTPS on all endpoints." → []
Correct: ["SECURITY"]
  Fix: "enforce" fires SECURITY in all enforcement contexts.

## H004-005 — Held-out: SECURITY routes to 5.2 (not 6.3) in "write tests" context
Request: "Write security tests that assert the AuthService blocks forged JWT tokens."
  Correct: CompiledPredicateIds: ["SECURITY", "IMPLEMENT", "TEST"] | SECURITY SubPhase: 5.2
  Teaching: Test-writing context → SECURITY 5.2. Audit context → SECURITY 6.3.
