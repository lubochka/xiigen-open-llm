# Layer 2 — Nine-Node Flow Pairs — Sub-Phase 2.1 (016)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: nine_node_flow

---

## Method note

File 016: TaxRequirementsDoc — DOCUMENT+ARCHITECTURE, TransitionType: documentation.
This file demonstrates the N01 side of a DOCUMENT+ARCHITECTURE scenario where
RequirementsAuthoringFlow21 handles the 2.1 transition.

---

## B06-2.1-016-N01 — DomainSplitterUnit: TaxRequirementsDoc (2.1 side)

```
source_id: xiigen-b06-2.1-016-n01
contract_signature: DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>
node_role: node-01-domain-splitter
scenario: TaxRequirementsDoc
pair_kind: nine_node_flow
```

A:
  TransitionId: "transition-tax-016-001"
  TransitionType: "documentation"
  SubPhaseId: "2.1"
  PredicateIds: ["DOCUMENT", "ARCHITECTURE"]
  UserRequest: "Document the tax calculation requirements and design the architecture for the tax service integration."
  Note: This N01 covers the 2.1 (DOCUMENT) transition only.
    ArchitectureFlow31 handles the 3.1 (ARCHITECTURE) transition in parallel.

B:
  DomainPackets:
    - PacketId: "tax-016-packet-document"
      DomainTag: "DOCUMENT"
      Focus: "Tax calculation requirements: jurisdiction determination, rate lookup, rounding rules, VAT, sales tax, GST"
      SubPhaseSignal: "2.1"
    - PacketId: "tax-016-packet-architecture"
      DomainTag: "ARCHITECTURE"
      Focus: "Tax service architecture context (passed to ArchitectureFlow31)"
      SubPhaseSignal: "3.1"
      Note: "Processed by separate flow"
  PacketCount: 2

---

## B06-2.1-016-N02 — DomainTranslatorUnit: tax requirements statement

```
source_id: xiigen-b06-2.1-016-n02
node_role: node-02-domain-translator
scenario: TaxRequirementsDoc
pair_kind: nine_node_flow
```

B:
  StatementId: "tax-016-stmt"
  TechnicalStatement: "Tax calculation requirements (2.1 side): (1) UC-T001 VAT (Value Added Tax) calculation for EU orders: rate by country; (2) UC-T002 US sales tax by state nexus; (3) UC-T003 GST (Goods and Services Tax) for CA/AU (Canada/Australia) orders; (4) UC-T004 Tax-exempt customer handling: exemption certificate validation; (5) Business rules: rounding to 2 decimal places; inclusive vs exclusive tax display per jurisdiction; line-item tax breakdown required; (6) Acceptance criteria: tax calculated within 100ms; displayed before payment confirmation."
  Confidence: 0.92
  Note: "2.1 side only — 3.1 architecture design covered by ArchitectureFlow31"

---

## B06-2.1-016-N03 — SchemePlannerUnit: tax requirements branches

```
source_id: xiigen-b06-2.1-016-n03
node_role: node-03-scheme-planner
scenario: TaxRequirementsDoc
pair_kind: nine_node_flow
```

B:
  WorkBranches:
    - BranchId: "tax-016-uc"; Scope: "4 tax jurisdiction use cases"; Priority: 1
    - BranchId: "tax-016-rules"; Scope: "Rounding and display business rules"; Priority: 2
    - BranchId: "tax-016-ac"; Scope: "Performance acceptance criteria"; Priority: 3
  BranchCount: 3; Confidence: 0.92

---

## B06-2.1-016-N09 — FormatterUnit: TaxRequirementsDoc complete (2.1 side)

```
source_id: xiigen-b06-2.1-016-n09
node_role: node-09-formatter
scenario: TaxRequirementsDoc
pair_kind: nine_node_flow
```

B:
  FlowId: "requirements-authoring-flow-2.1"
  OverallVerdict: "PASS"
  FinalHumanResult: |
    Tax calculation requirements (sub-phase 2.1) — TransitionType: documentation:
    UC-T001: VAT for EU orders (rate by country).
    UC-T002: US sales tax by state nexus.
    UC-T003: GST for CA/AU orders.
    UC-T004: Tax-exempt customer — certificate validation.
    Business rules: rounding to 2 decimal places; line-item breakdown required.
    Acceptance criteria: calculated < 100ms; displayed before payment confirmation.
    Note: Architecture flow (3.1) running concurrently with this flow.
  Metrics:
    use_case_count: "4"
    parallel_flows: "2 (2.1, 3.1)"
    transition_type: "documentation"
  Warnings: []
