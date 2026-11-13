# Layer 0-A — Compiler Pairs (056)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-056-001 — "RICE scoring" + "architect" fires ARCHITECTURE

A:
  run_id: "B36-0A-056-001"
  user_request: "Architect the scoring engine that implements RICE (Reach, Impact, Confidence, Effort) calculations."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["ARCHITECTURE"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-rice-scoring-001"
  Teaching: "architect" fires ARCHITECTURE. "scoring engine" is a code entity.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-056-002 — "story splitting" + "RICE" + "review" fires QA+DOCUMENT

A:
  run_id: "B36-0A-056-002"
  user_request: "Review the RICE scoring (Reach, Impact, Confidence, Effort) after story splitting."
  Task: "Identify predicates. 'story' and 'review' tokens."

B:
  CompiledPredicateIds: ["QA", "DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-rice-scoring-001"
    - "gap-project-planning-story-splitting-001"
  Teaching: "review" fires QA. "story" fires DOCUMENT. Two predicates fire.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-056-003 — "experiment" + "hypothesis" + "north-star" all gap

A:
  run_id: "B36-0A-056-003"
  user_request: "Generate an experiment hypothesis to improve the north-star metric."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-experiment-001"
    - "gap-project-planning-hypothesis-001"
    - "gap-project-planning-north-star-metric-001"
  Teaching: "generate" is neutral. All three are gap vocabulary. DomainId: "project-management".
    SCHEDULE_CAPABILITY_LEARNING emitted.

---

## B36-0A-056-004 — "MoSCoW" + "OKRs" + "KPIs" — three-gap zero predicate

A:
  run_id: "B36-0A-056-004"
  user_request: "Align MoSCoW (Must/Should/Could/Won't) priorities with OKRs and KPIs for the quarter."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-moscow-001"
    - "gap-project-planning-okr-001"
    - "gap-project-planning-kpi-001"
  Teaching: "align" is neutral. Three gap terms. DomainId: "project-management".
    SCHEDULE_CAPABILITY_LEARNING emitted.

---

## B36-0A-056-005 — "impact mapping" + "now-next-later" + "implement" fires IMPLEMENT

A:
  run_id: "B36-0A-056-005"
  user_request: "Implement the now-next-later features identified in the impact mapping."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-now-next-later-001"
    - "gap-project-planning-impact-mapping-001"
  Teaching: "implement" fires IMPLEMENT. "features" is a code entity.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.
