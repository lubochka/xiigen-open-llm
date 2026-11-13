# Layer 2 — Nine-Node 1.1 Pairs (026)
# Batch: B01-batch-001 | Scenario: B01 — ProductCard component flow
# Production date: 2026-05-06 | pair_kind: nine_node_flow

---

## B01-L2-026-001 — ProductCard: Node 01 DomainSplitterUnit

```
source_id: xiigen-b01-l2-026-001
pair_kind: nine_node_flow
```

A (SelectedLogicalTransition):
  TransitionId: "transition-B01-l2-026-1.1-documentation"
  TransitionType: "documentation"
  SubPhaseId: "1.1"
  PredicateIds: ["UI"]

B (DomainPacket):
  Packet[UI]: "Screen inventory: product card layout states and responsive variants"

---

## B01-L2-026-002 — ProductCard: Node 09 FinalHumanResult

```
source_id: xiigen-b01-l2-026-002
pair_kind: nine_node_flow
```

B (FinalHumanResult):
  FinalHumanResult:
    Title: "ProductCard Component Screen Inventory"
    SubPhaseId: "1.1"
    ScreenCount: 3
    Screens:
      - ScreenId: "card-default"
        Name: "Product Card Default"
        EntryConditions: ["product data loaded", "image available"]
        ExitTransitions: ["card click → ProductDetailPage", "add-to-cart → CartFlow"]
      - ScreenId: "card-image-error"
        Name: "Product Card Image Error"
        EntryConditions: ["image load failed"]
        ExitTransitions: ["same as default", "retry → DefaultCard"]
      - ScreenId: "card-out-of-stock"
        Name: "Product Card Out of Stock"
        EntryConditions: ["inventory count == 0"]
        ExitTransitions: ["notify me click → NotifyFlow", "browse → CatalogPage"]
    GateBResult: "PASS: 3 screens; entry/exit conditions met for all; no unresolved screens"
  Confidence: 0.88
  SelectionOutcome: "FORMATTED_COMPLETE"

---

## B01-L2-026-003 — ProductCard: documentation TransitionType → DomainSplitter produces UI packet only

```
source_id: xiigen-b01-l2-026-003
pair_kind: nine_node_flow
```

Rule: TransitionType: "documentation" with PredicateIds: ["UI"]
  → DomainSplitter produces ONE packet (UI packet only).
  → No IMPLEMENT packet produced (no IMPLEMENT predicate).
  → Layer 2 executes ScreenInventoryFlow11 path only.

Contrast: TransitionType: "feature_addition" with PredicateIds: ["UI", "IMPLEMENT"]
  → DomainSplitter produces TWO packets: UI + IMPLEMENT.
