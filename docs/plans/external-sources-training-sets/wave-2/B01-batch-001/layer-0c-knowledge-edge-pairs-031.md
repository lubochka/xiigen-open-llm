# Layer 0-C — Knowledge-Edge Pairs (031)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B01-0C-031-001 — "modal" fires UI; surrounding gap vocabulary does not suppress it

```
source_id: xiigen-b01-0c-031-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A-mixed (valid match present): "Add a confirmation popup and improve the checkout flow experience."
  → "popup" NOT in UI lexicon.
  → BUT: this request contains no registered UI lexicon terms at all.
  → "improve" = gap. "flow" = gap. "experience" = gap.
  → CompiledPredicateIds: [] → KNOWLEDGE_EDGE

A-fires: "Build a modal confirmation dialog to improve the checkout form completion."
  → "build" fires IMPLEMENT. "modal" fires UI. "form" fires UI.
  → CompiledPredicateIds: ["UI", "IMPLEMENT"] → FLOW_EXECUTING

Teaching point: "popup" is NOT in the UI lexicon. "modal" IS. They are distinct.

---

## B01-0C-031-002 — "hero section" context does not rescue gap vocabulary

```
source_id: xiigen-b01-0c-031-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A-gap: "Update the hero section's animation timing and scroll behavior."
  → "update" NOT in IMPLEMENT lexicon (non-code "update" doesn't fire IMPLEMENT).
  → "animation" NOT in any predicate lexicon.
  → "scroll" NOT in any predicate lexicon.
  → CompiledPredicateIds: [] → KNOWLEDGE_EDGE

A-fires: "Implement a hero section component with scroll-triggered visual animations."
  → "implement" fires IMPLEMENT. "component" fires UI. "visual" fires UI.
  → CompiledPredicateIds: ["UI", "IMPLEMENT"] → FLOW_EXECUTING

Rule: model must NOT infer predicates from "hero section" context alone.
Lexicon match is the only trigger — topic context does not count.
