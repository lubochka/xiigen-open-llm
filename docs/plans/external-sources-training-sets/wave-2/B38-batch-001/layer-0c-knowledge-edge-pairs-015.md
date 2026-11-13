# Layer 0-C — Knowledge-Edge Pairs (015)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-015-001 — Hotfix: "hotfix" is not a predicate trigger

"Hotfix" as a noun describing the type of change being released does not fire any predicate.
The predicate triggers come from the verbs and nouns surrounding the hotfix.

"Approve the hotfix" → only "approve" fires QA. "hotfix" is context.
"Execute the hotfix rollout" → "execute" is gap (rule 8); "rollout" is gap; result = knowledge_edge.

This parallels how "release", "deployment", and "launch" work: the noun describing the delivery artifact has no predicate by itself.

---

## B38-0C-015-002 — Hotfix rollout: pure gap boundary

Input: "Execute the hotfix rollout and monitor the go-live rollout after remediation."

Why every term is gap:
- "Execute" → rule 8 (execution context ≠ code authoring)
- "hotfix" → delivery noun, no predicate
- "rollout" → B38 gap vocabulary (listed explicitly)
- "monitor" → rule 7
- "go-live" → B38 gap vocabulary
- "remediation" → not in any predicate lexicon

Result: knowledge_edge. Five gap signals, zero predicates.

---

## B38-0C-015-003 — Hotfix vs build: critical boundary

"Review the hotfix" → ["QA"] (hotfix itself does not fire IMPLEMENT; only "review" fires QA)
"Review the hotfix build" → ["QA","IMPLEMENT"] ("build" fires IMPLEMENT from rule 6)

The word "build" is the difference between QA alone and QA+IMPLEMENT in a hotfix sentence.
"Build" appearing anywhere in the input — even as a descriptor like "hotfix build" — fires IMPLEMENT.
