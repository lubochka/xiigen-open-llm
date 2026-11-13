# Layer 0-C — Knowledge Edge Pairs (009)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## E009-001 — "TLS" abbreviation fires SECURITY in enforcement context

A: "Enforce TLS (Transport Layer Security) 1.3 on all microservice-to-microservice communication."
  "enforce" → SECURITY; "TLS" → SECURITY (transport security protocol)
  CompiledPredicateIds: ["SECURITY"]
  Teaching: "TLS" in enforcement context fires SECURITY. "TLS" as a purely informational mention (no action verb) is borderline.

---

## E009-002 — "data exfiltration" fires SECURITY

A: "Audit the DataExportService for data exfiltration risks — ensure only authorized fields are included in exports."
  "audit" → SECURITY; "data exfiltration" → SECURITY; "ensure" → gap; "authorized fields" → AUTHORIZATION
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"]
  Teaching: "data exfiltration" fires SECURITY as an attack vector noun.

---

## E009-003 — "input validation" fires SECURITY in enforcement context

A: "Implement strict input validation on all external API endpoints."
  "implement" → IMPLEMENT; "input validation" → SECURITY; "external API endpoints" → API_CONTRACT
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT", "API_CONTRACT"]
  Teaching: "input validation" fires SECURITY in enforcement/implementation context. It is security-action vocabulary.

---

## E009-004 — "certificate pinning" fires SECURITY

A: "Implement certificate pinning in the MobileApp SDK (Software Development Kit) to prevent MITM (Man-in-the-Middle) attacks."
  "implement" → IMPLEMENT; "certificate pinning" → SECURITY; "MITM attacks" → SECURITY
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT"]
  Teaching: "certificate pinning" and "MITM" both fire SECURITY. Both are security-action/attack vocabulary.

---

## E009-005 — "key rotation" fires SECURITY in cryptographic context

A: "Implement automatic key rotation for the DataEncryptionService every 30 days."
  "implement" → IMPLEMENT; "key rotation" → SECURITY (cryptographic key management); "30 days" → domain gap
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT"]
  Teaching: "key rotation" fires SECURITY as cryptographic security-action vocabulary.
