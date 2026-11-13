# Layer 0-C — Knowledge-Edge Pairs (040)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B36-0C-040-001 — "impact mapping" + "t-shirt sizing" gap signal

A:
  CompiledText: "Run impact mapping then use t-shirt sizing to estimate each outcome."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-impact-mapping-001"
    - "gap-project-planning-t-shirt-sizing-001"

---

## B36-0C-040-002 — "now-next-later" + "hypothesis" gap signal

A:
  CompiledText: "Generate hypotheses and organize them in a now-next-later view."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-hypothesis-001"
    - "gap-project-planning-now-next-later-001"

---

## B36-0C-040-003 — "experiment" + "MoSCoW" + "hypothesis" gap signal

A:
  CompiledText: "Prioritize experiments with MoSCoW (Must/Should/Could/Won't) based on each hypothesis."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-experiment-001"
    - "gap-project-planning-moscow-001"
    - "gap-project-planning-hypothesis-001"

---

## B36-0C-040-004 — "RICE" + "MoSCoW" + "now-next-later" mega gap signal

A:
  CompiledText: "Use RICE scoring (Reach, Impact, Confidence, Effort) and MoSCoW (Must/Should/Could/Won't) to populate a now-next-later roadmap."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-rice-scoring-001"
    - "gap-project-planning-moscow-001"
    - "gap-project-planning-now-next-later-001"
