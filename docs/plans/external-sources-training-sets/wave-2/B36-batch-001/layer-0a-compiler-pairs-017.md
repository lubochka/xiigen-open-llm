# Layer 0-A — Compiler Pairs (017)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-017-001 — Grooming pure gap: no predicate

A:
  run_id: "B36-0A-017-001"
  user_request: "Groom the product backlog before the next sprint planning session."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

Predicate evidence:
  "groom" — not in any predicate lexicon
  "backlog" — not in any predicate lexicon (gap)
  "sprint" — not in any predicate lexicon (gap)
  "planning session" — not in any predicate lexicon
  Result: CompiledPredicateIds: [], knowledge_edge
  CapabilityGapWarnings: ["gap-project-planning-backlog-001", "gap-project-planning-sprint-001"]

---

## B36-0A-017-002 — Refinement pure gap: synonymous with grooming

A:
  run_id: "B36-0A-017-002"
  user_request: "Run the backlog refinement meeting and reprioritize the tickets for the upcoming iteration."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

Predicate evidence:
  "backlog refinement" — not in any predicate lexicon (gap)
  "run" — not in any predicate lexicon
  "reprioritize" — not in any predicate lexicon
  "tickets" — not in any predicate lexicon (gap)
  "iteration" — not in any predicate lexicon (gap)
  Result: CompiledPredicateIds: [], knowledge_edge
  CapabilityGapWarnings: ["gap-project-planning-backlog-001", "gap-project-planning-ticket-001", "gap-project-planning-iteration-001"]

---

## B36-0A-017-003 — Grooming with "story" (DOCUMENT fires)

A:
  run_id: "B36-0A-017-003"
  user_request: "Groom the backlog and refine the user story acceptance criteria for each ticket."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "story" — fires DOCUMENT (literal; "user story" contains "story")
  "acceptance criteria" — fires DOCUMENT (criteria ∈ DOCUMENT lexicon)
  "groom" — not in any predicate lexicon
  "backlog" — not in any predicate lexicon (gap)
  "ticket" — not in any predicate lexicon (gap)
  Mixed: DOCUMENT fires on "story" and "criteria"; grooming/backlog/ticket are gap warnings
  CapabilityGapWarnings: ["gap-project-planning-backlog-001", "gap-project-planning-ticket-001"]

---

## B36-0A-017-004 — Grooming with IMPLEMENT: add features from refined backlog

A:
  run_id: "B36-0A-017-004"
  user_request: "After grooming, implement the highest-priority backlog features in the current sprint."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "implement" — fires IMPLEMENT
  "features" — fires IMPLEMENT
  "grooming" — not in any predicate lexicon (gap context)
  "backlog" — not in any predicate lexicon (gap)
  "sprint" — not in any predicate lexicon (gap)
  Mixed: IMPLEMENT dominates; grooming and backlog are gap warnings
  CapabilityGapWarnings: ["gap-project-planning-backlog-001", "gap-project-planning-sprint-001"]

---

## B36-0A-017-005 — Grooming: five gap vocabulary terms, zero predicate

A:
  run_id: "B36-0A-017-005"
  user_request: "Facilitate the backlog grooming session, triage the epic tickets, and estimate story-points before the sprint."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

Predicate evidence:
  "facilitate" — not in any predicate lexicon
  "backlog grooming" — not in any predicate lexicon (gap)
  "triage" — not in any predicate lexicon (gap vocabulary)
  "epic" — not in any predicate lexicon (gap)
  "tickets" — not in any predicate lexicon (gap)
  "estimate" — not in any predicate lexicon
  "story-points" — "story" fires DOCUMENT! Correction: CompiledPredicateIds: ["DOCUMENT"]
  Correction note: Even in a grooming context, "story" fires DOCUMENT. CompiledPredicateIds: ["DOCUMENT"]
  CapabilityGapWarnings: ["gap-project-planning-backlog-001", "gap-project-planning-epic-001", "gap-project-planning-ticket-001", "gap-project-planning-story-point-001: DOCUMENT fires on 'story' substring", "gap-project-planning-sprint-001"]

Corrected B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null
  CapabilityGapWarnings: ["gap-project-planning-backlog-001", "gap-project-planning-epic-001", "gap-project-planning-ticket-001", "gap-project-planning-story-point-001", "gap-project-planning-sprint-001"]
