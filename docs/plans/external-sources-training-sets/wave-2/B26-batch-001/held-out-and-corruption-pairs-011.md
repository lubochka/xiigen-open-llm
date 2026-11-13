# Held-Out and Corruption Pairs (011)
# Batch: B26-batch-001 | Scenario: B26 — Security audit
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## H011-001 — Held-out: "OWASP" fires SECURITY
Request: "Review the BillingService against the OWASP API Security Top 10."
  Correct: ["SECURITY"]
  Evidence: "review" (security co-occurrence) → SECURITY; "OWASP" → SECURITY

## H011-002 — Corruption v6-01: "security architecture" fires ARCHITECTURE only (drops SECURITY)
Corrupt: "Review the security architecture of the platform." → ["ARCHITECTURE"]
Correct: ["SECURITY", "ARCHITECTURE"]
  Fix: "security architecture" fires both. "security" co-occurrence cannot be ignored.

## H011-003 — Held-out: "directory traversal" fires SECURITY
Request: "Scan the FileStorageService for directory traversal vulnerabilities."
  Correct: ["SECURITY"]
  Evidence: "scan" → SECURITY; "directory traversal vulnerabilities" → SECURITY

## H011-004 — Corruption v6-02: SECURITY suppressed by "document" in security finding doc
Corrupt: "Document the security findings from the BillingService audit." → ["DOCUMENT"]
Correct: ["SECURITY", "DOCUMENT"]
  Fix: "security findings" fires SECURITY. "document" fires DOCUMENT. Both present.

## H011-005 — Held-out: "bcrypt" fires SECURITY in password hashing context
Request: "Audit the UserService — confirm passwords are hashed with bcrypt at cost factor ≥12."
  Correct: ["SECURITY"]
  Evidence: "audit" → SECURITY; "bcrypt" → SECURITY (cryptographic algorithm)
