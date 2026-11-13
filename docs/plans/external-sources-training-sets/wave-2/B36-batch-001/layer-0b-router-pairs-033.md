# Layer 0-B — Router Pairs (033)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-033-001 — "definition of done" routing complete table

A: [Reference: all routing outcomes for "definition of done" based on co-occurring verb]

B:
  RoutingTable_DefinitionOfDone:
    - verb: "align/set/present/track" → predicates: [] → KNOWLEDGE_EDGE; DomainId: project-management
    - verb: "define/specify/document" → predicates: ["DOCUMENT"] → Phase 2; DomainId: software-development
    - verb: "review" → predicates: ["QA"] → Phase 6.1; DomainId: software-development
    - verb: "implement" + code entity → predicates: ["IMPLEMENT"] → Phase 4.1; DomainId: software-development
    - verb: "test" → predicates: ["TEST"] → Phase 5.1; DomainId: software-development

---

## B36-0B-033-002 — "sprint goal" + "user story mapping" multi-gap routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Run a user story mapping session to set the sprint goal."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.1"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-user-story-mapping-001", "gap-project-planning-sprint-goal-001", "gap-project-planning-sprint-001"]
  Note: "story" fires DOCUMENT. Multiple gap warnings logged as metadata.

---

## B36-0B-033-003 — "kanban board" + "WIP" + "cycle time" knowledge_edge

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Monitor WIP limits and cycle time on the kanban board."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings: ["gap-project-planning-wip-limits-001", "gap-project-planning-cycle-time-001", "gap-project-planning-kanban-board-001"]

---

## B36-0B-033-004 — "retrospective action items" + QA + IMPLEMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "The retrospective action item is to build and review the CI/CD pipeline."
  CompiledPredicateIds: ["IMPLEMENT", "QA"]

B:
  RoutingDecisions:
    - PredictedPhaseId: "4" | PredictedSubPhaseId: "4.1" | EvidencePredicates: ["IMPLEMENT"]
    - PredictedPhaseId: "6" | PredictedSubPhaseId: "6.1" | EvidencePredicates: ["QA"]
  ExecutionOrder: "4.1 first; 6.1 last (QA-last)"
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-retrospective-action-items-001"]

---

## B36-0B-033-005 — "flow efficiency" knowledge_edge routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Calculate flow efficiency and present findings at the team retrospective."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings: ["gap-project-planning-flow-efficiency-001", "gap-project-planning-retrospective-001"]
