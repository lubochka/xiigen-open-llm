# Layer 0-B — Router Pairs (034)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-034-001 — "burndown" + "flow efficiency" knowledge_edge

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Optimize flow efficiency by addressing bottlenecks identified in the burndown."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings: ["gap-project-planning-flow-efficiency-001", "gap-project-planning-burndown-001"]

---

## B36-0B-034-002 — "sprint goal" + "acceptance criteria" + DOCUMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Communicate the sprint goal and define acceptance criteria for all items."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.4"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-sprint-goal-001", "gap-project-planning-acceptance-criteria-001"]

---

## B36-0B-034-003 — "epics breakdown" + "IMPLEMENT + DOCUMENT" multi-predicate routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Break down epics into tasks, document requirements, and build the first iteration."
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]

B:
  RoutingDecisions:
    - PredictedPhaseId: "4" | PredictedSubPhaseId: "4.1" | EvidencePredicates: ["IMPLEMENT"]
    - PredictedPhaseId: "2" | PredictedSubPhaseId: "2.4" | EvidencePredicates: ["DOCUMENT"]
  ExecutionOrder: "4.1 and 2.4 run in parallel"
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-epic-001", "gap-project-planning-iteration-001"]

---

## B36-0B-034-004 — "WIP limits" + "criteria" routes to Phase 2.4

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Specify the criteria for WIP limit changes across teams."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.4"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-wip-limits-001"]

---

## B36-0B-034-005 — "cycle time" knowledge_edge routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Track cycle time per ticket type and present findings in the retrospective."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings: ["gap-project-planning-cycle-time-001", "gap-project-planning-ticket-001", "gap-project-planning-retrospective-001"]
