# Layer 0-C — Knowledge-Edge Pairs (014)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-014-001 — Release train: "train" has no predicate

"Release train" is a delivery-planning concept. The word "train" does not fire any predicate.
The word "release" does not fire any predicate on its own.

"Approve the Q2 release train" → only "approve" fires (QA). "release train" contributes zero predicates.

This is important: when "service" is NOT present in a release train sentence, IMPLEMENT does not fire.
Compare:
- "Approve the Q2 release train" → ["QA"] only
- "Approve the Q2 release train containing the auth service" → ["QA","IMPLEMENT"] ("service" fires)

---

## B38-0C-014-002 — Release train launch: pure gap

Input: "Launch the Q2 release train on the scheduled go-live date."
- "Launch" → GAP
- "release train" → no predicate
- "go-live" → GAP

Result: knowledge_edge.
Gap IDs: gap-project-delivery-launch-001, gap-project-delivery-go-live-001

This demonstrates that "release train launch" is entirely outside the predicate model.
The system cannot route this input — it must record a knowledge_edge result.

---

## B38-0C-014-003 — Release train build: IMPLEMENT fires

Input: "Inspect the build artifacts in each release train slot and approve those that pass quality gates."
- "build" → IMPLEMENT (rule 6 — builds and build artifacts fire IMPLEMENT)
- "Inspect" → QA
- "approve" → QA
- "quality" → QA

Result: ["QA","IMPLEMENT"] — not knowledge_edge.

Lesson: Adding "build" to a release train sentence immediately exits knowledge_edge territory.
"build" is the code vocabulary that converts an otherwise operational sentence into a routable predicate result.
