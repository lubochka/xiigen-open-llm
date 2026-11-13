# Layer 0-C — Knowledge Edge Pairs (023)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E023-001 — "AES" and "RSA" fire SECURITY; generic algorithm names are gap

A-fires: "Audit the encryption service — is AES-256 (Advanced Encryption Standard 256-bit) used for data at rest, and RSA-2048 (Rivest-Shamir-Adleman 2048-bit) for asymmetric key exchange?"
  "audit" → SECURITY; "AES-256" → SECURITY (cipher/encrypt trigger); "RSA-2048" → SECURITY
  CompiledPredicateIds: ["SECURITY"]

A-gap: "The algorithm runs in O(n log n) time for average inputs."
  "algorithm" → gap (computational complexity, not cryptography)
  CompiledPredicateIds: []

---

## E023-002 — "HMAC" fires SECURITY; "MAC address" is gap

A-fires: "Review the API authentication — is each request signed with HMAC (Hash-based Message Authentication Code) using a per-session secret?"
  "review" (security context) → SECURITY; "HMAC" → SECURITY; "per-session secret" → SECURITY (session/secret triggers)
  CompiledPredicateIds: ["SECURITY"]

A-gap: "The MAC (Media Access Control) address is used to identify devices on the local network."
  "MAC address" → gap (networking hardware identifier, not cryptography)
  CompiledPredicateIds: []

---

## E023-003 — "CBC mode" fires SECURITY as a cipher vulnerability; "block cipher" alone is borderline

A-fires: "Audit the EncryptionService for CBC (Cipher Block Chaining) mode padding oracle vulnerabilities — is IV (Initialization Vector) randomness verified?"
  "audit" → SECURITY; "CBC mode" → SECURITY (cipher trigger); "padding oracle" → SECURITY; "IV randomness" → SECURITY
  CompiledPredicateIds: ["SECURITY"]

---

## E023-004 — "encryption at rest" fires SECURITY; "database field" alone is gap

A-fires: "Audit the database schema — are all PII fields encrypted at rest, or are any stored in plaintext?"
  "audit" → SECURITY; "encrypted at rest" → SECURITY (encrypt trigger); "PII" → context; "plaintext" → SECURITY (unencrypted = security concern)
  CompiledPredicateIds: ["SECURITY"]

A-gap: "Add a new database field to store the user's preferred language."
  "database field" → gap (schema change, not security audit)
  CompiledPredicateIds: []

---

## E023-005 — "end-to-end encryption" fires SECURITY; "end-to-end" alone is gap

A-fires: "Audit the messaging service — is end-to-end encryption enforced for all direct messages, and are encryption keys stored only on client devices?"
  "audit" → SECURITY; "end-to-end encryption" → SECURITY (encrypt trigger); "encryption keys" → SECURITY
  CompiledPredicateIds: ["SECURITY"]

A-gap: "The pipeline is end-to-end automated from commit to deployment."
  "end-to-end automated" → gap (CI/CD process description, not encryption)
  CompiledPredicateIds: []
