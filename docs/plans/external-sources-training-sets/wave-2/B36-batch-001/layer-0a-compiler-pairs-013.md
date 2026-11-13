# Layer 0-A — Compiler Pairs (013)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-013-001 — Planning-poker with pure gap vocabulary

A:
  run_id: "B36-0A-013-001"
  user_request: "Let's run a planning-poker session to estimate the backlog items for the next sprint."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

Predicate evidence:
  "planning-poker" — not in any predicate lexicon
  "run" — not in any predicate lexicon
  "session" — not in any predicate lexicon
  "estimate" — not in any predicate lexicon (gap vocabulary)
  "backlog" — not in any predicate lexicon (gap)
  "sprint" — not in any predicate lexicon (gap)
  Result: CompiledPredicateIds: [], knowledge_edge
  CapabilityGapWarnings: ["gap-project-planning-sprint-001", "gap-project-planning-backlog-001"]

---

## B36-0A-013-002 — Planning-poker with "story" false positive

A:
  run_id: "B36-0A-013-002"
  user_request: "Run planning poker and assign story point estimates for each backlog item."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "story" — fires DOCUMENT (literal lexicon match; "story points" contains "story")
  "planning-poker" — not in any predicate lexicon
  "estimate" — not in any predicate lexicon
  "backlog" — not in any predicate lexicon (gap)
  Mixed: DOCUMENT fires on "story"; estimation intent not captured
  CapabilityGapWarnings: ["gap-project-planning-story-point-001: DOCUMENT fires on 'story' substring; planning-poker and estimation intent not captured"]

---

## B36-0A-013-003 — Planning-poker session with "review" (QA fires)

A:
  run_id: "B36-0A-013-003"
  user_request: "Review the planning poker results and finalize the sprint estimate."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "review" — fires QA (literal lexicon match; fires even in planning poker context)
  "planning poker" — not in any predicate lexicon
  "estimate" — not in any predicate lexicon
  "sprint" — not in any predicate lexicon (gap)
  Mixed: QA fires on "review"; planning poker intent not captured
  CapabilityGapWarnings: ["gap-project-planning-sprint-001: sprint planning context not captured"]

---

## B36-0A-013-004 — Planning-poker with IMPLEMENT (build outcome)

A:
  run_id: "B36-0A-013-004"
  user_request: "After planning poker, implement the top-priority backlog features for the sprint."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "implement" — fires IMPLEMENT (literal match)
  "features" — fires IMPLEMENT (feature ∈ IMPLEMENT lexicon)
  "planning poker" — not in any predicate lexicon (gap)
  "backlog" — not in any predicate lexicon (gap)
  "sprint" — not in any predicate lexicon (gap)
  Mixed: IMPLEMENT dominates; planning poker context is gap warning
  CapabilityGapWarnings: ["gap-project-planning-backlog-001", "gap-project-planning-sprint-001"]

---

## B36-0A-013-005 — Planning-poker pure estimation: five-term zero-predicate case

A:
  run_id: "B36-0A-013-005"
  user_request: "Facilitate a planning poker session to groom and estimate the product backlog for the next iteration."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

Predicate evidence:
  "facilitate" — not in any predicate lexicon
  "planning poker" — not in any predicate lexicon
  "groom" — not in any predicate lexicon
  "estimate" — not in any predicate lexicon
  "backlog" — not in any predicate lexicon (gap)
  "iteration" — not in any predicate lexicon (gap)
  Result: CompiledPredicateIds: [], knowledge_edge
  CapabilityGapWarnings: ["gap-project-planning-backlog-001", "gap-project-planning-iteration-001"]
