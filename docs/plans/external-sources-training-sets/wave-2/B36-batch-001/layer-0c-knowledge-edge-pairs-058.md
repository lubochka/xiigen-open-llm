# Layer 0-C — Knowledge-Edge Pairs (058)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B36-0C-058-001 — Complete ceremony gap ID map

A:
  Task: "State the complete Round 3 ceremony gap IDs introduced in files 035–060."

B:
  CeremonyGapIds:
    - gap-project-planning-scrum-ceremony-001
    - gap-project-planning-daily-standup-001
    - gap-project-planning-sprint-demo-001
    - gap-project-planning-backlog-001 (refinement context)
    - gap-project-planning-refinement-001
    - gap-project-planning-product-increment-001
    - gap-project-planning-mvp-001
    - gap-project-planning-launch-readiness-001
    - gap-project-planning-go-live-checklist-001
    - gap-project-planning-feature-flag-001

---

## B36-0C-058-002 — Complete metrics gap ID map

A:
  Task: "State the complete Round 3 metrics gap IDs introduced in files 035–060."

B:
  MetricsGapIds:
    - gap-project-planning-okr-001
    - gap-project-planning-kpi-001
    - gap-project-planning-north-star-metric-001
    - gap-project-planning-impact-mapping-001
    - gap-project-planning-t-shirt-sizing-001
    - gap-project-planning-rice-scoring-001
    - gap-project-planning-moscow-001
    - gap-project-planning-now-next-later-001
    - gap-project-planning-hypothesis-001
    - gap-project-planning-experiment-001
    - gap-project-planning-story-splitting-001

---

## B36-0C-058-003 — "experiment" + "hypothesis" + "launch readiness" + "feature flag" gap signal

A:
  CompiledText: "Form hypotheses, run experiments, and enable the feature flag after confirming launch readiness."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-hypothesis-001"
    - "gap-project-planning-experiment-001"
    - "gap-project-planning-launch-readiness-001"
    - "gap-project-planning-feature-flag-001"

---

## B36-0C-058-004 — SCHEDULE_CAPABILITY_LEARNING canonical rule

A:
  Task: "Confirm: when is SCHEDULE_CAPABILITY_LEARNING emitted vs not emitted?"

B:
  Emitted: "CompiledPredicateIds: [] AND gap vocabulary present → ScheduleCapabilityLearningSent: 1"
  NotEmitted: "Any predicate fires → ScheduleCapabilityLearningSent: 0"
  Format: "One signal per request. All CapabilityGapIds bundled."
  Applies to: all B36 gap vocabulary across all files (001–060).
