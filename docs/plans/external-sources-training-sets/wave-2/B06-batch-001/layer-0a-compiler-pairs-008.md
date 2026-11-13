# Layer 0-A — Compiler Pairs (008)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: compiler

---

## Method note

File 008 covers pure DOCUMENT: spec/behavior terms. "specify"→spec, "behavior",
"describe", "use cases" all fire DOCUMENT. No other predicates present.

---

## B06-0A-008-001 — Pure DOCUMENT: behavior and use-case terms

```
source_id: xiigen-b06-0a-008-001
source_title: XIIGen B06 compiler — behavior and use-case pure DOCUMENT
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
  run_id: "B06-0a-008-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Specify the behavior of the order confirmation system and describe the notification use cases."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B06-0a-008-001-prompt"
  SourcePath: "wave-2/B06-batch-001/compiler-input/008-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Order confirmation behavior and notification use cases"
  CompiledText: "Specify the behavior of the order confirmation system and describe the notification use cases."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  DOCUMENT fires on: "specify" (→ spec/specification), "behavior" (→ behavior),
    "describe" (→ describe), "use cases" (→ use-case)
  No IMPLEMENT, ARCHITECTURE, or UI terms present.
  Result: ["DOCUMENT"] — pure DOCUMENT, single predicate.
  Confidence: 0.92

---

## B06-0A-008-002 — Pure DOCUMENT: "behavior" alone fires DOCUMENT

```
source_id: xiigen-b06-0a-008-002
source_title: XIIGen B06 compiler — behavior alone fires DOCUMENT
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
  run_id: "B06-0a-008-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Describe the expected behavior of the cart when an item goes out of stock during checkout."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B06-0a-008-002-prompt"
  SourcePath: "wave-2/B06-batch-001/compiler-input/008-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Cart behavior during out-of-stock checkout"
  CompiledText: "Describe the expected behavior of the cart when an item goes out of stock during checkout."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  DOCUMENT fires on: "describe" (→ describe), "behavior" (→ behavior)
  No other predicate terms present.
  "expected" — not in any predicate lexicon.
  "cart" — not in any predicate lexicon.
  "out of stock" — not in any predicate lexicon.
  Result: ["DOCUMENT"]
  Teaching: "behavior" fires DOCUMENT even without explicit requirements vocabulary.
  This helps avoid the corruption where "behavior" is routed to IMPLEMENT
  (the false assumption that behavior specification is coding work).
  Confidence: 0.92

---

## B06-0A-008-003 — Pure DOCUMENT: "specify" maps to spec/specification in lexicon

```
source_id: xiigen-b06-0a-008-003
source_title: XIIGen B06 compiler — specify maps to specification
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
  run_id: "B06-0a-008-003"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Specify the functional requirements for the address validation step in the checkout flow."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B06-0a-008-003-prompt"
  SourcePath: "wave-2/B06-batch-001/compiler-input/008-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Address validation functional requirements specification"
  CompiledText: "Specify the functional requirements for the address validation step in the checkout flow."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  DOCUMENT fires on: "specify" (→ specification), "functional" (→ functional),
    "requirements" (→ requirement)
  Three DOCUMENT terms — strong signal.
  No IMPLEMENT or other predicate terms.
  Result: ["DOCUMENT"]
  Teaching: "specify" is treated as equivalent to "specification" in the DOCUMENT lexicon.
  Confidence: 0.92

---

## B06-0A-008-004 — "behavior" is NOT IMPLEMENT: common misconception

```
source_id: xiigen-b06-0a-008-004
source_title: XIIGen B06 compiler — behavior is DOCUMENT not IMPLEMENT
source_license_state: first_party
pair_kind: compiler
```

Teaching note — "behavior" belongs to DOCUMENT, not IMPLEMENT:

  Common model error: routing "describe the system behavior" to IMPLEMENT
  because behavior implies code running.

  Correct rule: "behavior" is in the DOCUMENT lexicon. It signals
  requirements-authoring work — specifying expected system behavior as a
  requirements artifact, not implementing it.

  The model must not route DOCUMENT-behavior requests to 4.1 (Backend Services)
  unless an explicit IMPLEMENT verb is also present ("build", "implement", "write",
  "create", "develop", "code").

  Test case:
    "describe the behavior"      → DOCUMENT only   → routes to 2.1
    "implement the behavior"     → IMPLEMENT only   → routes to 4.1
    "describe and implement..."  → DOCUMENT+IMPLEMENT → routes to 2.1 + 4.1
    "specify the behavior"       → DOCUMENT only   → routes to 2.1
