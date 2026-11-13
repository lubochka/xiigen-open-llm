# Held-Out and Corruption Pairs (016)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## H016-001 — Held-out: "STRIDE" fires SECURITY
Request: "Review the threat model — validate STRIDE coverage for the AuthService."
  Correct: ["SECURITY"]
  Evidence: "review" (threat model context) → SECURITY; "STRIDE" → SECURITY (threat modeling framework); "threat model" → SECURITY

## H016-002 — Corruption v6-01: "threat model" without security vocabulary fires SECURITY (overfire)
Corrupt: "Create a threat model diagram for the new feature." → ["SECURITY"]
Correct: ["DOCUMENT"] (draw/create diagram → DOCUMENT)
  Fix: "create a diagram" → DOCUMENT, not SECURITY. "threat model" in a creation context shifts the predicate.

## H016-003 — Held-out: "attack surface" fires SECURITY
Request: "Audit the external-facing attack surface — enumerate all ingress points and authentication mechanisms."
  Correct: ["SECURITY"]
  Evidence: "audit" → SECURITY; "attack surface" → SECURITY; "authentication mechanisms" → SECURITY

## H016-004 — Corruption v6-03: AUTHORIZATION predicate dropped from privilege escalation request
Corrupt: "Audit the AdminService for privilege escalation paths." → ["SECURITY"]
Correct: ["SECURITY", "AUTHORIZATION"]
  Fix: "privilege escalation" fires BOTH SECURITY and AUTHORIZATION. AUTHORIZATION must not be dropped.

## H016-005 — Held-out: "mass assignment vulnerability" fires SECURITY
Request: "Audit the UserUpdateEndpoint for mass assignment vulnerabilities — can unintended fields be overwritten via bulk update?"
  Correct: ["SECURITY"]
  Evidence: "audit" → SECURITY; "mass assignment vulnerabilities" → SECURITY (mass assignment is a known vulnerability class)
