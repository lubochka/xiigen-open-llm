# Layer 0-C — Knowledge-Edge Pairs (027)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B36-0C-027-001 — "epics breakdown" SCHEDULE_CAPABILITY_LEARNING

A:
  CompiledText: "Break down the epics into smaller backlog items for the next iteration."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-epic-001"
    - "gap-project-planning-backlog-001"
    - "gap-project-planning-iteration-001"

---

## B36-0C-027-002 — "WIP limits" + "cycle time" SCHEDULE_CAPABILITY_LEARNING

A:
  CompiledText: "Reduce the WIP limits and track cycle time on the kanban board."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-wip-limits-001"
    - "gap-project-planning-cycle-time-001"
    - "gap-project-planning-kanban-board-001"

---

## B36-0C-027-003 — "burndown" pure gap SCHEDULE_CAPABILITY_LEARNING

A:
  CompiledText: "Present the burndown chart to the team at the end of each sprint."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-burndown-001"
    - "gap-project-planning-sprint-001"

---

## B36-0C-027-004 — New gap IDs introduced in Round 2 (025–036)

A:
  Task: "List all new gap IDs introduced in Round 2."

B:
  NewGapIds_Round2:
    - "gap-project-planning-definition-of-done-001" — definition of done criteria
    - "gap-project-planning-acceptance-criteria-001" — acceptance criteria (note: criteria fires DOCUMENT)
    - "gap-project-planning-user-story-mapping-001" — user story mapping sessions (note: story fires DOCUMENT)
    - "gap-project-planning-sprint-goal-001" — sprint goal setting
    - "gap-project-planning-retrospective-action-items-001" — retro action items
    - "gap-project-planning-kanban-board-001" — kanban board management
    - "gap-project-planning-wip-limits-001" — WIP (Work In Progress) limit management
    - "gap-project-planning-flow-efficiency-001" — flow efficiency measurement
    - "gap-project-planning-cycle-time-001" — cycle time tracking
  AlreadyExisting: "gap-project-planning-burndown-001, gap-project-planning-retrospective-001, gap-project-planning-epic-001, gap-project-planning-sprint-001"
