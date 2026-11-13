# Layer 0-B — Router Pairs (036)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-036-001 — Complete Round 2 routing reference table

A: [Reference: complete routing outcomes for Round 2 gap vocabulary, zero-predicate cases]

B:
  ZeroPredicateRouting_Round2:
    - "definition of done" (alone) → KNOWLEDGE_EDGE, DomainId: project-management
    - "sprint goal" (alone) → KNOWLEDGE_EDGE, DomainId: project-management
    - "retrospective action items" (alone) → KNOWLEDGE_EDGE, DomainId: project-management
    - "kanban board" (alone) → KNOWLEDGE_EDGE, DomainId: project-management
    - "WIP limits" (alone) → KNOWLEDGE_EDGE, DomainId: project-management
    - "flow efficiency" (alone) → KNOWLEDGE_EDGE, DomainId: project-management
    - "cycle time" (alone) → KNOWLEDGE_EDGE, DomainId: project-management
    - "epics breakdown" (alone) → KNOWLEDGE_EDGE, DomainId: project-management
    - "burndown" (alone) → KNOWLEDGE_EDGE, DomainId: project-management
  AlwaysPredicateRouting_Round2:
    - "acceptance criteria" → Phase 2.4, DomainId: software-development (criteria fires DOCUMENT)
    - "user story mapping" → Phase 2.1, DomainId: software-development (story fires DOCUMENT)

---

## B36-0B-036-002 — "sprint goal" + "definition of done" + "review" routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Review the sprint goal and definition of done before the sprint starts."
  CompiledPredicateIds: ["QA"]

B:
  PredictedPhaseId: "6"
  PredictedSubPhaseId: "6.1"
  EvidencePredicates: ["QA"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-sprint-goal-001", "gap-project-planning-definition-of-done-001", "gap-project-planning-sprint-001"]

---

## B36-0B-036-003 — "burndown" + "IMPLEMENT + DOCUMENT + QA" three-predicate routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Build the burndown tracker, document requirements, and review outputs."
  CompiledPredicateIds: ["IMPLEMENT", "DOCUMENT", "QA"]

B:
  RoutingDecisions:
    - PredictedPhaseId: "4" | PredictedSubPhaseId: "4.1" | EvidencePredicates: ["IMPLEMENT"]
    - PredictedPhaseId: "2" | PredictedSubPhaseId: "2.4" | EvidencePredicates: ["DOCUMENT"]
    - PredictedPhaseId: "6" | PredictedSubPhaseId: "6.1" | EvidencePredicates: ["QA"]
  ExecutionOrder: "4.1 and 2.4 parallel; 6.1 last (QA-last)"
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-burndown-001"]

---

## B36-0B-036-004 — "cycle time" + "flow efficiency" knowledge_edge

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Calculate flow efficiency and average cycle time for the past two sprints."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings: ["gap-project-planning-flow-efficiency-001", "gap-project-planning-cycle-time-001", "gap-project-planning-sprint-001"]

---

## B36-0B-036-005 — "kanban board" + "acceptance criteria" routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Specify the criteria for moving tickets to done on the kanban board."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.4"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-kanban-board-001", "gap-project-planning-ticket-001", "gap-project-planning-definition-of-done-001"]
