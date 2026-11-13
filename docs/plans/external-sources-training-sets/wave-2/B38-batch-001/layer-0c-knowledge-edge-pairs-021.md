# Layer 0-C — Knowledge-Edge Pairs (021)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-021-001 — Incident response: "remediation" has no predicate

"Remediation" and "incident" are not predicate triggers in B38.
They are delivery-context words that describe the situation but do not map to predicates.

"Review the incident response" → only "review" fires QA. "incident" and "response" are context.
"Execute the incident rollback" → "execute" is gap; "rollback" has no predicate → knowledge_edge.

---

## B38-0C-021-002 — Rollback: knowledge_edge boundary

"Execute the incident rollback and monitor the go-live rollout after remediation."

Why knowledge_edge:
- "Execute" → rule 8 (execution context gap)
- "incident" → no predicate
- "rollback" → no predicate (rollback is a recovery action, not a code-authoring action)
- "monitor" → rule 7 (gap)
- "go-live" → B38 gap vocabulary
- "rollout" → B38 gap vocabulary
- "remediation" → no predicate

Zero predicates → knowledge_edge.

Note: "rollback" is NOT in the B38 gap vocabulary list by name, but it has no predicate either. The result is the same: it contributes zero predicates.

---

## B38-0C-021-003 — Incident service fix: exits knowledge_edge

"Verify the incident-related fix to the auth service and sign off on the emergency release."
- "Verify" → QA
- "auth service" → IMPLEMENT (rule 2)
- "sign off" → QA

Result: ["QA","IMPLEMENT"] — not knowledge_edge.

The word "service" in the incident context fires IMPLEMENT just as in any other B38 context.
"Emergency release" is a context phrase; "emergency" does not fire any predicate.
"Fix" does not fire IMPLEMENT (fix is a process noun, not code authoring vocabulary in B38).
