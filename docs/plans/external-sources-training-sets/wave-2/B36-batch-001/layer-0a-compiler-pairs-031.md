# Layer 0-A — Compiler Pairs (031)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-031-001 — "definition of done" + "behavior" fires DOCUMENT

A:
  run_id: "B36-0A-031-001"
  user_request: "Update the definition of done to include expected behavior for each deliverable."
  Task: "Identify predicates. 'behavior' is in the DOCUMENT lexicon."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-definition-of-done-001"
  Teaching: "behavior" fires DOCUMENT unconditionally. "update" without code entity does not
    fire IMPLEMENT. "definition of done" is gap vocabulary. DomainId: "software-development".

---

## B36-0A-031-002 — "flow efficiency" + "implement" fires IMPLEMENT

A:
  run_id: "B36-0A-031-002"
  user_request: "Implement flow efficiency metrics reporting in the dashboard."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-flow-efficiency-001"
  Teaching: "implement" fires IMPLEMENT. "metrics reporting" + "dashboard" are code entities.
    "flow efficiency" is gap vocabulary that produces a gap warning. DomainId: "software-development".

---

## B36-0A-031-003 — "sprint goal" zero-predicate formulation

A:
  run_id: "B36-0A-031-003"
  user_request: "Set the sprint goal, share it with the team, and track progress toward it."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-sprint-goal-001"
    - "gap-project-planning-sprint-001"
  Teaching: "set", "share", "track" are neutral tokens. "sprint goal" and "sprint" are gap
    vocabulary. Zero predicates → knowledge_edge. DomainId: "project-management".

---

## B36-0A-031-004 — "kanban board" + "test" fires TEST

A:
  run_id: "B36-0A-031-004"
  user_request: "Test the kanban board integration with our project management API."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["TEST"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-kanban-board-001"
  Teaching: "test" fires TEST unconditionally. "kanban board" is gap vocabulary. "integration"
    with "API" is a code entity context. DomainId: "software-development".

---

## B36-0A-031-005 — All Round 2 gap terms in one zero-predicate sentence

A:
  run_id: "B36-0A-031-005"
  user_request: "During the sprint, maintain the definition of done, track the sprint goal, run user story mapping, break down epics, manage WIP limits on the kanban board, measure cycle time, flow efficiency, and present the burndown chart in the retrospective."
  Task: "Identify predicates. Note: no predicate keywords present."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-sprint-001"
    - "gap-project-planning-definition-of-done-001"
    - "gap-project-planning-sprint-goal-001"
    - "gap-project-planning-user-story-mapping-001"
    - "gap-project-planning-epic-001"
    - "gap-project-planning-wip-limits-001"
    - "gap-project-planning-kanban-board-001"
    - "gap-project-planning-cycle-time-001"
    - "gap-project-planning-flow-efficiency-001"
    - "gap-project-planning-burndown-001"
    - "gap-project-planning-retrospective-001"
  SCHEDULE_CAPABILITY_LEARNING: emitted once with all 11 gap IDs bundled.
  Teaching: "maintain", "track", "run", "break down", "manage", "measure", "present" are all
    neutral. Note "story" in "user story mapping" would fire DOCUMENT — but it is NOT present
    as isolated "story"; the compound "user story mapping" does contain "story" so DOCUMENT
    WOULD fire. Correction: This sentence contains "story" in "user story mapping" → DOCUMENT fires.

  Corrected B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  DomainId: "software-development"
  Teaching: "story" inside "user story mapping" fires DOCUMENT. There is no suppression for
    compound planning nouns. Lexicon matching is positional, not semantic.
