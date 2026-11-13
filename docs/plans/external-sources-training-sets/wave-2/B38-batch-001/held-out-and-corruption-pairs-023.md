# Held-Out and Corruption Pairs (023)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## B38-HO-023-001 — HELD-OUT: sign-off with three gap terms

Input: "Sign off on the go-live checklist and confirm launch readiness."

Correct result:
- "Sign off" → QA
- "go-live" → GAP warning
- "checklist" → GAP warning
- "launch" → GAP warning
- "confirm" → not in predicate lexicon
- PredicateSet: ["QA"]
- CapabilityGapWarnings: ["go-live","checklist","launch"]
- TransitionType: no_contribution
- DecisionCount: 1

Gate B assertion: PredicateSet == ["QA"] AND len(CapabilityGapWarnings) == 3 AND TransitionType == "no_contribution"

---

## B38-HO-023-002 — CORRUPTION (v6-01): QA suppressed by go-live/checklist context

Corrupt input processing:
- "go-live" and "checklist" present → model infers all vocabulary is operational
- QA is incorrectly suppressed: PredicateSet: [] (WRONG)
- Result: knowledge_edge (WRONG)

Why this is wrong:
This is the canonical v6-01 corruption.
"sign off" is in the QA lexicon. It fires QA regardless of surrounding vocabulary.
The presence of gap vocabulary adds CapabilityGapWarnings.
The presence of gap vocabulary NEVER suppresses predicates that have fired.

Correct: ["QA"] with three gap warnings. FLOW_EXECUTING, not knowledge_edge.

---

## B38-HO-023-003 — HELD-OUT: analytics service sign-off fires IMPLEMENT

Input: "Get sign-off on the analytics service before the release date."

Correct result:
- "sign-off" → QA
- "analytics service" → IMPLEMENT (service fires IMPLEMENT — rule 2)
- "release date" → context
- PredicateSet: ["QA","IMPLEMENT"]
- TransitionType: feature_addition
- DecisionCount: 2

Gate B assertion: PredicateSet == ["QA","IMPLEMENT"] AND TransitionType == "feature_addition"
