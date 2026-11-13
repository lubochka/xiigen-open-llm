# Layer 0-C — Knowledge-Edge Pairs (034)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B01-0C-034-001 — "modal" fires even when surrounded by gap vocabulary

```
source_id: xiigen-b01-0c-034-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A-mixed: "Optimize the checkout flow and add a confirmation modal for order placement."
  → "optimize" = gap. "flow" = gap. "checkout" alone = gap.
  → BUT: "modal" IS in the UI lexicon → fires UI.
  → CompiledPredicateIds: ["UI"] → FLOW_EXECUTING (not knowledge-edge)

Rule confirmed: gap vocabulary in a request does NOT suppress valid UI lexicon matches.
"modal" is a hard lexicon match; it fires UI regardless of surrounding gap words.

---

## B01-0C-034-002 — "widget" fires UI; "feature" fires IMPLEMENT

```
source_id: xiigen-b01-0c-034-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A-fires: "Add a review widget feature to the product detail page."
  → "widget" fires UI (component family; see B01-0A-025-003 for rule source).
  → "feature" fires IMPLEMENT (in IMPLEMENT lexicon).
  → "page" fires UI.
  → CompiledPredicateIds: ["UI", "IMPLEMENT"] → FLOW_EXECUTING

Teaching point: "feature" is in the IMPLEMENT lexicon.
Contrast: "enhancement", "improvement", "upgrade" are NOT in the IMPLEMENT lexicon.
