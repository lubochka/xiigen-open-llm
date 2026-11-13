# Layer 0-A — Compiler Pairs (015)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: compiler

---

## B01-0A-015-001 — SettingsDashboard screen map

```
source_id: xiigen-b01-0a-015-001
source_title: XIIGen B01 compiler-015 — SettingsDashboard screen map
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
  run_id: "B01-0a-015-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Map all screens in the settings dashboard — account, security, billing, integrations, and notification preference pages."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-015-001-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/015-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "SettingsDashboard screen inventory — account, security, billing, integrations, notifications"
  CompiledText: "Map all screens in the settings dashboard — account, security, billing, integrations, and notification preference pages."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "screens", "pages"
  "settings dashboard", "account", "security", "billing", "integrations", "notification" — context.
  Confidence: 0.92

---

## B01-0A-015-002 — SettingsDashboard layout and spacing

```
source_id: xiigen-b01-0a-015-002
source_title: XIIGen B01 compiler-015 — SettingsDashboard layout and spacing tokens
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
  run_id: "B01-0a-015-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Inventory the layout structure and spacing tokens used across all settings dashboard sections."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-015-002-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/015-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "SettingsDashboard layout structure and spacing tokens"
  CompiledText: "Inventory the layout structure and spacing tokens used across all settings dashboard sections."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "layout", "spacing"
  "structure", "tokens", "settings dashboard", "sections" — context nouns.
  Confidence: 0.92

---

## B01-0A-015-003 — SettingsDashboard form input patterns

```
source_id: xiigen-b01-0a-015-003
source_title: XIIGen B01 compiler-015 — SettingsDashboard form input patterns
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
  run_id: "B01-0a-015-003"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Catalogue the form input patterns used in the settings screens: text fields, toggles, dropdowns, and save/cancel button interactions."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-015-003-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/015-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "SettingsDashboard form patterns — text fields, toggles, dropdowns, buttons"
  CompiledText: "Catalogue the form input patterns used in the settings screens: text fields, toggles, dropdowns, and save/cancel button interactions."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "form", "input", "screens", "button", "interaction"
  "toggles", "dropdowns", "save", "cancel", "text fields" — context nouns.
  Confidence: 0.92

---

## B01-0A-015-004 — SettingsDashboard accessibility compliance

```
source_id: xiigen-b01-0a-015-004
source_title: XIIGen B01 compiler-015 — SettingsDashboard accessibility and WCAG
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
  run_id: "B01-0a-015-004"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Document the accessibility compliance requirements for the settings dashboard, specifying WCAG (Web Content Accessibility Guidelines) 2.1 AA criteria for each screen."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-015-004-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/015-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "SettingsDashboard WCAG 2.1 AA accessibility compliance per screen"
  CompiledText: "Document the accessibility compliance requirements for the settings dashboard, specifying WCAG 2.1 AA criteria for each screen."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "accessibility", "screen"
  DOCUMENT fires on: "document", "requirements", "criteria"
  Both predicates present.
  Confidence: 0.92

---

## B01-0A-015-005 — SettingsDashboard visual theme application

```
source_id: xiigen-b01-0a-015-005
source_title: XIIGen B01 compiler-015 — SettingsDashboard visual theme and color
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
  run_id: "B01-0a-015-005"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Inventory the visual theme and color palette applied across the settings dashboard screens."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-015-005-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/015-005"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "SettingsDashboard visual theme and color palette"
  CompiledText: "Inventory the visual theme and color palette applied across the settings dashboard screens."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "visual", "theme", "color", "screens"
  "palette", "settings dashboard" — context nouns.
  Confidence: 0.92
