# Layer 0-B — Router Pairs (050)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-050-001 — "t-shirt sizing" + "review" QA routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Review the t-shirt sizing estimates with the engineering team."
  CompiledPredicateIds: ["QA"]

B:
  PredictedPhaseId: "6"
  PredictedSubPhaseId: "6.1"
  EvidencePredicates: ["QA"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-t-shirt-sizing-001"]

---

## B36-0B-050-002 — "hypothesis" + "define" DOCUMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Define the success criteria for the hypothesis before running the experiment."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.4"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-hypothesis-001", "gap-project-planning-experiment-001"]

---

## B36-0B-050-003 — "north-star metric" + "implement" IMPLEMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Implement the north-star metric tracking in the analytics service."
  CompiledPredicateIds: ["IMPLEMENT"]

B:
  PredictedPhaseId: "4"
  PredictedSubPhaseId: "4.1"
  EvidencePredicates: ["IMPLEMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-north-star-metric-001"]

---

## B36-0B-050-004 — "impact mapping" + "design" ARCHITECTURE routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Design the system architecture to support the outcomes from the impact mapping."
  CompiledPredicateIds: ["ARCHITECTURE"]

B:
  PredictedPhaseId: "3"
  PredictedSubPhaseId: "3.4"
  EvidencePredicates: ["ARCHITECTURE"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-impact-mapping-001"]

---

## B36-0B-050-005 — v6-01 guard routing: "PLANNING_STRATEGY" invented predicate

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Plan the OKR strategy for next quarter."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings: ["gap-project-planning-okr-001"]
  Note: "v6-01 guard: no predicate fires. 'plan' is neutral. PLANNING_STRATEGY does not exist."
