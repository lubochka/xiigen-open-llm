# Layer 0-C — Knowledge-Edge Pairs (016)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-016-001 — Release notes: "document" fires DOCUMENT; "notes" does not

"Document the release notes" → DOCUMENT fires from "document"; "notes" is content context.
"Publish the release notes" → no predicate; "publish" is not a predicate trigger.
"Distribute the release notes" → no predicate; "distribute" is not a predicate trigger.

Knowledge edge: publishing and distributing release notes is outside the predicate model.
The system recognizes documentation actions ("document", "criteria") but not distribution actions.

---

## B38-0C-016-002 — "Service" in release notes: IMPLEMENT fires

"Document the release notes for the notification service" → ["QA"? → No. Only DOCUMENT from "document" and IMPLEMENT from "service".]

Wait — no QA terms present.
- "Document" → DOCUMENT
- "notification service" → IMPLEMENT
- "review" → not present here

Result: ["IMPLEMENT", "DOCUMENT"] — TransitionType: feature_addition.

But if "review" is added: "Document the release notes for the notification service and review them" → ["QA","IMPLEMENT","DOCUMENT"].

Key lesson: "service" fires IMPLEMENT in release notes sentences just as in any other B38 sentence. The release notes context does not suppress IMPLEMENT.

---

## B38-0C-016-003 — "Go-live day" publication: pure gap

Input: "Publish the release notes on the go-live day and distribute to all stakeholders."

Why knowledge_edge:
- "Publish" → not in predicate lexicon
- "release notes" → "notes" is not a predicate trigger; "release" is not a predicate trigger
- "go-live" → GAP
- "distribute" → not in predicate lexicon
- "stakeholders" → context word

Zero predicates → knowledge_edge.
Capability gap: gap-project-delivery-go-live-001.
