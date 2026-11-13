# Layer 0-A — Compiler Pairs
# Batch: B15-batch-001
# Scenario: B15 — Development from scratch — backend order processing service
# Contract: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: compiler

---

## Pair B15B1-WDA-compiler-001 — from-scratch backend service: IMPLEMENT fires

```
source_id: xiigen-b15-compiler-001
transition_type: feature_addition
phase: 4
sub_phase: 4.1
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B15-compiler-001"
  user_request: "Build an order processing service from scratch including order creation, status tracking, and fulfillment logic."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B15-compiler-001-prompt"
  SourcePath: "wave-2/B15-batch-001/compiler-input/001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Order processing service — from-scratch build with order creation, tracking, and fulfillment"
  CompiledText: "Build an order processing service from scratch including order creation, status tracking, and fulfillment logic."
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "build" → IMPLEMENT fires on "build"
  "service" → IMPLEMENT fires on "service"
  "order creation" — "create" is in IMPLEMENT lexicon → IMPLEMENT fires on "creation"
  "logic" → IMPLEMENT fires on "logic"
  "status tracking" — "tracking" not in TEST; "status" not in IMPLEMENT directly
  "fulfillment" — not in IMPLEMENT directly, but "fulfillment logic" contains "logic"
  Result: CompiledPredicateIds: ["IMPLEMENT"], Confidence: 0.92

---

## Pair B15B1-WDA-compiler-002 — from-scratch with spec: IMPLEMENT + DOCUMENT

```
source_id: xiigen-b15-compiler-002
transition_type: feature_addition
phase: 4
sub_phase: 4.1
coverage_status: mixed_signal
pair_kind: compiler
```

A:
  run_id: "B15-compiler-002"
  user_request: "Write the order service from scratch: implement the domain model, define the data contracts, and code the business rules."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B15-compiler-002-prompt"
  SourcePath: "wave-2/B15-batch-001/compiler-input/002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Order service from scratch: domain model, data contracts, business rules"
  CompiledText: "Write the order service from scratch: implement the domain model, define the data contracts, and code the business rules."
  CompiledPredicateIds: ["IMPLEMENT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "write" → IMPLEMENT fires on "write"
  "implement" → IMPLEMENT fires on "implement"
  "code" → IMPLEMENT fires on "code"
  "define" → DOCUMENT fires on "define"
  "data contracts" — "contract" is in API_CONTRACT → API_CONTRACT fires!
  "domain model" — "model" is in ARCHITECTURE → ARCHITECTURE fires!

  Correction: Multiple predicates fire.
  CompiledPredicateIds: ["IMPLEMENT", "DOCUMENT", "API_CONTRACT", "ARCHITECTURE"]

Corrected B:
  CompiledPredicateIds: ["IMPLEMENT", "DOCUMENT", "API_CONTRACT", "ARCHITECTURE"]
  SourceStatus: "ready"
  KnownFailureMode: null

Teaching point: "data contracts" fires API_CONTRACT on "contract"; "domain model"
fires ARCHITECTURE on "model". Even in a pure implementation request, these terms
add additional predicates that route to additional sub-phases.

---

## Pair B15B1-WDA-compiler-003 — IMPLEMENT dominant: pure build request

```
source_id: xiigen-b15-compiler-003
transition_type: feature_addition
phase: 4
sub_phase: 4.1
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B15-compiler-003"
  user_request: "Create the OrderRepository class and implement the CreateOrder, UpdateOrderStatus, and CancelOrder methods."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B15-compiler-003-prompt"
  SourcePath: "wave-2/B15-batch-001/compiler-input/003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "OrderRepository class implementation — CRUD operations"
  CompiledText: "Create the OrderRepository class and implement the CreateOrder, UpdateOrderStatus, and CancelOrder methods."
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "create" → IMPLEMENT fires on "create"
  "class" → IMPLEMENT fires on "class"
  "implement" → IMPLEMENT fires on "implement"
  "methods" — "method" is in IMPLEMENT lexicon → IMPLEMENT fires
  "CreateOrder", "UpdateOrderStatus", "CancelOrder" — method names; "Create", "Update"
    fire IMPLEMENT; "Order", "Status", "Cancel" do not independently fire predicates
  Result: CompiledPredicateIds: ["IMPLEMENT"], Confidence: 0.95 (high density match)
