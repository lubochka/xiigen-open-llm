# Layer 0-C — Knowledge-Edge Pairs (039)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B36-0C-039-001 — "hypothesis" gap signal

A:
  CompiledText: "Formulate a hypothesis for the new onboarding flow improvement."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-hypothesis-001"

---

## B36-0C-039-002 — "experiment" gap signal (no "test")

A:
  CompiledText: "Run a product experiment to validate the onboarding hypothesis."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-experiment-001"
    - "gap-project-planning-hypothesis-001"

---

## B36-0C-039-003 — "OKRs" + "KPIs" + "north-star" triple gap signal

A:
  CompiledText: "Establish OKRs, KPIs, and the north-star metric for the product team this quarter."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-okr-001"
    - "gap-project-planning-kpi-001"
    - "gap-project-planning-north-star-metric-001"

---

## B36-0C-039-004 — "RICE" + "MoSCoW" + "OKRs" triple gap signal

A:
  CompiledText: "Align RICE scoring (Reach, Impact, Confidence, Effort) with OKRs and apply MoSCoW (Must/Should/Could/Won't) for Q4 planning."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-rice-scoring-001"
    - "gap-project-planning-okr-001"
    - "gap-project-planning-moscow-001"
