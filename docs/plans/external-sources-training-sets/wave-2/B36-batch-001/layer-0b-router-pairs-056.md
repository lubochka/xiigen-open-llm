# Layer 0-B — Router Pairs (056)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-056-001 — "RICE scoring" + "architect" ARCHITECTURE routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Architect the scoring engine that implements RICE (Reach, Impact, Confidence, Effort) calculations."
  CompiledPredicateIds: ["ARCHITECTURE"]

B:
  PredictedPhaseId: "3"
  PredictedSubPhaseId: "3.4"
  EvidencePredicates: ["ARCHITECTURE"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-rice-scoring-001"]

---

## B36-0B-056-002 — "story splitting" + "RICE" + "review" QA+DOCUMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Review the RICE scoring (Reach, Impact, Confidence, Effort) after story splitting."
  CompiledPredicateIds: ["QA", "DOCUMENT"]

B:
  RoutingDecisions:
    - PredictedPhaseId: "2" | PredictedSubPhaseId: "2.4" | EvidencePredicates: ["DOCUMENT"]
    - PredictedPhaseId: "6" | PredictedSubPhaseId: "6.1" | EvidencePredicates: ["QA"]
  ExecutionOrder: "2.4 runs first; 6.1 runs after (QA-last)"
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-rice-scoring-001", "gap-project-planning-story-splitting-001"]

---

## B36-0B-056-003 — "experiment" + "hypothesis" + "north-star" knowledge_edge routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Generate an experiment hypothesis to improve the north-star metric."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings:
    - "gap-project-planning-experiment-001"
    - "gap-project-planning-hypothesis-001"
    - "gap-project-planning-north-star-metric-001"

---

## B36-0B-056-004 — "MoSCoW" + "OKRs" + "KPIs" knowledge_edge routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Align MoSCoW (Must/Should/Could/Won't) priorities with OKRs and KPIs for the quarter."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings:
    - "gap-project-planning-moscow-001"
    - "gap-project-planning-okr-001"
    - "gap-project-planning-kpi-001"

---

## B36-0B-056-005 — "impact mapping" + "now-next-later" + "implement" IMPLEMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Implement the now-next-later features identified in the impact mapping."
  CompiledPredicateIds: ["IMPLEMENT"]

B:
  PredictedPhaseId: "4"
  PredictedSubPhaseId: "4.1"
  EvidencePredicates: ["IMPLEMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-now-next-later-001", "gap-project-planning-impact-mapping-001"]
