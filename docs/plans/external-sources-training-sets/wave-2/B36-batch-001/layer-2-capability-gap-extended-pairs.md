# Layer 2 — Capability Gap Extended Pairs (replaces nine-node layer)
# Batch: B36-batch-001
# Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Note: Layer 2 is replaced for gap batches. No nine-node flow executes for
#       pure project planning requests. This file extends the gap vocabulary
#       coverage across the full project planning lexicon and teaches the model
#       the complete boundary between development predicates and gap vocabulary.
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: capability_gap_extended

---

## Method note

Gap batches do not have a Layer 2 nine-node flow because no sub-phase owns
pure project planning vocabulary. Instead, Layer 2 is extended capability-gap
teaching: additional vocabulary coverage, additional boundary cases, and
structured gap-ID emission pairs that the model must learn to produce.

The model must learn:
  1. Which terms belong to which gap domains
  2. That gap domains cannot co-exist with registered sub-phases (no fallback)
  3. The correct gap-ID naming convention
  4. How SCHEDULE_CAPABILITY_LEARNING is triggered and what it means

---

## Pair B36B1-WDA-gap-001 — full project planning vocabulary map

```
source_id: xiigen-b36-gap-001
pair_kind: capability_gap_extended
```

A:
  Task: "Identify which project planning terms have no registered predicate
         and emit the correct capability gap ID for each."
  VocabularySet:
    - sprint
    - roadmap
    - milestone
    - backlog
    - velocity
    - standup
    - retrospective
    - burndown
    - capacity
    - iteration
    - epic
    - ticket
    - story-point (as a compound)

B:
  GapVocabularyMap:
    - term: "sprint"
      predicate_match: null
      gap_id: "gap-project-planning-sprint-001"
      notes: "Core Scrum term; not in any of the nine predicate lexicons"

    - term: "roadmap"
      predicate_match: null
      gap_id: "gap-project-planning-roadmap-001"
      notes: "Strategic planning artifact; no predicate owns planning artifact vocabulary"

    - term: "milestone"
      predicate_match: null
      gap_id: "gap-project-planning-milestone-001"
      notes: "Timeline marker; no predicate owns schedule vocabulary"

    - term: "backlog"
      predicate_match: null
      gap_id: "gap-project-planning-backlog-001"
      notes: "Work queue concept; no predicate owns queue management vocabulary"

    - term: "velocity"
      predicate_match: null
      gap_id: "gap-project-planning-velocity-001"
      notes: "Team throughput metric; no predicate owns planning metrics vocabulary"

    - term: "standup"
      predicate_match: null
      gap_id: "gap-project-planning-standup-001"
      notes: "Scrum ceremony; no predicate owns ceremony vocabulary"

    - term: "retrospective"
      predicate_match: null
      gap_id: "gap-project-planning-retrospective-001"
      notes: "Scrum ceremony; not in any lexicon despite 'review' being in QA"

    - term: "burndown"
      predicate_match: null
      gap_id: "gap-project-planning-burndown-001"
      notes: "Progress chart; no predicate owns visualization or charting vocabulary"

    - term: "capacity"
      predicate_match: null
      gap_id: "gap-project-planning-capacity-001"
      notes: "Team planning term; not in ARCHITECTURE (which owns system capacity)"

    - term: "iteration"
      predicate_match: null
      gap_id: "gap-project-planning-iteration-001"
      notes: "Agile cycle term; 'iteration' is not in IMPLEMENT lexicon"

    - term: "epic"
      predicate_match: null
      gap_id: "gap-project-planning-epic-001"
      notes: "Jira/planning hierarchy term; no predicate owns issue hierarchy vocabulary"

    - term: "ticket"
      predicate_match: null
      gap_id: "gap-project-planning-ticket-001"
      notes: "Issue tracker term; no predicate owns issue tracker vocabulary"

    - term: "story-point"
      predicate_match: "DOCUMENT (on 'story' substring)"
      gap_id: "gap-project-planning-story-point-001"
      notes: "IMPORTANT: 'story' within 'story points' fires DOCUMENT. This is a
              false-positive in intent terms — the developer means estimation units,
              not user stories. The compiler is literal. DOCUMENT fires. The gap ID
              covers the estimation intent that is not captured by the DOCUMENT routing."

  TotalGapCount: 13
  TermsWithFalsePositivePredicate: ["story-point"]
  RecommendedAction: "SCHEDULE_CAPABILITY_LEARNING for all 13 gap IDs"

---

## Pair B36B1-WDA-gap-002 — ARCHITECTURE false-negative boundary

```
source_id: xiigen-b36-gap-002
pair_kind: capability_gap_extended
```

A:
  CompiledText: "Design the sprint capacity model and architect the team's quarterly delivery plan."
  Task: "Identify which predicates fire and explain the boundary between
         ARCHITECTURE (system design) and project planning vocabulary."

B:
  PredicateAnalysis:
    - term: "design"
      fires: "ARCHITECTURE (design → design)"
      note: "ARCHITECTURE fires on 'design'"

    - term: "architect"
      fires: "ARCHITECTURE (architect → architect)"
      note: "ARCHITECTURE fires on 'architect'"

    - term: "sprint"
      fires: null
      note: "Gap vocabulary — no predicate"

    - term: "capacity"
      fires: null
      note: "Gap vocabulary — 'capacity' is NOT in ARCHITECTURE lexicon even though
             ARCHITECTURE owns system-level capacity concerns. The ARCHITECTURE
             predicate fires on: architect, design, structure, pattern, component,
             module, layer, service, system. 'Team capacity' is not system capacity."

    - term: "delivery plan"
      fires: null
      note: "Gap vocabulary — planning artifact"

  CompiledPredicateIds: ["ARCHITECTURE"]
  SourceStatus: "ready"
  Routing: "→ Phase 3 sub-phases (Technical Design area)"
  CapabilityGapWarnings:
    - "gap-project-planning-sprint-001: 'sprint' not captured"
    - "gap-project-planning-capacity-001: 'team capacity' is not system capacity — ARCHITECTURE does not fire"
    - "gap-project-planning-delivery-plan-001: planning artifact not captured"

  Teaching point: "ARCHITECTURE fires on 'design' and 'architect' — but the routing
  goes to Phase 3 (Technical Design), NOT to a project planning sub-phase. The sprint
  and capacity vocabulary produce gap warnings but do not alter the routing. The model
  must learn that 'architect' and 'design' are ARCHITECTURE terms even in a project
  planning sentence."

