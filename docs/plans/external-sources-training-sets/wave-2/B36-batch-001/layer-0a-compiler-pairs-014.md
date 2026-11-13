# Layer 0-A — Compiler Pairs (014)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-014-001 — Effort-estimate pure gap: no predicate

A:
  run_id: "B36-0A-014-001"
  user_request: "Provide an effort estimate for each item in the backlog before we commit to the sprint."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

Predicate evidence:
  "effort estimate" — not in any predicate lexicon (gap: estimation vocabulary)
  "backlog" — not in any predicate lexicon (gap)
  "sprint" — not in any predicate lexicon (gap)
  "commit" — not in any predicate lexicon in this context (commit ≠ code commit)
  Result: CompiledPredicateIds: [], knowledge_edge
  CapabilityGapWarnings: ["gap-project-planning-backlog-001", "gap-project-planning-sprint-001", "gap-project-planning-estimation-001"]

---

## B36-0A-014-002 — Effort-estimate with "story" false positive (DOCUMENT fires)

A:
  run_id: "B36-0A-014-002"
  user_request: "Estimate the effort for each user story in the backlog and record the story point values."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "story" — fires DOCUMENT (literal; "user story" and "story point" both contain "story")
  "estimate" — not in any predicate lexicon
  "effort" — not in any predicate lexicon
  "backlog" — not in any predicate lexicon (gap)
  "record" — not in DOCUMENT lexicon (record ≠ document/spec/define)
  Mixed: DOCUMENT fires twice via "user story" and "story point"
  CapabilityGapWarnings: ["gap-project-planning-story-point-001: effort estimation intent not captured; DOCUMENT fires on 'story' substring", "gap-project-planning-backlog-001"]

---

## B36-0A-014-003 — Effort-estimate with IMPLEMENT co-signal

A:
  run_id: "B36-0A-014-003"
  user_request: "Estimate the effort required to implement the payment gateway feature before the sprint starts."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "implement" — fires IMPLEMENT
  "feature" — fires IMPLEMENT
  "estimate" — not in any predicate lexicon
  "effort" — not in any predicate lexicon
  "sprint" — not in any predicate lexicon (gap)
  Mixed: IMPLEMENT dominates; effort estimation is gap warning
  CapabilityGapWarnings: ["gap-project-planning-estimation-001: effort estimation context not captured", "gap-project-planning-sprint-001"]

---

## B36-0A-014-004 — Effort-estimate with DOCUMENT co-signal

A:
  run_id: "B36-0A-014-004"
  user_request: "Document the effort estimates and define acceptance criteria for all backlog items."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "document" — fires DOCUMENT
  "define" — fires DOCUMENT
  "acceptance criteria" — fires DOCUMENT (criteria ∈ DOCUMENT lexicon)
  "effort estimates" — "estimate" not in predicate lexicon; "document" fires DOCUMENT
  "backlog" — not in any predicate lexicon (gap)
  Mixed: DOCUMENT fires on three tokens; effort estimation and backlog are gap warnings
  CapabilityGapWarnings: ["gap-project-planning-estimation-001", "gap-project-planning-backlog-001"]

---

## B36-0A-014-005 — Effort-estimate: five gap terms, no predicate

A:
  run_id: "B36-0A-014-005"
  user_request: "Run a team effort-estimate session for the epic backlog and align on sprint capacity before grooming."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

Predicate evidence:
  "effort-estimate" — not in any predicate lexicon (gap)
  "epic" — not in any predicate lexicon (gap)
  "backlog" — not in any predicate lexicon (gap)
  "sprint" — not in any predicate lexicon (gap)
  "capacity" — not in any predicate lexicon in team context (gap)
  "grooming" — not in any predicate lexicon
  Result: CompiledPredicateIds: [], knowledge_edge
  CapabilityGapWarnings: ["gap-project-planning-estimation-001", "gap-project-planning-epic-001", "gap-project-planning-backlog-001", "gap-project-planning-sprint-001", "gap-project-planning-capacity-001"]
