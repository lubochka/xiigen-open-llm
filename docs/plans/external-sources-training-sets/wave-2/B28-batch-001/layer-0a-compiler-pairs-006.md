# Layer 0-A — Compiler Pairs (006)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: compiler

---

## B28-compiler-006-001 — QA: acceptance testing gate

A: "Run QA acceptance testing on the CheckoutService before the production promotion."
  "QA acceptance testing" → QA | "run" → gap (alone)
  CompiledPredicateIds: ["QA"]

---

## B28-compiler-006-002 — QA+TEST+SECURITY: security QA

A: "Write penetration tests for the AuthService and get QA sign-off on the security release."
  "write penetration tests" → TEST + SECURITY ("penetration tests") | "QA sign-off" → QA
  CompiledPredicateIds: ["QA", "TEST", "SECURITY"]

---

## B28-compiler-006-003 — gap: "rollback if metrics degrade"

A: "Rollback the deployment if error rate exceeds 5% within 1 hour."
  "rollback" → gap | "error rate" → gap (observability) | "deployment" → gap
  CompiledPredicateIds: [] → gap

---

## B28-compiler-006-004 — QA+IMPLEMENT+TEST: full QA pipeline

A: "Build the end-to-end QA pipeline, write acceptance tests, and get QA approval before each release."
  "build" → IMPLEMENT | "QA pipeline" → QA + IMPLEMENT | "write acceptance tests" → TEST | "QA approval" → QA
  CompiledPredicateIds: ["QA", "IMPLEMENT", "TEST"]

---

## B28-compiler-006-005 — gap: "quality assurance" spelled out

A: "Perform quality assurance on the v2.1 release."
  "quality assurance" → gap (spelled-out form; strict abbreviation gate — only "QA" fires QA)
  CompiledPredicateIds: [] → gap
  Teaching: "quality assurance" spelled out does NOT fire QA. Only "QA" as an abbreviation fires QA predicate.

