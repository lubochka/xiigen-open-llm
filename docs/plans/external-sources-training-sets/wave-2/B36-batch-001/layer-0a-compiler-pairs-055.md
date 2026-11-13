# Layer 0-A — Compiler Pairs (055)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-055-001 — "impact mapping" + "story splitting" + "implement" fires IMPLEMENT+DOCUMENT

A:
  run_id: "B36-0A-055-001"
  user_request: "After impact mapping and story splitting, implement the top-priority stories."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-impact-mapping-001"
    - "gap-project-planning-story-splitting-001"
  Teaching: "story" fires DOCUMENT (false positive in "stories"). "implement" fires IMPLEMENT.
    Both predicates fire. DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-055-002 — "OKRs" + "KPIs" + "north-star" all gap

A:
  run_id: "B36-0A-055-002"
  user_request: "Cascade OKRs down to KPIs and track them against the north-star metric."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-okr-001"
    - "gap-project-planning-kpi-001"
    - "gap-project-planning-north-star-metric-001"
  Teaching: "cascade" and "track" are neutral. All three are gap vocabulary.
    DomainId: "project-management". SCHEDULE_CAPABILITY_LEARNING emitted.

---

## B36-0A-055-003 — "experiment" + "implement" fires IMPLEMENT (experiment is gap)

A:
  run_id: "B36-0A-055-003"
  user_request: "Implement the experiment as a feature flag to measure its impact."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-experiment-001"
    - "gap-project-planning-feature-flag-001"
  Teaching: "implement" fires IMPLEMENT. "experiment" and "feature flag" are gap vocabulary.
    "measure" is neutral. DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-055-004 — "t-shirt sizing" + "define" fires DOCUMENT

A:
  run_id: "B36-0A-055-004"
  user_request: "Define the t-shirt sizing scale for the engineering team."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-t-shirt-sizing-001"
  Teaching: "define" fires DOCUMENT. DomainId: "software-development".
    SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-055-005 — "now-next-later" + "OKRs" + "review" fires QA

A:
  run_id: "B36-0A-055-005"
  user_request: "Review the now-next-later roadmap against the OKRs with the product team."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-now-next-later-001"
    - "gap-project-planning-okr-001"
  Teaching: "review" fires QA. Two gap terms. DomainId: "software-development".
    SCHEDULE_CAPABILITY_LEARNING NOT emitted.
