# Layer 0-A — Compiler Pairs (028)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-028-001 — "definition of done" + predicate mix

A:
  run_id: "B36-0A-028-001"
  user_request: "Review the definition of done and update it for the next sprint."
  Task: "Identify predicates. 'review' is in QA lexicon; 'update' check for code entity."

B:
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-definition-of-done-001"
    - "gap-project-planning-sprint-001"
  Teaching: "review" fires QA unconditionally. "update" without a code entity does not fire
    IMPLEMENT. DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING: NOT emitted.

---

## B36-0A-028-002 — "sprint goal" + IMPLEMENT

A:
  run_id: "B36-0A-028-002"
  user_request: "Set the sprint goal and build the authentication module to meet it."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-sprint-goal-001"
    - "gap-project-planning-sprint-001"
  Teaching: "build" fires IMPLEMENT; "authentication module" is the code entity. Sprint goal
    is gap vocabulary. DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING: NOT emitted.

---

## B36-0A-028-003 — "kanban board" + ARCHITECTURE

A:
  run_id: "B36-0A-028-003"
  user_request: "Design the kanban board structure for the engineering team's workflow."
  Task: "Identify predicates. 'design' is in the ARCHITECTURE lexicon."

B:
  CompiledPredicateIds: ["ARCHITECTURE"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-kanban-board-001"
  Teaching: "design" fires ARCHITECTURE unconditionally. "structure" also fires ARCHITECTURE.
    Two ARCHITECTURE triggers → one ARCHITECTURE predicate output. DomainId: "software-development".

---

## B36-0A-028-004 — "WIP limits" pure vs mixed

A:
  run_id: "B36-0A-028-004"
  Task: "Two versions: pure WIP limits request vs WIP limits + IMPLEMENT."

B:
  Version1:
    request: "Analyze and adjust WIP limits across all kanban swim lanes."
    predicates: []
    gap_warnings: ["gap-project-planning-wip-limits-001", "gap-project-planning-kanban-board-001"]
    DomainId: "project-management"
  Version2:
    request: "Implement the WIP limit enforcement logic in the project tracking system."
    predicates: ["IMPLEMENT"]
    gap_warnings: ["gap-project-planning-wip-limits-001"]
    DomainId: "software-development"
  Teaching: "analyze" and "adjust" are neutral. "implement" + "logic" fires IMPLEMENT.
    Same planning concept (WIP limits) — predicate presence drives DomainId.

---

## B36-0A-028-005 — "flow efficiency" + "cycle time" co-occurrence

A:
  run_id: "B36-0A-028-005"
  user_request: "Calculate flow efficiency and average cycle time for the past two sprints and present in retrospective."
  Task: "Identify predicates and gaps."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-flow-efficiency-001"
    - "gap-project-planning-cycle-time-001"
    - "gap-project-planning-sprint-001"
    - "gap-project-planning-retrospective-001"
  Teaching: "calculate", "average", "present" are neutral tokens. All four gap terms fire
    gap warnings. One SCHEDULE_CAPABILITY_LEARNING signal emitted with all four IDs bundled.
    DomainId: "project-management".
