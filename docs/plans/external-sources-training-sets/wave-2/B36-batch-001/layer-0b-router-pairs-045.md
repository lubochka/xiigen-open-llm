# Layer 0-B — Router Pairs (045)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-045-001 — "MoSCoW" + "backlog" knowledge_edge routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Apply MoSCoW (Must/Should/Could/Won't) prioritization to the backlog."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings: ["gap-project-planning-moscow-001", "gap-project-planning-backlog-001"]

---

## B36-0B-045-002 — "now-next-later" + "roadmap" knowledge_edge routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Organize the roadmap using a now-next-later framework."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings: ["gap-project-planning-now-next-later-001", "gap-project-planning-product-roadmap-001"]

---

## B36-0B-045-003 — "hypothesis" knowledge_edge routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Formulate a hypothesis for the new onboarding flow improvement."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings: ["gap-project-planning-hypothesis-001"]

---

## B36-0B-045-004 — "experiment" knowledge_edge routing (not TEST)

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Run a product experiment to validate the onboarding hypothesis."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings: ["gap-project-planning-experiment-001", "gap-project-planning-hypothesis-001"]
  Note: "'experiment' does not fire TEST. 'validate' is neutral. Knowledge-edge path."

---

## B36-0B-045-005 — "MoSCoW" + "specify" DOCUMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Specify the must-have requirements using MoSCoW (Must/Should/Could/Won't) criteria."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.4"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-moscow-001"]
