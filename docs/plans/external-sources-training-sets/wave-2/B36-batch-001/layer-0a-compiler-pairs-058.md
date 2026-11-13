# Layer 0-A — Compiler Pairs (058)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-058-001 — "RICE scoring" + "story splitting" + "implement" fires DOCUMENT+IMPLEMENT

A:
  run_id: "B36-0A-058-001"
  user_request: "After RICE scoring (Reach, Impact, Confidence, Effort) and story splitting, implement the top-priority stories."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-rice-scoring-001"
    - "gap-project-planning-story-splitting-001"
  Teaching: "story" fires DOCUMENT (false positive). "implement" fires IMPLEMENT.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-058-002 — "north-star" + "OKRs" + "review" + "implement" fires QA+IMPLEMENT

A:
  run_id: "B36-0A-058-002"
  user_request: "Review the north-star metric against OKRs and implement the missing tracker."
  Task: "Identify predicates. QA-last applies."

B:
  CompiledPredicateIds: ["QA", "IMPLEMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-north-star-metric-001"
    - "gap-project-planning-okr-001"
  Teaching: "review" fires QA. "implement" fires IMPLEMENT. QA-last: IMPLEMENT executes first.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-058-003 — "impact mapping" + "t-shirt sizing" + "MoSCoW" all gap

A:
  run_id: "B36-0A-058-003"
  user_request: "Run impact mapping, use t-shirt sizing, and apply MoSCoW (Must/Should/Could/Won't) to finalize scope."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-impact-mapping-001"
    - "gap-project-planning-t-shirt-sizing-001"
    - "gap-project-planning-moscow-001"
  Teaching: "run", "use", "apply", "finalize" are neutral. Three gap terms.
    DomainId: "project-management". SCHEDULE_CAPABILITY_LEARNING emitted.

---

## B36-0A-058-004 — "hypothesis" + "experiment" + "review" fires QA

A:
  run_id: "B36-0A-058-004"
  user_request: "Review the experiment results and decide if the hypothesis was validated."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-experiment-001"
    - "gap-project-planning-hypothesis-001"
  Teaching: "review" fires QA. "decide" and "validated" are neutral.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-058-005 — "KPIs" + "OKRs" + "north-star" + "build" fires IMPLEMENT

A:
  run_id: "B36-0A-058-005"
  user_request: "Build the KPI and OKR dashboard that tracks the north-star metric."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-kpi-001"
    - "gap-project-planning-okr-001"
    - "gap-project-planning-north-star-metric-001"
  Teaching: "build" fires IMPLEMENT. "dashboard" is a code entity. Three gap terms.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.
