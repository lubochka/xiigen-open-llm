# Layer 0-A — Compiler Pairs (036)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-036-001 — "complete planning boundary reference" for Round 2 vocabulary

A:
  run_id: "B36-0A-036-001"
  Task: "Complete boundary table for all Round 2 (025–036) gap vocabulary."

B:
  BoundaryTable_Round2:
    GAP only (zero predicates possible):
      - "definition of done" (if no DOCUMENT verb co-occurs)
      - "sprint goal" (if no DOCUMENT/QA/IMPLEMENT verb co-occurs)
      - "retrospective action items" (if no IMPLEMENT code entity co-occurs)
      - "kanban board"
      - "WIP limits" (Work In Progress limits)
      - "flow efficiency"
      - "cycle time"
      - "epics breakdown"
      - "burndown"

    Always fires DOCUMENT (contains lexicon token):
      - "acceptance criteria" (token: "criteria")
      - "user story mapping" (token: "story")
      - "epics + specify/define/document/requirements"
      - "definition of done + specify/define/document"

    Fires DOCUMENT or other predicate by verb:
      - "define X" → DOCUMENT
      - "specify X" → DOCUMENT
      - "review X" → QA
      - "implement X + code entity" → IMPLEMENT
      - "build X + code entity" → IMPLEMENT
      - "design X" → ARCHITECTURE

---

## B36-0A-036-002 — "WIP limits" + "cycle time" + "flow efficiency" mega zero-predicate

A:
  run_id: "B36-0A-036-002"
  user_request: "Monitor WIP limits, cycle time, and flow efficiency metrics on the kanban board."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-wip-limits-001"
    - "gap-project-planning-cycle-time-001"
    - "gap-project-planning-flow-efficiency-001"
    - "gap-project-planning-kanban-board-001"
  Teaching: "monitor" is neutral. All four terms are gap vocabulary. One SCHEDULE_CAPABILITY_LEARNING
    signal emitted with all four IDs bundled. DomainId: "project-management".

---

## B36-0A-036-003 — "definition of done" + "acceptance criteria" compound gap

A:
  run_id: "B36-0A-036-003"
  user_request: "Align the definition of done with the acceptance criteria for the sprint items."
  Task: "Identify predicates. 'criteria' in 'acceptance criteria'."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-definition-of-done-001"
    - "gap-project-planning-acceptance-criteria-001"
    - "gap-project-planning-sprint-001"
  Teaching: "criteria" fires DOCUMENT unconditionally. "align" is neutral. DomainId: "software-development".
    SCHEDULE_CAPABILITY_LEARNING: NOT emitted.

---

## B36-0A-036-004 — "burndown" + "kanban board" + "WIP limits" v6-02 corruption

A:
  run_id: "B36-0A-036-004"
  user_request: "Review the burndown chart and adjust WIP limits on the kanban board."

CORRUPT B (do not reproduce):
  CompiledPredicateIds: []
  Note: "sentence is about planning; review means inspect, not QA"

Why corrupt:
  "'review' fires QA unconditionally. No context suppresses it. v6-02 error: predicate suppressed
   because sentence 'feels like planning'. The correct output has ['QA'] in CompiledPredicateIds."

Correct B:
  CompiledPredicateIds: ["QA"]
  CapabilityGapWarnings: ["gap-project-planning-burndown-001", "gap-project-planning-wip-limits-001", "gap-project-planning-kanban-board-001"]
  DomainId: "software-development"

---

## B36-0A-036-005 — "sprint goal" + "definition of done" + "acceptance criteria" triple gap

A:
  run_id: "B36-0A-036-005"
  user_request: "Set the sprint goal and ensure alignment between the definition of done and acceptance criteria."
  Task: "Identify predicates. 'criteria' in 'acceptance criteria'."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-sprint-goal-001"
    - "gap-project-planning-sprint-001"
    - "gap-project-planning-definition-of-done-001"
    - "gap-project-planning-acceptance-criteria-001"
  Teaching: "criteria" fires DOCUMENT. "set" and "ensure alignment" are neutral. Four gap
    warnings logged. DomainId: "software-development".
