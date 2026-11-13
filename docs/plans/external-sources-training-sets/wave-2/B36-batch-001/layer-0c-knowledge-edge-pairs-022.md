# Layer 0-C — Knowledge-Edge Pairs (022)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B36-0C-022-001 — Roadmap + DOCUMENT: not knowledge-edge

A:
  DocumentId: "B36-0C-022-001-prompt"
  CompiledText: "Document the roadmap items and specify the requirements for each quarterly milestone."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  FlowId: "documentation-requirements-flow-2.4"
  OverallVerdict: "ROUTED"
  KnowledgeEdge: false
  CapabilityGapWarnings:
    - "gap-project-planning-roadmap-001"
    - "gap-project-planning-milestone-001"

---

## B36-0C-022-002 — Backlog + DOCUMENT (functional+spec): not knowledge-edge

A:
  DocumentId: "B36-0C-022-002-prompt"
  CompiledText: "Define the backlog requirements and document the functional specifications for each epic."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  FlowId: "documentation-requirements-flow-2.1"
  OverallVerdict: "ROUTED"
  KnowledgeEdge: false
  CapabilityGapWarnings:
    - "gap-project-planning-backlog-001"
    - "gap-project-planning-epic-001"

---

## B36-0C-022-003 — Velocity + DOCUMENT: not knowledge-edge

A:
  DocumentId: "B36-0C-022-003-prompt"
  CompiledText: "Document the team velocity metrics and specify how they relate to the sprint capacity."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  FlowId: "documentation-requirements-flow-2.1"
  OverallVerdict: "ROUTED"
  KnowledgeEdge: false
  CapabilityGapWarnings:
    - "gap-project-planning-velocity-001"
    - "gap-project-planning-sprint-001"
    - "gap-project-planning-capacity-001"
  Teaching: DOCUMENT fires on "document" and "specify". Velocity, sprint, and capacity
    are gap warnings. No knowledge-edge — DOCUMENT predicate prevents it.
    SCHEDULE_CAPABILITY_LEARNING: NOT emitted.

---

## B36-0C-022-004 — Milestone + DOCUMENT (use-case): not knowledge-edge

A:
  DocumentId: "B36-0C-022-004-prompt"
  CompiledText: "Describe the deliverables for each roadmap milestone and define the use-case for each major epic."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  FlowId: "documentation-requirements-flow-2.1"
  OverallVerdict: "ROUTED"
  KnowledgeEdge: false
  CapabilityGapWarnings:
    - "gap-project-planning-milestone-001"
    - "gap-project-planning-roadmap-001"
    - "gap-project-planning-epic-001"
  Teaching: DOCUMENT fires on "describe", "define", "use-case". Planning vocabulary
    (milestone, roadmap, epic) produces gap warnings but does not prevent routing.
    The model must NOT suppress DOCUMENT to preserve knowledge-edge for this request.
