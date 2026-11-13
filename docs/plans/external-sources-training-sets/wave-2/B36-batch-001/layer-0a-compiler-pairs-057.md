# Layer 0-A — Compiler Pairs (057)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-057-001 — "t-shirt sizing" + "hypothesis" + "test" fires TEST

A:
  run_id: "B36-0A-057-001"
  user_request: "Test the t-shirt sizing hypothesis with a small team before full adoption."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["TEST"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-t-shirt-sizing-001"
    - "gap-project-planning-hypothesis-001"
  Teaching: "test" fires TEST. "t-shirt sizing" and "hypothesis" are gap vocabulary.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-057-002 — "north-star" + "story splitting" — "story" fires DOCUMENT

A:
  run_id: "B36-0A-057-002"
  user_request: "Use story splitting to break down north-star metric improvements into deliverables."
  Task: "Identify predicates. 'story' in 'story splitting'."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-story-splitting-001"
    - "gap-project-planning-north-star-metric-001"
  Teaching: "story" fires DOCUMENT. "use" and "break down" are neutral.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-057-003 — "OKRs" + "review" + "implement" fires QA+IMPLEMENT

A:
  run_id: "B36-0A-057-003"
  user_request: "Review the OKRs and implement the tracking module for the dashboard."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["QA", "IMPLEMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-okr-001"
  Teaching: "review" fires QA. "implement" fires IMPLEMENT. "tracking module" is a code entity.
    QA-last applies. DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-057-004 — "KPIs" + "design" fires ARCHITECTURE

A:
  run_id: "B36-0A-057-004"
  user_request: "Design the KPI (Key Performance Indicator) monitoring architecture."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["ARCHITECTURE"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-kpi-001"
  Teaching: "design" fires ARCHITECTURE. "monitoring architecture" reinforces ARCHITECTURE.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-057-005 — "experiment" + "hypothesis" + "MoSCoW" all gap

A:
  run_id: "B36-0A-057-005"
  user_request: "Prioritize experiments using MoSCoW (Must/Should/Could/Won't) based on each hypothesis."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-experiment-001"
    - "gap-project-planning-moscow-001"
    - "gap-project-planning-hypothesis-001"
  Teaching: "prioritize" is neutral. All three are gap vocabulary.
    DomainId: "project-management". SCHEDULE_CAPABILITY_LEARNING emitted.
