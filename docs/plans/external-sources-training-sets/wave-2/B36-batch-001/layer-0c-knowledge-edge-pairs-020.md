# Layer 0-C — Knowledge-Edge Pairs (020)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B36-0C-020-001 — Sprint + IMPLEMENT: not knowledge-edge

A:
  DocumentId: "B36-0C-020-001-prompt"
  CompiledText: "Plan the sprint and build the authentication features scheduled for this iteration."
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  FlowId: "backend-service-flow-4.1"
  OverallVerdict: "ROUTED"
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings:
    - "gap-project-planning-sprint-001"
    - "gap-project-planning-iteration-001"
  SCHEDULE_CAPABILITY_LEARNING: NOT emitted

---

## B36-0C-020-002 — Roadmap + IMPLEMENT: not knowledge-edge

A:
  DocumentId: "B36-0C-020-002-prompt"
  CompiledText: "Implement the first quarter roadmap items: build the API service and create the database module."
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  FlowId: "backend-service-flow-4.1"
  OverallVerdict: "ROUTED"
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings:
    - "gap-project-planning-roadmap-001"

---

## B36-0C-020-003 — Epic + IMPLEMENT: not knowledge-edge

A:
  DocumentId: "B36-0C-020-003-prompt"
  CompiledText: "In this sprint, implement the user authentication epic: build the login module and create the token service."
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  FlowId: "backend-service-flow-4.1"
  OverallVerdict: "ROUTED"
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings:
    - "gap-project-planning-epic-001"
    - "gap-project-planning-sprint-001"

---

## B36-0C-020-004 — Sprint + IMPLEMENT + DOCUMENT: not knowledge-edge, two flows

A:
  DocumentId: "B36-0C-020-004-prompt"
  CompiledText: "For this sprint: build the notification service, implement the email feature, and document the API requirements."
  CompiledPredicateIds: ["IMPLEMENT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  Flows: ["backend-service-flow-4.1", "documentation-requirements-flow-2.4"]
  OverallVerdict: "ROUTED"
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings:
    - "gap-project-planning-sprint-001"
  Teaching: Two predicates fire. Two flows execute in parallel. Sprint is gap warning.
    DomainId is "software-development" because predicates fired.
    SCHEDULE_CAPABILITY_LEARNING is NOT emitted.
