# Layer 0-C — Knowledge-Edge Pairs (028)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B36-0C-028-001 — DomainId transition for Round 2 vocabulary

A:
  Task: "For each Round 2 term, show the DomainId before and after a predicate is added."

B:
  Transitions:
    - term: "definition of done"
      Version1: "Align on definition of done." → predicates: [] → DomainId: project-management
      Version2: "Review the definition of done." → predicates: ["QA"] → DomainId: software-development
    - term: "sprint goal"
      Version1: "Set the sprint goal." → predicates: [] → DomainId: project-management
      Version2: "Define the sprint goal." → predicates: ["DOCUMENT"] → DomainId: software-development
    - term: "kanban board"
      Version1: "Optimize the kanban board." → predicates: [] → DomainId: project-management
      Version2: "Design the kanban board system." → predicates: ["ARCHITECTURE"] → DomainId: software-development

---

## B36-0C-028-002 — "cycle time" + "retrospective" — one SCHEDULE_CAPABILITY_LEARNING

A:
  CompiledText: "Track cycle time per ticket and present at the retrospective."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-cycle-time-001"
    - "gap-project-planning-ticket-001"
    - "gap-project-planning-retrospective-001"
  Rule: Three gap IDs → one SCHEDULE_CAPABILITY_LEARNING signal (Rule 22).

---

## B36-0C-028-003 — "sprint goal" + "epics" + "definition of done" SCHEDULE_CAPABILITY_LEARNING

A:
  CompiledText: "Plan the sprint: sprint goal, epics breakdown, definition of done."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-sprint-001"
    - "gap-project-planning-sprint-goal-001"
    - "gap-project-planning-epic-001"
    - "gap-project-planning-definition-of-done-001"
  Rule: Four gap IDs → one SCHEDULE_CAPABILITY_LEARNING signal.

---

## B36-0C-028-004 — "flow efficiency" + "burndown" SCHEDULE_CAPABILITY_LEARNING

A:
  CompiledText: "Optimize flow efficiency by addressing bottlenecks identified in the burndown."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-flow-efficiency-001"
    - "gap-project-planning-burndown-001"
