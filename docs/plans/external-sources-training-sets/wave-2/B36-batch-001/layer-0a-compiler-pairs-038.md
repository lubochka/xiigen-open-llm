# Layer 0-A — Compiler Pairs (038)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-038-001 — "backlog refinement" pure gap

A:
  run_id: "B36-0A-038-001"
  user_request: "Schedule the backlog refinement session for next Tuesday."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-backlog-001"
    - "gap-project-planning-refinement-001"
  Teaching: "schedule" is neutral. DomainId: "project-management".
    SCHEDULE_CAPABILITY_LEARNING emitted.

---

## B36-0A-038-002 — "product increment" pure gap

A:
  run_id: "B36-0A-038-002"
  user_request: "Verify the product increment meets the definition of done before releasing."
  Task: "Identify predicates. Note 'verify'."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-product-increment-001"
    - "gap-project-planning-definition-of-done-001"
  Teaching: "verify" is not in the QA lexicon — it is neutral in this context.
    Only "review", "approve", "test" unconditionally fire QA. DomainId: "project-management".
    SCHEDULE_CAPABILITY_LEARNING emitted.

---

## B36-0A-038-003 — "MVP definition" + "define" fires DOCUMENT

A:
  run_id: "B36-0A-038-003"
  user_request: "Define the MVP for the first release and align the team on scope."
  Task: "Identify predicates. 'define' token present."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-mvp-001"
    - "gap-project-planning-release-cycle-001"
  Teaching: "define" fires DOCUMENT unconditionally. "align" is neutral.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-038-004 — "sprint demo" + "build" fires IMPLEMENT

A:
  run_id: "B36-0A-038-004"
  user_request: "Build the demo environment for the sprint demo next Friday."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-sprint-demo-001"
    - "gap-project-planning-sprint-001"
  Teaching: "build" fires IMPLEMENT. "environment" is a code entity co-occurring with "build".
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-038-005 — "backlog refinement" + "document" fires DOCUMENT

A:
  run_id: "B36-0A-038-005"
  user_request: "Document the outcomes of the backlog refinement and update the backlog."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-backlog-001"
    - "gap-project-planning-refinement-001"
  Teaching: "document" fires DOCUMENT. "update" without code entity is neutral.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.
