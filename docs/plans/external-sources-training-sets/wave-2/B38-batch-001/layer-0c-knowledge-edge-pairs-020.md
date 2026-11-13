# Layer 0-C — Knowledge-Edge Pairs (020)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-020-001 — Post-release: "validate" fires QA; "deployment" is gap

Post-release validation sentences commonly use "validate" and "verify" — both are in QA lexicon.
"deployment" in post-release context is operational (rule 4) → GAP, not IMPLEMENT.

"Validate the production environment after release and verify the deployment is stable."
→ "Validate" → QA; "verify" → QA; "deployment" → GAP

Result: ["QA"] with CapabilityGapWarning: ["deployment"]

Not knowledge_edge because QA fires.

---

## B38-0C-020-002 — Post-release monitor: knowledge_edge boundary

Input: "Monitor the production rollout and check for errors after go-live."

Why knowledge_edge:
- "Monitor" → rule 7 (not in predicate lexicon)
- "production rollout" → "rollout" is B38 gap vocabulary
- "check" → not in predicate lexicon
- "go-live" → B38 gap vocabulary

Zero predicates → knowledge_edge.

This is one of the core B38 knowledge edge patterns: monitoring + rollout + go-live is entirely gap vocabulary. The operations team asking for a monitoring runbook is outside the predicate model.

---

## B38-0C-020-003 — Adding "service" to post-release monitoring changes the result

"Monitor the payment service post-release." → "service" fires IMPLEMENT (rule 2).
Even though "monitor" is gap, "service" fires IMPLEMENT.
Result: ["IMPLEMENT"] with CapabilityGapWarning: ["monitor"].
NOT knowledge_edge.

This parallels the pattern from B38-0C-002-004 (original batch):
"Monitor the auth service post-release." → ["IMPLEMENT"] with gap warning on monitor.

Lesson: "service" overrides gap context. Adding "service" to any monitoring sentence immediately exits knowledge_edge territory.
