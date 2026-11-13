# Layer 0-C — Knowledge-Edge Pairs (034)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B36-0C-034-001 — "burndown" pure gap signal

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

## B36-0C-034-002 — "sprint goal" pure gap signal

A:
  CompiledText: "Set the sprint goal, share it with the team, and track progress toward it."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-sprint-goal-001"
    - "gap-project-planning-sprint-001"

---

## B36-0C-034-003 — "flow efficiency" + "burndown" signal

A:
  CompiledText: "Optimize flow efficiency by addressing bottlenecks identified in the burndown."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-flow-efficiency-001"
    - "gap-project-planning-burndown-001"

---

## B36-0C-034-004 — "kanban board" + "retrospective" signal

A:
  CompiledText: "Move overdue tickets on the kanban board and discuss in the retrospective."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-ticket-001"
    - "gap-project-planning-kanban-board-001"
    - "gap-project-planning-retrospective-001"
