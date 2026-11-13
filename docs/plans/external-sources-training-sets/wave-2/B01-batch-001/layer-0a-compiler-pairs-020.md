# Layer 0-A — Compiler Pairs (020)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: compiler

---

## B01-0A-020-001 — AddressBook screen inventory

```
source_id: xiigen-b01-0a-020-001
source_title: XIIGen B01 compiler-020 — AddressBook screen inventory
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
  run_id: "B01-0a-020-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Map all screens in the address book — address list page, add new address form, edit address form, and delete confirmation modal."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-020-001-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/020-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "AddressBook screens — list, add form, edit form, delete modal"
  CompiledText: "Map all screens in the address book — address list page, add new address form, edit address form, and delete confirmation modal."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "screens", "page", "form", "modal"
  "address book", "address list", "add new", "edit", "delete confirmation" — context nouns.
  Confidence: 0.92

---

## B01-0A-020-002 — DeliveryOptions screen and interaction

```
source_id: xiigen-b01-0a-020-002
source_title: XIIGen B01 compiler-020 — DeliveryOptions screen and interaction patterns
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
  run_id: "B01-0a-020-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Inventory the delivery options selection screen — radio input controls, speed tiers, pricing display, and confirm button interaction."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-020-002-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/020-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "DeliveryOptions screen — radio inputs, speed tiers, pricing, confirm button"
  CompiledText: "Inventory the delivery options selection screen — radio input controls, speed tiers, pricing display, and confirm button interaction."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "screen", "input", "button", "interaction"
  "delivery options", "radio", "speed tiers", "pricing", "confirm" — context nouns.
  Confidence: 0.92

---

## B01-0A-020-003 — AddressBook form input patterns

```
source_id: xiigen-b01-0a-020-003
source_title: XIIGen B01 compiler-020 — AddressBook form input patterns and validation
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
  run_id: "B01-0a-020-003"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Catalogue all form input patterns in the address book — text fields, country dropdown, and validation error states."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-020-003-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/020-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "AddressBook form inputs — text fields, country dropdown, validation errors"
  CompiledText: "Catalogue all form input patterns in the address book — text fields, country dropdown, and validation error states."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "form", "input"
  "address book", "text fields", "country dropdown", "validation error" — context nouns.
  Confidence: 0.92

---

## B01-0A-020-004 — DeliveryOptions responsive layout

```
source_id: xiigen-b01-0a-020-004
source_title: XIIGen B01 compiler-020 — DeliveryOptions responsive layout
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
  run_id: "B01-0a-020-004"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Define the responsive layout for the delivery options screen — stacked cards on mobile, side-by-side grid on desktop."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-020-004-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/020-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "DeliveryOptions responsive layout — stacked mobile, grid desktop"
  CompiledText: "Define the responsive layout for the delivery options screen — stacked cards on mobile, side-by-side grid on desktop."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "responsive", "layout", "screen"
  DOCUMENT fires on: "define"
  Both predicates present.
  Confidence: 0.92

---

## B01-0A-020-005 — AddressBook and DeliveryOptions design-system compliance

```
source_id: xiigen-b01-0a-020-005
source_title: XIIGen B01 compiler-020 — AddressBook and DeliveryOptions design-system compliance
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
  run_id: "B01-0a-020-005"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "List the design-system components and theme tokens used across the address book and delivery options screens."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-020-005-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/020-005"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "AddressBook and DeliveryOptions design-system components and theme tokens"
  CompiledText: "List the design-system components and theme tokens used across the address book and delivery options screens."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "design-system", "component", "theme", "screens"
  "address book", "delivery options", "tokens" — context nouns.
  Confidence: 0.92
