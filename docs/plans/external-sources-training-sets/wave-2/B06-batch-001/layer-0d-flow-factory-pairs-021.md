# Layer 0-D — Flow Factory Selection Pairs (021)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## Method note

File 021 covers FraudDetection factory selection — three pairs.

---

## B06-0D-021-001 — FraudDetection: DOCUMENT → RequirementsAuthoringFlow21

```
source_id: xiigen-b06-0d-021-001
source_license_state: first_party
pair_kind: factory_selection
```

A:
  RoutingDecision:
    PredictedSubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]
    Context: "FraudDetection requirements"

B:
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  FlowAlias: "RequirementsAuthoringFlow21"
  TrustState: "trusted_learned"; FallbackUsed: false; Warnings: []

---

## B06-0D-021-002 — FraudDetection: DOCUMENT+IMPLEMENT → two flows

```
source_id: xiigen-b06-0d-021-002
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

## B06-0D-021-003 — FraudDetection: no "security flow" exists; 2.1 is correct

```
source_id: xiigen-b06-0d-021-003
source_license_state: first_party
pair_kind: factory_selection
```

Factory anti-pattern for fraud and security domains:

  Anti-pattern: Selecting a non-existent "security-requirements-flow-2.3" for fraud detection.
  Correct: FraudDetection is a business requirement domain → 2.1 → RequirementsAuthoringFlow21.

  Sub-phase reference for clarification:
    2.1 Business Requirements and Use Cases — includes fraud detection, data privacy,
      compliance, and security requirements when expressed as use cases and business rules.
    2.3 Non-Functional Requirements — includes performance, scalability, availability.
    No sub-phase 2.5 or "security" sub-phase exists in the current flow map.

  Anti-pattern: Routing fraud detection requirements to 2.3.
    Correct only if: The request explicitly frames the need as a non-functional
    security requirement (e.g., "the system must withstand X fraud attempts per second").
    For use cases and business rules framing → 2.1 is correct.
