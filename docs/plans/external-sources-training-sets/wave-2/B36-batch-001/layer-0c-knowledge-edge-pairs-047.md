# Layer 0-C — Knowledge-Edge Pairs (047)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B36-0C-047-001 — "RICE" + "t-shirt sizing" + "MoSCoW" + "now-next-later" mega gap

A:
  CompiledText: "Use RICE scoring (Reach, Impact, Confidence, Effort), t-shirt sizing, and MoSCoW (Must/Should/Could/Won't) for now-next-later roadmap planning."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-rice-scoring-001"
    - "gap-project-planning-t-shirt-sizing-001"
    - "gap-project-planning-moscow-001"
    - "gap-project-planning-now-next-later-001"

---

## B36-0C-047-002 — "hypothesis" + "OKRs" + "KPIs" gap signal

A:
  CompiledText: "Form hypotheses tied to OKRs and measure them with KPIs."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-hypothesis-001"
    - "gap-project-planning-okr-001"
    - "gap-project-planning-kpi-001"

---

## B36-0C-047-003 — "experiment" fires PREDICATE_FIRED only when "test" present

A:
  CompiledText: "Test the experiment results and track improvements in the north-star metric."
  CompiledPredicateIds: ["TEST"]

B:
  OverallVerdict: "PREDICATE_FIRED"
  ScheduleCapabilityLearningSent: 0
  CapabilityGapIds: []
  CapabilityGapWarnings:
    - "gap-project-planning-experiment-001"
    - "gap-project-planning-north-star-metric-001"
  Teaching: "test" fires TEST. SCHEDULE_CAPABILITY_LEARNING NOT sent. Gap IDs forwarded as metadata.

---

## B36-0C-047-004 — "impact mapping" + "RICE" + "hypothesis" gap signal

A:
  CompiledText: "Run impact mapping, generate hypotheses, then score with RICE (Reach, Impact, Confidence, Effort)."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-impact-mapping-001"
    - "gap-project-planning-hypothesis-001"
    - "gap-project-planning-rice-scoring-001"
