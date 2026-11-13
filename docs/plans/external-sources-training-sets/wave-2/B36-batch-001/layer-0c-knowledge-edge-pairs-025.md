# Layer 0-C — Knowledge-Edge Pairs (025)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B36-0C-025-001 — "definition of done" SCHEDULE_CAPABILITY_LEARNING

A:
  CompiledText: "Align the team on our definition of done before the sprint kickoff."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-definition-of-done-001"
    - "gap-project-planning-sprint-001"
  Signal:
    signal: "SCHEDULE_CAPABILITY_LEARNING"
    capabilityGapIds: ["gap-project-planning-definition-of-done-001", "gap-project-planning-sprint-001"]

---

## B36-0C-025-002 — "acceptance criteria" does NOT emit SCHEDULE_CAPABILITY_LEARNING

A:
  CompiledText: "Write out the acceptance criteria for the backlog items."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  OverallVerdict: "ROUTED"
  ScheduleCapabilityLearningSent: 0
  KnowledgeEdge: false
  CapabilityGapWarnings: ["gap-project-planning-acceptance-criteria-001", "gap-project-planning-backlog-001"]
  Rule: SCHEDULE_CAPABILITY_LEARNING is NOT emitted. "criteria" fired DOCUMENT → predicate present.

---

## B36-0C-025-003 — "user story mapping" does NOT emit SCHEDULE_CAPABILITY_LEARNING

A:
  CompiledText: "Facilitate a user story mapping session for the upcoming epic."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  OverallVerdict: "ROUTED"
  ScheduleCapabilityLearningSent: 0
  KnowledgeEdge: false
  CapabilityGapWarnings: ["gap-project-planning-user-story-mapping-001", "gap-project-planning-epic-001"]
  Rule: "story" fires DOCUMENT → predicate present → no SCHEDULE_CAPABILITY_LEARNING.
    Even though the intent is planning, the predicate fired.

---

## B36-0C-025-004 — "sprint goal" SCHEDULE_CAPABILITY_LEARNING

A:
  CompiledText: "Set the sprint goal and track progress toward it."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-sprint-goal-001"
    - "gap-project-planning-sprint-001"
  Signal:
    signal: "SCHEDULE_CAPABILITY_LEARNING"
    capabilityGapIds: ["gap-project-planning-sprint-goal-001", "gap-project-planning-sprint-001"]
