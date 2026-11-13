# Layer 0-A — Compiler Pairs (052)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-052-001 — "t-shirt sizing" + "story points" + "backlog" — "story" fires

A:
  run_id: "B36-0A-052-001"
  user_request: "Convert t-shirt sizing to story points and update the backlog."
  Task: "Identify predicates. 'story' in 'story points'. 'update' + 'backlog'."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-t-shirt-sizing-001"
    - "gap-project-planning-story-point-001"
    - "gap-project-planning-backlog-001"
  Teaching: "story" fires DOCUMENT (false positive). "update" without code entity is neutral.
    "backlog" is gap vocabulary. DomainId: "software-development".
    SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-052-002 — "OKRs" + "implement" fires IMPLEMENT

A:
  run_id: "B36-0A-052-002"
  user_request: "Implement the OKR tracking feature in the company dashboard."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-okr-001"
  Teaching: "implement" fires IMPLEMENT. "feature" and "dashboard" are code entities.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-052-003 — "RICE scoring" + "MoSCoW" + "story splitting" — "story" fires

A:
  run_id: "B36-0A-052-003"
  user_request: "Apply MoSCoW (Must/Should/Could/Won't) and RICE scoring (Reach, Impact, Confidence, Effort) after story splitting."
  Task: "Identify predicates. 'story' in 'story splitting'."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-moscow-001"
    - "gap-project-planning-rice-scoring-001"
    - "gap-project-planning-story-splitting-001"
  Teaching: "story" fires DOCUMENT. "apply" is neutral. Three gap terms.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-052-004 — "north-star metric" + "test" fires TEST

A:
  run_id: "B36-0A-052-004"
  user_request: "Test whether the north-star metric increased after the experiment."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["TEST"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-north-star-metric-001"
    - "gap-project-planning-experiment-001"
  Teaching: "test" fires TEST. "north-star metric" and "experiment" are gap vocabulary.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-052-005 — "now-next-later" + "hypothesis" + "impact mapping" mega gap

A:
  run_id: "B36-0A-052-005"
  user_request: "Run impact mapping to generate hypotheses, then organize them in a now-next-later view."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-impact-mapping-001"
    - "gap-project-planning-hypothesis-001"
    - "gap-project-planning-now-next-later-001"
  Teaching: "run", "generate", "organize" are neutral. Three gap terms.
    DomainId: "project-management". SCHEDULE_CAPABILITY_LEARNING emitted.
