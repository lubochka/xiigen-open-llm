# Layer 0-D — Flow Factory Selection Pairs (024)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## Method note

File 024 is the complete factory map summary for files 013-024, covering all
new domains, trust states, and flow selection rules introduced in sub-phase 2.1.

---

## B06-0D-024-001 — Complete factory map for files 013-024

```
source_id: xiigen-b06-0d-024-001
source_license_state: first_party
pair_kind: factory_selection
```

Complete flow factory map update (adds to 001-012 baseline):

  Files 013-024 add 10 new domain contexts to the factory's coverage:
  ShippingRequirements, InventoryManagement, LoyaltyProgram, ReturnAndRefund,
  TaxCalculation, MultiCurrency, BundleAndDiscount, SubscriptionBilling,
  FraudDetection, DataPrivacy.

  All 10 new domains route to the same flows as the original domains:
    DOCUMENT → 2.1 → RequirementsAuthoringFlow21
    DOCUMENT + stakeholder → 2.4 → StakeholderRequirementsFlow24 (in addition to 2.1)
    IMPLEMENT → 4.1 → ImplementationFlow41

  New stakeholder-triggered 2.4 cases added:
    LoyaltyProgram (file 015, pair 2)
    DataPrivacy (file 022, pair 2)

---

## B06-0D-024-002 — Factory trust state summary for new domains

```
source_id: xiigen-b06-0d-024-002
source_license_state: first_party
pair_kind: factory_selection
```

Trust state assignment for new domain flows:

  Flow                      | TrustState
  --------------------------|-----------------
  RequirementsAuthoringFlow21 | trusted_learned (B06 batch-001 promoted)
  StakeholderRequirementsFlow24 | deterministic_generic (no learned flow yet)
  ImplementationFlow41       | deterministic_generic (not covered in B06)
  ArchitectureFlow31         | deterministic_generic (not covered in B06)
  ScreenInventoryFlow11      | deterministic_generic (not covered in B06)

  Note: Only RequirementsAuthoringFlow21 is promoted to trusted_learned by B06.
  Other flows remain at deterministic_generic until their own training batches are produced.

---

## B06-0D-024-003 — Factory anti-patterns for new domains

```
source_id: xiigen-b06-0d-024-003
source_license_state: first_party
pair_kind: factory_selection
```

Factory anti-patterns for files 013-024:

  AP-F1: Creating domain-specific flows (e.g., FraudDetectionFlow21, TaxFlow21).
    Correct: RequirementsAuthoringFlow21 handles all requirement domains.
    The flow is domain-agnostic at the factory level.

  AP-F2: Routing to non-existent sub-phases (2.5 "security", 2.6 "compliance").
    Correct: Sub-phase map is fixed: 2.1, 2.2, 2.3, 2.4, 3.1, 4.1, 1.1.
    No new sub-phases are created for new domains.

  AP-F3: Setting SelectedFlowId to null when trust state is probationary_rejected.
    Correct: Factory always produces a valid SelectedFlowId using universal fallback.

  AP-F4: Selecting only one flow for DOCUMENT+stakeholder requests (missing 2.4 flow).
    Correct: When 2.4 routing decision exists, StakeholderRequirementsFlow24 must also be selected.
