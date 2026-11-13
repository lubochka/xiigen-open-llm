# Layer 0-C — Knowledge Edge Pairs (026) — CompoundInterestAlgorithm: "run" and "compute" gap patterns
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E026-001 — Gap: "run" with financial algorithm → knowledge_edge

A: "Run the CompoundInterestAlgorithm with a 6% annual rate compounded monthly."
B:
  CompiledPredicateIds: []
  GapVocabulary: ["run the algorithm", "6% annual rate", "compounded monthly"]
  Note: "run" is an execution verb. None of the domain terms (6%, compounded, annual rate) elevate it.
  SCHEDULE_CAPABILITY_LEARNING: true

---

## E026-002 — Gap: "compute" is a domain verb (same family as "calculate") → knowledge_edge

A: "Compute the compound interest for a £10,000 deposit over 5 years at 3% compounded quarterly."
B:
  CompiledPredicateIds: []
  GapVocabulary: ["compute the compound interest", "compounded quarterly"]
  Note: "compute" is a mathematical domain verb; not in any predicate lexicon.
  Same gap class as "calculate", "determine", "derive", "evaluate".

---

## E026-003 — Contrast table: compounding-domain verbs vs TEST-firing verbs

| Action Verb | CompiledPredicateIds | Gap or Routable |
|-------------|---------------------|-----------------|
| run | [] | knowledge_edge |
| compute | [] | knowledge_edge |
| calculate | [] | knowledge_edge |
| determine | [] | knowledge_edge |
| validate | ["TEST"] | routable → 5.1 |
| assert | ["TEST"] | routable → 5.1 |
| write tests | ["TEST","IMPLEMENT"] | routable → 5.1 + 4.1 |

CompoundInterestAlgorithm domain provides no predicate signals. Predicates come from action verbs only.
