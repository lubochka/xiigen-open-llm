# Layer 0-A — Compiler Pairs (021)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-021-001 — Sprint planning + QA: "review the sprint deliverables"

A:
  run_id: "B36-0A-021-001"
  user_request: "Review the sprint deliverables and approve the features before releasing to production."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "review" — fires QA
  "approve" — fires QA
  "sprint" — not in any predicate lexicon (gap)
  "features" — fires IMPLEMENT; combined with "review" → both QA and IMPLEMENT fire
  Correction: CompiledPredicateIds: ["QA", "IMPLEMENT"] because "features" fires IMPLEMENT
  CapabilityGapWarnings: ["gap-project-planning-sprint-001"]

Corrected B:
  CompiledPredicateIds: ["QA", "IMPLEMENT"]
  CapabilityGapWarnings: ["gap-project-planning-sprint-001"]

---

## B36-0A-021-002 — Velocity + QA: "review the velocity report"

A:
  run_id: "B36-0A-021-002"
  user_request: "Review the team velocity report from the last sprint and identify any testing gaps."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "review" — fires QA
  "testing" — fires QA or TEST; "testing gaps" context → QA
  "velocity" — not in any predicate lexicon (gap)
  "sprint" — not in any predicate lexicon (gap)
  Mixed: QA fires; velocity and sprint are gap warnings
  CapabilityGapWarnings: ["gap-project-planning-velocity-001", "gap-project-planning-sprint-001"]

---

## B36-0A-021-003 — Milestone + QA: "review the milestone completion"

A:
  run_id: "B36-0A-021-003"
  user_request: "Review the milestone completion criteria and approve the Q3 deliverables for sign-off."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["QA", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "review" — fires QA
  "approve" — fires QA
  "criteria" — fires DOCUMENT
  "milestone" — not in any predicate lexicon (gap)
  Mixed: QA fires on "review" and "approve"; DOCUMENT fires on "criteria"; milestone is gap warning
  CapabilityGapWarnings: ["gap-project-planning-milestone-001"]

---

## B36-0A-021-004 — Roadmap + QA: "review the roadmap items"

A:
  run_id: "B36-0A-021-004"
  user_request: "Review the product roadmap items and test whether the current sprint covers the Q3 goals."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["QA", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "review" — fires QA
  "test" — fires TEST (literal match; fires even when "test whether" is hypothetical)
  "roadmap" — not in any predicate lexicon (gap)
  "sprint" — not in any predicate lexicon (gap)
  Mixed: QA and TEST fire; roadmap and sprint are gap warnings
  CapabilityGapWarnings: ["gap-project-planning-roadmap-001", "gap-project-planning-sprint-001"]

---

## B36-0A-021-005 — Backlog + QA only: pure review without implementation

A:
  run_id: "B36-0A-021-005"
  user_request: "Review the prioritized backlog with the stakeholders and get approval on the sprint scope."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["QA", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "review" — fires QA
  "approval" — fires QA (approve ∈ QA lexicon)
  "stakeholders" — fires DOCUMENT
  "backlog" — not in any predicate lexicon (gap)
  "sprint" — not in any predicate lexicon (gap)
  Mixed: QA fires on "review" and "approval"; DOCUMENT fires on "stakeholders"; planning vocabulary is gap
  CapabilityGapWarnings: ["gap-project-planning-backlog-001", "gap-project-planning-sprint-001"]
