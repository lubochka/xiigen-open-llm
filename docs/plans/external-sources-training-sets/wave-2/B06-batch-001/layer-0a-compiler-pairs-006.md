# Layer 0-A — Compiler Pairs (006)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: compiler

---

## Method note

File 006 covers the FeatureNarrativeAuthoring scenario: "narrative", "epic", "define",
"capability", "requirements" all fire DOCUMENT. "write" fires IMPLEMENT. Dual predicate.

---

## B06-0A-006-001 — Feature narrative with epic: DOCUMENT+IMPLEMENT

```
source_id: xiigen-b06-0a-006-001
source_title: XIIGen B06 compiler — feature narrative epic DOCUMENT+IMPLEMENT
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
  run_id: "B06-0a-006-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Write the feature narrative for the subscription management epic and define the capability requirements."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B06-0a-006-001-prompt"
  SourcePath: "wave-2/B06-batch-001/compiler-input/006-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Subscription management epic narrative and capability requirements"
  CompiledText: "Write the feature narrative for the subscription management epic and define the capability requirements."
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  DOCUMENT fires on: "narrative" (→ narrative), "epic" (→ epics),
    "define" (→ define), "capability" (→ capability), "requirements" (→ requirement)
  IMPLEMENT fires on: "write" (→ write)
  Five DOCUMENT terms plus one IMPLEMENT term.
  Result: ["DOCUMENT", "IMPLEMENT"]
  Confidence: 0.92

---

## B06-0A-006-002 — "narrative" alone fires DOCUMENT: Rule 13

```
source_id: xiigen-b06-0a-006-002
source_title: XIIGen B06 compiler — narrative fires DOCUMENT
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
  run_id: "B06-0a-006-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Create the product narrative for the returns feature and document the capability gaps in the current checkout flow."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B06-0a-006-002-prompt"
  SourcePath: "wave-2/B06-batch-001/compiler-input/006-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Returns feature narrative and checkout capability gap documentation"
  CompiledText: "Create the product narrative for the returns feature and document the capability gaps in the current checkout flow."
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  DOCUMENT fires on: "narrative" (→ narrative), "document" (→ document),
    "capability" (→ capability)
  IMPLEMENT fires on: "create" (→ create)
  Result: ["DOCUMENT", "IMPLEMENT"]
  Note: "feature" is in IMPLEMENT lexicon but here appears as noun referent;
  however "create" already fires IMPLEMENT so the result is dual regardless.
  Confidence: 0.92

---

## B06-0A-006-003 — "epics" fires DOCUMENT: Rule 15

```
source_id: xiigen-b06-0a-006-003
source_title: XIIGen B06 compiler — epics fires DOCUMENT
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
  run_id: "B06-0a-006-003"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Define the epics for the checkout redesign and describe the capability requirements for each epic."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B06-0a-006-003-prompt"
  SourcePath: "wave-2/B06-batch-001/compiler-input/006-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Checkout redesign epics and capability requirements"
  CompiledText: "Define the epics for the checkout redesign and describe the capability requirements for each epic."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  DOCUMENT fires on: "define" (→ define), "epics" (→ epics),
    "describe" (→ describe), "capability" (→ capability), "requirements" (→ requirement)
  No IMPLEMENT terms: "define" and "describe" are DOCUMENT-only verbs.
  Result: ["DOCUMENT"] — single predicate.
  Confidence: 0.92

---

## B06-0A-006-004 — "capability" fires DOCUMENT: Rule 16

```
source_id: xiigen-b06-0a-006-004
source_title: XIIGen B06 compiler — capability fires DOCUMENT
source_license_state: first_party
pair_kind: compiler
```

B06 Core Rule 16 teaching — "capability" is in DOCUMENT lexicon:

  "capability" fires DOCUMENT. It signals requirements-authoring work —
  the request is asking for a description of what the system can do, not
  for implementation of it.

  Context examples where "capability" fires DOCUMENT:
    "define the capability requirements"          → DOCUMENT
    "describe the system capabilities"            → DOCUMENT
    "document the capability gaps"                → DOCUMENT
    "author the capability brief"                 → DOCUMENT

  "capability" does NOT fire IMPLEMENT. Even though implementing a capability
  is coding work, the word "capability" itself is a documentation signal.

  Contrast:
    "define the capability"  → DOCUMENT only
    "build the capability"   → IMPLEMENT only (build is in IMPLEMENT lexicon)
    "build and define the capability" → IMPLEMENT + DOCUMENT (both fire)
