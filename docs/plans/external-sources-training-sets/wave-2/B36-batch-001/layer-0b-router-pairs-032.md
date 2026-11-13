# Layer 0-B — Router Pairs (032)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-032-001 — "sprint goal" + "definition of done" + "acceptance criteria" routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Set the sprint goal and ensure alignment between the definition of done and acceptance criteria."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.4"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-sprint-goal-001", "gap-project-planning-sprint-001", "gap-project-planning-definition-of-done-001", "gap-project-planning-acceptance-criteria-001"]

---

## B36-0B-032-002 — "epics breakdown" + "milestone" knowledge_edge routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Plan the epics breakdown and map each epic to a sprint milestone."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings: ["gap-project-planning-epic-001", "gap-project-planning-sprint-001", "gap-project-planning-milestone-001"]

---

## B36-0B-032-003 — "cycle time" + "build" routes to Phase 4.1 (IMPLEMENT)

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Build a cycle time tracker for the engineering dashboard."
  CompiledPredicateIds: ["IMPLEMENT"]

B:
  PredictedPhaseId: "4"
  PredictedSubPhaseId: "4.1"
  EvidencePredicates: ["IMPLEMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-cycle-time-001"]

---

## B36-0B-032-004 — "burndown" + "kanban" + "review" routes to Phase 6.1 (QA)

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Review the burndown chart and adjust WIP limits on the kanban board."
  CompiledPredicateIds: ["QA"]

B:
  PredictedPhaseId: "6"
  PredictedSubPhaseId: "6.1"
  EvidencePredicates: ["QA"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-burndown-001", "gap-project-planning-wip-limits-001", "gap-project-planning-kanban-board-001"]

---

## B36-0B-032-005 — Round 2 FailureClassification summary

A: [Reference: When is FailureClassification 'no_predicate_match_capability_gap' used in Round 2?]

B:
  Rule: "FailureClassification: 'no_predicate_match_capability_gap' when CompiledPredicateIds: []
    AND Round 2 gap vocabulary is present (definition of done, sprint goal, epics, kanban, WIP limits, etc.)"
  Examples:
    - "Align on the definition of done." → no_predicate_match_capability_gap
    - "Break down the epics." → no_predicate_match_capability_gap
    - "Optimize kanban WIP limits." → no_predicate_match_capability_gap
  NOT used when:
    - Any predicate fires (criteria, story, review, define, etc.)
    - Request has zero gap vocabulary (completely unrecognized)
