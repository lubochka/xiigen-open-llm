# Layer 0-C — Knowledge-Edge Pairs (Extended)
# Batch: B36-batch-001
# Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Contract: AdaptiveLogicalOrchestrator — knowledge-edge result path
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: knowledge_edge

---

## Method note

B36 is a capability-gap batch. Layer 0-C is extended with additional
knowledge-edge pairs covering the full vocabulary range of project planning
terms. Layer 2 is replaced by these extended knowledge-edge pairs because
no registered flow exists for pure project planning (no sub-phase owns this domain).

For each gap vocabulary term, the model must learn:
  1. That the term fires no predicate
  2. That the correct orchestrator response is KNOWLEDGE_EDGE
  3. The specific FailureClassification and capability gap ID to emit
  4. The recommended action: SCHEDULE_CAPABILITY_LEARNING

---

## Pair B36B1-WDA-kedge-001 — sprint planning: pure gap, no predicate

```
source_id: xiigen-b36-kedge-001
source_title: XIIGen B36 knowledge-edge — sprint planning vocabulary
pair_kind: knowledge_edge
```

A:
  DocumentId: "B36-compiler-001-prompt"
  CompiledText: "We need to plan the next sprint and update the roadmap with the new milestones for Q3."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

B:
  FlowId: "knowledge-edge-missing-flow"
  ScenarioId: "adaptive-logical-orchestration"
  OverallVerdict: "KNOWLEDGE_EDGE"
  FinalHumanResult: null
  Metrics:
    knowledge_edge_reason: "no_predicate_match_capability_gap"
    matched_predicate_count: "0"
    capability_gap_id: "gap-project-planning-sprint-roadmap-001"
    capability_gap_description: "Request uses project planning vocabulary (sprint, roadmap, milestone) that has no registered predicate. The orchestrator cannot route to any development sub-phase. Capability learning is scheduled to potentially extend the predicate lexicon or create a project planning routing layer."
    recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
    next_flow_id: "null"
  EvidenceIds: ["gap-project-planning-sprint-roadmap-001"]
  Warnings:
    - "knowledge_edge: capability_gap vocabulary detected: sprint, roadmap, milestone"
    - "capability_gap_id: gap-project-planning-sprint-roadmap-001"
    - "no_registered_sub_phase_for_project_planning"

---

## Pair B36B1-WDA-kedge-002 — backlog grooming: pure gap, no predicate

```
source_id: xiigen-b36-kedge-002
source_title: XIIGen B36 knowledge-edge — backlog grooming vocabulary
pair_kind: knowledge_edge
```

A:
  DocumentId: "B36-kedge-002-prompt"
  CompiledText: "Let's groom the backlog and estimate story points for the items we want to pull into the next iteration."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

Predicate analysis:
  "backlog" — not in any predicate lexicon
  "estimate" — not in any predicate lexicon
  "story points" — "story" is in DOCUMENT lexicon → DOCUMENT fires!

  Correction: "story" matches DOCUMENT lexicon.
  CompiledPredicateIds: ["DOCUMENT"] — DOCUMENT fires on "story"
  SourceStatus: "ready"

  This is a boundary teaching case: "story" in "story points" fires the DOCUMENT
  predicate even though the developer intent is project estimation, not documentation.
  The compiler is literal — it fires on term presence, not semantic intent.

  Corrected B output:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null
  CapabilityGapWarnings: ["gap-project-planning-backlog-estimation: 'backlog', 'story points', 'iteration' have no registered predicate; 'story' fires DOCUMENT but intent is project planning estimation"]

Corrected B:
  DocumentId: "B36-kedge-002-prompt"
  CompiledText: "Let's groom the backlog and estimate story points for the items we want to pull into the next iteration."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Routing consequence:
  DOCUMENT predicate fires on "story" → router routes to 2.1 (use cases / user stories).
  The actual user intent (backlog estimation) is NOT routed. The model must learn that
  "story points" contains "story" which fires DOCUMENT — a false-positive in intent
  terms, but correct in lexicon-match terms. The capability gap (backlog, estimation,
  iteration) is logged as a warning but does not block routing.

---

## Pair B36B1-WDA-kedge-003 — velocity and retrospective: pure gap

```
source_id: xiigen-b36-kedge-003
source_title: XIIGen B36 knowledge-edge — velocity and retrospective vocabulary
pair_kind: knowledge_edge
```

A:
  DocumentId: "B36-kedge-003-prompt"
  CompiledText: "We need to review our team velocity and run the sprint retrospective before planning the next cycle."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

Predicate analysis:
  "velocity" — not in any predicate lexicon
  "sprint" — not in any predicate lexicon
  "retrospective" — not in any predicate lexicon
  "review" — in QA lexicon (review) → QA fires!
  "cycle" — not in any predicate lexicon

  Correction: "review" matches QA lexicon.
  CompiledPredicateIds: ["QA"] — QA fires on "review"

  This pair teaches: "review" fires QA even in a project management context. The
  QA predicate routes to Phase 6 review sub-phases (6.1-6.4). The intent is a
  project management ceremony, but the routing goes to QA. Capability gap warning
  is emitted for the project planning vocabulary.

Corrected B:
  DocumentId: "B36-kedge-003-prompt"
  CompiledText: "We need to review our team velocity and run the sprint retrospective before planning the next cycle."
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
  KnownFailureMode: null
  CapabilityGapWarnings: ["gap-project-planning-velocity-retrospective: 'velocity', 'retrospective', 'sprint' have no registered predicate"]
