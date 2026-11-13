# Held-Out and Corruption Pairs (014)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## H014-001 — Held-out: "encrypt PII" fires SECURITY even with GDPR label
Request: "Audit the UserProfile table for PII fields stored without AES-256 encryption — GDPR Article 32 non-compliant."
  Correct: ["SECURITY"]
  Evidence: "audit" → SECURITY; "AES-256 encryption" → SECURITY; "GDPR" is in the request but is gap — "AES-256" provides the SECURITY signal.

## H014-002 — Corruption v6-01: "GDPR Article 32" fires SECURITY (invention)
Corrupt: "Ensure GDPR Article 32 technical measures are in place." → ["SECURITY"]
Correct: []
  Fix: "GDPR Article 32" is a regulatory reference — gap. "ensure" is gap. Zero predicates.

## H014-003 — Held-out: "data breach vulnerability" fires SECURITY
Request: "Assess the DataExportService for data breach vulnerability — can an attacker exfiltrate user PII via the export endpoint?"
  Correct: ["SECURITY"]
  Evidence: "assess" + "data breach vulnerability" + "exfiltrate" → SECURITY

## H014-004 — Corruption v6-03: DOCUMENT predicate collapsed
Corrupt: "Audit the PII encryption and document all unencrypted fields." → ["SECURITY"]
Correct: ["SECURITY", "DOCUMENT"]
  Fix: "document" fires DOCUMENT predicate independently. Never collapse multi-predicate requests.

## H014-005 — Held-out: "TLS in transit" fires SECURITY; "data in transit" alone is gap
Request: "Audit the DataIngestionService — is all PII transmitted over TLS 1.3?"
  Correct: ["SECURITY"]
  Evidence: "audit" → SECURITY; "TLS 1.3" → SECURITY; "PII" → context (not a predicate trigger alone)
