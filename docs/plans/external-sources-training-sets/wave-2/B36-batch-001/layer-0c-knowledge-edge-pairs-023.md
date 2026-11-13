# Layer 0-C — Knowledge-Edge Pairs (023)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B36-0C-023-001 — "retrospective" alone: knowledge-edge

A:
  DocumentId: "B36-0C-023-001-prompt"
  CompiledText: "Run the sprint retrospective and capture what went well."
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
    capability_gap_id: "gap-project-planning-retrospective-001"
    capability_gap_description: "'retrospective' is gap vocabulary. 'review' fires QA; 'retrospective' does not. Zero predicates."
    recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
  EvidenceIds: ["gap-project-planning-retrospective-001", "gap-project-planning-sprint-001"]

BOUNDARY TEACHING:
  "retrospective" ≠ "review"
  "retrospective" is gap → knowledge-edge
  "review" is QA → routes to Phase 6
  The model must not confuse these two tokens.

---

## B36-0C-023-002 — "review" in planning context: NOT knowledge-edge

A:
  DocumentId: "B36-0C-023-002-prompt"
  CompiledText: "Review the sprint outcomes and assess team performance."
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  FlowId: "qa-review-flow-6.1"
  OverallVerdict: "ROUTED"
  KnowledgeEdge: false
  CapabilityGapWarnings:
    - "gap-project-planning-sprint-001"

BOUNDARY TEACHING: "review" fires QA. Not knowledge-edge. Sprint is gap warning.

---

## B36-0C-023-003 — "retrospective" + "review": QA fires, retrospective is gap

A:
  DocumentId: "B36-0C-023-003-prompt"
  CompiledText: "In the retrospective, review the code quality from the last sprint."
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  FlowId: "qa-review-flow-6.1"
  OverallVerdict: "ROUTED"
  KnowledgeEdge: false
  CapabilityGapWarnings:
    - "gap-project-planning-retrospective-001: 'retrospective' is gap vocabulary, not QA"
    - "gap-project-planning-sprint-001"

BOUNDARY TEACHING: When "retrospective" and "review" appear together, only "review" drives
  the QA routing. "Retrospective" is emitted as a gap warning, not as a routing signal.

---

## B36-0C-023-004 — Retrospective-only sentence: knowledge-edge

A:
  DocumentId: "B36-0C-023-004-prompt"
  CompiledText: "Facilitate the team retrospective and identify process improvements for the next sprint."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

B:
  FlowId: "knowledge-edge-missing-flow"
  OverallVerdict: "KNOWLEDGE_EDGE"
  FinalHumanResult: null
  Metrics:
    knowledge_edge_reason: "no_predicate_match_capability_gap"
    capability_gap_id: "gap-project-planning-retrospective-001"
    recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
  EvidenceIds: ["gap-project-planning-retrospective-001", "gap-project-planning-sprint-001"]
