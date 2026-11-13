# Layer 0-A — Compiler Pairs (017)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: compiler

---

## B01-0A-017-001 — CartView screen inventory

```
source_id: xiigen-b01-0a-017-001
source_title: XIIGen B01 compiler-017 — CartView screen inventory
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
  run_id: "B01-0a-017-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Map all screens in the cart view — empty cart, cart with items, and cart with applied promo code — including the page layout for each."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-017-001-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/017-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "CartView screens — empty, with items, promo applied; page layout per state"
  CompiledText: "Map all screens in the cart view — empty cart, cart with items, and cart with applied promo code — including the page layout for each."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "screens", "page", "layout"
  "cart view", "empty cart", "items", "promo code" — context nouns.
  Confidence: 0.92

---

## B01-0A-017-002 — MiniCart component and interaction

```
source_id: xiigen-b01-0a-017-002
source_title: XIIGen B01 compiler-017 — MiniCart component and interaction states
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
  run_id: "B01-0a-017-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Inventory the MiniCart component — its collapsed icon button, expanded panel, and interactive item-removal action."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-017-002-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/017-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "MiniCart component — collapsed icon, expanded panel, item-removal interaction"
  CompiledText: "Inventory the MiniCart component — its collapsed icon button, expanded panel, and interactive item-removal action."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "component", "button", "interaction" (via "interactive")
  "MiniCart", "collapsed", "expanded", "panel", "item-removal" — context nouns.
  Confidence: 0.92

---

## B01-0A-017-003 — CartView responsive layout and spacing

```
source_id: xiigen-b01-0a-017-003
source_title: XIIGen B01 compiler-017 — CartView responsive layout and spacing
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
  run_id: "B01-0a-017-003"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Catalogue the responsive layout and spacing decisions for the CartView screen on mobile and desktop."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-017-003-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/017-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "CartView responsive layout and spacing — mobile and desktop"
  CompiledText: "Catalogue the responsive layout and spacing decisions for the CartView screen on mobile and desktop."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "responsive", "layout", "spacing", "screen"
  "CartView", "mobile", "desktop" — context nouns.
  Confidence: 0.92

---

## B01-0A-017-004 — CartView color and visual theme

```
source_id: xiigen-b01-0a-017-004
source_title: XIIGen B01 compiler-017 — CartView color and visual theme
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
  run_id: "B01-0a-017-004"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Describe the color palette and visual theme applied to the cart screen, including CTA (Call to Action) button colors and typography."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-017-004-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/017-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "CartView color palette, visual theme, CTA buttons, typography"
  CompiledText: "Describe the color palette and visual theme applied to the cart screen, including CTA button colors and typography."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "color", "visual", "screen", "button", "typography"
  DOCUMENT fires on: "describe"
  Both predicates present.
  Confidence: 0.92

---

## B01-0A-017-005 — MiniCart wireframe

```
source_id: xiigen-b01-0a-017-005
source_title: XIIGen B01 compiler-017 — MiniCart wireframe for collapsed and expanded states
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
  run_id: "B01-0a-017-005"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Produce a wireframe for the MiniCart showing the collapsed icon state and expanded panel layout."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-017-005-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/017-005"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "MiniCart wireframe — collapsed icon, expanded panel"
  CompiledText: "Produce a wireframe for the MiniCart showing the collapsed icon state and expanded panel layout."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "wireframe", "layout"
  "MiniCart", "collapsed", "icon", "expanded", "panel" — context nouns.
  Confidence: 0.92
