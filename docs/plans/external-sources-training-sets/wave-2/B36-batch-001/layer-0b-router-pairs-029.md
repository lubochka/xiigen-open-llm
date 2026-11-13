# Layer 0-B — Router Pairs (029)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-029-001 — "retrospective action items" + "IMPLEMENT + QA" multi-predicate routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Build the retrospective action tracking dashboard and review the results."
  CompiledPredicateIds: ["IMPLEMENT", "QA"]

B:
  RoutingDecisions:
    - PredictedPhaseId: "4" | PredictedSubPhaseId: "4.1" | EvidencePredicates: ["IMPLEMENT"]
    - PredictedPhaseId: "6" | PredictedSubPhaseId: "6.1" | EvidencePredicates: ["QA"]
  ExecutionOrder: "4.1 executes first; 6.1 executes last (QA-last rule)"
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-retrospective-action-items-001", "gap-project-planning-retrospective-001"]

---

## B36-0B-029-002 — "WIP limits" + "implement" routes to Phase 4.1

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Implement WIP limit validation in the kanban service backend."
  CompiledPredicateIds: ["IMPLEMENT"]

B:
  PredictedPhaseId: "4"
  PredictedSubPhaseId: "4.1"
  EvidencePredicates: ["IMPLEMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-wip-limits-001", "gap-project-planning-kanban-board-001"]

---

## B36-0B-029-003 — "sprint goal" knowledge_edge routing (pure)

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Set the sprint goal, share it with the team, and track progress toward it."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings: ["gap-project-planning-sprint-goal-001", "gap-project-planning-sprint-001"]

---

## B36-0B-029-004 — "kanban board" + "test" routes to Phase 5.1 (TEST)

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Test the kanban board integration with our project management API."
  CompiledPredicateIds: ["TEST"]

B:
  PredictedPhaseId: "5"
  PredictedSubPhaseId: "5.1"
  EvidencePredicates: ["TEST"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-kanban-board-001"]

---

## B36-0B-029-005 — Round 2 DomainId routing summary

A: [Reference: What DomainId does the router produce for each Round 2 predicate case?]

B:
  Round2DomainIdSummary:
    - "definition of done" + no predicate → "project-management"
    - "acceptance criteria" (criteria fires DOCUMENT) → "software-development"
    - "user story mapping" (story fires DOCUMENT) → "software-development"
    - "epics breakdown" + no predicate → "project-management"
    - "sprint goal" + no predicate → "project-management"
    - "retrospective action items" + IMPLEMENT → "software-development"
    - "kanban board" + no predicate → "project-management"
    - "WIP limits" + no predicate → "project-management"
    - "flow efficiency" + no predicate → "project-management"
    - "cycle time" + no predicate → "project-management"
    - "burndown" + review → "software-development"
  Rule: DomainId is set by predicate presence, not by vocabulary proportion.
