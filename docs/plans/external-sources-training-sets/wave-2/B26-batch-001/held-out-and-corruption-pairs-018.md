# Held-Out and Corruption Pairs (018)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## H018-001 — Held-out: "penetration test" fires SECURITY
Request: "Review the penetration test report for the AccountService — focus on credential theft findings."
  Correct: ["SECURITY"]
  Evidence: "review" (security context) → SECURITY; "penetration test" → SECURITY (penetration trigger); "credential theft" → SECURITY (credential trigger)

## H018-002 — Corruption v6-01: "schedule pen test" fires SECURITY (invention)
Corrupt: "Schedule the penetration test for next quarter." → ["SECURITY"]
Correct: []
  Fix: Scheduling a future test is a logistics action — gap. SECURITY fires on reviewing/auditing pentest findings, not on scheduling the event.

## H018-003 — Held-out: "red team" fires SECURITY
Request: "Review the red team findings for the AuthService — what credential exfiltration paths were discovered?"
  Correct: ["SECURITY"]
  Evidence: "review" (security context) → SECURITY; "red team" → SECURITY; "credential exfiltration" → SECURITY

## H018-004 — Corruption v6-01: "brute-force" routes to IMPLEMENT (wrong predicate)
Corrupt: "Audit the LoginService for brute-force resistance." → IMPLEMENT → 4.1
Correct: SECURITY → 6.3 (audit + brute-force vulnerability vocabulary → 6.3, not 4.1)
  Fix: "brute-force" fires SECURITY, not IMPLEMENT. "audit" confirms 6.3 context. IMPLEMENT is not present.

## H018-005 — Held-out: "MFA bypass" fires SECURITY
Request: "Audit the AdminConsole for MFA bypass paths — can an attacker skip TOTP verification?"
  Correct: ["SECURITY"]
  Evidence: "audit" → SECURITY; "MFA bypass" → SECURITY (mfa trigger); "TOTP" → SECURITY (authentication mechanism)
