# Layer 0-B — Router Pairs (059)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-059-001 — "RICE + MoSCoW + story + now-next-later" DOCUMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Prioritize stories using RICE scoring (Reach, Impact, Confidence, Effort) and MoSCoW (Must/Should/Could/Won't), then place them in the now-next-later framework."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.4"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings:
    - "gap-project-planning-rice-scoring-001"
    - "gap-project-planning-moscow-001"
    - "gap-project-planning-now-next-later-001"

---

## B36-0B-059-002 — "KPIs" + "hypothesis" + "test" TEST routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Test whether the new KPIs align with the north-star hypothesis."
  CompiledPredicateIds: ["TEST"]

B:
  PredictedPhaseId: "5"
  PredictedSubPhaseId: "5.1"
  EvidencePredicates: ["TEST"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings:
    - "gap-project-planning-kpi-001"
    - "gap-project-planning-north-star-metric-001"
    - "gap-project-planning-hypothesis-001"

---

## B36-0B-059-003 — "impact mapping" + "RICE" + "story" + "implement" DOCUMENT+IMPLEMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "After impact mapping and RICE scoring (Reach, Impact, Confidence, Effort), implement story-split features."
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]

B:
  RoutingDecisions:
    - PredictedPhaseId: "2" | PredictedSubPhaseId: "2.4" | EvidencePredicates: ["DOCUMENT"]
    - PredictedPhaseId: "4" | PredictedSubPhaseId: "4.1" | EvidencePredicates: ["IMPLEMENT"]
  ExecutionOrder: "2.4 and 4.1 run in parallel"
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings:
    - "gap-project-planning-impact-mapping-001"
    - "gap-project-planning-rice-scoring-001"
    - "gap-project-planning-story-splitting-001"

---

## B36-0B-059-004 — "experiment" + "now-next-later" + "OKRs" knowledge_edge routing

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

## B36-0B-059-005 — v6-03 guard: DomainId must be software-development when predicate fires

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Prioritize user stories in the backlog for next sprint."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.4"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-backlog-001", "gap-project-planning-sprint-001"]
  Note: "v6-03 guard: DOCUMENT fired → DomainId must be 'software-development'.
    Gap vocabulary (backlog, sprint) does not override the DomainId rule."
