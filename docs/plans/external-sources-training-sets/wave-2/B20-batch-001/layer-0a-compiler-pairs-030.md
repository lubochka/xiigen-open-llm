# Layer 0-A — Compiler Pairs (030) — IRRCalculator: TEST boundary with iterative algorithm domain
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: compiler

---

## P030-001 — TEST+IMPLEMENT: "implement and spec" IRR (Internal Rate of Return)

A: "Implement the IRRCalculator using Newton-Raphson iteration and spec the unit tests for convergence and non-convergence cases."
  "implement" → IMPLEMENT | "spec" → TEST | "unit tests" → TEST (deduplicates)
  "Newton-Raphson", "convergence" — domain terms; no predicate.
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition | SubPhaseId: 5.1 + 4.1

---

## P030-002 — TEST: "test-driven" morphological form fires TEST

A: "Use a test-driven approach to develop the IRRCalculator iteration logic."
  "test-driven" → TEST (morphological form of "test"). "develop" → IMPLEMENT.
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition

---

## P030-003 — GAP: "iterate", "converge" — domain algorithm verbs, not predicates

A: "Iterate the IRR calculation until it converges to within 0.001% tolerance."
  "iterate", "converge" — algorithm domain verbs; zero predicates.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## P030-004 — TEST: "mock" in test context fires TEST

A: "Mock the cash flow data provider and assert the IRRCalculator handles a non-convergent series by returning an error."
  "mock" → TEST (test-adjacent context) | "assert" → TEST (deduplicates)
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P030-005 — Deduplication: "testing" gerund fires TEST once

A: "Testing the IRRCalculator for edge cases, including single cash flow and all-zero series."
  "testing" → TEST (gerund form). Only one TEST predicate regardless of how many test-related terms appear.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation
