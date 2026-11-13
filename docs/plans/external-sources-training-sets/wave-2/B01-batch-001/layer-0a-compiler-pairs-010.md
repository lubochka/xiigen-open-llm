# Layer 0-A — Compiler Pairs (010)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: compiler

---

## B01-0A-010-001 — Full multi-predicate: UI + DOCUMENT + IMPLEMENT + ARCHITECTURE

```
source_id: xiigen-b01-0a-010-001
source_title: XIIGen B01 compiler-010 — four-predicate full request
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
  run_id: "B01-0a-010-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Design the architecture for the onboarding screen flow, implement the form components, document the user requirements, and map all screen interactions."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-010-001-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/0a-010-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Onboarding screen flow: architecture design, form component implementation, requirements documentation, and screen interaction mapping"
  CompiledText: "Design the architecture for the onboarding screen flow, implement the form components, document the user requirements, and map all screen interactions."
  CompiledPredicateIds: ["UI", "ARCHITECTURE", "IMPLEMENT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  ARCHITECTURE fires on: "design" + "architecture" (ARCHITECTURE fires when both
    "design" and "architecture" appear, or when "design" + a structural noun appears)
  IMPLEMENT fires on: "implement" (IMPLEMENT lexicon)
  UI fires on: "screen" (UI lexicon — "screen flow", "screen interactions"),
    "form" (UI lexicon), "component" (UI lexicon — "components"),
    "interaction" (UI lexicon — "interactions")
  DOCUMENT fires on: "document" (DOCUMENT lexicon), "requirement" (DOCUMENT lexicon
    — "requirements")
  Four-predicate result: ["UI", "ARCHITECTURE", "IMPLEMENT", "DOCUMENT"]
  TransitionType: feature_addition (IMPLEMENT present)
  Confidence: 0.92

---

## B01-0A-010-002 — Four-predicate routing consequence preview

```
source_id: xiigen-b01-0a-010-002
source_title: XIIGen B01 compiler-010 — four routing decisions preview
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
  run_id: "B01-0a-010-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Build the full product listing screen architecture, implement the responsive component library, write the functional specification for each page, and define the interaction model for all UI elements."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-010-002-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/0a-010-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Product listing screen architecture, responsive component build, functional spec, and interaction model"
  CompiledText: "Build the full product listing screen architecture, implement the responsive component library, write the functional specification for each page, and define the interaction model for all UI elements."
  CompiledPredicateIds: ["UI", "ARCHITECTURE", "IMPLEMENT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "screen" (UI lexicon), "responsive" (UI lexicon), "component" (UI
    lexicon), "library" — "library" is IMPLEMENT, not UI; "page" (UI lexicon),
    "interaction" (UI lexicon), "ui" (UI lexicon)
  ARCHITECTURE fires on: "architecture" (structural term)
  IMPLEMENT fires on: "build" (IMPLEMENT lexicon), "implement" (IMPLEMENT lexicon),
    "write" (IMPLEMENT lexicon), "library" (IMPLEMENT lexicon)
  DOCUMENT fires on: "functional" (DOCUMENT lexicon), "specification" (DOCUMENT
    lexicon), "define" (DOCUMENT lexicon)
  Final predicates: ["UI", "ARCHITECTURE", "IMPLEMENT", "DOCUMENT"]
  Routing consequence: four independent routing decisions will be produced.
  Confidence: 0.92

---

## B01-0A-010-003 — Four-predicate ordering rule

```
source_id: xiigen-b01-0a-010-003
source_title: XIIGen B01 compiler-010 — canonical predicate ordering
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: compiler
```

Ordering rule (supplements pairs 001-002):

  When multiple predicates fire, CompiledPredicateIds is ordered as follows:
    1. UI (if present)
    2. ARCHITECTURE (if present)
    3. IMPLEMENT (if present)
    4. DOCUMENT (if present)

  This ordering is canonical across all B01 pairs. The router then processes
  each predicate independently regardless of order. The order affects only
  the sequence of routing decisions produced, not the routing targets.

  Example from pair 001: ["UI", "ARCHITECTURE", "IMPLEMENT", "DOCUMENT"]
    → Router produces four RoutingDecision entries:
      1. UI → 1.1 (Screen Flow and UX Definition)
      2. ARCHITECTURE → 3.1 (System Architecture)
      3. IMPLEMENT → 4.1 (Implementation)
      4. DOCUMENT → 2.1 (Business Requirements)

  The model must never collapse two predicates into a single routing decision
  (Corruption-v6-03) or suppress a predicate from the output (Corruption-v6-02).
