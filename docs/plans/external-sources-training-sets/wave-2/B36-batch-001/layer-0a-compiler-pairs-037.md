# Layer 0-A — Compiler Pairs (037)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-037-001 — "scrum ceremony" pure gap

A:
  run_id: "B36-0A-037-001"
  user_request: "Facilitate the scrum ceremonies for this sprint."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-scrum-ceremony-001"
    - "gap-project-planning-sprint-001"
  Teaching: "facilitate" is neutral. "scrum" and "ceremony" are gap vocabulary. DomainId: "project-management".
    SCHEDULE_CAPABILITY_LEARNING emitted.

---

## B36-0A-037-002 — "daily standup" pure gap

A:
  run_id: "B36-0A-037-002"
  user_request: "Run the daily standup and capture blockers for the team."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-standup-001"
    - "gap-project-planning-daily-standup-001"
  Teaching: "run", "capture" are neutral planning tokens. DomainId: "project-management".
    SCHEDULE_CAPABILITY_LEARNING emitted.

---

## B36-0A-037-003 — "sprint demo" pure gap

A:
  run_id: "B36-0A-037-003"
  user_request: "Prepare the sprint demo and present completed items to stakeholders."
  Task: "Identify predicates. Note 'stakeholders' token."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-sprint-demo-001"
    - "gap-project-planning-sprint-001"
  Teaching: "stakeholders" fires DOCUMENT unconditionally. "prepare" and "present" are neutral.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-037-004 — "sprint review" fires QA (not gap)

A:
  run_id: "B36-0A-037-004"
  user_request: "Conduct the sprint review with the product owner and stakeholders."
  Task: "Identify predicates. 'review' in 'sprint review'."

B:
  CompiledPredicateIds: ["DOCUMENT", "QA"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-sprint-001"
  Teaching: "review" fires QA unconditionally regardless of planning context.
    "stakeholders" fires DOCUMENT. Both predicates fire. DomainId: "software-development".
    SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-037-005 — "scrum ceremony" reference table for Round 3 vocabulary

A:
  run_id: "B36-0A-037-005"
  Task: "State boundary table for Round 3 (037–048) scrum ceremony vocabulary."

B:
  BoundaryTable_Round3_CeremoniesBlock:
    GAP only:
      - "scrum ceremony" / "ceremonies"
      - "daily standup" / "standup"
      - "sprint demo"
      - "backlog refinement" / "refinement"
      - "product increment"
      - "MVP" (Minimum Viable Product) definition
      - "launch readiness"
      - "go-live checklist"
    Fires DOCUMENT (token):
      - "sprint demo" + "stakeholders" → DOCUMENT
    Fires QA (token):
      - "sprint review" → QA (via "review")
      - "release criteria" → QA (via "criteria" or "review")
      - "go-live checklist" + "review" → QA
    Fires IMPLEMENT (token):
      - "feature flag" + code entity → IMPLEMENT
      - "feature flag" + "build"/"implement"/"deploy" → IMPLEMENT
