# Layer 0-B — Router Pairs (049)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-049-001 — "OKRs" + "KPIs" + "north-star" knowledge_edge routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Establish OKRs, KPIs, and the north-star metric for the product team."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings:
    - "gap-project-planning-okr-001"
    - "gap-project-planning-kpi-001"
    - "gap-project-planning-north-star-metric-001"

---

## B36-0B-049-002 — "impact mapping" + "story splitting" DOCUMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Use impact mapping and story splitting to break down the quarterly goals."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.4"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-impact-mapping-001", "gap-project-planning-story-splitting-001"]

---

## B36-0B-049-003 — "RICE" + "MoSCoW" + "specify" DOCUMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Specify the scoring criteria using RICE (Reach, Impact, Confidence, Effort) and MoSCoW (Must/Should/Could/Won't)."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.4"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-rice-scoring-001", "gap-project-planning-moscow-001"]

---

## B36-0B-049-004 — "experiment" + "now-next-later" knowledge_edge routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Prioritize experiments using a now-next-later framework to meet OKRs."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings:
    - "gap-project-planning-experiment-001"
    - "gap-project-planning-now-next-later-001"
    - "gap-project-planning-okr-001"

---

## B36-0B-049-005 — "now-next-later" + "build" IMPLEMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Build a now-next-later planning widget for the team dashboard."
  CompiledPredicateIds: ["IMPLEMENT"]

B:
  PredictedPhaseId: "4"
  PredictedSubPhaseId: "4.1"
  EvidencePredicates: ["IMPLEMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-now-next-later-001"]
