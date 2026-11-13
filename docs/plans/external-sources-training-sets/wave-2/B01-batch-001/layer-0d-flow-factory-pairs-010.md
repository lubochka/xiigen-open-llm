# Layer 0-D — Flow Factory Selection Pairs (010)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B01-0D-010-001 — v6 factory corruption: wrong flow for UI predicate

```
source_id: xiigen-b01-0d-010-v6-corrupt
source_title: XIIGen B01 factory-010 — v6 factory corruption: UI maps to wrong flow
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
pair_kind: factory_selection
corruption_type: factory-corruption-v6-wrong-flow
```

A:
  predicate_context: ["UI"]
  routing_decisions:
    - sub_phase_id: "1.1", evidence_predicates: ["UI"]
  Sub-phase 1.1 flow registry: registered_flow_id: null; fallback: "ScreenInventoryFlow11"

B-correct:
  FlowSelections:
    - SubPhaseId: "1.1"
      SelectedFlowId: "ScreenInventoryFlow11"
      TrustState: "deterministic_generic"
      FallbackUsed: false
      Warnings: []
      SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"
  TotalFlowsSelected: 1

B-corrupt:
  FlowSelections:
    - SubPhaseId: "1.1"
      SelectedFlowId: "BusinessRequirementsFlow21"
      TrustState: "deterministic_generic"
      FallbackUsed: false
      Warnings: []
      SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"
  TotalFlowsSelected: 1

Corruption explanation:
  The corrupted factory selects BusinessRequirementsFlow21 for sub-phase 1.1.
  BusinessRequirementsFlow21 belongs to sub-phase 2.1 (DOCUMENT predicate).
  Using it for 1.1 (UI predicate) causes the nine-node chain to process the
  request as a requirements document task instead of a screen inventory task.
  The output is a malformed requirements document with no screen IDs, no
  entry/exit transitions, and no UX structure. This corruption is detected
  by comparing SelectedFlowId against the expected flow for the sub-phase.

---

## B01-0D-010-002 — factory corruption: probationary promoted without Gate B

```
source_id: xiigen-b01-0d-010-v6-no-gateb
source_title: XIIGen B01 factory-010 — factory corruption: probationary used as trusted_learned
source_license_state: first_party
pair_kind: factory_selection
corruption_type: factory-corruption-v6-premature-promotion
```

A:
  sub_phase_id: "1.1"
  flow_registry_state:
    registered_flow_id: "ScreenInventoryFlow11-candidate-v1"
    trust_state: "probationary"
    gate_b_passed: false
    gate_b_reason: "5_of_9_nodes_covered"
  fallback_flow_id: "ScreenInventoryFlow11"

B-correct:
  SelectedFlowId: "ScreenInventoryFlow11"
  TrustState: "deterministic_generic"
  FallbackUsed: true
  SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED"

B-corrupt:
  SelectedFlowId: "ScreenInventoryFlow11-candidate-v1"
  TrustState: "trusted_learned"
  FallbackUsed: false
  Warnings: []
  SelectionOutcome: "TRUSTED_LEARNED_SELECTED"

Corruption explanation:
  The corrupted factory marks a probationary (Gate B failed) flow as trusted_learned
  and uses it directly. This is the most dangerous factory corruption: it suppresses
  all warnings and marks the outcome as TRUSTED_LEARNED_SELECTED when it should be
  REJECTED_PROBATIONARY_FALLBACK_USED. The incomplete flow (5/9 nodes) runs in
  production with no warnings.

---

## B01-0D-010-003 — Factory corruption detection rules

```
source_id: xiigen-b01-0d-010-detection
source_title: XIIGen B01 factory-010 — factory corruption detection checklist
source_license_state: first_party
pair_kind: factory_selection
coverage_status: covered
```

Factory corruption detection rules:

Rule 1 (wrong flow for sub-phase):
  IF SubPhaseId == "1.1" AND SelectedFlowId contains "BusinessRequirements" or "Implementation"
  THEN: factory corruption — wrong flow selected for sub-phase 1.1

Rule 2 (premature trusted_learned):
  IF flow_registry_state.gate_b_passed == false
  AND output TrustState == "trusted_learned"
  THEN: factory corruption — premature promotion

Rule 3 (suppressed warnings on fallback):
  IF FallbackUsed == true
  AND Warnings array is empty
  THEN: factory corruption — fallback warnings suppressed

Rule 4 (wrong SelectionOutcome for trust state):
  Valid combinations only:
    trusted_learned → TRUSTED_LEARNED_SELECTED
    probationary_accepted → ACCEPTED_PROBATIONARY_GATE_B_PASSED
    deterministic_generic (no candidate) → DETERMINISTIC_GENERIC_SELECTED
    deterministic_generic (rejected candidate) → REJECTED_PROBATIONARY_FALLBACK_USED
  Any other combination is a factory corruption.
