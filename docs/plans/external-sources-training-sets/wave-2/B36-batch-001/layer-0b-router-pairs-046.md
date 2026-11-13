# Layer 0-B — Router Pairs (046)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-046-001 — "RICE scoring" + "review" QA routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Review the RICE scoring (Reach, Impact, Confidence, Effort) results with the product team."
  CompiledPredicateIds: ["QA"]

B:
  PredictedPhaseId: "6"
  PredictedSubPhaseId: "6.1"
  EvidencePredicates: ["QA"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-rice-scoring-001"]

---

## B36-0B-046-002 — "now-next-later" + "build" IMPLEMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Build the now-next-later roadmap view in the project dashboard."
  CompiledPredicateIds: ["IMPLEMENT"]

B:
  PredictedPhaseId: "4"
  PredictedSubPhaseId: "4.1"
  EvidencePredicates: ["IMPLEMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-now-next-later-001", "gap-project-planning-product-roadmap-001"]

---

## B36-0B-046-003 — "t-shirt sizing" + "story points" DOCUMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Convert t-shirt sizing estimates to story points for the backlog items."
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

## B36-0B-046-004 — "hypothesis" + "test" TEST routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Test the hypothesis about user engagement with the new onboarding flow."
  CompiledPredicateIds: ["TEST"]

B:
  PredictedPhaseId: "5"
  PredictedSubPhaseId: "5.1"
  EvidencePredicates: ["TEST"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-hypothesis-001"]

---

## B36-0B-046-005 — "experiment" + "test" TEST routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Test the experiment results to see if the feature flag improved conversion."
  CompiledPredicateIds: ["TEST"]

B:
  PredictedPhaseId: "5"
  PredictedSubPhaseId: "5.1"
  EvidencePredicates: ["TEST"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-experiment-001", "gap-project-planning-feature-flag-001"]
