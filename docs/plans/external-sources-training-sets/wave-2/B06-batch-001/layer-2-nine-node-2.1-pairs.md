# Layer 2 — Nine-Node Flow Pairs — Sub-Phase 2.1 Business Requirements and Use Cases
# Batch: B06-batch-001
# Scenario: B06 — Checkout flow business requirements and use cases
# Contract: RequirementsAuthoringFlow21 — nine sequential logical units
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: nine_node_flow

---

## Method note

Layer 2 pairs cover the nine canonical logical units for sub-phase 2.1.
The flow is invoked with SelectedLogicalTransition from B06B1-WDA-T1-001.
Node input chaining: Nodes 02, 04, 05, 08 process one item at a time;
Nodes 03, 06, 07, 09 take collection inputs.

---

## Pair B06B1-2.1-N01-001 — Node 01 DomainSplitterUnit

```
source_id: xiigen-b06-2.1-n01-001
contract_signature: DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>
transition_type: feature_addition
phase: 2
sub_phase: 2.1
node_role: node-01-domain-splitter
pair_kind: nine_node_flow
```

A:
  TransitionId: "transition-B06-t1-001-2.1-feature_addition"
  TransitionType: "feature_addition"
  PhaseId: "2"
  SubPhaseId: "2.1"
  UserRequest: "We need to define the business requirements and use cases for the checkout flow, including guest checkout, registered user checkout, and payment failure scenarios."
  PredicateIds: ["DOCUMENT"]
  EvidenceIds: ["B06-compiler-001-prompt"]

B:
  DomainPackets:
    - PacketId: "B06-2.1-packet-document"
      DomainTag: "DOCUMENT"
      Focus: "Use case authoring for checkout flow — guest checkout, registered user checkout, payment failure scenarios; business rules per actor; acceptance criteria per use case"
      SourceTransitionId: "transition-B06-t1-001-2.1-feature_addition"
      UserSignal: "define, business requirements, use cases, checkout flow, guest checkout, payment failure"

  [unit result envelope]
    UnitId: "node-01-domain-splitter"
    UnitRole: "domain-splitter"
    SubPhaseId: "2.1"
    PacketCount: 1
    Confidence: 0.92
    Warnings: []

---

## Pair B06B1-2.1-N02-001 — Node 02 DomainTranslatorUnit

```
source_id: xiigen-b06-2.1-n02-001
contract_signature: DomainTranslatorUnit : ILogicalUnit<DomainPacket, TranslatedDomainStatement>
transition_type: feature_addition
phase: 2
sub_phase: 2.1
node_role: node-02-domain-translator
pair_kind: nine_node_flow
```

A: [DomainPacket from Node 01 B — DOCUMENT packet]
  PacketId: "B06-2.1-packet-document"
  DomainTag: "DOCUMENT"
  Focus: "Use case authoring for checkout flow"
  UserSignal: "define, requirements, use cases, guest checkout, payment failure"

B:
  StatementId: "B06-2.1-stmt-document"
  DomainTag: "DOCUMENT"
  TechnicalStatement: "The checkout flow requires three fully structured use cases: (1) Guest checkout — actor is unauthenticated visitor, precondition is cart with items and valid email, main flow is cart review → address entry → payment → confirmation, alternative flows handle payment failure and address validation errors; (2) Registered user checkout — actor is authenticated user, preconditions include active account and saved payment method option, main flow adds optional saved payment step and order history linkage; (3) Payment failure and retry — actor is either guest or registered user, scenario covers partial authorization, network timeout, and declined card with retry path and order abandon path. Each use case must carry actor, preconditions, main flow steps (numbered), alternative flows, and postconditions. Acceptance criteria must be stated as testable assertions for each use case."
  SourcePacketId: "B06-2.1-packet-document"
  KeyTerms: ["use case", "actor", "preconditions", "main flow", "alternative flows", "postconditions", "acceptance criteria", "guest checkout", "registered user", "payment failure"]

  [unit result envelope]
    UnitId: "node-02-domain-translator"
    UnitRole: "domain-translator"
    SubPhaseId: "2.1"
    Confidence: 0.92
    Warnings: []

---

## Pair B06B1-2.1-N03-001 — Node 03 SchemePlannerUnit

```
source_id: xiigen-b06-2.1-n03-001
contract_signature: SchemePlannerUnit : ILogicalUnit<TranslatedDomainStatement[], WorkBranch[]>
transition_type: feature_addition
phase: 2
sub_phase: 2.1
node_role: node-03-scheme-planner
pair_kind: nine_node_flow
```

