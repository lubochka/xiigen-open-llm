# Layer 0-D — Flow Factory Selection Pairs (008)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## Method note

File 008 covers DOCUMENT + ARCHITECTURE factory: RequirementsAuthoringFlow21 + ArchitectureFlow31.

---

## B06-0D-008-001 — DOCUMENT+ARCHITECTURE factory: 2.1 + 3.1

```
source_id: xiigen-b06-0d-008-001
source_title: XIIGen B06 factory — DOCUMENT+ARCHITECTURE two flow instantiation
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
pair_kind: factory_selection
```

A:
  routing_decisions:
    - sub_phase_id: "2.1"
      evidence_predicate: "DOCUMENT"
      flow_registry_state:
        registered_flow_id: null
      fallback_flow_id: "universal-nine-node-development-flow-2.1"

    - sub_phase_id: "3.1"
      evidence_predicate: "ARCHITECTURE"
      flow_registry_state:
        registered_flow_id: null
      fallback_flow_id: "universal-nine-node-development-flow-3.1"

B:
  FlowInstantiations:
    - sub_phase_id: "2.1"
      SelectedFlowId: "universal-nine-node-development-flow-2.1"
      TrustState: "deterministic_generic"
      FallbackUsed: false
      SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"
      Warnings: []

    - sub_phase_id: "3.1"
      SelectedFlowId: "universal-nine-node-development-flow-3.1"
      TrustState: "deterministic_generic"
      FallbackUsed: false
      SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"
      Warnings: []

  InstantiationCount: 2
  TransitionType: "documentation"

Note: DOCUMENT + ARCHITECTURE without IMPLEMENT → TransitionType: documentation.

---

## B06-0D-008-002 — ArchitectureFlow31 identity

```
source_id: xiigen-b06-0d-008-002
source_title: XIIGen B06 factory — ArchitectureFlow31 identity
source_license_state: first_party
pair_kind: factory_selection
```

Teaching note — ArchitectureFlow31:

  FlowId: "universal-nine-node-development-flow-3.1"
  Also referenced as: "ArchitectureFlow31"
  Sub-phase: 3.1 (Technical Architecture and System Design)
  Purpose: Document system architecture; define component relationships;
    create architectural diagrams; specify service boundaries

  B06 does not deeply cover ArchitectureFlow31 execution (that is covered
  in B36 or B37 batches). In B06, ArchitectureFlow31 appears only in
  factory selection pairs when ARCHITECTURE predicate fires alongside DOCUMENT.
  The factory produces the instantiation; execution is out-of-scope for B06.

---

## B06-0D-008-003 — documentation TransitionType: DOCUMENT+ARCHITECTURE only

```
source_id: xiigen-b06-0d-008-003
source_title: XIIGen B06 factory — documentation TransitionType
source_license_state: first_party
pair_kind: factory_selection
```

Teaching note — documentation TransitionType:

  TransitionType: "documentation" is assigned when:
    CompiledPredicateIds contains only DOCUMENT and/or ARCHITECTURE
    No IMPLEMENT, no UI

  documentation TransitionType means: the output artifacts are documentation
  or design artifacts only. No new code is being written in this transition.

  Examples in B06:
    - "Describe the checkout requirements" → DOCUMENT → documentation
    - "Design the architecture and document the requirements" → ARCHITECTURE+DOCUMENT → documentation
    - "Define the use cases" → DOCUMENT → documentation

  Contrast:
    - "Write the requirements" → DOCUMENT+IMPLEMENT → feature_addition
    - "Design the architecture and implement the service" → ARCHITECTURE+IMPLEMENT → feature_addition
