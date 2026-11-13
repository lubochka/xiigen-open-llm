# Layer 0-A — Compiler Pairs (029) — NPVCalculator: TEST firing and financial domain gaps
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: compiler

---

## P029-001 — TEST+IMPLEMENT: "build NPV (Net Present Value) calculator with test coverage"

A: "Build the NPVCalculator and create unit tests covering positive, negative, and zero net present value outcomes."
  "build" → IMPLEMENT | "create" → IMPLEMENT (deduplicates) | "unit tests" → TEST
  "net present value" — domain phrase; no predicate.
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition | SubPhaseId: 5.1 + 4.1

---

## P029-002 — GAP: "calculate NPV" — domain verb, not IMPLEMENT

A: "Calculate the NPV for a 5-year project with a 10% discount rate."
  "calculate" — domain verb; NOT in IMPLEMENT predicate lexicon (same class as "compute", "process").
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## P029-003 — TEST: "validate NPV formula" fires TEST

A: "Validate the NPV formula implementation handles a discount rate of zero without division error."
  "validate" → TEST. Universally applies regardless of context.
  "discount rate", "division error" — domain phrases; no predicate.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P029-004 — TEST: "write test harness" fires TEST

A: "Write a test harness for the NPVCalculator that exercises all cash flow permutations."
  "write" → IMPLEMENT | "test harness" → TEST (compound noun in TEST lexicon)
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition

---

## P029-005 — GAP: "discount rate", "cash flows", "present value" — none fire predicates

A: "Update the NPV calculator to accept variable discount rates and irregular cash flows."
  "update" — domain verb; NOT in IMPLEMENT lexicon. No construction verb.
  "discount rates", "cash flows" — domain terms; no predicate.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
  Note: "update" is not in the IMPLEMENT lexicon. Only "write", "create", "build", "develop", "implement", "add", "extend", "generate" fire IMPLEMENT.
