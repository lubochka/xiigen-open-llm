# Layer 0-B — Router Pairs (054)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-054-001 — "now-next-later" + "requirements" DOCUMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Document the requirements for now-next-later items on the roadmap."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.4"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-now-next-later-001", "gap-project-planning-product-roadmap-001"]

---

## B36-0B-054-002 — "MoSCoW" + "user stories" DOCUMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Use MoSCoW (Must/Should/Could/Won't) to prioritize user stories in the backlog."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.4"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-moscow-001", "gap-project-planning-backlog-001"]

---

## B36-0B-054-003 — "north-star" + "KPIs" + "build" IMPLEMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Build a report that visualizes north-star metric alongside KPIs."
  CompiledPredicateIds: ["IMPLEMENT"]

B:
  PredictedPhaseId: "4"
  PredictedSubPhaseId: "4.1"
  EvidencePredicates: ["IMPLEMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-north-star-metric-001", "gap-project-planning-kpi-001"]

---

## B36-0B-054-004 — "hypothesis" + "review" QA routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Review the experiment hypothesis with the data team before launch."
  CompiledPredicateIds: ["QA"]

B:
  PredictedPhaseId: "6"
  PredictedSubPhaseId: "6.1"
  EvidencePredicates: ["QA"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-hypothesis-001", "gap-project-planning-experiment-001"]

---

## B36-0B-054-005 — "t-shirt sizing" + "RICE" + "MoSCoW" + "stories" DOCUMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Apply t-shirt sizing, RICE scoring (Reach, Impact, Confidence, Effort), and MoSCoW (Must/Should/Could/Won't) to user stories."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.4"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings:
    - "gap-project-planning-t-shirt-sizing-001"
    - "gap-project-planning-rice-scoring-001"
    - "gap-project-planning-moscow-001"
