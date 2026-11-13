# Layer 0-D — Flow Factory Selection Pairs (012)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## Method note

File 012 is the complete B06 factory map summary.

---

## B06-0D-012-001 — Complete B06 factory map

```
source_id: xiigen-b06-0d-012-001
source_title: XIIGen B06 factory — complete B06 factory map
source_license_state: first_party
pair_kind: factory_selection
```

B06 Complete Factory Map:

  Sub-phase | Flow ID                                      | Alias
  ----------|----------------------------------------------|-----------------------------
  1.1       | universal-nine-node-development-flow-1.1     | ScreenInventoryFlow11
  2.1       | universal-nine-node-development-flow-2.1     | RequirementsAuthoringFlow21
  2.4       | universal-nine-node-development-flow-2.4     | StakeholderRequirementsFlow24
  3.1       | universal-nine-node-development-flow-3.1     | ArchitectureFlow31
  4.1       | universal-nine-node-development-flow-4.1     | ImplementationFlow41

  All flows use the same nine-node structure: N01 through N09.
  All flows default to deterministic_generic when no learned flow is registered.
  RequirementsAuthoringFlow21 (2.1) is the primary B06 focus.

---

## B06-0D-012-002 — B06 factory selection predicate → sub-phase → flow table

```
source_id: xiigen-b06-0d-012-002
source_title: XIIGen B06 factory — predicate to sub-phase to flow table
source_license_state: first_party
pair_kind: factory_selection
```

Complete predicate → sub-phase → flow selection chain for B06:

  Predicate    | Sub-phase | Flow selected (generic)
  -------------|-----------|----------------------------------------
  DOCUMENT     | 2.1       | universal-nine-node-development-flow-2.1
  DOCUMENT     | 2.4*      | universal-nine-node-development-flow-2.4
  IMPLEMENT    | 4.1       | universal-nine-node-development-flow-4.1
  UI           | 1.1       | universal-nine-node-development-flow-1.1
  ARCHITECTURE | 3.1       | universal-nine-node-development-flow-3.1

  * 2.4 selection: only when "stakeholder" or other 2.4 sub-phase signals present

---

## B06-0D-012-003 — Factory summary: trust states and outcomes

```
source_id: xiigen-b06-0d-012-003
source_title: XIIGen B06 factory — trust states and outcomes summary
source_license_state: first_party
pair_kind: factory_selection
```

B06 Factory Trust State Summary:

  Trust state              | Condition                           | Flow used         | FallbackUsed | Warnings
  -------------------------|-------------------------------------|-------------------|--------------|----------
  deterministic_generic    | No registered flow                  | universal generic | false        | none
  probationary_rejected    | Registered; Gate B failed           | universal generic | true         | 3+ warnings
  probationary_accepted    | Registered; Gate B passed; <25 pairs| candidate flow    | false        | 3 warnings
  trusted_learned          | Registered; Gate B passed; >=25     | learned flow      | false        | none

  The factory ALWAYS produces a valid SelectedFlowId.
  If no learned flow exists or the candidate is rejected, the universal
  generic flow is used. The pipeline never halts due to missing flow.
  Production always continues — quality may vary between generic and learned.
