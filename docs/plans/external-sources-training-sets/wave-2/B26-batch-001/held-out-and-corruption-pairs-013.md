# Held-Out and Corruption Pairs (013)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## H013-001 — Held-out: "authentication bypass" fires SECURITY
Request: "Audit the UserService for authentication bypass vulnerabilities in the password reset flow."
  Correct: ["SECURITY"]
  Evidence: "audit" → SECURITY; "authentication bypass" → SECURITY; "vulnerabilities" → SECURITY

## H013-002 — Corruption v6-01: "GDPR" fires SECURITY (invention)
Corrupt: "Ensure GDPR compliance for all user data." → ["SECURITY"]
Correct: []
  Fix: "GDPR" is gap vocabulary. "ensure" is gap. Zero predicates → knowledge_edge.
  v6-01 corruption: incorrectly fires SECURITY on regulatory framework label.

## H013-003 — Held-out: SECURITY+QA fires when both present
Request: "Audit the API authentication layer for bypass vulnerabilities and QA the token validation controls."
  Correct: ["SECURITY", "QA"]
  Evidence: "audit" + "authentication" + "vulnerabilities" → SECURITY; "QA" → QA

## H013-004 — Corruption v6-03: SECURITY routed to 5.2 in audit context
Corrupt: "Audit the REST API for authentication bypass." → SECURITY → 5.2
Correct: SECURITY → 6.3 (audit verb + security vocabulary → SecurityComplianceReviewFlow63)
  Fix: Audit/review context → 6.3. Test-writing context → 5.2. "audit" is the 6.3 indicator.

## H013-005 — Held-out: "certificate pinning" fires SECURITY
Request: "Review the mobile app for certificate pinning gaps — can an attacker intercept TLS traffic?"
  Correct: ["SECURITY"]
  Evidence: "review" (security context) → SECURITY; "certificate pinning" → SECURITY (certificate trigger); "TLS traffic" → SECURITY
