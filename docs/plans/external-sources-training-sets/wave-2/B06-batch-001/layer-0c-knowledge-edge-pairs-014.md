# Layer 0-C — Knowledge-Edge Pairs (014)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## Method note

File 014 covers InventoryManagement knowledge-edge boundaries — three pairs.

---

## B06-0C-014-001 — Inventory: "epics" fires DOCUMENT; "track" does not

```
source_id: xiigen-b06-0c-014-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-014-001-kedge"
  CompiledText: "Inventory: epics fires DOCUMENT; track alone does not"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Cases:
    - request: "Specify the epics for inventory reservation at checkout"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — specify fires DOCUMENT, epics fires DOCUMENT"
    - request: "Track inventory levels during checkout"
      predicates: []
      result: "KNOWLEDGE_EDGE — track is not a predicate term"
      capability_gap_id: "gap-inventory-tracking-001"
    - request: "Document the inventory behavior and use cases"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — document fires DOCUMENT, behavior fires DOCUMENT, use cases fires DOCUMENT"

---

## B06-0C-014-002 — Inventory: "monitor" and "optimize" are gaps

```
source_id: xiigen-b06-0c-014-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-014-002-kedge"
  CompiledText: "Inventory gap: monitor, optimize"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Cases:
    - request: "Monitor inventory stock levels across checkout sessions"
      predicates: []
      result: "KNOWLEDGE_EDGE — monitor is not in any predicate lexicon"
      capability_gap_id: "gap-inventory-monitor-001"
    - request: "Optimize inventory reservation timeout settings"
      predicates: []
      result: "KNOWLEDGE_EDGE — optimize is not in any predicate lexicon"
      capability_gap_id: "gap-inventory-optimize-001"
    - request: "Define the inventory timeout requirements"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — define fires DOCUMENT, requirements fires DOCUMENT"

---

## B06-0C-014-003 — Inventory: "capability" vs "manage" boundary

```
source_id: xiigen-b06-0c-014-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-014-003-kedge"
  CompiledText: "Inventory: capability fires DOCUMENT; manage does not"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Cases:
    - request: "Define the capability requirements for inventory management at checkout"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — define fires DOCUMENT, capability fires DOCUMENT, requirements fires DOCUMENT"
    - request: "Manage inventory levels for the checkout flow"
      predicates: []
      result: "KNOWLEDGE_EDGE — manage alone is not a predicate term"
      capability_gap_id: "gap-inventory-manage-001"
    - request: "Describe inventory management behavior"
      predicates: ["DOCUMENT"]
      result: "NOT knowledge-edge — describe fires DOCUMENT, behavior fires DOCUMENT"
