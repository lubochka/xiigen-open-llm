# Layer 0-C — Knowledge-Edge Pairs (046)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B36-0C-046-001 — "MoSCoW" + "RICE" gap signal

A:
  CompiledText: "Combine MoSCoW (Must/Should/Could/Won't) and RICE scoring (Reach, Impact, Confidence, Effort) to decide what to build next."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-moscow-001"
    - "gap-project-planning-rice-scoring-001"

---

## B36-0C-046-002 — "now-next-later" + "OKRs" gap signal

A:
  CompiledText: "Map OKRs to now-next-later buckets for the product planning session."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-now-next-later-001"
    - "gap-project-planning-okr-001"

---

## B36-0C-046-003 — "feature flag" + "MVP" + "launch readiness" gap signal

A:
  CompiledText: "Enable the feature flag after the MVP passes launch readiness checks."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-feature-flag-001"
    - "gap-project-planning-mvp-001"
    - "gap-project-planning-launch-readiness-001"

---

## B36-0C-046-004 — "go-live checklist" + "sprint demo" + "product increment" gap signal

A:
  CompiledText: "Review the product increment in the sprint demo and complete the go-live checklist."
  CompiledPredicateIds: ["QA"]

B:
  OverallVerdict: "PREDICATE_FIRED"
  ScheduleCapabilityLearningSent: 0
  CapabilityGapIds: []
  CapabilityGapWarnings:
    - "gap-project-planning-product-increment-001"
    - "gap-project-planning-sprint-demo-001"
    - "gap-project-planning-go-live-checklist-001"
  Teaching: "review" fires QA. SCHEDULE_CAPABILITY_LEARNING NOT sent (predicate fired).
