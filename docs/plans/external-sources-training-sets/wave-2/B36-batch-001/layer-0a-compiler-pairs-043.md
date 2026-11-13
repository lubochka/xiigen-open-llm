# Layer 0-A — Compiler Pairs (043)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-043-001 — "OKRs" (Objectives and Key Results) pure gap

A:
  run_id: "B36-0A-043-001"
  user_request: "Set the OKRs (Objectives and Key Results) for the upcoming quarter."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-okr-001"
  Teaching: "set" is neutral. OKRs (Objectives and Key Results) is gap vocabulary.
    DomainId: "project-management". SCHEDULE_CAPABILITY_LEARNING emitted.

---

## B36-0A-043-002 — "KPIs" (Key Performance Indicators) pure gap

A:
  run_id: "B36-0A-043-002"
  user_request: "Track KPIs (Key Performance Indicators) for the sprint velocity and throughput."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-kpi-001"
    - "gap-project-planning-velocity-001"
    - "gap-project-planning-sprint-001"
  Teaching: "track" is neutral. KPIs (Key Performance Indicators), velocity, and sprint
    are all gap vocabulary. DomainId: "project-management". SCHEDULE_CAPABILITY_LEARNING emitted.

---

## B36-0A-043-003 — "north-star metric" pure gap

A:
  run_id: "B36-0A-043-003"
  user_request: "Align the team around the north-star metric before starting the sprint."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-north-star-metric-001"
    - "gap-project-planning-sprint-001"
  Teaching: "align" is neutral. "north-star metric" is gap vocabulary.
    DomainId: "project-management". SCHEDULE_CAPABILITY_LEARNING emitted.

---

## B36-0A-043-004 — "OKRs" + "define" fires DOCUMENT

A:
  run_id: "B36-0A-043-004"
  user_request: "Define the OKRs (Objectives and Key Results) for each product team this quarter."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-okr-001"
  Teaching: "define" fires DOCUMENT unconditionally. OKRs is gap vocabulary.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-043-005 — "KPIs" + "build" fires IMPLEMENT

A:
  run_id: "B36-0A-043-005"
  user_request: "Build the KPI (Key Performance Indicator) tracking dashboard."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-kpi-001"
  Teaching: "build" fires IMPLEMENT. "dashboard" is a code entity. KPI is gap vocabulary.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.
