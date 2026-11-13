# Held-Out and Corruption Pairs (005)
# Batch: B26-batch-001 | Scenario: B26 — Security audit
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## H005-001 — Held-out: "IDOR" fires SECURITY+AUTHORIZATION
Request: "Audit the ProductService for IDOR vulnerabilities."
  Correct: ["SECURITY", "AUTHORIZATION"]

## H005-002 — Corruption v6-01: "security training" fires SECURITY (invention)
Corrupt: "Run phishing awareness training for the engineering team." → ["SECURITY"]
Correct: []
  Fix: Security awareness training is organizational — gap.

## H005-003 — Held-out: "AES-256" fires SECURITY
Request: "Audit the DataVaultService to confirm AES-256 is used for data at rest."
  Correct: ["SECURITY"]
  Evidence: "audit" → SECURITY; "AES-256" → SECURITY (cryptographic algorithm)

## H005-004 — Corruption v6-03: SECURITY+AUTHORIZATION collapsed to AUTHORIZATION only
Corrupt: "Audit the AdminService for privilege escalation paths." → ["AUTHORIZATION"]
Correct: ["SECURITY", "AUTHORIZATION"]
  Fix: "audit" + "privilege escalation" fires SECURITY. AUTHORIZATION fires on "privilege escalation" as access control failure. Both fire.

## H005-005 — Held-out: "plaintext" fires SECURITY
Request: "Confirm the UserService never stores passwords in plaintext."
  Correct: ["SECURITY"]
  Evidence: "confirm" is gap; "plaintext passwords" → SECURITY (credential exposure)
  Note: "confirm" does not fire TEST here — it is gap (not "verify"/"assert"/"validate")
