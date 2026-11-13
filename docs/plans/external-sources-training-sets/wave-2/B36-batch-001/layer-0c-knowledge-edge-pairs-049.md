# Layer 0-C — Knowledge-Edge Pairs (049)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B36-0C-049-001 — "KPIs" + "RICE" + "OKRs" gap signal

A:
  CompiledText: "Score initiatives with RICE (Reach, Impact, Confidence, Effort), tie to OKRs, and measure with KPIs."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-rice-scoring-001"
    - "gap-project-planning-okr-001"
    - "gap-project-planning-kpi-001"

---

## B36-0C-049-002 — "hypothesis" + "experiment" + "MoSCoW" gap signal

A:
  CompiledText: "Apply MoSCoW (Must/Should/Could/Won't) to choose which experiments and hypotheses to pursue."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-hypothesis-001"
    - "gap-project-planning-experiment-001"
    - "gap-project-planning-moscow-001"

---

## B36-0C-049-003 — "north-star" + "now-next-later" + "impact mapping" gap signal

A:
  CompiledText: "Use impact mapping to break the north-star metric into now-next-later items."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-north-star-metric-001"
    - "gap-project-planning-now-next-later-001"
    - "gap-project-planning-impact-mapping-001"

---

## B36-0C-049-004 — "t-shirt sizing" + "story splitting" — "story" fires DOCUMENT

A:
  CompiledText: "Apply story splitting and t-shirt sizing to the epic before sprint planning."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  OverallVerdict: "PREDICATE_FIRED"
  ScheduleCapabilityLearningSent: 0
  CapabilityGapIds: []
  CapabilityGapWarnings:
    - "gap-project-planning-story-splitting-001"
    - "gap-project-planning-t-shirt-sizing-001"
    - "gap-project-planning-epic-001"
    - "gap-project-planning-sprint-001"
  Teaching: "story" fires DOCUMENT. SCHEDULE_CAPABILITY_LEARNING NOT sent.
