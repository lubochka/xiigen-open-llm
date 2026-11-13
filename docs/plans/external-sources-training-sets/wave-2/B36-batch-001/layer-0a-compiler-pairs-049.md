# Layer 0-A — Compiler Pairs (049)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-049-001 — "OKRs" + "KPIs" + "north-star" mega gap

A:
  run_id: "B36-0A-049-001"
  user_request: "Establish OKRs, KPIs, and the north-star metric for the product team."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-okr-001"
    - "gap-project-planning-kpi-001"
    - "gap-project-planning-north-star-metric-001"
  Teaching: "establish" is neutral. All three are gap vocabulary. DomainId: "project-management".
    SCHEDULE_CAPABILITY_LEARNING emitted with all three IDs bundled.

---

## B36-0A-049-002 — "impact mapping" + "story splitting" — "story" fires DOCUMENT

A:
  run_id: "B36-0A-049-002"
  user_request: "Use impact mapping and story splitting to break down the quarterly goals."
  Task: "Identify predicates. 'story' in 'story splitting'."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-impact-mapping-001"
    - "gap-project-planning-story-splitting-001"
  Teaching: "story" fires DOCUMENT (false positive in "story splitting"). "use" is neutral.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-049-003 — "RICE scoring" + "MoSCoW" + "specify" fires DOCUMENT

A:
  run_id: "B36-0A-049-003"
  user_request: "Specify the scoring criteria using RICE (Reach, Impact, Confidence, Effort) and MoSCoW (Must/Should/Could/Won't)."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-rice-scoring-001"
    - "gap-project-planning-moscow-001"
  Teaching: "specify" fires DOCUMENT. "criteria" fires DOCUMENT (additive).
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-049-004 — "experiment" vs "test" boundary reminder

A:
  run_id: "B36-0A-049-004"
  Task: "State the boundary between 'experiment' and 'test' in B36."

B:
  ExperimentVsTestBoundary:
    "experiment" alone → gap, DomainId: project-management, SCHEDULE_CAPABILITY_LEARNING emitted
    "test" alone → TEST fires, DomainId: software-development
    "experiment" + "test" → TEST fires (test dominates), "experiment" produces gap warning
    "run an experiment" → gap only (no TEST)
    "test the experiment" → TEST fires; "experiment" → gap warning

  Rule: The literal token "test" is the only trigger for TEST.
    "validate", "verify", "run an experiment" are all neutral.

---

## B36-0A-049-005 — "now-next-later" + "build" fires IMPLEMENT

A:
  run_id: "B36-0A-049-005"
  user_request: "Build a now-next-later planning widget for the team dashboard."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-now-next-later-001"
  Teaching: "build" fires IMPLEMENT. "widget" is a code entity. "dashboard" is a code entity.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.
