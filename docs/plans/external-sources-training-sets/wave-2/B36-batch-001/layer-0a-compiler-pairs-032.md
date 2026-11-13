# Layer 0-A — Compiler Pairs (032)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-032-001 — "acceptance criteria" pure gap (noun only)

A:
  run_id: "B36-0A-032-001"
  user_request: "Ensure acceptance criteria are met before marking a backlog item done."
  Task: "Identify predicates. 'criteria' is in DOCUMENT lexicon."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-acceptance-criteria-001"
    - "gap-project-planning-backlog-001"
    - "gap-project-planning-definition-of-done-001"
  Teaching: "criteria" fires DOCUMENT even as a noun in a planning sentence. "ensure" and
    "marking" are neutral. DomainId: "software-development".

---

## B36-0A-032-002 — "epics breakdown" zero-predicate

A:
  run_id: "B36-0A-032-002"
  user_request: "Plan the epics breakdown and map each epic to a sprint milestone."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-epic-001"
    - "gap-project-planning-sprint-001"
    - "gap-project-planning-milestone-001"
  Teaching: "plan" and "map" are neutral tokens. "epics", "sprint", "milestone" are gap
    vocabulary. Zero predicates → knowledge_edge. DomainId: "project-management".

---

## B36-0A-032-003 — "WIP limits" + "implement" fires IMPLEMENT

A:
  run_id: "B36-0A-032-003"
  user_request: "Implement WIP limit validation in the kanban service backend."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-wip-limits-001"
    - "gap-project-planning-kanban-board-001"
  Teaching: "implement" fires IMPLEMENT. "validation" + "service backend" are code entities.
    "WIP limits" and "kanban" are gap vocabulary producing warnings. DomainId: "software-development".

---

## B36-0A-032-004 — "burndown" + "functional" fires DOCUMENT

A:
  run_id: "B36-0A-032-004"
  user_request: "Create a functional burndown chart component for the project dashboard."
  Task: "Identify predicates. 'functional' is in DOCUMENT lexicon."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-burndown-001"
  Teaching: "functional" fires DOCUMENT. "create" + "component" fires IMPLEMENT (code entity).
    Two predicates → multi-predicate output. "burndown" is gap vocabulary. DomainId: "software-development".

---

## B36-0A-032-005 — "cycle time" + "approve" fires QA

A:
  run_id: "B36-0A-032-005"
  user_request: "Approve the cycle time benchmarks for the release."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-cycle-time-001"
    - "gap-project-planning-release-cycle-001"
  Teaching: "approve" fires QA unconditionally. "cycle time" and "release" are gap vocabulary.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING: NOT emitted.
