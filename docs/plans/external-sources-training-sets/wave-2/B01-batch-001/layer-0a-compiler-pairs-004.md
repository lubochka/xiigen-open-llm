# Layer 0-A — Compiler Pairs (004)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: compiler

---

## B01-0A-004-001 — UI + DOCUMENT: "document" + "page" + "specification"

```
source_id: xiigen-b01-0a-004-001
source_title: XIIGen B01 compiler-004 — checkout UI flow documentation and page specification
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
  run_id: "B01-0a-004-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Document the complete user interface flows for the checkout process and produce a page specification."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-004-001-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/0a-004-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Checkout user interface flow documentation and page specification"
  CompiledText: "Document the complete user interface flows for the checkout process and produce a page specification."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "interface" (UI lexicon), "page" (UI lexicon)
  DOCUMENT fires on: "document" (DOCUMENT lexicon), "specification" (DOCUMENT lexicon
    — via "page specification")
  The two predicates fire independently. Their evidence tokens do not overlap.
  Neither suppresses the other. Both must appear in CompiledPredicateIds.
  Confidence: 0.92

---

## B01-0A-004-002 — UI + DOCUMENT: "form" + "specification" variant

```
source_id: xiigen-b01-0a-004-002
source_title: XIIGen B01 compiler-004 — checkout form UI and specification variant
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
  run_id: "B01-0a-004-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Write a specification for the checkout screen form layout including all input fields and their visual states."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-004-002-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/0a-004-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Checkout screen form layout specification with input states"
  CompiledText: "Write a specification for the checkout screen form layout including all input fields and their visual states."
  CompiledPredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "screen" (UI lexicon), "form" (UI lexicon), "layout" (UI lexicon),
    "input" (UI lexicon), "visual" (UI lexicon)
  DOCUMENT fires on: "specification" (DOCUMENT lexicon)
  IMPLEMENT fires on: "write" (IMPLEMENT lexicon — "write a specification")
  Note on "write": the IMPLEMENT lexicon includes "write" regardless of context.
    "Write the user stories" fires DOCUMENT on "story" and IMPLEMENT on "write".
    "Write a specification" fires DOCUMENT on "specification" and IMPLEMENT on "write".
    This is the dual-predicate rule: the IMPLEMENT fire from "write" does not
    suppress or cancel the DOCUMENT fire from "specification".
  Final predicates: ["UI", "DOCUMENT", "IMPLEMENT"]
  Confidence: 0.92

---

## B01-0A-004-003 — UI + DOCUMENT: routing to both 1.1 and 2.1 confirmed

```
source_id: xiigen-b01-0a-004-003
source_title: XIIGen B01 compiler-004 — dual routing confirmation for UI+DOCUMENT
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
  run_id: "B01-0a-004-003"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Map the user dashboard interface screens and define the requirements for each screen's behavior."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-004-003-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/0a-004-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "User dashboard screen inventory and behavior requirements"
  CompiledText: "Map the user dashboard interface screens and define the requirements for each screen's behavior."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "interface" (UI lexicon), "screen" (UI lexicon — "screens")
  DOCUMENT fires on: "define" (DOCUMENT lexicon), "requirement" (DOCUMENT lexicon
    — via "requirements"), "behavior" (DOCUMENT lexicon)
  Routing consequence: two routing decisions will follow (1.1 for UI, 2.1 for
    DOCUMENT). The compiler produces both predicates; the router will split them.
  Confidence: 0.92
