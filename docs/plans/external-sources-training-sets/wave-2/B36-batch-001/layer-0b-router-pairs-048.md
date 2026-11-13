# Layer 0-B — Router Pairs (048)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-048-001 — "MoSCoW" + "now-next-later" + "roadmap" knowledge_edge routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Use MoSCoW (Must/Should/Could/Won't) and now-next-later to organize the roadmap."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings:
    - "gap-project-planning-moscow-001"
    - "gap-project-planning-now-next-later-001"
    - "gap-project-planning-product-roadmap-001"

---

## B36-0B-048-002 — "hypothesis" + "experiment" + "test" TEST routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Formulate the hypothesis, run the experiment, and test the output."
  CompiledPredicateIds: ["TEST"]

B:
  PredictedPhaseId: "5"
  PredictedSubPhaseId: "5.1"
  EvidencePredicates: ["TEST"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-hypothesis-001", "gap-project-planning-experiment-001"]

---

## B36-0B-048-003 — "RICE" + "OKRs" + "KPIs" knowledge_edge routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Align RICE scoring (Reach, Impact, Confidence, Effort) with OKRs and KPIs for Q4."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings:
    - "gap-project-planning-rice-scoring-001"
    - "gap-project-planning-okr-001"
    - "gap-project-planning-kpi-001"

---

## B36-0B-048-004 — "story splitting" + "define" DOCUMENT routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Define the split stories for the checkout epic."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.4"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-story-splitting-001", "gap-project-planning-epic-001"]

---

## B36-0B-048-005 — Complete Round 3 routing reference (all gap terms)

A: [Reference: complete routing for all Round 3 (037–060) gap vocabulary]

B:
  CompleteZeroPredicateRouting_AllRounds:
    Round1 (sprint/backlog/velocity/capacity/planning-poker/story-points) → KNOWLEDGE_EDGE
    Round2 (definition-of-done, acceptance-criteria, user-story-mapping, epics, sprint-goal,
             retrospective-action-items, kanban-board, WIP-limits, flow-efficiency, cycle-time,
             burndown) → KNOWLEDGE_EDGE
    Round3 Ceremonies (scrum ceremony, daily standup, sprint demo, backlog refinement,
             product increment, MVP, launch readiness, go-live checklist, feature flag) → KNOWLEDGE_EDGE
    Round3 Metrics (OKRs, KPIs, north-star metric, impact mapping, t-shirt sizing, RICE scoring,
             MoSCoW, now-next-later, hypothesis, experiment) → KNOWLEDGE_EDGE

  Rule: All gap vocabulary routes to KNOWLEDGE_EDGE when no predicate fires.
    DomainId: "project-management" in all KNOWLEDGE_EDGE cases.
