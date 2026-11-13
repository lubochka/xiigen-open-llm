# Layer 0-C — Knowledge-Edge Pairs (014)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: knowledge-edge-pairs

---

## B37-0C-014-001 — "change-request" knowledge edge: change management vocabulary

**Knowledge edge: change request is project managing gap vocabulary**

Change requests are project management artefacts — formal requests to change scope, schedule, budget, or resources. "Change request" and related vocabulary (change log, change advisory board, change freeze) are B37 gap vocabulary with no registered predicate.

**Gap ID:** gap-project-managing-manage-delivery-001 (change request is delivery management vocabulary)

**Zero-predicate scenarios:**
- "Process the change request" → []
- "Coordinate the change request" → []
- "Approve the change request" → []
- "Track open change requests" → [] (track ≠ TEST)
- "Escalate the change request" → [] (escalate not in predicate lexicon)

---

## B37-0C-014-002 — "change-request" boundary: process ≠ IMPLEMENT

**Knowledge edge: "process" is not an IMPLEMENT trigger**

IMPLEMENT fires on: build, implement, create, write (code), service, module.
"Process" is NOT on this list.

| Input | Predicates | Reason |
|-------|-----------|--------|
| "Process the change request" | [] | "process" not in IMPLEMENT lexicon |
| "Create the change request form" | [IMPLEMENT] | "create" fires IMPLEMENT |
| "Build the change request system" | [IMPLEMENT] | "build" fires IMPLEMENT |
| "Implement the change request" | [IMPLEMENT] | "implement" fires IMPLEMENT |
| "Track the change request" | [] | "track" ≠ TEST, not in any lexicon |

---

## B37-0C-014-003 — "change-request" in DOCUMENT context

**Input:** "Document the change request with full impact analysis."

**Knowledge edge analysis:**
- DOCUMENT fires on "document"
- "change request" → gap warning: gap-project-managing-manage-delivery-001
- Intent: change management record (managing intent)
- Routing: DocumentationFlow (2.1)
- Change management process intent: NOT addressed

**Teaching point:** "Documenting a change request" is a project management activity. The compiler fires DOCUMENT regardless of the managing intent. The routing addresses documentation only.

---

## B37-0C-014-004 — "change-request" knowledge edge gap ID

**Gap ID confirmation:** gap-project-managing-manage-delivery-001

Change request vocabulary maps to the manage-delivery gap because change management is a delivery management activity — coordinating changes to what gets delivered, when, and at what cost.

**Vocabulary family:**
- change request
- change advisory board (CAB)
- change log
- change freeze
- change control
- change impact analysis

All map to: gap-project-managing-manage-delivery-001

---

## B37-0C-014-005 — Five zero-predicate change-request test cases

1. "Submit the change request for approval." → []
2. "Track all open change requests." → []
3. "Coordinate the change advisory board meeting." → []
4. "Escalate the overdue change request to the programme manager." → []
5. "Process all change requests before the delivery milestone." → []

All five: DomainId "project-management", knowledge_edge, SCL emitted, gap-project-managing-manage-delivery-001 (and coordinate-001 / escalation-001 / track-001 as appropriate).
