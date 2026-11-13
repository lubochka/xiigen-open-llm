# Layer 0-A — Compiler Pairs
# Batch: B36-batch-001
# Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Contract: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: compiler

---

## Method note

B36 is a CAPABILITY GAP batch. The project planning vocabulary (sprint,
roadmap, milestone, backlog, velocity, standup, retrospective) has NO
matching terms in any of the nine current predicate lexicons. The compiler
produces CompiledPredicateIds: [] for these requests.

This batch teaches the model to correctly produce zero-predicate compiler
outputs when project planning vocabulary is used without any development
signal terms. It also teaches the boundary where project planning vocabulary
co-occurs with development vocabulary (mixed signal).

Gap vocabulary with NO current predicate:
  sprint, roadmap, milestone, backlog, velocity, standup, retrospective,
  burndown, capacity, iteration, epic, ticket, story-point

---

## Pair B36B1-WDA-compiler-001 — pure project planning: zero predicates

```
source_id: xiigen-b36-compiler-001
source_title: XIIGen B36 compiler — pure project planning, no predicate
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: unknown
phase: null
sub_phase: null
task_type: project-planning-gap
coverage_status: knowledge_edge
pair_kind: compiler
```

A:
  run_id: "B36-compiler-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "We need to plan the next sprint and update the roadmap with the new milestones for Q3."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B36-compiler-001-prompt"
  SourcePath: "wave-2/B36-batch-001/compiler-input/001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Sprint planning and Q3 roadmap milestone update"
  CompiledText: "We need to plan the next sprint and update the roadmap with the new milestones for Q3."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

Predicate evidence:
  "sprint" — not in any predicate lexicon
  "roadmap" — not in any predicate lexicon
  "milestones" — not in any predicate lexicon
  "Q3" — not in any predicate lexicon
  "plan" — not in DOCUMENT lexicon (which requires: document, spec, specification,
    requirement, story, criteria, describe, define, functional, behavior, use-case...)
  "update" — not in IMPLEMENT lexicon (which requires: implement, code, build,
    develop, write, create, function, class, logic...)
  Result: CompiledPredicateIds: [], Confidence: 0.70, SourceStatus: knowledge_edge
  Capability gap ID: gap-project-planning-sprint-roadmap-001

---

## Pair B36B1-WDA-compiler-002 — roadmap with stakeholder: DOCUMENT fires partially

```
source_id: xiigen-b36-compiler-002
source_title: XIIGen B36 compiler — roadmap with stakeholder requirements signal
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: unknown
phase: null
sub_phase: null
task_type: project-planning-gap
coverage_status: mixed_signal
pair_kind: compiler
```

A:
  run_id: "B36-compiler-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Define the quarterly roadmap and document the stakeholder requirements for each milestone in the product delivery plan."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B36-compiler-002-prompt"
  SourcePath: "wave-2/B36-batch-001/compiler-input/002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Quarterly roadmap and stakeholder requirements per milestone"
  CompiledText: "Define the quarterly roadmap and document the stakeholder requirements for each milestone in the product delivery plan."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  DOCUMENT fires on: "define" (→ define), "document" (→ document),
    "stakeholder" (→ stakeholder), "requirements" (→ requirement)
  "roadmap" — NOT in any predicate lexicon; does not fire DOCUMENT
  "milestone" — NOT in any predicate lexicon
  "quarterly" — NOT in any predicate lexicon
  Mixed signal: project planning vocabulary present (roadmap, milestone) but
  DOCUMENT fires on the co-occurring development vocabulary (define, document,
  stakeholder, requirements). Compiler correctly produces DOCUMENT.
  The router will send this to 2.1/2.4 — the project planning intent is handled
  via the DOCUMENT routing, even though roadmap/milestone are not captured.
  Capability gap partially present: roadmap and milestone terms are capability gaps
  that could enrich the predicate set if the lexicon were extended.
  Confidence: 0.92

---

## Pair B36B1-WDA-compiler-003 — sprint planning with feature build: IMPLEMENT fires

```
source_id: xiigen-b36-compiler-003
source_title: XIIGen B36 compiler — sprint planning with implementation signal
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
phase: 4
sub_phase: 4.1
task_type: project-planning-gap
coverage_status: mixed_signal
pair_kind: compiler
```

A:
  run_id: "B36-compiler-003"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Plan the sprint to implement the checkout feature and build the payment integration module."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B36-compiler-003-prompt"
  SourcePath: "wave-2/B36-batch-001/compiler-input/003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Sprint plan for checkout feature implementation and payment integration"
  CompiledText: "Plan the sprint to implement the checkout feature and build the payment integration module."
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  IMPLEMENT fires on: "implement" (→ implement), "build" (→ build), "feature" (→ feature)
  "sprint" — NOT in any predicate lexicon
  "plan" — NOT in any predicate lexicon
  "integration module" — "integration" is not in IMPLEMENT lexicon directly
    (API_CONTRACT fires on "integration" only if "api" or similar co-occurs)
  Mixed signal: "sprint" and "plan" are capability gaps but "implement", "build",
  "feature" correctly fire IMPLEMENT. The compiler produces IMPLEMENT.
  The router sends to 4.1 (Backend Services and Business Logic). The project
  planning aspect of "sprint" is a capability gap and produces a warning.
  Confidence: 0.92
  Note: EvidenceIds should include the capability gap ID for "sprint":
    CapabilityGapWarnings: ["gap-project-planning-sprint-not-captured:capability_gap"]
