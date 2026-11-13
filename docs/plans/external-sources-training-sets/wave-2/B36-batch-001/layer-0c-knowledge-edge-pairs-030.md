# Layer 0-C — Knowledge-Edge Pairs (030)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B36-0C-030-001 — "flow efficiency" + "WIP limits" + "cycle time" mega-gap signal

A:
  CompiledText: "Monitor WIP limits, cycle time, and flow efficiency metrics on the kanban board."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-wip-limits-001"
    - "gap-project-planning-cycle-time-001"
    - "gap-project-planning-flow-efficiency-001"
    - "gap-project-planning-kanban-board-001"
  Rule: Four gap IDs → one signal with all four IDs bundled (Rule 22).

---

## B36-0C-030-002 — "definition of done" + "sprint goal" SCHEDULE_CAPABILITY_LEARNING

A:
  CompiledText: "Align sprint goal with the definition of done for the current iteration."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-sprint-goal-001"
    - "gap-project-planning-definition-of-done-001"
    - "gap-project-planning-sprint-001"
    - "gap-project-planning-iteration-001"

---

## B36-0C-030-003 — "retrospective action items" with IMPLEMENT — no signal

A:
  CompiledText: "The retrospective action item is to build an automated deployment pipeline."
  CompiledPredicateIds: ["IMPLEMENT"]

B:
  OverallVerdict: "ROUTED"
  ScheduleCapabilityLearningSent: 0
  KnowledgeEdge: false
  CapabilityGapWarnings: ["gap-project-planning-retrospective-001"]
  Rule: IMPLEMENT fired → no SCHEDULE_CAPABILITY_LEARNING.

---

## B36-0C-030-004 — Round 2 complete SCHEDULE_CAPABILITY_LEARNING rule summary

A:
  Task: "Which Round 2 gap terms always suppress SCHEDULE_CAPABILITY_LEARNING?"

B:
  AlwaysSuppressed (predicate always fires):
    - "acceptance criteria" → "criteria" fires DOCUMENT → no signal
    - "user story mapping" → "story" fires DOCUMENT → no signal
  MaySuppressed (predicate fires when verb co-occurs):
    - "definition of done" + define/review/document → suppressed
    - "sprint goal" + define/review/implement → suppressed
    - "retrospective action items" + build → suppressed
    - "kanban board" + design/test/implement → suppressed
    - "WIP limits" + implement/approve/review → suppressed
    - "flow efficiency" + implement/design → suppressed
    - "cycle time" + approve/build → suppressed
    - "burndown" + review/functional → suppressed
  NeverSuppressed (no predicate token exists):
    - "epics breakdown" alone
    - "retrospective action items" alone (no predicate verb)
