# Held-Out and Corruption Pairs (003)
# Batch: B26-batch-001 | Scenario: B26 — Security audit
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## H003-001 — Held-out: "QA" fires QA regardless of position
Request: "Before the release, QA the security configuration of the InvoiceService."
  Correct: ["QA", "SECURITY"]
  Evidence: "QA" abbreviation fires; "security configuration" → SECURITY

## H003-002 — Corruption v6-01: "quality assurance" fires QA (invention)
Corrupt: "Perform quality assurance on the security controls." → ["QA", "SECURITY"]
Correct: ["SECURITY"]
  Fix: "quality assurance" spelled out is gap for QA. Only "QA" abbreviation fires QA.

## H003-003 — Held-out: "vulnerability" standalone fires SECURITY
Request: "There is a vulnerability in the JWT validation of the AuthService."
  Correct: ["SECURITY"]
  Evidence: "vulnerability" as standalone noun fires SECURITY

## H003-004 — Corruption v6-03: SECURITY+QA collapsed to SECURITY only
Corrupt: "Audit the PaymentService and QA before release." → ["SECURITY"]
Correct: ["SECURITY", "QA"]
  Fix: "QA" abbreviation fires QA independently. Collapse is wrong.

## H003-005 — Held-out: "secure" adjective is gap
Request: "Make the CheckoutService more secure for production."
  Correct: []
  GapHandler: SCHEDULE_CAPABILITY_LEARNING, domainId: "software-development"
  Teaching: "more secure" adjective is gap. "secure" without attack/enforcement vocabulary → gap.
