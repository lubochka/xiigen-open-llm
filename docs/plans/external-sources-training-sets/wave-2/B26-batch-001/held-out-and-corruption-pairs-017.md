# Held-Out and Corruption Pairs (017)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## H017-001 — Held-out: "SSRF" fires SECURITY
Request: "Audit the PaymentCallbackService for SSRF vulnerabilities — can the callback URL be pointed to an internal IP?"
  Correct: ["SECURITY"]
  Evidence: "audit" → SECURITY; "SSRF" → SECURITY (attack vector); "internal IP" → SECURITY context

## H017-002 — Corruption v6-01: "internal IP" alone fires SECURITY (overfire)
Corrupt: "Configure the service to connect to the internal IP 10.0.0.5." → ["SECURITY"]
Correct: []
  Fix: "internal IP" in a configuration context is gap. SECURITY fires on "SSRF" or "attack/vulnerability" co-occurrence, not on "internal IP" as a network address.

## H017-003 — Held-out: "path traversal" fires SECURITY
Request: "Audit the FileDownloadService for path traversal vulnerabilities — can %2e%2e%2f sequences escape the download directory?"
  Correct: ["SECURITY"]
  Evidence: "audit" → SECURITY; "path traversal vulnerabilities" → SECURITY; "directory" escape context → SECURITY

## H017-004 — Corruption v6-03: SSRF and XXE collapsed to single routing decision
Corrupt: "Audit the OrderService for SSRF and XXE." → SECURITY → single decision at 6.3 → PASS
Correct: SECURITY → 6.3 | Result may be FAIL (if vulnerabilities found). PASS/FAIL is determined by audit findings, not by routing.
  Fix: Routing and audit verdict are independent. The arbiter determines FAIL, not the router.

## H017-005 — Held-out: "deserialization vulnerability" fires SECURITY
Request: "Audit the SettingsService for insecure deserialization — can a crafted JSON payload trigger remote code execution?"
  Correct: ["SECURITY"]
  Evidence: "audit" → SECURITY; "insecure deserialization" → SECURITY; "remote code execution" → SECURITY
