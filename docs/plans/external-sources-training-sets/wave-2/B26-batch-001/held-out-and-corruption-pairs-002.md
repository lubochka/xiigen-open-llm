# Held-Out and Corruption Pairs (002)
# Batch: B26-batch-001 | Scenario: B26 — Security audit
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## H002-001 — Held-out: "audit" verb fires SECURITY; "audit" noun does not
Request A: "Audit the AdminService for SQL injection." → ["SECURITY"]
Request B: "The security audit is scheduled for Q3." → []
  Evidence B: "audit" (noun event) → gap; "security" without action verb → gap

## H002-002 — Corruption v6-01: GDPR fires SECURITY (invention)
Corrupt: "Ensure GDPR compliance." → ["SECURITY"]
Correct: []
  Fix: GDPR is compliance vocabulary — gap. SECURITY fires on attack/enforcement vocabulary.

## H002-003 — Held-out: "privilege escalation" fires AUTHORIZATION+SECURITY
Request: "Audit the AdminPanel for privilege escalation paths."
  Correct: ["SECURITY", "AUTHORIZATION"]
  Evidence: "audit" → SECURITY; "privilege escalation" → SECURITY+AUTHORIZATION

## H002-004 — Corruption v6-01: "code review" fires SECURITY (invention)
Corrupt: "Do a code review of the PaymentService." → ["SECURITY"]
Correct: []
  Fix: "code review" without security co-occurrence is gap.

## H002-005 — Corruption v6-02: SECURITY suppressed in XSS context
Corrupt: "Audit the SearchService for XSS vulnerabilities." → []
Correct: ["SECURITY"]
  Fix: "audit" + "XSS vulnerabilities" — both SECURITY triggers present. Cannot be suppressed.
