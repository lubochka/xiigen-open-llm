# Layer 0-B — Router Pairs (039)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-039-001 — "sprint review" QA routing (lone QA)

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Lead the sprint review session and capture feedback from the product owner."
  CompiledPredicateIds: ["QA"]

B:
  PredictedPhaseId: "6"
  PredictedSubPhaseId: "6.1"
  EvidencePredicates: ["QA"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-sprint-001"]

---

## B36-0B-039-002 — "release criteria" + "criteria" DOCUMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Establish the release criteria before the team signs off on the sprint."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.4"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-release-cycle-001", "gap-project-planning-sprint-001"]

---

## B36-0B-039-003 — "feature flag" + "implement" IMPLEMENT routing

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

---

## B36-0B-039-004 — "go-live checklist" + "review" QA routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Review the go-live checklist before the production deployment."
  CompiledPredicateIds: ["QA"]

B:
  PredictedPhaseId: "6"
  PredictedSubPhaseId: "6.1"
  EvidencePredicates: ["QA"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-go-live-checklist-001"]

---

## B36-0B-039-005 — "product increment" + "assess" knowledge_edge routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Assess the product increment against the sprint goal and capacity."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings:
    - "gap-project-planning-product-increment-001"
    - "gap-project-planning-sprint-goal-001"
    - "gap-project-planning-sprint-001"
    - "gap-project-planning-capacity-001"
