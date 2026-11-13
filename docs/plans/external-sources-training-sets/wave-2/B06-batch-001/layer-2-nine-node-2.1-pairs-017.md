# Layer 2 — Nine-Node Flow Pairs — Sub-Phase 2.1 (017)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: nine_node_flow

---

## Method note

File 017: MultiCurrencySpec — DOCUMENT alone, TransitionType: documentation.

---

## B06-2.1-017-N01 — DomainSplitterUnit: MultiCurrencySpec

```
source_id: xiigen-b06-2.1-017-n01
contract_signature: DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>
node_role: node-01-domain-splitter
scenario: MultiCurrencySpec
pair_kind: nine_node_flow
```

A:
  TransitionId: "transition-currency-017-001"
  TransitionType: "documentation"
  SubPhaseId: "2.1"
  PredicateIds: ["DOCUMENT"]
  UserRequest: "Specify the multi-currency spec for the payment gateway integration."

B:
  DomainPackets:
    - PacketId: "currency-017-packet-document"
      DomainTag: "DOCUMENT"
      Focus: "Multi-currency specification: supported currencies, precision rules, display vs settlement currency, FX markup policy, refund currency handling"
      SubPhaseSignal: "2.1"
  PacketCount: 1

---

## B06-2.1-017-N02 — DomainTranslatorUnit: multi-currency statement

```
source_id: xiigen-b06-2.1-017-n02
node_role: node-02-domain-translator
scenario: MultiCurrencySpec
pair_kind: nine_node_flow
```

B:
  StatementId: "currency-017-stmt"
  TechnicalStatement: "Multi-currency specification: (1) Supported currencies: USD, EUR, GBP, CAD, AUD, JPY (6 initial); (2) Display vs settlement: display in customer locale currency; settle in merchant base currency; (3) FX (Foreign Exchange) markup: 1.5% markup on interbank rate; disclosed at checkout; (4) Precision rules: 2 decimal places for USD/EUR/GBP; 0 decimal places for JPY; (5) Refund currency: refunded in original payment currency; FX markup reversed on full refund; (6) Acceptance criteria: currency auto-detected on page load; customer can override; rate displayed with refresh timestamp."
  Confidence: 0.92

---

## B06-2.1-017-N03 — SchemePlannerUnit: currency branches

```
source_id: xiigen-b06-2.1-017-n03
node_role: node-03-scheme-planner
scenario: MultiCurrencySpec
pair_kind: nine_node_flow
```

B:
  WorkBranches:
    - BranchId: "currency-017-supported"; Scope: "Supported currencies and precision rules"; Priority: 1
    - BranchId: "currency-017-display-settle"; Scope: "Display vs settlement currency logic"; Priority: 2
    - BranchId: "currency-017-refund"; Scope: "Refund currency and FX reversal rules"; Priority: 3
    - BranchId: "currency-017-ac"; Scope: "Auto-detection acceptance criteria"; Priority: 4
  BranchCount: 4; Confidence: 0.92

---

## B06-2.1-017-N09 — FormatterUnit: MultiCurrencySpec complete

```
source_id: xiigen-b06-2.1-017-n09
node_role: node-09-formatter
scenario: MultiCurrencySpec
pair_kind: nine_node_flow
```

B:
  FlowId: "requirements-authoring-flow-2.1"
  OverallVerdict: "PASS"
  FinalHumanResult: |
    Multi-currency spec (sub-phase 2.1) — TransitionType: documentation:
    Currencies: USD, EUR, GBP, CAD, AUD, JPY. Precision: 2 decimal places except JPY (0).
    Display: customer locale; Settlement: merchant base currency.
    FX markup: 1.5% on interbank rate; disclosed at checkout.
    Refund: original currency; FX markup reversed on full refund.
    Acceptance criteria: auto-detected on load; customer override available; rate shown with timestamp.
  Metrics:
    currency_count: "6"
    transition_type: "documentation"
  Warnings: []
