# Layer 0-C — Knowledge-Edge Pairs (026)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B01-0C-026-001 — "improve sidebar search flow" — all three terms are gap

```
source_id: xiigen-b01-0c-026-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A-gap: "Refine the filter options and improve the sidebar search flow."
  → "refine" NOT in any lexicon.
  → "improve" NOT in IMPLEMENT lexicon (confirmed gap from file 024 reference).
  → "sidebar" NOT in UI lexicon (confirmed gap).
  → "search" NOT in any predicate lexicon.
  → "flow" NOT in UI lexicon (confirmed gap).
  → CompiledPredicateIds: [] → KNOWLEDGE_EDGE

A-fires: "Build a filter panel with search input and collapsible sidebar sections."
  → "panel" fires UI. "input" fires UI. "build" fires IMPLEMENT.
  → CompiledPredicateIds: ["UI", "IMPLEMENT"] → FLOW_EXECUTING

Teaching point: request topic (filter panel) does not rescue gap vocabulary.
Lexicon match is required. "sidebar" and "flow" are never in UI lexicon regardless of context.

---

## B01-0C-026-002 — "enhance experience + improve journey" — all gap

```
source_id: xiigen-b01-0c-026-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A-gap: "Enhance the product card experience and improve the overall checkout journey."
  → "enhance" NOT in any lexicon.
  → "experience" NOT in UI lexicon (only "ux" exact abbreviation fires UI).
  → "improve" NOT in IMPLEMENT lexicon.
  → "journey" NOT in any predicate lexicon.
  → CompiledPredicateIds: [] → KNOWLEDGE_EDGE

Boundary rule:
  "ux" (2 chars, exact) → fires UI
  "user experience" (longform) → does NOT fire UI
  "experience" (single word) → does NOT fire UI
