# Layer 0-C — Knowledge Edge Pairs (012)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## E012-001 — "race condition" is domain gap; fires SECURITY only when paired with security context

A: "Fix the race condition in the InventoryService stock deduction."
  "fix" → gap; "race condition" → gap (concurrency domain term, not security-action)
  CompiledPredicateIds: [] → knowledge_edge
B: "Audit the PaymentService for time-of-check/time-of-use (TOCTOU) race condition vulnerabilities."
  "audit" → SECURITY; "TOCTOU race condition vulnerabilities" → SECURITY
  CompiledPredicateIds: ["SECURITY"]
  Teaching: "race condition" alone is domain gap. "TOCTOU vulnerability" fires SECURITY.

---

## E012-002 — "HTTPS" fires SECURITY in enforcement context; gap in informational context

A: "Enforce HTTPS on all external endpoints." → ["SECURITY"]
  "enforce" → SECURITY; "HTTPS" → SECURITY
B: "The service uses HTTPS for all external calls." → []
  Informational statement: "uses" → gap; "HTTPS" without enforcement/audit verb → gap
  Teaching: "HTTPS" fires SECURITY only when paired with enforcement/audit vocabulary. Informational mentions are gap.

---

## E012-003 — "two-factor authentication" fires SECURITY

A: "Implement two-factor authentication (2FA) for the AdminService login flow."
  "implement" → IMPLEMENT; "two-factor authentication" → SECURITY; "2FA" → SECURITY (abbreviation)
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT"]
  Teaching: "two-factor authentication" and "2FA" fire SECURITY as authentication security vocabulary.

---

## E012-004 — "nonce" fires SECURITY in cryptographic context

A: "Verify the CSRF protection middleware generates a unique nonce for each request."
  "verify" → TEST; "CSRF protection" → SECURITY; "nonce" → SECURITY (cryptographic nonce)
  CompiledPredicateIds: ["TEST", "SECURITY"]
  Teaching: "nonce" fires SECURITY as a cryptographic protection term.

---

## E012-005 — "QA" in "QA lead" fires QA; role titles with "QA" fire the predicate

A: "Have the QA lead review the security test coverage before sign-off."
  "QA lead" → QA (abbreviation fires); "review" → gap (no security co-occurrence here); "security test coverage" → SECURITY+TEST via "test"
  CompiledPredicateIds: ["QA", "SECURITY", "TEST"]
  Teaching: "QA" in role titles fires QA predicate. TEST fires via "test coverage".
