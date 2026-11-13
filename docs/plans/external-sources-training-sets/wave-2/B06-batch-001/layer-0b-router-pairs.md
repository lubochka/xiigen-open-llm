# Layer 0-B — Router Pairs
# Batch: B06-batch-001
# Scenario: B06 — Checkout flow business requirements and use cases
# Contract: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: router

---

## Method note

Layer 0-B pairs mirror Layer 0-A B outputs as their A inputs.

Routing rules demonstrated in this batch:
  [DOCUMENT] alone           → single decision: 2.1 (Business Requirements and Use Cases)
  [DOCUMENT, IMPLEMENT]      → two decisions: 2.1 + 4.1 (IMPLEMENT signals backend work)
  [DOCUMENT] high-signal     → single decision: 2.1 (many DOCUMENT terms, no competing predicates)

---

## Pair B06B1-WDA-router-001

```
source_id: xiigen-b06-router-001
source_title: XIIGen B06 router — DOCUMENT-only routing to sub-phase 2.1
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
phase: 2
sub_phase: 2.1
task_type: requirements-and-business-rules
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — taken verbatim from Layer 0-A pair B06B1-WDA-compiler-001 B output]
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

B:
  RoutingDecisions:
    - DocumentId: "B06-compiler-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.37
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 1

Routing rationale:
  DOCUMENT predicate (define, requirements, use-case) routes to 2.1 (Business
  Requirements and Use Cases). Sub-phases 2.2-2.4 also carry DOCUMENT but 2.1
  is the primary owner of use-case and requirements vocabulary. NearestWrongSubPhaseScore
  0.37 reflects that 2.4 (Stakeholder Requirements) is the closest competitor but
  lacks the use-case specificity of this request.

---

## Pair B06B1-WDA-router-002

```
source_id: xiigen-b06-router-002
source_title: XIIGen B06 router — DOCUMENT+IMPLEMENT dual-branch routing to 2.1 and 4.1
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
phase: 2
sub_phase: 2.1
task_type: requirements-and-business-rules
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — taken verbatim from Layer 0-A pair B06B1-WDA-compiler-002 B output (corrected)]
  DocumentId: "B06-compiler-002-prompt"
  SourcePath: "wave-2/B06-batch-001/compiler-input/002"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Payment feature user stories and acceptance criteria"
  CompiledText: "Write the user stories and acceptance criteria for the payment processing feature, covering credit card, PayPal, and refund flows."
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B06-compiler-002-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.41
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B06-compiler-002-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.38
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 2

Routing rationale:
  "story" and "criteria" fire DOCUMENT → routes to 2.1 (requirements authoring).
  "write" fires IMPLEMENT → routes to 4.1 (Backend Services and Business Logic).
  Two predicates produce two independent routing decisions. The model must not
  suppress the 4.1 decision because "write" appears in a documentation context —
  the compiler detected it as IMPLEMENT and the router must honor that. Domain
  disambiguation happens in the orchestrator's flow selection, not the router.

---

## Pair B06B1-WDA-router-003

```
source_id: xiigen-b06-router-003
source_title: XIIGen B06 router — strong DOCUMENT signal routes to 2.1
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
phase: 2
sub_phase: 2.1
task_type: requirements-and-business-rules
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — taken verbatim from Layer 0-A pair B06B1-WDA-compiler-003 B output]
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

B:
  RoutingDecisions:
    - DocumentId: "B06-compiler-003-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.44
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 1

Routing rationale:
  Six DOCUMENT lexicon terms fired (describe, functional, behavior, capability,
  requirements, stakeholder). The high signal density reinforces 2.1. The
  "stakeholder" term also partially matches 2.4 (External Stakeholder Requirements)
  but the presence of "functional", "capability", and "behavior" makes 2.1 the
  primary target. NearestWrongSubPhaseScore 0.44 reflects the 2.4 proximity.
