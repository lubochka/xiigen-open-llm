# Layer 0-A — Compiler Pairs (008)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: compiler

---

## C008-001 — SECURITY via "open redirect" vulnerability

A: "Audit the CheckoutService for open redirect vulnerabilities that could enable phishing attacks."
  "audit" → SECURITY; "open redirect" → SECURITY; "vulnerabilities" → SECURITY; "phishing" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C008-002 — SECURITY + IMPLEMENT via "enforce HTTPS"

A: "Enforce HTTPS (HyperText Transfer Protocol Secure) on all external endpoints of the CustomerPortal."
  "enforce" → SECURITY; "HTTPS" → SECURITY (transport security); "endpoints" → API_CONTRACT; "implement" implicit in enforce
  CompiledPredicateIds: ["SECURITY", "API_CONTRACT"] | SubPhase: 6.3

---

## C008-003 — SECURITY + QA + ARCHITECTURE via "security design review"

A: "QA the security design of the new EventBus architecture — audit message signing and replay protection."
  "QA" → QA; "security design" → SECURITY+ARCHITECTURE; "audit" → SECURITY; "message signing" → SECURITY; "replay protection" → SECURITY
  CompiledPredicateIds: ["QA", "SECURITY", "ARCHITECTURE"] | SubPhase: 6.3/3.1

---

## C008-004 — SECURITY via "insecure deserialization" check

A: "Scan the OrderImportService for insecure deserialization vulnerabilities in the JSON payload processor."
  "scan" → SECURITY; "insecure deserialization" → SECURITY; "vulnerabilities" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C008-005 — SECURITY + TEST via "security regression test"

A: "Write security regression tests to ensure previously patched XSS vulnerabilities do not re-emerge."
  "write" → IMPLEMENT; "security regression tests" → TEST via "tests"; "XSS" → SECURITY; "patched vulnerabilities" → SECURITY
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT", "TEST"] | SubPhase: 5.2
