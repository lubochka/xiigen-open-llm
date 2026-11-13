# Layer 0-C — Knowledge Edge Pairs (058) — ProbabilityEngine: gap analysis for probability-domain verbs
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E058-001 — "calculate the probability" does NOT fire IMPLEMENT

"Calculate the probability of rolling a six on a fair die using the ProbabilityEngine." → CompiledPredicateIds: []
"calculate" → domain computation verb; zero predicates.
Probability computation verbs: calculate, compute, estimate, derive — all zero predicates.

---

## E058-002 — "evaluate probability distribution" does NOT fire TEST

"Evaluate the probability distribution across all possible outcomes in the ProbabilityEngine." → CompiledPredicateIds: []
"evaluate" → domain computation verb; NOT in the TEST predicate lexicon.
Critical boundary: "evaluate" (zero predicates) ≠ "assert" / "validate" / "verify" (TEST fires).

---

## E058-003 — "Bayes theorem" domain term does NOT fire any predicate

"Apply Bayes theorem in the ProbabilityEngine to update prior probabilities." → CompiledPredicateIds: []
"apply" (standalone) → NOT in IMPLEMENT lexicon; zero predicates.
"Bayes theorem" → domain mathematical theorem; does not fire predicates.

---

## E058-004 — "ArgumentOutOfRangeException" does NOT fire TEST independently

"Assert that the ProbabilityEngine throws ArgumentOutOfRangeException for probability values above 1.0." → ["TEST"]
  "assert" → TEST fires.
"The ProbabilityEngine should throw ArgumentOutOfRangeException for values above 1.0." → []
  "should throw" → modal + domain verb; does not fire TEST. Same subject matter, zero predicates.
Teaching: "should" + domain behaviour description never fires predicates. Only test-lexicon action verbs fire TEST.
