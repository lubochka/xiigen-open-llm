# Layer 0-C — Knowledge-Edge Pairs (016)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: knowledge-edge-pairs

---

## B37-0C-016-001 — "decision-log" knowledge edge: governance record vocabulary

**Knowledge edge: decision log is project governance gap vocabulary**

A decision log records all significant decisions made during a project — the decision, date, owner, rationale, and alternatives considered. "Decision log" is B37 gap vocabulary with no registered predicate.

**Gap ID:** gap-project-managing-manage-delivery-001 (decision tracking is delivery governance vocabulary)

**Related gap IDs:**
- gap-project-managing-accountability-001 (decisions have owners — accountability)
- gap-project-managing-track-001 (if "track" appears with decision log)

---

## B37-0C-016-002 — "decision-log" boundary: "log" vs "document"

**Knowledge edge: "log" does not fire DOCUMENT; "document" does**

| Token | Predicate fires? | Notes |
|-------|-----------------|-------|
| "log" | No | "log" not in DOCUMENT trigger list |
| "document" | Yes → DOCUMENT | DOCUMENT trigger (Rule 8) |
| "decision log" | No | compound not in predicate lexicon |
| "update the log" | No | "update" not in predicate lexicon |
| "document the decision" | Yes → DOCUMENT | "document" fires DOCUMENT |

**Teaching point:** "Update the decision log" → zero predicates. "Document the decision" → DOCUMENT fires. The difference is the trigger token.

---

## B37-0C-016-003 — "decision-log" with "architectural": DOCUMENT + ARCHITECTURE fire

**Input:** "Document all architectural decisions in the decision log."

Knowledge edge analysis:
- DOCUMENT fires on "document"
- ARCHITECTURE fires on "architectural" (architecture/design/structure family — Rule 7/Rule 20)
- Decision log → gap warning: gap-project-managing-manage-delivery-001

This is the most common multi-predicate pattern for decision log requests: combining architectural documentation with a decision log.

Result: ["DOCUMENT", "ARCHITECTURE"]

---

## B37-0C-016-004 — "decision-log" gap ID reference

**Gap ID:** gap-project-managing-manage-delivery-001

Decision log vocabulary family:
- decision log
- decision register
- decision record (ADR — Architecture Decision Record)
- actions and decisions log
- decisions and actions tracker

**Note on ADRs:** "Architecture Decision Record (ADR)" contains the word "Architecture" which fires ARCHITECTURE. An ADR request will typically produce ["ARCHITECTURE"] or ["ARCHITECTURE", "DOCUMENT"] predicates due to the trigger tokens.

---

## B37-0C-016-005 — Five test cases for decision log

1. "Update the decision log." → [] — DomainId "project-management"
2. "Track decisions in the decision log." → [] — DomainId "project-management"
3. "Review the decision log." → [QA] — DomainId "software-development"
4. "Document all decisions in the decision log." → [DOCUMENT] — DomainId "software-development"
5. "Document all architectural decisions in the decision log." → [DOCUMENT, ARCHITECTURE] — DomainId "software-development"

Cases 1-2: knowledge_edge, SCL emitted.
Cases 3-5: routing to appropriate flows, no SCL.
