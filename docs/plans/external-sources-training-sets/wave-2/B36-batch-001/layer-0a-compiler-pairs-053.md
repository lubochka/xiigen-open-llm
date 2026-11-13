# Layer 0-A — Compiler Pairs (053)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-053-001 — "KPIs" + "review" fires QA

A:
  run_id: "B36-0A-053-001"
  user_request: "Review the KPIs (Key Performance Indicators) at the end of the sprint retrospective."
  Task: "Identify predicates. Note 'retrospective'."

B:
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-kpi-001"
    - "gap-project-planning-retrospective-001"
    - "gap-project-planning-sprint-001"
  Teaching: "review" fires QA. "retrospective" is gap vocabulary (does NOT fire QA).
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-053-002 — "story splitting" + "acceptance criteria" — two DOCUMENT triggers

A:
  run_id: "B36-0A-053-002"
  user_request: "After story splitting, define acceptance criteria for each sub-story."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-story-splitting-001"
    - "gap-project-planning-acceptance-criteria-001"
  Teaching: "story" fires DOCUMENT (false positive). "define" fires DOCUMENT. "criteria" fires DOCUMENT.
    All three contribute to one predicate. DomainId: "software-development".
    SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-053-003 — "experiment" v6-02 guard: not TEST

A:
  run_id: "B36-0A-053-003"
  user_request: "Run a product experiment to measure user engagement."

CORRUPT B (do not reproduce):
  CompiledPredicateIds: ["TEST"]
  Note: "experiment means testing hypothesis"

Why corrupt:
  "'experiment' does NOT fire TEST. Only the literal token 'test' fires TEST.
   v6-02: predicate fired by semantic association rather than literal lexicon match.
   Correct output: CompiledPredicateIds: [] (knowledge_edge)."

Correct B:
  CompiledPredicateIds: []
  CapabilityGapWarnings: ["gap-project-planning-experiment-001"]
  DomainId: "project-management"

---

## B36-0A-053-004 — "RICE scoring" + "t-shirt sizing" + "specify" fires DOCUMENT

A:
  run_id: "B36-0A-053-004"
  user_request: "Specify the prioritization approach: RICE scoring (Reach, Impact, Confidence, Effort) for epics, t-shirt sizing for tasks."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-rice-scoring-001"
    - "gap-project-planning-t-shirt-sizing-001"
    - "gap-project-planning-epic-001"
  Teaching: "specify" fires DOCUMENT. Three gap terms produce three gap warnings.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-053-005 — "impact mapping" + "OKRs" + "implement" fires IMPLEMENT

A:
  run_id: "B36-0A-053-005"
  user_request: "Implement the feature set identified from impact mapping to meet OKR targets."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-impact-mapping-001"
    - "gap-project-planning-okr-001"
  Teaching: "implement" fires IMPLEMENT. "feature set" is a code entity.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.
