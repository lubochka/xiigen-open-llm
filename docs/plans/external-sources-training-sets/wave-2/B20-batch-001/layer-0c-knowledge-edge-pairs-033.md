# Layer 0-C — Knowledge Edge Pairs (033) — MarkupCalculator: "apply" and "launch" gaps
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E033-001 — Gap: "apply" standalone → knowledge_edge

A: "Apply a 25% markup to all items in the clearance category."
B:
  CompiledPredicateIds: []
  GapVocabulary: ["apply", "25% markup", "clearance category"]
  Note: "apply" standalone is NOT in the IMPLEMENT predicate lexicon.
  "apply" fires no predicate when used as a business operation verb.
  Contrast: "apply a fix" → also zero predicates. "apply" never fires IMPLEMENT.

---

## E033-002 — Gap: "launch" → knowledge_edge

A: "Launch the MarkupCalculator to price all SKUs in the catalogue."
B:
  CompiledPredicateIds: []
  GapVocabulary: ["launch", "SKUs", "catalogue"]
  Note: "launch" is an execution verb; zero predicates. "SKU (Stock Keeping Unit)" is a domain abbreviation; no predicate.

---

## E033-003 — "test coverage" rescues a "ensure" sentence from being zero-predicate

A: "Ensure test coverage for the MarkupCalculator includes 100% branch coverage."
  "ensure" alone → would be [] (informal QA). But "test coverage" is a TEST-firing compound noun.
  The presence of "test coverage" fires TEST even when the main verb is "ensure".
  CompiledPredicateIds: ["TEST"] — the compound noun "test coverage" overrides the zero-predicate main verb.
  This is an important boundary: noun compounds in the TEST lexicon fire TEST regardless of the main verb.
