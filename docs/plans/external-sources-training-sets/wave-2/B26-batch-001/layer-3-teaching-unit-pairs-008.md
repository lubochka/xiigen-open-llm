# Layer 3 — Teaching Unit Pairs (008)
# Batch: B26-batch-001 | Scenario: B26 — Security audit
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU008-001 — Teach: "IDOR" fires SECURITY+AUTHORIZATION
  "Audit for IDOR vulnerabilities." → ["SECURITY", "AUTHORIZATION"]
  "Customers should not access other customers' data." → ["AUTHORIZATION"] (no SECURITY without attack vocabulary)
  Rule: IDOR fires both predicates. The sentence "customers cannot access" fires AUTHORIZATION only.

## TU008-002 — Teach: "session management" fires SECURITY in audit context
  "Audit the session management — verify expiry and logout invalidation." → ["SECURITY", "TEST"]
  "The session management uses 30-minute timeouts." → [] (informational — gap)
  Rule: "session management" fires SECURITY with audit/verify action verb. Informational mentions are gap.

## TU008-003 — Teach: risk vocabulary is gap
  "risk", "risk assessment", "risk management", "risk mitigation" → all gap
  "Security risk" → SECURITY only because "security" is present, not "risk"
  "Perform a risk assessment." → [] | "Audit security risks." → ["SECURITY"]
  Rule: "risk" alone is gap. SECURITY fires on "security" co-occurrence, not on "risk".

## TU008-004 — Teach: "mass assignment" fires SECURITY
  "Audit the UserController for mass assignment vulnerabilities." → ["SECURITY"]
  Rule: "mass assignment" is a security vulnerability class — fires SECURITY.

## TU008-005 — Teach: HTTPS fires SECURITY in enforcement context; gap in informational
  "Enforce HTTPS on all endpoints." → ["SECURITY"] | "The service uses HTTPS." → [] (gap)
  "Audit for HTTP endpoints that should be HTTPS." → ["SECURITY"]
  Rule: HTTPS fires SECURITY in enforcement/audit context. Informational "uses HTTPS" is gap.
