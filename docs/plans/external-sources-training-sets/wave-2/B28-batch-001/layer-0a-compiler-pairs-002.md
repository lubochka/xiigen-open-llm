# Layer 0-A — Compiler Pairs (002)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate, sub-phase 7.1/7.3
# Production date: 2026-05-05 | pair_kind: compiler

---

## B28-compiler-002-001 — QA: release gate sign-off

A: "Get QA sign-off on the OrderService before promoting to production."
  "QA sign-off" → QA | "promoting to production" → gap (deployment vocabulary)
  CompiledPredicateIds: ["QA"]

---

## B28-compiler-002-002 — QA+TEST: automated QA suite

A: "Write the automated QA test suite and run QA gating before the v2.0 release."
  "write" → TEST (writing tests) | "automated QA test suite" → QA + TEST | "QA gating" → QA
  CompiledPredicateIds: ["QA", "TEST"]

---

## B28-compiler-002-003 — QA+IMPLEMENT: QA harness

A: "Build the QA testing harness for the payment pipeline and get QA approval before release."
  "build" → IMPLEMENT | "QA testing harness" → QA + IMPLEMENT | "QA approval" → QA
  CompiledPredicateIds: ["QA", "IMPLEMENT"]

---

## B28-compiler-002-004 — gap: "deploy and monitor"

A: "Deploy the new service and monitor the error rate for 24 hours."
  "deploy" → gap | "monitor" → gap | "error rate" → gap (observability vocabulary)
  CompiledPredicateIds: [] → gap
  Teaching: "deploy" and "monitor" are universally gap. Neither fires QA, IMPLEMENT, or any predicate.

---

## B28-compiler-002-005 — QA: "QA" abbreviation gate

A: "QA the release candidate before the sprint demo."
  "QA" → QA (abbreviation gate) | "release candidate" → gap | "sprint demo" → gap
  vs "Quality assurance the release candidate." → [] ("quality assurance" spelled out = gap)
  CompiledPredicateIds: ["QA"]
  Teaching: "QA" abbreviation fires QA. "quality assurance" spelled out → gap. Strict abbreviation gate.

