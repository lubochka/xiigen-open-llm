# Layer 0-D — Flow Factory Selection Pairs (006)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B01-0D-006-001 — UI + DOCUMENT factory: ScreenInventoryFlow11 + DocumentationFlow

```
source_id: xiigen-b01-0d-006-001
source_title: XIIGen B01 factory-006 — two-flow factory selection for UI+DOCUMENT
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
sub_phase_id: "1.1 + 2.1"
pair_kind: factory_selection
```

A:
  predicate_context: ["UI", "DOCUMENT"]
  routing_decisions:
    - sub_phase_id: "1.1"
      evidence_predicates: ["UI"]
    - sub_phase_id: "2.1"
      evidence_predicates: ["DOCUMENT"]

  Sub-phase 1.1 flow registry:
    registered_flow_id: null
    trust_state: null
    fallback_flow_id: "ScreenInventoryFlow11"

  Sub-phase 2.1 flow registry:
    registered_flow_id: null
    trust_state: null
    fallback_flow_id: "BusinessRequirementsFlow21"

B:
  FlowSelections:
    - SubPhaseId: "1.1"
      SelectedFlowId: "ScreenInventoryFlow11"
      TrustState: "deterministic_generic"
      FallbackUsed: false
      Warnings: []
      SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

    - SubPhaseId: "2.1"
      SelectedFlowId: "BusinessRequirementsFlow21"
      TrustState: "deterministic_generic"
      FallbackUsed: false
      Warnings: []
      SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

  TotalFlowsSelected: 2

Factory logic:
  Two routing decisions → two independent factory selections. The factory evaluates
  each sub-phase independently. Both happen to be deterministic_generic in this batch.
  TotalFlowsSelected: 2. The flows run in priority order: 1.1 first, then 2.1.

---

## B01-0D-006-002 — DocumentationFlow21 identity

```
source_id: xiigen-b01-0d-006-002
source_title: XIIGen B01 factory-006 — BusinessRequirementsFlow21 identity
source_license_state: first_party
pair_kind: factory_selection
coverage_status: covered
```

BusinessRequirementsFlow21 identity:

  FlowId: "BusinessRequirementsFlow21"
  FlowName: "Business Requirements Flow for Sub-Phase 2.1"
  Phase: 2
  SubPhase: 2.1
  NodeCount: 9
  TrustState: deterministic_generic (in B01 current state)

  Node chain (parallel to ScreenInventoryFlow11 but for requirements):
    Node 01 — DomainSplitterUnit: SelectedLogicalTransition → DomainPacket[] (DOCUMENT domain)
    Node 02 — DomainTranslatorUnit: DomainPacket → TranslatedDomainStatement
    Node 03 — SchemePlannerUnit: statements → WorkBranch[] (requirement branches)
    Node 04 — DomainProcessorUnit: WorkBranch → ProcessedBranchResult (requirement items)
    Node 05 — ApplierUnit: ProcessedBranchResult → AppliedBranchResult (applied requirements)
    Node 06 — ValidationUnit: all results → ValidationReport
    Node 07 — IntegrationUnit: ValidationReport → IntegratedFlowResult
    Node 08 — QualityUnit: IntegratedFlowResult → QualityMetrics
    Node 09 — FormatterUnit: QualityMetrics → FinalHumanResult (requirements document)

  Gate B: All requirements have acceptance criteria; no orphan requirements;
    requirement count >= expected_requirements for the scenario.

---

## B01-0D-006-003 — Two-flow execution order for UI+DOCUMENT

```
source_id: xiigen-b01-0d-006-003
source_title: XIIGen B01 factory-006 — two-flow execution ordering
source_license_state: first_party
pair_kind: factory_selection
coverage_status: covered
```

Execution order for two-flow selection:

  When ScreenInventoryFlow11 (1.1) and BusinessRequirementsFlow21 (2.1) are
  both selected, the execution order is:

  Step 1: ScreenInventoryFlow11 executes (sub-phase 1.1 priority = 1)
    Input: SelectedLogicalTransition (UI predicate branch)
    Output: FinalHumanResult — screen inventory document
    Duration: 9 nodes

  Step 2: BusinessRequirementsFlow21 executes (sub-phase 2.1 priority = 3)
    Input: SelectedLogicalTransition (DOCUMENT predicate branch)
    Reference: ScreenInventoryFlow11 output available for cross-reference
    Output: FinalHumanResult — requirements document
    Duration: 9 nodes

  Combined final output:
    - Screen inventory (from 1.1 flow)
    - Requirements document (from 2.1 flow)
    Both are independently valid artifacts. The requirements document may
    reference screens from the inventory by ScreenId.
