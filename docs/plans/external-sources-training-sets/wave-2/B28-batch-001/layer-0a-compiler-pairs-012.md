# Layer 0-A — Compiler Pairs (012)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: compiler

---

## B28-compiler-012-001 — QA: "QA owner" fires QA

A: "Assign a QA owner to the ProductService release and ensure sign-off before merge."
  "QA owner" → QA | "ensure" → gap alone | "sign-off" → QA (in QA context)
  CompiledPredicateIds: ["QA"]

---

## B28-compiler-012-002 — QA+TEST: E2E (End-to-End) tests + QA gate

A: "Write E2E (End-to-End) tests for the checkout flow and get QA sign-off before go-live."
  "write E2E tests" → TEST | "checkout flow" → domain gap | "QA sign-off" → QA
  CompiledPredicateIds: ["QA", "TEST"]

---

## B28-compiler-012-003 — gap: "SLO (Service Level Objective) monitoring"

A: "Set up SLO monitoring for the PaymentService in production."
  "set up" → gap | "SLO monitoring" → gap (observability) | "production" → gap
  CompiledPredicateIds: [] → gap

---

## B28-compiler-012-004 — QA+IMPLEMENT+TEST+DOCUMENT+SECURITY: full secure release

A: "Implement regression tests, write the security audit report, get QA sign-off, and document the release."
  "implement regression tests" → IMPLEMENT + TEST | "security audit report" → SECURITY + DOCUMENT | "QA sign-off" → QA | "document the release" → DOCUMENT
  CompiledPredicateIds: ["QA", "IMPLEMENT", "TEST", "SECURITY", "DOCUMENT"]

---

## B28-compiler-012-005 — QA: deduplication — multiple QA triggers → one QA predicate

A: "QA sign-off required: QA lead must approve and QA testing must pass before the v6 release."
  Multiple QA triggers → still ["QA"] (set deduplication)
  CompiledPredicateIds: ["QA"]
  Teaching: Multiple QA triggers in one request → one QA predicate (set, not multiset).

