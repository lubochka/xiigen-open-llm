# Layer 0-C — Knowledge-Edge Pairs (031)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B36-0C-031-001 — "epics breakdown" SCHEDULE_CAPABILITY_LEARNING

A:
  CompiledText: "Plan the epics breakdown and map each epic to a sprint milestone."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-epic-001"
    - "gap-project-planning-sprint-001"
    - "gap-project-planning-milestone-001"

---

## B36-0C-031-002 — "sprint goal" with "define" — no SCHEDULE_CAPABILITY_LEARNING

A:
  CompiledText: "Define the sprint goal and communicate it to the team before standup."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  OverallVerdict: "ROUTED"
  ScheduleCapabilityLearningSent: 0
  KnowledgeEdge: false
  CapabilityGapWarnings: ["gap-project-planning-sprint-goal-001", "gap-project-planning-standup-001"]

---

## B36-0C-031-003 — "definition of done" (noun only) — SCHEDULE_CAPABILITY_LEARNING

A:
  Task: "The sentence uses 'definition' as a noun, not 'define' as a verb."
  CompiledText: "Revisit the definition of done for the current sprint."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-definition-of-done-001"
    - "gap-project-planning-sprint-001"
  Teaching: "definition" (noun) does NOT fire DOCUMENT. Only the verb "define" fires DOCUMENT.
    "definition of done" as a compound noun is gap vocabulary.

---

## B36-0C-031-004 — "kanban board" pure gap SCHEDULE_CAPABILITY_LEARNING

A:
  CompiledText: "Move overdue tickets to the blocked column on the kanban board."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-ticket-001"
    - "gap-project-planning-kanban-board-001"
