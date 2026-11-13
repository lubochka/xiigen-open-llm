# Layer 0-A — Compiler Pairs (039)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-039-001 — "sprint review" QA fires; "sprint" is gap

A:
  run_id: "B36-0A-039-001"
  user_request: "Lead the sprint review session and capture feedback from the product owner."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-sprint-001"
  Teaching: "review" fires QA unconditionally. "lead" and "capture" are neutral.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-039-002 — "release criteria" fires QA via "criteria"

A:
  run_id: "B36-0A-039-002"
  user_request: "Establish the release criteria before the team signs off on the sprint."
  Task: "Identify predicates. 'criteria' token present."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-release-cycle-001"
    - "gap-project-planning-sprint-001"
  Teaching: "criteria" fires DOCUMENT unconditionally — not QA. "criteria" is a DOCUMENT
    lexicon token. "signs off" is neutral. DomainId: "software-development".
    SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-039-003 — "feature flag" + IMPLEMENT

A:
  run_id: "B36-0A-039-003"
  user_request: "Implement the feature flag for the dark mode rollout."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-feature-flag-001"
  Teaching: "implement" fires IMPLEMENT. "feature flag" is planning vocabulary with a code
    entity present. DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-039-004 — "go-live checklist" + "review" fires QA

A:
  run_id: "B36-0A-039-004"
  user_request: "Review the go-live checklist before the production deployment."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-go-live-checklist-001"
  Teaching: "review" fires QA unconditionally. "go-live checklist" is gap vocabulary.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-039-005 — "product increment" pure gap with planning verbs

A:
  run_id: "B36-0A-039-005"
  user_request: "Assess the product increment against the sprint goal and capacity."
  Task: "Identify predicates. Note 'assess'."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-product-increment-001"
    - "gap-project-planning-sprint-goal-001"
    - "gap-project-planning-sprint-001"
    - "gap-project-planning-capacity-001"
  Teaching: "assess" is neutral — not in QA lexicon. All four terms are gap vocabulary.
    DomainId: "project-management". SCHEDULE_CAPABILITY_LEARNING emitted.
