# Held-Out and Corruption Pairs (006)
# Batch: B26-batch-001 | Scenario: B26 — Security audit
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## H006-001 — Held-out: "threat model" fires SECURITY without "audit" verb
Request: "Document the threat model for the new EventBus including attack surfaces."
  Correct: ["SECURITY", "DOCUMENT"]
  Evidence: "document" → DOCUMENT; "threat model" → SECURITY

## H006-002 — Corruption v6-01: "zero-trust" fires ARCHITECTURE only (drops SECURITY)
Corrupt: "Design a zero-trust network architecture." → ["ARCHITECTURE"]
Correct: ["SECURITY", "ARCHITECTURE"]
  Fix: "zero-trust" fires SECURITY. Both predicates fire.

## H006-003 — Held-out: "API key rotation" fires SECURITY+API_CONTRACT
Request: "Audit all API key rotation policies on the ExternalIntegrationService."
  Correct: ["SECURITY", "API_CONTRACT"]
  Evidence: "audit" → SECURITY; "API key" → SECURITY (credential) + API_CONTRACT (API governance)

## H006-004 — Corruption v6-02: QA suppressed when SECURITY fires
Corrupt: "Audit the security controls and QA before the release." → ["SECURITY"]
Correct: ["SECURITY", "QA"]
  Fix: "QA" abbreviation fires QA regardless of other predicates present. No suppression.

## H006-005 — Held-out: "encrypt" fires SECURITY in all contexts
Request: "Encrypt the PII fields in the UserProfile table before persisting to the database."
  Correct: ["SECURITY"]
  Evidence: "encrypt" → SECURITY (security-action verb); "PII" reinforces context
