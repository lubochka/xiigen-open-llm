# Layer 3 — Teaching Unit Pairs (003)
# Batch: B26-batch-001 | Scenario: B26 — Security audit
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU003-001 — Teach: "secure" as adjective is gap
  "Make the application more secure." → [] (gap)
  "Secure the application by implementing TLS." → ["SECURITY", "IMPLEMENT"]
  Rule: "secure" as adjective is gap. As a verb ("secure the endpoint") it fires SECURITY only with enforcement context.

## TU003-002 — Teach: XSS/SQL/CSRF/SSRF/IDOR abbreviations fire SECURITY
  All injection and attack-class abbreviations fire SECURITY:
  XSS, SQL injection, CSRF, SSRF, IDOR, XXE, LFI, RFI, SSTI → all fire SECURITY
  Rule: Attack vector abbreviations and full names are SECURITY predicate triggers.

## TU003-003 — Teach: "code review" without security co-occurrence is gap
  "Do a code review of the PaymentService." → [] (gap)
  "Do a security code review of the PaymentService." → ["SECURITY"]
  Rule: "review" fires SECURITY only with security-vocabulary co-occurrence.

## TU003-004 — Teach: "privilege escalation" fires AUTHORIZATION+SECURITY
  "Audit the AdminService for privilege escalation paths." → ["SECURITY", "AUTHORIZATION"]
  Rule: Attack vectors that are also authorization failures fire both predicates.
  IDOR, broken access control, privilege escalation → ["SECURITY", "AUTHORIZATION"]

## TU003-005 — Teach: "enforce" fires SECURITY in all contexts
  "Enforce HTTPS." → ["SECURITY"] | "Enforce password complexity." → ["SECURITY"]
  "Enforce rate limits." → ["SECURITY"] | "Enforce input validation." → ["SECURITY"]
  Rule: "enforce" fires SECURITY regardless of what policy is being enforced — it is a security-action verb.
