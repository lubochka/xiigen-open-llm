# Layer 2 — Nine-Node Flow Pairs — Sub-Phase 2.1 (018)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: nine_node_flow

---

## Method note

File 018: BundleDiscountRequirements — DOCUMENT alone, TransitionType: documentation.

---

## B06-2.1-018-N01 — DomainSplitterUnit: BundleDiscountRequirements

```
source_id: xiigen-b06-2.1-018-n01
contract_signature: DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>
node_role: node-01-domain-splitter
scenario: BundleDiscountRequirements
pair_kind: nine_node_flow
```

A:
  TransitionId: "transition-bundle-018-001"
  TransitionType: "documentation"
  SubPhaseId: "2.1"
  PredicateIds: ["DOCUMENT"]
  UserRequest: "Define the bundle and discount requirements for checkout, covering percentage discounts, coupons, and minimum order thresholds."

B:
  DomainPackets:
    - PacketId: "bundle-018-packet-document"
      DomainTag: "DOCUMENT"
      Focus: "Bundle and discount requirements: percentage discounts, fixed-amount coupons, BOGO (Buy One Get One) rules, minimum order thresholds, stacking rules"
      SubPhaseSignal: "2.1"
  PacketCount: 1

---

## B06-2.1-018-N02 — DomainTranslatorUnit: bundle discount statement

```
source_id: xiigen-b06-2.1-018-n02
node_role: node-02-domain-translator
scenario: BundleDiscountRequirements
pair_kind: nine_node_flow
```

B:
  StatementId: "bundle-018-stmt"
  TechnicalStatement: "Bundle and discount requirements: (1) UC-D001 Percentage discount via coupon code: 5%-50% range; single-use enforcement; (2) UC-D002 Fixed-amount coupon: min-order-value prerequisite; (3) UC-D003 BOGO (Buy One Get One) promotion: buy N get M free; bundle defined in product catalog; (4) Stacking rules: maximum 1 coupon per order; loyalty discount stackable with coupons; sale prices not stackable with coupons; (5) Acceptance criteria: AC-D001 invalid/expired coupon returns specific error message; AC-D002 discount applied to eligible items only (excludes sale items); AC-D003 total recalculates in real time on coupon application."
  Confidence: 0.92

---

## B06-2.1-018-N03 — SchemePlannerUnit: bundle discount branches

```
source_id: xiigen-b06-2.1-018-n03
node_role: node-03-scheme-planner
scenario: BundleDiscountRequirements
pair_kind: nine_node_flow
```

B:
  WorkBranches:
    - BranchId: "bundle-018-uc"; Scope: "3 discount use cases (% coupon, fixed, BOGO)"; Priority: 1
    - BranchId: "bundle-018-stacking"; Scope: "Discount stacking rules"; Priority: 2
    - BranchId: "bundle-018-ac"; Scope: "Acceptance criteria (AC-D001 through AC-D003)"; Priority: 3
  BranchCount: 3; Confidence: 0.92

---

## B06-2.1-018-N09 — FormatterUnit: BundleDiscountRequirements complete

```
source_id: xiigen-b06-2.1-018-n09
node_role: node-09-formatter
scenario: BundleDiscountRequirements
pair_kind: nine_node_flow
```

B:
  FlowId: "requirements-authoring-flow-2.1"
  OverallVerdict: "PASS"
  FinalHumanResult: |
    Bundle and discount requirements (sub-phase 2.1) — TransitionType: documentation:
    UC-D001: % coupon (5-50%); single-use enforcement.
    UC-D002: Fixed-amount coupon; min order value required.
    UC-D003: BOGO promotion; catalog-defined bundles.
    Stacking: 1 coupon max; loyalty stackable; sale prices not stackable.
    Acceptance criteria: invalid coupon error message; discount on eligible items only; real-time total recalculation.
  Metrics:
    use_case_count: "3"
    acceptance_criteria_count: "3"
    transition_type: "documentation"
  Warnings: []
