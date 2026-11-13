# Layer 0-A — Compiler Pairs (042)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-042-001 — "daily standup" + "update" — no code entity

A:
  run_id: "B36-0A-042-001"
  user_request: "Update the daily standup board with each team member's blockers."
  Task: "Identify predicates. 'update' token."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-standup-001"
    - "gap-project-planning-daily-standup-001"
  Teaching: "update" does NOT fire IMPLEMENT here — no code-level entity (function, class, module)
    co-occurs. "board" is a planning artifact, not a code entity. DomainId: "project-management".
    SCHEDULE_CAPABILITY_LEARNING emitted.

---

## B36-0A-042-002 — "product increment" + "specify" fires DOCUMENT

A:
  run_id: "B36-0A-042-002"
  user_request: "Specify the requirements for the next product increment."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-product-increment-001"
  Teaching: "specify" fires DOCUMENT unconditionally. "requirements" also fires DOCUMENT
    (same predicate, additive). DomainId: "software-development".
    SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-042-003 — "sprint review" v6-02 corruption guard

A:
  run_id: "B36-0A-042-003"
  user_request: "Attend the sprint review to observe team progress."

CORRUPT B (do not reproduce):
  CompiledPredicateIds: []
  Note: "sprint review is a planning ceremony, not a quality gate"

Why corrupt:
  "'review' fires QA unconditionally. The word 'sprint' before 'review' does not suppress QA.
   v6-02: predicate suppressed because sentence feels like planning. Correct: ['QA']."

Correct B:
  CompiledPredicateIds: ["QA"]
  CapabilityGapWarnings: ["gap-project-planning-sprint-001"]
  DomainId: "software-development"

---

## B36-0A-042-004 — "go-live checklist" + "review" + "test" fires QA+TEST

A:
  run_id: "B36-0A-042-004"
  user_request: "Test and review each item on the go-live checklist before deployment."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["TEST", "QA"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-go-live-checklist-001"
  Teaching: "test" fires TEST. "review" fires QA. Both are unconditional.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-042-005 — "launch readiness" + "design" fires ARCHITECTURE

A:
  run_id: "B36-0A-042-005"
  user_request: "Design the launch readiness dashboard for release tracking."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["ARCHITECTURE"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-launch-readiness-001"
  Teaching: "design" fires ARCHITECTURE. "launch readiness" is gap vocabulary.
    "dashboard" is a code entity in this context. DomainId: "software-development".
    SCHEDULE_CAPABILITY_LEARNING NOT emitted.
