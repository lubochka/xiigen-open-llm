# Layer 0-C — Knowledge-Edge Pairs (014)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B36-0C-014-001 — Effort-estimate: knowledge-edge full path

A:
  DocumentId: "B36-0C-014-001-prompt"
  CompiledText: "Provide an effort estimate for each item in the backlog before we commit to the sprint."
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
    capability_gap_id: "gap-project-planning-estimation-001"
    capability_gap_description: "Effort estimation vocabulary (effort estimate, backlog, sprint) has no registered predicate."
    recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
    next_flow_id: "null"
  EvidenceIds: ["gap-project-planning-estimation-001", "gap-project-planning-backlog-001", "gap-project-planning-sprint-001"]
  Warnings:
    - "knowledge_edge: capability_gap vocabulary detected: effort estimate, backlog, sprint"
    - "no_registered_sub_phase_for_effort_estimation"

---

## B36-0C-014-002 — Effort-estimate with DOCUMENT (story fires): mixed path

A:
  DocumentId: "B36-0C-014-002-prompt"
  CompiledText: "Estimate the effort for each user story in the backlog and record the story point values."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  FlowId: "documentation-requirements-flow-2.1"
  OverallVerdict: "ROUTED"
  KnowledgeEdge: false
  CapabilityGapWarnings:
    - "gap-project-planning-story-point-001: effort estimation intent not captured; DOCUMENT fires on 'story' substring"
    - "gap-project-planning-backlog-001"
  Teaching: The model must NOT suppress DOCUMENT to preserve the estimation intent.
    DOCUMENT fires on "story". The routing is correct in lexicon terms.

---

## B36-0C-014-003 — Effort-estimate five gap terms: knowledge-edge

A:
  DocumentId: "B36-0C-014-003-prompt"
  CompiledText: "Run a team effort-estimate session for the epic backlog and align on sprint capacity before grooming."
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
    capability_gap_id: "gap-project-planning-estimation-001"
    capability_gap_description: "Five gap terms: effort-estimate, epic, backlog, sprint, capacity — all gap vocabulary."
    recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
    next_flow_id: "null"
  EvidenceIds: ["gap-project-planning-estimation-001", "gap-project-planning-epic-001", "gap-project-planning-backlog-001", "gap-project-planning-sprint-001", "gap-project-planning-capacity-001"]

---

## B36-0C-014-004 — Effort-estimate with IMPLEMENT: mixed, not knowledge-edge

A:
  DocumentId: "B36-0C-014-004-prompt"
  CompiledText: "Estimate the effort required to implement the payment gateway feature before the sprint starts."
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  FlowId: "backend-service-flow-4.1"
  OverallVerdict: "ROUTED"
  KnowledgeEdge: false
  CapabilityGapWarnings:
    - "gap-project-planning-estimation-001: effort estimation context not captured"
    - "gap-project-planning-sprint-001"
  Teaching: IMPLEMENT dominates. Effort estimation and sprint are gap warnings.
    SCHEDULE_CAPABILITY_LEARNING is NOT emitted (predicates fired).
