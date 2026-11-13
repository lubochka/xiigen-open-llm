# Layer 0-C — Knowledge-Edge Pairs (018)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-018-001 — Production handoff: "handoff" has no predicate

"Handoff" is an operational delivery term in B38. It does not fire any predicate.
"Production handoff" similarly does not fire any predicate — both words are context.

What fires predicates in handoff sentences:
- "sign-off" → QA (sign-off is in QA lexicon; "handoff" is NOT "sign-off")
- "service" → IMPLEMENT (if a service is mentioned)
- "criteria" → DOCUMENT (if criteria are mentioned)

Key distinction: "sign-off" ≠ "handoff"
- "sign-off" contains "sign" (approval action) and fires QA
- "handoff" is an operational transfer term and fires nothing

---

## B38-0C-018-002 — Handoff runbook: gap boundary

Input: "Execute the handoff runbook and monitor the production environment after cutover."

Why knowledge_edge:
- "Execute" → rule 8 (execution context gap)
- "handoff" → no predicate
- "runbook" → B38 gap vocabulary
- "monitor" → rule 7 (gap)
- "production environment" → context phrase
- "cutover" → B38 gap vocabulary

Zero predicates → knowledge_edge.
Gap IDs: gap-project-delivery-runbook-001, gap-project-delivery-cutover-001

---

## B38-0C-018-003 — "Compliance" fires QA (not gap) in handoff context

Input: "Confirm compliance with the handoff checklist and get approval from all stakeholders."

Why this is NOT knowledge_edge:
- "compliance" → QA (compliance fires QA in release context)
- "approval" → QA (approve fires QA)
- "handoff" → no predicate (context)
- "checklist" → GAP (gets warning but does not suppress QA — v6-01 rule)

Result: ["QA"] with CapabilityGapWarning on "checklist".
Not knowledge_edge — QA fires.

Lesson: "compliance" is in the QA lexicon. It fires QA even when surrounded by gap vocabulary.
