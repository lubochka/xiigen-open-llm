# Layer 0-D — Flow Factory Selection Pairs (007)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B01-0D-007-001 — UI + IMPLEMENT factory: ScreenInventoryFlow11 + ImplementationFlow

```
source_id: xiigen-b01-0d-007-001
source_title: XIIGen B01 factory-007 — two-flow factory selection for UI+IMPLEMENT
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
sub_phase_id: "1.1 + 4.1"
pair_kind: factory_selection
```

A:
  predicate_context: ["UI", "IMPLEMENT"]
  routing_decisions:
    - sub_phase_id: "1.1"
      evidence_predicates: ["UI"]
    - sub_phase_id: "4.1"
      evidence_predicates: ["IMPLEMENT"]

  Sub-phase 1.1 flow registry:
    registered_flow_id: null
    trust_state: null
    fallback_flow_id: "ScreenInventoryFlow11"

  Sub-phase 4.1 flow registry:
    registered_flow_id: null
    trust_state: null
    fallback_flow_id: "ImplementationFlow41"

B:
  FlowSelections:
    - SubPhaseId: "1.1"
      SelectedFlowId: "ScreenInventoryFlow11"
      TrustState: "deterministic_generic"
      FallbackUsed: false
      Warnings: []
      SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

    - SubPhaseId: "4.1"
      SelectedFlowId: "ImplementationFlow41"
      TrustState: "deterministic_generic"
      FallbackUsed: false
      Warnings: []
      SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

  TotalFlowsSelected: 2

Factory logic:
  UI+IMPLEMENT = two flows. ScreenInventoryFlow11 handles the UX screen mapping;
  ImplementationFlow41 handles the code/component implementation. Both run
  independently and produce separate artifacts.

---

## B01-0D-007-002 — ImplementationFlow41 identity

```
source_id: xiigen-b01-0d-007-002
source_title: XIIGen B01 factory-007 — ImplementationFlow41 identity
source_license_state: first_party
pair_kind: factory_selection
coverage_status: covered
```

ImplementationFlow41 identity:

  FlowId: "ImplementationFlow41"
  FlowName: "Implementation Flow for Sub-Phase 4.1"
  Phase: 4
  SubPhase: 4.1
  NodeCount: 9
  TrustState: deterministic_generic

  Node chain (for implementation work):
    Node 01 — DomainSplitterUnit: SelectedLogicalTransition → DomainPacket[] (IMPLEMENT domain)
    Node 02 — DomainTranslatorUnit: DomainPacket → TranslatedDomainStatement (tech statements)
    Node 03 — SchemePlannerUnit: statements → WorkBranch[] (implementation branches)
    Node 04 — DomainProcessorUnit: WorkBranch → ProcessedBranchResult (code/component spec)
    Node 05 — ApplierUnit: ProcessedBranchResult → AppliedBranchResult
    Node 06 — ValidationUnit: all results → ValidationReport (code quality checks)
    Node 07 — IntegrationUnit: ValidationReport → IntegratedFlowResult
    Node 08 — QualityUnit: IntegratedFlowResult → QualityMetrics
    Node 09 — FormatterUnit: QualityMetrics → FinalHumanResult (implementation plan/code)

---

## B01-0D-007-003 — UI+IMPLEMENT execution order

```
source_id: xiigen-b01-0d-007-003
source_title: XIIGen B01 factory-007 — UI+IMPLEMENT execution order
source_license_state: first_party
pair_kind: factory_selection
coverage_status: covered
```

Execution order for UI+IMPLEMENT (feature_addition):

  Step 1: ScreenInventoryFlow11 executes (priority 1)
    Output: screen inventory with component specs

  Step 2: ImplementationFlow41 executes (priority 4)
    Input: SelectedLogicalTransition (IMPLEMENT branch)
    Reference: ScreenInventoryFlow11 output available — component specs
      from the screen inventory can inform what components to implement
    Output: implementation plan or component code

  Why 1.1 runs before 4.1:
    The screen inventory defines WHAT components exist; the implementation
    flow then decides HOW to build them. Running implementation first would
    mean building components without a complete screen context.

  Combined output:
    - Screen inventory with component specifications (1.1)
    - Component implementation plan or code stubs (4.1)
