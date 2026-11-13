# Held-Out and Corruption Pairs (014)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## B38-HO-014-001 — HELD-OUT: release train approval without service

Input: "Approve the Q2 release train and confirm all feature teams have signed off."

Correct result:
- "Approve" → QA
- "signed off" → QA (consolidates)
- "release train" → no predicate
- "Q2" → context
- "feature teams" → context
- PredicateSet: ["QA"]
- TransitionType: no_contribution
- DecisionCount: 1

Gate B assertion: PredicateSet == ["QA"] AND TransitionType == "no_contribution"

---

## B38-HO-014-002 — CORRUPTION: "train" wrongly fires IMPLEMENT

Corrupt input processing:
- "train" → IMPLEMENT (WRONG — "train" in "release train" refers to a delivery schedule, not a model or ML pipeline)

Corrupted result:
- PredicateSet: ["QA","IMPLEMENT"] (WRONG — "train" does not fire IMPLEMENT in release context)
- TransitionType: feature_addition (WRONG)

Why this is wrong:
"release train" is a delivery-planning compound noun. "train" here means a sequence of releases.
IMPLEMENT fires from: service, build, pipeline, implementation, code, module.
"train" is not in the IMPLEMENT lexicon in this context.
