# Layer 0-C — Knowledge-Edge Pairs (028)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B01-0C-028-001 — "gallery" alone does not fire UI; "component" does

```
source_id: xiigen-b01-0c-028-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A-gap: "Set up a gallery view for the product catalog media assets."
  → "gallery" NOT in UI lexicon (it is a domain noun, not a UI lexicon term).
  → "view" NOT in UI lexicon.
  → "catalog" NOT in any lexicon.
  → "media" NOT in any lexicon.
  → CompiledPredicateIds: [] → KNOWLEDGE_EDGE

A-fires: "Build an image gallery component with a responsive layout for product images."
  → "build" fires IMPLEMENT. "component" fires UI. "layout" fires UI. "responsive" fires UI.
  → CompiledPredicateIds: ["UI", "IMPLEMENT"] → FLOW_EXECUTING

Teaching point: domain nouns like "gallery", "catalog", "media" are not in the UI lexicon.
Only the 22 registered UI lexicon terms trigger the UI predicate.

---

## B01-0C-028-002 — Gap vocabulary does not suppress valid matches (boundary rule)

```
source_id: xiigen-b01-0c-028-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A-mixed (gap+valid): "Improve the modal dialog component and optimize the navigation sidebar flow."
  → "improve" = gap. "optimize" = gap. "navigation" = gap. "sidebar" = gap. "flow" = gap.
  → BUT: "modal" fires UI. "component" fires UI.
  → CompiledPredicateIds: ["UI"] → FLOW_EXECUTING (gap terms do NOT suppress "modal" and "component")

Rule: gap vocabulary in a request does NOT suppress valid lexicon matches in the same request.
Compare with cross-layer-synthesis-006.md ("navigation" gap + "page" UI → still FLOW_EXECUTING).
