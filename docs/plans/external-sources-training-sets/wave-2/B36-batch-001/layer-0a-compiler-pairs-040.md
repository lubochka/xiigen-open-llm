# Layer 0-A — Compiler Pairs (040)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-040-001 — "launch readiness" pure gap

A:
  run_id: "B36-0A-040-001"
  user_request: "Track launch readiness across all teams for the Q3 release."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-launch-readiness-001"
    - "gap-project-planning-release-cycle-001"
  Teaching: "track" is neutral. "launch readiness" is gap vocabulary.
    DomainId: "project-management". SCHEDULE_CAPABILITY_LEARNING emitted.

---

## B36-0A-040-002 — "go-live checklist" pure gap

A:
  run_id: "B36-0A-040-002"
  user_request: "Complete the go-live checklist and share it with all stakeholders."
  Task: "Identify predicates. 'stakeholders' token."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-go-live-checklist-001"
  Teaching: "stakeholders" fires DOCUMENT unconditionally. "complete" is neutral.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-040-003 — "feature flag" pure gap (no code verb)

A:
  run_id: "B36-0A-040-003"
  user_request: "Plan the feature flag rollout schedule for the next three sprints."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-feature-flag-001"
    - "gap-project-planning-sprint-001"
  Teaching: "plan" is neutral. No code entity co-occurs with "feature flag" here.
    "rollout" is gap vocabulary. DomainId: "project-management". SCHEDULE_CAPABILITY_LEARNING emitted.

---

## B36-0A-040-004 — "release criteria" + "review" fires QA+DOCUMENT

A:
  run_id: "B36-0A-040-004"
  user_request: "Review the release criteria and document the approval process."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["QA", "DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-release-cycle-001"
  Teaching: "review" fires QA. "criteria" fires DOCUMENT. "document" also fires DOCUMENT.
    Two distinct predicates from three tokens. DomainId: "software-development".
    SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-040-005 — "planning + IMPLEMENT combo" — planning + build new service

A:
  run_id: "B36-0A-040-005"
  user_request: "During sprint planning, also build the notification microservice."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-sprint-001"
  Teaching: "build" fires IMPLEMENT. "notification microservice" is a code entity.
    "planning" is gap vocabulary. DomainId: "software-development".
    SCHEDULE_CAPABILITY_LEARNING NOT emitted.
