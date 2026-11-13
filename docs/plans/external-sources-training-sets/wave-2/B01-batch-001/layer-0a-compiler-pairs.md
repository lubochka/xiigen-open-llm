# Layer 0-A — Compiler Pairs
# Batch: B01-batch-001
# Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Contract: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: compiler

---

## Method note

Layer 0-A pairs teach the compiler contract. The A side is a raw
LogicalOrchestratorRequest from a user. The B side is the compiled
SubPhaseRouterSourceDocument. The compiler identifies which predicates
fire based on the presence of lexicon terms in the user request.

Predicate lexicon relevant to this batch:
  UI:       ui, interface, ux, screen, page, modal, form, layout, component,
            responsive, accessibility, visual, wireframe, prototype, interaction,
            button, input, typography, color, spacing, design-system, theme
  DOCUMENT: document, spec, specification, requirement, story, criteria,
            describe, define, functional, behavior, use-case, narrative,
            feature-brief, capability, epics

---

## Pair B01B1-WDA-compiler-001

```
source_id: xiigen-b01-compiler-001
source_title: XIIGen B01 compiler — SaaS onboarding UX screen flow mapping
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
  run_id: "B01-compiler-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "We need to map all screen flows and create a complete screen inventory for the SaaS onboarding UX, covering signup, email activation, and the first-run experience."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-compiler-001-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "SaaS onboarding screen flow and UX mapping"
  CompiledText: "We need to map all screen flows and create a complete screen inventory for the SaaS onboarding UX, covering signup, email activation, and the first-run experience."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "screen" (×2 — "screen flows", "screen inventory"), "ux"
  No other predicate lexicon terms present.
  "onboarding", "signup", "email activation", "first-run" contain no current predicate terms.
  Confidence: 0.92 (≥1 predicate matched)

---

## Pair B01B1-WDA-compiler-002

```
source_id: xiigen-b01-compiler-002
source_title: XIIGen B01 compiler — Admin interface page inventory with navigation
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
  run_id: "B01-compiler-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Create an inventory of all pages in the admin interface, listing each page's purpose, navigation links, and interaction patterns."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-compiler-002-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Admin interface page inventory with navigation and interaction patterns"
  CompiledText: "Create an inventory of all pages in the admin interface, listing each page's purpose, navigation links, and interaction patterns."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "page", "interface", "interaction"
  "navigation links" — "navigation" is not in the UI lexicon; only the presence of "page",
  "interface", and "interaction" trigger UI.
  No DOCUMENT fires: "listing" and "purpose" are not in the DOCUMENT lexicon.
  Confidence: 0.92 (≥1 predicate matched)

---

## Pair B01B1-WDA-compiler-003

```
source_id: xiigen-b01-compiler-003
source_title: XIIGen B01 compiler — Checkout interface flow with page specification
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
  run_id: "B01-compiler-003"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Document the complete user interface flows for the checkout process and produce a page specification with form interaction details at each step."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-compiler-003-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Checkout interface flow documentation and page specification"
  CompiledText: "Document the complete user interface flows for the checkout process and produce a page specification with form interaction details at each step."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "interface", "page", "form", "interaction"
  DOCUMENT fires on: "document", "specification" (via "specification" in "page specification")
  Both predicates are present — the model must not collapse them into one.
  Confidence: 0.92 (≥1 predicate matched)
