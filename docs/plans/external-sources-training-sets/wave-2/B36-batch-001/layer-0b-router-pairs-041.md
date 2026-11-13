# Layer 0-B — Router Pairs (041)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-041-001 — "sprint demo" + "review" QA routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Review the sprint demo outputs before the final release decision."
  CompiledPredicateIds: ["QA"]

B:
  PredictedPhaseId: "6"
  PredictedSubPhaseId: "6.1"
  EvidencePredicates: ["QA"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-sprint-demo-001", "gap-project-planning-sprint-001"]

---

## B36-0B-041-002 — "backlog refinement" + "define" DOCUMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Define the acceptance criteria during backlog refinement."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.4"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings:
    - "gap-project-planning-backlog-001"
    - "gap-project-planning-refinement-001"
    - "gap-project-planning-acceptance-criteria-001"

---

## B36-0B-041-003 — "scrum ceremony" + "iteration" knowledge_edge routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Facilitate all four scrum ceremonies during the two-week iteration."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings: ["gap-project-planning-scrum-ceremony-001", "gap-project-planning-iteration-001"]

---

## B36-0B-041-004 — "launch readiness" + "test" TEST routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Test the launch readiness checklist items for the payment service."
  CompiledPredicateIds: ["TEST"]

B:
  PredictedPhaseId: "5"
  PredictedSubPhaseId: "5.1"
  EvidencePredicates: ["TEST"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-launch-readiness-001", "gap-project-planning-go-live-checklist-001"]

---

## B36-0B-041-005 — "MVP" + "implement" IMPLEMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Implement the core features listed in the MVP definition."
  CompiledPredicateIds: ["IMPLEMENT"]

B:
  PredictedPhaseId: "4"
  PredictedSubPhaseId: "4.1"
  EvidencePredicates: ["IMPLEMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-mvp-001"]
