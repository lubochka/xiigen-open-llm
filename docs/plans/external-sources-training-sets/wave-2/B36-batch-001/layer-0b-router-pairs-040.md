# Layer 0-B — Router Pairs (040)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-040-001 — "launch readiness" knowledge_edge routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Track launch readiness across all teams for the Q3 release."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings: ["gap-project-planning-launch-readiness-001", "gap-project-planning-release-cycle-001"]

---

## B36-0B-040-002 — "go-live checklist" + "stakeholders" DOCUMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Complete the go-live checklist and share it with all stakeholders."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.4"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-go-live-checklist-001"]

---

## B36-0B-040-003 — "feature flag" + "plan" knowledge_edge routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Plan the feature flag rollout schedule for the next three sprints."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings: ["gap-project-planning-feature-flag-001", "gap-project-planning-sprint-001"]

---

## B36-0B-040-004 — "release criteria" + "review" + "document" QA+DOCUMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Review the release criteria and document the approval process."
  CompiledPredicateIds: ["QA", "DOCUMENT"]

B:
  RoutingDecisions:
    - PredictedPhaseId: "2" | PredictedSubPhaseId: "2.4" | EvidencePredicates: ["DOCUMENT"]
    - PredictedPhaseId: "6" | PredictedSubPhaseId: "6.1" | EvidencePredicates: ["QA"]
  ExecutionOrder: "2.4 runs first; 6.1 runs after (QA-last)"
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-release-cycle-001"]

---

## B36-0B-040-005 — "sprint planning" + "build" IMPLEMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "During sprint planning, also build the notification microservice."
  CompiledPredicateIds: ["IMPLEMENT"]

B:
  PredictedPhaseId: "4"
  PredictedSubPhaseId: "4.1"
  EvidencePredicates: ["IMPLEMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-sprint-001"]
