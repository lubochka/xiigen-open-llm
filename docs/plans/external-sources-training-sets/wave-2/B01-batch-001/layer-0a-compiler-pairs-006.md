# Layer 0-A — Compiler Pairs (006)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: compiler

---

## B01-0A-006-001 — UI + DOCUMENT + IMPLEMENT: "write" + "screen" + "specification"

```
source_id: xiigen-b01-0a-006-001
source_title: XIIGen B01 compiler-006 — screen specifications + responsive implementation
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
  run_id: "B01-0a-006-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Write the screen specifications for the user dashboard and implement the responsive layout components."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-006-001-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/0a-006-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "User dashboard screen specifications and responsive layout component implementation"
  CompiledText: "Write the screen specifications for the user dashboard and implement the responsive layout components."
  CompiledPredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "screen" (UI lexicon), "responsive" (UI lexicon), "layout" (UI lexicon),
    "component" (UI lexicon — "components")
  DOCUMENT fires on: "specification" (DOCUMENT lexicon — "specifications")
  IMPLEMENT fires on: "write" (IMPLEMENT lexicon), "implement" (IMPLEMENT lexicon),
    "component" — "component" is a UI term but "implement the ... components" also
    provides IMPLEMENT evidence
  Three-predicate result: ["UI", "DOCUMENT", "IMPLEMENT"]
  TransitionType: feature_addition (IMPLEMENT present)
  Confidence: 0.92

---

## B01-0A-006-002 — "write" dual-predicate rule with DOCUMENT context

```
source_id: xiigen-b01-0a-006-002
source_title: XIIGen B01 compiler-006 — "write the user stories" dual-predicate demonstration
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
  run_id: "B01-0a-006-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Write the user stories for the dashboard screen interface and build the component library."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-006-002-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/0a-006-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Dashboard screen user stories and component library build"
  CompiledText: "Write the user stories for the dashboard screen interface and build the component library."
  CompiledPredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  DOCUMENT fires on: "story" (DOCUMENT lexicon — via "user stories")
  UI fires on: "screen" (UI lexicon), "interface" (UI lexicon), "component" (UI lexicon)
  IMPLEMENT fires on: "write" (IMPLEMENT lexicon), "build" (IMPLEMENT lexicon),
    "library" (IMPLEMENT lexicon)
  Dual-predicate rule: "write the user stories" → DOCUMENT fires on "story" AND
    IMPLEMENT fires on "write". These are independent fires; neither cancels the other.
  Confidence: 0.92

---

## B01-0A-006-003 — "create" in UX context: both UI and IMPLEMENT can fire

```
source_id: xiigen-b01-0a-006-003
source_title: XIIGen B01 compiler-006 — "create" with UI context
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
  run_id: "B01-0a-006-003"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Create an inventory of all dashboard screens and implement the missing UI components for the settings page."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-006-003-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/0a-006-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Dashboard screen inventory and settings page UI component implementation"
  CompiledText: "Create an inventory of all dashboard screens and implement the missing UI components for the settings page."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "screen" (UI lexicon — "screens"), "ui" (UI lexicon), "component"
    (UI lexicon — "components"), "page" (UI lexicon)
  IMPLEMENT fires on: "create" (IMPLEMENT lexicon), "implement" (IMPLEMENT lexicon)
  DOCUMENT: "inventory" is not in the DOCUMENT lexicon. No DOCUMENT terms present.
  "Create an inventory" → IMPLEMENT fires on "create"; the inventory task itself
    is UX work; both UI and IMPLEMENT are correct simultaneously.
  Final predicates: ["UI", "IMPLEMENT"]
  Confidence: 0.92
