# Layer 0-B — Router Pairs (058)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-058-001 — "RICE" + "story splitting" + "implement" DOCUMENT+IMPLEMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "After RICE scoring (Reach, Impact, Confidence, Effort) and story splitting, implement the top-priority stories."
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]

B:
  RoutingDecisions:
    - PredictedPhaseId: "2" | PredictedSubPhaseId: "2.4" | EvidencePredicates: ["DOCUMENT"]
    - PredictedPhaseId: "4" | PredictedSubPhaseId: "4.1" | EvidencePredicates: ["IMPLEMENT"]
  ExecutionOrder: "2.4 and 4.1 run in parallel"
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-rice-scoring-001", "gap-project-planning-story-splitting-001"]

---

## B36-0B-058-002 — "north-star" + "OKRs" + "review" + "implement" QA+IMPLEMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Review the north-star metric against OKRs and implement the missing tracker."
  CompiledPredicateIds: ["QA", "IMPLEMENT"]

B:
  RoutingDecisions:
    - PredictedPhaseId: "4" | PredictedSubPhaseId: "4.1" | EvidencePredicates: ["IMPLEMENT"]
    - PredictedPhaseId: "6" | PredictedSubPhaseId: "6.1" | EvidencePredicates: ["QA"]
  ExecutionOrder: "4.1 runs first; 6.1 runs after (QA-last)"
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-north-star-metric-001", "gap-project-planning-okr-001"]

---

## B36-0B-058-003 — "impact mapping" + "t-shirt sizing" + "MoSCoW" knowledge_edge routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Run impact mapping, use t-shirt sizing, and apply MoSCoW (Must/Should/Could/Won't) to finalize scope."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings:
    - "gap-project-planning-impact-mapping-001"
    - "gap-project-planning-t-shirt-sizing-001"
    - "gap-project-planning-moscow-001"

---

## B36-0B-058-004 — "hypothesis" + "experiment" + "review" QA routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Review the experiment results and decide if the hypothesis was validated."
  CompiledPredicateIds: ["QA"]

B:
  PredictedPhaseId: "6"
  PredictedSubPhaseId: "6.1"
  EvidencePredicates: ["QA"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-experiment-001", "gap-project-planning-hypothesis-001"]

---

## B36-0B-058-005 — "KPIs" + "OKRs" + "north-star" + "build" IMPLEMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Build the KPI and OKR dashboard that tracks the north-star metric."
  CompiledPredicateIds: ["IMPLEMENT"]

B:
  PredictedPhaseId: "4"
  PredictedSubPhaseId: "4.1"
  EvidencePredicates: ["IMPLEMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings:
    - "gap-project-planning-kpi-001"
    - "gap-project-planning-okr-001"
    - "gap-project-planning-north-star-metric-001"
