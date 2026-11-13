# Layer 0-D — Flow Factory Selection Pairs (018)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## Method note

File 018 covers MultiCurrency factory selection — three pairs.

---

## B06-0D-018-001 — MultiCurrency: DOCUMENT → RequirementsAuthoringFlow21

```
source_id: xiigen-b06-0d-018-001
source_license_state: first_party
pair_kind: factory_selection
```

A:
  RoutingDecision:
    PredictedSubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]
    Context: "MultiCurrency requirements"

B:
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  FlowAlias: "RequirementsAuthoringFlow21"
  TrustState: "trusted_learned"; FallbackUsed: false; Warnings: []

---

## B06-0D-018-002 — MultiCurrency: DOCUMENT+IMPLEMENT → two flows

```
source_id: xiigen-b06-0d-018-002
source_license_state: first_party
pair_kind: factory_selection
```

A:
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]
    - PredictedSubPhaseId: "4.1"; EvidencePredicates: ["IMPLEMENT"]

B:
  SelectedFlows:
    - FlowId: "universal-nine-node-development-flow-2.1"; Alias: "RequirementsAuthoringFlow21"
      TrustState: "trusted_learned"; FallbackUsed: false
    - FlowId: "universal-nine-node-development-flow-4.1"; Alias: "ImplementationFlow41"
      TrustState: "deterministic_generic"; FallbackUsed: false
  FlowCount: 2

---

## B06-0D-018-003 — MultiCurrency: domain does not change flow selection

```
source_id: xiigen-b06-0d-018-003
source_license_state: first_party
pair_kind: factory_selection
```

Factory teaching:

  MultiCurrency domain specifics (currency types, exchange rates, FX markup)
  do NOT affect flow selection. The factory receives only the SubPhaseId and
  TrustState — domain context is NOT an input to the factory.

  Correct: PredictedSubPhaseId: "2.1" → RequirementsAuthoringFlow21 (always)
  Incorrect: Selecting a "CurrencyRequirementsFlow" that does not exist.

  The nine-node flow handles domain context internally (via N02 DomainTranslatorUnit).
  The factory only selects which nine-node instance to run based on sub-phase.
  DomainId: software-development is set on the transition, not used for flow selection.
