# Layer 2 — Capability Gap Extended Pairs (002)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: capability_gap_extended

---

## B36-L2-002-001 — iteration/epic/ticket vocabulary: all gap

**Input:** "Plan the iteration with epic tickets and assign story points."

**Full knowledge_edge trace:**

**Stage 1 — Compiler:**
- plan: no predicate (Rule 17)
- iteration: GAP → iteration-001
- epic: GAP → epic-001
- tickets: GAP → ticket-001
- assign: no predicate
- story: DOCUMENT fires → predicate fires!

**Wait — "story" fires DOCUMENT. This is not a pure gap case.**

**Revised input to get pure gap:** "Plan the iteration with epic items and assign estimates."

**Stage 1 — Compiler (revised):**
- plan: no predicate
- iteration: GAP → iteration-001
- epic: GAP → epic-001
- items: no predicate
- assign: no predicate
- estimates: no predicate

**Predicate output:** []

**Stage 2 — Router:** KNOWLEDGE_EDGE path
**Stage 3 — Factory:** not called
**Stage 4 — Orchestrator:**
- TransitionType: unknown
- DomainId: "project-management"
- FinalHumanResult: null
- SCHEDULE_CAPABILITY_LEARNING emitted with capabilityGapIds: [iteration-001, epic-001]

---

## B36-L2-002-002 — "plan the iteration with epic tickets" — distinguishing "tickets" from "items"

"Tickets" is gap vocabulary (ticket-001). It would be emitted as a CapabilityGapWarning. "Items" is neutral. Using "items" instead of "tickets" in a sentence does not change the gap nature — the user still means work items in a backlog/epic.

**Key point:** The gap IDs represent the planning vocabulary terms that the compiler cannot handle. Both "tickets" and "items" in an epic context represent the same planning concept. But only "tickets" is registered as gap vocabulary (ticket-001). "Items" is neutral — not gap, not a predicate.

---

## B36-L2-002-003 — iteration/epic/ticket triple gap

**Input:** "Break the epic into tickets and assign them to the iteration."

**Gap IDs emitted:**
- gap-project-planning-epic-001
- gap-project-planning-ticket-001
- gap-project-planning-iteration-001

**KNOWLEDGE_EDGE trace:**
- All tokens: no predicate
- predicates: []
- KNOWLEDGE_EDGE: true
- DomainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING: one signal, all three gap IDs bundled

---

## B36-L2-002-004 — gap vocabulary completeness for iteration/epic/ticket

All three terms (iteration, epic, ticket) represent core agile project management concepts. None has a registered predicate. None triggers any lexicon match except as gap vocabulary. They appear frequently together in project management sentences and are handled identically: gap warning + KNOWLEDGE_EDGE.
