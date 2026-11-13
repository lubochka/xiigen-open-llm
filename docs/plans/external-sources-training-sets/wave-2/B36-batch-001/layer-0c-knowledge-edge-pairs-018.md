# Layer 0-C — Knowledge-Edge Pairs (018)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B36-0C-018-001 — Prioritize/triage: knowledge-edge full path

A:
  DocumentId: "B36-0C-018-001-prompt"
  CompiledText: "Prioritize the backlog items and triage the incoming tickets for the upcoming sprint."
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
    capability_gap_description: "Prioritization vocabulary (prioritize, backlog, triage, tickets, sprint) has no registered predicate."
    recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
    next_flow_id: "null"
  EvidenceIds: ["gap-project-planning-backlog-001", "gap-project-planning-ticket-001", "gap-project-planning-sprint-001"]

---

## B36-0C-018-002 — Triage five gap terms: knowledge-edge

A:
  DocumentId: "B36-0C-018-002-prompt"
  CompiledText: "Triage the epic backlog, prioritize the sprint tickets, and set velocity targets for the next iteration."
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
  EvidenceIds: ["gap-project-planning-epic-001", "gap-project-planning-backlog-001", "gap-project-planning-sprint-001", "gap-project-planning-ticket-001", "gap-project-planning-velocity-001", "gap-project-planning-iteration-001"]

---

## B36-0C-018-003 — Prioritize with DOCUMENT: not knowledge-edge

A:
  DocumentId: "B36-0C-018-003-prompt"
  CompiledText: "Triage the incoming backlog and define the priority criteria for each ticket type."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  FlowId: "documentation-requirements-flow-2.4"
  OverallVerdict: "ROUTED"
  KnowledgeEdge: false
  CapabilityGapWarnings:
    - "gap-project-planning-backlog-001"
    - "gap-project-planning-ticket-001"

---

## B36-0C-018-004 — "triage" alone: pure gap, no DOCUMENT confusion

A:
  DocumentId: "B36-0C-018-004-prompt"
  CompiledText: "Triage the new bug tickets and assign them to the right sprint."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

Predicate analysis:
  "triage" — not in any predicate lexicon
  "tickets" — not in any predicate lexicon (gap)
  "assign" — not in any predicate lexicon
  "sprint" — not in any predicate lexicon (gap)
  Note: "bug" is NOT in TEST or QA lexicon in this context (bug ≠ "test", "test case")
  Result: CompiledPredicateIds: [], knowledge_edge

B:
  FlowId: "knowledge-edge-missing-flow"
  OverallVerdict: "KNOWLEDGE_EDGE"
  FinalHumanResult: null
  Metrics:
    knowledge_edge_reason: "no_predicate_match_capability_gap"
    capability_gap_id: "gap-project-planning-ticket-001"
    recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
  EvidenceIds: ["gap-project-planning-ticket-001", "gap-project-planning-sprint-001"]
  Teaching: "triage" and "tickets" are gap vocabulary. Neither fires TEST or QA.
    "Bug tickets" as a planning/assignment task does not fire TEST.
