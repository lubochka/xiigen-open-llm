# Layer 0-B — Router Pairs (051)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-051-001 — "MoSCoW" + "now-next-later" + "review" QA routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Review the MoSCoW (Must/Should/Could/Won't) and now-next-later priorities with the team."
  CompiledPredicateIds: ["QA"]

B:
  PredictedPhaseId: "6"
  PredictedSubPhaseId: "6.1"
  EvidencePredicates: ["QA"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-moscow-001", "gap-project-planning-now-next-later-001"]

---

## B36-0B-051-002 — "RICE" + "story splitting" DOCUMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Use RICE scoring (Reach, Impact, Confidence, Effort) to prioritize after story splitting."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.4"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-rice-scoring-001", "gap-project-planning-story-splitting-001"]

---

## B36-0B-051-003 — "north-star" + "KPIs" + "OKRs" knowledge_edge routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Align north-star metric, KPIs (Key Performance Indicators), and OKRs (Objectives and Key Results) for the quarter."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings:
    - "gap-project-planning-north-star-metric-001"
    - "gap-project-planning-kpi-001"
    - "gap-project-planning-okr-001"

---

## B36-0B-051-004 — "impact mapping" + "requirements" DOCUMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Capture the requirements identified in the impact mapping session."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.4"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-impact-mapping-001"]

---

## B36-0B-051-005 — "hypothesis" + "experiment" knowledge_edge routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Formulate a hypothesis and run an experiment to validate the pricing change."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings: ["gap-project-planning-hypothesis-001", "gap-project-planning-experiment-001"]
