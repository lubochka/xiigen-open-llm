# Layer 0-A — Compiler Pairs (009)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: compiler

---

## Method note

File 009 covers the full four-predicate scenario: ARCHITECTURE+DOCUMENT+IMPLEMENT+UI.
"design" and "architecture" fire ARCHITECTURE; "write", "implement" fire IMPLEMENT;
"requirements", "spec", "document" fire DOCUMENT; "ui" fires UI.

---

## B06-0A-009-001 — Four predicates: ARCHITECTURE+DOCUMENT+IMPLEMENT+UI

```
source_id: xiigen-b06-0a-009-001
source_title: XIIGen B06 compiler — full four-predicate compilation
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
phase: 2
sub_phase: 2.1
task_type: requirements-and-business-rules
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B06-0a-009-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Design the architecture, write the requirements spec, implement the checkout service, and document the UI flow."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B06-0a-009-001-prompt"
  SourcePath: "wave-2/B06-batch-001/compiler-input/009-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Checkout service: full architecture, requirements, implementation, and UI flow"
  CompiledText: "Design the architecture, write the requirements spec, implement the checkout service, and document the UI flow."
  CompiledPredicateIds: ["ARCHITECTURE", "DOCUMENT", "IMPLEMENT", "UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  ARCHITECTURE fires on: "design" (→ design), "architecture" (→ architecture)
  DOCUMENT fires on: "requirements" (→ requirement), "spec" (→ specification),
    "document" (→ document)
  IMPLEMENT fires on: "write" (→ write), "implement" (→ implement)
  UI fires on: "ui" (→ ui)
  Four independent predicates all fire.
  Canonical order: ["ARCHITECTURE", "DOCUMENT", "IMPLEMENT", "UI"] (alphabetical)
  Confidence: 0.92

---

## B06-0A-009-002 — Four predicates: alternative formulation

```
source_id: xiigen-b06-0a-009-002
source_title: XIIGen B06 compiler — four predicates alternate formulation
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
phase: 2
sub_phase: 2.1
task_type: requirements-and-business-rules
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B06-0a-009-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Define the business requirements, design the system architecture, build the backend checkout service, and specify the screen layout."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B06-0a-009-002-prompt"
  SourcePath: "wave-2/B06-batch-001/compiler-input/009-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Business requirements, architecture, backend, and screen layout for checkout"
  CompiledText: "Define the business requirements, design the system architecture, build the backend checkout service, and specify the screen layout."
  CompiledPredicateIds: ["ARCHITECTURE", "DOCUMENT", "IMPLEMENT", "UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  DOCUMENT fires on: "define" (→ define), "business" (→ business),
    "requirements" (→ requirement), "specify" (→ specification)
  ARCHITECTURE fires on: "design" (→ design), "architecture" (→ architecture)
  IMPLEMENT fires on: "build" (→ build), "backend" (→ backend)
  UI fires on: "screen" (→ screen), "layout" (→ layout)
  Confidence: 0.92

---

## B06-0A-009-003 — Four routing decisions from four predicates

```
source_id: xiigen-b06-0a-009-003
source_title: XIIGen B06 compiler — four predicates produce four routing decisions
source_license_state: first_party
pair_kind: compiler
```

Teaching note — four-predicate routing consequence:

  When compiler produces ["ARCHITECTURE", "DOCUMENT", "IMPLEMENT", "UI"]:

  Routing decisions produced by MultiSubPhaseRouterUnit:
    - ARCHITECTURE → 3.1 (Technical Architecture and System Design)
    - DOCUMENT     → 2.1 (Business Requirements and Use Cases)
    - IMPLEMENT    → 4.1 (Backend Services and Business Logic)
    - UI           → 1.1 (Screen Inventory and UX Requirements)

  TransitionType: feature_addition (IMPLEMENT present)

  Flow factory decisions:
    - RequirementsAuthoringFlow21 for 2.1
    - ArchitectureFlow31 for 3.1
    - ImplementationFlow41 for 4.1
    - ScreenInventoryFlow11 for 1.1

  Each flow executes its own nine-node chain. All four chains are
  queued and executed independently. No flow is suppressed by the presence
  of another.

---

## B06-0A-009-004 — All four predicates are independent, none suppresses another

```
source_id: xiigen-b06-0a-009-004
source_title: XIIGen B06 compiler — predicate independence rule
source_license_state: first_party
pair_kind: compiler
```

Teaching note — predicate independence:

  The four predicates ARCHITECTURE, DOCUMENT, IMPLEMENT, and UI are fully
  independent. When multiple predicates are compiled:

  1. No predicate is "primary" and no predicate "wins" over others.
  2. CompiledPredicateIds contains ALL matching predicates.
  3. Each predicate routes independently in the router.
  4. Four predicates → four routing decisions → four flow factory instantiations.

  Common wrong behaviors to avoid:
    - Selecting only DOCUMENT and discarding IMPLEMENT (v6-02 corruption)
    - Routing both DOCUMENT and IMPLEMENT to 2.1 only (v6-03 corruption)
    - Picking the "most likely" predicate and discarding others
    - Ordering predicates by "importance" rather than alphabetically

  The compiler's job is complete predicate detection.
  The router's job is independent routing per predicate.
  Suppression is never correct.
