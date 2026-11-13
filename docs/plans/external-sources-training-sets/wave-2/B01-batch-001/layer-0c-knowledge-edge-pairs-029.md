# Layer 0-C — Knowledge-Edge Pairs (029)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B01-0C-029-001 — "tab" alone is gap; "component" + "accessibility" fire UI

```
source_id: xiigen-b01-0c-029-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A-gap: "Add a tab switcher to the settings area with keyboard shortcuts."
  → "tab" NOT in UI lexicon (domain noun, not a registered UI term).
  → "switcher" NOT in any lexicon.
  → "shortcuts" NOT in any lexicon.
  → CompiledPredicateIds: [] → KNOWLEDGE_EDGE

A-fires: "Build a tab component with keyboard accessibility and visual focus indicator."
  → "build" fires IMPLEMENT. "component" fires UI. "accessibility" fires UI. "visual" fires UI.
  → CompiledPredicateIds: ["UI", "IMPLEMENT"] → FLOW_EXECUTING

Teaching point: "tab" by itself does not fire the UI predicate.
It requires "component", "accessibility", "interaction" or another UI lexicon term.

---

## B01-0C-029-002 — "tooltip" alone is gap; "component" fires UI

```
source_id: xiigen-b01-0c-029-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A-gap: "Show a tooltip when users hover over disabled items."
  → "tooltip" NOT in UI lexicon (domain noun, not registered).
  → "hover" NOT in any lexicon.
  → "disabled" NOT in any lexicon.
  → CompiledPredicateIds: [] → KNOWLEDGE_EDGE

A-fires: "Create a tooltip component with hover delay and accessibility labels."
  → "create" fires IMPLEMENT. "component" fires UI. "accessibility" fires UI.
  → CompiledPredicateIds: ["UI", "IMPLEMENT"] → FLOW_EXECUTING

Teaching point: domain-specific widget names ("tooltip", "tab", "breadcrumb", "gallery") are NOT in the UI lexicon.
Always pair them with a lexicon term ("component", "screen", "modal", etc.) to fire UI.
