# Layer 0-A — Compiler Pairs (055) — UnitConverter: TEST fires on test vocabulary, not unit-domain terms
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: compiler

---

## P055-001 — TEST+IMPLEMENT: "implement UnitConverter and write test harness"

A: "Implement the UnitConverter and write a test harness for metric to imperial length conversions."
  "implement" → IMPLEMENT | "write" → IMPLEMENT (deduplicates) | "test harness" → TEST
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition | SubPhaseId: 5.1 + 4.1

---

## P055-002 — TEST: "assert" fires TEST on conversion factor correctness

A: "Assert that the UnitConverter returns 2.54 centimetres per inch with precision to four decimal places."
  "assert" → TEST.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P055-003 — GAP: "convert metres to feet" fires zero predicates

A: "Convert the measurement from metres to feet using the UnitConverter."
  "convert" → domain operation verb; not in any predicate lexicon.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## P055-004 — TEST: "validate" fires TEST for UnitConverter unsupported unit type

A: "Validate that the UnitConverter throws UnsupportedConversionException for incompatible unit types."
  "validate" → TEST universally.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P055-005 — GAP: "apply conversion factor" fires zero predicates

A: "Apply the conversion factor from the UnitConverter lookup table to the input measurement."
  "apply" (standalone) → domain operation verb; not in IMPLEMENT lexicon.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
