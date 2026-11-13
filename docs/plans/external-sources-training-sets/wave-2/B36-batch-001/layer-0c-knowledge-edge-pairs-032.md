# Layer 0-C — Knowledge-Edge Pairs (032)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B36-0C-032-001 — "burndown" + "kanban" + "WIP" SCHEDULE_CAPABILITY_LEARNING

A:
  CompiledText: "Optimize the kanban board to improve flow efficiency and reduce cycle time."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-kanban-board-001"
    - "gap-project-planning-flow-efficiency-001"
    - "gap-project-planning-cycle-time-001"

---

## B36-0C-032-002 — "WIP limits" + "criteria" — predicate fires — no signal

A:
  CompiledText: "Define WIP limits and specify the criteria for moving tickets between stages."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  OverallVerdict: "ROUTED"
  ScheduleCapabilityLearningSent: 0
  CapabilityGapWarnings: ["gap-project-planning-wip-limits-001", "gap-project-planning-ticket-001"]

---

## B36-0C-032-003 — "cycle time" pure gap SCHEDULE_CAPABILITY_LEARNING

A:
  CompiledText: "Track cycle time per ticket type and present findings in the retrospective."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-cycle-time-001"
    - "gap-project-planning-ticket-001"
    - "gap-project-planning-retrospective-001"

---

## B36-0C-032-004 — Mega gap — 9 terms — one SCHEDULE_CAPABILITY_LEARNING

A:
  CompiledText: "Manage kanban board, track WIP limits, cycle time, flow efficiency, sprint goal, definition of done, burndown, epics breakdown, and retrospective action items."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-kanban-board-001"
    - "gap-project-planning-wip-limits-001"
    - "gap-project-planning-cycle-time-001"
    - "gap-project-planning-flow-efficiency-001"
    - "gap-project-planning-sprint-goal-001"
    - "gap-project-planning-definition-of-done-001"
    - "gap-project-planning-burndown-001"
    - "gap-project-planning-epic-001"
    - "gap-project-planning-retrospective-action-items-001"
  Rule: Nine gap IDs → still ONE SCHEDULE_CAPABILITY_LEARNING signal (Rule 22).
