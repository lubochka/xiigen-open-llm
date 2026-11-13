# Layer 0-A — Compiler Pairs (004)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: compiler

---

## Method note

File 004 covers the DOCUMENT + ARCHITECTURE triple: "design" fires ARCHITECTURE,
"specification" fires DOCUMENT, "write" fires IMPLEMENT. Three predicates from one request.

---

## B06-0A-004-001 — Three-predicate: ARCHITECTURE+DOCUMENT+IMPLEMENT

```
source_id: xiigen-b06-0a-004-001
source_title: XIIGen B06 compiler — architecture + document + implement triple
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
  run_id: "B06-0a-004-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Design the technical architecture for the checkout service and write the functional specification."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B06-0a-004-001-prompt"
  SourcePath: "wave-2/B06-batch-001/compiler-input/004-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Checkout service architecture design and functional specification"
  CompiledText: "Design the technical architecture for the checkout service and write the functional specification."
  CompiledPredicateIds: ["ARCHITECTURE", "DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  ARCHITECTURE fires on: "design" (→ design), "architecture" (→ architecture)
  DOCUMENT fires on: "functional" (→ functional), "specification" (→ specification)
  IMPLEMENT fires on: "write" (→ write)
  Three independent predicates compile from one request.
  The order in CompiledPredicateIds is canonical: alphabetical — ARCHITECTURE, DOCUMENT, IMPLEMENT.
  Confidence: 0.92

---

## B06-0A-004-002 — ARCHITECTURE+DOCUMENT without IMPLEMENT

```
source_id: xiigen-b06-0a-004-002
source_title: XIIGen B06 compiler — architecture + document, no implement
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
phase: 2
sub_phase: 2.1
task_type: requirements-and-business-rules
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B06-0a-004-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Design the architecture for the checkout service and document the functional requirements."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B06-0a-004-002-prompt"
  SourcePath: "wave-2/B06-batch-001/compiler-input/004-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Checkout service architecture and functional requirements"
  CompiledText: "Design the architecture for the checkout service and document the functional requirements."
  CompiledPredicateIds: ["ARCHITECTURE", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  ARCHITECTURE fires on: "design" (→ design), "architecture" (→ architecture)
  DOCUMENT fires on: "document" (→ document), "functional" (→ functional),
    "requirements" (→ requirement)
  IMPLEMENT does NOT fire: "document" is in DOCUMENT lexicon, not IMPLEMENT.
  "design" is in ARCHITECTURE lexicon only.
  Result: ["ARCHITECTURE", "DOCUMENT"] — no IMPLEMENT.
  Contrast with pair 004-001: "write" was replaced by "document" → IMPLEMENT drops out.
  Confidence: 0.92

---

## B06-0A-004-003 — ARCHITECTURE fires: "design" and "architecture" as separate terms

```
source_id: xiigen-b06-0a-004-003
source_title: XIIGen B06 compiler — design and architecture lexicon terms
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
  run_id: "B06-0a-004-003"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Design the system architecture for checkout, create the component diagram, and specify the service interfaces."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B06-0a-004-003-prompt"
  SourcePath: "wave-2/B06-batch-001/compiler-input/004-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Checkout system architecture, component diagram, service interfaces"
  CompiledText: "Design the system architecture for checkout, create the component diagram, and specify the service interfaces."
  CompiledPredicateIds: ["ARCHITECTURE", "DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  ARCHITECTURE fires on: "design" (→ design), "architecture" (→ architecture),
    "diagram" (→ diagram), "component" (→ component)
  DOCUMENT fires on: "specify" (→ spec/specification)
  IMPLEMENT fires on: "create" (→ create)
  Result: ["ARCHITECTURE", "DOCUMENT", "IMPLEMENT"]
  Confidence: 0.92

---

## B06-0A-004-004 — Multi-predicate routing: three routing decisions preview

```
source_id: xiigen-b06-0a-004-004
source_title: XIIGen B06 compiler — three predicates produce three routing decisions
source_license_state: first_party
pair_kind: compiler
```

Teaching note — multi-predicate routing consequence:

  When compiler produces ["ARCHITECTURE", "DOCUMENT", "IMPLEMENT"], the router
  produces three independent routing decisions:
    - ARCHITECTURE → 3.1 (Technical Architecture and System Design)
    - DOCUMENT     → 2.1 (Business Requirements and Use Cases)
    - IMPLEMENT    → 4.1 (Backend Services and Business Logic)

  Each routing decision is independent. No predicate suppresses another.
  The TransitionOrchestratorUnit then assigns TransitionType: feature_addition
  because IMPLEMENT is present (implementation work is being requested).

  Note: DOCUMENT alone → documentation TransitionType.
  DOCUMENT + IMPLEMENT → feature_addition TransitionType.
  DOCUMENT + ARCHITECTURE (no IMPLEMENT) → documentation TransitionType.
  DOCUMENT + ARCHITECTURE + IMPLEMENT → feature_addition TransitionType.
