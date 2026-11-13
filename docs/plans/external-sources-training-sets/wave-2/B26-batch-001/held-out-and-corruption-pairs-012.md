# Held-Out and Corruption Pairs (012)
# Batch: B26-batch-001 | Scenario: B26 — Security audit
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## H012-001 — Held-out: "XXE" fires SECURITY
Request: "Scan the DataImportService for XXE injection vulnerabilities in the XML parser."
  Correct: ["SECURITY"]
  Evidence: "scan" → SECURITY; "XXE injection vulnerabilities" → SECURITY

## H012-002 — Corruption v6-01: "pen test" fires TEST (invention)
Corrupt: "Pentest the CheckoutService API." → ["TEST", "API_CONTRACT"]
Correct: ["SECURITY", "API_CONTRACT"]
  Fix: "pentest" fires SECURITY (penetration testing vocabulary), NOT TEST. TEST requires "test/tests/testing/verify/assert/validate/spec".

## H012-003 — Held-out: SECURITY+QA fires when both present
Request: "Audit the authentication layer for vulnerabilities and QA the access control policies."
  Correct: ["SECURITY", "QA"]
  Evidence: "audit" + "authentication" + "vulnerabilities" → SECURITY; "QA" → QA

## H012-004 — Corruption v6-03: Four-predicate request collapsed to two
Corrupt: "Write security tests, implement mitigations, QA controls, document findings." → ["SECURITY", "DOCUMENT"]
Correct: ["SECURITY", "IMPLEMENT", "TEST", "QA", "DOCUMENT"]
  Fix: All five predicates fire. No collapse permitted.

## H012-005 — Held-out: "open redirect" fires SECURITY
Request: "Audit the PasswordResetService for open redirect vulnerabilities."
  Correct: ["SECURITY"]
  Evidence: "audit" → SECURITY; "open redirect vulnerabilities" → SECURITY
