# Layer 0-C — Knowledge-Edge Pairs (026)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B36-0C-026-001 — "kanban board" + "WIP limits" SCHEDULE_CAPABILITY_LEARNING

A:
  CompiledText: "Set up the kanban board columns and configure the WIP limits."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-kanban-board-001"
    - "gap-project-planning-wip-limits-001"
  Signal:
    signal: "SCHEDULE_CAPABILITY_LEARNING"
    capabilityGapIds: ["gap-project-planning-kanban-board-001", "gap-project-planning-wip-limits-001"]

---

## B36-0C-026-002 — "flow efficiency" + "cycle time" SCHEDULE_CAPABILITY_LEARNING

A:
  CompiledText: "Measure flow efficiency and cycle time across the kanban board."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-flow-efficiency-001"
    - "gap-project-planning-cycle-time-001"
    - "gap-project-planning-kanban-board-001"
  Signal:
    signal: "SCHEDULE_CAPABILITY_LEARNING"
    capabilityGapIds: ["gap-project-planning-flow-efficiency-001", "gap-project-planning-cycle-time-001", "gap-project-planning-kanban-board-001"]

---

## B36-0C-026-003 — "burndown" with "stakeholder" does NOT emit SCHEDULE_CAPABILITY_LEARNING

A:
  CompiledText: "Generate the sprint burndown chart and share it with stakeholders."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  OverallVerdict: "ROUTED"
  ScheduleCapabilityLearningSent: 0
  KnowledgeEdge: false
  CapabilityGapWarnings: ["gap-project-planning-burndown-001", "gap-project-planning-sprint-001"]
  Rule: "stakeholder" fires DOCUMENT → predicate present → no SCHEDULE_CAPABILITY_LEARNING.

---

## B36-0C-026-004 — "retrospective action items" SCHEDULE_CAPABILITY_LEARNING

A:
  CompiledText: "Capture the retrospective action items and assign owners."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-retrospective-001"
    - "gap-project-planning-retrospective-action-items-001"
  Signal:
    signal: "SCHEDULE_CAPABILITY_LEARNING"
    capabilityGapIds: ["gap-project-planning-retrospective-001", "gap-project-planning-retrospective-action-items-001"]
