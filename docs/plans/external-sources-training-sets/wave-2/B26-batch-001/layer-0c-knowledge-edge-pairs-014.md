# Layer 0-C — Knowledge Edge Pairs (014)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E014-001 — "GDPR" alone is gap; "encrypt PII per GDPR" fires SECURITY via "encrypt"

A-gap: "Ensure GDPR compliance for all data processing activities."
  CompiledPredicateIds: [] — "GDPR", "ensure", "compliance" are all gap.

A-with-trigger: "Encrypt all PII fields per GDPR requirements."
  "Encrypt" → SECURITY (encrypt trigger); "PII" → context; "GDPR requirements" → gap label but co-occurrence with "encrypt" provides SECURITY.
  CompiledPredicateIds: ["SECURITY"] — SECURITY fires on "encrypt", not on "GDPR".
  Teaching: GDPR is always gap. The technical action ("encrypt") is what fires SECURITY.

---

## E014-002 — "data retention policy" is gap; "encrypt retained data" fires SECURITY

A-gap: "Define the data retention policy for user records."
  "define" → gap; "data retention" → gap; "policy" → gap
  CompiledPredicateIds: []

A-with-trigger: "Audit the data retention storage for encryption of retained PII."
  "audit" → SECURITY; "encryption" → SECURITY | CompiledPredicateIds: ["SECURITY"]

---

## E014-003 — "right to erasure" is gap vocabulary

A: "Implement the right to erasure flow for GDPR data deletion requests."
  "implement" → IMPLEMENT; "right to erasure" → gap; "GDPR" → gap; "data deletion" → gap
  CompiledPredicateIds: ["IMPLEMENT"] — only IMPLEMENT fires; SECURITY does not fire on data deletion vocabulary.
  Teaching: GDPR data rights vocabulary (right to erasure, data portability, consent withdrawal) is gap for SECURITY.

---

## E014-004 — "data breach" fires SECURITY via vulnerability context

A: "Assess the UserDataService for data breach vulnerability — could an attacker exfiltrate PII via the export endpoint?"
  "assess" + "vulnerability" → SECURITY; "data breach" → SECURITY (breach/vulnerability); "exfiltrate" → SECURITY
  CompiledPredicateIds: ["SECURITY"]
  Teaching: "data breach" and "exfiltrate" are security-action vocabulary that fire SECURITY.

---

## E014-005 — "consent management" alone is gap; "audit consent token" fires SECURITY

A-gap: "Review the consent management UI for accessibility improvements."
  CompiledPredicateIds: [] — "consent management", "accessibility" are gap.

A-with-trigger: "Audit the consent token storage for encryption and session binding."
  "audit" → SECURITY; "token storage" → SECURITY (token trigger); "encryption" → SECURITY; "session binding" → SECURITY (session trigger)
  CompiledPredicateIds: ["SECURITY"]
