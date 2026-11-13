# Layer 2 — Capability Gap Extended Pairs (008)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: capability_gap_extended

---

## B36-L2-008-001 — sprint backlog grooming: full knowledge_edge trace

**Input:** "Groom the sprint backlog and reorder tickets by priority for the next iteration."

**Stage 1 — Compiler:**
- groom: no predicate
- sprint: GAP → sprint-001
- backlog: GAP → backlog-001
- reorder: no predicate
- tickets: GAP → ticket-001
- priority: no predicate
- iteration: GAP → iteration-001

**Predicate output:** []

**Stage 2 — Router:**
- predicates: [] → KNOWLEDGE_EDGE
- CapabilityGapWarnings: sprint-001, backlog-001, ticket-001, iteration-001

**Stage 3 — Factory:** not called

**Stage 4 — Orchestrator:**
- knowledgeEdge: true
- transitionType: unknown
- phaseId: null
- domainId: "project-management"
- finalHumanResult: null
- SCHEDULE_CAPABILITY_LEARNING:
  ```json
  {
    "signal": "SCHEDULE_CAPABILITY_LEARNING",
    "capabilityGapIds": ["gap-project-planning-sprint-001", "gap-project-planning-backlog-001", "gap-project-planning-ticket-001", "gap-project-planning-iteration-001"]
  }
  ```

---

## B36-L2-008-002 — sprint backlog grooming gap: what it represents

The backlog grooming gap (backlog-001 + sprint-001 + ticket-001 + iteration-001) represents the orchestrator's inability to:
- Prioritize backlog items for a sprint
- Split or refine backlog tickets
- Estimate effort for backlog grooming sessions
- Sequence work items based on business value, dependencies, or team capacity

These activities require deep knowledge of the project context — not available to the general orchestrator.

---

## B36-L2-008-003 — "prioritize" vs "prioritize tickets": same gap behavior

Whether the user says "prioritize the backlog" or "prioritize the tickets" or "prioritize backlog items by sprint priority," the result is the same: no predicates fire, KNOWLEDGE_EDGE, gap warnings for backlog/sprint/ticket.

"Prioritize" is not in any predicate lexicon. It does not fire IMPLEMENT, DOCUMENT, QA, or ARCHITECTURE. It is simply a neutral verb in the compiler's perspective.

---

## B36-L2-008-004 — grooming session frequency and gap accumulation

Sprint backlog grooming is typically done before each sprint. This means gap-project-planning-backlog-001 and gap-project-planning-sprint-001 will appear frequently in SCHEDULE_CAPABILITY_LEARNING signals. The AutonomousLearningPipeline tracks frequency. High-frequency gaps are prioritized for capability development.

The combination of backlog + sprint + ticket in grooming context is one of the most frequent B36 gap patterns and represents a high-priority capability development target.
