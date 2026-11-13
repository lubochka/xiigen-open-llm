# Layer 0-A — Compiler Pairs (014)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: compiler

---

## B01-0A-014-001 — NotificationCenter screen inventory

```
source_id: xiigen-b01-0a-014-001
source_title: XIIGen B01 compiler-014 — NotificationCenter screen inventory
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
  run_id: "B01-0a-014-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Map all screens in the notification center, including the full notification list page, detail view, and empty state."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-014-001-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/014-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "NotificationCenter screens — list, detail, empty state"
  CompiledText: "Map all screens in the notification center, including the full notification list page, detail view, and empty state."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "screens", "page"
  "notification center", "list", "detail view", "empty state" — context nouns; no predicate.
  Confidence: 0.92

---

## B01-0A-014-002 — AlertBanner component variants

```
source_id: xiigen-b01-0a-014-002
source_title: XIIGen B01 compiler-014 — AlertBanner component variants and spacing
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
  run_id: "B01-0a-014-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Inventory all AlertBanner component variants — info, warning, error, success — including color, spacing, and icon placement."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-014-002-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/014-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "AlertBanner variants — info, warning, error, success; color, spacing, icon"
  CompiledText: "Inventory all AlertBanner component variants — info, warning, error, success — including color, spacing, and icon placement."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "component", "color", "spacing"
  "AlertBanner", "variants", "info", "warning", "error", "success", "icon placement" — context.
  Confidence: 0.92

---

## B01-0A-014-003 — NotificationCenter wireframe and interaction patterns

```
source_id: xiigen-b01-0a-014-003
source_title: XIIGen B01 compiler-014 — NotificationCenter wireframe and interactions
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
  run_id: "B01-0a-014-003"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Produce a wireframe for the notification center showing interaction states: unread, read, dismissed, and bulk-action mode."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-014-003-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/014-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "NotificationCenter wireframe — unread, read, dismissed, bulk-action states"
  CompiledText: "Produce a wireframe for the notification center showing interaction states: unread, read, dismissed, and bulk-action mode."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "wireframe", "interaction"
  "notification center", "unread", "read", "dismissed", "bulk-action" — context nouns.
  Confidence: 0.92

---

## B01-0A-014-004 — AlertBanner accessibility specification

```
source_id: xiigen-b01-0a-014-004
source_title: XIIGen B01 compiler-014 — AlertBanner accessibility specification
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
  run_id: "B01-0a-014-004"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Define the accessibility specification for the AlertBanner: ARIA (Accessible Rich Internet Applications) roles, focus behavior, and color contrast requirements."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-014-004-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/014-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "AlertBanner accessibility — ARIA roles, focus, color contrast"
  CompiledText: "Define the accessibility specification for the AlertBanner: ARIA roles, focus behavior, and color contrast requirements."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "accessibility", "color"
  DOCUMENT fires on: "define", "specification", "requirements"
  Both predicates present.
  Confidence: 0.92

---

## B01-0A-014-005 — NotificationCenter design-system tokens

```
source_id: xiigen-b01-0a-014-005
source_title: XIIGen B01 compiler-014 — NotificationCenter design-system token audit
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
  run_id: "B01-0a-014-005"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "List every design-system token used in the notification center screens and alert banner components."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-014-005-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/014-005"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "NotificationCenter and AlertBanner design-system token inventory"
  CompiledText: "List every design-system token used in the notification center screens and alert banner components."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "design-system", "screens", "components"
  "token", "notification center", "alert banner" — context nouns.
  Confidence: 0.92
