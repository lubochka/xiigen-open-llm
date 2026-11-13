# Layer 2 — Nine-Node Flow Pairs — Sub-Phase 2.1 (022)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: nine_node_flow

---

## Method note

File 022: StakeholderShipping — DOCUMENT routes to 2.1+2.4 (stakeholder signal).
TransitionType: documentation. This file covers the 2.1 side of the nine-node flow.
The 2.4 side runs concurrently in StakeholderRequirementsFlow24.

---

## B06-2.1-022-N01 — DomainSplitterUnit: StakeholderShipping (2.1 side)

```
source_id: xiigen-b06-2.1-022-n01
contract_signature: DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>
node_role: node-01-domain-splitter
scenario: StakeholderShipping
pair_kind: nine_node_flow
```

A:
  TransitionId: "transition-stkhdr-shipping-022-001"
  TransitionType: "documentation"
  SubPhaseId: "2.1"
  PredicateIds: ["DOCUMENT"]
  UserRequest: "Describe the stakeholder requirements for shipping carrier integration, covering logistics partner, ops team, and customer-facing perspectives."
  Note: "stakeholder" term present → routes to 2.1 AND 2.4. This nine-node covers 2.1 only.
    StakeholderRequirementsFlow24 handles 2.4 side concurrently.

B:
  DomainPackets:
    - PacketId: "stkhdr-shipping-022-packet-2.1"
      DomainTag: "DOCUMENT"
      Focus: "Business requirements for shipping carrier integration: API capabilities, SLA (Service-Level Agreement) requirements, rate calculation use cases"
      SubPhaseSignal: "2.1"
    - PacketId: "stkhdr-shipping-022-packet-2.4"
      DomainTag: "DOCUMENT"
      Focus: "Stakeholder requirements: logistics partner needs, ops team workflow, customer shipping experience"
      SubPhaseSignal: "2.4"
      Note: "Processed by StakeholderRequirementsFlow24"
  PacketCount: 2

---

## B06-2.1-022-N02 — DomainTranslatorUnit: stakeholder shipping statement (2.1 side)

```
source_id: xiigen-b06-2.1-022-n02
node_role: node-02-domain-translator
scenario: StakeholderShipping
pair_kind: nine_node_flow
```

B:
  StatementId: "stkhdr-shipping-022-stmt-2.1"
  TechnicalStatement: "Shipping carrier integration business requirements (2.1 side): (1) UC-SS001 Carrier rate lookup API integration: real-time rate retrieval per carrier; (2) UC-SS002 Carrier SLA compliance: on-time delivery rate >= 98% required; carrier removed if SLA breached 3 consecutive months; (3) UC-SS003 Customer carrier preference: customer can select preferred carrier if multiple available; (4) Business rules: carrier eligibility by destination zone; weight and dimension limits per carrier; insurance required on orders > $200."
  Confidence: 0.92

---

## B06-2.1-022-N03 — SchemePlannerUnit: stakeholder shipping branches

```
source_id: xiigen-b06-2.1-022-n03
node_role: node-03-scheme-planner
scenario: StakeholderShipping
pair_kind: nine_node_flow
```

B:
  WorkBranches:
    - BranchId: "stkhdr-022-uc"; Scope: "3 carrier integration use cases (2.1 side)"; Priority: 1
    - BranchId: "stkhdr-022-sla"; Scope: "Carrier SLA business rules"; Priority: 2
    - BranchId: "stkhdr-022-ac"; Scope: "Acceptance criteria"; Priority: 3
  BranchCount: 3; Confidence: 0.92

---

## B06-2.1-022-N09 — FormatterUnit: StakeholderShipping (2.1 side) complete

```
source_id: xiigen-b06-2.1-022-n09
node_role: node-09-formatter
scenario: StakeholderShipping
pair_kind: nine_node_flow
```

B:
  FlowId: "requirements-authoring-flow-2.1"
  OverallVerdict: "PASS"
  FinalHumanResult: |
    Stakeholder shipping requirements — 2.1 side (sub-phase 2.1) — TransitionType: documentation:
    UC-SS001: Carrier rate lookup API — real-time rates per carrier.
    UC-SS002: Carrier SLA compliance — >= 98% on-time; removal after 3 consecutive breaches.
    UC-SS003: Customer carrier preference selection.
    Business rules: zone eligibility; weight/dimension limits; insurance > $200.
    Note: 2.4 side (stakeholder perspectives) processed concurrently by StakeholderRequirementsFlow24.
  Metrics:
    use_case_count: "3"
    parallel_flows: "2 (2.1, 2.4)"
    transition_type: "documentation"
  Warnings: []
