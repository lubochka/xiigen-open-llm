# Layer 0-B — Router Pairs (055)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-055-001 — "impact mapping" + "story" + "implement" DOCUMENT+IMPLEMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "After impact mapping and story splitting, implement the top-priority stories."
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]

B:
  RoutingDecisions:
    - PredictedPhaseId: "2" | PredictedSubPhaseId: "2.4" | EvidencePredicates: ["DOCUMENT"]
    - PredictedPhaseId: "4" | PredictedSubPhaseId: "4.1" | EvidencePredicates: ["IMPLEMENT"]
  ExecutionOrder: "2.4 and 4.1 run in parallel"
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-impact-mapping-001", "gap-project-planning-story-splitting-001"]

---

## B36-0B-055-002 — "OKRs" + "KPIs" + "north-star" knowledge_edge routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Cascade OKRs down to KPIs and track them against the north-star metric."
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

## B36-0B-055-003 — "experiment" + "implement" IMPLEMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Implement the experiment as a feature flag to measure its impact."
  CompiledPredicateIds: ["IMPLEMENT"]

B:
  PredictedPhaseId: "4"
  PredictedSubPhaseId: "4.1"
  EvidencePredicates: ["IMPLEMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-experiment-001", "gap-project-planning-feature-flag-001"]

---

## B36-0B-055-004 — "t-shirt sizing" + "define" DOCUMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Define the t-shirt sizing scale for the engineering team."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.4"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-t-shirt-sizing-001"]

---

## B36-0B-055-005 — "now-next-later" + "OKRs" + "review" QA routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Review the now-next-later roadmap against the OKRs with the product team."
  CompiledPredicateIds: ["QA"]

B:
  PredictedPhaseId: "6"
  PredictedSubPhaseId: "6.1"
  EvidencePredicates: ["QA"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-now-next-later-001", "gap-project-planning-okr-001"]
