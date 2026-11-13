# Layer 0-A — Compiler Pairs (041)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-041-001 — "sprint demo" + "review" fires QA

A:
  run_id: "B36-0A-041-001"
  user_request: "Review the sprint demo outputs before the final release decision."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-sprint-demo-001"
    - "gap-project-planning-sprint-001"
  Teaching: "review" fires QA. "sprint demo" is gap vocabulary. DomainId: "software-development".
    SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-041-002 — "backlog refinement" + "define" fires DOCUMENT

A:
  run_id: "B36-0A-041-002"
  user_request: "Define the acceptance criteria during backlog refinement."
  Task: "Identify predicates. 'criteria' and 'define' tokens."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-backlog-001"
    - "gap-project-planning-refinement-001"
    - "gap-project-planning-acceptance-criteria-001"
  Teaching: "define" fires DOCUMENT. "criteria" also fires DOCUMENT (same predicate, second trigger).
    Both are in the DOCUMENT lexicon. DomainId: "software-development".
    SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-041-003 — "scrum ceremony" + "facilitate" pure gap

A:
  run_id: "B36-0A-041-003"
  user_request: "Facilitate all four scrum ceremonies during the two-week iteration."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-scrum-ceremony-001"
    - "gap-project-planning-iteration-001"
  Teaching: "facilitate" is neutral. Four scrum ceremonies and iteration are gap vocabulary.
    DomainId: "project-management". SCHEDULE_CAPABILITY_LEARNING emitted.

---

## B36-0A-041-004 — "launch readiness" + "test" fires TEST

A:
  run_id: "B36-0A-041-004"
  user_request: "Test the launch readiness checklist items for the payment service."
  Task: "Identify predicates. 'test' token."

B:
  CompiledPredicateIds: ["TEST"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-launch-readiness-001"
    - "gap-project-planning-go-live-checklist-001"
  Teaching: "test" fires TEST unconditionally. "launch readiness" is gap vocabulary.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-041-005 — "MVP definition" + "implement" fires IMPLEMENT

A:
  run_id: "B36-0A-041-005"
  user_request: "Implement the core features listed in the MVP definition."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-mvp-001"
  Teaching: "implement" fires IMPLEMENT. "core features" is a code entity in context.
    "MVP definition" is gap vocabulary. DomainId: "software-development".
    SCHEDULE_CAPABILITY_LEARNING NOT emitted.
