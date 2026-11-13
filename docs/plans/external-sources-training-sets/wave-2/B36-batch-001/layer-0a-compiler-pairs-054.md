# Layer 0-A — Compiler Pairs (054)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-054-001 — "now-next-later" + "requirements" fires DOCUMENT

A:
  run_id: "B36-0A-054-001"
  user_request: "Document the requirements for now-next-later items on the roadmap."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-now-next-later-001"
    - "gap-project-planning-product-roadmap-001"
  Teaching: "requirements" fires DOCUMENT. "document" also fires DOCUMENT (additive).
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-054-002 — "MoSCoW" + "story" fires DOCUMENT (two triggers)

A:
  run_id: "B36-0A-054-002"
  user_request: "Use MoSCoW (Must/Should/Could/Won't) to prioritize user stories in the backlog."
  Task: "Identify predicates. 'stories' token."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-moscow-001"
    - "gap-project-planning-backlog-001"
  Teaching: "stories" fires DOCUMENT (false positive). "prioritize" is neutral.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-054-003 — "north-star metric" + "KPIs" + "build" fires IMPLEMENT

A:
  run_id: "B36-0A-054-003"
  user_request: "Build a report that visualizes north-star metric alongside KPIs."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-north-star-metric-001"
    - "gap-project-planning-kpi-001"
  Teaching: "build" fires IMPLEMENT. "report" is a code entity.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-054-004 — "hypothesis" + "review" fires QA

A:
  run_id: "B36-0A-054-004"
  user_request: "Review the experiment hypothesis with the data team before launch."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-hypothesis-001"
    - "gap-project-planning-experiment-001"
  Teaching: "review" fires QA. "hypothesis" and "experiment" are gap vocabulary.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-054-005 — "t-shirt sizing" + "RICE" + "MoSCoW" + "story" fires DOCUMENT

A:
  run_id: "B36-0A-054-005"
  user_request: "Apply t-shirt sizing, RICE scoring (Reach, Impact, Confidence, Effort), and MoSCoW (Must/Should/Could/Won't) to user stories."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-t-shirt-sizing-001"
    - "gap-project-planning-rice-scoring-001"
    - "gap-project-planning-moscow-001"
  Teaching: "stories" fires DOCUMENT. "apply" is neutral. Three gap terms.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.
