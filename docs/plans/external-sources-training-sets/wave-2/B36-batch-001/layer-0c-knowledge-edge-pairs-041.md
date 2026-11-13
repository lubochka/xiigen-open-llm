# Layer 0-C — Knowledge-Edge Pairs (041)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B36-0C-041-001 — "OKRs" + "cascade" gap signal

A:
  CompiledText: "Cascade OKRs (Objectives and Key Results) from company level to team level."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-okr-001"

---

## B36-0C-041-002 — "KPIs" + "OKRs" gap signal (two IDs)

A:
  CompiledText: "Align KPIs (Key Performance Indicators) with the quarterly OKRs."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-kpi-001"
    - "gap-project-planning-okr-001"

---

## B36-0C-041-003 — "north-star metric" + "KPIs" gap signal

A:
  CompiledText: "Track KPIs (Key Performance Indicators) alongside the north-star metric each week."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-north-star-metric-001"
    - "gap-project-planning-kpi-001"

---

## B36-0C-041-004 — "hypothesis" + "experiment" + "now-next-later" gap signal

A:
  CompiledText: "Generate hypotheses from experiments and organize them in a now-next-later view."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-hypothesis-001"
    - "gap-project-planning-experiment-001"
    - "gap-project-planning-now-next-later-001"
