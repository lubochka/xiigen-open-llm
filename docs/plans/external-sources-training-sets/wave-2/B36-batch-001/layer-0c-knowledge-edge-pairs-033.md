# Layer 0-C — Knowledge-Edge Pairs (033)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B36-0C-033-001 — "sprint goal" + "definition of done" + "review" — no signal

A:
  CompiledText: "Review the sprint goal and definition of done before the sprint starts."
  CompiledPredicateIds: ["QA"]

B:
  OverallVerdict: "ROUTED"
  ScheduleCapabilityLearningSent: 0
  CapabilityGapWarnings: ["gap-project-planning-sprint-goal-001", "gap-project-planning-definition-of-done-001", "gap-project-planning-sprint-001"]

---

## B36-0C-033-002 — "epics breakdown" + "DOCUMENT" — no signal

A:
  CompiledText: "Document the epics breakdown and specify acceptance criteria for each."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  OverallVerdict: "ROUTED"
  ScheduleCapabilityLearningSent: 0
  CapabilityGapWarnings: ["gap-project-planning-epic-001", "gap-project-planning-acceptance-criteria-001"]

---

## B36-0C-033-003 — "cycle time" + "build" — no signal

A:
  CompiledText: "Build a cycle time tracker for the engineering dashboard."
  CompiledPredicateIds: ["IMPLEMENT"]

B:
  OverallVerdict: "ROUTED"
  ScheduleCapabilityLearningSent: 0
  CapabilityGapWarnings: ["gap-project-planning-cycle-time-001"]

---

## B36-0C-033-004 — Round 2 complete KNOWLEDGE_EDGE signal format

A:
  Task: "Show the canonical SCHEDULE_CAPABILITY_LEARNING signal format for a Round 2 request."

B:
  CanonicalFormat:
    signal: "SCHEDULE_CAPABILITY_LEARNING"
    capabilityGapIds: ["gap-project-planning-kanban-board-001", "gap-project-planning-wip-limits-001"]
    emittedAt: "KNOWLEDGE_EDGE state entry"
    consumer: "AutonomousLearningPipeline (subproject 16)"
  Rules:
    - One signal per request regardless of how many gap IDs
    - All gap IDs from the request bundled in one signal
    - Never emitted when any predicate fires
    - DomainId is set to "project-management" when this signal fires
