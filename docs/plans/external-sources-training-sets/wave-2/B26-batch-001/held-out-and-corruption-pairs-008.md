# Held-Out and Corruption Pairs (008)
# Batch: B26-batch-001 | Scenario: B26 — Security audit
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## H008-001 — Held-out: "session fixation" fires SECURITY
Request: "Audit the LoginService for session fixation and session hijacking."
  Correct: ["SECURITY"]
  Evidence: "audit" → SECURITY; "session fixation" + "session hijacking" → SECURITY attack vectors

## H008-002 — Corruption v6-01: "security posture" fires ARCHITECTURE (incorrect)
Corrupt: "Review the security posture of the microservices platform." → ["ARCHITECTURE"]
Correct: ["SECURITY"]
  Fix: "security posture" fires SECURITY via "security" co-occurrence. No ARCHITECTURE without "design/diagram/structure" vocabulary.

## H008-003 — Held-out: "information disclosure" fires SECURITY
Request: "Scan the ErrorHandlingMiddleware for information disclosure in stack trace responses."
  Correct: ["SECURITY"]
  Evidence: "scan" → SECURITY; "information disclosure" → SECURITY

## H008-004 — Corruption v6-02: SECURITY suppressed in "enforce" context
Corrupt: "Enforce input validation on all API endpoints." → ["API_CONTRACT"]
Correct: ["SECURITY", "API_CONTRACT"]
  Fix: "enforce" fires SECURITY; "API endpoints" fires API_CONTRACT. Both fire.

## H008-005 — Held-out: "nonce" fires SECURITY
Request: "Verify the CSRF middleware generates a unique nonce per request."
  Correct: ["TEST", "SECURITY"]
  Evidence: "verify" → TEST; "CSRF middleware" → SECURITY; "nonce" → SECURITY
