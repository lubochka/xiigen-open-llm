# Layer 0-A — Compiler Pairs (009)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: compiler

---

## B28-compiler-009-001 — QA: "QA review" fires QA

A: "Schedule a QA review for the SearchService before the next sprint release."
  "QA review" → QA | "schedule" → gap (alone)
  CompiledPredicateIds: ["QA"]

---

## B28-compiler-009-002 — QA+SECURITY+TEST: security QA gate

A: "QA the security audit findings and write regression tests to confirm the SQL injection fix."
  "QA" → QA | "security audit findings" → SECURITY ("audit") | "write regression tests" → TEST | "confirm" → gap alone
  CompiledPredicateIds: ["QA", "SECURITY", "TEST"]

---

## B28-compiler-009-003 — gap: observability vocabulary

A: "Set up alerting for 5xx error rates above 1% in the production environment."
  "set up alerting" → gap | "5xx error rates" → gap (observability) | "production environment" → gap
  CompiledPredicateIds: [] → gap

---

## B28-compiler-009-004 — QA+IMPLEMENT: build and QA

A: "Build the QA automation framework and run QA on the RefundService release."
  "build" → IMPLEMENT | "QA automation framework" → QA + IMPLEMENT | "run QA" → QA
  CompiledPredicateIds: ["QA", "IMPLEMENT"]

---

## B28-compiler-009-005 — QA: "QA" compound with "gate", "block", "approval", "criteria"

A: "Define the QA gate criteria for the production promotion checklist."
  "QA gate criteria" → QA | "define" → ARCHITECTURE? → QA-criteria definition ≠ structural design → just QA
  CompiledPredicateIds: ["QA"]
  Teaching: "QA gate", "QA block", "QA approval", "QA criteria" — all compound nouns containing QA fire QA predicate.

