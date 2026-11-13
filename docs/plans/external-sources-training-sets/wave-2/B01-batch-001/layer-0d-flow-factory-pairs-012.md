# Layer 0-D — Flow Factory Selection Pairs (012)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B01-0D-012-001 — Complete B01 factory map

```
source_id: xiigen-b01-0d-012-001
source_title: XIIGen B01 factory-012 — complete factory map for all predicate combinations
source_license_state: first_party
pair_kind: factory_selection
coverage_status: covered
```

Complete B01 factory selection map:

| Predicates | Flows selected | Flow IDs | TotalFlowsSelected |
|---|---|---|---|
| [] | 0 | knowledge-edge-missing-flow (synthetic) | 0 |
| [UI] | 1 | ScreenInventoryFlow11 | 1 |
| [DOCUMENT] | 1 | BusinessRequirementsFlow21 | 1 |
| [IMPLEMENT] | 1 | ImplementationFlow41 | 1 |
| [ARCHITECTURE] | 1 | SystemArchitectureFlow31 | 1 |
| [UI, DOCUMENT] | 2 | ScreenInventoryFlow11 + BusinessRequirementsFlow21 | 2 |
| [UI, IMPLEMENT] | 2 | ScreenInventoryFlow11 + ImplementationFlow41 | 2 |
| [UI, ARCHITECTURE] | 2 | ScreenInventoryFlow11 + SystemArchitectureFlow31 | 2 |
| [DOCUMENT, IMPLEMENT] | 2 | BusinessRequirementsFlow21 + ImplementationFlow41 | 2 |
| [ARCHITECTURE, IMPLEMENT] | 2 | SystemArchitectureFlow31 + ImplementationFlow41 | 2 |
| [ARCHITECTURE, DOCUMENT] | 2 | SystemArchitectureFlow31 + BusinessRequirementsFlow21 | 2 |
| [UI, DOCUMENT, IMPLEMENT] | 3 | Screen+Requirements+Implementation | 3 |
| [UI, ARCHITECTURE, IMPLEMENT] | 3 | Screen+Architecture+Implementation | 3 |
| [UI, ARCHITECTURE, DOCUMENT] | 3 | Screen+Architecture+Requirements | 3 |
| [UI, ARCHITECTURE, IMPLEMENT, DOCUMENT] | 4 | All four flows | 4 |

---

## B01-0D-012-002 — Trust state prevalence in B01 batch current state

```
source_id: xiigen-b01-0d-012-002
source_title: XIIGen B01 factory-012 — trust state snapshot for B01 batch
source_license_state: first_party
pair_kind: factory_selection
coverage_status: covered
```

Trust state snapshot for B01 (as of wave-2 batch production):

  ScreenInventoryFlow11 (sub-phase 1.1):
    Current state: deterministic_generic
    Candidate registered: none
    Pairs needed for Gate B: 12 (node coverage) — not yet met
    Next action: produce evidence pairs for all 9 nodes → this batch does this

  BusinessRequirementsFlow21 (sub-phase 2.1):
    Current state: deterministic_generic
    Candidate registered: none
    Next action: B06 batch covers 2.1 with requirements pairs

  SystemArchitectureFlow31 (sub-phase 3.1):
    Current state: deterministic_generic
    Candidate registered: none
    Next action: architecture batch (future wave)

  ImplementationFlow41 (sub-phase 4.1):
    Current state: deterministic_generic
    Candidate registered: none
    Next action: implementation batches (future wave)

---

## B01-0D-012-003 — Factory selection rules: complete summary

```
source_id: xiigen-b01-0d-012-003
source_title: XIIGen B01 factory-012 — factory selection rules summary
source_license_state: first_party
pair_kind: factory_selection
coverage_status: covered
```

Factory selection rules (complete summary):

  Rule 1: One factory call per routing decision. TotalFlowsSelected == DecisionCount.

  Rule 2: Flow ID must match the correct sub-phase:
    1.1 → ScreenInventoryFlow11 (or registered variant)
    2.1 → BusinessRequirementsFlow21 (or registered variant)
    3.1 → SystemArchitectureFlow31 (or registered variant)
    4.1 → ImplementationFlow41 (or registered variant)

  Rule 3: Trust state selection order for each sub-phase:
    Priority 1: trusted_learned (if registered) → use it, Warnings: [], no fallback
    Priority 2: probationary_accepted (Gate B passed) → use it, issue monitoring warnings
    Priority 3: probationary (Gate B not evaluated or failed) → reject, use fallback
    Priority 4: no registration → deterministic_generic, FallbackUsed: false

  Rule 4: FallbackUsed: true ONLY when a registered candidate was rejected.
    FallbackUsed: false in all other cases (including deterministic_generic with no candidate).

  Rule 5: SelectionOutcome must exactly match the trust state:
    trusted_learned → TRUSTED_LEARNED_SELECTED
    probationary_accepted → ACCEPTED_PROBATIONARY_GATE_B_PASSED
    rejected probationary → REJECTED_PROBATIONARY_FALLBACK_USED
    no candidate (generic) → DETERMINISTIC_GENERIC_SELECTED
    knowledge-edge → KNOWLEDGE_EDGE_SYNTHETIC_FLOW
