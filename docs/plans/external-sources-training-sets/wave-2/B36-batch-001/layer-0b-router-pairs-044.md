# Layer 0-B — Router Pairs (044)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-044-001 — "impact mapping" knowledge_edge routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Run an impact mapping session to align features with business goals."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings: ["gap-project-planning-impact-mapping-001"]

---

## B36-0B-044-002 — "story splitting" + "story" DOCUMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Practice story splitting to break down the large epic into smaller items."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.4"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-story-splitting-001", "gap-project-planning-epic-001"]
  Note: "'story' in 'story splitting' fires DOCUMENT — false positive but correct output."

---

## B36-0B-044-003 — "t-shirt sizing" + "backlog" knowledge_edge routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Use t-shirt sizing to estimate effort for the backlog items."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings: ["gap-project-planning-t-shirt-sizing-001", "gap-project-planning-backlog-001"]

---

## B36-0B-044-004 — "RICE scoring" + "roadmap" knowledge_edge routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Apply RICE scoring (Reach, Impact, Confidence, Effort) to prioritize the roadmap."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings: ["gap-project-planning-rice-scoring-001", "gap-project-planning-product-roadmap-001"]

---

## B36-0B-044-005 — "impact mapping" + "document" DOCUMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Document the outcomes of the impact mapping workshop."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.4"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-impact-mapping-001"]
