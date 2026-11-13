# Layer 0-A — Compiler Pairs (034)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-034-001 — "sprint goal" + "requirements" fires DOCUMENT

A:
  run_id: "B36-0A-034-001"
  user_request: "Document the sprint goal requirements and share with the product team."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-sprint-goal-001"
    - "gap-project-planning-sprint-001"
  Teaching: "document" fires DOCUMENT. "requirements" fires DOCUMENT. Two triggers → one
    DOCUMENT output. "sprint goal" is gap vocabulary. DomainId: "software-development".

---

## B36-0A-034-002 — "retrospective action items" zero-predicate list

A:
  run_id: "B36-0A-034-002"
  user_request: "List the retrospective action items from last sprint and assign priorities."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-retrospective-action-items-001"
    - "gap-project-planning-retrospective-001"
    - "gap-project-planning-sprint-001"
  Teaching: "list", "assign priorities" (planning context, no code entity) are neutral.
    Zero predicates → knowledge_edge. DomainId: "project-management".

---

## B36-0A-034-003 — "cycle time" pure gap comparison

A:
  run_id: "B36-0A-034-003"
  Task: "Compile: 'Measure cycle time' vs 'Build a cycle time tracker'."

B:
  Version1:
    request: "Measure cycle time for each ticket type."
    predicates: []
    gap_warnings: ["gap-project-planning-cycle-time-001", "gap-project-planning-ticket-001"]
    DomainId: "project-management"
  Version2:
    request: "Build a cycle time tracker for the engineering dashboard."
    predicates: ["IMPLEMENT"]
    gap_warnings: ["gap-project-planning-cycle-time-001"]
    DomainId: "software-development"
  Teaching: "measure" is neutral; "build" fires IMPLEMENT. Same planning concept — different
    verb determines predicate presence.

---

## B36-0A-034-004 — "epics breakdown" + "architecture" fires ARCHITECTURE

A:
  run_id: "B36-0A-034-004"
  user_request: "Document the architecture for the epics breakdown into microservices."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-epic-001"
  Teaching: "document" fires DOCUMENT. "architecture" fires ARCHITECTURE. Two predicates fired.
    "epics breakdown" is gap vocabulary. DomainId: "software-development".

---

## B36-0A-034-005 — "flow efficiency" + "burndown" zero-predicate summary

A:
  run_id: "B36-0A-034-005"
  user_request: "Optimize flow efficiency by addressing bottlenecks identified in the burndown."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-flow-efficiency-001"
    - "gap-project-planning-burndown-001"
  Teaching: "optimize", "addressing", "identified" are neutral. Both "flow efficiency" and
    "burndown" are gap vocabulary. Zero predicates → knowledge_edge. DomainId: "project-management".
