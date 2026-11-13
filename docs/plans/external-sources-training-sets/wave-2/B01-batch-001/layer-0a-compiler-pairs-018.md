# Layer 0-A — Compiler Pairs (018)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: compiler

---

## B01-0A-018-001 — WishlistPage screen inventory

```
source_id: xiigen-b01-0a-018-001
source_title: XIIGen B01 compiler-018 — WishlistPage screen inventory
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
  run_id: "B01-0a-018-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Map all screens in the wishlist page — populated wishlist, empty wishlist, and the move-to-cart interaction modal."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-018-001-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/018-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "WishlistPage screens — populated, empty, move-to-cart modal"
  CompiledText: "Map all screens in the wishlist page — populated wishlist, empty wishlist, and the move-to-cart interaction modal."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "screens", "page", "interaction", "modal"
  "wishlist", "populated", "empty", "move-to-cart" — context nouns.
  Confidence: 0.92

---

## B01-0A-018-002 — SavedItems component inventory

```
source_id: xiigen-b01-0a-018-002
source_title: XIIGen B01 compiler-018 — SavedItems component and layout inventory
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
  run_id: "B01-0a-018-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Inventory all SavedItems UI components — item card layout, share button, remove button, and add-to-cart button."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-018-002-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/018-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "SavedItems components — card layout, share/remove/add-to-cart buttons"
  CompiledText: "Inventory all SavedItems UI components — item card layout, share button, remove button, and add-to-cart button."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "UI", "component", "layout", "button"
  "SavedItems", "item card", "share", "remove", "add-to-cart" — context nouns.
  Confidence: 0.92

---

## B01-0A-018-003 — WishlistPage visual design tokens

```
source_id: xiigen-b01-0a-018-003
source_title: XIIGen B01 compiler-018 — WishlistPage visual design tokens
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
  run_id: "B01-0a-018-003"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Catalogue the visual design tokens — color, spacing, and typography — used in the wishlist page and saved items components."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-018-003-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/018-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "WishlistPage and SavedItems visual tokens — color, spacing, typography"
  CompiledText: "Catalogue the visual design tokens — color, spacing, and typography — used in the wishlist page and saved items components."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "visual", "color", "spacing", "typography", "page", "component"
  "design tokens", "wishlist", "saved items" — context nouns.
  Confidence: 0.92

---

## B01-0A-018-004 — WishlistPage accessibility requirements

```
source_id: xiigen-b01-0a-018-004
source_title: XIIGen B01 compiler-018 — WishlistPage accessibility requirements
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
  run_id: "B01-0a-018-004"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Define the accessibility requirements for the wishlist page — ARIA labels for each button and color contrast compliance."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-018-004-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/018-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "WishlistPage accessibility — ARIA labels, button compliance, color contrast"
  CompiledText: "Define the accessibility requirements for the wishlist page — ARIA labels for each button and color contrast compliance."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "accessibility", "page", "button", "color"
  DOCUMENT fires on: "define", "requirements"
  Both predicates present.
  Confidence: 0.92

---

## B01-0A-018-005 — SavedItems responsive wireframe

```
source_id: xiigen-b01-0a-018-005
source_title: XIIGen B01 compiler-018 — SavedItems responsive wireframe
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
  run_id: "B01-0a-018-005"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Produce a responsive wireframe for the saved items screen at mobile (1 column) and desktop (3 column grid) breakpoints."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-018-005-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/018-005"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "SavedItems responsive wireframe — 1-column mobile, 3-column desktop"
  CompiledText: "Produce a responsive wireframe for the saved items screen at mobile (1 column) and desktop (3 column grid) breakpoints."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "responsive", "wireframe", "screen"
  "saved items", "mobile", "desktop", "1 column", "3 column grid", "breakpoints" — context.
  Confidence: 0.92
