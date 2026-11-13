# Layer 0-B — Router Pairs (052)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-052-001 — "t-shirt sizing" + "story" DOCUMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Convert t-shirt sizing to story points and update the backlog."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.4"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings:
    - "gap-project-planning-t-shirt-sizing-001"
    - "gap-project-planning-story-point-001"
    - "gap-project-planning-backlog-001"

---

## B36-0B-052-002 — "OKRs" + "implement" IMPLEMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Implement the OKR tracking feature in the company dashboard."
  CompiledPredicateIds: ["IMPLEMENT"]

B:
  PredictedPhaseId: "4"
  PredictedSubPhaseId: "4.1"
  EvidencePredicates: ["IMPLEMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-okr-001"]

---

## B36-0B-052-003 — "RICE" + "MoSCoW" + "story splitting" DOCUMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Apply MoSCoW (Must/Should/Could/Won't) and RICE scoring (Reach, Impact, Confidence, Effort) after story splitting."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.4"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings:
    - "gap-project-planning-moscow-001"
    - "gap-project-planning-rice-scoring-001"
    - "gap-project-planning-story-splitting-001"

---

## B36-0B-052-004 — "north-star" + "test" TEST routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Test whether the north-star metric increased after the experiment."
  CompiledPredicateIds: ["TEST"]

B:
  PredictedPhaseId: "5"
  PredictedSubPhaseId: "5.1"
  EvidencePredicates: ["TEST"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-north-star-metric-001", "gap-project-planning-experiment-001"]

---

## B36-0B-052-005 — "now-next-later" + "hypothesis" + "impact mapping" knowledge_edge routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Run impact mapping to generate hypotheses, then organize them in a now-next-later view."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings:
    - "gap-project-planning-impact-mapping-001"
    - "gap-project-planning-hypothesis-001"
    - "gap-project-planning-now-next-later-001"
