# Layer 0-A — Compiler Pairs (049) — StringParser: TEST fires on "assert", not on parse-domain terms
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: compiler

---

## P049-001 — TEST+IMPLEMENT: "create StringParser and write unit tests"

A: "Create the StringParser class and write unit tests for delimiter-based splitting."
  "create" → IMPLEMENT | "write" → IMPLEMENT (deduplicates) | "unit tests" → TEST
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition | SubPhaseId: 5.1 + 4.1

---

## P049-002 — TEST: "assert" fires TEST on StringParser null-input handling

A: "Assert that the StringParser throws ArgumentNullException when input is null."
  "assert" → TEST.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P049-003 — GAP: "parse the input string" fires zero predicates

A: "Parse the input string using the configured delimiter in the StringParser."
  "parse" → domain operation verb; not in any predicate lexicon.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## P049-004 — TEST: "validate" fires TEST for StringParser empty-string edge case

A: "Validate that the StringParser returns an empty collection when given an empty string."
  "validate" → TEST universally.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P049-005 — GAP: "split the token stream" fires zero predicates

A: "Split the token stream at semicolon delimiters using the StringParser."
  "split" → domain operation verb; not in any predicate lexicon.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