A: [All TranslatedDomainStatements]
  - StatementId: "B06-2.1-stmt-document"
    TechnicalStatement: "Three use cases required: guest checkout, registered user checkout, payment failure and retry. Each needs actor, preconditions, main flow, alternative flows, postconditions, and testable acceptance criteria."

B:
  WorkBranches:
    - BranchId: "B06-2.1-branch-guest-checkout"
      BranchTag: "UC-GUEST-CHECKOUT"
      Scope: "Guest checkout use case — actor: unauthenticated visitor; full use case structure with payment failure alternative flow"
      Priority: 1
      SourceStatementIds: ["B06-2.1-stmt-document"]

    - BranchId: "B06-2.1-branch-registered-checkout"
      BranchTag: "UC-REGISTERED-CHECKOUT"
      Scope: "Registered user checkout use case — actor: authenticated user; extends guest flow with saved payment and order history linkage"
      Priority: 2
      SourceStatementIds: ["B06-2.1-stmt-document"]

    - BranchId: "B06-2.1-branch-payment-failure"
      BranchTag: "UC-PAYMENT-FAILURE"
      Scope: "Payment failure and retry use case — covers partial authorization, network timeout, declined card, retry path, and order abandon path"
      Priority: 3
      SourceStatementIds: ["B06-2.1-stmt-document"]

    - BranchId: "B06-2.1-branch-acceptance-criteria"
      BranchTag: "ACCEPTANCE-CRITERIA"
      Scope: "Testable acceptance criteria for all three use cases — one assertion per flow step"
      Priority: 4
      SourceStatementIds: ["B06-2.1-stmt-document"]

  [unit result envelope]
    UnitId: "node-03-scheme-planner"
    UnitRole: "scheme-planner"
    SubPhaseId: "2.1"
    BranchCount: 4
    Confidence: 0.92
    Warnings: []

---

## Pair B06B1-2.1-N04-001 — Node 04 DomainProcessorUnit

```
source_id: xiigen-b06-2.1-n04-001
contract_signature: DomainProcessorUnit : ILogicalUnit<WorkBranch, ProcessedBranchResult>
transition_type: feature_addition
phase: 2
sub_phase: 2.1
node_role: node-04-domain-processor
pair_kind: nine_node_flow
```

Note: Shows the UC-GUEST-CHECKOUT branch.

A: [WorkBranch from Node 03 B — UC-GUEST-CHECKOUT branch]
  BranchId: "B06-2.1-branch-guest-checkout"
  BranchTag: "UC-GUEST-CHECKOUT"
  Scope: "Guest checkout use case — actor: unauthenticated visitor"

B:
  ProcessedResultId: "B06-2.1-proc-guest-checkout"
  BranchId: "B06-2.1-branch-guest-checkout"
  BranchTag: "UC-GUEST-CHECKOUT"
  UseCase:
    UseCaseId: "UC-001"
    Title: "Guest Checkout"
    Actor: "Unauthenticated Visitor"
    Preconditions:
      - "At least one item is in the shopping cart"
      - "A valid email address is available to the visitor"
      - "The selected item(s) are in stock"
    MainFlow:
      - Step: 1
        Action: "Visitor reviews cart contents, quantities, and item prices"
      - Step: 2
        Action: "Visitor enters shipping address and selects shipping method"
      - Step: 3
        Action: "Visitor enters payment details (credit card, PayPal, or saved wallet)"
      - Step: 4
        Action: "System validates payment details and reserves inventory"
      - Step: 5
        Action: "System creates order record and sends confirmation email to provided address"
    AlternativeFlows:
      - FlowId: "AF-001"
        Trigger: "Payment authorization fails at Step 4"
        Steps: ["System returns payment error with reason code", "Visitor may retry with different payment method", "If retry limit (3) reached, order is abandoned and inventory released"]
      - FlowId: "AF-002"
        Trigger: "Address validation fails at Step 2"
        Steps: ["System highlights invalid fields", "Visitor corrects address", "Return to Step 2"]
    Postconditions:
      - "Order record created with status PENDING_FULFILLMENT"
      - "Inventory reserved for ordered items"
      - "Confirmation email dispatched to visitor email address"
      - "No account created unless visitor explicitly opts in"

  [unit result envelope]
    UnitId: "node-04-domain-processor"
    UnitRole: "domain-processor"
    SubPhaseId: "2.1"
    Confidence: 0.92
    Warnings: []

---

## Pair B06B1-2.1-N05-001 — Node 05 ApplierUnit

