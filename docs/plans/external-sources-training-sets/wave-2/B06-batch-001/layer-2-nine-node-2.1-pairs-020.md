# Layer 2 — Nine-Node Flow Pairs — Sub-Phase 2.1 (020)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: nine_node_flow

---

## Method note

File 020: FraudDetectionRequirements — DOCUMENT+SECURITY framing (compiler produces
DOCUMENT alone — no SECURITY predicate in lexicon). TransitionType: documentation.

---

## B06-2.1-020-N01 — DomainSplitterUnit: FraudDetectionRequirements

```
source_id: xiigen-b06-2.1-020-n01
contract_signature: DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>
node_role: node-01-domain-splitter
scenario: FraudDetectionRequirements
pair_kind: nine_node_flow
```

A:
  TransitionId: "transition-fraud-020-001"
  TransitionType: "documentation"
  SubPhaseId: "2.1"
  PredicateIds: ["DOCUMENT"]
  UserRequest: "Define the fraud detection requirements for checkout, covering velocity checks, device fingerprinting, and 3DS challenge triggers."
  Note: Scenario label includes "+SECURITY" but SECURITY is not a predicate.
    PredicateIds: ["DOCUMENT"] — fraud detection is handled as a DOCUMENT requirement.
    Security context is handled within the nine-node flow via DomainTranslatorUnit.

B:
  DomainPackets:
    - PacketId: "fraud-020-packet-document"
      DomainTag: "DOCUMENT"
      Focus: "Fraud detection requirements: risk scoring rules, velocity checks, device fingerprinting, 3DS (3-D Secure) challenge triggers, decline rules"
      SubPhaseSignal: "2.1"
      SecurityContext: "fraud detection and risk management requirements"
  PacketCount: 1

---

## B06-2.1-020-N02 — DomainTranslatorUnit: fraud detection statement

```
source_id: xiigen-b06-2.1-020-n02
node_role: node-02-domain-translator
scenario: FraudDetectionRequirements
pair_kind: nine_node_flow
```

B:
  StatementId: "fraud-020-stmt"
  TechnicalStatement: "Fraud detection requirements: (1) UC-F001 Velocity check: max 3 payment attempts per session; 5 per 24h per account; (2) UC-F002 Device fingerprinting: new device triggers 3DS (3-D Secure) challenge; trusted device list maintained per account; (3) UC-F003 High-risk transaction: score > 80 → auto-decline; score 60-80 → manual review queue; (4) UC-F004 Chargeback dispute evidence collection: order data, device fingerprint, session log captured at purchase; (5) Business rules: 3DS required for transactions > $500; new shipping address + new payment method → forced 3DS; (6) Acceptance criteria: fraud check < 200ms; 3DS challenge presented within 500ms of high-risk signal."
  Confidence: 0.92

---

## B06-2.1-020-N03 — SchemePlannerUnit: fraud detection branches

```
source_id: xiigen-b06-2.1-020-n03
node_role: node-03-scheme-planner
scenario: FraudDetectionRequirements
pair_kind: nine_node_flow
```

B:
  WorkBranches:
    - BranchId: "fraud-020-uc"; Scope: "4 fraud detection use cases"; Priority: 1
    - BranchId: "fraud-020-rules"; Scope: "3DS trigger and velocity business rules"; Priority: 2
    - BranchId: "fraud-020-ac"; Scope: "Performance acceptance criteria"; Priority: 3
  BranchCount: 3; Confidence: 0.92

---

## B06-2.1-020-N09 — FormatterUnit: FraudDetectionRequirements complete

```
source_id: xiigen-b06-2.1-020-n09
node_role: node-09-formatter
scenario: FraudDetectionRequirements
pair_kind: nine_node_flow
```

B:
  FlowId: "requirements-authoring-flow-2.1"
  OverallVerdict: "PASS"
  FinalHumanResult: |
    Fraud detection requirements (sub-phase 2.1) — TransitionType: documentation:
    UC-F001: Velocity checks (3/session, 5/24h per account).
    UC-F002: Device fingerprinting → 3DS for new device.
    UC-F003: Risk scoring: > 80 auto-decline; 60-80 manual review.
    UC-F004: Chargeback evidence collection at purchase.
    Business rules: 3DS required > $500; new device + new address → forced 3DS.
    Acceptance criteria: fraud check < 200ms; 3DS challenge < 500ms.
  Metrics:
    use_case_count: "4"
    transition_type: "documentation"
    security_context: "fraud detection requirements"
  Warnings: []
