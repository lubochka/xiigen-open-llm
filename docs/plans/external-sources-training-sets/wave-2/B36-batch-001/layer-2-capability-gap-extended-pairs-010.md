# Layer 2 — Capability Gap Extended Pairs (010)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: capability_gap_extended

---

## B36-L2-010-001 — mixed: "implement epic features" → IMPLEMENT fires; epic is gap → CapabilityGapWarning trace

**Input:** "Implement the user authentication epic features before the milestone deadline."

**Stage 1 — Compiler:**
- implement: IMPLEMENT fires
- user: no predicate
- authentication: no predicate
- epic: GAP → epic-001
- features: IMPLEMENT fires (feature)
- milestone: GAP → milestone-001
- deadline: no predicate

**Predicate output:** ["IMPLEMENT"]

**CapabilityGapWarnings:** epic-001, milestone-001

**Stage 2 — Router:**
- IMPLEMENT → Phase 4.1
- DomainId: "software-development"
- KNOWLEDGE_EDGE: false

**Stage 3 — Factory:** BackendServiceFlow41

**Stage 4 — Orchestrator:**
- transitionType: feature_addition
- phaseId: "4.1"
- finalHumanResult: populated
- SCHEDULE_CAPABILITY_LEARNING: NOT emitted
- capabilityGapWarnings: epic-001, milestone-001

---

## B36-L2-010-002 — epic-001 in CapabilityGapWarning: what it means in mixed-signal

When epic-001 appears in a CapabilityGapWarning for an IMPLEMENT-routed request, it means:
- The implementation work was successfully routed to Phase 4.1
- The "epic" organizational context is noted but not served
- The orchestrator cannot help the user track whether this implementation satisfies Epic E-X
- The orchestrator cannot update the epic status after implementation

The implementation proceeds normally. The epic tracking gap is informational.

---

## B36-L2-010-003 — milestone-001 in mixed-signal: deadline context

gap-project-planning-milestone-001 in CapabilityGapWarnings for an IMPLEMENT request means:
- The implementation was routed correctly
- The milestone deadline context is noted
- The orchestrator cannot verify whether the implementation will meet the milestone deadline
- The orchestrator cannot flag milestone risk based on implementation complexity

Again, informational only. The implementation routing is unaffected.

---

## B36-L2-010-004 — gap-project-planning-sprint-not-captured: mixed-signal form

When "sprint" appears in a request that also fires IMPLEMENT or another predicate, the mixed-signal gap ID is used:

gap-project-planning-sprint-not-captured

This signals that sprint context was present but not captured by the implementation routing. The implementation will happen, but the sprint planning context (which sprint, sprint velocity impact, sprint capacity) is not served.

**Example:**
Input: "Implement the sprint authentication feature."
Gap IDs: gap-project-planning-sprint-not-captured (mixed-signal form, not sprint-001)
Predicates: ["IMPLEMENT"]
