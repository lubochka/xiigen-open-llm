# Layer 0-A — Compiler Pairs (013)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: compiler

---

## B01-0A-013-001 — UserProfilePage: avatar and display name fields

```
source_id: xiigen-b01-0a-013-001
source_title: XIIGen B01 compiler-013 — UserProfilePage avatar and display name
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
  run_id: "B01-0a-013-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "List all the UI components on the user profile page — avatar upload, display name input, and bio form."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-013-001-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/013-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "UserProfilePage component inventory — avatar, display name, bio"
  CompiledText: "List all the UI components on the user profile page — avatar upload, display name input, and bio form."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "UI" (direct term), "page", "component", "input", "form"
  "avatar upload", "display name", "bio" — context nouns; no predicate fires independently.
  No DOCUMENT or IMPLEMENT lexicon terms present.
  Confidence: 0.92

---

## B01-0A-013-002 — UserProfilePage: edit interaction modal

```
source_id: xiigen-b01-0a-013-002
source_title: XIIGen B01 compiler-013 — UserProfilePage edit modal interaction
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
  run_id: "B01-0a-013-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Describe the interaction pattern for the edit profile modal, including button placement and typography choices."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-013-002-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/013-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "UserProfilePage edit modal — interaction, buttons, typography"
  CompiledText: "Describe the interaction pattern for the edit profile modal, including button placement and typography choices."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "interaction", "modal", "button", "typography"
  DOCUMENT fires on: "describe"
  Both predicates are present — model must not collapse them.
  Confidence: 0.92

---

## B01-0A-013-003 — UserProfilePage: responsive layout specification

```
source_id: xiigen-b01-0a-013-003
source_title: XIIGen B01 compiler-013 — UserProfilePage responsive layout spec
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
  run_id: "B01-0a-013-003"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Define the responsive layout breakpoints for the user profile page, covering mobile, tablet, and desktop spacing."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-013-003-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/013-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "UserProfilePage responsive layout breakpoints — mobile, tablet, desktop"
  CompiledText: "Define the responsive layout breakpoints for the user profile page, covering mobile, tablet, and desktop spacing."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "responsive", "layout", "page", "spacing"
  DOCUMENT fires on: "define"
  Confidence: 0.92

---

## B01-0A-013-004 — UserProfilePage: color and theme tokens

```
source_id: xiigen-b01-0a-013-004
source_title: XIIGen B01 compiler-013 — UserProfilePage color and theme tokens
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
  run_id: "B01-0a-013-004"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Inventory the color tokens and theme settings applied to the user profile screen."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-013-004-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/013-004"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "UserProfilePage color tokens and theme settings"
  CompiledText: "Inventory the color tokens and theme settings applied to the user profile screen."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "color", "theme", "screen"
  "tokens", "settings", "profile" — context nouns; no independent predicate.
  Confidence: 0.92

---

## B01-0A-013-005 — UserProfilePage: accessibility audit

```
source_id: xiigen-b01-0a-013-005
source_title: XIIGen B01 compiler-013 — UserProfilePage accessibility audit
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
  run_id: "B01-0a-013-005"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Map the accessibility requirements for each visual element on the user profile page."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-013-005-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/013-005"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "UserProfilePage accessibility requirements per visual element"
  CompiledText: "Map the accessibility requirements for each visual element on the user profile page."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "accessibility", "visual", "page"
  "requirements" does not fire DOCUMENT — "requirement" does (singular); "requirements" also fires DOCUMENT.
  Correction: DOCUMENT fires on "requirements" (plural form of "requirement").
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  Confidence: 0.92

Predicate evidence (corrected):
  UI fires on: "accessibility", "visual", "page"
  DOCUMENT fires on: "requirements" (plural of "requirement" — predicate fires on root)
  Both predicates present.
