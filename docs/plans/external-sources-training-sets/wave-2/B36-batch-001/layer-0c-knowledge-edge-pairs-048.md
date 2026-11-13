# Layer 0-C — Knowledge-Edge Pairs (048)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B36-0C-048-001 — "OKRs" + "north-star" + "impact mapping" gap signal

A:
  CompiledText: "Use impact mapping to connect features to OKRs and the north-star metric."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-impact-mapping-001"
    - "gap-project-planning-okr-001"
    - "gap-project-planning-north-star-metric-001"

---

## B36-0C-048-002 — "scrum ceremonies" + "sprint demo" + "sprint review" — "review" fires QA

A:
  CompiledText: "Run the sprint demo and sprint review during the scrum ceremonies."
  CompiledPredicateIds: ["QA"]

B:
  OverallVerdict: "PREDICATE_FIRED"
  ScheduleCapabilityLearningSent: 0
  CapabilityGapIds: []
  CapabilityGapWarnings:
    - "gap-project-planning-scrum-ceremony-001"
    - "gap-project-planning-sprint-demo-001"
    - "gap-project-planning-sprint-001"
  Teaching: "review" fires QA. SCHEDULE_CAPABILITY_LEARNING NOT sent.

---

## B36-0C-048-003 — "MoSCoW" + "t-shirt sizing" + "backlog" + "sprint" gap signal

A:
  CompiledText: "Prioritize the backlog using MoSCoW (Must/Should/Could/Won't) and t-shirt sizing before the sprint."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-moscow-001"
    - "gap-project-planning-t-shirt-sizing-001"
    - "gap-project-planning-backlog-001"
    - "gap-project-planning-sprint-001"

---

## B36-0C-048-004 — "experiment" + "now-next-later" + "OKRs" gap signal

A:
  CompiledText: "Prioritize experiments using a now-next-later framework aligned with OKRs."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-experiment-001"
    - "gap-project-planning-now-next-later-001"
    - "gap-project-planning-okr-001"
