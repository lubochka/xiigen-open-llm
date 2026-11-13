# Layer 0-A — Compiler Pairs (044)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-044-001 — "impact mapping" pure gap

A:
  run_id: "B36-0A-044-001"
  user_request: "Run an impact mapping session to align features with business goals."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-impact-mapping-001"
  Teaching: "run" and "align" are neutral. "impact mapping" is gap vocabulary.
    DomainId: "project-management". SCHEDULE_CAPABILITY_LEARNING emitted.

---

## B36-0A-044-002 — "story splitting" pure gap

A:
  run_id: "B36-0A-044-002"
  user_request: "Practice story splitting to break down the large epic into smaller items."
  Task: "Identify predicates. Note 'story' in 'story splitting'."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-story-splitting-001"
    - "gap-project-planning-epic-001"
  Teaching: "story" fires DOCUMENT unconditionally — even in "story splitting".
    This is the same false-positive mechanism as "story points". DomainId: "software-development".
    SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-044-003 — "t-shirt sizing" pure gap

A:
  run_id: "B36-0A-044-003"
  user_request: "Use t-shirt sizing to estimate effort for the backlog items."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-t-shirt-sizing-001"
    - "gap-project-planning-backlog-001"
  Teaching: "use" and "estimate" are neutral. "t-shirt sizing" is gap vocabulary.
    DomainId: "project-management". SCHEDULE_CAPABILITY_LEARNING emitted.

---

## B36-0A-044-004 — "RICE scoring" pure gap

A:
  run_id: "B36-0A-044-004"
  user_request: "Apply RICE scoring (Reach, Impact, Confidence, Effort) to prioritize the roadmap."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-rice-scoring-001"
    - "gap-project-planning-product-roadmap-001"
  Teaching: "apply" and "prioritize" are neutral. RICE scoring (Reach, Impact, Confidence, Effort)
    and roadmap are gap vocabulary. DomainId: "project-management". SCHEDULE_CAPABILITY_LEARNING emitted.

---

## B36-0A-044-005 — "impact mapping" + "document" fires DOCUMENT

A:
  run_id: "B36-0A-044-005"
  user_request: "Document the outcomes of the impact mapping workshop."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-impact-mapping-001"
  Teaching: "document" fires DOCUMENT. "workshop" is neutral.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.
