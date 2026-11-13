# Layer 0-B — Router Pairs (043)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-043-001 — "OKRs" knowledge_edge routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Set the OKRs (Objectives and Key Results) for the upcoming quarter."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings: ["gap-project-planning-okr-001"]

---

## B36-0B-043-002 — "KPIs" + "sprint" + "velocity" knowledge_edge routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Track KPIs (Key Performance Indicators) for the sprint velocity and throughput."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings:
    - "gap-project-planning-kpi-001"
    - "gap-project-planning-velocity-001"
    - "gap-project-planning-sprint-001"

---

## B36-0B-043-003 — "north-star metric" + "sprint" knowledge_edge routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Align the team around the north-star metric before starting the sprint."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings: ["gap-project-planning-north-star-metric-001", "gap-project-planning-sprint-001"]

---

## B36-0B-043-004 — "OKRs" + "define" DOCUMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Define the OKRs (Objectives and Key Results) for each product team this quarter."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.4"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-okr-001"]

---

## B36-0B-043-005 — "KPIs" + "build" IMPLEMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Build the KPI (Key Performance Indicator) tracking dashboard."
  CompiledPredicateIds: ["IMPLEMENT"]

B:
  PredictedPhaseId: "4"
  PredictedSubPhaseId: "4.1"
  EvidencePredicates: ["IMPLEMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-kpi-001"]
