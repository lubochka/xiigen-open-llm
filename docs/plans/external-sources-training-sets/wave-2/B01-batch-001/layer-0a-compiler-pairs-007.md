# Layer 0-A — Compiler Pairs (007)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: compiler

---

## B01-0A-007-001 — UI heavy + ARCHITECTURE: "wireframe" + "prototype" + "component" + "design"

```
source_id: xiigen-b01-0a-007-001
source_title: XIIGen B01 compiler-007 — wireframe/prototype checkout design
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
  run_id: "B01-0a-007-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Design the wireframes and prototype screens for the mobile checkout experience with interactive components."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-007-001-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/0a-007-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Mobile checkout wireframe and prototype with interactive components"
  CompiledText: "Design the wireframes and prototype screens for the mobile checkout experience with interactive components."
  CompiledPredicateIds: ["UI", "ARCHITECTURE"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "wireframe" (UI lexicon), "prototype" (UI lexicon — B01 rule 4:
    "prototype" IS in the UI lexicon in B01, unlike B32 where it is gap vocabulary),
    "screen" (UI lexicon), "interaction" (via "interactive", UI lexicon),
    "component" (UI lexicon — "components")
  ARCHITECTURE fires on: "design" + structural/UI noun → ARCHITECTURE fires.
    "Design the wireframes" = "design" + structural noun → ARCHITECTURE.
    This is a B01-specific rule: "design" followed by an architectural or structural
    noun fires ARCHITECTURE even in UI context.
  IMPLEMENT: no IMPLEMENT lexicon terms present.
  Final predicates: ["UI", "ARCHITECTURE"]
  Confidence: 0.92

---

## B01-0A-007-002 — "prototype" IS in UI lexicon (B01 rule 4)

```
source_id: xiigen-b01-0a-007-002
source_title: XIIGen B01 compiler-007 — prototype UI lexicon membership
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
  run_id: "B01-0a-007-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Build an interactive prototype for the onboarding wizard screens."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-007-002-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/0a-007-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Interactive onboarding wizard screen prototype"
  CompiledText: "Build an interactive prototype for the onboarding wizard screens."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "prototype" (UI lexicon — B01 explicit rule; this is NOT gap
    vocabulary in B01 as it was in B32), "screen" (UI lexicon — "screens")
  IMPLEMENT fires on: "build" (IMPLEMENT lexicon)
  "interactive" → maps to "interaction" in UI lexicon; UI fires on this too.
  B32 contrast: In B32 (vibe coding context), "prototype" was treated as
    gap vocabulary because the vibe coding scenario had no registered prototype
    flow. In B01 (SaaS onboarding UX scenario), "prototype" is explicitly in
    the UI predicate lexicon and fires UI directly.
  Final predicates: ["UI", "IMPLEMENT"]
  Confidence: 0.92

---

## B01-0A-007-003 — wireframe-only: no ARCHITECTURE when "design" absent

```
source_id: xiigen-b01-0a-007-003
source_title: XIIGen B01 compiler-007 — wireframe without "design" keyword
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
  run_id: "B01-0a-007-003"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Create wireframes for every onboarding screen including the mobile responsive layouts."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-007-003-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/0a-007-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Onboarding screen wireframes with mobile responsive layouts"
  CompiledText: "Create wireframes for every onboarding screen including the mobile responsive layouts."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "wireframe" (UI lexicon — "wireframes"), "screen" (UI lexicon),
    "responsive" (UI lexicon), "layout" (UI lexicon — "layouts")
  IMPLEMENT fires on: "create" (IMPLEMENT lexicon)
  ARCHITECTURE: "design" is absent, so the ARCHITECTURE fire rule does not trigger.
    "wireframe" alone does not fire ARCHITECTURE — only the combination of "design"
    + structural noun triggers ARCHITECTURE.
  Final predicates: ["UI", "IMPLEMENT"]
  Confidence: 0.92
