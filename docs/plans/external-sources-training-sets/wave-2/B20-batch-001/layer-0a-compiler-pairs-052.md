# Layer 0-A — Compiler Pairs (052) — TimeZoneConverter: TEST fires on test vocabulary, not timezone terms
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: compiler

---

## P052-001 — TEST+IMPLEMENT: "extend TimeZoneConverter and add regression tests"

A: "Extend the TimeZoneConverter to support IANA (Internet Assigned Numbers Authority) timezone IDs and add regression tests."
  "extend" → IMPLEMENT | "add" → IMPLEMENT (deduplicates) | "regression tests" → TEST
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition | SubPhaseId: 5.1 + 4.1

---

## P052-002 — TEST: "verify" fires TEST on timezone offset correctness

A: "Verify that the TimeZoneConverter correctly converts UTC (Coordinated Universal Time) +0 to GMT (Greenwich Mean Time) -5 for New York."
  "verify" → TEST. UTC (Coordinated Universal Time) and GMT (Greenwich Mean Time) are domain terms; neither fires a predicate.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P052-003 — GAP: "convert timezone" fires zero predicates

A: "Convert the meeting timestamp from UTC (Coordinated Universal Time) to Tokyo Standard Time using the TimeZoneConverter."
  "convert" → domain operation verb; not in any predicate lexicon.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## P052-004 — TEST: "assert" fires TEST for DST (Daylight Saving Time) transition edge case

A: "Assert that the TimeZoneConverter handles DST (Daylight Saving Time) transitions without producing a 1-hour offset error."
  "assert" → TEST. DST (Daylight Saving Time) is a domain abbreviation; it does not fire any predicate.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P052-005 — GAP: "apply timezone offset" fires zero predicates

A: "Apply the timezone offset to the stored UTC (Coordinated Universal Time) timestamp before display."
  "apply" (standalone) → domain operation verb; not in IMPLEMENT lexicon.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
