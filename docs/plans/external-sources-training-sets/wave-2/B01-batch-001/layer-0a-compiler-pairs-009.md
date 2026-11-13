# Layer 0-A — Compiler Pairs (009)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: compiler

---

## B01-0A-009-001 — Design system: "design-system" + "typography" + "color" + "spacing"

```
source_id: xiigen-b01-0a-009-001
source_title: XIIGen B01 compiler-009 — design system documentation with token definitions
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
  run_id: "B01-0a-009-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Create the design system documentation for the onboarding UI — define the typography, color tokens, and spacing scale."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-009-001-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/0a-009-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Onboarding UI design system documentation with typography, color, and spacing"
  CompiledText: "Create the design system documentation for the onboarding UI — define the typography, color tokens, and spacing scale."
  CompiledPredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "design-system" (UI lexicon — B01 rule 13: "design-system" / "design
    system" fires UI), "ui" (UI lexicon), "typography" (UI lexicon — B01 rule 21),
    "color" (UI lexicon — B01 rule 22), "spacing" (UI lexicon — B01 rule 22)
  DOCUMENT fires on: "documentation" (DOCUMENT lexicon — "design system documentation"),
    "define" (DOCUMENT lexicon)
  IMPLEMENT fires on: "create" (IMPLEMENT lexicon)
  Final predicates: ["UI", "DOCUMENT", "IMPLEMENT"]
  Confidence: 0.92

---

## B01-0A-009-002 — "theme" fires UI

```
source_id: xiigen-b01-0a-009-002
source_title: XIIGen B01 compiler-009 — theme fires UI
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
  run_id: "B01-0a-009-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Define the dark and light theme tokens for the onboarding screens and update the color palette."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-009-002-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/0a-009-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Dark and light theme tokens for onboarding screens with color palette update"
  CompiledText: "Define the dark and light theme tokens for the onboarding screens and update the color palette."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "theme" (UI lexicon — B01 rule 14: "theme" → UI fires),
    "screen" (UI lexicon — "screens"), "color" (UI lexicon)
  DOCUMENT fires on: "define" (DOCUMENT lexicon)
  Final predicates: ["UI", "DOCUMENT"]
  Confidence: 0.92

---

## B01-0A-009-003 — Design system full token set: all five UI token terms

```
source_id: xiigen-b01-0a-009-003
source_title: XIIGen B01 compiler-009 — design system full token inventory
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
  run_id: "B01-0a-009-003"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Document the complete design system component library: typography scale, color system, spacing tokens, theme variants, and layout grid."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-009-003-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/0a-009-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Complete design system documentation: typography, color, spacing, theme, layout"
  CompiledText: "Document the complete design system component library: typography scale, color system, spacing tokens, theme variants, and layout grid."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "design-system" (UI lexicon — "design system"), "component" (UI lexicon),
    "typography" (UI lexicon), "color" (UI lexicon), "spacing" (UI lexicon),
    "theme" (UI lexicon), "layout" (UI lexicon)
  DOCUMENT fires on: "document" (DOCUMENT lexicon)
  Final predicates: ["UI", "DOCUMENT"]
  Confidence: 0.92
