# Layer 0-C — Knowledge-Edge Pairs (060)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B36-0C-060-001 — Complete gap vocabulary count (all rounds)

A:
  Task: "State the complete count of gap vocabulary terms introduced across all B36 files."

B:
  GapVocabularyCount:
    Round1 (001–024): sprint, roadmap, milestone, backlog, velocity, standup, retrospective,
      burndown, capacity, iteration, epic, ticket, story-point, estimation, release-cycle,
      planning-poker, effort-estimate, grooming, triage, prioritize, refinement, product-roadmap
      → 22 terms

    Round2 (025–036): definition-of-done, acceptance-criteria, user-story-mapping, epics-breakdown,
      sprint-goal, retrospective-action-items, kanban-board, WIP-limits, flow-efficiency, cycle-time,
      burndown (already counted), complete-planning-reference
      → 11 new terms

    Round3 Ceremonies (037–048): scrum-ceremony, daily-standup, sprint-demo, backlog-refinement,
      product-increment, MVP, launch-readiness, go-live-checklist, feature-flag, release-criteria
      → 10 new terms

    Round3 Metrics (049–060): OKRs, KPIs, north-star-metric, impact-mapping, story-splitting,
      t-shirt-sizing, RICE-scoring, MoSCoW, now-next-later, hypothesis, experiment
      → 11 new terms

  Total: 54 distinct gap vocabulary entries across all B36 files.

---

## B36-0C-060-002 — "experiment" + "hypothesis" + "feature flag" + "launch readiness" gap signal

A:
  CompiledText: "Run experiments with feature flags, confirm hypotheses, and check launch readiness."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-experiment-001"
    - "gap-project-planning-feature-flag-001"
    - "gap-project-planning-hypothesis-001"
    - "gap-project-planning-launch-readiness-001"

---

## B36-0C-060-003 — Full Round 3 gap vocabulary zero-predicate example

A:
  CompiledText: "Organize all OKRs, KPIs, north-star metric, RICE scores, MoSCoW priorities, now-next-later roadmap, hypotheses, and experiments."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-okr-001"
    - "gap-project-planning-kpi-001"
    - "gap-project-planning-north-star-metric-001"
    - "gap-project-planning-rice-scoring-001"
    - "gap-project-planning-moscow-001"
    - "gap-project-planning-now-next-later-001"
    - "gap-project-planning-hypothesis-001"
    - "gap-project-planning-experiment-001"

---

## B36-0C-060-004 — SCHEDULE_CAPABILITY_LEARNING max bundling (8 IDs, 1 signal)

A:
  Task: "Confirm: 8 gap IDs → how many SCHEDULE_CAPABILITY_LEARNING signals?"

B:
  Answer: "One signal. All 8 IDs bundled into one CapabilityGapIds list."
  Rule: "Multiple signals for one request is always incorrect. Maximum: 1 signal per request."
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds: [8 IDs bundled in single signal — see 060-003 above]
