# Layer 0-A — Compiler Pairs (033) — MarkupCalculator: TEST predicate with pricing domain
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: compiler

---

## P033-001 — TEST: "test coverage" fires TEST

A: "Ensure test coverage for the MarkupCalculator includes 100% branch coverage on the rounding logic."
  "ensure" — informal QA; does NOT fire TEST. "test coverage" → TEST (compound noun in TEST lexicon).
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1
  Note: "ensure" is zero-predicate, but "test coverage" is a TEST-firing compound noun.

---

## P033-002 — TEST+IMPLEMENT: "build and create test suite"

A: "Build the MarkupCalculator and create a test suite covering retail, wholesale, and bulk pricing tiers."
  "build" → IMPLEMENT | "create" → IMPLEMENT (deduplicates) | "test suite" → TEST
  "retail", "wholesale", "bulk pricing tiers" — domain terms; no predicate.
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition

---

## P033-003 — GAP: "launch" is execution verb — zero predicates

A: "Launch the MarkupCalculator to price all SKUs (Stock Keeping Units) in the catalogue."
  "launch" — execution verb; zero predicates. "SKUs" — domain abbreviation; no predicate.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## P033-004 — TEST: "mock price source" fires TEST via "mock"

A: "Mock the price source and assert the MarkupCalculator applies a 30% markup correctly."
  "mock" → TEST (test-adjacent) | "assert" → TEST (deduplicates)
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P033-005 — Boundary: "markup", "margin", "price tier" — domain nouns, never fire predicates

A: "Apply a 25% markup to all items in the clearance category."
  "apply" — action verb; NOT in IMPLEMENT lexicon. "markup" — domain noun; no predicate.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
