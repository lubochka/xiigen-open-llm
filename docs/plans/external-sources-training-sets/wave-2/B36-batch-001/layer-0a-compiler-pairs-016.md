# Layer 0-A — Compiler Pairs (016)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-016-001 — Product-roadmap pure gap: no predicate

A:
  run_id: "B36-0A-016-001"
  user_request: "Present the product roadmap to the board and walk them through the Q3 and Q4 milestones."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

Predicate evidence:
  "product roadmap" — not in any predicate lexicon (gap)
  "present" — not in any predicate lexicon
  "milestones" — not in any predicate lexicon (gap)
  Result: CompiledPredicateIds: [], knowledge_edge
  CapabilityGapWarnings: ["gap-project-planning-roadmap-001", "gap-project-planning-milestone-001"]

---

## B36-0A-016-002 — Product-roadmap with DOCUMENT co-signal

A:
  run_id: "B36-0A-016-002"
  user_request: "Document the product roadmap and specify the requirements for each milestone deliverable."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "document" — fires DOCUMENT
  "specify" — fires DOCUMENT
  "requirements" — fires DOCUMENT
  "product roadmap" — not in any predicate lexicon (gap)
  "milestone" — not in any predicate lexicon (gap)
  Mixed: DOCUMENT fires on three tokens; roadmap and milestone are gap warnings
  CapabilityGapWarnings: ["gap-project-planning-roadmap-001", "gap-project-planning-milestone-001"]

---

## B36-0A-016-003 — Product-roadmap with "design" (ARCHITECTURE fires)

A:
  run_id: "B36-0A-016-003"
  user_request: "Design the product roadmap structure and architect the delivery phases for each epic."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["ARCHITECTURE"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "design" — fires ARCHITECTURE (always fires; even in roadmap context)
  "architect" — fires ARCHITECTURE
  "structure" — fires ARCHITECTURE
  "product roadmap" — not in any predicate lexicon (gap)
  "epic" — not in any predicate lexicon (gap)
  Mixed: ARCHITECTURE fires on three tokens; roadmap and epic are gap warnings
  CapabilityGapWarnings: ["gap-project-planning-roadmap-001", "gap-project-planning-epic-001"]

---

## B36-0A-016-004 — Product-roadmap with QA: "review the roadmap"

A:
  run_id: "B36-0A-016-004"
  user_request: "Review the product roadmap with stakeholders and approve the milestone targets."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["QA", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "review" — fires QA
  "approve" — fires QA (approve ∈ QA lexicon)
  "stakeholders" — fires DOCUMENT
  "product roadmap" — not in any predicate lexicon (gap)
  "milestone" — not in any predicate lexicon (gap)
  Mixed: both QA and DOCUMENT fire; roadmap and milestone are gap warnings
  CapabilityGapWarnings: ["gap-project-planning-roadmap-001", "gap-project-planning-milestone-001"]

---

## B36-0A-016-005 — Product-roadmap pure multi-term: five gap terms

A:
  run_id: "B36-0A-016-005"
  user_request: "Align the product roadmap with sprint velocity, epic priorities, and burndown projections for Q3."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

Predicate evidence:
  "align" — not in any predicate lexicon
  "product roadmap" — not in any predicate lexicon (gap)
  "sprint" — not in any predicate lexicon (gap)
  "velocity" — not in any predicate lexicon (gap)
  "epic" — not in any predicate lexicon (gap)
  "burndown" — not in any predicate lexicon (gap)
  Result: CompiledPredicateIds: [], knowledge_edge
  CapabilityGapWarnings: ["gap-project-planning-roadmap-001", "gap-project-planning-sprint-001", "gap-project-planning-velocity-001", "gap-project-planning-epic-001", "gap-project-planning-burndown-001"]
