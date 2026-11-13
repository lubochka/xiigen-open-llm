# Layer 0-A — Compiler Pairs
# Batch: B06-batch-001
# Scenario: B06 — Checkout flow business requirements and use cases
# Contract: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: compiler

---

## Method note

Layer 0-A pairs teach the compiler contract for functional specification
and requirements work. The DOCUMENT predicate is the primary owner of
sub-phase 2.1 (Business Requirements and Use Cases).

Predicate lexicon relevant to this batch:
  DOCUMENT:  document, spec, specification, requirement, story, criteria,
             describe, define, functional, behavior, use-case, narrative,
             feature-brief, capability, epics
  IMPLEMENT: implement, code, build, develop, write, create, function,
             class, logic, library, handler, backend, feature, method, refactor
  UI:        ui, interface, ux, screen, page, form, component, layout

Key teaching: DOCUMENT fires for requirements-authoring and specification
work. IMPLEMENT fires for coding and implementation work. Requests that ask
to "write code" differ from requests that ask to "write specifications".

---

## Pair B06B1-WDA-compiler-001

```
source_id: xiigen-b06-compiler-001
source_title: XIIGen B06 compiler — checkout flow business requirements
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
  run_id: "B06-compiler-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "We need to define the business requirements and use cases for the checkout flow, including guest checkout, registered user checkout, and payment failure scenarios."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B06-compiler-001-prompt"
  SourcePath: "wave-2/B06-batch-001/compiler-input/001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Checkout flow business requirements and use cases"
  CompiledText: "We need to define the business requirements and use cases for the checkout flow, including guest checkout, registered user checkout, and payment failure scenarios."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  DOCUMENT fires on: "define" (→ define), "requirements" (→ requirement),
    "use cases" (→ use-case)
  "business" — not independently a DOCUMENT term; it does not appear in the
    DOCUMENT lexicon on its own. However "business" occurs in "capability" context —
    see lexicon note below.
  No other predicate lexicon terms present. "checkout", "guest", "payment failure"
  contain no predicate terms.
  Confidence: 0.92 (≥1 predicate matched)

Lexicon note:
  The DOCUMENT lexicon includes "functional", "business", "capability", "behavior",
  "use-case", "narrative", "feature-brief", "epics" — these are all document-authoring
  signal words. "business requirements" fires DOCUMENT because "business" is in the
  DOCUMENT lexicon (it signals business-domain specification work).

---

## Pair B06B1-WDA-compiler-002

```
source_id: xiigen-b06-compiler-002
source_title: XIIGen B06 compiler — user story authoring for payment feature
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
  run_id: "B06-compiler-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Write the user stories and acceptance criteria for the payment processing feature, covering credit card, PayPal, and refund flows."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B06-compiler-002-prompt"
  SourcePath: "wave-2/B06-batch-001/compiler-input/002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Payment feature user stories and acceptance criteria"
  CompiledText: "Write the user stories and acceptance criteria for the payment processing feature, covering credit card, PayPal, and refund flows."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  DOCUMENT fires on: "story" (→ story), "criteria" (→ criteria)
  "write" — this is in the IMPLEMENT lexicon BUT here it is used in a documentation
    context ("write the user stories") rather than a code context. However the
    compiler operates on lexicon term presence, not intent. So "write" fires IMPLEMENT?

  Resolution: "write" IS in IMPLEMENT lexicon. This pair demonstrates a multi-predicate
    scenario: DOCUMENT fires (story, criteria) AND IMPLEMENT fires (write).
    Both predicates are correctly compiled.

  Correction to B output:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]

  Note: This teaches the model that "write the user stories" fires both DOCUMENT
  (story) and IMPLEMENT (write), producing a dual-predicate result. The router
  will then produce decisions for 2.1 (DOCUMENT → requirements) and 4.1 (IMPLEMENT →
  backend) — but the 4.1 routing is lower-confidence given the documentation context.
  This is an important boundary case.

  [Corrected B output:]
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null
  Confidence: 0.92

---

## Pair B06B1-WDA-compiler-003

```
source_id: xiigen-b06-compiler-003
source_title: XIIGen B06 compiler — functional spec for order cancellation capability
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
  run_id: "B06-compiler-003"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Describe the functional behavior and capability requirements for order cancellation, including the stakeholder rules for each cancellation window and refund eligibility."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B06-compiler-003-prompt"
  SourcePath: "wave-2/B06-batch-001/compiler-input/003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Order cancellation functional specification and capability requirements"
  CompiledText: "Describe the functional behavior and capability requirements for order cancellation, including the stakeholder rules for each cancellation window and refund eligibility."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  DOCUMENT fires on: "describe" (→ describe), "functional" (→ functional),
    "behavior" (→ behavior), "capability" (→ capability), "requirements" (→ requirement),
    "stakeholder" (→ stakeholder)
  This is a strong DOCUMENT signal — 6 independent lexicon terms fire.
  No IMPLEMENT, ARCHITECTURE, or other predicate terms are present.
  Confidence: 0.92 (≥1 predicate matched; high signal strength)
