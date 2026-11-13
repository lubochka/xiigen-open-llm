# Layer 0-B — Router Pairs (042)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-042-001 — "daily standup" + "update" knowledge_edge routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Update the daily standup board with each team member's blockers."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings: ["gap-project-planning-standup-001", "gap-project-planning-daily-standup-001"]

---

## B36-0B-042-002 — "product increment" + "specify" DOCUMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Specify the requirements for the next product increment."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.4"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-product-increment-001"]

---

## B36-0B-042-003 — "sprint review" v6-02 guard routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Attend the sprint review to observe team progress."
  CompiledPredicateIds: ["QA"]

B:
  PredictedPhaseId: "6"
  PredictedSubPhaseId: "6.1"
  EvidencePredicates: ["QA"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-sprint-001"]
  Note: "v6-02 guard: 'review' fires QA unconditionally — even in 'sprint review' or 'observe' context."

---

## B36-0B-042-004 — "go-live checklist" + "test" + "review" TEST+QA routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Test and review each item on the go-live checklist before deployment."
  CompiledPredicateIds: ["TEST", "QA"]

B:
  RoutingDecisions:
    - PredictedPhaseId: "5" | PredictedSubPhaseId: "5.1" | EvidencePredicates: ["TEST"]
    - PredictedPhaseId: "6" | PredictedSubPhaseId: "6.1" | EvidencePredicates: ["QA"]
  ExecutionOrder: "5.1 runs first; 6.1 runs after (QA-last)"
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-go-live-checklist-001"]

---

## B36-0B-042-005 — "launch readiness" + "design" ARCHITECTURE routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Design the launch readiness dashboard for release tracking."
  CompiledPredicateIds: ["ARCHITECTURE"]

B:
  PredictedPhaseId: "3"
  PredictedSubPhaseId: "3.4"
  EvidencePredicates: ["ARCHITECTURE"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-launch-readiness-001"]
