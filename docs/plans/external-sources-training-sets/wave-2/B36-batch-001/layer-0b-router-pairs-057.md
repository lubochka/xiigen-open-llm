# Layer 0-B — Router Pairs (057)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-057-001 — "t-shirt sizing" + "test" TEST routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Test the t-shirt sizing hypothesis with a small team before full adoption."
  CompiledPredicateIds: ["TEST"]

B:
  PredictedPhaseId: "5"
  PredictedSubPhaseId: "5.1"
  EvidencePredicates: ["TEST"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-t-shirt-sizing-001", "gap-project-planning-hypothesis-001"]

---

## B36-0B-057-002 — "north-star" + "story splitting" DOCUMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Use story splitting to break down north-star metric improvements into deliverables."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.4"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-story-splitting-001", "gap-project-planning-north-star-metric-001"]

---

## B36-0B-057-003 — "OKRs" + "review" + "implement" QA+IMPLEMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Review the OKRs and implement the tracking module for the dashboard."
  CompiledPredicateIds: ["QA", "IMPLEMENT"]

B:
  RoutingDecisions:
    - PredictedPhaseId: "4" | PredictedSubPhaseId: "4.1" | EvidencePredicates: ["IMPLEMENT"]
    - PredictedPhaseId: "6" | PredictedSubPhaseId: "6.1" | EvidencePredicates: ["QA"]
  ExecutionOrder: "4.1 runs first; 6.1 runs after (QA-last)"
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-okr-001"]

---

## B36-0B-057-004 — "KPIs" + "design" ARCHITECTURE routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Design the KPI (Key Performance Indicator) monitoring architecture."
  CompiledPredicateIds: ["ARCHITECTURE"]

B:
  PredictedPhaseId: "3"
  PredictedSubPhaseId: "3.4"
  EvidencePredicates: ["ARCHITECTURE"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-kpi-001"]

---

## B36-0B-057-005 — "experiment" + "MoSCoW" + "hypothesis" knowledge_edge routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Prioritize experiments using MoSCoW (Must/Should/Could/Won't) based on each hypothesis."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings:
    - "gap-project-planning-experiment-001"
    - "gap-project-planning-moscow-001"
    - "gap-project-planning-hypothesis-001"
