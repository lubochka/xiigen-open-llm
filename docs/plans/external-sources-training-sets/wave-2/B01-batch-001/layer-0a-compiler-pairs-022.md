# Layer 0-A — Compiler Pairs (022)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: compiler

---

## Method note (022)

This file demonstrates IMPLEMENT+UI cases — "build the checkout screen."
When IMPLEMENT fires alongside UI, TransitionType becomes feature_addition.
IMPLEMENT is triggered by: build, implement, create, write, develop, scaffold.

---

## B01-0A-022-001 — IMPLEMENT+UI: build the checkout screen

```
source_id: xiigen-b01-0a-022-001
source_title: XIIGen B01 compiler-022 — build the checkout screen (IMPLEMENT+UI)
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
  run_id: "B01-0a-022-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Build the checkout screen with all form inputs, a responsive layout, and the confirm payment button."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-022-001-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/022-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Build checkout screen — form inputs, responsive layout, confirm button"
  CompiledText: "Build the checkout screen with all form inputs, a responsive layout, and the confirm payment button."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  IMPLEMENT fires on: "build"
  UI fires on: "screen", "form", "input", "responsive", "layout", "button"
  TransitionType: feature_addition (IMPLEMENT present).
  Confidence: 0.92

---

## B01-0A-022-002 — IMPLEMENT+UI: create the profile page component

```
source_id: xiigen-b01-0a-022-002
source_title: XIIGen B01 compiler-022 — create the profile page component (IMPLEMENT+UI)
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
  run_id: "B01-0a-022-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Create the user profile page component with an avatar input, display name, and bio form section."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-022-002-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/022-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Create user profile page component — avatar, display name, bio form"
  CompiledText: "Create the user profile page component with an avatar input, display name, and bio form section."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  IMPLEMENT fires on: "create"
  UI fires on: "page", "component", "input", "form"
  TransitionType: feature_addition (IMPLEMENT present).
  Confidence: 0.92

---

## B01-0A-022-003 — IMPLEMENT+UI: implement the filter panel UI

```
source_id: xiigen-b01-0a-022-003
source_title: XIIGen B01 compiler-022 — implement the filter panel UI (IMPLEMENT+UI)
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
  run_id: "B01-0a-022-003"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Implement the filter panel UI — checkbox inputs, slider components, and accessible dropdown interactions."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-022-003-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/022-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Implement FilterPanel UI — checkbox inputs, sliders, accessible dropdowns"
  CompiledText: "Implement the filter panel UI — checkbox inputs, slider components, and accessible dropdown interactions."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  IMPLEMENT fires on: "implement"
  UI fires on: "UI", "input", "component", "accessibility" (via "accessible"), "interaction"
  TransitionType: feature_addition (IMPLEMENT present).
  Confidence: 0.92

---

## B01-0A-022-004 — IMPLEMENT+UI: develop the notification center screen

```
source_id: xiigen-b01-0a-022-004
source_title: XIIGen B01 compiler-022 — develop the notification center screen (IMPLEMENT+UI)
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
  run_id: "B01-0a-022-004"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Develop the notification center screen with all visual states — unread badges, color-coded alert banners, and a responsive layout."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-022-004-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/022-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Develop NotificationCenter screen — unread badges, color banners, responsive layout"
  CompiledText: "Develop the notification center screen with all visual states — unread badges, color-coded alert banners, and a responsive layout."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  IMPLEMENT fires on: "develop"
  UI fires on: "screen", "visual", "color", "responsive", "layout"
  TransitionType: feature_addition (IMPLEMENT present).
  Confidence: 0.92

---

## B01-0A-022-005 — IMPLEMENT+UI: write the wishlist component logic

```
source_id: xiigen-b01-0a-022-005
source_title: XIIGen B01 compiler-022 — write wishlist component logic (IMPLEMENT+UI)
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
  run_id: "B01-0a-022-005"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Write the wishlist component so that it renders saved items in a responsive grid with accessible button interactions."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-022-005-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/022-005"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Write wishlist component — responsive grid, accessible button interactions"
  CompiledText: "Write the wishlist component so that it renders saved items in a responsive grid with accessible button interactions."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  IMPLEMENT fires on: "write", "component" (also fires UI — component is in both)
  UI fires on: "component", "responsive", "accessible" (accessibility root), "button", "interaction"
  Note: "component" appears in UI lexicon; IMPLEMENT still fires independently via "write".
  TransitionType: feature_addition (IMPLEMENT present).
  Confidence: 0.92
