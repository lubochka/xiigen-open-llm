# Layer 2 — Nine-Node Flow Pairs — Sub-Phase 2.1 (013)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: nine_node_flow

---

## Method note

File 013: ShippingRequirements — DOCUMENT alone, TransitionType: documentation.
Nine nodes: DomainSplitter→DomainTranslator→SchemePlanner→DomainProcessor→Applier→ResultTranslator→ArbiterSelector→Arbiter→Formatter.

---

## B06-2.1-013-N01 — DomainSplitterUnit: ShippingRequirements

```
source_id: xiigen-b06-2.1-013-n01
contract_signature: DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>
node_role: node-01-domain-splitter
scenario: ShippingRequirements
pair_kind: nine_node_flow
```

A:
  TransitionId: "transition-shipping-013-001"
  TransitionType: "documentation"
  SubPhaseId: "2.1"
  PredicateIds: ["DOCUMENT"]
  UserRequest: "Define the shipping requirements for the checkout flow, covering standard, express, and overnight delivery options."

B:
  DomainPackets:
    - PacketId: "shipping-013-packet-document"
      DomainTag: "DOCUMENT"
      Focus: "Shipping requirements: use cases for standard/express/overnight delivery, acceptance criteria, business rules for carrier selection and delivery windows"
      SubPhaseSignal: "2.1"
  PacketCount: 1
  SelectedLogicalTransition: "documentation"

---

## B06-2.1-013-N02 — DomainTranslatorUnit: shipping requirements statement

```
source_id: xiigen-b06-2.1-013-n02
contract_signature: DomainTranslatorUnit : ILogicalUnit<DomainPacket, TranslatedDomainStatement>
node_role: node-02-domain-translator
scenario: ShippingRequirements
pair_kind: nine_node_flow
```

B:
  StatementId: "shipping-013-stmt"
  TechnicalStatement: "Checkout shipping requirements: (1) UC-S001 Standard delivery selection (3-5 business days); (2) UC-S002 Express delivery selection (1-2 business days); (3) UC-S003 Overnight delivery selection (next day); (4) Business rules: carrier eligibility per delivery zone, weight limits per delivery type, address validation before carrier assignment; (5) Acceptance criteria: all three delivery types available for valid domestic addresses; international addresses show eligible options only."
  KeyTerms: ["standard delivery", "express delivery", "overnight delivery", "carrier eligibility", "delivery zone", "address validation"]
  Confidence: 0.92

---

## B06-2.1-013-N03 — SchemePlannerUnit: shipping branches

```
source_id: xiigen-b06-2.1-013-n03
contract_signature: SchemePlannerUnit : ILogicalUnit<TranslatedDomainStatement[], WorkBranch[]>
node_role: node-03-scheme-planner
scenario: ShippingRequirements
pair_kind: nine_node_flow
```

B:
  WorkBranches:
    - BranchId: "shipping-013-uc"; Scope: "3 delivery use cases"; Priority: 1
    - BranchId: "shipping-013-rules"; Scope: "Carrier eligibility business rules"; Priority: 2
    - BranchId: "shipping-013-ac"; Scope: "Acceptance criteria for delivery options"; Priority: 3
  BranchCount: 3; Confidence: 0.92

---

## B06-2.1-013-N09 — FormatterUnit: ShippingRequirements complete

```
source_id: xiigen-b06-2.1-013-n09
contract_signature: FormatterUnit : ILogicalUnit<ReviewedResultSet, HumanResult>
node_role: node-09-formatter
scenario: ShippingRequirements
pair_kind: nine_node_flow
```

B:
  FlowId: "requirements-authoring-flow-2.1"
  OverallVerdict: "PASS"
  FinalHumanResult: |
    Shipping requirements (sub-phase 2.1) — TransitionType: documentation:
    UC-S001: Standard delivery (3-5 business days) — address validation, carrier assignment.
    UC-S002: Express delivery (1-2 business days) — premium carrier, zone eligibility check.
    UC-S003: Overnight delivery (next day) — overnight carrier, cutoff time enforcement.
    Business rules: carrier eligibility by zone, weight tier limits, address validation pre-assignment.
    Acceptance criteria: delivery options visible for valid addresses; ineligible options hidden.
  Metrics:
    use_case_count: "3"
    business_rule_count: "3"
    transition_type: "documentation"
  Warnings: []
