# Layer 0-A — Compiler Pairs (021)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: compiler

---

## Method note (021)

This file demonstrates DOCUMENT+UI dual-predicate cases where both lexicons fire.
The key teaching point: DOCUMENT and UI never suppress each other.
Both appear in CompiledPredicateIds whenever both lexicons match.

---

## B01-0A-021-001 — DOCUMENT+UI: define screen behavior narrative

```
source_id: xiigen-b01-0a-021-001
source_title: XIIGen B01 compiler-021 — define screen behavior narrative (DOCUMENT+UI)
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B01-0a-021-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Define the screen behavior narrative for each page in the checkout flow, describing interaction and visual feedback."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-021-001-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/021-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Checkout flow screen behavior narrative — interactions and visual feedback"
  CompiledText: "Define the screen behavior narrative for each page in the checkout flow, describing interaction and visual feedback."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "screen", "page", "interaction", "visual"
  DOCUMENT fires on: "define", "narrative", "behavior", "describing" (root: describe)
  Both predicates fire independently. Neither suppresses the other.
  TransitionType: feature_addition (UI present without IMPLEMENT; DOCUMENT also present but UI drives feature_addition)
  Confidence: 0.92

---

## B01-0A-021-002 — DOCUMENT+UI: document the form wireframe specification

```
source_id: xiigen-b01-0a-021-002
source_title: XIIGen B01 compiler-021 — document form wireframe specification (DOCUMENT+UI)
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B01-0a-021-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Document a wireframe specification for the user registration form, capturing the layout, input fields, and button states."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-021-002-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/021-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "User registration form wireframe specification — layout, inputs, button states"
  CompiledText: "Document a wireframe specification for the user registration form, capturing the layout, input fields, and button states."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "wireframe", "form", "layout", "input", "button"
  DOCUMENT fires on: "document", "specification"
  Both predicates fire. TransitionType: feature_addition (UI present).
  Confidence: 0.92

---

## B01-0A-021-003 — DOCUMENT+UI: describe modal interaction criteria

```
source_id: xiigen-b01-0a-021-003
source_title: XIIGen B01 compiler-021 — describe modal interaction criteria (DOCUMENT+UI)
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B01-0a-021-003"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Describe the acceptance criteria for the delete confirmation modal: what visual states must it show, and what interactions must be supported?"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-021-003-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/021-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Delete confirmation modal — acceptance criteria, visual states, interactions"
  CompiledText: "Describe the acceptance criteria for the delete confirmation modal: what visual states must it show, and what interactions must be supported?"
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "modal", "visual", "interaction"
  DOCUMENT fires on: "describe", "criteria"
  Both predicates fire. TransitionType: feature_addition (UI present).
  Confidence: 0.92

---

## B01-0A-021-004 — DOCUMENT+UI: use-case for accessibility compliance on screen

```
source_id: xiigen-b01-0a-021-004
source_title: XIIGen B01 compiler-021 — use-case for accessibility on screen (DOCUMENT+UI)
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B01-0a-021-004"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Write a use-case for accessibility-first design on the profile screen, specifying which components must be keyboard navigable."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-021-004-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/021-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Accessibility-first profile screen use-case — keyboard navigable components"
  CompiledText: "Write a use-case for accessibility-first design on the profile screen, specifying which components must be keyboard navigable."
  CompiledPredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "accessibility", "screen", "component"
  DOCUMENT fires on: "use-case"
  IMPLEMENT fires on: "write"
  Three predicates present. TransitionType: feature_addition (IMPLEMENT present).
  Confidence: 0.92

---

## B01-0A-021-005 — DOCUMENT+UI: feature-brief for responsive page layout

```
source_id: xiigen-b01-0a-021-005
source_title: XIIGen B01 compiler-021 — feature-brief for responsive page layout (DOCUMENT+UI)
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B01-0a-021-005"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Produce a feature-brief for the responsive layout system covering all onboarding pages, specifying spacing and color tokens per breakpoint."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-021-005-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/021-005"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Onboarding responsive layout system feature-brief — spacing and color tokens per breakpoint"
  CompiledText: "Produce a feature-brief for the responsive layout system covering all onboarding pages, specifying spacing and color tokens per breakpoint."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "responsive", "layout", "pages", "spacing", "color"
  DOCUMENT fires on: "feature-brief", "specifying" (root: spec)
  Both predicates present. TransitionType: feature_addition (UI without IMPLEMENT → feature_addition).
  Confidence: 0.92
