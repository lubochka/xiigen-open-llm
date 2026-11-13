# Layer 2 — Nine-Node Flow Pairs — Sub-Phase 2.1 (004)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: nine_node_flow

---

## Method note

File 004 covers CancellationPolicySpec: order cancellation business rules (DOCUMENT).
Business rules documented, cancellation windows defined. TransitionType: documentation.

---

## B06-2.1-004-N01 — Node 01 DomainSplitterUnit: CancellationPolicySpec

```
source_id: xiigen-b06-2.1-004-n01
contract_signature: DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>
transition_type: documentation
phase: 2
sub_phase: 2.1
node_role: node-01-domain-splitter
scenario: CancellationPolicySpec
pair_kind: nine_node_flow
```

A:
  TransitionId: "transition-B06-cancellation-policy-2.1"
  TransitionType: "documentation"
  SubPhaseId: "2.1"
  UserRequest: "Document the business rules for order cancellation — cancellation windows, refund eligibility, and inventory release."
  PredicateIds: ["DOCUMENT"]

B:
  DomainPackets:
    - PacketId: "B06-2.1-004-packet-document"
      DomainTag: "DOCUMENT"
      Focus: "Order cancellation business rules — cancellation windows by order status, refund eligibility rules, inventory release timing, and stakeholder rules for each window"
      UserSignal: "business rules, cancellation, refund eligibility, inventory release"
  PacketCount: 1
  Confidence: 0.92

---

## B06-2.1-004-N02 — Node 02 DomainTranslatorUnit: cancellation rules

```
source_id: xiigen-b06-2.1-004-n02
contract_signature: DomainTranslatorUnit : ILogicalUnit<DomainPacket, TranslatedDomainStatement>
node_role: node-02-domain-translator
scenario: CancellationPolicySpec
pair_kind: nine_node_flow
```

A: [DomainPacket from N01]

B:
  StatementId: "B06-2.1-004-stmt-document"
  TechnicalStatement: "Order cancellation business rules by cancellation window: (1) Pre-fulfillment window (order status PENDING): cancellation allowed immediately; full refund to original payment method; inventory released within 5 minutes; (2) In-fulfillment window (status PROCESSING): cancellation not allowed; customer must wait for delivery and initiate return; (3) Post-delivery window (status DELIVERED, within 30 days): return initiation allowed; refund issued after item return receipt confirmed; full refund if item returned in original condition; partial refund (80%) if item opened but undamaged; (4) After 30 days: no cancellation or refund available. Business rules: BR-C01 (cancellation only in PENDING); BR-C02 (refund to original payment method); BR-C03 (inventory released within 5 minutes of PENDING cancellation); BR-C04 (30-day return window); BR-C05 (partial refund 80% for opened items)."
  KeyTerms: ["cancellation window", "PENDING", "PROCESSING", "DELIVERED", "refund eligibility", "inventory release"]
  Confidence: 0.92

---

## B06-2.1-004-N03 — Node 03 SchemePlannerUnit: cancellation branches

```
source_id: xiigen-b06-2.1-004-n03
contract_signature: SchemePlannerUnit : ILogicalUnit<TranslatedDomainStatement[], WorkBranch[]>
node_role: node-03-scheme-planner
scenario: CancellationPolicySpec
pair_kind: nine_node_flow
```

B:
  WorkBranches:
    - BranchId: "B06-2.1-004-branch-pre-fulfillment"
      BranchTag: "CANCEL-PRE-FULFILLMENT"
      Scope: "Pre-fulfillment cancellation: PENDING status, full refund, 5-min inventory release"
      Priority: 1

    - BranchId: "B06-2.1-004-branch-in-fulfillment"
      BranchTag: "CANCEL-IN-FULFILLMENT"
      Scope: "In-fulfillment: cancellation not allowed, return path only"
      Priority: 2

    - BranchId: "B06-2.1-004-branch-post-delivery"
      BranchTag: "CANCEL-POST-DELIVERY"
      Scope: "Post-delivery return: 30-day window, full/partial refund"
      Priority: 3

    - BranchId: "B06-2.1-004-branch-business-rules"
      BranchTag: "CANCEL-BUSINESS-RULES"
      Scope: "5 business rules (BR-C01 through BR-C05)"
      Priority: 4

  BranchCount: 4
  Confidence: 0.92

---

## B06-2.1-004-N09 — Node 09 FormatterUnit: CancellationPolicySpec complete

```
source_id: xiigen-b06-2.1-004-n09
contract_signature: FormatterUnit : ILogicalUnit<ReviewedResultSet, HumanResult>
node_role: node-09-formatter
scenario: CancellationPolicySpec
pair_kind: nine_node_flow
```

B:
  FlowId: "requirements-authoring-flow-2.1"
  OverallVerdict: "PASS"
  FinalHumanResult: |
    Order cancellation policy specification (sub-phase 2.1):

    Pre-fulfillment window (PENDING): Cancellation allowed immediately. Full refund
      to original payment method. Inventory released within 5 minutes. BR-C01, BR-C02, BR-C03.

    In-fulfillment window (PROCESSING): Cancellation not allowed. Customer directed
      to post-delivery return path. No refund at this stage.

    Post-delivery window (DELIVERED, <= 30 days): Return initiation allowed. Full refund
      if item in original condition. Partial refund (80%) if opened but undamaged. BR-C04, BR-C05.

    After 30 days: No cancellation or refund. Policy expiry enforced.

    5 business rules documented (BR-C01 through BR-C05).
    Arbiter reviews: PASS (completeness — 3 windows + no-action case), PASS (rule clarity).

  Metrics:
    cancellation_window_count: "3 + expired"
    business_rule_count: "5"
    refund_tiers: "full, partial (80%), none"
  Warnings: []