```
source_id: xiigen-b06-2.1-n05-001
contract_signature: ApplierUnit : ILogicalUnit<ProcessedBranchResult, AppliedBranchResult>
transition_type: feature_addition
phase: 2
sub_phase: 2.1
node_role: node-05-applier
pair_kind: nine_node_flow
```

A: [ProcessedBranchResult from Node 04 B — UC-GUEST-CHECKOUT]
  ProcessedResultId: "B06-2.1-proc-guest-checkout"
  BranchTag: "UC-GUEST-CHECKOUT"
  UseCaseId: "UC-001 Guest Checkout"

B:
  AppliedResultId: "B06-2.1-applied-guest-checkout"
  BranchId: "B06-2.1-branch-guest-checkout"
  BranchTag: "UC-GUEST-CHECKOUT"
  AppliedItems:
    - ItemId: "applied-B06-2.1-01"
      Target: "Requirements document — UC-001 Guest Checkout"
      Change: "Register UC-001 Guest Checkout as a first-class use case in the checkout requirements document. Main flow (5 steps) and two alternative flows documented with retry limit business rule (3 attempts)."
      Outcome: "UC-001 authored and registered; retry limit rule captured as business constraint"

    - ItemId: "applied-B06-2.1-02"
      Target: "Business rules register"
      Change: "Add business rule BR-001: 'Guest checkout does not create a user account unless the visitor explicitly opts in at confirmation.' Add BR-002: 'Payment retry limit for guest checkout is 3 attempts before the order is abandoned.'"
      Outcome: "Two business rules registered; available for downstream implementation and test design"

    - ItemId: "applied-B06-2.1-03"
      Target: "Inventory reservation rule"
      Change: "Document rule: inventory is reserved at Step 4 (payment validation) and released if order is abandoned after retry limit. Reservation TTL: 30 minutes."
      Outcome: "Inventory reservation rule captured with TTL; critical for concurrent checkout scenarios"

  ApplicationSummary: "UC-001 Guest Checkout authored: 5 main steps, 2 alternative flows, 3 postconditions, 2 business rules, inventory reservation rule with 30-minute TTL."
  Warnings: []

  [unit result envelope]
    UnitId: "node-05-applier"
    UnitRole: "applier"
    SubPhaseId: "2.1"
    AppliedItemCount: 3
    Confidence: 0.92
    Warnings: []

---

## Pair B06B1-2.1-N06-001 — Node 06 ResultTranslatorUnit

```
source_id: xiigen-b06-2.1-n06-001
contract_signature: ResultTranslatorUnit : ILogicalUnit<AppliedBranchResult[], TranslatedResultFragment[]>
transition_type: feature_addition
phase: 2
sub_phase: 2.1
node_role: node-06-result-translator
pair_kind: nine_node_flow
```

A: [All AppliedBranchResults — one per WorkBranch]
  - AppliedResultId: "B06-2.1-applied-guest-checkout", BranchTag: "UC-GUEST-CHECKOUT"
  - AppliedResultId: "B06-2.1-applied-registered-checkout", BranchTag: "UC-REGISTERED-CHECKOUT"
  - AppliedResultId: "B06-2.1-applied-payment-failure", BranchTag: "UC-PAYMENT-FAILURE"
  - AppliedResultId: "B06-2.1-applied-acceptance-criteria", BranchTag: "ACCEPTANCE-CRITERIA"

B:
  TranslatedResultFragments:
    - FragmentId: "frag-B06-2.1-use-cases"
      FragmentType: "use_case_document"
      Content:
        UseCases:
          - UseCaseId: "UC-001"
            Title: "Guest Checkout"
            Actor: "Unauthenticated Visitor"
            MainFlowStepCount: 5
            AlternativeFlowCount: 2
            BusinessRules: ["BR-001 (no auto account creation)", "BR-002 (retry limit 3)"]
          - UseCaseId: "UC-002"
            Title: "Registered User Checkout"
            Actor: "Authenticated User"
            MainFlowStepCount: 6
            AlternativeFlowCount: 2
            BusinessRules: ["BR-003 (saved payment optional)", "BR-004 (order linked to account history)"]
          - UseCaseId: "UC-003"
            Title: "Payment Failure and Retry"
            Actor: "Guest or Registered User"
            MainFlowStepCount: 4
            AlternativeFlowCount: 3
            BusinessRules: ["BR-002 (retry limit 3)", "BR-005 (inventory released on abandon)"]
      SourceBranchIds: ["B06-2.1-branch-guest-checkout", "B06-2.1-branch-registered-checkout", "B06-2.1-branch-payment-failure"]

    - FragmentId: "frag-B06-2.1-acceptance-criteria"
      FragmentType: "acceptance_criteria"
      Content:
        Criteria:
          - CriteriaId: "AC-001"
            UseCaseRef: "UC-001"
            Assertion: "Given a cart with items and a valid email, when the guest submits payment, then an order is created and a confirmation email is sent within 30 seconds"
          - CriteriaId: "AC-002"
            UseCaseRef: "UC-001"
            Assertion: "Given a declined payment, when the guest has attempted 3 times, then the order is abandoned and inventory is released"
          - CriteriaId: "AC-003"
            UseCaseRef: "UC-002"
            Assertion: "Given a logged-in user with a saved payment method, when they confirm checkout, then the saved method is pre-selected and the order is linked to their account history"
          - CriteriaId: "AC-004"
            UseCaseRef: "UC-003"
            Assertion: "Given a payment timeout, when the system retries, then the retry is logged and the user is notified of the delay"
      SourceBranchIds: ["B06-2.1-branch-acceptance-criteria"]

  [unit result envelope]
    UnitId: "node-06-result-translator"
    UnitRole: "result-translator"
    SubPhaseId: "2.1"
    FragmentCount: 2
    Confidence: 0.92
    Warnings: []

