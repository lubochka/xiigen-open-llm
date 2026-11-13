# Layer 0-A — Compiler Pairs (007)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: compiler

---

## B28-compiler-007-001 — QA+ARCHITECTURE: QA the infrastructure design

A: "Architect the QA testing infrastructure and get QA sign-off before production."
  "architect" → ARCHITECTURE | "QA testing infrastructure" → QA + ARCHITECTURE | "QA sign-off" → QA
  CompiledPredicateIds: ["QA", "ARCHITECTURE"]

---

## B28-compiler-007-002 — QA: "QA process" fires QA

A: "Define the QA process for the release pipeline."
  "QA process" → QA | "define" → ARCHITECTURE? → in QA process context: ARCHITECTURE not fired (process definition ≠ structural design)
  CompiledPredicateIds: ["QA"]

---

## B28-compiler-007-003 — QA+TEST: regression testing before release

A: "Run regression tests and QA sign-off before the hotfix goes live."
  "run regression tests" → TEST ("regression tests" fires TEST) | "QA sign-off" → QA
  CompiledPredicateIds: ["QA", "TEST"]

---

## B28-compiler-007-004 — gap: CI/CD vocabulary

A: "Configure the GitHub Actions workflow to trigger on pull request merge."
  "configure" → gap | "GitHub Actions workflow" → gap | "pull request merge" → gap
  CompiledPredicateIds: [] → gap

---

## B28-compiler-007-005 — QA: "QA checklist" fires QA

A: "Complete the QA checklist for the ProductService release."
  "QA checklist" → QA (compound noun with QA abbreviation) | "complete" → gap (alone)
  CompiledPredicateIds: ["QA"]

