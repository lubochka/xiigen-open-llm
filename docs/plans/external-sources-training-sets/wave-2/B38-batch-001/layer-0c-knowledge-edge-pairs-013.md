# Layer 0-C — Knowledge-Edge Pairs (013)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-013-001 — Environment promotion: gap vs predicate boundary

Scenario: EnvironmentPromotionGate
Rule demonstrated: "environment" is context, not a predicate trigger.

Knowledge edges in this scenario:
1. "promotion criteria" → "criteria" fires DOCUMENT (rule 3). "promotion" alone does not fire anything.
2. "staging environment" → "environment" is not in any predicate lexicon. It is infrastructure context.
3. "production" → context word. Not in any predicate lexicon.
4. "promote" (as verb) → not in any predicate lexicon. It is an operational verb.

Knowledge edge: "environment promotion" contains no predicate triggers on its own.
Predicate triggers in this scenario come only from: "criteria" (DOCUMENT) and QA terms (verify/approve).

---

## B38-0C-013-002 — Why "service" overrides context in environment promotion

Consider: "Approve the auth service promotion from staging to production."
- "approve" → QA
- "auth service" → IMPLEMENT (rule 2 — always)
- "promotion" → not a predicate trigger
- "staging" / "production" → context words

Knowledge edge lesson: "service" fires IMPLEMENT regardless of surrounding operational vocabulary ("staging", "production", "promotion"). The compiler is literal and evaluates each word independently.

The word "service" is the code-signal that forces IMPLEMENT. The operational words around it do not suppress IMPLEMENT.

This is the same principle as B38 rule 2 applied to the environment promotion scenario.

---

## B38-0C-013-003 — Environment cutover: pure gap boundary

Input: "Run the environment cutover and monitor the promotion rollout."

Why this is knowledge_edge:
- "Run" → not IMPLEMENT (execution verb, not code-authoring)
- "environment cutover" → "cutover" is B38 gap vocabulary
- "monitor" → not in predicate lexicon (rule 7)
- "promotion rollout" → "rollout" is B38 gap vocabulary

No predicate fires. Result: knowledge_edge.

What would need to change for a predicate to fire:
- Adding "service" → fires IMPLEMENT
- Adding "criteria" → fires DOCUMENT
- Adding "review", "verify", "approve", "sign-off", "audit", "compliance", "inspect" → fires QA
