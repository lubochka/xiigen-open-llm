# Held-Out and Corruption Pairs (023)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## H023-001 — Held-out: "bcrypt cost factor" fires SECURITY
Request: "Audit the UserService password hashing — is bcrypt cost factor ≥ 12, or is MD5 still in use?"
  Correct: ["SECURITY"]
  Evidence: "audit" → SECURITY; "bcrypt cost factor" → SECURITY (hash trigger); "MD5" → SECURITY (insecure hash algorithm)

## H023-002 — Corruption v6-01: "database field" fires SECURITY (invention)
Corrupt: "Add a new database field to store the user's preferred language." → ["SECURITY"]
Correct: []
  Fix: Schema changes without security/vulnerability vocabulary are gap. Zero predicates.

## H023-003 — Held-out: "AES-GCM vs CBC" fires SECURITY (cipher triggers)
Request: "Review the encryption mode — is AES-GCM in use, or is the vulnerable AES-CBC still active?"
  Correct: ["SECURITY"]
  Evidence: "review" (security context) → SECURITY; "AES-GCM" → SECURITY; "AES-CBC" → SECURITY; "vulnerable" → SECURITY

## H023-004 — Corruption v6-03: IMPLEMENT dropped from "audit + implement" request
Corrupt: "Audit encryption for CBC mode vulnerabilities and implement AES-GCM as replacement." → ["SECURITY"]
Correct: ["SECURITY", "IMPLEMENT"]
  Fix: "implement" fires IMPLEMENT independently. TransitionType changes to feature_addition when IMPLEMENT is present.

## H023-005 — Held-out: "keys in source control" fires SECURITY (critical finding)
Request: "Audit the repository for encryption keys committed to source control."
  Correct: ["SECURITY"]
  Evidence: "audit" → SECURITY; "encryption keys" → SECURITY (encrypt trigger); "source control" → context (secret exposure pattern)
