# Layer 0-B — Router Pairs (047)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-047-001 — "north-star metric" + "define" DOCUMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Define the north-star metric for the growth team this quarter."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.4"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-north-star-metric-001"]

---

## B36-0B-047-002 — "story splitting" + "implement" DOCUMENT+IMPLEMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "After story splitting, implement each sub-story as a separate service endpoint."
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]

B:
  RoutingDecisions:
    - PredictedPhaseId: "2" | PredictedSubPhaseId: "2.4" | EvidencePredicates: ["DOCUMENT"]
    - PredictedPhaseId: "4" | PredictedSubPhaseId: "4.1" | EvidencePredicates: ["IMPLEMENT"]
  ExecutionOrder: "2.4 and 4.1 run in parallel"
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-story-splitting-001"]

---

## B36-0B-047-003 — "impact mapping" + "design" ARCHITECTURE routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Design the architecture to support the outcomes from impact mapping."
  CompiledPredicateIds: ["ARCHITECTURE"]

B:
  PredictedPhaseId: "3"
  PredictedSubPhaseId: "3.4"
  EvidencePredicates: ["ARCHITECTURE"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-impact-mapping-001"]

---

## B36-0B-047-004 — "OKRs" + "review" QA routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Review quarterly OKRs (Objectives and Key Results) progress with the leadership team."
  CompiledPredicateIds: ["QA"]

B:
  PredictedPhaseId: "6"
  PredictedSubPhaseId: "6.1"
  EvidencePredicates: ["QA"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-okr-001"]

---

## B36-0B-047-005 — Round 3 complete routing table (metrics block)

A: [Reference: complete routing for Round 3 metrics and prioritization vocabulary]

B:
  ZeroPredicateRouting_Round3_Metrics:
    - "OKRs" (alone) → KNOWLEDGE_EDGE, DomainId: project-management
    - "KPIs" (alone) → KNOWLEDGE_EDGE, DomainId: project-management
    - "north-star metric" (alone) → KNOWLEDGE_EDGE, DomainId: project-management
    - "impact mapping" (alone) → KNOWLEDGE_EDGE, DomainId: project-management
    - "t-shirt sizing" (alone) → KNOWLEDGE_EDGE, DomainId: project-management
    - "RICE scoring" (alone) → KNOWLEDGE_EDGE, DomainId: project-management
    - "MoSCoW" (alone) → KNOWLEDGE_EDGE, DomainId: project-management
    - "now-next-later" (alone) → KNOWLEDGE_EDGE, DomainId: project-management
    - "hypothesis" (alone) → KNOWLEDGE_EDGE, DomainId: project-management
    - "experiment" (alone) → KNOWLEDGE_EDGE, DomainId: project-management
  Predicate-fired:
    - any + "review" → QA → Phase 6.1
    - "story splitting" / "story points" → DOCUMENT → Phase 2.4
    - any + "implement"/"build" + code entity → IMPLEMENT → Phase 4.1
    - any + "test" → TEST → Phase 5.1
    - any + "design"/"architect" → ARCHITECTURE → Phase 3.4
