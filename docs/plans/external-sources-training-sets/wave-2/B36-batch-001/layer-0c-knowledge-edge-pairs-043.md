# Layer 0-C — Knowledge-Edge Pairs (043)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B36-0C-043-001 — "hypothesis" + "experiment" gap signal

A:
  CompiledText: "Formulate a hypothesis and design an experiment to validate it."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-hypothesis-001"
    - "gap-project-planning-experiment-001"

---

## B36-0C-043-002 — "impact mapping" + "story splitting" — "story" fires DOCUMENT

A:
  CompiledText: "Use impact mapping and story splitting to break down goals."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  OverallVerdict: "PREDICATE_FIRED"
  ScheduleCapabilityLearningSent: 0
  CapabilityGapIds: []
  CapabilityGapWarnings:
    - "gap-project-planning-impact-mapping-001"
    - "gap-project-planning-story-splitting-001"
  Teaching: "story" fires DOCUMENT. SCHEDULE_CAPABILITY_LEARNING NOT sent.

---

## B36-0C-043-003 — "OKRs" + "KPIs" + "north-star" + "impact mapping" quad gap signal

A:
  CompiledText: "Map OKRs to KPIs using impact mapping, then cascade to the north-star metric."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-okr-001"
    - "gap-project-planning-kpi-001"
    - "gap-project-planning-impact-mapping-001"
    - "gap-project-planning-north-star-metric-001"

---

## B36-0C-043-004 — "experiment" + "OKRs" + "now-next-later" gap signal

A:
  CompiledText: "Prioritize experiments in the now-next-later framework to hit OKRs."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-experiment-001"
    - "gap-project-planning-now-next-later-001"
    - "gap-project-planning-okr-001"
