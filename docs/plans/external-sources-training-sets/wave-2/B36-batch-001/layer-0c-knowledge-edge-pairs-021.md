# Layer 0-C — Knowledge-Edge Pairs (021)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B36-0C-021-001 — Sprint + QA: not knowledge-edge

A:
  DocumentId: "B36-0C-021-001-prompt"
  CompiledText: "Review the sprint deliverables and approve the features before releasing to production."
  CompiledPredicateIds: ["QA", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  Flows: ["qa-review-flow-6.1", "backend-service-flow-4.1"]
  OverallVerdict: "ROUTED"
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings:
    - "gap-project-planning-sprint-001"
  Teaching: QA fires on "review" and "approve". IMPLEMENT fires on "features".
    Sprint is gap. Two routing decisions produced. Not knowledge-edge.

---

## B36-0C-021-002 — Velocity + QA: not knowledge-edge, gap warnings

A:
  DocumentId: "B36-0C-021-002-prompt"
  CompiledText: "Review the team velocity report from the last sprint and identify any testing gaps."
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  FlowId: "qa-review-flow-6.1"
  OverallVerdict: "ROUTED"
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings:
    - "gap-project-planning-velocity-001"
    - "gap-project-planning-sprint-001"

---

## B36-0C-021-003 — Milestone + QA + DOCUMENT: not knowledge-edge, two flows

A:
  DocumentId: "B36-0C-021-003-prompt"
  CompiledText: "Review the milestone completion criteria and approve the Q3 deliverables for sign-off."
  CompiledPredicateIds: ["QA", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  Flows: ["qa-review-flow-6.1", "documentation-requirements-flow-2.4"]
  OverallVerdict: "ROUTED"
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings:
    - "gap-project-planning-milestone-001"

---

## B36-0C-021-004 — Backlog + QA + DOCUMENT: not knowledge-edge

A:
  DocumentId: "B36-0C-021-004-prompt"
  CompiledText: "Review the prioritized backlog with the stakeholders and get approval on the sprint scope."
  CompiledPredicateIds: ["QA", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  Flows: ["qa-review-flow-6.1", "documentation-requirements-flow-2.4"]
  OverallVerdict: "ROUTED"
  KnowledgeEdge: false
  DomainId: "software-development"
  CapabilityGapWarnings:
    - "gap-project-planning-backlog-001"
    - "gap-project-planning-sprint-001"
  Teaching: Two predicates fire (QA via "review"/"approval", DOCUMENT via "stakeholders").
    Both routings proceed. Gap vocabulary (backlog, sprint) produces gap warnings but does
    NOT trigger knowledge-edge. DomainId: "software-development" because predicates fired.