---

## Pair B06B1-2.1-N07-001 — Node 07 ArbiterSelectorUnit

```
source_id: xiigen-b06-2.1-n07-001
contract_signature: ArbiterSelectorUnit : ILogicalUnit<TranslatedResultFragment[], ArbiterReviewRequest[]>
transition_type: feature_addition
phase: 2
sub_phase: 2.1
node_role: node-07-arbiter-selector
pair_kind: nine_node_flow
```

A: [TranslatedResultFragments from Node 06 B — 2 fragments]
  - FragmentId: "frag-B06-2.1-use-cases", FragmentType: "use_case_document"
  - FragmentId: "frag-B06-2.1-acceptance-criteria", FragmentType: "acceptance_criteria"

B:
  ArbiterReviewRequests:
    - RequestId: "arbiter-req-B06-2.1-01"
      ArbiterRole: "requirements-completeness-arbiter"
      FragmentIds: ["frag-B06-2.1-use-cases"]
      ReviewFocus: "Verify all three use cases are present and each has actor, preconditions, main flow, alternative flows, and postconditions"
      AcceptanceCriteria: "UseCaseCount == 3; every use case has MainFlowStepCount >= 3; every use case has AlternativeFlowCount >= 1"

    - RequestId: "arbiter-req-B06-2.1-02"
      ArbiterRole: "testability-arbiter"
      FragmentIds: ["frag-B06-2.1-acceptance-criteria"]
      ReviewFocus: "Verify acceptance criteria are testable assertions — Given/When/Then format with measurable outcomes"
      AcceptanceCriteria: "CriteriaCount >= 3; all criteria use Given/When/Then structure; no vague qualifiers (better, faster, nice)"

  [unit result envelope]
    UnitId: "node-07-arbiter-selector"
    UnitRole: "arbiter-selector"
    SubPhaseId: "2.1"
    RequestCount: 2
    Confidence: 0.92
    Warnings: []

---

## Pair B06B1-2.1-N08-001 — Node 08 ArbiterUnit

```
source_id: xiigen-b06-2.1-n08-001
contract_signature: ArbiterUnit : ILogicalUnit<ArbiterReviewRequest, ArbiterVerdict>
transition_type: feature_addition
phase: 2
sub_phase: 2.1
node_role: node-08-arbiter
pair_kind: nine_node_flow
```

Note: Shows the requirements-completeness review request.

A: [ArbiterReviewRequest from Node 07 B — requirements completeness]
  RequestId: "arbiter-req-B06-2.1-01"
  ArbiterRole: "requirements-completeness-arbiter"
  FragmentIds: ["frag-B06-2.1-use-cases"]
  ReviewFocus: "Verify all three use cases present with complete structure"
  AcceptanceCriteria: "UseCaseCount == 3; MainFlowStepCount >= 3 each; AlternativeFlowCount >= 1 each"

