# Layer 0-C — Knowledge Edge Pairs (027) — AmortizationSchedule: "confirm" and "update" gap patterns
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E027-001 — Gap: "confirm" → knowledge_edge (informal QA)

A: "Confirm the AmortizationSchedule calculates the correct remaining balance after 24 payments."
B:
  CompiledPredicateIds: []
  GapVocabulary: ["confirm", "remaining balance", "after 24 payments"]
  Note: "confirm" is informal QA language. Not in TEST predicate lexicon.
  Compare: "validate the remaining balance" → TEST (fires); "confirm the remaining balance" → [] (gap).

---

## E027-002 — Gap: "update" is NOT in IMPLEMENT lexicon → knowledge_edge

A: "Update the AmortizationSchedule to support bi-weekly payment periods."
B:
  CompiledPredicateIds: []
  GapVocabulary: ["update", "bi-weekly payment periods"]
  Note: "update" is NOT in the IMPLEMENT predicate lexicon.
  IMPLEMENT lexicon: write, create, build, develop, implement, add, extend, generate.
  "update", "modify", "change", "fix", "patch" are NOT in the IMPLEMENT lexicon.
  SCHEDULE_CAPABILITY_LEARNING: true

---

## E027-003 — Gap vocabulary for amortization domain: all zero-predicate

The following phrases produce zero predicates in the AmortizationSchedule context:
  "remaining balance", "payment period", "bi-weekly", "monthly instalment", "early payoff penalty",
  "amortisation table", "mortgage schedule", "principal reduction", "interest portion"
  None of these fire any predicate. Predicates fire from action verbs only.
