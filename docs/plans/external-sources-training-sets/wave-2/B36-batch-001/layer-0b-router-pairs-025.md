# Layer 0-B — Router Pairs (025)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-025-001 — "definition of done" knowledge_edge routing

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-025-001-prompt"
  CompiledText: "Align the team on our definition of done before the sprint kickoff."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedPhaseId: null
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings: ["gap-project-planning-definition-of-done-001", "gap-project-planning-sprint-001"]
  FailureClassification: "no_predicate_match_capability_gap"

---

## B36-0B-025-002 — "acceptance criteria" routes to Phase 2.4 (DOCUMENT)

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-025-002-prompt"
  CompiledText: "Write out the acceptance criteria for the backlog items."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.4"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-acceptance-criteria-001", "gap-project-planning-backlog-001"]

---

## B36-0B-025-003 — "sprint goal" + "stakeholder" routes to Phase 2 (DOCUMENT)

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-025-003-prompt"
  CompiledText: "Communicate the sprint goal to all stakeholders before the kickoff."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.1"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-sprint-goal-001", "gap-project-planning-sprint-001"]

---

## B36-0B-025-004 — "retrospective action item = build" routes to Phase 4.1 (IMPLEMENT)

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-025-004-prompt"
  CompiledText: "The retrospective action item is to build an automated deployment pipeline."
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"

B:
  PredictedPhaseId: "4"
  PredictedSubPhaseId: "4.1"
  EvidencePredicates: ["IMPLEMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-retrospective-001"]

---

## B36-0B-025-005 — "kanban board" + "WIP limits" knowledge_edge routing

A: [SubPhaseRouterSourceDocument]
  DocumentId: "B36-025-005-prompt"
  CompiledText: "Set up the kanban board columns and configure the WIP limits."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedPhaseId: null
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings: ["gap-project-planning-kanban-board-001", "gap-project-planning-wip-limits-001"]
  FailureClassification: "no_predicate_match_capability_gap"