---

## Pair B36B1-WDA-gap-003 — standup and retrospective: full knowledge-edge path

```
source_id: xiigen-b36-gap-003
pair_kind: capability_gap_extended
```

A:
  DocumentId: "B36-gap-003-prompt"
  CompiledText: "Run our daily standup and capture the action items from yesterday's retrospective."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

Predicate analysis:
  "standup" — not in any predicate lexicon
  "capture" — not in DOCUMENT lexicon (capture ≠ document, spec, define, describe)
  "action items" — not in any predicate lexicon
  "retrospective" — not in any predicate lexicon (distinct from "review" which IS in QA)
  Result: CompiledPredicateIds: []

B:
  FlowId: "knowledge-edge-missing-flow"
  ScenarioId: "adaptive-logical-orchestration"
  OverallVerdict: "KNOWLEDGE_EDGE"
  FinalHumanResult: null
  Metrics:
    knowledge_edge_reason: "no_predicate_match_capability_gap"
    matched_predicate_count: "0"
    capability_gap_id: "gap-project-planning-standup-retrospective-001"
    capability_gap_description: "Request uses Scrum ceremony vocabulary (standup,
      retrospective, action items) that has no registered predicate. Neither 'capture'
      nor 'run' are in any predicate lexicon in this context. Capability learning is
      scheduled."
    recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
    next_flow_id: "null"
  EvidenceIds: ["gap-project-planning-standup-retrospective-001"]
  Warnings:
    - "knowledge_edge: capability_gap vocabulary detected: standup, retrospective"
    - "capability_gap_id: gap-project-planning-standup-retrospective-001"
    - "no_registered_sub_phase_for_scrum_ceremonies"

Teaching point: "retrospective" is distinct from "review". "review" fires QA.
"retrospective" does not. The model must not confuse these two terms.

---

## Pair B36B1-WDA-gap-004 — burndown chart request: full knowledge-edge path

```
source_id: xiigen-b36-gap-004
pair_kind: capability_gap_extended
```

A:
  DocumentId: "B36-gap-004-prompt"
  CompiledText: "Generate the sprint burndown chart and update the milestone tracker for Q3."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

Predicate analysis:
  "generate" — not in any predicate lexicon (generate ≠ implement/build/write)
  "sprint" — not in any predicate lexicon
  "burndown" — not in any predicate lexicon
  "chart" — not in any predicate lexicon
  "update" — not in IMPLEMENT lexicon (update requires: code/build/implement context)
  "milestone" — not in any predicate lexicon
  "tracker" — not in any predicate lexicon
  Result: CompiledPredicateIds: []

B:
  FlowId: "knowledge-edge-missing-flow"
  ScenarioId: "adaptive-logical-orchestration"
  OverallVerdict: "KNOWLEDGE_EDGE"
  FinalHumanResult: null
  Metrics:
    knowledge_edge_reason: "no_predicate_match_capability_gap"
    matched_predicate_count: "0"
    capability_gap_id: "gap-project-planning-burndown-milestone-tracker-001"
    capability_gap_description: "Request uses project planning artifact vocabulary
      (burndown chart, milestone tracker) with no registered predicate. 'generate'
      and 'update' do not fire any predicate in this context."
    recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
    next_flow_id: "null"
  EvidenceIds: ["gap-project-planning-burndown-milestone-tracker-001"]
  Warnings:
    - "knowledge_edge: capability_gap vocabulary detected: sprint, burndown, milestone, tracker"
    - "capability_gap_id: gap-project-planning-burndown-milestone-tracker-001"

Teaching point: "update" does NOT fire IMPLEMENT here. IMPLEMENT fires on
update only when co-occurring with code-level terms (function, class, logic,
module). "Update the milestone tracker" is a project management sentence —
IMPLEMENT does not fire.

---

## Pair B36B1-WDA-gap-005 — gap-ID naming convention reference

```
source_id: xiigen-b36-gap-005
pair_kind: capability_gap_extended
```

A:
  Task: "State the gap-ID naming convention for project planning vocabulary gaps."

B:
  Convention: "gap-{domain}-{term(s)}-{sequence}"
  Domain: "project-planning"
  Examples:
    - "gap-project-planning-sprint-001"
    - "gap-project-planning-roadmap-001"
    - "gap-project-planning-sprint-roadmap-001" (compound when multiple terms co-occur in same request)
    - "gap-project-planning-standup-retrospective-001"
    - "gap-project-planning-burndown-milestone-tracker-001"
  Rules:
    - Use hyphens throughout (no underscores in the ID string)
    - The domain segment matches the DomainId emitted by the orchestrator: "project-management"
    - Terms are listed in order of appearance in the request
    - Sequence starts at 001 and increments per new gap ID registered
    - When a compound term spans multiple gap vocabulary items, list both (sprint-roadmap, not just sprint)
  Note: The gap-ID is not the same as the CapabilityGapWarning string. The gap-ID
  is the registered identifier; the warning string is a human-readable description.
