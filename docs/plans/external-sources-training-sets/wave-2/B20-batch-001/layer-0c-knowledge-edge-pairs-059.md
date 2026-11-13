# Layer 0-C — Knowledge Edge Pairs (059) — RandomGenerator: gap analysis for "generate" boundary case
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E059-001 — "produce random samples" does NOT fire IMPLEMENT

"Produce random samples from the distribution using the configured RandomGenerator." → CompiledPredicateIds: []
"produce" → domain operation verb; NOT in the IMPLEMENT predicate lexicon.
Confirmed: "produce" is a gap verb. It does not fire IMPLEMENT even when it describes construction-like output.

---

## E059-002 — "generate" DOES fire IMPLEMENT (boundary case for RandomGenerator)

"Generate a random number between 1 and 100 using the RandomGenerator." → CompiledPredicateIds: ["IMPLEMENT"]
"generate" → IS in the IMPLEMENT predicate lexicon.
This is the only utility operation verb that fires IMPLEMENT.
All other utility operation verbs (produce, output, compute, calculate, format, convert) do NOT fire IMPLEMENT.

---

## E059-003 — "seeded reproducibility" domain term does NOT fire TEST

"Ensure the RandomGenerator produces the same sequence for the same seed." → CompiledPredicateIds: []
"ensure" → informal QA; zero predicates. "seeded reproducibility" → domain term; does not fire TEST.
Contrast: "validate that the RandomGenerator produces the same sequence for the same seed." → ["TEST"]
  "validate" fires TEST; "ensure" does not. The domain term does not determine outcome.

---

## E059-004 — "uniform distribution" domain term does NOT fire any predicate

"Configure the RandomGenerator to use a uniform distribution." → CompiledPredicateIds: []
"configure" → domain operation verb; NOT in any predicate lexicon.
"uniform distribution" → domain statistical term; zero predicates.
Confirmed: "configure" is a recurring zero-predicate trap. IMPLEMENT does not include "configure".
