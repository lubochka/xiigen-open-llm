# Layer 0-B — Router Pairs (027)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## B36-0B-027-001 — "kanban board" + "design" routes to Phase 3.4 (ARCHITECTURE)

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Design the kanban board structure for the engineering team's workflow."
  CompiledPredicateIds: ["ARCHITECTURE"]

B:
  PredictedPhaseId: "3"
  PredictedSubPhaseId: "3.4"
  EvidencePredicates: ["ARCHITECTURE"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-kanban-board-001"]

---

## B36-0B-027-002 — "user story mapping" routes to Phase 2.1 (DOCUMENT via "story")

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Facilitate a user story mapping session for the upcoming epic."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.1"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-user-story-mapping-001", "gap-project-planning-epic-001"]
  Note: "story" fires DOCUMENT → Phase 2.1 (story/narrative documentation sub-phase)

---

## B36-0B-027-003 — "cycle time" + "implement" routes to Phase 4.1 (IMPLEMENT)

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Track cycle time and build a feature to surface it in the dashboard."
  CompiledPredicateIds: ["IMPLEMENT"]

B:
  PredictedPhaseId: "4"
  PredictedSubPhaseId: "4.1"
  EvidencePredicates: ["IMPLEMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-cycle-time-001"]

---

## B36-0B-027-004 — "WIP limits" + "criteria" routes to Phase 2.4 (DOCUMENT)

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Define WIP limits and specify the criteria for moving tickets between stages."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  PredictedPhaseId: "2"
  PredictedSubPhaseId: "2.4"
  EvidencePredicates: ["DOCUMENT"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-wip-limits-001", "gap-project-planning-ticket-001"]
  Note: "criteria" triggers sub-phase 2.4 (requirements/acceptance criteria documentation)

---

## B36-0B-027-005 — "flow efficiency" + "design" routes to Phase 3.4 (ARCHITECTURE)

A: [SubPhaseRouterSourceDocument]
  CompiledText: "Design a system to automatically calculate flow efficiency from kanban data."
  CompiledPredicateIds: ["ARCHITECTURE"]

B:
  PredictedPhaseId: "3"
  PredictedSubPhaseId: "3.4"
  EvidencePredicates: ["ARCHITECTURE"]
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings: ["gap-project-planning-flow-efficiency-001", "gap-project-planning-kanban-board-001"]
