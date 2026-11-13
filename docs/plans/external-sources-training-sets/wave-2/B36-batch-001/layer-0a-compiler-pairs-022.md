# Layer 0-A — Compiler Pairs (022)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-022-001 — Roadmap + DOCUMENT: "document the roadmap items"

A:
  run_id: "B36-0A-022-001"
  user_request: "Document the roadmap items and specify the requirements for each quarterly milestone."
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
  "roadmap" — not in any predicate lexicon (gap)
  "milestone" — not in any predicate lexicon (gap)
  Mixed: DOCUMENT fires on three tokens; roadmap and milestone are gap warnings
  CapabilityGapWarnings: ["gap-project-planning-roadmap-001", "gap-project-planning-milestone-001"]

---

## B36-0A-022-002 — Sprint + DOCUMENT: "write the sprint criteria"

A:
  run_id: "B36-0A-022-002"
  user_request: "Write the acceptance criteria for each sprint ticket and describe the expected behavior."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "criteria" — fires DOCUMENT
  "acceptance criteria" — fires DOCUMENT
  "describe" — fires DOCUMENT
  "behavior" — fires DOCUMENT (behavior ∈ DOCUMENT lexicon: "functional behavior")
  "sprint" — not in any predicate lexicon (gap)
  "ticket" — not in any predicate lexicon (gap)
  Mixed: DOCUMENT fires on multiple tokens; sprint and ticket are gap warnings
  CapabilityGapWarnings: ["gap-project-planning-sprint-001", "gap-project-planning-ticket-001"]

---

## B36-0A-022-003 — Backlog + DOCUMENT: "define backlog requirements"

A:
  run_id: "B36-0A-022-003"
  user_request: "Define the backlog requirements and document the functional specifications for each epic."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "define" — fires DOCUMENT
  "requirements" — fires DOCUMENT
  "document" — fires DOCUMENT
  "functional" — fires DOCUMENT (functional ∈ DOCUMENT lexicon)
  "specifications" — fires DOCUMENT (spec/specification ∈ DOCUMENT lexicon)
  "backlog" — not in any predicate lexicon (gap)
  "epic" — not in any predicate lexicon (gap)
  Mixed: DOCUMENT fires on five tokens; backlog and epic are gap warnings
  CapabilityGapWarnings: ["gap-project-planning-backlog-001", "gap-project-planning-epic-001"]

---

## B36-0A-022-004 — Velocity + DOCUMENT: "document velocity metrics"

A:
  run_id: "B36-0A-022-004"
  user_request: "Document the team velocity metrics and specify how they relate to the sprint capacity."
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
  "velocity" — not in any predicate lexicon (gap)
  "sprint" — not in any predicate lexicon (gap)
  "capacity" — not in any predicate lexicon in team context (gap)
  Mixed: DOCUMENT fires; velocity/sprint/capacity are gap warnings
  CapabilityGapWarnings: ["gap-project-planning-velocity-001", "gap-project-planning-sprint-001", "gap-project-planning-capacity-001"]

---

## B36-0A-022-005 — Milestone + DOCUMENT: "describe milestone deliverables"

A:
  run_id: "B36-0A-022-005"
  user_request: "Describe the deliverables for each roadmap milestone and define the use-case for each major epic."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "describe" — fires DOCUMENT
  "define" — fires DOCUMENT
  "use-case" — fires DOCUMENT (use-case ∈ DOCUMENT lexicon)
  "milestone" — not in any predicate lexicon (gap)
  "roadmap" — not in any predicate lexicon (gap)
  "epic" — not in any predicate lexicon (gap)
  Mixed: DOCUMENT fires on three tokens; milestone/roadmap/epic are gap warnings
  CapabilityGapWarnings: ["gap-project-planning-milestone-001", "gap-project-planning-roadmap-001", "gap-project-planning-epic-001"]
