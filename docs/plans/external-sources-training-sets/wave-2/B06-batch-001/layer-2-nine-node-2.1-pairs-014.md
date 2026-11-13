# Layer 2 — Nine-Node Flow Pairs — Sub-Phase 2.1 (014)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: nine_node_flow

---

## Method note

File 014: LoyaltyProgramSpec — DOCUMENT alone, TransitionType: documentation.

---

## B06-2.1-014-N01 — DomainSplitterUnit: LoyaltyProgramSpec

```
source_id: xiigen-b06-2.1-014-n01
contract_signature: DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>
node_role: node-01-domain-splitter
scenario: LoyaltyProgramSpec
pair_kind: nine_node_flow
```

A:
  TransitionId: "transition-loyalty-014-001"
  TransitionType: "documentation"
  SubPhaseId: "2.1"
  PredicateIds: ["DOCUMENT"]
  UserRequest: "Specify the feature-brief for loyalty rewards at checkout, including acceptance criteria for points balance display and redemption confirmation."

B:
  DomainPackets:
    - PacketId: "loyalty-014-packet-document"
      DomainTag: "DOCUMENT"
      Focus: "Loyalty program feature brief: points balance display, redemption flow, acceptance criteria for member and non-member checkout paths"
      SubPhaseSignal: "2.1"
  PacketCount: 1
  SelectedLogicalTransition: "documentation"

---

## B06-2.1-014-N02 — DomainTranslatorUnit: loyalty spec statement

```
source_id: xiigen-b06-2.1-014-n02
contract_signature: DomainTranslatorUnit : ILogicalUnit<DomainPacket, TranslatedDomainStatement>
node_role: node-02-domain-translator
scenario: LoyaltyProgramSpec
pair_kind: nine_node_flow
```

B:
  StatementId: "loyalty-014-stmt"
  TechnicalStatement: "Loyalty program checkout feature brief: (1) Points balance display: member sees available points before applying; balance refreshed at cart load; (2) Points redemption: member selects redemption amount; partial redemption allowed; minimum redemption threshold enforced; (3) Non-member path: join prompt displayed; no redemption available; (4) Acceptance criteria: AC-L001 balance displayed within 200ms of cart load; AC-L002 redemption confirmed with updated total before payment; AC-L003 insufficient-points error shown with required-points calculation."
  KeyTerms: ["points balance", "redemption", "partial redemption", "minimum threshold", "non-member path"]
  Confidence: 0.92

---

## B06-2.1-014-N03 — SchemePlannerUnit: loyalty branches

```
source_id: xiigen-b06-2.1-014-n03
node_role: node-03-scheme-planner
scenario: LoyaltyProgramSpec
pair_kind: nine_node_flow
```

B:
  WorkBranches:
    - BranchId: "loyalty-014-display"; Scope: "Points balance display use case"; Priority: 1
    - BranchId: "loyalty-014-redemption"; Scope: "Redemption flow use case"; Priority: 2
    - BranchId: "loyalty-014-ac"; Scope: "Acceptance criteria (AC-L001 through AC-L003)"; Priority: 3
  BranchCount: 3; Confidence: 0.92

---

## B06-2.1-014-N09 — FormatterUnit: LoyaltyProgramSpec complete

```
source_id: xiigen-b06-2.1-014-n09
node_role: node-09-formatter
scenario: LoyaltyProgramSpec
pair_kind: nine_node_flow
```

B:
  FlowId: "requirements-authoring-flow-2.1"
  OverallVerdict: "PASS"
  FinalHumanResult: |
    Loyalty program checkout feature brief (sub-phase 2.1) — TransitionType: documentation:
    Points balance display: available points shown at cart load, refreshed < 200ms.
    Redemption flow: member selects redemption amount; partial redemption supported;
      minimum threshold enforced; updated total displayed before payment.
    Non-member path: join prompt; no redemption.
    Acceptance criteria: AC-L001 display < 200ms, AC-L002 confirmation before payment,
      AC-L003 insufficient-points error with required-amount calculation.
  Metrics:
    acceptance_criteria_count: "3"
    transition_type: "documentation"
  Warnings: []
