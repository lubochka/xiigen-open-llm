# Layer 0-C — Knowledge-Edge Pairs (017)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B36-0C-017-001 — Grooming: knowledge-edge full path

A:
  DocumentId: "B36-0C-017-001-prompt"
  CompiledText: "Groom the product backlog before the next sprint planning session."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

B:
  FlowId: "knowledge-edge-missing-flow"
  ScenarioId: "adaptive-logical-orchestration"
  OverallVerdict: "KNOWLEDGE_EDGE"
  FinalHumanResult: null
  Metrics:
    knowledge_edge_reason: "no_predicate_match_capability_gap"
    matched_predicate_count: "0"
    capability_gap_id: "gap-project-planning-backlog-001"
    capability_gap_description: "Grooming vocabulary (groom, backlog, sprint) has no registered predicate."
    recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
    next_flow_id: "null"
  EvidenceIds: ["gap-project-planning-backlog-001", "gap-project-planning-sprint-001"]

---

## B36-0C-017-002 — Refinement: knowledge-edge full path

A:
  DocumentId: "B36-0C-017-002-prompt"
  CompiledText: "Run the backlog refinement meeting and reprioritize the tickets for the upcoming iteration."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

B:
  FlowId: "knowledge-edge-missing-flow"
  ScenarioId: "adaptive-logical-orchestration"
  OverallVerdict: "KNOWLEDGE_EDGE"
  FinalHumanResult: null
  Metrics:
    knowledge_edge_reason: "no_predicate_match_capability_gap"
    matched_predicate_count: "0"
    capability_gap_id: "gap-project-planning-backlog-001"
    recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
  EvidenceIds: ["gap-project-planning-backlog-001", "gap-project-planning-ticket-001", "gap-project-planning-iteration-001"]

---

## B36-0C-017-003 — Grooming with DOCUMENT (story+criteria): not knowledge-edge

A:
  DocumentId: "B36-0C-017-003-prompt"
  CompiledText: "Groom the backlog and refine the user story acceptance criteria for each ticket."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  FlowId: "documentation-requirements-flow-2.1"
  OverallVerdict: "ROUTED"
  KnowledgeEdge: false
  CapabilityGapWarnings:
    - "gap-project-planning-backlog-001"
    - "gap-project-planning-ticket-001"

---

## B36-0C-017-004 — Grooming with story false positive: SCHEDULE_CAPABILITY_LEARNING check

A:
  DocumentId: "B36-0C-017-004-prompt"
  CompiledText: "Facilitate the backlog grooming session, triage the epic tickets, and estimate story-points before the sprint."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  FlowId: "documentation-requirements-flow-2.1"
  OverallVerdict: "ROUTED"
  KnowledgeEdge: false
  CapabilityGapWarnings:
    - "gap-project-planning-backlog-001"
    - "gap-project-planning-epic-001"
    - "gap-project-planning-ticket-001"
    - "gap-project-planning-story-point-001: DOCUMENT fires on 'story' substring"
    - "gap-project-planning-sprint-001"
  SCHEDULE_CAPABILITY_LEARNING: NOT emitted (DOCUMENT predicate fired)
  Teaching: SCHEDULE_CAPABILITY_LEARNING is only emitted when CompiledPredicateIds: [].
    When DOCUMENT fires (even via false positive), capability learning is NOT scheduled.
    Gap warnings are still emitted as metadata.
