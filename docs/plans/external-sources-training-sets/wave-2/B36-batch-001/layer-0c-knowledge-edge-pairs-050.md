# Layer 0-C — Knowledge-Edge Pairs (050)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B36-0C-050-001 — All Round 3 ceremony vocabulary gap signal

A:
  CompiledText: "Plan the scrum ceremonies: daily standup, sprint demo, sprint review, and backlog refinement."
  CompiledPredicateIds: ["QA"]

B:
  OverallVerdict: "PREDICATE_FIRED"
  ScheduleCapabilityLearningSent: 0
  CapabilityGapIds: []
  CapabilityGapWarnings:
    - "gap-project-planning-scrum-ceremony-001"
    - "gap-project-planning-standup-001"
    - "gap-project-planning-sprint-demo-001"
    - "gap-project-planning-backlog-001"
    - "gap-project-planning-refinement-001"
    - "gap-project-planning-sprint-001"
  Teaching: "review" in "sprint review" fires QA. SCHEDULE_CAPABILITY_LEARNING NOT sent.

---

## B36-0C-050-002 — "OKRs" + "KPIs" + "north-star" + "RICE" + "MoSCoW" + "now-next-later" max gap

A:
  CompiledText: "Score with RICE (Reach, Impact, Confidence, Effort), prioritize with MoSCoW (Must/Should/Could/Won't), organize in now-next-later, cascade to OKRs and KPIs, track with north-star metric."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-rice-scoring-001"
    - "gap-project-planning-moscow-001"
    - "gap-project-planning-now-next-later-001"
    - "gap-project-planning-okr-001"
    - "gap-project-planning-kpi-001"
    - "gap-project-planning-north-star-metric-001"

---

## B36-0C-050-003 — "hypothesis" + "experiment" + "t-shirt sizing" + "RICE" gap signal

A:
  CompiledText: "Size experiments with t-shirt sizing, score with RICE (Reach, Impact, Confidence, Effort), and prioritize by hypothesis strength."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-experiment-001"
    - "gap-project-planning-t-shirt-sizing-001"
    - "gap-project-planning-rice-scoring-001"
    - "gap-project-planning-hypothesis-001"

---

## B36-0C-050-004 — "impact mapping" + "story splitting" + "hypothesis" gap signal

A:
  CompiledText: "Use impact mapping, generate hypotheses, then apply story splitting."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-impact-mapping-001"
    - "gap-project-planning-hypothesis-001"
    - "gap-project-planning-story-splitting-001"
  Note: "story splitting" alone → gap. It is "story" token that fires DOCUMENT.
    "story splitting" without "story" as a standalone noun in the sentence is
    analyzed by exact token presence. If "story" appears in the text, DOCUMENT fires.
    If only "splitting" appears, no predicate fires.
