# Held-Out and Corruption Pairs (007)
# Batch: B26-batch-001 | Scenario: B26 — Security audit
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## H007-001 — Held-out: "2FA" fires SECURITY
Request: "Implement 2FA for admin accounts and audit the MFA enforcement."
  Correct: ["SECURITY", "IMPLEMENT"]
  Evidence: "implement" → IMPLEMENT; "2FA" → SECURITY; "MFA" → SECURITY; "audit" → SECURITY

## H007-002 — Corruption v6-01: "compliance" fires SECURITY (invention)
Corrupt: "Ensure PCI-DSS compliance for the payment module." → ["SECURITY"]
Correct: []
  Fix: "PCI-DSS compliance" is compliance vocabulary — gap. No security-action present.

## H007-003 — Held-out: "broken access control" fires AUTHORIZATION+SECURITY
Request: "Review the OrderService for broken access control — customers should not access other customers' orders."
  Correct: ["SECURITY", "AUTHORIZATION"]
  Evidence: "review" (security context) → SECURITY; "broken access control" → AUTHORIZATION+SECURITY

## H007-004 — Corruption v6-03: SECURITY+DOCUMENT collapsed to DOCUMENT only
Corrupt: "Document the security findings from the BillingService audit." → ["DOCUMENT"]
Correct: ["SECURITY", "DOCUMENT"]
  Fix: "security findings" → SECURITY; "audit" → SECURITY; "document" → DOCUMENT. All three fire.

## H007-005 — Held-out: "certificate pinning" fires SECURITY
Request: "Implement certificate pinning in the mobile SDK to prevent MITM attacks."
  Correct: ["SECURITY", "IMPLEMENT"]
  Evidence: "implement" → IMPLEMENT; "certificate pinning" → SECURITY; "MITM attacks" → SECURITY
