# Layer 0-B — Router Pairs (030)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-030-001 — "definition of done" + "behavior" routes to Phase 2 (DOCUMENT)

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Update the definition of done to include expected behavior for each deliverable."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.1"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-definition-of-done-001"]

---

## B36-0B-030-002 — "acceptance criteria" + "implement" routes to Phase 4.1 + 2.4

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Implement the acceptance criteria validation feature for the backlog items."
  CompiledPredicateIds: ["IMPLEMENT", "DOCUMENT"]

B:
  RoutingDecisions:
    - PredictedPhaseId: "4" | PredictedSubPhaseId: "4.1" | EvidencePredicates: ["IMPLEMENT"]
    - PredictedPhaseId: "2" | PredictedSubPhaseId: "2.4" | EvidencePredicates: ["DOCUMENT"]
  ExecutionOrder: "4.1 and 2.4 run in parallel"
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-acceptance-criteria-001", "gap-project-planning-backlog-001"]

---

## B36-0B-030-003 — "epics breakdown" + "DOCUMENT" routes to Phase 2 (DOCUMENT)

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Document the epics breakdown and specify acceptance criteria for each."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.4"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-epic-001", "gap-project-planning-acceptance-criteria-001"]

---

## B36-0B-030-004 — "sprint goal" + "requirements" routes to Phase 2.4 (DOCUMENT)

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Document the sprint goal requirements and share with the product team."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.4"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-sprint-goal-001", "gap-project-planning-sprint-001"]

---

## B36-0B-030-005 — "burndown" + "WIP" knowledge_edge routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Present the burndown chart to the team at the end of each sprint."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings: ["gap-project-planning-burndown-001", "gap-project-planning-sprint-001"]
