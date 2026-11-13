# Layer 0-C — Knowledge-Edge Pairs (022)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-022-001 — Staged rollout: "rollout" is gap; "staged" is context

"Staged rollout" as a compound noun is B38 gap vocabulary (via "rollout").
"Staged" is a descriptor that modifies "rollout". Neither word fires a predicate.

"Approve the staged rollout gate" → only "approve" fires QA. "staged rollout" gets a gap warning.
"Launch the staged rollout" → "launch" is gap + "rollout" is gap → knowledge_edge.

Key: the gate or approval action ("approve", "validate") fires QA. The rollout itself is always gap.

---

## B38-0C-022-002 — ARCHITECTURE predicate boundary in staged rollout

"Review the staged rollout architecture design and approve the phased deployment approach."
- "Review" → QA
- "staged rollout" → GAP (rollout = gap)
- "architecture" → ARCHITECTURE predicate fires
- "design" → supports ARCHITECTURE context
- "approve" → QA
- "phased deployment" → "deployment" is GAP (rule 4)

Result: ["QA","ARCHITECTURE"] with CapabilityGapWarnings: ["rollout","deployment"]

ARCHITECTURE is a separate predicate from IMPLEMENT. It routes to Phase 5 (architecture/design phases).
ARCHITECTURE does NOT produce feature_addition — only IMPLEMENT does.
TransitionType: no_contribution even with ARCHITECTURE present.

---

## B38-0C-022-003 — Staged rollout launch: knowledge_edge boundary

Input: "Launch the staged rollout on Friday and monitor each environment tier for issues."

Why knowledge_edge:
- "Launch" → B38 gap vocabulary
- "staged rollout" → B38 gap vocabulary (via "rollout")
- "monitor" → rule 7
- "environment tier" → context phrase
- "issues" → not in predicate lexicon

Zero predicates → knowledge_edge.
Gap IDs: gap-project-delivery-launch-001, gap-project-delivery-rollout-golive-001.
