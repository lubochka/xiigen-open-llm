# Layer 2 — Nine-Node Flow Pairs — Sub-Phase 2.1 (002)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: nine_node_flow

---

## Method note

File 002 covers PaymentProcessingRequirements: acceptance criteria + use cases for payment.
Five use cases, 12 acceptance criteria. N01 through N09.

---

## B06-2.1-002-N01 — Node 01 DomainSplitterUnit: PaymentProcessingRequirements

```
source_id: xiigen-b06-2.1-002-n01
contract_signature: DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>
transition_type: documentation
phase: 2
sub_phase: 2.1
node_role: node-01-domain-splitter
scenario: PaymentProcessingRequirements
pair_kind: nine_node_flow
```

A:
  TransitionId: "transition-B06-payment-requirements-2.1"
  TransitionType: "documentation"
  SubPhaseId: "2.1"
  UserRequest: "Define the acceptance criteria and use cases for the payment processing feature, including the business rules for failed payments."
  PredicateIds: ["DOCUMENT"]

B:
  DomainPackets:
    - PacketId: "B06-2.1-002-packet-document"
      DomainTag: "DOCUMENT"
      Focus: "Payment processing use cases and acceptance criteria — successful payment, declined card, network timeout, partial authorization, and refund initiation; business rules for retry limits and inventory hold"
      UserSignal: "acceptance criteria, use cases, payment processing, business rules, failed payments"

  UnitId: "node-01-domain-splitter"
  PacketCount: 1
  Confidence: 0.92

---

## B06-2.1-002-N02 — Node 02 DomainTranslatorUnit: payment use cases

```
source_id: xiigen-b06-2.1-002-n02
contract_signature: DomainTranslatorUnit : ILogicalUnit<DomainPacket, TranslatedDomainStatement>
node_role: node-02-domain-translator
scenario: PaymentProcessingRequirements
pair_kind: nine_node_flow
```

A: [DomainPacket from N01]
  PacketId: "B06-2.1-002-packet-document"
  Focus: "Payment processing use cases and acceptance criteria"

B:
  StatementId: "B06-2.1-002-stmt-document"
  TechnicalStatement: "Five payment use cases required: (1) Successful Payment — actor: registered or guest user; main flow: cart → payment entry → authorization → confirmation; (2) Declined Card — actor: any user; triggers: insufficient funds, expired card, CVV (Card Verification Value) mismatch; retry path up to 3 attempts; (3) Network Timeout — actor: any user; system retries once automatically; user notified with delay message; (4) Partial Authorization — actor: registered user with split payment; partially authorized amount captured; remainder charged to secondary method; (5) Refund Initiation — actor: registered user; post-order actor; main flow: select order → request refund → system validates eligibility → refund queued. Each use case needs actor, preconditions, main flow, alternative flows, postconditions, and 2-3 acceptance criteria in Given/When/Then format."
  KeyTerms: ["payment use case", "declined card", "network timeout", "partial authorization", "refund", "retry", "Given/When/Then"]
  Confidence: 0.92

---

## B06-2.1-002-N03 — Node 03 SchemePlannerUnit: payment work branches

```
source_id: xiigen-b06-2.1-002-n03
contract_signature: SchemePlannerUnit : ILogicalUnit<TranslatedDomainStatement[], WorkBranch[]>
node_role: node-03-scheme-planner
scenario: PaymentProcessingRequirements
pair_kind: nine_node_flow
```

A: [TranslatedDomainStatement from N02]
  StatementId: "B06-2.1-002-stmt-document"

B:
  WorkBranches:
    - BranchId: "B06-2.1-002-branch-uc-payment-success"
      BranchTag: "UC-PAY-SUCCESS"
      Scope: "Successful payment use case"
      Priority: 1

    - BranchId: "B06-2.1-002-branch-uc-declined"
      BranchTag: "UC-PAY-DECLINED"
      Scope: "Declined card use case — retry logic and abandonment"
      Priority: 2

    - BranchId: "B06-2.1-002-branch-uc-timeout"
      BranchTag: "UC-PAY-TIMEOUT"
      Scope: "Network timeout use case — auto retry and notification"
      Priority: 3

    - BranchId: "B06-2.1-002-branch-uc-partial"
      BranchTag: "UC-PAY-PARTIAL"
      Scope: "Partial authorization use case — split payment"
      Priority: 4

    - BranchId: "B06-2.1-002-branch-uc-refund"
      BranchTag: "UC-PAY-REFUND"
      Scope: "Refund initiation use case"
      Priority: 5

    - BranchId: "B06-2.1-002-branch-ac"
      BranchTag: "ACCEPTANCE-CRITERIA"
      Scope: "12 Given/When/Then acceptance criteria across all payment use cases"
      Priority: 6

  BranchCount: 6
  Confidence: 0.92

---

## B06-2.1-002-N09 — Node 09 FormatterUnit: PaymentProcessingRequirements complete

```
source_id: xiigen-b06-2.1-002-n09
contract_signature: FormatterUnit : ILogicalUnit<ReviewedResultSet, HumanResult>
node_role: node-09-formatter
scenario: PaymentProcessingRequirements
pair_kind: nine_node_flow
```

A: [All ArbiterVerdicts — PASS]
  Verdicts:
    - VerdictId: "verdict-B06-2.1-002-completeness", Verdict: "PASS"
    - VerdictId: "verdict-B06-2.1-002-testability", Verdict: "PASS"

B:
  FlowId: "requirements-authoring-flow-2.1"
  OverallVerdict: "PASS"
  FinalHumanResult: |
    Payment processing requirements (sub-phase 2.1):

    5 use cases authored:
    UC-PAY-001 Successful Payment: actor any user; main flow 5 steps (cart review →
      payment entry → authorization → confirmation → email); business rules BR-P01
      (authorization timeout 30s), BR-P02 (confirmation email within 60s).

    UC-PAY-002 Declined Card: actor any user; main flow includes retry path (max 3);
      alternative flows: retry, switch payment method, abandon; business rule BR-P03
      (retry limit 3, then order abandoned).

    UC-PAY-003 Network Timeout: actor any user; system auto-retry once; user notification
      on delay; business rule BR-P04 (auto retry interval 5s).

    UC-PAY-004 Partial Authorization: actor registered user; split payment across two
      methods; business rule BR-P05 (secondary method must be pre-registered).

    UC-PAY-005 Refund Initiation: actor registered user; eligibility validation; refund
      queued within 24h; business rule BR-P06 (refund window 30 days from order).

    12 acceptance criteria: 2-3 per use case, Given/When/Then format.
    Arbiter reviews: PASS (completeness — 5 of 5 use cases), PASS (testability — 12 criteria).

  Metrics:
    use_case_count: "5"
    acceptance_criteria_count: "12"
    business_rule_count: "6"
    branch_count: "6"
    verdict_count: "2"
  Warnings: []
  Confidence: 0.92
