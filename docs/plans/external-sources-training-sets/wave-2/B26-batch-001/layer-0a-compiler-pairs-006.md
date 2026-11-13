# Layer 0-A — Compiler Pairs (006)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: compiler

---

## C006-001 — SECURITY via JWT (JSON Web Token) signature verification

A: "Audit the AuthService to confirm JWT tokens are validated with the correct signing algorithm and key length."
  "audit" → SECURITY; "JWT tokens validated" → SECURITY; "signing algorithm" → SECURITY; "key length" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C006-002 — SECURITY + ARCHITECTURE via "security architecture review"

A: "Review the security architecture of the microservices mesh — identify trust boundary violations."
  "review" (security context) → SECURITY; "security architecture" → SECURITY+ARCHITECTURE; "trust boundary" → SECURITY; "identify" → gap
  CompiledPredicateIds: ["SECURITY", "ARCHITECTURE"] | SubPhase: 6.3/3.1

---

## C006-003 — SECURITY + QA + TEST via pre-release security QA

A: "QA the ShippingService before release: assert no hardcoded credentials, verify TLS (Transport Layer Security) is enforced."
  "QA" → QA; "assert" → TEST; "hardcoded credentials" → SECURITY; "verify" → TEST; "TLS enforced" → SECURITY
  CompiledPredicateIds: ["QA", "TEST", "SECURITY"] | SubPhase: 6.3

---

## C006-004 — SECURITY + IMPLEMENT via "implement rate limiting"

A: "Implement rate limiting on the LoginService to prevent brute-force attacks."
  "implement" → IMPLEMENT; "rate limiting" → SECURITY (denial-of-service prevention); "brute-force attacks" → SECURITY
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT"] | SubPhase: 5.2/4.1
  Note: Implementation context → SECURITY routes to 5.2 (not audit context 6.3)

---

## C006-005 — SECURITY via "dependency vulnerability scan"

A: "Scan the project dependencies for known CVE (Common Vulnerabilities and Exposures) vulnerabilities."
  "scan" → SECURITY; "CVE vulnerabilities" → SECURITY; "dependencies" → gap
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3
