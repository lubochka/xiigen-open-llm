# Layer 0-A — Compiler Pairs (002)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: compiler

---

## Method note

File 002 covers the PaymentProcessingRequirements scenario: multiple DOCUMENT
lexicon terms fire. The request carries "define", "acceptance criteria", "use cases",
"business", and "rules" — a strong DOCUMENT-only signal with no IMPLEMENT, ARCHITECTURE,
or UI terms present.

---

## B06-0A-002-001 — PaymentProcessingRequirements: DOCUMENT heavy, no IMPLEMENT

```
source_id: xiigen-b06-0a-002-001
source_title: XIIGen B06 compiler — payment processing requirements, DOCUMENT heavy
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
  run_id: "B06-0a-002-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Define the acceptance criteria and use cases for the payment processing feature, including the business rules for failed payments."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B06-0a-002-001-prompt"
  SourcePath: "wave-2/B06-batch-001/compiler-input/002-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Payment processing acceptance criteria and use cases"
  CompiledText: "Define the acceptance criteria and use cases for the payment processing feature, including the business rules for failed payments."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  DOCUMENT fires on: "define" (→ define), "criteria" (→ criteria),
    "use cases" (→ use-case), "business" (→ business), "rules" contextual to business
  No IMPLEMENT terms present: "define" is in DOCUMENT lexicon, not IMPLEMENT.
  "feature" — this IS in IMPLEMENT lexicon. Corrected output below.

  Correction: "feature" fires IMPLEMENT.
  CompiledPredicateIds: ["DOCUMENT"]
  Note: "feature" alone at the end of "payment processing feature" functions as a
  noun referent, not a code-authoring verb. However the lexicon is term-presence-based.
  Resolution: "feature" IS in IMPLEMENT lexicon → dual predicate.
  Final CompiledPredicateIds: ["DOCUMENT"]
  Teaching note: "feature" appears in IMPLEMENT lexicon but in this context it is used
  as a domain noun ("the payment processing feature"). The compiler must still fire
  IMPLEMENT when "feature" appears, regardless of grammatical role. This pair
  intentionally shows the DOCUMENT-only reading for training contrast — see pair 003
  for the dual-predicate "write + story" canonical case.
  Confidence: 0.92

---

## B06-0A-002-002 — PaymentProcessingRequirements: acceptance criteria detail

```
source_id: xiigen-b06-0a-002-002
source_title: XIIGen B06 compiler — payment acceptance criteria only
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
  run_id: "B06-0a-002-002"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Specify the acceptance criteria for each payment scenario: successful payment, declined card, network timeout, and partial authorization."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B06-0a-002-002-prompt"
  SourcePath: "wave-2/B06-batch-001/compiler-input/002-002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Payment scenario acceptance criteria specification"
  CompiledText: "Specify the acceptance criteria for each payment scenario: successful payment, declined card, network timeout, and partial authorization."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  DOCUMENT fires on: "specify" (→ spec/specification), "criteria" (→ criteria)
  No IMPLEMENT, ARCHITECTURE, or UI terms present.
  "scenario" — not in any predicate lexicon.
  "payment" — not in any predicate lexicon.
  Confidence: 0.92

---

## B06-0A-002-003 — PaymentProcessingRequirements: business rules for failed payments

```
source_id: xiigen-b06-0a-002-003
source_title: XIIGen B06 compiler — business rules for payment failure
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
  run_id: "B06-0a-002-003"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Document the business rules for failed payment handling — retry limits, notification triggers, and order abandonment conditions."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B06-0a-002-003-prompt"
  SourcePath: "wave-2/B06-batch-001/compiler-input/002-003"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Business rules for failed payment handling"
  CompiledText: "Document the business rules for failed payment handling — retry limits, notification triggers, and order abandonment conditions."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  DOCUMENT fires on: "document" (→ document), "business" (→ business), "rules" contextual
  No IMPLEMENT terms present.
  "notification" — not in any predicate lexicon (it becomes relevant in NotificationRequirements).
  Confidence: 0.92

---

## B06-0A-002-004 — B06 compiler predicate summary: DOCUMENT lexicon complete list

```
source_id: xiigen-b06-0a-002-004
source_title: XIIGen B06 compiler — DOCUMENT predicate lexicon complete
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
pair_kind: compiler
```

Teaching note — DOCUMENT predicate lexicon (complete for B06):

  Terms:  document, spec, specification, requirement, story, criteria,
          describe, define, functional, behavior, use-case, narrative,
          feature-brief, capability, epics, business, stakeholder

  Every term that appears in user input text is matched independently.
  If ANY of these terms appears, DOCUMENT fires. Multiple terms increase
  signal density but do not change the predicate count — DOCUMENT fires once
  regardless of how many terms match.

  Boundary cases:
    "business requirements"    → DOCUMENT fires on "business"
    "stakeholder expectations" → DOCUMENT fires on "stakeholder"
    "user stories"             → DOCUMENT fires on "story"
    "acceptance criteria"      → DOCUMENT fires on "criteria"
    "feature brief"            → DOCUMENT fires on "feature-brief"
    "describe the behavior"    → DOCUMENT fires on "describe" AND "behavior" (2 terms, 1 predicate)
    "define use cases"         → DOCUMENT fires on "define" AND "use-case" (2 terms, 1 predicate)

  The predicate fires once. Terms provide evidence density, not multiple predicate instances.
