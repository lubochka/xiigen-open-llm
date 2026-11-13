# Layer 0-A — Compiler Pairs (024)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-024-001 — Complete gap vocabulary: all 15 gap terms, zero predicate

A:
  run_id: "B36-0A-024-001"
  user_request: "Plan the sprint backlog, update the roadmap, track milestones, review velocity, run the standup, hold the retrospective, view the burndown chart, manage team capacity, and plan the next iteration."
  Task: "Identify all gap terms, confirm no predicate fires, list all gap IDs."

B:
  CompiledPredicateIds: []
  NOTE: "review" fires QA! Correction required.
  "review" is in the request → QA fires → CompiledPredicateIds: ["QA"]

  Corrected B:
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-sprint-001"
    - "gap-project-planning-backlog-001"
    - "gap-project-planning-roadmap-001"
    - "gap-project-planning-milestone-001"
    - "gap-project-planning-velocity-001"
    - "gap-project-planning-standup-001"
    - "gap-project-planning-retrospective-001"
    - "gap-project-planning-burndown-001"
    - "gap-project-planning-capacity-001"
    - "gap-project-planning-iteration-001"

  Teaching: Even a request with 10+ gap terms produces QA if "review" is present. Zero predicates only if NO lexicon token appears.

---

## B36-0A-024-002 — Complete gap vocabulary: zero-predicate version (no review)

A:
  run_id: "B36-0A-024-002"
  user_request: "Plan the sprint, groom the backlog, align the roadmap, hit the milestone, track velocity, run standups, facilitate retrospective, view burndown, manage capacity, plan the iteration, break down the epic, assign tickets, estimate story-points, set effort-estimates, finalize the release-plan."
  Task: "Note: 'story-points' contains 'story' → DOCUMENT fires. Identify all predicates and gaps."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null
  CapabilityGapWarnings:
    - "gap-project-planning-sprint-001"
    - "gap-project-planning-backlog-001"
    - "gap-project-planning-roadmap-001"
    - "gap-project-planning-milestone-001"
    - "gap-project-planning-velocity-001"
    - "gap-project-planning-standup-001"
    - "gap-project-planning-retrospective-001"
    - "gap-project-planning-burndown-001"
    - "gap-project-planning-capacity-001"
    - "gap-project-planning-iteration-001"
    - "gap-project-planning-epic-001"
    - "gap-project-planning-ticket-001"
    - "gap-project-planning-story-point-001: DOCUMENT fires on 'story' substring"
    - "gap-project-planning-estimation-001"
    - "gap-project-planning-release-cycle-001"
  SCHEDULE_CAPABILITY_LEARNING: emitted for all 15 gap IDs

---

## B36-0A-024-003 — DomainId determination: zero predicates vs any predicate

A:
  run_id: "B36-0A-024-003"
  Task: "State the DomainId rule for B36 gap batches with examples."

B:
  Rule: "DomainId: 'project-management' ONLY when CompiledPredicateIds: []. Any predicate fired → DomainId: 'software-development'."
  Examples:
    - input: "Plan the sprint and groom the backlog."
      predicates: []
      DomainId: "project-management"
    - input: "Review the sprint deliverables."
      predicates: ["QA"]
      DomainId: "software-development"
    - input: "Build the sprint tracker."
      predicates: ["IMPLEMENT"]
      DomainId: "software-development"
    - input: "Estimate story points for the backlog."
      predicates: ["DOCUMENT"]
      DomainId: "software-development"
  Hard rule: Gap vocabulary proportion is irrelevant. One predicate token → "software-development".

---

## B36-0A-024-004 — New gap vocabulary: planning-poker, effort-estimate, grooming, triage, prioritize

A:
  run_id: "B36-0A-024-004"
  Task: "Confirm that planning-poker, effort-estimate, grooming, triage, and prioritize are gap vocabulary."

B:
  GapVocabularyExtension:
    - term: "planning-poker"
      predicate_match: null
      gap_id: "gap-project-planning-sprint-001 (compound with sprint)"
      notes: "Estimation ceremony; not in any predicate lexicon"
    - term: "effort-estimate"
      predicate_match: null
      gap_id: "gap-project-planning-estimation-001"
      notes: "Effort sizing; not in any predicate lexicon"
    - term: "grooming"
      predicate_match: null
      gap_id: "gap-project-planning-backlog-001 (compound with backlog)"
      notes: "Backlog refinement synonym; not in any predicate lexicon"
    - term: "triage"
      predicate_match: null
      gap_id: "gap-project-planning-ticket-001 (compound with ticket)"
      notes: "Issue prioritization act; not in any predicate lexicon"
    - term: "prioritize"
      predicate_match: null
      gap_id: "gap-project-planning-backlog-001 (compound with backlog)"
      notes: "Ordering action; not in any predicate lexicon"
    - term: "product-roadmap"
      predicate_match: null
      gap_id: "gap-project-planning-roadmap-001"
      notes: "Compound; 'roadmap' is the gap token"
    - term: "release-planning"
      predicate_match: null
      gap_id: "gap-project-planning-release-cycle-001"
      notes: "Release schedule planning; 'release' alone ≠ DOCUMENT"

---

## B36-0A-024-005 — Complete boundary table: gap vs predicate for B36 vocabulary

A:
  run_id: "B36-0A-024-005"
  Task: "Full boundary table: which B36 planning tokens fire a predicate and which do not."

B:
  BoundaryTable:
    GAP (no predicate fires):
      - sprint, roadmap, milestone, backlog, velocity, standup, retrospective
      - burndown, capacity (team), iteration, epic, ticket, estimation
      - release-cycle, planning-poker, effort-estimate, grooming, triage, prioritize
      - groom, facilitate, track, run (ceremony context), generate, capture
      - update (planning context without code entity)

    PREDICATE FIRES (even in planning sentences):
      - "review" → QA
      - "approve" → QA
      - "test" → TEST
      - "story" → DOCUMENT (false positive in "story points")
      - "stories" → DOCUMENT
      - "document" → DOCUMENT
      - "define" → DOCUMENT
      - "specify" → DOCUMENT
      - "requirements" → DOCUMENT
      - "stakeholder" → DOCUMENT
      - "criteria" → DOCUMENT
      - "use-case" → DOCUMENT
      - "functional" → DOCUMENT
      - "behavior" → DOCUMENT
      - "implement" → IMPLEMENT
      - "feature" → IMPLEMENT
      - "build" → IMPLEMENT
      - "create" (with code entity) → IMPLEMENT
      - "write" (with code entity) → IMPLEMENT
      - "design" → ARCHITECTURE
      - "architect" → ARCHITECTURE
      - "structure" → ARCHITECTURE
      - "architecture" → ARCHITECTURE

  Hard rule: No planning context suppresses a predicate. Lexicon match fires unconditionally.
