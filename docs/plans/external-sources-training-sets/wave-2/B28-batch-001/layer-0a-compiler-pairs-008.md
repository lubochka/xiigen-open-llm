# Layer 0-A — Compiler Pairs (008)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: compiler

---

## B28-compiler-008-001 — QA: "QA signoff" (no space variant) fires QA

A: "Get QA signoff on the PaymentService before the Friday release."
  "QA signoff" → QA (same as "QA sign-off") | "Friday release" → gap
  CompiledPredicateIds: ["QA"]

---

## B28-compiler-008-002 — QA+TEST+DOCUMENT: full release package

A: "Write integration tests, get QA sign-off, and document the release notes for v5.0."
  "write integration tests" → TEST | "QA sign-off" → QA | "document the release notes" → DOCUMENT
  CompiledPredicateIds: ["QA", "TEST", "DOCUMENT"]

---

## B28-compiler-008-003 — gap: "provision the staging environment"

A: "Provision the staging environment for the upcoming release."
  "provision" → gap | "staging environment" → gap | "upcoming release" → gap
  CompiledPredicateIds: [] → gap

---

## B28-compiler-008-004 — QA+IMPLEMENT: implement QA gate

A: "Implement the automated QA gate in the release pipeline."
  "implement" → IMPLEMENT | "automated QA gate" → QA + IMPLEMENT | "release pipeline" → gap
  CompiledPredicateIds: ["QA", "IMPLEMENT"]

---

## B28-compiler-008-005 — QA: "QA" in action verb context

A: "QA this before it ships."
  "QA this" → QA (QA as imperative verb) | "ships" → gap (deployment vocabulary)
  CompiledPredicateIds: ["QA"]
  Teaching: "QA" as an imperative verb ("QA this", "QA the feature") fires QA predicate.

