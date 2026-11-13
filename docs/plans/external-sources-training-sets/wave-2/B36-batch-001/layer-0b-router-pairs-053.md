# Layer 0-B — Router Pairs (053)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-053-001 — "KPIs" + "retrospective" + "review" QA routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Review the KPIs (Key Performance Indicators) at the end of the sprint retrospective."
  CompiledPredicateIds: ["QA"]

B:
  PredictedPhaseId: "6"
  PredictedSubPhaseId: "6.1"
  EvidencePredicates: ["QA"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings:
    - "gap-project-planning-kpi-001"
    - "gap-project-planning-retrospective-001"
    - "gap-project-planning-sprint-001"

---

## B36-0B-053-002 — "story splitting" + "acceptance criteria" DOCUMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "After story splitting, define acceptance criteria for each sub-story."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.4"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings:
    - "gap-project-planning-story-splitting-001"
    - "gap-project-planning-acceptance-criteria-001"

---

## B36-0B-053-003 — "RICE" + "t-shirt sizing" + "specify" DOCUMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Specify the prioritization approach: RICE scoring (Reach, Impact, Confidence, Effort) for epics, t-shirt sizing for tasks."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.4"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings:
    - "gap-project-planning-rice-scoring-001"
    - "gap-project-planning-t-shirt-sizing-001"
    - "gap-project-planning-epic-001"

---

## B36-0B-053-004 — "impact mapping" + "OKRs" + "implement" IMPLEMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Implement the feature set identified from impact mapping to meet OKR targets."
  CompiledPredicateIds: ["IMPLEMENT"]

B:
  PredictedPhaseId: "4"
  PredictedSubPhaseId: "4.1"
  EvidencePredicates: ["IMPLEMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-impact-mapping-001", "gap-project-planning-okr-001"]

---

## B36-0B-053-005 — "experiment" v6-02 guard routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Run a product experiment to measure user engagement."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings: ["gap-project-planning-experiment-001"]
  Note: "v6-02 guard: 'experiment' does not fire TEST. No predicate fires here."
