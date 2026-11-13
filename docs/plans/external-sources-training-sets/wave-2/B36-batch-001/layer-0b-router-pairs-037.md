# Layer 0-B — Router Pairs (037)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-037-001 — Complete Round 3 routing table: scrum ceremony block

A: [Reference: routing outcomes for Round 3 scrum-ceremony gap vocabulary]

B:
  ZeroPredicateRouting_Round3_Ceremonies:
    - "scrum ceremony" (alone) → KNOWLEDGE_EDGE, DomainId: project-management
    - "daily standup" (alone) → KNOWLEDGE_EDGE, DomainId: project-management
    - "sprint demo" (alone) → KNOWLEDGE_EDGE, DomainId: project-management
    - "backlog refinement" (alone) → KNOWLEDGE_EDGE, DomainId: project-management
    - "product increment" (alone) → KNOWLEDGE_EDGE, DomainId: project-management
    - "MVP definition" (alone) → KNOWLEDGE_EDGE, DomainId: project-management
    - "launch readiness" (alone) → KNOWLEDGE_EDGE, DomainId: project-management
    - "go-live checklist" (alone) → KNOWLEDGE_EDGE, DomainId: project-management
    - "feature flag" (no code verb) → KNOWLEDGE_EDGE, DomainId: project-management
  Predicate-fired cases:
    - "sprint review" → QA fires → Phase 6.1, DomainId: software-development
    - "sprint demo" + "stakeholders" → DOCUMENT fires → Phase 2.4, DomainId: software-development
    - "feature flag" + "implement" → IMPLEMENT fires → Phase 4.1, DomainId: software-development

---

## B36-0B-037-002 — "scrum ceremony" knowledge_edge routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Facilitate the scrum ceremonies for this sprint."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings: ["gap-project-planning-scrum-ceremony-001", "gap-project-planning-sprint-001"]

---

## B36-0B-037-003 — "sprint demo" + "stakeholders" DOCUMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Prepare the sprint demo and present completed items to stakeholders."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.4"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-sprint-demo-001", "gap-project-planning-sprint-001"]

---

## B36-0B-037-004 — "sprint review" QA routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Conduct the sprint review with the product owner and stakeholders."
  CompiledPredicateIds: ["DOCUMENT", "QA"]

B:
  RoutingDecisions:
    - PredictedPhaseId: "2" | PredictedSubPhaseId: "2.4" | EvidencePredicates: ["DOCUMENT"]
    - PredictedPhaseId: "6" | PredictedSubPhaseId: "6.1" | EvidencePredicates: ["QA"]
  ExecutionOrder: "2.4 runs first; 6.1 runs after (QA-last)"
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-sprint-001"]

---

## B36-0B-037-005 — "feature flag" + "implement" IMPLEMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Implement the feature flag for the dark mode rollout."
  CompiledPredicateIds: ["IMPLEMENT"]

B:
  PredictedPhaseId: "4"
  PredictedSubPhaseId: "4.1"
  EvidencePredicates: ["IMPLEMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-feature-flag-001"]
