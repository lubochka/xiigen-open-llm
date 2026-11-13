# Layer 0-C — Knowledge-Edge Pairs (023)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## Gap vocabulary comprehensive list

This file provides the authoritative B01 gap vocabulary reference paired with
contrast terms from the UI lexicon to make the boundary sharp.

---

## B01-0C-023-001 — gap vocabulary: navigation vs. button

```
source_id: xiigen-b01-0c-023-gap-nav
source_license_state: first_party
pair_kind: knowledge_edge
```

Gap term: "navigation" → does NOT fire UI
UI term: "button" → fires UI
Contrast pair:
  A-gap: "Improve the navigation between checkout steps."
  → CompiledPredicateIds: [] → KNOWLEDGE_EDGE
  A-normal: "Add a back button between checkout steps."
  → CompiledPredicateIds: ["UI"] (button) + ["IMPLEMENT"] (add) → FLOW_EXECUTING

---

## B01-0C-023-002 — gap vocabulary: menu vs. modal

```
source_id: xiigen-b01-0c-023-gap-menu
source_license_state: first_party
pair_kind: knowledge_edge
```

Gap term: "menu" → does NOT fire UI
UI term: "modal" → fires UI
Contrast pair:
  A-gap: "Update the dropdown menu with new options."
  → CompiledPredicateIds: [] → KNOWLEDGE_EDGE
  A-normal: "Add a confirmation modal before deleting an address."
  → CompiledPredicateIds: ["UI"] (modal) + ["IMPLEMENT"] (add) → FLOW_EXECUTING

---

## B01-0C-023-003 — gap vocabulary: sidebar vs. layout

```
source_id: xiigen-b01-0c-023-gap-sidebar
source_license_state: first_party
pair_kind: knowledge_edge
```

Gap terms: "sidebar", "drawer", "panel" (generic) → do NOT fire UI
UI term: "layout" → fires UI
Contrast pair:
  A-gap: "Slide out the sidebar on mobile."
  → CompiledPredicateIds: [] → KNOWLEDGE_EDGE
  A-normal: "Redesign the layout for the mobile view of the settings page."
  → UI fires on: "layout", "page" → CompiledPredicateIds: ["UI"] → FLOW_EXECUTING
    Note: "redesign" does NOT fire IMPLEMENT.
