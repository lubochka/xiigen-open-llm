# Layer 0-A — Compiler Pairs (019)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: compiler

---

## B01-0A-019-001 — OrderHistoryPage screen inventory

```
source_id: xiigen-b01-0a-019-001
source_title: XIIGen B01 compiler-019 — OrderHistoryPage screen inventory
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
  run_id: "B01-0a-019-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Map all screens in the order history section — the order list page, individual order detail screen, and receipt download modal."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-019-001-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/019-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "OrderHistoryPage screens — list page, order detail screen, receipt modal"
  CompiledText: "Map all screens in the order history section — the order list page, individual order detail screen, and receipt download modal."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "screens", "page", "screen", "modal"
  "order history", "order list", "individual", "receipt", "download" — context nouns.
  Confidence: 0.92

---

## B01-0A-019-002 — TrackingView screen and interaction states

```
source_id: xiigen-b01-0a-019-002
source_title: XIIGen B01 compiler-019 — TrackingView screen and interaction states
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
  run_id: "B01-0a-019-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Inventory all states of the order tracking screen — processing, shipped, out for delivery, delivered, and delivery exception."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-019-002-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/019-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "TrackingView screen states — processing, shipped, out for delivery, delivered, exception"
  CompiledText: "Inventory all states of the order tracking screen — processing, shipped, out for delivery, delivered, and delivery exception."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "screen"
  "order tracking", "processing", "shipped", "out for delivery", "delivered", "exception" — context.
  Confidence: 0.92

---

## B01-0A-019-003 — OrderHistoryPage layout and typography

```
source_id: xiigen-b01-0a-019-003
source_title: XIIGen B01 compiler-019 — OrderHistoryPage layout and typography specification
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
  run_id: "B01-0a-019-003"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Specify the layout grid and typography scale for the order history page, from order summary rows to detail headers."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-019-003-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/019-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "OrderHistoryPage layout grid and typography scale"
  CompiledText: "Specify the layout grid and typography scale for the order history page, from order summary rows to detail headers."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "layout", "typography", "page"
  DOCUMENT fires on: "specify" (root: "spec" fires DOCUMENT; "specify" is recognized as spec root)
  Both predicates present.
  Confidence: 0.92

---

## B01-0A-019-004 — TrackingView visual design

```
source_id: xiigen-b01-0a-019-004
source_title: XIIGen B01 compiler-019 — TrackingView visual design and color
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
  run_id: "B01-0a-019-004"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Map the visual color coding and icon set used across all order tracking screen states."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-019-004-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/019-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "TrackingView visual color coding and icon set per state"
  CompiledText: "Map the visual color coding and icon set used across all order tracking screen states."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "visual", "color", "screen"
  "order tracking", "coding", "icon set" — context nouns.
  Confidence: 0.92

---

## B01-0A-019-005 — OrderHistoryPage accessibility

```
source_id: xiigen-b01-0a-019-005
source_title: XIIGen B01 compiler-019 — OrderHistoryPage accessibility and contrast
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
  run_id: "B01-0a-019-005"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Document the accessibility requirements for the order history page — screen reader labels and color contrast for status badges."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-019-005-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/019-005"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "OrderHistoryPage accessibility — screen reader labels, color contrast"
  CompiledText: "Document the accessibility requirements for the order history page — screen reader labels and color contrast for status badges."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "accessibility", "page", "color"
  DOCUMENT fires on: "document", "requirements"
  Both predicates present.
  Confidence: 0.92
