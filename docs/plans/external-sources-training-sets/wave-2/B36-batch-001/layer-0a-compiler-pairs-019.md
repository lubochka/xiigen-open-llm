# Layer 0-A — Compiler Pairs (019)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-019-001 — "story" false positive: pure story-points context

A:
  run_id: "B36-0A-019-001"
  user_request: "Estimate the story points for each backlog item before the sprint starts."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "story" — fires DOCUMENT (literal match; "story points" contains "story")
  "estimate" — not in any predicate lexicon
  "backlog" — not in any predicate lexicon (gap)
  "sprint" — not in any predicate lexicon (gap)
  Teaching: DOCUMENT fires on "story" even though developer intent is effort estimation.
    The compiler is literal. False positive in intent terms; correct in lexicon terms.
  CapabilityGapWarnings: ["gap-project-planning-story-point-001: DOCUMENT fires on 'story' substring; effort estimation intent not captured", "gap-project-planning-backlog-001", "gap-project-planning-sprint-001"]

---

## B36-0A-019-002 — "user story" fires DOCUMENT

A:
  run_id: "B36-0A-019-002"
  user_request: "Write up the user story for the checkout flow and assign story points."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "story" — fires DOCUMENT (fires twice: "user story" and "story points")
  "write" — fires DOCUMENT ("write" in requirements/story context ∈ DOCUMENT lexicon) or IMPLEMENT depending on co-occurrence; here "write up" + "user story" → DOCUMENT
  In this case: intent is clearly documentation (user story authoring), so DOCUMENT is correct in both intent and lexicon terms
  CapabilityGapWarnings: ["gap-project-planning-story-point-001: story-point assignment intent partially captured via DOCUMENT"]

---

## B36-0A-019-003 — "stories" plural fires DOCUMENT

A:
  run_id: "B36-0A-019-003"
  user_request: "Assign story points to all backlog stories before the sprint planning meeting."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "story" — fires DOCUMENT (fires via "story points" and "stories")
  "stories" — fires DOCUMENT ("stories" contains "story" substring)
  "backlog" — not in any predicate lexicon (gap)
  "sprint" — not in any predicate lexicon (gap)
  Teaching: Both "story points" and "stories" fire DOCUMENT via the "story" substring.
  CapabilityGapWarnings: ["gap-project-planning-story-point-001", "gap-project-planning-backlog-001", "gap-project-planning-sprint-001"]

---

## B36-0A-019-004 — "story" false positive but no project planning gap term

A:
  run_id: "B36-0A-019-004"
  user_request: "Create a user story for the notification feature."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "story" — fires DOCUMENT
  "feature" — fires IMPLEMENT
  "create" — context-dependent; co-occurring with "feature" → IMPLEMENT fires
  No project planning gap vocabulary in this request
  Teaching: when "story" fires DOCUMENT alongside "feature" (IMPLEMENT), both predicates fire.
    No gap warnings needed — no planning vocabulary present.
  CapabilityGapWarnings: []

---

## B36-0A-019-005 — "story" with retrospective (gap + DOCUMENT false positive)

A:
  run_id: "B36-0A-019-005"
  user_request: "In the retrospective, review the story points assigned last sprint and update velocity."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["QA", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "review" — fires QA
  "story" — fires DOCUMENT (via "story points")
  "retrospective" — NOT in QA lexicon (gap vocabulary); distinct from "review"
  "sprint" — not in any predicate lexicon (gap)
  "velocity" — not in any predicate lexicon (gap)
  "update" — not in IMPLEMENT lexicon (no code-level entity co-occurs)
  Mixed: QA and DOCUMENT fire; retrospective/sprint/velocity are gap warnings
  Teaching: "retrospective" ≠ QA. "review" = QA. The two tokens have distinct lexicon status.
  CapabilityGapWarnings: ["gap-project-planning-retrospective-001: 'retrospective' is gap, not QA", "gap-project-planning-sprint-001", "gap-project-planning-velocity-001", "gap-project-planning-story-point-001: DOCUMENT fires on 'story' substring"]
