# Layer 0-B — Router Pairs (038)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-038-001 — "backlog refinement" knowledge_edge routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Schedule the backlog refinement session for next Tuesday."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings: ["gap-project-planning-backlog-001", "gap-project-planning-refinement-001"]

---

## B36-0B-038-002 — "product increment" + "verify" knowledge_edge routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Verify the product increment meets the definition of done before releasing."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings: ["gap-project-planning-product-increment-001", "gap-project-planning-definition-of-done-001"]

---

## B36-0B-038-003 — "MVP" + "define" DOCUMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Define the MVP for the first release and align the team on scope."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.4"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-mvp-001", "gap-project-planning-release-cycle-001"]

---

## B36-0B-038-004 — "sprint demo" + "build" IMPLEMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Build the demo environment for the sprint demo next Friday."
  CompiledPredicateIds: ["IMPLEMENT"]

B:
  PredictedPhaseId: "4"
  PredictedSubPhaseId: "4.1"
  EvidencePredicates: ["IMPLEMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-sprint-demo-001", "gap-project-planning-sprint-001"]

---

## B36-0B-038-005 — "backlog refinement" + "document" DOCUMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Document the outcomes of the backlog refinement and update the backlog."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.4"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-backlog-001", "gap-project-planning-refinement-001"]
