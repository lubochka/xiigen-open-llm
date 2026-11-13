# Layer 0-A — Compiler Pairs (047)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-047-001 — "north-star metric" + "define" fires DOCUMENT

A:
  run_id: "B36-0A-047-001"
  user_request: "Define the north-star metric for the growth team this quarter."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-north-star-metric-001"
  Teaching: "define" fires DOCUMENT. "north-star metric" is gap vocabulary.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-047-002 — "story splitting" + "implement" fires IMPLEMENT

A:
  run_id: "B36-0A-047-002"
  user_request: "After story splitting, implement each sub-story as a separate service endpoint."
  Task: "Identify predicates. 'story' in 'story splitting'."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-story-splitting-001"
  Teaching: "story" fires DOCUMENT (false positive). "implement" fires IMPLEMENT.
    "service endpoint" is a code entity. Both predicates fire. DomainId: "software-development".
    SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-047-003 — "impact mapping" + "architecture" fires ARCHITECTURE

A:
  run_id: "B36-0A-047-003"
  user_request: "Design the architecture to support the outcomes from impact mapping."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["ARCHITECTURE"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-impact-mapping-001"
  Teaching: "design" fires ARCHITECTURE. "architecture" also fires ARCHITECTURE (additive).
    "impact mapping" is gap vocabulary. DomainId: "software-development".
    SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-047-004 — "OKRs" + "review" fires QA

A:
  run_id: "B36-0A-047-004"
  user_request: "Review quarterly OKRs (Objectives and Key Results) progress with the leadership team."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-okr-001"
  Teaching: "review" fires QA unconditionally. OKRs is gap vocabulary.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-047-005 — Round 3 (049–060) gap vocabulary boundary reference

A:
  run_id: "B36-0A-047-005"
  Task: "State the boundary table for Round 3 (049–060) metrics and prioritization vocabulary."

B:
  BoundaryTable_Round3_MetricsBlock:
    GAP only (zero predicates possible):
      - "OKRs" (Objectives and Key Results)
      - "KPIs" (Key Performance Indicators)
      - "north-star metric"
      - "impact mapping"
      - "t-shirt sizing"
      - "RICE scoring" (Reach, Impact, Confidence, Effort)
      - "MoSCoW" (Must/Should/Could/Won't)
      - "now-next-later"
      - "hypothesis"
      - "experiment" (product context, no "test" token)
    Always fires DOCUMENT (token):
      - "story" in "story splitting" → DOCUMENT
      - "define" + any gap term → DOCUMENT
      - "criteria" + any gap term → DOCUMENT
    Fires IMPLEMENT:
      - "build"/"implement" + code entity
    Fires TEST:
      - "test" (literal token, always)
    experiment + test → TEST fires (test dominates)
    experiment alone → gap only
