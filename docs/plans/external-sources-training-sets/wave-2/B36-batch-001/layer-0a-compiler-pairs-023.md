# Layer 0-A — Compiler Pairs (023)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-023-001 — "retrospective" does NOT fire QA; "review" DOES fire QA

A:
  run_id: "B36-0A-023-001"
  user_request: "Run the sprint retrospective and capture what went well."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

Predicate evidence:
  "retrospective" — NOT in QA lexicon; this is a gap vocabulary term
  "run" — not in any predicate lexicon
  "sprint" — not in any predicate lexicon (gap)
  "capture" — not in DOCUMENT lexicon
  BOUNDARY TEACHING: "retrospective" ≠ "review". "review" fires QA. "retrospective" does not.
  Result: CompiledPredicateIds: [], knowledge_edge
  CapabilityGapWarnings: ["gap-project-planning-retrospective-001", "gap-project-planning-sprint-001"]

---

## B36-0A-023-002 — "review" fires QA in planning context

A:
  run_id: "B36-0A-023-002"
  user_request: "Review the sprint outcomes and assess team performance."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "review" — fires QA (literal; fires even in sprint assessment context)
  "sprint" — not in any predicate lexicon (gap)
  "assess" — not in any predicate lexicon
  BOUNDARY TEACHING: "review" fires QA always — not just for code/software reviews.
  CapabilityGapWarnings: ["gap-project-planning-sprint-001"]

---

## B36-0A-023-003 — "retrospective" and "review" in same sentence

A:
  run_id: "B36-0A-023-003"
  user_request: "In the retrospective, review the code quality from the last sprint."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "review" — fires QA (literal match)
  "retrospective" — does NOT fire QA; this is gap vocabulary
  "code" — IMPLEMENT co-occurrence check: "review the code quality" → "code" alone without build/implement context does not fire IMPLEMENT; but "code quality" is a QA signal
  "sprint" — not in any predicate lexicon (gap)
  Mixed: QA fires on "review"; retrospective is gap warning; "review" dominates
  CapabilityGapWarnings: ["gap-project-planning-retrospective-001: 'retrospective' is gap, not QA", "gap-project-planning-sprint-001"]

---

## B36-0A-023-004 — "sprint review" (both sprint and review present): only QA fires

A:
  run_id: "B36-0A-023-004"
  user_request: "Run the sprint review meeting and demonstrate the completed features to stakeholders."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["QA", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "review" — fires QA (literal; "sprint review" contains "review")
  "features" — fires IMPLEMENT (feature ∈ IMPLEMENT lexicon)
  "stakeholders" — fires DOCUMENT
  "sprint" — not in any predicate lexicon (gap)
  Note: "sprint review" is a Scrum ceremony, but "review" fires QA and "features" fires IMPLEMENT
  Correction: CompiledPredicateIds: ["QA", "IMPLEMENT", "DOCUMENT"]
  BOUNDARY TEACHING: The word "review" in "sprint review" fires QA. "Sprint" remains gap.
  CapabilityGapWarnings: ["gap-project-planning-sprint-001"]

Corrected B:
  CompiledPredicateIds: ["QA", "IMPLEMENT", "DOCUMENT"]
  CapabilityGapWarnings: ["gap-project-planning-sprint-001"]

---

## B36-0A-023-005 — Zero predicates: retrospective-only sentence

A:
  run_id: "B36-0A-023-005"
  user_request: "Facilitate the team retrospective and identify process improvements for the next sprint."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

Predicate evidence:
  "facilitate" — not in any predicate lexicon
  "retrospective" — NOT in any predicate lexicon (gap vocabulary, not QA)
  "process improvements" — not in any predicate lexicon
  "sprint" — not in any predicate lexicon (gap)
  BOUNDARY TEACHING: "retrospective" and "sprint" are both gap. Neither fires any predicate.
  Result: CompiledPredicateIds: [], knowledge_edge
  CapabilityGapWarnings: ["gap-project-planning-retrospective-001", "gap-project-planning-sprint-001"]
