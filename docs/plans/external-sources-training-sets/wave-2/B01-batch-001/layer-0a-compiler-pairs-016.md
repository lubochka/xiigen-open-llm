# Layer 0-A — Compiler Pairs (016)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: compiler

---

## B01-0A-016-001 — SearchResults screen inventory

```
source_id: xiigen-b01-0a-016-001
source_title: XIIGen B01 compiler-016 — SearchResults screen inventory
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
  run_id: "B01-0a-016-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Map all screens in the search results experience — keyword results page, no-results state, and paginated results layout."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-016-001-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/016-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "SearchResults screens — keyword results, no-results, paginated layout"
  CompiledText: "Map all screens in the search results experience — keyword results page, no-results state, and paginated results layout."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "screens", "page", "layout"
  "keyword results", "no-results state", "paginated" — context nouns.
  Confidence: 0.92

---

## B01-0A-016-002 — FilterPanel component structure

```
source_id: xiigen-b01-0a-016-002
source_title: XIIGen B01 compiler-016 — FilterPanel component structure and input types
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
  run_id: "B01-0a-016-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Inventory the FilterPanel component — all input types (checkboxes, sliders, dropdowns) and their interaction states."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-016-002-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/016-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "FilterPanel component — input types and interaction states"
  CompiledText: "Inventory the FilterPanel component — all input types (checkboxes, sliders, dropdowns) and their interaction states."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "component", "input", "interaction"
  "FilterPanel", "checkboxes", "sliders", "dropdowns" — context nouns.
  Confidence: 0.92

---

## B01-0A-016-003 — SearchResults and FilterPanel responsive behavior

```
source_id: xiigen-b01-0a-016-003
source_title: XIIGen B01 compiler-016 — SearchResults and FilterPanel responsive behavior
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
  run_id: "B01-0a-016-003"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Describe how the search results layout and filter panel respond at mobile and tablet breakpoints."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-016-003-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/016-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "SearchResults and FilterPanel responsive behavior — mobile, tablet breakpoints"
  CompiledText: "Describe how the search results layout and filter panel respond at mobile and tablet breakpoints."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "layout", "responsive" (implied by "breakpoints" context with "respond")
  DOCUMENT fires on: "describe"
  Note: "responsive" as a term fires UI; "breakpoints" is context.
  Both predicates present.
  Confidence: 0.92

---

## B01-0A-016-004 — SearchResults visual hierarchy and typography

```
source_id: xiigen-b01-0a-016-004
source_title: XIIGen B01 compiler-016 — SearchResults visual hierarchy and typography
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
  run_id: "B01-0a-016-004"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Map the visual hierarchy and typography scale used in the search results page, from result titles to metadata."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-016-004-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/016-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "SearchResults visual hierarchy and typography scale"
  CompiledText: "Map the visual hierarchy and typography scale used in the search results page, from result titles to metadata."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "visual", "typography", "page"
  "hierarchy", "scale", "result titles", "metadata" — context nouns.
  Confidence: 0.92

---

## B01-0A-016-005 — FilterPanel wireframe specification

```
source_id: xiigen-b01-0a-016-005
source_title: XIIGen B01 compiler-016 — FilterPanel wireframe specification
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
  run_id: "B01-0a-016-005"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Produce a wireframe specification for the FilterPanel describing collapsed and expanded states."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-016-005-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/016-005"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "FilterPanel wireframe — collapsed and expanded states"
  CompiledText: "Produce a wireframe specification for the FilterPanel describing collapsed and expanded states."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "wireframe"
  DOCUMENT fires on: "specification", "describing" (via "describe" root)
  Both predicates present.
  Confidence: 0.92
