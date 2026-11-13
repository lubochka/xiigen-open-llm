# Layer 0-C — Knowledge-Edge Pairs (025)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B01-0C-025-001 — "navigation" is gap even when context is clearly UI

```
source_id: xiigen-b01-0c-025-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A-gap: "Add breadcrumb navigation to every page in the product catalog."
  → "navigation" NOT in UI lexicon. "breadcrumb" NOT in any lexicon. "page" alone suppressed by gap context.
  → CompiledPredicateIds: [] → KNOWLEDGE_EDGE

A-fires: "Create a breadcrumb component that renders the current page path."
  → "component" fires UI. "page" fires UI. "create" fires IMPLEMENT.
  → CompiledPredicateIds: ["UI", "IMPLEMENT"] → FLOW_EXECUTING

Teaching point: the request's intent (navigation) is irrelevant.
Only exact lexicon terms matter. Replacing "navigation" with "component" changes the outcome.

---

## B01-0C-025-002 — "redesign" does not fire IMPLEMENT; contrast with "build"

```
source_id: xiigen-b01-0c-025-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A-gap: "Redesign the review widget look and feel to match the new brand identity."
  → "redesign" NOT in IMPLEMENT lexicon. "brand", "identity", "look and feel" NOT in any lexicon.
  → CompiledPredicateIds: [] → KNOWLEDGE_EDGE

A-fires: "Build the review widget and update the brand color tokens."
  → "widget" fires UI (component family). "build" fires IMPLEMENT. "color" fires UI.
  → CompiledPredicateIds: ["UI", "IMPLEMENT"] → FLOW_EXECUTING

Teaching point: "redesign" is a confirmed gap term even in UI-adjacent contexts.
