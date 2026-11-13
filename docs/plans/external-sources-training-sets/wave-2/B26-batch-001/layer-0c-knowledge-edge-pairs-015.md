# Layer 0-C — Knowledge Edge Pairs (015)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E015-001 — "PCI-DSS" alone is gap; "tokenize PAN" fires SECURITY via "token"

A-gap: "Achieve PCI-DSS Level 1 certification."
  CompiledPredicateIds: [] — "PCI-DSS", "certification", "achieve" all gap.

A-with-trigger: "Audit the payment card tokenization flow — confirm PAN is replaced by token before persistence."
  "audit" → SECURITY; "tokenization" → SECURITY (token trigger); "PAN" → context
  CompiledPredicateIds: ["SECURITY"]

---

## E015-002 — "card data environment" is gap but "encrypt card data" fires SECURITY

A-gap: "Define the card data environment boundaries for the PCI-DSS assessment."
  CompiledPredicateIds: [] — "define", "card data environment", "PCI-DSS", "assessment" all gap in isolation.

A-with-trigger: "Audit the card data environment for unencrypted PAN in transit."
  "audit" → SECURITY; "unencrypted PAN" → SECURITY (encrypt trigger — negated form still fires); "in transit" → context
  CompiledPredicateIds: ["SECURITY"]

---

## E015-003 — "CVV" alone is gap; "audit CVV storage" fires SECURITY

A-gap: "Display the last 4 digits and CVV in the card management UI."
  CompiledPredicateIds: [] — UI display request; "CVV" alone is gap.

A-with-trigger: "Audit the database schema for CVV retention — PCI-DSS prohibits storing CVV after authorization."
  "audit" → SECURITY; "CVV retention" → SECURITY (credential/hash context); "prohibits storing" → SECURITY context
  CompiledPredicateIds: ["SECURITY"]

---

## E015-004 — "cipher suite" fires SECURITY; "cipher" is an explicit trigger

A: "Review the cipher suite configuration on all HTTPS (Hypertext Transfer Protocol Secure) load balancers — reject RC4 (Rivest Cipher 4) and DES (Data Encryption Standard)."
  "review" (security context) → SECURITY; "cipher suite" → SECURITY (cipher trigger); "HTTPS" → SECURITY (tls context); "RC4", "DES" → SECURITY (cipher algorithm names)
  CompiledPredicateIds: ["SECURITY"] — strong signal via multiple cipher/tls triggers.

---

## E015-005 — "hash" fires SECURITY; bcrypt/SHA are explicit triggers

A: "Assess the password storage — is bcrypt used with a cost factor ≥ 12, or is MD5 still in use?"
  "assess" + security vocabulary → SECURITY; "password storage" → SECURITY; "bcrypt" → SECURITY (hash trigger); "MD5" → SECURITY (hash algorithm)
  CompiledPredicateIds: ["SECURITY"]
