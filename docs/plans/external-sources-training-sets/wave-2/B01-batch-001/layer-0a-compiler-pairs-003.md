# Layer 0-A — Compiler Pairs (003)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: compiler

---

## B01-0A-003-001 — UI alone: "form" + "interaction" + "input" fire

```
source_id: xiigen-b01-0a-003-001
source_title: XIIGen B01 compiler-003 — checkout form interaction mapping
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
  run_id: "B01-0a-003-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Map the checkout form interactions and identify each input field's validation behavior."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-003-001-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/0a-003-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Checkout form interaction and input validation mapping"
  CompiledText: "Map the checkout form interactions and identify each input field's validation behavior."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "form" (UI lexicon), "interaction" (UI lexicon — "interactions"),
    "input" (UI lexicon — "input field")
  "validation behavior" — "behavior" is in the DOCUMENT lexicon; however,
    the phrase "validation behavior" describes a form field property, not a
    standalone documentation artifact request. The DOCUMENT predicate requires
    a request intent to produce a specification or use-case document. Here the
    intent is to map (UX task), not to document. "behavior" alone is borderline;
    the absence of any other DOCUMENT lexicon term (document, spec, specification,
    requirement, story, criteria, describe, define, functional, use-case, narrative,
    feature-brief, capability, epics) means DOCUMENT does not fire.
  Final predicates: ["UI"]
  Confidence: 0.92

---

## B01-0A-003-002 — CheckoutInterfaceFlow: form + interaction + screen = UI

```
source_id: xiigen-b01-0a-003-002
source_title: XIIGen B01 compiler-003 — checkout interface UI flow
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
  run_id: "B01-0a-003-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Create the checkout screen flow showing each form step, button states, and input field interactions from cart review to order confirmation."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-003-002-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/0a-003-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Checkout screen flow with form steps, button states, and input interactions"
  CompiledText: "Create the checkout screen flow showing each form step, button states, and input field interactions from cart review to order confirmation."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "screen" (UI lexicon), "form" (UI lexicon), "button" (UI lexicon),
    "input" (UI lexicon), "interaction" (via "interactions", UI lexicon)
  "Create" — IMPLEMENT fires on "create" in some cases, but "create the checkout
    screen flow" is a UX artifact creation task, not a software implementation.
    No IMPLEMENT lexicon terms (code, build, develop, write, function, class,
    logic, library, handler, middleware, controller, backend, method, refactor)
    are present. IMPLEMENT does not fire.
  Final predicates: ["UI"]
  Confidence: 0.92

---

## B01-0A-003-003 — "behavior" borderline: DOCUMENT fires only with additional DOCUMENT terms

```
source_id: xiigen-b01-0a-003-003
source_title: XIIGen B01 compiler-003 — "behavior" borderline analysis
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
  run_id: "B01-0a-003-003"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Describe the functional behavior of each checkout form screen so developers can implement the validation rules."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-003-003-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/0a-003-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Functional behavior description for checkout form screens"
  CompiledText: "Describe the functional behavior of each checkout form screen so developers can implement the validation rules."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  UI fires on: "form" (UI lexicon), "screen" (UI lexicon)
  DOCUMENT fires on: "describe" (DOCUMENT lexicon), "functional" (DOCUMENT lexicon),
    "behavior" (DOCUMENT lexicon)
  "implement" — IMPLEMENT fires. Final predicates: ["UI", "DOCUMENT", "IMPLEMENT"]

  Correction: "implement the validation rules" contains "implement" which is in
    the IMPLEMENT lexicon. Revised final predicates: ["UI", "DOCUMENT", "IMPLEMENT"]
  Updated B:
    CompiledPredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]
  Confidence: 0.92

Rule captured: when "describe" + "functional" + "behavior" all appear, DOCUMENT
fires regardless of other predicates. "implement" in the same request adds
IMPLEMENT. The full predicate set must include all fired predicates.
