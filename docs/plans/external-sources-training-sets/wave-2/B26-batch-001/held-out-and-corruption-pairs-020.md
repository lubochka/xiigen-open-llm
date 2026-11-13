# Held-Out and Corruption Pairs (020)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## H020-001 — Held-out: "TLS termination" fires SECURITY
Request: "Audit the load balancer TLS termination — confirm TLS 1.3 is enforced and older versions are rejected."
  Correct: ["SECURITY"]
  Evidence: "audit" → SECURITY; "TLS termination" → SECURITY (tls trigger); "TLS 1.3" → SECURITY

## H020-002 — Corruption v6-01: "network latency review" fires SECURITY (invention)
Corrupt: "Review the network latency between the auth service and database." → ["SECURITY"]
Correct: []
  Fix: "network latency" is a performance term — gap. "review" alone without security co-occurrence → gap.

## H020-003 — Held-out: "mutual TLS" fires SECURITY
Request: "Review the inter-service communication — is mutual TLS enforced for all internal service-to-service calls?"
  Correct: ["SECURITY"]
  Evidence: "review" (security context) → SECURITY; "mutual TLS" → SECURITY; "certificate validation" → SECURITY

## H020-004 — Corruption v6-03: replay attack routing flipped to IMPLEMENT
Corrupt: "Audit the WebhookService for replay attack vulnerabilities." → IMPLEMENT → 4.1
Correct: SECURITY → 6.3 (audit + attack vulnerability → SecurityComplianceReviewFlow63)
  Fix: "audit" + "replay attack vulnerabilities" → SECURITY → 6.3. No IMPLEMENT predicate present.

## H020-005 — Held-out: "SSH open to 0.0.0.0/0" fires SECURITY (network security context)
Request: "Audit the firewall rules — is SSH port 22 restricted to VPN CIDR, or open to the internet?"
  Correct: ["SECURITY"]
  Evidence: "audit" → SECURITY; "firewall rules" → SECURITY (security context); "SSH" + "internet" exposure → SECURITY
