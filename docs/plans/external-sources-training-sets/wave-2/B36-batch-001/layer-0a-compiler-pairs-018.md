# Layer 0-A — Compiler Pairs (018)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-018-001 — Prioritize pure gap: no predicate

A:
  run_id: "B36-0A-018-001"
  user_request: "Prioritize the backlog items and triage the incoming tickets for the upcoming sprint."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

Predicate evidence:
  "prioritize" — not in any predicate lexicon
  "backlog" — not in any predicate lexicon (gap)
  "triage" — not in any predicate lexicon (gap vocabulary)
  "tickets" — not in any predicate lexicon (gap)
  "sprint" — not in any predicate lexicon (gap)
  Result: CompiledPredicateIds: [], knowledge_edge
  CapabilityGapWarnings: ["gap-project-planning-backlog-001", "gap-project-planning-ticket-001", "gap-project-planning-sprint-001"]

---

## B36-0A-018-002 — Triage with DOCUMENT co-signal: "define priority criteria"

A:
  run_id: "B36-0A-018-002"
  user_request: "Triage the incoming backlog and define the priority criteria for each ticket type."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" — fires DOCUMENT
  "criteria" — fires DOCUMENT
  "triage" — not in any predicate lexicon (gap vocabulary)
  "backlog" — not in any predicate lexicon (gap)
  "ticket" — not in any predicate lexicon (gap)
  Mixed: DOCUMENT fires on "define" and "criteria"; triage/backlog/ticket are gap warnings
  CapabilityGapWarnings: ["gap-project-planning-backlog-001", "gap-project-planning-ticket-001"]

---

## B36-0A-018-003 — Prioritize with "review" (QA fires)

A:
  run_id: "B36-0A-018-003"
  user_request: "Review the prioritized backlog and approve the sprint scope for the team."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "review" — fires QA (literal; fires even in prioritization context)
  "approve" — fires QA
  "prioritized backlog" — not in any predicate lexicon (gap)
  "sprint" — not in any predicate lexicon (gap)
  Mixed: QA fires on "review" and "approve"; backlog and sprint are gap warnings
  CapabilityGapWarnings: ["gap-project-planning-backlog-001", "gap-project-planning-sprint-001"]

---

## B36-0A-018-004 — Triage: purely gap with five gap terms

A:
  run_id: "B36-0A-018-004"
  user_request: "Triage the epic backlog, prioritize the sprint tickets, and set velocity targets for the next iteration."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

Predicate evidence:
  "triage" — not in any predicate lexicon (gap vocabulary)
  "epic" — not in any predicate lexicon (gap)
  "backlog" — not in any predicate lexicon (gap)
  "sprint" — not in any predicate lexicon (gap)
  "tickets" — not in any predicate lexicon (gap)
  "velocity" — not in any predicate lexicon (gap)
  "iteration" — not in any predicate lexicon (gap)
  Result: CompiledPredicateIds: [], knowledge_edge
  CapabilityGapWarnings: ["gap-project-planning-epic-001", "gap-project-planning-backlog-001", "gap-project-planning-sprint-001", "gap-project-planning-ticket-001", "gap-project-planning-velocity-001", "gap-project-planning-iteration-001"]

---

## B36-0A-018-005 — Prioritize with IMPLEMENT: "build the top-priority features"

A:
  run_id: "B36-0A-018-005"
  user_request: "Prioritize the backlog and build the top three features in this sprint."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "build" — fires IMPLEMENT
  "features" — fires IMPLEMENT
  "prioritize" — not in any predicate lexicon
  "backlog" — not in any predicate lexicon (gap)
  "sprint" — not in any predicate lexicon (gap)
  Mixed: IMPLEMENT dominates; prioritization and backlog are gap warnings
  CapabilityGapWarnings: ["gap-project-planning-backlog-001", "gap-project-planning-sprint-001"]
