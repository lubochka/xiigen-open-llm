# Layer 3 — Teaching Unit Pairs (020)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## B38-TU-020-001 — Teaching: post-release validation is QA alone (not knowledge_edge)

LESSON: Post-release validation uses "validate" and "verify" which are in the QA lexicon.

"Validate the production environment after release" → ["QA"] (validate fires QA)
"Monitor the production rollout" → [] → knowledge_edge (monitor is gap; rollout is gap)

The difference:
- "validate" / "verify" → QA predicate → FLOW_EXECUTING
- "monitor" / "check" → no predicate → knowledge_edge

Post-release validation tasks that use QA vocabulary are routable.
Post-release monitoring tasks are outside the predicate model (knowledge_edge).

---

## B38-TU-020-002 — Teaching: "deployment" as a standalone noun is always gap

LESSON: "Deployment" in post-release context is operational, not code authoring.

"Verify the deployment is stable" → QA fires on "verify"; "deployment" gets gap warning
"Deploy the service" → "deploy" is gap; "service" fires IMPLEMENT → ["IMPLEMENT"] with gap warning
"Monitor the deployment" → both gap → knowledge_edge

Rule 4: "deployment" and "deploy" without code co-occurrence do not fire IMPLEMENT.
The distinction: code is written (IMPLEMENT); deployment just runs existing code (operational gap).

---

## B38-TU-020-003 — Teaching: service saves post-release from knowledge_edge

LESSON: Adding "service" to any post-release sentence forces IMPLEMENT to fire.

"Monitor the production rollout after go-live" → knowledge_edge
"Monitor the payment service post-release" → ["IMPLEMENT"] (service fires; monitor gets gap warning)
"Verify the payment service is stable post-release" → ["QA","IMPLEMENT"]

The pattern: service vocabulary = IMPLEMENT, regardless of context.
Post-release, pre-release, during-release — "service" always fires IMPLEMENT.
