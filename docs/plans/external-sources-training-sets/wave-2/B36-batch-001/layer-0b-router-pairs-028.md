# Layer 0-B — Router Pairs (028)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-028-001 — "sprint goal" + "definition of done" knowledge_edge routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Align sprint goal with the definition of done for the current iteration."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings: ["gap-project-planning-sprint-goal-001", "gap-project-planning-definition-of-done-001", "gap-project-planning-sprint-001", "gap-project-planning-iteration-001"]

---

## B36-0B-028-002 — "epics breakdown" + "DOCUMENT + ARCHITECTURE" multi-predicate routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Document the architecture for the epics breakdown into microservices."
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE"]

B:
  RoutingDecisions:
    - PredictedPhaseId: "2" | PredictedSubPhaseId: "2.1" | EvidencePredicates: ["DOCUMENT"]
    - PredictedPhaseId: "3" | PredictedSubPhaseId: "3.4" | EvidencePredicates: ["ARCHITECTURE"]
  DecisionCount: 2
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-epic-001"]

---

## B36-0B-028-003 — "acceptance criteria" + "test" routes to Phase 5.1 (TEST)

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Run the acceptance tests for the sprint deliverables."
  CompiledPredicateIds: ["TEST"]

B:
  PredictedPhaseId: "5"
  PredictedSubPhaseId: "5.1"
  EvidencePredicates: ["TEST"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-sprint-001"]
  Note: "acceptance tests" → TEST via "test" token. "acceptance criteria" alone → DOCUMENT via "criteria".
    The adjacent noun determines the predicate token that fires.

---

## B36-0B-028-004 — "cycle time" + "approve" routes to Phase 6.1 (QA)

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Approve the cycle time benchmarks for the release."
  CompiledPredicateIds: ["QA"]

B:
  PredictedPhaseId: "6"
  PredictedSubPhaseId: "6.1"
  EvidencePredicates: ["QA"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-cycle-time-001", "gap-project-planning-release-cycle-001"]

---

## B36-0B-028-005 — "burndown" + "kanban" + "WIP" knowledge_edge mega-gap

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Monitor WIP limits, cycle time, and flow efficiency metrics on the kanban board."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings: ["gap-project-planning-wip-limits-001", "gap-project-planning-cycle-time-001", "gap-project-planning-flow-efficiency-001", "gap-project-planning-kanban-board-001"]
  SCHEDULE_CAPABILITY_LEARNING: one signal, four IDs bundled
