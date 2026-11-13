# Held-Out and Corruption Pairs (010)
# Batch: B26-batch-001 | Scenario: B26 — Security audit
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## H010-001 — Held-out: "SSRF" fires SECURITY
Request: "Audit the WebhookService for SSRF vulnerabilities in URL handling."
  Correct: ["SECURITY"]
  Evidence: "audit" → SECURITY; "SSRF vulnerabilities" → SECURITY

## H010-002 — Corruption v6-01: "risk" fires SECURITY (invention)
Corrupt: "Assess the security risks for the DataIngestionPipeline." → ["SECURITY"]
Correct: []
  Fix: "security risks" — "security" alone without attack vocabulary is insufficient; "risk assessment" is gap.
  Strict ruling: "Assess security risks" → [] (assess is gap; risk vocabulary is gap)

## H010-003 — Held-out: "data exfiltration" fires SECURITY
Request: "Audit the DataExportService for data exfiltration risks — only authorized fields should be exported."
  Correct: ["SECURITY", "AUTHORIZATION"]
  Evidence: "audit" → SECURITY; "data exfiltration" → SECURITY; "authorized fields" → AUTHORIZATION

## H010-004 — Corruption v6-03: SECURITY+DOCUMENT collapsed to SECURITY
Corrupt: "Audit the AdminService and document the privilege escalation findings." → ["SECURITY"]
Correct: ["SECURITY", "AUTHORIZATION", "DOCUMENT"]
  Fix: "privilege escalation" → AUTHORIZATION; "document" → DOCUMENT. Three predicates.

## H010-005 — Held-out: "key length" fires SECURITY
Request: "Audit the RSA key length used in the AuthService — confirm minimum 2048-bit."
  Correct: ["SECURITY"]
  Evidence: "audit" → SECURITY; "RSA key length" → SECURITY (cryptographic key management)
