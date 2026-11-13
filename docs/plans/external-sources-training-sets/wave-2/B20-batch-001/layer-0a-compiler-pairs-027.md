# Layer 0-A — Compiler Pairs (027) — AmortizationSchedule: TEST vs execution boundary
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: compiler

---

## P027-001 — TEST+IMPLEMENT: "develop and write tests" for AmortizationSchedule

A: "Develop the AmortizationSchedule service and write unit tests covering principal reduction, interest payments, and final payment rounding."
  "develop" → IMPLEMENT | "write" → IMPLEMENT (deduplicates) | "unit tests" → TEST
  "principal reduction", "interest payments", "final payment rounding" — domain phrases; no predicate.
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition | SubPhaseId: 5.1 + 4.1

---

## P027-002 — GAP: "generate amortization schedule" — zero predicates

A: "Generate the amortization schedule for a 30-year mortgage at 4.5% interest."
  "generate" — execution/output verb; NOT in TEST or IMPLEMENT predicate lexicon in isolation.
  Wait: "generate" is in the IMPLEMENT lexicon. Check: "generate" → IMPLEMENT.
  CompiledPredicateIds: ["IMPLEMENT"] | TransitionType: feature_addition | SubPhaseId: 4.1
  Note: "generate" fires IMPLEMENT (construction verb), not TEST.

---

## P027-003 — TEST: "validate" fires on amortization edge cases

A: "Validate that the AmortizationSchedule handles early payoff scenarios correctly."
  "validate" → TEST universally. "early payoff" — domain term; no predicate.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P027-004 — GAP: "confirm" does NOT fire TEST

A: "Confirm the AmortizationSchedule calculates the correct remaining balance after 24 payments."
  "confirm" — informal QA language; NOT in TEST predicate lexicon.
  "remaining balance" — domain phrase; no predicate.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## P027-005 — TEST: "test fixture" fires TEST

A: "Create a test fixture that preloads a 15-year amortization schedule for use across all AmortizationSchedule tests."
  "test fixture" → TEST (compound noun in test infrastructure) | "create" → IMPLEMENT
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition
