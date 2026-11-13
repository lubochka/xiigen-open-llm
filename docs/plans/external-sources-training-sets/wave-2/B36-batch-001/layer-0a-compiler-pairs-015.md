# Layer 0-A — Compiler Pairs (015)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-015-001 — Release-planning pure gap: no predicate

A:
  run_id: "B36-0A-015-001"
  user_request: "Create the release plan for Q4 and map each milestone to a sprint delivery date."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

Predicate evidence:
  "create" — IMPLEMENT requires code-level entity co-occurrence ("create" alone without function/class/module does not fire in a release planning context)
  "release plan" — not in any predicate lexicon; "release" is not in DOCUMENT lexicon
  "milestone" — not in any predicate lexicon (gap)
  "sprint" — not in any predicate lexicon (gap)
  Result: CompiledPredicateIds: [], knowledge_edge
  CapabilityGapWarnings: ["gap-project-planning-milestone-001", "gap-project-planning-sprint-001", "gap-project-planning-release-cycle-001"]

---

## B36-0A-015-002 — Release-planning with QA boundary ("review" fires)

A:
  run_id: "B36-0A-015-002"
  user_request: "Review the release plan and confirm the milestone dates with the stakeholders."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "review" — fires QA (literal; even in release planning context)
  "release plan" — not in any predicate lexicon (gap)
  "milestone" — not in any predicate lexicon (gap)
  "stakeholders" — "stakeholder" is in DOCUMENT lexicon → DOCUMENT fires too
  Mixed: both QA and DOCUMENT fire
  Correction: CompiledPredicateIds: ["QA", "DOCUMENT"]
  CapabilityGapWarnings: ["gap-project-planning-release-cycle-001", "gap-project-planning-milestone-001"]

---

## B36-0A-015-003 — Release-planning with IMPLEMENT boundary (build features)

A:
  run_id: "B36-0A-015-003"
  user_request: "Release planning complete — now build the authentication module and implement the login flow."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "build" — fires IMPLEMENT
  "implement" — fires IMPLEMENT
  "module" — co-occurrence with "build" reinforces IMPLEMENT
  "login flow" — not a separate predicate but co-occurs with IMPLEMENT
  "release planning" — not in any predicate lexicon (gap)
  Mixed: IMPLEMENT dominates; release planning is gap warning
  CapabilityGapWarnings: ["gap-project-planning-release-cycle-001: release planning context not captured; routing is for IMPLEMENT task"]

---

## B36-0A-015-004 — Release-planning: "create release notes" fires DOCUMENT

A:
  run_id: "B36-0A-015-004"
  user_request: "Create the release notes document and define the scope for the next release cycle."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "document" — fires DOCUMENT (literal; "release notes document")
  "define" — fires DOCUMENT
  "release cycle" — not in any predicate lexicon (gap); "release" alone ≠ DOCUMENT
  Mixed: DOCUMENT fires on "document" and "define"; release cycle is gap warning
  CapabilityGapWarnings: ["gap-project-planning-release-cycle-001: release cycle scope not captured by DOCUMENT routing"]

---

## B36-0A-015-005 — Release-planning pure: five terms, no predicate

A:
  run_id: "B36-0A-015-005"
  user_request: "Finalize the release plan timeline, assign milestones to epics, and track sprint velocity against the target."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

Predicate evidence:
  "finalize" — not in any predicate lexicon
  "release plan" — not in any predicate lexicon (gap)
  "milestones" — not in any predicate lexicon (gap)
  "epics" — not in any predicate lexicon (gap)
  "sprint" — not in any predicate lexicon (gap)
  "velocity" — not in any predicate lexicon (gap)
  "track" — not in any predicate lexicon
  Result: CompiledPredicateIds: [], knowledge_edge
  CapabilityGapWarnings: ["gap-project-planning-release-cycle-001", "gap-project-planning-milestone-001", "gap-project-planning-epic-001", "gap-project-planning-sprint-001", "gap-project-planning-velocity-001"]
