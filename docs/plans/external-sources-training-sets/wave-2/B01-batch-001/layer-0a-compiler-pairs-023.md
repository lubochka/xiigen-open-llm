# Layer 0-A — Compiler Pairs (023)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: compiler

---

## Method note (023)

This file demonstrates three-predicate cases: UI + DOCUMENT + IMPLEMENT.
All three predicates fire simultaneously. TransitionType is always feature_addition
when IMPLEMENT is present, regardless of the other predicates.

---

## B01-0A-023-001 — UI+DOCUMENT+IMPLEMENT: write user stories for the checkout screen

```
source_id: xiigen-b01-0a-023-001
source_title: XIIGen B01 compiler-023 — write user stories for checkout screen (UI+DOCUMENT+IMPLEMENT)
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
  run_id: "B01-0a-023-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Write user stories for the checkout screen, specifying the form interactions and button accessibility requirements."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-023-001-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/023-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Checkout screen user stories — form interactions, button accessibility"
  CompiledText: "Write user stories for the checkout screen, specifying the form interactions and button accessibility requirements."
  CompiledPredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  IMPLEMENT fires on: "write" (regardless of object noun)
  DOCUMENT fires on: "story" (plural: "stories"), "specifying" (root: spec), "requirements"
  UI fires on: "screen", "form", "interaction", "button", "accessibility"
  Three predicates. TransitionType: feature_addition (IMPLEMENT present).
  "write user stories" → DOCUMENT + IMPLEMENT (both fire as stated in predicate rules).
  Confidence: 0.92

---

## B01-0A-023-002 — UI+DOCUMENT+IMPLEMENT: implement and document the profile form

```
source_id: xiigen-b01-0a-023-002
source_title: XIIGen B01 compiler-023 — implement and document profile form (UI+DOCUMENT+IMPLEMENT)
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
  run_id: "B01-0a-023-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Implement the user profile form and document the specification for each input field's behavior and visual state."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-023-002-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/023-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "User profile form — implement + document specification for input field behavior"
  CompiledText: "Implement the user profile form and document the specification for each input field's behavior and visual state."
  CompiledPredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  IMPLEMENT fires on: "implement"
  DOCUMENT fires on: "document", "specification", "behavior"
  UI fires on: "form", "input", "visual"
  Three predicates. TransitionType: feature_addition.
  Confidence: 0.92

---

## B01-0A-023-003 — UI+DOCUMENT+IMPLEMENT: build and define the modal interaction spec

```
source_id: xiigen-b01-0a-023-003
source_title: XIIGen B01 compiler-023 — build and define modal interaction spec (UI+DOCUMENT+IMPLEMENT)
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
  run_id: "B01-0a-023-003"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Build the delete confirmation modal and define the specification for its interaction states, including accessible button labels."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-023-003-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/023-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Delete confirmation modal — build + spec for interaction states and accessible buttons"
  CompiledText: "Build the delete confirmation modal and define the specification for its interaction states, including accessible button labels."
  CompiledPredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  IMPLEMENT fires on: "build"
  DOCUMENT fires on: "define", "specification"
  UI fires on: "modal", "interaction", "accessible" (accessibility root), "button"
  Three predicates. TransitionType: feature_addition.
  Confidence: 0.92

---

## B01-0A-023-004 — UI+DOCUMENT+IMPLEMENT: create and document responsive layout requirements

```
source_id: xiigen-b01-0a-023-004
source_title: XIIGen B01 compiler-023 — create and document responsive layout requirements (UI+DOCUMENT+IMPLEMENT)
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
  run_id: "B01-0a-023-004"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Create a responsive grid layout for the search results page and document the requirements for mobile, tablet, and desktop breakpoints."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-023-004-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/023-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Search results responsive layout — create grid + document breakpoint requirements"
  CompiledText: "Create a responsive grid layout for the search results page and document the requirements for mobile, tablet, and desktop breakpoints."
  CompiledPredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  IMPLEMENT fires on: "create"
  DOCUMENT fires on: "document", "requirements"
  UI fires on: "responsive", "layout", "page"
  Three predicates. TransitionType: feature_addition.
  Confidence: 0.92

---

## B01-0A-023-005 — UI+DOCUMENT+IMPLEMENT: develop and specify the color theme system

```
source_id: xiigen-b01-0a-023-005
source_title: XIIGen B01 compiler-023 — develop and specify color theme system (UI+DOCUMENT+IMPLEMENT)
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
  run_id: "B01-0a-023-005"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Develop the color theme system for the design-system and specify the requirements for dark and light mode visual tokens."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-023-005-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/023-005"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Color theme system — develop + specify dark/light mode visual tokens"
  CompiledText: "Develop the color theme system for the design-system and specify the requirements for dark and light mode visual tokens."
  CompiledPredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  IMPLEMENT fires on: "develop"
  DOCUMENT fires on: "specify" (spec root), "requirements"
  UI fires on: "color", "theme", "design-system", "visual"
  Three predicates. TransitionType: feature_addition.
  Confidence: 0.92
