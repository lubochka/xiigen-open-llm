# Layer 0-B — Router Pairs (035)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-035-001 — "kanban board" + "use-case" routes to Phase 2 (DOCUMENT)

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Outline the use-case for our kanban board workflow optimization."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.1"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-kanban-board-001", "gap-project-planning-flow-efficiency-001"]

---

## B36-0B-035-002 — "burndown" + "functional" routes to Phase 2 + 4 (DOCUMENT + IMPLEMENT)

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Create a functional burndown chart component for the project dashboard."
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]

B:
  RoutingDecisions:
    - PredictedPhaseId: "4" | PredictedSubPhaseId: "4.1" | EvidencePredicates: ["IMPLEMENT"]
    - PredictedPhaseId: "2" | PredictedSubPhaseId: "2.1" | EvidencePredicates: ["DOCUMENT"]
  ExecutionOrder: "4.1 and 2.1 run in parallel"
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-burndown-001"]

---

## B36-0B-035-003 — "definition of done" + "sprint goal" + "epics" knowledge_edge

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Plan the sprint: set the sprint goal, epics breakdown, and definition of done."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings: ["gap-project-planning-sprint-001", "gap-project-planning-sprint-goal-001", "gap-project-planning-epic-001", "gap-project-planning-definition-of-done-001"]

---

## B36-0B-035-004 — "WIP limits" + "design" routes to Phase 3.4 (ARCHITECTURE)

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Design the WIP limit enforcement architecture for the project tracking system."
  CompiledPredicateIds: ["ARCHITECTURE"]

B:
  PredictedPhaseId: "3"
  PredictedSubPhaseId: "3.4"
  EvidencePredicates: ["ARCHITECTURE"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-wip-limits-001"]

---

## B36-0B-035-005 — "retrospective action items" + knowledge_edge + SCHEDULE_CAPABILITY_LEARNING

A: [Reference: Does SCHEDULE_CAPABILITY_LEARNING fire for retrospective action items?]

B:
  Rule: "SCHEDULE_CAPABILITY_LEARNING fires when CompiledPredicateIds: [] and gap vocabulary
    is present. For 'retrospective action items' — if no predicate co-occurs, then yes."
  Example:
    request: "Capture retrospective action items and assign owners."
    predicates: []
    SCHEDULE_CAPABILITY_LEARNING: emitted
    capabilityGapIds: ["gap-project-planning-retrospective-action-items-001", "gap-project-planning-retrospective-001"]
  Counter-example:
    request: "Build the retrospective action item tracking dashboard."
    predicates: ["IMPLEMENT"]
    SCHEDULE_CAPABILITY_LEARNING: NOT emitted
