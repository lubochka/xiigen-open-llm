# Layer 0-C — Knowledge-Edge Pairs (013)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B36-0C-013-001 — Planning-poker: knowledge-edge full path

A:
  DocumentId: "B36-0C-013-001-prompt"
  CompiledText: "Let's run a planning-poker session to estimate the backlog items for the next sprint."
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
    capability_gap_id: "gap-project-planning-sprint-001"
    capability_gap_description: "Planning-poker and estimation vocabulary (planning-poker, estimate, backlog, sprint) has no registered predicate. Capability learning scheduled."
    recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
    next_flow_id: "null"
  EvidenceIds: ["gap-project-planning-sprint-001", "gap-project-planning-backlog-001"]
  Warnings:
    - "knowledge_edge: capability_gap vocabulary detected: planning-poker, backlog, sprint"
    - "no_registered_sub_phase_for_project_planning_estimation"

---

## B36-0C-013-002 — Planning-poker with DOCUMENT (story): not knowledge-edge

A:
  DocumentId: "B36-0C-013-002-prompt"
  CompiledText: "Run planning poker and assign story point estimates for each backlog item."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  FlowId: "documentation-requirements-flow-2.1"
  ScenarioId: "adaptive-logical-orchestration"
  OverallVerdict: "ROUTED"
  KnowledgeEdge: false
  CapabilityGapWarnings:
    - "gap-project-planning-story-point-001: DOCUMENT fires on 'story' substring; planning-poker and estimation intent not captured"
    - "gap-project-planning-backlog-001"

---

## B36-0C-013-003 — Planning-poker with QA: not knowledge-edge

A:
  DocumentId: "B36-0C-013-003-prompt"
  CompiledText: "Review the planning poker results and finalize the sprint estimate."
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  FlowId: "qa-review-flow-6.1"
  ScenarioId: "adaptive-logical-orchestration"
  OverallVerdict: "ROUTED"
  KnowledgeEdge: false
  CapabilityGapWarnings:
    - "gap-project-planning-sprint-001"

---

## B36-0C-013-004 — Planning-poker five gap terms: full knowledge-edge path

A:
  DocumentId: "B36-0C-013-004-prompt"
  CompiledText: "Facilitate a planning poker session to groom and estimate the product backlog for the next iteration."
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
    capability_gap_description: "Five gap terms present (planning-poker, groom, estimate, backlog, iteration) with zero predicates."
    recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
    next_flow_id: "null"
  EvidenceIds: ["gap-project-planning-backlog-001", "gap-project-planning-iteration-001"]
  Warnings:
    - "knowledge_edge: capability_gap vocabulary detected: planning-poker, backlog, iteration"
