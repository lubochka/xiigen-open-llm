# Layer 0-B — Router Pairs (026)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-026-001 — "flow efficiency" + "cycle time" knowledge_edge routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Measure flow efficiency and cycle time across the kanban board."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings: ["gap-project-planning-flow-efficiency-001", "gap-project-planning-cycle-time-001", "gap-project-planning-kanban-board-001"]

---

## B36-0B-026-002 — "burndown" + "review" routes to Phase 6.1 (QA)

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Review the burndown chart at end of sprint to assess progress."
  CompiledPredicateIds: ["QA"]

B:
  PredictedPhaseId: "6"
  PredictedSubPhaseId: "6.1"
  EvidencePredicates: ["QA"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-burndown-001", "gap-project-planning-sprint-001"]

---

## B36-0B-026-003 — "epics breakdown" knowledge_edge routing

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Break down the epics into smaller backlog items for the next iteration."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  PredictedSubPhaseId: null
  KnowledgeEdge: true
  DomainId: "project-management"
  CapabilityGapWarnings: ["gap-project-planning-epic-001", "gap-project-planning-backlog-001", "gap-project-planning-iteration-001"]

---

## B36-0B-026-004 — "sprint goal" + "build" routes to Phase 4.1 (IMPLEMENT)

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Set the sprint goal and build the authentication module to meet it."
  CompiledPredicateIds: ["IMPLEMENT"]

B:
  PredictedPhaseId: "4"
  PredictedSubPhaseId: "4.1"
  EvidencePredicates: ["IMPLEMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-sprint-goal-001", "gap-project-planning-sprint-001"]

---

## B36-0B-026-005 — "definition of done" + "review" routes to Phase 6.1 (QA)

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Review the definition of done and update it for the next sprint."
  CompiledPredicateIds: ["QA"]

B:
  PredictedPhaseId: "6"
  PredictedSubPhaseId: "6.1"
  EvidencePredicates: ["QA"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-definition-of-done-001", "gap-project-planning-sprint-001"]
  Note: Gap vocabulary does NOT appear in EvidencePredicates. Only predicate tokens appear there.
