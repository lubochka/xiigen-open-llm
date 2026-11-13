# Layer 0-D — Flow Factory Selection Pairs (035)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B06-0D-035-001 — DigitalProductDelivery: DOCUMENT → RequirementsAuthoringFlow21

```
source_id: xiigen-b06-0d-035-001
source_license_state: first_party
pair_kind: factory_selection
```

A:
  RoutingDecisions: [SubPhaseId: "2.1"]

B:
  Flow_2_1:
    SelectedFlowId: "universal-nine-node-development-flow-2.1"
    FlowAlias: "RequirementsAuthoringFlow21"
    TrustState: "trusted_learned"
    FallbackUsed: false
    Warnings: []

---

## B06-0D-035-002 — DigitalProductDelivery: three-predicate → three flows

```
source_id: xiigen-b06-0d-035-002
source_license_state: first_party
pair_kind: factory_selection
```

A:
  RoutingDecisions: [SubPhaseId: "2.1", SubPhaseId: "3.1", SubPhaseId: "4.1"]

B:
  Flow_2_1:
    SelectedFlowId: "universal-nine-node-development-flow-2.1"
    FlowAlias: "RequirementsAuthoringFlow21"
    TrustState: "trusted_learned"
  Flow_3_1:
    SelectedFlowId: "universal-nine-node-development-flow-3.1"
    FlowAlias: "ArchitectureFlow31"
    TrustState: "deterministic_generic"
  Flow_4_1:
    SelectedFlowId: "universal-nine-node-development-flow-4.1"
    FlowAlias: "ImplementationFlow41"
    TrustState: "deterministic_generic"

---

## B06-0D-035-003 — DigitalProductDelivery: knowledge-edge → no flows

```
source_id: xiigen-b06-0d-035-003
source_license_state: first_party
pair_kind: factory_selection
```

A:
  RoutingDecisions: []
  KnowledgeEdge: true
  Context: "Email the download link to the customer after payment."

B:
  SelectedFlows: []
  FlowCount: 0
  KnowledgeEdge: true
  PipelineAction: "Escalate to knowledge-edge handler. No flow selected."
  Teaching: When KnowledgeEdge is true, DecisionCount: 0, and the factory produces zero flows.
    This is correct behavior — not an error. The pipeline takes a different path.
