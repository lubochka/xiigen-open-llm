# Layer 0-B — Router Pairs (060)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-060-001 — Complete Round 3 routing reference (all 60 files)

A: [Reference: complete routing reference for all B36 gap vocabulary across all rounds]

B:
  CompleteRoutingReference:
    Zero predicates → KNOWLEDGE_EDGE, null sub_phase_id, DomainId: project-management
    DOCUMENT only → Phase 2, sub_phase: 2.4, DomainId: software-development
    IMPLEMENT only → Phase 4, sub_phase: 4.1, DomainId: software-development
    QA only → Phase 6, sub_phase: 6.1, DomainId: software-development
    TEST only → Phase 5, sub_phase: 5.1, DomainId: software-development
    ARCHITECTURE only → Phase 3, sub_phase: 3.4, DomainId: software-development
    QA + IMPLEMENT → 4.1 first; 6.1 last (QA-last rule)
    QA + DOCUMENT → 2.4 first; 6.1 last (QA-last rule)
    QA + DOCUMENT + IMPLEMENT → 4.1 and 2.4 parallel; 6.1 last
    TEST + QA → 5.1 first; 6.1 last
    DOCUMENT + IMPLEMENT → 2.4 and 4.1 parallel

---

## B36-0B-060-002 — "five predicates" combo routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Design the architecture, implement the feature flag, test the hypothesis, document OKRs, review the sprint demo, and build the KPI dashboard."
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT", "TEST", "DOCUMENT", "QA"]

B:
  RoutingDecisions:
    - PredictedPhaseId: "3" | PredictedSubPhaseId: "3.4" | EvidencePredicates: ["ARCHITECTURE"]
    - PredictedPhaseId: "4" | PredictedSubPhaseId: "4.1" | EvidencePredicates: ["IMPLEMENT"]
    - PredictedPhaseId: "5" | PredictedSubPhaseId: "5.1" | EvidencePredicates: ["TEST"]
    - PredictedPhaseId: "2" | PredictedSubPhaseId: "2.4" | EvidencePredicates: ["DOCUMENT"]
    - PredictedPhaseId: "6" | PredictedSubPhaseId: "6.1" | EvidencePredicates: ["QA"]
  ExecutionOrder: "3.4, 4.1, 5.1, 2.4 run first (parallel); 6.1 runs last (QA-last)"
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings:
    - "gap-project-planning-feature-flag-001"
    - "gap-project-planning-hypothesis-001"
    - "gap-project-planning-okr-001"
    - "gap-project-planning-sprint-demo-001"
    - "gap-project-planning-kpi-001"

---

## B36-0B-060-003 — QA-last final canonical statement

A: [Reference: QA-last rule canonical statement for all B36 files]

B:
  QALastCanonical:
    Rule: "When CompiledPredicateIds contains 'QA' AND at least one other predicate,
      the QA flow (sub_phase 6.1) executes AFTER all other flows complete."
    Exception: "QA alone → no ordering constraint; 6.1 executes normally."
    All other predicates (IMPLEMENT, DOCUMENT, TEST, ARCHITECTURE): run first or in parallel.
    Applies: across all 60 B36 layer-0b files.

---

## B36-0B-060-004 — KNOWLEDGE_EDGE routing: only one canonical path

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Cascade OKRs to KPIs, track against north-star, run impact mapping, and use t-shirt sizing."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings:
    - "gap-project-planning-okr-001"
    - "gap-project-planning-kpi-001"
    - "gap-project-planning-north-star-metric-001"
    - "gap-project-planning-impact-mapping-001"
    - "gap-project-planning-t-shirt-sizing-001"

---

## B36-0B-060-005 — SCHEDULE_CAPABILITY_LEARNING routing note

A: [Reference: SCHEDULE_CAPABILITY_LEARNING and routing]

B:
  RoutingNote:
    "SCHEDULE_CAPABILITY_LEARNING is emitted by the orchestrator layer, not the router layer.
    The router receives CompiledPredicateIds: [] and routes to KNOWLEDGE_EDGE.
    The router does NOT emit SCHEDULE_CAPABILITY_LEARNING signals.
    The orchestrator (Layer 1) emits the signal after observing KNOWLEDGE_EDGE routing."
  Rule: "Router output + KNOWLEDGE_EDGE → orchestrator emits signal. Router emits nothing."
