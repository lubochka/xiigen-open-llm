# Layer 0-C — Knowledge Edge Pairs (011)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## E011-001 — "AES" and "RSA" fire SECURITY as cryptographic algorithm references

A: "Upgrade the DataVaultService to use AES-256 (Advanced Encryption Standard, 256-bit) instead of DES (Data Encryption Standard)."
  "upgrade" → IMPLEMENT (borderline); "AES-256" → SECURITY; "DES" → SECURITY (cryptographic algorithms)
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT"]
  Teaching: Cryptographic algorithm names (AES, RSA, DES, SHA, HMAC) fire SECURITY in implementation/audit contexts.

---

## E011-002 — "authorization header" fires API_CONTRACT; not AUTHORIZATION predicate

A: "Audit the OrdersController to confirm all endpoints require an Authorization header."
  "audit" → SECURITY; "Authorization header" → API_CONTRACT (HTTP header specification); "endpoints" → API_CONTRACT
  CompiledPredicateIds: ["SECURITY", "API_CONTRACT"]
  Teaching: HTTP "Authorization header" fires API_CONTRACT (it's a protocol specification). The AUTHORIZATION predicate fires on role/permission/access control vocabulary, not HTTP header names.

---

## E011-003 — "information disclosure" fires SECURITY

A: "Scan the ErrorHandlingMiddleware for information disclosure vulnerabilities — stack traces in 500 responses."
  "scan" → SECURITY; "information disclosure vulnerabilities" → SECURITY; "stack traces" → domain gap
  CompiledPredicateIds: ["SECURITY"]
  Teaching: "information disclosure" fires SECURITY as an OWASP vulnerability class.

---

## E011-004 — "timing attack" fires SECURITY

A: "Implement constant-time comparison for the AuthService token validation to prevent timing attacks."
  "implement" → IMPLEMENT; "constant-time comparison" → SECURITY; "timing attacks" → SECURITY
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT"]
  Teaching: "timing attack" fires SECURITY as a cryptographic attack vector noun.

---

## E011-005 — "password hash" fires SECURITY; "bcrypt/argon2" fire SECURITY

A: "Audit the UserService to verify passwords are hashed with bcrypt (cost factor ≥12) and never stored in SHA-1 (Secure Hash Algorithm 1)."
  "audit" → SECURITY; "passwords hashed with bcrypt" → SECURITY; "SHA-1" → SECURITY (deprecated algorithm warning)
  CompiledPredicateIds: ["SECURITY"]
  Teaching: Password hashing algorithm names (bcrypt, argon2, SHA-1, MD5) fire SECURITY when in credential management context.
