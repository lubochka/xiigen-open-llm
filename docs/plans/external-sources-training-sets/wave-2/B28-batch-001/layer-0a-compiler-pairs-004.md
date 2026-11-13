# Layer 0-A — Compiler Pairs (004)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: compiler

---

## B28-compiler-004-001 — QA+ARCHITECTURE: QA infra design

A: "Design the QA infrastructure and get QA sign-off on the topology."
  "design" → ARCHITECTURE | "QA infrastructure" → QA + ARCHITECTURE | "QA sign-off" → QA
  CompiledPredicateIds: ["QA", "ARCHITECTURE"]

---

## B28-compiler-004-002 — QA: "QA lead" fires QA

A: "The QA lead needs to approve the PaymentService before production promotion."
  "QA lead" → QA (role title containing QA abbreviation fires QA) | "approve" → QA
  CompiledPredicateIds: ["QA"]

---

## B28-compiler-004-003 — QA+TEST: smoke testing gate

A: "Write the smoke test suite for the LoginService and QA sign-off before the release."
  "write" → TEST | "smoke test suite" → TEST | "QA sign-off" → QA
  CompiledPredicateIds: ["QA", "TEST"]

---

## B28-compiler-004-004 — gap: "push to production"

A: "Push the feature branch to production after the sprint review."
  "push to production" → gap | "sprint review" → gap
  CompiledPredicateIds: [] → gap

---

## B28-compiler-004-005 — QA+API_CONTRACT: QA the API

A: "QA the REST (Representational State Transfer) API contract and validate the OpenAPI schema before release."
  "QA" → QA | "REST API contract" → API_CONTRACT | "OpenAPI schema" → API_CONTRACT | "validate" → TEST
  CompiledPredicateIds: ["QA", "API_CONTRACT", "TEST"]

