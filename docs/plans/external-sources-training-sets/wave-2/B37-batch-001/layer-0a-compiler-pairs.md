# Layer 0-A — Compiler Pairs
# Batch: B37-batch-001
# Scenario: B37 — Project managing — progress, coordination, and status reporting
# Contract: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: compiler

---

## Method note

B37 is a CAPABILITY GAP batch. The project managing vocabulary (track, coordinate,
progress, status-report, manage, blockers, dependencies, stakeholder-update,
escalation, accountability, follow-up, action-items) has NO matching terms in
any of the nine current predicate lexicons when used in a project management
context (as distinct from software development context).

Gap vocabulary with NO current predicate (in project managing context):
  track (in "track progress"), coordinate, blockers, dependencies (when used
  as "manage dependencies" not "API dependency"), stakeholder-update,
  escalation, accountability, follow-up, action-items, on-track, off-track,
  RAG-status (Red/Amber/Green), RAID-log

IMPORTANT BOUNDARIES in this batch:
  - "manage" fires nothing (not in any predicate lexicon)
  - "dependencies" fires API_CONTRACT only when co-occurring with API/contract terms
  - "progress" fires nothing in a project managing context
  - "track" fires nothing (track ≠ TEST, which requires: test, verify, assert,
    validate, coverage, spec, case, suite)
  - "status" fires nothing
  - "coordinate" fires nothing
  - "report" fires nothing (report ≠ DOCUMENT, which requires: document, spec,
    define, describe, requirement, story, criteria)

---

## Pair B37B1-WDA-compiler-001 — pure project managing: zero predicates

```
source_id: xiigen-b37-compiler-001
source_title: XIIGen B37 compiler — pure project managing, no predicate
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: unknown
phase: null
sub_phase: null
task_type: project-managing-gap
coverage_status: knowledge_edge
pair_kind: compiler
```

A:
  run_id: "B37-compiler-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Track progress across all active projects and provide a status report to stakeholders by Friday."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B37-compiler-001-prompt"
  SourcePath: "wave-2/B37-batch-001/compiler-input/001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Project progress tracking and stakeholder status report"
  CompiledText: "Track progress across all active projects and provide a status report to stakeholders by Friday."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

Predicate evidence:
  "track" — not in any predicate lexicon
  "progress" — not in any predicate lexicon
  "active projects" — not in any predicate lexicon
  "provide" — not in any predicate lexicon
  "status report" — "status" not in DOCUMENT lexicon; "report" not in DOCUMENT lexicon
  "stakeholders" — "stakeholder" IS in DOCUMENT lexicon → DOCUMENT fires!

  Correction: "stakeholders" fires DOCUMENT via "stakeholder" match.
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

  This is a boundary teaching case: "stakeholders" in a status-reporting context
  fires DOCUMENT even though the intent is project management communication, not
  requirements documentation. The compiler fires on term presence, not intent.

Corrected B:
  DocumentId: "B37-compiler-001-prompt"
  SourcePath: "wave-2/B37-batch-001/compiler-input/001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Project progress tracking and stakeholder status report"
  CompiledText: "Track progress across all active projects and provide a status report to stakeholders by Friday."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null
  CapabilityGapWarnings:
    - "gap-project-managing-track-progress-001: 'track progress' not captured; DOCUMENT fires on 'stakeholder' only"
    - "gap-project-managing-status-report-001: 'status report' intent not captured by DOCUMENT routing"

Predicate evidence (corrected):
  "stakeholders" → DOCUMENT fires via "stakeholder" match
  "track", "progress", "status report", "active projects" — all gap vocabulary
  Result: CompiledPredicateIds: ["DOCUMENT"], Confidence: 0.92, SourceStatus: ready
  Capability gap ID: gap-project-managing-track-progress-001

---

## Pair B37B1-WDA-compiler-002 — coordination request: QA false-positive boundary

```
source_id: xiigen-b37-compiler-002
source_title: XIIGen B37 compiler — pure project coordination, no predicate
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: unknown
phase: null
sub_phase: null
task_type: project-managing-gap
coverage_status: knowledge_edge
pair_kind: compiler
```

A:
  run_id: "B37-compiler-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Coordinate the cross-team dependencies and resolve the blockers preventing the release."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B37-compiler-002-prompt"
  SourcePath: "wave-2/B37-batch-001/compiler-input/002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Cross-team dependency coordination and blocker resolution"
  CompiledText: "Coordinate the cross-team dependencies and resolve the blockers preventing the release."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

Predicate evidence:
  "coordinate" — not in any predicate lexicon
  "cross-team" — not in any predicate lexicon
  "dependencies" — NOT in API_CONTRACT in this context (API_CONTRACT fires on
    dependency only when co-occurring with: api, contract, endpoint, interface,
    integration, swagger, openapi, graphql, rest. "cross-team dependencies" has
    no such co-occurrence)
  "resolve" — not in any predicate lexicon
  "blockers" — not in any predicate lexicon
  "preventing" — not in any predicate lexicon
  "release" — not in any predicate lexicon (IMPLEMENT does not own "release")
  Result: CompiledPredicateIds: [], SourceStatus: knowledge_edge
  Capability gap IDs: ["gap-project-managing-coordinate-001",
                       "gap-project-managing-blockers-001",
                       "gap-project-managing-dependencies-team-001"]

---

## Pair B37B1-WDA-compiler-003 — managing with IMPLEMENT co-signal: mixed signal

```
source_id: xiigen-b37-compiler-003
source_title: XIIGen B37 compiler — project managing with implementation co-signal
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
phase: 4
sub_phase: 4.1
task_type: project-managing-gap
coverage_status: mixed_signal
pair_kind: compiler
```

A:
  run_id: "B37-compiler-003"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Manage the delivery of the authentication module — build the service, track progress, and escalate blockers."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B37-compiler-003-prompt"
  SourcePath: "wave-2/B37-batch-001/compiler-input/003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Authentication module delivery: build, track, and escalate"
  CompiledText: "Manage the delivery of the authentication module — build the service, track progress, and escalate blockers."
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null
  CapabilityGapWarnings:
    - "gap-project-managing-track-progress-001: 'track progress' not captured"
    - "gap-project-managing-escalation-001: 'escalate blockers' not captured"
    - "gap-project-managing-manage-delivery-001: 'manage the delivery' not captured"

Predicate evidence:
  IMPLEMENT fires on: "build" (→ build), "service" (→ service/module)
  "manage" — not in any predicate lexicon
  "delivery" — not in any predicate lexicon
  "authentication module" — "module" is in IMPLEMENT lexicon → IMPLEMENT fires!
    (combined with "build" this is high confidence)
  "track" — not in any predicate lexicon
  "progress" — not in any predicate lexicon
  "escalate" — not in any predicate lexicon
  "blockers" — not in any predicate lexicon
  Result: CompiledPredicateIds: ["IMPLEMENT"], Confidence: 0.92
  Mixed signal: project managing vocabulary present (manage, track, escalate,
  blockers) but IMPLEMENT fires on "build", "service", "module". The compiler
  correctly produces IMPLEMENT. Gap warnings are emitted for the managing vocab.
