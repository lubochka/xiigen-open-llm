# Layer 0-A — Compiler Pairs (026)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-026-001 — "retrospective action items" gap vs IMPLEMENT

A:
  run_id: "B36-0A-026-001"
  user_request: "Capture the retrospective action items and assign owners."
  Task: "Identify predicates. Note: no code entity alongside 'assign'."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-retrospective-001"
    - "gap-project-planning-retrospective-action-items-001"
  Teaching: "capture" and "assign" (non-code context) are not predicate keywords. "retrospective
    action items" is gap vocabulary. Zero predicates → knowledge_edge.

---

## B36-0A-026-002 — "retrospective action item = build" fires IMPLEMENT

A:
  run_id: "B36-0A-026-002"
  user_request: "The retrospective action item is to build an automated deployment pipeline."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-retrospective-001"
  Teaching: "build" fires IMPLEMENT unconditionally. The retrospective framing does not suppress
    the predicate. "automated deployment pipeline" is a code entity. DomainId: "software-development".

---

## B36-0A-026-003 — "kanban board" is gap vocabulary

A:
  run_id: "B36-0A-026-003"
  user_request: "Set up the kanban board columns and configure the WIP limits."
  Task: "Identify predicates. 'set up' and 'configure' — check if code entity co-occurs."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-kanban-board-001"
    - "gap-project-planning-wip-limits-001"
  Teaching: "set up" and "configure" without a code-level entity (function/class/module) do not
    fire IMPLEMENT. "kanban board" and "WIP limits" (Work In Progress limits) are gap vocabulary.
    Zero predicates → knowledge_edge.

---

## B36-0A-026-004 — "flow efficiency" is gap vocabulary

A:
  run_id: "B36-0A-026-004"
  user_request: "Measure flow efficiency and cycle time across the kanban board."
  Task: "Identify predicates and gaps."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-flow-efficiency-001"
    - "gap-project-planning-cycle-time-001"
    - "gap-project-planning-kanban-board-001"
  Teaching: "measure" is not a predicate keyword. "flow efficiency", "cycle time", and "kanban
    board" are gap vocabulary. Zero predicates → knowledge_edge. DomainId: "project-management".

---

## B36-0A-026-005 — "burndown" pure gap

A:
  run_id: "B36-0A-026-005"
  user_request: "Generate the sprint burndown chart and share it with stakeholders."
  Task: "Identify predicates. Note: 'stakeholders' is in the DOCUMENT lexicon."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-burndown-001"
    - "gap-project-planning-sprint-001"
  Teaching: "stakeholder" fires DOCUMENT unconditionally. "generate" alone does not fire any
    predicate. "burndown" and "sprint" are gap vocabulary. DomainId: "software-development" because
    DOCUMENT fired via "stakeholders".