B:
  VerdictId: "verdict-B06-2.1-01-completeness"
  RequestId: "arbiter-req-B06-2.1-01"
  ArbiterRole: "requirements-completeness-arbiter"
  Verdict: "PASS"
  ReviewedFragmentId: "frag-B06-2.1-use-cases"
  Findings:
    - FindingId: "finding-B06-2.1-01"
      Type: "pass"
      Description: "UseCaseCount 3 equals threshold 3. UC-001 has 5 main steps and 2 alternative flows. UC-002 has 6 main steps and 2 alternative flows. UC-003 has 4 main steps and 3 alternative flows. All three satisfy actor, preconditions, main flow, alternative flows, and postconditions."
    - FindingId: "finding-B06-2.1-02"
      Type: "observation"
      Description: "UC-003 (Payment Failure) is referenced as an alternative flow of both UC-001 and UC-002. Its promotion to a standalone use case is correct — it has sufficient complexity and distinct actors to warrant independent documentation."
  CriteriaResult:
    UseCaseCount: "3 (== 3: PASS)"
    MinMainFlowSteps: "4 (>= 3: PASS)"
    MinAlternativeFlows: "2 (>= 1: PASS)"
  AcceptanceMet: true

  [unit result envelope]
    UnitId: "node-08-arbiter"
    UnitRole: "arbiter"
    SubPhaseId: "2.1"
    Confidence: 0.92
    Warnings: []

---

## Pair B06B1-2.1-N09-001 — Node 09 FormatterUnit

```
source_id: xiigen-b06-2.1-n09-001
contract_signature: FormatterUnit : ILogicalUnit<ReviewedResultSet, HumanResult>
transition_type: feature_addition
phase: 2
sub_phase: 2.1
node_role: node-09-formatter
pair_kind: nine_node_flow
```

A: [ReviewedResultSet — all ArbiterVerdicts]
  Verdicts:
    - VerdictId: "verdict-B06-2.1-01-completeness", Verdict: "PASS", AcceptanceMet: true
    - VerdictId: "verdict-B06-2.1-02-testability", Verdict: "PASS", AcceptanceMet: true

B:
  FlowId: "requirements-authoring-flow-2.1"
  ScenarioId: "adaptive-logical-orchestration"
  OverallVerdict: "PASS"
  FinalHumanResult: |
    Business requirements and use cases for the checkout flow (sub-phase 2.1):

    UC-001 — Guest Checkout (actor: Unauthenticated Visitor)
      Preconditions: cart has items; valid email available; stock confirmed.
      Main flow (5 steps): cart review → address + shipping → payment entry
        → payment validation and inventory reserve → order creation + email.
      Alternative flows: payment failure with 3-retry limit then abandon;
        address validation error with correction loop.
      Postconditions: order PENDING_FULFILLMENT; inventory reserved; email sent;
        no auto account creation (BR-001). Retry limit: 3 (BR-002).

    UC-002 — Registered User Checkout (actor: Authenticated User)
      Extends UC-001 main flow with saved payment pre-selection (step 3a)
      and order linkage to account history (postcondition).
      Business rules: BR-003 (saved payment optional), BR-004 (history linkage).

    UC-003 — Payment Failure and Retry (actor: Guest or Registered User)
      Covers partial authorization, network timeout, declined card.
      Retry path (up to 3 attempts); abandon path releases inventory.
      Business rules: BR-002 (retry limit), BR-005 (inventory release on abandon).

    Acceptance criteria: 4 Given/When/Then assertions covering confirmation
    email timing, retry limit enforcement, saved payment pre-selection, and
    timeout notification. Arbiter reviews: PASS (completeness) and PASS (testability).

  Metrics:
    node_count: "9"
    sub_phase_id: "2.1"
    node_01_unit_id: "node-01-domain-splitter"
    node_02_unit_id: "node-02-domain-translator"
    node_03_unit_id: "node-03-scheme-planner"
    node_04_unit_id: "node-04-domain-processor"
    node_05_unit_id: "node-05-applier"
    node_06_unit_id: "node-06-result-translator"
    node_07_unit_id: "node-07-arbiter-selector"
    node_08_unit_id: "node-08-arbiter"
    node_09_unit_id: "node-09-formatter"
    branch_count: "4"
    use_case_count: "3"
    business_rule_count: "5"
    acceptance_criteria_count: "4"
    fragment_count: "2"
    verdict_count: "2"

  EvidenceIds:
    - "B06-2.1-stmt-document"
    - "B06-2.1-branch-guest-checkout"
    - "B06-2.1-branch-registered-checkout"
    - "B06-2.1-branch-payment-failure"
    - "B06-2.1-branch-acceptance-criteria"
    - "frag-B06-2.1-use-cases"
    - "frag-B06-2.1-acceptance-criteria"
    - "verdict-B06-2.1-01-completeness"
    - "verdict-B06-2.1-02-testability"

  Warnings: []

  [unit result envelope]
    UnitId: "node-09-formatter"
    UnitRole: "formatter"
    SubPhaseId: "2.1"
    Confidence: 0.92
    Warnings: []
