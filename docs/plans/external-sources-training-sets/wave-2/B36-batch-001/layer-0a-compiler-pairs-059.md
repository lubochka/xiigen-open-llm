# Layer 0-A — Compiler Pairs (059)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-059-001 — "RICE + MoSCoW + story + now-next-later" — "story" fires DOCUMENT

A:
  run_id: "B36-0A-059-001"
  user_request: "Prioritize stories using RICE scoring (Reach, Impact, Confidence, Effort) and MoSCoW (Must/Should/Could/Won't), then place them in the now-next-later framework."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-rice-scoring-001"
    - "gap-project-planning-moscow-001"
    - "gap-project-planning-now-next-later-001"
  Teaching: "stories" fires DOCUMENT (false positive). "prioritize" and "place" are neutral.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-059-002 — "KPIs" + "hypothesis" + "test" fires TEST

A:
  run_id: "B36-0A-059-002"
  user_request: "Test whether the new KPIs align with the north-star hypothesis."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["TEST"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-kpi-001"
    - "gap-project-planning-north-star-metric-001"
    - "gap-project-planning-hypothesis-001"
  Teaching: "test" fires TEST. Three gap terms produce three warnings.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-059-003 — "impact mapping" + "RICE" + "story splitting" + "implement" fires DOCUMENT+IMPLEMENT

A:
  run_id: "B36-0A-059-003"
  user_request: "After impact mapping and RICE scoring (Reach, Impact, Confidence, Effort), implement story-split features."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-impact-mapping-001"
    - "gap-project-planning-rice-scoring-001"
    - "gap-project-planning-story-splitting-001"
  Teaching: "story" fires DOCUMENT (false positive). "implement" fires IMPLEMENT.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-059-004 — "experiment" + "now-next-later" + "OKRs" all gap

A:
  run_id: "B36-0A-059-004"
  user_request: "Prioritize experiments using a now-next-later framework to meet OKRs."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-experiment-001"
    - "gap-project-planning-now-next-later-001"
    - "gap-project-planning-okr-001"
  Teaching: "prioritize" is neutral. All three are gap vocabulary.
    DomainId: "project-management". SCHEDULE_CAPABILITY_LEARNING emitted.

---

## B36-0A-059-005 — v6-03 guard: DomainId wrong when "story" fires

A:
  run_id: "B36-0A-059-005"
  user_request: "Prioritize user stories in the backlog for next sprint."

CORRUPT B (do not reproduce):
  CompiledPredicateIds: ["DOCUMENT"]
  DomainId: "project-management"
  Note: "heavy planning vocabulary overrides DomainId"

Why corrupt:
  "v6-03: DomainId is binary. DOCUMENT fired via 'stories' → DomainId must be 'software-development'.
   Gap vocabulary (backlog, sprint) does not override the rule. One predicate → 'software-development'."

Correct B:
  CompiledPredicateIds: ["DOCUMENT"]
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-backlog-001", "gap-project-planning-sprint-001"]
