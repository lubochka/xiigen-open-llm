# Layer 0-C — Knowledge-Edge Pairs (030)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B01-0C-030-001 — "pagination" alone is gap; "component" fires UI

```
source_id: xiigen-b01-0c-030-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A-gap: "Add pagination to the product results so users can move between pages."
  → "pagination" NOT in UI lexicon (domain noun).
  → "results" NOT in any lexicon.
  → CompiledPredicateIds: [] → KNOWLEDGE_EDGE

A-fires: "Build a pagination component with previous/next buttons and page number display."
  → "build" fires IMPLEMENT. "component" fires UI. "button" fires UI. "page" fires UI.
  → CompiledPredicateIds: ["UI", "IMPLEMENT"] → FLOW_EXECUTING

Teaching point: feature names ("pagination", "filter", "review") are not UI lexicon terms.
The corresponding component/page/button trigger words are what matter.

---

## B01-0C-030-002 — "date picker" alone is gap; "component" + "input" fire UI

```
source_id: xiigen-b01-0c-030-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A-gap: "Let users select a date from a calendar to filter search results."
  → "calendar" NOT in any UI lexicon.
  → "select" NOT in IMPLEMENT lexicon (not a code-action verb).
  → "filter" NOT in any predicate lexicon.
  → CompiledPredicateIds: [] → KNOWLEDGE_EDGE

A-fires: "Create a date picker component with input validation and accessible month view."
  → "create" fires IMPLEMENT. "component" fires UI. "input" fires UI. "accessibility" fires UI.
  → CompiledPredicateIds: ["UI", "IMPLEMENT"] → FLOW_EXECUTING
