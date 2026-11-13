# Layer 2 — Nine-Node 1.1 Pairs (024)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: nine_node_flow
# Complete nine-node rule summary for sub-phase 1.1

---

## Complete Nine-Node Rule Summary for Sub-Phase 1.1 (ScreenInventoryFlow11)

---

### Node summary reference

| Node | Unit | Input type | Output type |
|---|---|---|---|
| 01 | DomainSplitterUnit | SelectedLogicalTransition | DomainPacket[] |
| 02 | DomainTranslatorUnit | DomainPacket | TranslatedPacket |
| 03 | SchemePlannerUnit | TranslatedPacket[] | WorkBranches |
| 04 | DomainProcessorUnit | WorkBranch | ProcessedItems |
| 05 | ApplierUnit | ProcessedItems | AppliedItems |
| 06 | ResultTranslatorUnit | AppliedItems[] | ResultTranslation |
| 07 | ArbiterSelectorUnit | ResultTranslation | ArbiterType |
| 08 | ArbiterUnit | ArbiterType + items | ArbiterVerdict |
| 09 | FormatterUnit | ArbiterVerdict[] | FinalHumanResult |

---

### EM-9 guard rules

1. Node 01 A input MUST be SelectedLogicalTransition — never a raw string
2. Node 01 A must contain: TransitionId, TransitionType, SubPhaseId, PredicateIds
3. Each node A input is the canonical output type of the preceding node
4. Nodes 03, 06, 07, 09: accept collection inputs (arrays of preceding node outputs)
5. Nodes 02, 04, 05, 08: process one item at a time

---

### Gate B criteria for sub-phase 1.1

| Assertion | Threshold |
|---|---|
| ScreenCount | >= 3 |
| All screens have entry/exit conditions | true |
| No unresolved screens | Unresolved == 0 |
| Arbiter verdict | APPROVED or APPROVED_WITH_WARNING |
| Confidence | >= 0.70 |

---

### B01-L2-024-001 — Rule summary pair: Node 01 EM-9 guard

A (correct):
  TransitionId: "transition-B01-l2-024-1.1-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  PredicateIds: ["UI"]

A (corrupt — EM-9 violation):
  "Map all screens in the settings dashboard."  [raw string — WRONG]

Rule: Node 01 A side is ALWAYS SelectedLogicalTransition. Never a raw string.

---

### B01-L2-024-002 — Rule summary pair: Node 09 FinalHumanResult Gate B

Correct Node 09 output for sub-phase 1.1:
  FinalHumanResult:
    Title: [descriptive scenario title]
    SubPhaseId: "1.1"
    ScreenCount: [integer >= 3]
    Screens: [list with ScreenId, Name, EntryConditions, ExitTransitions]
    GateBResult: "PASS: [N] screens; [conditions met]"
  Confidence: [>= 0.70]
  SelectionOutcome: "FORMATTED_COMPLETE"

Corrupt Node 09 output (v6-01):
  FinalHumanResult:
    Title: "Settings Dashboard"  [missing ScreenCount, missing entry/exit conditions]
    GateBResult: "PASS"  [no supporting evidence]
  Warning: missing Confidence, missing Screens list

Rule: FinalHumanResult must be complete. ScreenCount, Screens list, GateBResult with evidence
are all required. "PASS" without evidence counts as Corruption-v6-01.
