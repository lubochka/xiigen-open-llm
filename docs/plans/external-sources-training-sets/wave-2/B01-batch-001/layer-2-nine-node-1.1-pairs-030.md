# Layer 2 — Nine-Node 1.1 Pairs (030)
# Batch: B01-batch-001 | Scenario: B01 — AccordionPanel component flow
# Production date: 2026-05-06 | pair_kind: nine_node_flow

---

## B01-L2-030-001 — AccordionPanel: Node 01 DomainSplitterUnit

```
source_id: xiigen-b01-l2-030-001
pair_kind: nine_node_flow
```

A (SelectedLogicalTransition):
  TransitionId: "transition-B01-l2-030-1.1-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  PredicateIds: ["UI", "IMPLEMENT"]

B (DomainPackets):
  Packet[UI]: "Screen inventory: accordion panel expand/collapse states"
  Packet[IMPLEMENT]: "Implementation: accordion panel component code"

---

## B01-L2-030-002 — AccordionPanel: Node 09 FinalHumanResult

```
source_id: xiigen-b01-l2-030-002
pair_kind: nine_node_flow
```

B (FinalHumanResult):
  FinalHumanResult:
    Title: "AccordionPanel Component Screen Inventory"
    SubPhaseId: "1.1"
    ScreenCount: 4
    Screens:
      - ScreenId: "accordion-all-collapsed"
        Name: "Accordion All Items Collapsed"
        EntryConditions: ["initial load", "all items closed"]
        ExitTransitions: ["item click → SingleExpandedState"]
      - ScreenId: "accordion-single-expanded"
        Name: "Accordion Single Item Expanded"
        EntryConditions: ["one item clicked"]
        ExitTransitions: ["same item click → AllCollapsed", "different item click → DifferentExpanded or MultiExpanded"]
      - ScreenId: "accordion-multiple-expanded"
        Name: "Accordion Multiple Items Expanded"
        EntryConditions: ["multi-expand mode enabled"]
        ExitTransitions: ["item close click → ReducedExpanded"]
      - ScreenId: "accordion-keyboard-focus"
        Name: "Accordion Keyboard Focus State"
        EntryConditions: ["keyboard navigation active", "item focused via Tab"]
        ExitTransitions: ["Enter/Space → toggleExpanded", "Tab → nextItem", "Shift+Tab → prevItem"]
    GateBResult: "PASS: 4 screens; entry/exit conditions met for all; no unresolved screens"
  Confidence: 0.90
  SelectionOutcome: "FORMATTED_COMPLETE"

---

## B01-L2-030-003 — AccordionPanel: accessibility state as required screen

```
source_id: xiigen-b01-l2-030-003
pair_kind: nine_node_flow
```

Best practice confirmed in file 030:
  Keyboard focus/navigation states are required screens for interactive components.
  "Accordion keyboard focus state" is a legitimate screen with entry/exit conditions.
  Accessibility states are not optional — they are first-class screens in B01 UI component inventory.
