# Layer 0-C — Knowledge-Edge Pairs (024)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B36-0C-024-001 — SCHEDULE_CAPABILITY_LEARNING: one signal per request rule

A:
  Task: "Demonstrate that SCHEDULE_CAPABILITY_LEARNING is emitted once per request, regardless of how many gap IDs are detected."
  CompiledText: "Plan the sprint, groom the backlog, track velocity, and set milestone dates."
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  ScheduleCapabilityLearningSent: 1
  CapabilityGapIds:
    - "gap-project-planning-sprint-001"
    - "gap-project-planning-backlog-001"
    - "gap-project-planning-velocity-001"
    - "gap-project-planning-milestone-001"
  Signal:
    signal: "SCHEDULE_CAPABILITY_LEARNING"
    capabilityGapIds: ["gap-project-planning-sprint-001", "gap-project-planning-backlog-001", "gap-project-planning-velocity-001", "gap-project-planning-milestone-001"]
  Rule: All gap IDs are bundled into ONE SCHEDULE_CAPABILITY_LEARNING signal. Four gap IDs
    → one signal (not four signals). This is the canonical rule (Rule 22).

---

## B36-0C-024-002 — SCHEDULE_CAPABILITY_LEARNING NOT emitted when predicate fires

A:
  Task: "Confirm that SCHEDULE_CAPABILITY_LEARNING is NOT emitted when any predicate fires, even with gap vocabulary present."
  CompiledText: "Document the sprint velocity metrics."
  CompiledPredicateIds: ["DOCUMENT"]

B:
  OverallVerdict: "ROUTED"
  KnowledgeEdge: false
  ScheduleCapabilityLearningSent: 0
  CapabilityGapWarnings:
    - "gap-project-planning-velocity-001"
    - "gap-project-planning-sprint-001"
  Rule: SCHEDULE_CAPABILITY_LEARNING is NOT emitted when predicates fire.
    Gap warnings are still logged as metadata.

---

## B36-0C-024-003 — Complete gap ID reference for 015 new vocabulary

A:
  Task: "List all new gap IDs added in the B36-batch-001 013-024 extension."

B:
  NewGapIds:
    - "gap-project-planning-estimation-001" — covers: effort-estimate, planning-poker estimation, story-point estimation intent
    - "gap-project-planning-release-cycle-001" — covers: release plan, release planning, release cycle
    - "gap-project-planning-story-point-001" — covers: story-point compound; note DOCUMENT fires on 'story' substring
  ExtendedCompoundIds:
    - "gap-project-planning-backlog-estimation" — covers: backlog + estimation co-occurrence
    - "gap-project-planning-sprint-not-captured" — covers: sprint framing around IMPLEMENT task

---

## B36-0C-024-004 — DomainId transition: gap to predicate

A:
  Scenario: "The same request is edited: first version has no predicate; second version adds 'review'."

B:
  Version1: "Plan the sprint and align the roadmap milestones."
    CompiledPredicateIds: []
    DomainId: "project-management"
    OverallVerdict: "KNOWLEDGE_EDGE"

  Version2: "Plan the sprint and review the roadmap milestones."
    CompiledPredicateIds: ["QA"] (review fires QA)
    DomainId: "software-development"
    OverallVerdict: "ROUTED"

  Teaching: One word change ("align" → "review") switches DomainId and OverallVerdict.
    The model must learn this DomainId boundary precisely. "Align" does not fire any
    predicate. "Review" fires QA. The DomainId follows the predicate, not the intent.
