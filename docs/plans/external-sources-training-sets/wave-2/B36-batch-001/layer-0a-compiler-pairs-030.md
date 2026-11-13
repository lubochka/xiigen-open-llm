# Layer 0-A — Compiler Pairs (030)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-030-001 — "epics breakdown" + DOCUMENT

A:
  run_id: "B36-0A-030-001"
  user_request: "Document the epics breakdown and specify acceptance criteria for each."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-epic-001"
    - "gap-project-planning-acceptance-criteria-001"
  Teaching: "document" fires DOCUMENT. "specify" fires DOCUMENT. "criteria" fires DOCUMENT.
    Three DOCUMENT triggers → one DOCUMENT predicate output. Gap warnings logged. DomainId: "software-development".

---

## B36-0A-030-002 — "sprint goal" + "stakeholder" fires DOCUMENT

A:
  run_id: "B36-0A-030-002"
  user_request: "Communicate the sprint goal to all stakeholders before the kickoff."
  Task: "Identify predicates. 'stakeholder' is in the DOCUMENT lexicon."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-sprint-goal-001"
    - "gap-project-planning-sprint-001"
  Teaching: "stakeholder" fires DOCUMENT unconditionally. "communicate" is neutral. "sprint goal"
    and "sprint" are gap vocabulary. DomainId: "software-development".

---

## B36-0A-030-003 — "kanban board" + "flow efficiency" zero-predicate

A:
  run_id: "B36-0A-030-003"
  user_request: "Optimize the kanban board to improve flow efficiency and reduce cycle time."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-kanban-board-001"
    - "gap-project-planning-flow-efficiency-001"
    - "gap-project-planning-cycle-time-001"
  Teaching: "optimize", "improve", "reduce" are neutral tokens. All three terms are gap
    vocabulary. Zero predicates → knowledge_edge. DomainId: "project-management".

---

## B36-0A-030-004 — "WIP limits" + "criteria" fires DOCUMENT

A:
  run_id: "B36-0A-030-004"
  user_request: "Define WIP limits and specify the criteria for moving tickets between stages."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-wip-limits-001"
    - "gap-project-planning-ticket-001"
  Teaching: "define" fires DOCUMENT. "specify" fires DOCUMENT. "criteria" fires DOCUMENT.
    Three DOCUMENT triggers → one DOCUMENT predicate output. "WIP limits" and "ticket" are gap vocabulary.

---

## B36-0A-030-005 — "burndown" zero-predicate (pure observation)

A:
  run_id: "B36-0A-030-005"
  user_request: "Present the burndown chart to the team at the end of each sprint."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-burndown-001"
    - "gap-project-planning-sprint-001"
  Teaching: "present" is not a predicate keyword. "burndown" and "sprint" are gap vocabulary.
    Zero predicates → knowledge_edge. DomainId: "project-management".
