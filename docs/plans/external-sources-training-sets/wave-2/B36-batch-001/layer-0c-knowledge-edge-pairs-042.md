# Layer 0-C — Knowledge-Edge Pairs (042)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B36-0C-042-001 — "impact mapping" + "OKRs" gap signal

A:
  CompiledText: "Use impact mapping to validate that features contribute to OKRs."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-impact-mapping-001"
    - "gap-project-planning-okr-001"

---

## B36-0C-042-002 — "RICE scoring" + "north-star" gap signal

A:
  CompiledText: "Use RICE scoring (Reach, Impact, Confidence, Effort) to prioritize features that move the north-star metric."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-rice-scoring-001"
    - "gap-project-planning-north-star-metric-001"

---

## B36-0C-042-003 — "t-shirt sizing" + "RICE" + "story" fires DOCUMENT (not gap)

A:
  CompiledText: "Convert t-shirt sizing estimates to story points and RICE scores."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  OverallVerdict: "PREDICATE_FIRED"
  ScheduleCapabilityLearningSent: 0
  CapabilityGapIds: []
  CapabilityGapWarnings:
    - "gap-project-planning-t-shirt-sizing-001"
    - "gap-project-planning-rice-scoring-001"
    - "gap-project-planning-story-point-001"
  Teaching: "story" fires DOCUMENT (false positive). SCHEDULE_CAPABILITY_LEARNING NOT sent.
    Gap IDs forwarded as metadata warnings only, not as learning signal IDs.

---

## B36-0C-042-004 — "MoSCoW" + "now-next-later" + "roadmap" gap signal

A:
  CompiledText: "Use MoSCoW (Must/Should/Could/Won't) and now-next-later to finalize the roadmap."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-moscow-001"
    - "gap-project-planning-now-next-later-001"
    - "gap-project-planning-product-roadmap-001"
