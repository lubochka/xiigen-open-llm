# Layer 1 — Transition Orchestrator Pairs (011)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B01-L1-011-001 — Phase 1 sub-phase flow routing

```
source_id: xiigen-b01-l1-011-001
source_title: XIIGen B01 layer1-011 — Phase 1 sub-phase routing and flow map
source_license_state: first_party
pair_kind: transition_orchestrator
coverage_status: covered
```

Phase 1 sub-phase routing in B01:

| Sub-phase | Name | Flow | Registered in B01 |
|---|---|---|---|
| 1.1 | Screen Flow and UX Definition | ScreenInventoryFlow11 | YES (9 pairs in original file) |
| 1.2 | Wireframes and Prototypes | WireframePrototypeFlow12 | NO (future B02 batch) |
| 1.3 | Design System | DesignSystemFlow13 | NO (future B03 batch) |
| 1.4 | Accessibility | AccessibilityFlow14 | NO (future B04 batch) |
| 1.5 | UI Testing | UITestingFlow15 | NO (future B05 batch) |

In B01, only sub-phase 1.1 has a registered flow (ScreenInventoryFlow11).
Sub-phases 1.2-1.5 use deterministic_generic flows if reached.

All UI-predicate signals route to 1.1 in B01 because:
  - 1.1 is the first sub-phase and handles the broadest UI requests
  - B01 covers the "screen inventory and flow mapping" use case
  - More specific sub-phases (1.2-1.5) are covered in subsequent batches

---

## B01-L1-011-002 — EM-9 guard: Node 01 input must be SelectedLogicalTransition

```
source_id: xiigen-b01-l1-011-002
source_title: XIIGen B01 layer1-011 — EM-9 guard annotation
source_license_state: first_party
pair_kind: transition_orchestrator
coverage_status: covered
```

EM-9 guard — Layer 2 Node 01 input requirement:

  The EM-9 guard states: Node 01 input (A side of every Layer 2 pair) MUST be
  a SelectedLogicalTransition object — NEVER a raw user request string.

  Valid Node 01 A-side input:
    TransitionId: "transition-B01-l1-001-1.1-feature_addition"
    TransitionType: "feature_addition"
    PhaseId: "1"
    SubPhaseId: "1.1"
    DomainId: "software-development"
    UserRequest: "[verbatim request]"
    PredicateIds: ["UI"]
    ... (all required fields)

  Invalid Node 01 A-side inputs (EM-9 violation):
    A: "Map all screen flows for the SaaS onboarding UX."
      → Raw string — EM-9 violation

    A: { UserRequest: "Map all screen flows...", PredicateIds: ["UI"] }
      → Partial structure missing required fields — EM-9 violation

  The guard exists because the nine-node flow needs the full transition context
  (TransitionType, DomainId, SubPhaseId, EvidenceIds) to function correctly.
  A raw string provides none of this context.

---

## B01-L1-011-003 — v6 ordering violations for B01

```
source_id: xiigen-b01-l1-011-003
source_title: XIIGen B01 layer1-011 — v6 ordering violations
source_license_state: first_party
pair_kind: transition_orchestrator
corruption_type: ordering-violation
coverage_status: covered
```

v6 ordering violations (Layer 1 specific):

Ordering violation 1: Wrong TransitionType drives SubPhaseId assignment
  Correct: When UI present → SubPhaseId: "1.1" regardless of other predicates
  Corrupt: When [UI, DOCUMENT] → SubPhaseId: "2.1" (DOCUMENT incorrectly selected as primary)
  Impact: Flow execution starts with BusinessRequirementsFlow21 instead of ScreenInventoryFlow11

Ordering violation 2: Flow execution order reversed
  Correct: ScreenInventoryFlow11 (1.1) before BusinessRequirementsFlow21 (2.1)
  Corrupt: BusinessRequirementsFlow21 (2.1) before ScreenInventoryFlow11 (1.1)
  Impact: Requirements document produced before screen IDs exist; cannot reference screens by ID

Ordering violation 3: SelectedLogicalTransition uses wrong SubPhaseId
  Correct: PredicateIds: ["UI"] → SubPhaseId: "1.1"
  Corrupt: PredicateIds: ["UI"] → SubPhaseId: "2.4" (wrong — 2.4 is stakeholder requirements)
  Impact: Wrong flow invoked; ScreenInventoryFlow11 never runs

All ordering violations produce incorrect nine-node flow chains. The only
correction is to re-run the TransitionOrchestratorUnit with the correct
SubPhaseId selection rule applied.
