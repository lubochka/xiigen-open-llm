# Layer 3 — Teaching Unit Pairs (022)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## B38-TU-022-001 — Teaching: ARCHITECTURE predicate does not produce feature_addition

LESSON: ARCHITECTURE fires from "architecture" vocabulary but does not produce feature_addition.

"Review the staged rollout architecture design" → ["QA","ARCHITECTURE"]
TransitionType: no_contribution

Only IMPLEMENT produces feature_addition. ARCHITECTURE, DOCUMENT, SECURITY, QA alone → all produce no_contribution.

This is a subtle distinction:
- IMPLEMENT = code is being written (new feature or update)
- ARCHITECTURE = design is being reviewed (no code authored yet)
- feature_addition requires actual code authoring (IMPLEMENT)

---

## B38-TU-022-002 — Teaching: "rollout" is gap everywhere

LESSON: "Rollout" in any context is B38 gap vocabulary. It never fires a predicate.

"staged rollout" → gap
"rollout gate" → gate is context; "rollout" still gets gap warning
"rollout architecture" → "architecture" fires ARCHITECTURE; "rollout" gets gap warning
"rollout success criteria" → "criteria" fires DOCUMENT; "rollout" gets gap warning

"Rollout" never changes a predicate result. It only adds a CapabilityGapWarning.

---

## B38-TU-022-003 — Teaching: launch + rollout + monitor = triple gap → knowledge_edge

LESSON: Three of the most common B38 operational terms are all gap vocabulary.

"Launch the staged rollout on Friday and monitor each environment tier for issues."
- "launch" → GAP
- "rollout" → GAP
- "monitor" → GAP

Three gap terms → zero predicates → knowledge_edge.

This is the "operations team asks for help with launch day" pattern.
The system cannot route this. The knowledge_edge records the gap for future capability development.

Adding any QA/IMPLEMENT/DOCUMENT vocabulary immediately exits knowledge_edge.
The minimum: "Review the staged rollout" → ["QA"] (just one QA term changes the result).
