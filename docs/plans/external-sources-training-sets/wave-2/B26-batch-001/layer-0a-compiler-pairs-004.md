# Layer 0-A — Compiler Pairs (004)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: compiler

---

## C004-001 — SECURITY via "plaintext password" storage check

A: "Audit the UserService to confirm passwords are never stored in plaintext."
  "audit" → SECURITY; "passwords plaintext" → SECURITY (credential exposure); "confirm" is gap but SECURITY already fires
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C004-002 — SECURITY + IMPLEMENT via "patch vulnerability"

A: "Identify the path traversal vulnerability in the FileUploadService and implement a path sanitization fix."
  "identify" is gap; "path traversal vulnerability" → SECURITY; "implement" → IMPLEMENT; "path sanitization" → SECURITY
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT"] | SubPhase: 6.3/4.1

---

## C004-003 — SECURITY + QA via "security QA gate"

A: "QA sign off on the security hardening changes to the BillingService before the patch is deployed."
  "QA" → QA; "security hardening" → SECURITY; "sign off" → gap; "deployed" → gap
  CompiledPredicateIds: ["QA", "SECURITY"] | SubPhase: 6.3

---

## C004-004 — SECURITY via "secret management" review

A: "Review the ShippingService configuration for hardcoded API keys and secrets."
  "review" (security co-occurrence) → SECURITY; "hardcoded API keys" → SECURITY (credential exposure); "secrets" → SECURITY; "API keys" → API_CONTRACT
  CompiledPredicateIds: ["SECURITY", "API_CONTRACT"] | SubPhase: 6.3

---

## C004-005 — Gap: "audit" as noun (upcoming event) does not fire SECURITY

A: "We have an external security audit scheduled for next month — prepare the documentation."
  "audit" here is a noun (event description); "scheduled" → gap; "prepare" → gap; "documentation" → DOCUMENT if "document" verb, but "prepare" is gap
  CompiledPredicateIds: ["DOCUMENT"]
  Teaching: "audit" as a future event noun is gap for SECURITY. DOCUMENT fires on "documentation" if "prepare documentation" approximates "document". Borderline — "prepare" is gap; only "documentation" as a standalone noun is borderline DOCUMENT.
  Strict ruling: CompiledPredicateIds: [] (neither "prepare" nor "documentation" alone fires without an action verb)
