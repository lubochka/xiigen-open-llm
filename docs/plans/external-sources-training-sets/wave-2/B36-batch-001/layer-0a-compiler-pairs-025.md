# Layer 0-A — Compiler Pairs (025)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-025-001 — "definition of done" is gap vocabulary

A:
  run_id: "B36-0A-025-001"
  user_request: "Update our definition of done for the current sprint."
  Task: "Identify predicates and gaps."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-definition-of-done-001"
    - "gap-project-planning-sprint-001"
  Teaching: "definition of done" is planning vocabulary — no predicate match. "update" without
    a code-level entity does not fire IMPLEMENT. Zero predicates → knowledge_edge.

---

## B36-0A-025-002 — "acceptance criteria" fires DOCUMENT

A:
  run_id: "B36-0A-025-002"
  user_request: "Write out the acceptance criteria for the backlog items."
  Task: "Identify predicates. Note: 'criteria' is in the DOCUMENT lexicon."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-backlog-001"
    - "gap-project-planning-acceptance-criteria-001"
  Teaching: "criteria" fires DOCUMENT unconditionally. "acceptance criteria" contains "criteria"
    → DOCUMENT fires. "write" with a document entity also fires DOCUMENT. DomainId: "software-development".

---

## B36-0A-025-003 — "user story mapping" is gap vocabulary

A:
  run_id: "B36-0A-025-003"
  user_request: "Facilitate a user story mapping session for the upcoming epic."
  Task: "Identify predicates. Note: 'story' substring in 'story mapping'."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-user-story-mapping-001"
    - "gap-project-planning-epic-001"
  Teaching: "story" in "user story mapping" fires DOCUMENT (false positive — same rule as
    "story points"). The planning intent (mapping session) does not suppress the predicate.
    DomainId: "software-development" because DOCUMENT fired.

---

## B36-0A-025-004 — "epics breakdown" is pure gap vocabulary

A:
  run_id: "B36-0A-025-004"
  user_request: "Break down the epics into smaller backlog items for the next iteration."
  Task: "Identify predicates and gaps."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-epic-001"
    - "gap-project-planning-backlog-001"
    - "gap-project-planning-iteration-001"
  Teaching: "break down" is not a predicate keyword. "epics", "backlog", "iteration" are gap
    vocabulary. Zero predicates → knowledge_edge. DomainId: "project-management".

---

## B36-0A-025-005 — "sprint goal" is gap vocabulary

A:
  run_id: "B36-0A-025-005"
  user_request: "Define the sprint goal and communicate it to the team before standup."
  Task: "Identify predicates. Note: 'define' is in the DOCUMENT lexicon."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-sprint-goal-001"
    - "gap-project-planning-standup-001"
  Teaching: "define" fires DOCUMENT unconditionally even in planning context. "sprint goal"
    and "standup" are gap vocabulary. DomainId: "software-development" because DOCUMENT fired.
