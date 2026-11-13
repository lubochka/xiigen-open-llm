# Layer 2 — Nine-Node Flow Pairs — Sub-Phase 2.1 (009)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: nine_node_flow

---

## Method note

File 009 covers StakeholderRequirements: external stakeholder requirements (DOCUMENT → 2.1 + 2.4).
This file covers the 2.1 side of the dual routing. The 2.4 StakeholderRequirementsFlow24
is described but its nine-node execution is referenced, not fully expanded here.

---

## B06-2.1-009-N01 — Node 01 DomainSplitterUnit: StakeholderRequirements (2.1 side)

```
source_id: xiigen-b06-2.1-009-n01
contract_signature: DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>
transition_type: documentation
phase: 2
sub_phase: 2.1
node_role: node-01-domain-splitter
scenario: StakeholderRequirements
pair_kind: nine_node_flow
```

A:
  TransitionId: "transition-B06-stakeholder-requirements-2.1"
  TransitionType: "documentation"
  SubPhaseId: "2.1"
  UserRequest: "Define the checkout use cases and document the stakeholder requirements for external payment partners and marketplace vendors."
  PredicateIds: ["DOCUMENT"]
  Note: Dual routing — this N01 is for the 2.1 transition. A separate N01 is invoked
    for the 2.4 transition via StakeholderRequirementsFlow24.

B:
  DomainPackets:
    - PacketId: "B06-2.1-009-packet-document-2.1"
      DomainTag: "DOCUMENT"
      Focus: "Checkout use cases for external payment partners and marketplace vendors — partner checkout integration use cases, vendor fulfillment requirements, cross-system business rules"
      SubPhaseSignal: "2.1"
      UserSignal: "define, use cases, checkout"

  PacketCount: 1
  Confidence: 0.92

Note — 2.4 transition:
  The DOCUMENT predicate also routes to 2.4 (StakeholderRequirementsFlow24)
  due to "stakeholder requirements", "external payment partners", "marketplace vendors".
  The 2.4 flow runs independently. Both flows receive the same UserRequest.

---

## B06-2.1-009-N02 — Node 02 DomainTranslatorUnit: partner use cases

```
source_id: xiigen-b06-2.1-009-n02
contract_signature: DomainTranslatorUnit : ILogicalUnit<DomainPacket, TranslatedDomainStatement>
node_role: node-02-domain-translator
scenario: StakeholderRequirements
pair_kind: nine_node_flow
```

B:
  StatementId: "B06-2.1-009-stmt-document-2.1"
  TechnicalStatement: "Checkout use cases from the perspective of external partners: (1) Payment Partner Integration: payment gateway is an external partner; use case covers: auth request, capture, void, refund APIs (Application Programming Interfaces); partner SLA (Service-Level Agreement): auth response < 2s; (2) Marketplace Vendor Fulfillment: each item in cart may have a different marketplace vendor; use case covers: order split by vendor, fulfillment notification per vendor, vendor tracking number per split; (3) Shipping Carrier Integration: shipping carrier provides tracking; use case covers: tracking number assignment, status update webhook, delivery confirmation webhook; (4) Tax and Compliance Service: third-party tax service calculates applicable taxes; use case covers: cart total + address → tax calculation request; tax rate applied to checkout total. Business rules: BR-S01 (payment partner auth < 2s SLA); BR-S02 (order split at vendor level); BR-S03 (tracking per split shipment)."
  KeyTerms: ["payment partner", "marketplace vendor", "order split", "shipping carrier", "tax service", "SLA"]
  Confidence: 0.92

---

## B06-2.1-009-N03 — Node 03 SchemePlannerUnit: stakeholder use case branches

```
source_id: xiigen-b06-2.1-009-n03
contract_signature: SchemePlannerUnit : ILogicalUnit<TranslatedDomainStatement[], WorkBranch[]>
node_role: node-03-scheme-planner
scenario: StakeholderRequirements
pair_kind: nine_node_flow
```

B:
  WorkBranches:
    - BranchId: "B06-2.1-009-branch-payment-partner"   ; Scope: "Payment gateway partner use case"   ; Priority: 1
    - BranchId: "B06-2.1-009-branch-vendor-fulfill"    ; Scope: "Marketplace vendor fulfillment"      ; Priority: 2
    - BranchId: "B06-2.1-009-branch-shipping-carrier"  ; Scope: "Shipping carrier integration"        ; Priority: 3
    - BranchId: "B06-2.1-009-branch-tax-service"       ; Scope: "Tax and compliance service"          ; Priority: 4
    - BranchId: "B06-2.1-009-branch-rules"             ; Scope: "3 business rules BR-S01/S02/S03"     ; Priority: 5

  BranchCount: 5
  Confidence: 0.92

---

## B06-2.1-009-N09 — Node 09 FormatterUnit: StakeholderRequirements 2.1 side complete

```
source_id: xiigen-b06-2.1-009-n09
contract_signature: FormatterUnit : ILogicalUnit<ReviewedResultSet, HumanResult>
node_role: node-09-formatter
scenario: StakeholderRequirements
pair_kind: nine_node_flow
```

B:
  FlowId: "requirements-authoring-flow-2.1"
  OverallVerdict: "PASS"
  FinalHumanResult: |
    External partner checkout use cases (sub-phase 2.1 — 2.1 routing side):

    UC-EXT-001 Payment Partner Integration: auth/capture/void/refund API calls; SLA:
      auth < 2s. BR-S01.

    UC-EXT-002 Marketplace Vendor Fulfillment: cart split by vendor; fulfillment
      notification and tracking per vendor split. BR-S02.

    UC-EXT-003 Shipping Carrier Integration: tracking number assignment; delivery
      confirmation webhook; status update webhook.

    UC-EXT-004 Tax and Compliance Service: tax calculation on cart + address;
      tax rate applied to checkout total before payment.

    3 business rules (BR-S01 through BR-S03).
    Note: 2.4 routing also produced — StakeholderRequirementsFlow24 ran separately
    and produced the formal stakeholder requirements document for external partners.
    See layer-2 2.4 pairs for 2.4 flow output.
    Arbiter reviews: PASS (4 partner use cases), PASS (SLA testability).

  Metrics:
    partner_use_case_count: "4"
    business_rule_count: "3"
    routing_note: "2.1 + 2.4 dual routing; this file covers 2.1 side"
  Warnings: []
