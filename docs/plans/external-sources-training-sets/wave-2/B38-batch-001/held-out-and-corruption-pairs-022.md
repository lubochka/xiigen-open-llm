# Held-Out and Corruption Pairs (022)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## B38-HO-022-001 — HELD-OUT: staged rollout architecture is QA+ARCHITECTURE, not feature_addition

Input: "Review the staged rollout architecture design and approve the phased deployment approach."

Correct result:
- "Review" → QA
- "staged rollout" → GAP (rollout = gap; "staged" = context)
- "architecture" → ARCHITECTURE predicate
- "design" → ARCHITECTURE support
- "approve" → QA (consolidates)
- "phased deployment" → "deployment" = GAP (rule 4)
- PredicateSet: ["QA","ARCHITECTURE"]
- CapabilityGapWarnings: ["rollout","deployment"]
- TransitionType: no_contribution (ARCHITECTURE ≠ feature_addition)
- DecisionCount: 2

Gate B assertion: PredicateSet == ["QA","ARCHITECTURE"] AND TransitionType == "no_contribution"

---

## B38-HO-022-002 — CORRUPTION: ARCHITECTURE wrongly produces feature_addition

Corrupt behavior:
- TransitionType: feature_addition (WRONG — ARCHITECTURE does not produce feature_addition)

Why this is wrong:
feature_addition requires IMPLEMENT in the predicate set.
ARCHITECTURE represents design review, not code authoring.
ARCHITECTURE + QA → no_contribution.
ARCHITECTURE + IMPLEMENT + QA → feature_addition (because of IMPLEMENT, not ARCHITECTURE).

---

## B38-HO-022-003 — HELD-OUT: staged rollout launch is knowledge_edge

Input: "Launch the staged rollout on Friday and monitor each environment tier for issues."

Correct result:
- PredicateSet: []
- CapabilityGapWarnings: ["launch","rollout","monitor"]
- ResultState: knowledge_edge

Gate B assertion: PredicateSet == [] AND "launch" in CapabilityGapWarnings AND "rollout" in CapabilityGapWarnings
