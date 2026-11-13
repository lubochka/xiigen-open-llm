# Layer 0-A — Compiler Pairs (045)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-045-001 — "MoSCoW" (Must/Should/Could/Won't) pure gap

A:
  run_id: "B36-0A-045-001"
  user_request: "Apply MoSCoW (Must/Should/Could/Won't) prioritization to the backlog."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-moscow-001"
    - "gap-project-planning-backlog-001"
  Teaching: "apply" is neutral. MoSCoW (Must/Should/Could/Won't) and backlog are gap vocabulary.
    DomainId: "project-management". SCHEDULE_CAPABILITY_LEARNING emitted.

---

## B36-0A-045-002 — "now-next-later" pure gap

A:
  run_id: "B36-0A-045-002"
  user_request: "Organize the roadmap using a now-next-later framework."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-now-next-later-001"
    - "gap-project-planning-product-roadmap-001"
  Teaching: "organize" is neutral. "now-next-later" and "roadmap" are gap vocabulary.
    DomainId: "project-management". SCHEDULE_CAPABILITY_LEARNING emitted.

---

## B36-0A-045-003 — "hypothesis" pure gap

A:
  run_id: "B36-0A-045-003"
  user_request: "Formulate a hypothesis for the new onboarding flow improvement."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-hypothesis-001"
  Teaching: "formulate" is neutral. "hypothesis" in a planning/product context is gap vocabulary.
    DomainId: "project-management". SCHEDULE_CAPABILITY_LEARNING emitted.

---

## B36-0A-045-004 — "experiment" in planning context (gap, not TEST)

A:
  run_id: "B36-0A-045-004"
  user_request: "Run a product experiment to validate the onboarding hypothesis."
  Task: "Identify predicates. Distinguish 'experiment' (gap) from TEST."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-experiment-001"
    - "gap-project-planning-hypothesis-001"
  Teaching: "run" is neutral. "experiment" in a product/planning context is gap vocabulary —
    it does NOT fire TEST. TEST fires on "test" (literal token). "validate" is neutral.
    DomainId: "project-management". SCHEDULE_CAPABILITY_LEARNING emitted.

---

## B36-0A-045-005 — "MoSCoW" + "specify" fires DOCUMENT

A:
  run_id: "B36-0A-045-005"
  user_request: "Specify the must-have requirements using MoSCoW (Must/Should/Could/Won't) criteria."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-moscow-001"
  Teaching: "specify" fires DOCUMENT. "requirements" fires DOCUMENT (additive).
    "criteria" fires DOCUMENT (additive). Three DOCUMENT triggers; one predicate.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.
