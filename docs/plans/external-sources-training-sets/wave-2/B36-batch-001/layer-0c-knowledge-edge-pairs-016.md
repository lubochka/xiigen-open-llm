# Layer 0-C — Knowledge-Edge Pairs (016)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B36-0C-016-001 — Product-roadmap: knowledge-edge full path

A:
  DocumentId: "B36-0C-016-001-prompt"
  CompiledText: "Present the product roadmap to the board and walk them through the Q3 and Q4 milestones."
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
    capability_gap_id: "gap-project-planning-roadmap-001"
    capability_gap_description: "Product roadmap vocabulary (roadmap, milestone) has no registered predicate."
    recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
    next_flow_id: "null"
  EvidenceIds: ["gap-project-planning-roadmap-001", "gap-project-planning-milestone-001"]
  Warnings:
    - "knowledge_edge: capability_gap vocabulary detected: roadmap, milestone"

---

## B36-0C-016-002 — Product-roadmap five gap terms: knowledge-edge

A:
  DocumentId: "B36-0C-016-002-prompt"
  CompiledText: "Align the product roadmap with sprint velocity, epic priorities, and burndown projections for Q3."
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
    capability_gap_id: "gap-project-planning-roadmap-001"
    recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
  EvidenceIds: ["gap-project-planning-roadmap-001", "gap-project-planning-sprint-001", "gap-project-planning-velocity-001", "gap-project-planning-epic-001", "gap-project-planning-burndown-001"]

---

## B36-0C-016-003 — Product-roadmap with ARCHITECTURE: not knowledge-edge

A:
  DocumentId: "B36-0C-016-003-prompt"
  CompiledText: "Design the product roadmap structure and architect the delivery phases for each epic."
  CompiledPredicateIds: ["ARCHITECTURE"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  FlowId: "architecture-design-flow-3.4"
  OverallVerdict: "ROUTED"
  KnowledgeEdge: false
  CapabilityGapWarnings:
    - "gap-project-planning-roadmap-001"
    - "gap-project-planning-epic-001"
  Teaching: ARCHITECTURE fires on "design" and "architect" even in roadmap planning sentences.
    The routing goes to Phase 3 (Technical Design), not a project planning sub-phase.

---

## B36-0C-016-004 — DomainId: project-management when knowledge-edge

A:
  DocumentId: "B36-0C-016-004-prompt"
  CompiledText: "Present the product roadmap and review the milestones."
  Note: "review" fires QA — corrected predicates below.

Predicate analysis:
  "review" — fires QA
  Corrected CompiledPredicateIds: ["QA"]

B (corrected for QA):
  FlowId: "qa-review-flow-6.1"
  OverallVerdict: "ROUTED"
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-roadmap-001", "gap-project-planning-milestone-001"]

Alternative B (if "review" absent: "Present the product roadmap and walk through milestones"):
  OverallVerdict: "KNOWLEDGE_EDGE"
  DomainId: "project-management"
  Teaching: DomainId is "project-management" ONLY when CompiledPredicateIds: [].
    When QA fires (via "review"), DomainId becomes "software-development".
