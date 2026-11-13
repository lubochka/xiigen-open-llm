# Layer 3 — Teaching Unit Pairs (016)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## B38-TU-016-001 — Teaching: "document" fires DOCUMENT (not QA)

LESSON: The verb "document" fires DOCUMENT, not QA.

"Document the release notes" → ["DOCUMENT"] (if no QA terms present; "sign off" would add QA)
"Review the release notes" → ["QA"] (review fires QA)
"Document the release notes and sign off" → ["QA","DOCUMENT"] (both fire)

"Document" is the clearest DOCUMENT predicate trigger. It is a direct action: "write this content".
"Review", "verify", "approve" fire QA (checking/approving content).
These are separate predicates serving separate purposes.

---

## B38-TU-016-002 — Teaching: "service" in release notes context still fires IMPLEMENT

LESSON: The phrase "release notes for the notification service" contains "service" → IMPLEMENT fires.

"Document the release notes for v3.2" → ["DOCUMENT"] (no service/QA terms)
"Document the release notes for the notification service" → ["IMPLEMENT","DOCUMENT"] ("service" forces IMPLEMENT)
"Document the release notes for the notification service and review" → ["QA","IMPLEMENT","DOCUMENT"]

This is rule 2 applied to the release notes context.
The user's intent is to document release notes, not to implement code.
But "service" is a literal code-signal that fires IMPLEMENT regardless of intent.

---

## B38-TU-016-003 — Teaching: "publish" and "distribute" have no predicates

LESSON: Distributing or publishing documentation is outside the predicate model.

"Publish the release notes" → no predicate
"Distribute the release notes" → no predicate
"Send the release notes to stakeholders" → no predicate

The system handles: authoring documentation (DOCUMENT), reviewing it (QA).
The system does not handle: publishing or distributing finalized documents.

If the only action is publication/distribution with no service/review/criteria vocabulary → knowledge_edge.
