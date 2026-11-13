# Layer 0-C — Knowledge Edge Pairs (025) — InterestCalculator: "check" and "ensure" gap patterns
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E025-001 — Gap: "check" in financial calculator context → knowledge_edge

A: "Check that the InterestCalculator returns correct interest for edge cases."
B:
  CompiledPredicateIds: []
  TransitionType: unknown
  SourceStatus: knowledge_edge
  GapVocabulary: ["check that", "returns correct", "for edge cases"]
  Note: "check" is informal QA language. The XIIGen (XIIGEN) phase model has no routing for informal verification requests.
  SCHEDULE_CAPABILITY_LEARNING: true

---

## E025-002 — Gap: "testable" adjective → knowledge_edge

A: "Ensure the InterestCalculator is testable with dependency-injected rate providers."
B:
  CompiledPredicateIds: []
  GapVocabulary: ["ensure", "is testable", "dependency-injected"]
  Note: "ensure" is informal QA (zero predicate). "testable" is an adjective — does NOT fire TEST.
  The adjective form "testable" never fires TEST regardless of surrounding context.

---

## E025-003 — Contrast: "is testable" (gap) vs "write tests" (routable)

| Request | CompiledPredicateIds | SourceStatus |
|---------|---------------------|--------------|
| "Ensure the InterestCalculator is testable" | [] | knowledge_edge |
| "Write tests for the InterestCalculator" | ["TEST", "IMPLEMENT"] | routable |
| "Validate the InterestCalculator result" | ["TEST"] | routable |

"is testable" describes an architectural quality; it does not request test creation.
Only test creation/assertion requests fire TEST.
