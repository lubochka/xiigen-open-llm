# Layer 0-B — Router Pairs (031)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-031-001 — "user story mapping" + "build" routes to Phase 4.1 + 2.1

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Facilitate a user story mapping session and build the resulting feature list."
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]

B:
  RoutingDecisions:
    - PredictedPhaseId: "4" | PredictedSubPhaseId: "4.1" | EvidencePredicates: ["IMPLEMENT"]
    - PredictedPhaseId: "2" | PredictedSubPhaseId: "2.1" | EvidencePredicates: ["DOCUMENT"]
  ExecutionOrder: "4.1 and 2.1 run in parallel"
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-user-story-mapping-001"]

---

## B36-0B-031-002 — "retrospective action items" knowledge_edge routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "List the retrospective action items from last sprint and assign priorities."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings: ["gap-project-planning-retrospective-action-items-001", "gap-project-planning-retrospective-001", "gap-project-planning-sprint-001"]

---

## B36-0B-031-003 — "flow efficiency" + "implement" routes to Phase 4.1 (IMPLEMENT)

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Implement flow efficiency metrics reporting in the dashboard."
  CompiledPredicateIds: ["IMPLEMENT"]

B:
  PredictedPhaseId: "4"
  PredictedSubPhaseId: "4.1"
  EvidencePredicates: ["IMPLEMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-flow-efficiency-001"]

---

## B36-0B-031-004 — "WIP limits" + "approve" routes to Phase 6.1 (QA)

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Approve the new WIP limits before rolling them out to all teams."
  CompiledPredicateIds: ["QA"]

B:
  PredictedPhaseId: "6"
  PredictedSubPhaseId: "6.1"
  EvidencePredicates: ["QA"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-wip-limits-001"]

---

## B36-0B-031-005 — "kanban board" + "flow efficiency" + "cycle time" knowledge_edge

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Optimize the kanban board to improve flow efficiency and reduce cycle time."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings: ["gap-project-planning-kanban-board-001", "gap-project-planning-flow-efficiency-001", "gap-project-planning-cycle-time-001"]
