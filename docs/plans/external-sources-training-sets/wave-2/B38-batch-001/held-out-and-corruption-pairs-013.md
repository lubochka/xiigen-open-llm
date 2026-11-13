# Held-Out and Corruption Pairs (013)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## B38-HO-013-001 — HELD-OUT: environment promotion with criteria

Input: "Verify that the staging environment meets all promotion criteria before approving."

Correct result:
- "Verify" → QA
- "approving" → QA (consolidates)
- "promotion criteria" → DOCUMENT (criteria fires DOCUMENT — rule 3)
- "staging environment" → no predicate (context)
- PredicateSet: ["QA","DOCUMENT"]
- TransitionType: no_contribution
- Phase routing: 2.1 + 7.1
- DecisionCount: 2

Gate B (held-out assertion): PredicateSet == ["QA","DOCUMENT"] AND TransitionType == "no_contribution"

---

## B38-HO-013-002 — CORRUPTION: "promotion" wrongly fires IMPLEMENT

Corrupt input processing:
- "promotion" → IMPLEMENT (WRONG — promotion is operational context, not code authoring)

Corrupted result:
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT"] (WRONG — extra IMPLEMENT from "promotion")
- TransitionType: feature_addition (WRONG — should be no_contribution)

Why this is wrong:
"promotion" describes a process action (moving code between environments), not code authoring.
IMPLEMENT requires code-authoring vocabulary: service, build, pipeline, implementation.
"promotion" is not in the IMPLEMENT lexicon.

Correct: ["QA","DOCUMENT"]; no_contribution.

---

## B38-HO-013-003 — HELD-OUT: pure cutover gap

Input: "Run the environment cutover and monitor the promotion rollout."

Correct result:
- PredicateSet: []
- CapabilityGapWarnings: ["cutover","monitor","rollout"]
- ResultState: knowledge_edge

Gate B assertion: PredicateSet == [] AND "cutover" in CapabilityGapWarnings AND "rollout" in CapabilityGapWarnings
