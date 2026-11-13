# Layer 0-C — Knowledge-Edge Pairs (019)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B36-0C-019-001 — "story" false positive: knowledge-edge suppressed by DOCUMENT

A:
  DocumentId: "B36-0C-019-001-prompt"
  CompiledText: "Estimate the story points for each backlog item before the sprint starts."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  FlowId: "documentation-requirements-flow-2.1"
  OverallVerdict: "ROUTED"
  KnowledgeEdge: false
  CapabilityGapWarnings:
    - "gap-project-planning-story-point-001: DOCUMENT fires on 'story' substring; effort estimation intent not captured"
    - "gap-project-planning-backlog-001"
    - "gap-project-planning-sprint-001"
  SCHEDULE_CAPABILITY_LEARNING: NOT emitted (DOCUMENT fired)
  Teaching: DOCUMENT fires on "story" → knowledge-edge is suppressed. The model must NOT
    emit KNOWLEDGE_EDGE when a predicate fires. Gap warnings are emitted as metadata only.

---

## B36-0C-019-002 — "user story" correct DOCUMENT routing

A:
  DocumentId: "B36-0C-019-002-prompt"
  CompiledText: "Write up the user story for the checkout flow and assign story points."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  FlowId: "documentation-requirements-flow-2.1"
  OverallVerdict: "ROUTED"
  KnowledgeEdge: false
  CapabilityGapWarnings:
    - "gap-project-planning-story-point-001: story-point assignment intent partially captured via DOCUMENT"
  Teaching: When "story" appears in "user story" (authoring context), DOCUMENT routing is
    both lexicon-correct AND intent-correct. The story-point assignment part is a gap warning.

---

## B36-0C-019-003 — "story" + retrospective: QA fires, knowledge-edge suppressed

A:
  DocumentId: "B36-0C-019-003-prompt"
  CompiledText: "In the retrospective, review the story points assigned last sprint and update velocity."
  CompiledPredicateIds: ["QA", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  FlowId: "multi-phase: 6.1 (QA) + 2.1 (DOCUMENT)"
  OverallVerdict: "ROUTED"
  KnowledgeEdge: false
  CapabilityGapWarnings:
    - "gap-project-planning-retrospective-001: 'retrospective' is gap, NOT QA"
    - "gap-project-planning-sprint-001"
    - "gap-project-planning-velocity-001"
    - "gap-project-planning-story-point-001"
  Teaching: "retrospective" does NOT fire QA. "review" fires QA. Both are present; only
    "review" drives the QA routing. "retrospective" is emitted as a gap warning.

---

## B36-0C-019-004 — Boundary: "story" false positive forces routing away from knowledge-edge

A:
  DocumentId: "B36-0C-019-004-prompt"
  CompiledText: "Assign story points to all backlog stories before the sprint planning meeting."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  FlowId: "documentation-requirements-flow-2.1"
  OverallVerdict: "ROUTED"
  KnowledgeEdge: false
  CapabilityGapWarnings:
    - "gap-project-planning-story-point-001"
    - "gap-project-planning-backlog-001"
    - "gap-project-planning-sprint-001"
  Teaching: The false positive on "story" forces routing away from knowledge-edge.
    Even though the developer intends effort estimation, the compiler is literal.
    The model must NOT suppress DOCUMENT to preserve knowledge-edge for this intent.
