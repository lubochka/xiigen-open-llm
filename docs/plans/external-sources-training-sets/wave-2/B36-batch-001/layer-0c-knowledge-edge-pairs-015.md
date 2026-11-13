# Layer 0-C — Knowledge-Edge Pairs (015)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B36-0C-015-001 — Release-planning: knowledge-edge full path

A:
  DocumentId: "B36-0C-015-001-prompt"
  CompiledText: "Create the release plan for Q4 and map each milestone to a sprint delivery date."
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
    capability_gap_id: "gap-project-planning-release-cycle-001"
    capability_gap_description: "Release planning vocabulary (release plan, milestone, sprint) has no registered predicate."
    recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
    next_flow_id: "null"
  EvidenceIds: ["gap-project-planning-release-cycle-001", "gap-project-planning-milestone-001", "gap-project-planning-sprint-001"]
  Warnings:
    - "knowledge_edge: capability_gap vocabulary detected: release plan, milestone, sprint"
    - "no_registered_sub_phase_for_release_planning"

---

## B36-0C-015-002 — Release-planning five gap terms: knowledge-edge

A:
  DocumentId: "B36-0C-015-002-prompt"
  CompiledText: "Finalize the release plan timeline, assign milestones to epics, and track sprint velocity against the target."
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
    capability_gap_id: "gap-project-planning-release-cycle-001"
    recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
  EvidenceIds: ["gap-project-planning-release-cycle-001", "gap-project-planning-milestone-001", "gap-project-planning-epic-001", "gap-project-planning-sprint-001", "gap-project-planning-velocity-001"]

---

## B36-0C-015-003 — Release-planning with QA+DOCUMENT: not knowledge-edge

A:
  DocumentId: "B36-0C-015-003-prompt"
  CompiledText: "Review the release plan and confirm the milestone dates with the stakeholders."
  CompiledPredicateIds: ["QA", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  FlowId: "multi-phase-flow: 6.1 and 2.4"
  OverallVerdict: "ROUTED"
  KnowledgeEdge: false
  CapabilityGapWarnings:
    - "gap-project-planning-release-cycle-001"
    - "gap-project-planning-milestone-001"

---

## B36-0C-015-004 — Release-planning boundary: "create" without code entity

A:
  DocumentId: "B36-0C-015-004-prompt"
  CompiledText: "Create the release plan and set the milestones for Q3 and Q4."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  KnownFailureMode: "no_predicate_match_capability_gap"

Predicate analysis:
  "create" — in planning context without code-level entity (function/class/module): does NOT fire IMPLEMENT
  "release plan" — gap vocabulary
  "milestones" — gap vocabulary
  Result: knowledge_edge

B:
  FlowId: "knowledge-edge-missing-flow"
  OverallVerdict: "KNOWLEDGE_EDGE"
  FinalHumanResult: null
  Metrics:
    knowledge_edge_reason: "no_predicate_match_capability_gap"
    capability_gap_id: "gap-project-planning-release-cycle-001"
    recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
  Teaching: "create" without a code-level entity (function, class, module, service, component)
    does NOT fire IMPLEMENT. "Create the release plan" is planning vocabulary.
