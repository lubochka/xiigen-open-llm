# Layer 0-A — Compiler Pairs (051)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-051-001 — "MoSCoW" + "now-next-later" + "review" fires QA

A:
  run_id: "B36-0A-051-001"
  user_request: "Review the MoSCoW (Must/Should/Could/Won't) and now-next-later priorities with the team."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-moscow-001"
    - "gap-project-planning-now-next-later-001"
  Teaching: "review" fires QA. Two gap terms produce two gap warnings. DomainId: "software-development".
    SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-051-002 — "story splitting" + "RICE scoring" — "story" fires DOCUMENT

A:
  run_id: "B36-0A-051-002"
  user_request: "Use RICE scoring (Reach, Impact, Confidence, Effort) to prioritize after story splitting."
  Task: "Identify predicates. 'story' in 'story splitting'."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-rice-scoring-001"
    - "gap-project-planning-story-splitting-001"
  Teaching: "story" fires DOCUMENT (false positive). "prioritize" is neutral.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-051-003 — "north-star metric" + "KPIs" + "OKRs" full reference

A:
  run_id: "B36-0A-051-003"
  user_request: "Align north-star metric, KPIs (Key Performance Indicators), and OKRs (Objectives and Key Results) for the quarter."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-north-star-metric-001"
    - "gap-project-planning-kpi-001"
    - "gap-project-planning-okr-001"
  Teaching: "align" is neutral. Three gap terms. DomainId: "project-management".
    SCHEDULE_CAPABILITY_LEARNING emitted with all three IDs bundled.

---

## B36-0A-051-004 — "impact mapping" + "requirements" fires DOCUMENT

A:
  run_id: "B36-0A-051-004"
  user_request: "Capture the requirements identified in the impact mapping session."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-impact-mapping-001"
  Teaching: "requirements" fires DOCUMENT unconditionally. "capture" is neutral.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-051-005 — "hypothesis" + "experiment" pure gap (no test/review)

A:
  run_id: "B36-0A-051-005"
  user_request: "Formulate a hypothesis and run an experiment to validate the pricing change."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-hypothesis-001"
    - "gap-project-planning-experiment-001"
  Teaching: "formulate", "run", "validate" are all neutral. "hypothesis" and "experiment"
    are gap vocabulary. DomainId: "project-management". SCHEDULE_CAPABILITY_LEARNING emitted.
