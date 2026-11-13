# Layer 2 — Nine-Node Flow Pairs — Sub-Phase 2.1 (006)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: nine_node_flow

---

## Method note

File 006 covers RefundPolicyRequirements: refund and return policy (DOCUMENT).
6 business rules documented.

---

## B06-2.1-006-N01 — Node 01 DomainSplitterUnit: RefundPolicyRequirements

```
source_id: xiigen-b06-2.1-006-n01
contract_signature: DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>
transition_type: documentation
phase: 2
sub_phase: 2.1
node_role: node-01-domain-splitter
scenario: RefundPolicyRequirements
pair_kind: nine_node_flow
```

A:
  TransitionId: "transition-B06-refund-policy-2.1"
  TransitionType: "documentation"
  SubPhaseId: "2.1"
  UserRequest: "Document the refund and return policy requirements — covering physical goods, digital goods, and subscription cancellations."
  PredicateIds: ["DOCUMENT"]

B:
  DomainPackets:
    - PacketId: "B06-2.1-006-packet-document"
      DomainTag: "DOCUMENT"
      Focus: "Refund and return policy requirements — physical goods return window and eligibility; digital goods refund policy; subscription cancellation and prorated refund; processing timeline and method"
      UserSignal: "refund, return policy, requirements, physical goods, digital goods, subscriptions"
  PacketCount: 1
  Confidence: 0.92

---

## B06-2.1-006-N02 — Node 02 DomainTranslatorUnit: refund policy rules

```
source_id: xiigen-b06-2.1-006-n02
contract_signature: DomainTranslatorUnit : ILogicalUnit<DomainPacket, TranslatedDomainStatement>
node_role: node-02-domain-translator
scenario: RefundPolicyRequirements
pair_kind: nine_node_flow
```

B:
  StatementId: "B06-2.1-006-stmt-document"
  TechnicalStatement: "Refund and return policy by product type: Physical goods: return window 30 days from delivery; item must be in original packaging; full refund if unopened; 80% refund if opened but undamaged; 50% refund if damaged due to customer misuse; no refund for consumables once opened. Digital goods: refund allowed within 48 hours of purchase if license has not been activated; once activated, no refund unless product is defective; defective digital product: full refund after verification; refund processing: 3-5 business days to original payment method. Subscriptions: cancellation at any time; prorated refund for unused billing period; minimum 1 day prorated; refund issued within 5 business days. Business rules: BR-R01 (30-day physical return window); BR-R02 (80% opened goods refund); BR-R03 (digital 48h pre-activation refund); BR-R04 (no refund post-activation unless defective); BR-R05 (prorated subscription refund); BR-R06 (5-day processing for subscription refunds)."
  KeyTerms: ["refund policy", "return window", "prorated refund", "digital goods", "subscription cancellation", "defective product"]
  Confidence: 0.92

---

## B06-2.1-006-N03 — Node 03 SchemePlannerUnit: refund branches

```
source_id: xiigen-b06-2.1-006-n03
contract_signature: SchemePlannerUnit : ILogicalUnit<TranslatedDomainStatement[], WorkBranch[]>
node_role: node-03-scheme-planner
scenario: RefundPolicyRequirements
pair_kind: nine_node_flow
```

B:
  WorkBranches:
    - BranchId: "B06-2.1-006-branch-physical"
      BranchTag: "REFUND-PHYSICAL"
      Scope: "Physical goods return policy — 30-day window, condition tiers"
      Priority: 1

    - BranchId: "B06-2.1-006-branch-digital"
      BranchTag: "REFUND-DIGITAL"
      Scope: "Digital goods refund policy — 48h pre-activation window"
      Priority: 2

    - BranchId: "B06-2.1-006-branch-subscription"
      BranchTag: "REFUND-SUBSCRIPTION"
      Scope: "Subscription cancellation and prorated refund"
      Priority: 3

    - BranchId: "B06-2.1-006-branch-rules"
      BranchTag: "REFUND-BUSINESS-RULES"
      Scope: "6 business rules BR-R01 through BR-R06"
      Priority: 4

  BranchCount: 4
  Confidence: 0.92

---

## B06-2.1-006-N09 — Node 09 FormatterUnit: RefundPolicyRequirements complete

```
source_id: xiigen-b06-2.1-006-n09
contract_signature: FormatterUnit : ILogicalUnit<ReviewedResultSet, HumanResult>
node_role: node-09-formatter
scenario: RefundPolicyRequirements
pair_kind: nine_node_flow
```

B:
  FlowId: "requirements-authoring-flow-2.1"
  OverallVerdict: "PASS"
  FinalHumanResult: |
    Refund and return policy requirements (sub-phase 2.1):

    Physical goods: 30-day return window from delivery. Full refund (unopened),
      80% (opened, undamaged), 50% (customer-damaged). No refund on opened consumables.
      BR-R01, BR-R02.

    Digital goods: 48h refund window pre-activation. Post-activation: no refund unless
      defective. Defective goods: full refund after verification. BR-R03, BR-R04.

    Subscriptions: Cancellation any time. Prorated refund for unused period (min 1 day).
      Processed in 5 business days. BR-R05, BR-R06.

    6 business rules documented (BR-R01 through BR-R06).
    Arbiter reviews: PASS (3 product type policies complete), PASS (rule atomicity).

  Metrics:
    product_type_count: "3"
    business_rule_count: "6"
    refund_tier_count: "3"
  Warnings: []
