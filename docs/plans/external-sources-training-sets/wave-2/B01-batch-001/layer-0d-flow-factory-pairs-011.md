# Layer 0-D — Flow Factory Selection Pairs (011)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B01-0D-011-001 — ARCHITECTURE + UI factory: SystemArchitectureFlow31 + ScreenInventoryFlow11

```
source_id: xiigen-b01-0d-011-001
source_title: XIIGen B01 factory-011 — ARCHITECTURE+UI two-flow selection
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
sub_phase_id: "1.1 + 3.1"
pair_kind: factory_selection
```

A:
  predicate_context: ["UI", "ARCHITECTURE"]
  routing_decisions:
    - sub_phase_id: "1.1", evidence_predicates: ["UI"]
    - sub_phase_id: "3.1", evidence_predicates: ["ARCHITECTURE"]

  Sub-phase 1.1: registered_flow_id: null; fallback: "ScreenInventoryFlow11"
  Sub-phase 3.1: registered_flow_id: null; fallback: "SystemArchitectureFlow31"

B:
  FlowSelections:
    - SubPhaseId: "1.1"
      SelectedFlowId: "ScreenInventoryFlow11"
      TrustState: "deterministic_generic"
      FallbackUsed: false
      Warnings: []
      SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

    - SubPhaseId: "3.1"
      SelectedFlowId: "SystemArchitectureFlow31"
      TrustState: "deterministic_generic"
      FallbackUsed: false
      Warnings: []
      SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

  TotalFlowsSelected: 2
  TransitionType: documentation

Factory logic:
  ARCHITECTURE+UI without IMPLEMENT → documentation TransitionType. Two flows
  selected: ScreenInventoryFlow11 for UX screens (1.1) and SystemArchitectureFlow31
  for architecture diagrams (3.1). No implementation flow selected because IMPLEMENT
  was not in the predicate list.

---

## B01-0D-011-002 — SystemArchitectureFlow31 identity

```
source_id: xiigen-b01-0d-011-002
source_title: XIIGen B01 factory-011 — SystemArchitectureFlow31 identity
source_license_state: first_party
pair_kind: factory_selection
coverage_status: covered
```

SystemArchitectureFlow31 identity:

  FlowId: "SystemArchitectureFlow31"
  FlowName: "System Architecture Flow for Sub-Phase 3.1"
  Phase: 3
  SubPhase: 3.1
  NodeCount: 9
  TrustState: deterministic_generic

  Node chain (for architecture work):
    Node 01 — DomainSplitterUnit: SelectedLogicalTransition → DomainPacket[] (ARCHITECTURE domain)
    Node 02 — DomainTranslatorUnit: DomainPacket → TranslatedDomainStatement (architecture statements)
    Node 03 — SchemePlannerUnit: statements → WorkBranch[] (architecture components)
    Node 04 — DomainProcessorUnit: WorkBranch → ProcessedBranchResult (system component specs)
    Node 05 — ApplierUnit: ProcessedBranchResult → AppliedBranchResult
    Node 06 — ValidationUnit: all results → ValidationReport (architecture integrity checks)
    Node 07 — IntegrationUnit: ValidationReport → IntegratedFlowResult
    Node 08 — QualityUnit: IntegratedFlowResult → QualityMetrics
    Node 09 — FormatterUnit: QualityMetrics → FinalHumanResult (architecture document)

---

## B01-0D-011-003 — All four flows in B01: complete reference

```
source_id: xiigen-b01-0d-011-003
source_title: XIIGen B01 factory-011 — complete B01 flow reference
source_license_state: first_party
pair_kind: factory_selection
coverage_status: covered
```

Complete flow reference for B01:

| Flow ID | Sub-phase | Predicate | Purpose |
|---|---|---|---|
| ScreenInventoryFlow11 | 1.1 | UI | Screen mapping and UX definition |
| BusinessRequirementsFlow21 | 2.1 | DOCUMENT | Requirements and use-case documents |
| SystemArchitectureFlow31 | 3.1 | ARCHITECTURE | Architecture diagrams and system design |
| ImplementationFlow41 | 4.1 | IMPLEMENT | Code and component implementation |
| knowledge-edge-missing-flow | null | (none) | Synthetic flow for no-predicate results |

All five are nine-node flows except knowledge-edge-missing-flow, which produces
a synthetic result without executing nodes.

Each flow is independently registered. The factory evaluates each flow's trust
state separately. A trusted_learned state for one flow does not affect another.
