# Layer 0-C — Knowledge-Edge Pairs (033)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B01-0C-033-001 — "dark mode" is gap; "color" fires UI

```
source_id: xiigen-b01-0c-033-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A-gap: "Add dark mode support to the product card and hero section."
  → "dark mode" NOT in UI lexicon (compound; "mode" alone not registered; confirmed gap from file 024).
  → "support" NOT in IMPLEMENT lexicon.
  → CompiledPredicateIds: [] → KNOWLEDGE_EDGE

A-fires: "Build a product card component with dark and light color theme variants."
  → "build" fires IMPLEMENT. "component" fires UI. "color" fires UI. "theme" fires UI.
  → CompiledPredicateIds: ["UI", "IMPLEMENT"] → FLOW_EXECUTING

Teaching point: "dark mode" as a compound does not fire UI. "color" and "theme" do.
Design-system token vocabulary ("color", "theme", "typography", "spacing") is the correct UI trigger.

---

## B01-0C-033-002 — "run" and "allow" are gap; contrast with "implement"

```
source_id: xiigen-b01-0c-033-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A-gap: "Allow users to run a date range filter from the date picker."
  → "allow" NOT in IMPLEMENT lexicon (confirmed gap).
  → "run" NOT in IMPLEMENT lexicon (confirmed gap).
  → "range" NOT in any predicate lexicon.
  → CompiledPredicateIds: [] → KNOWLEDGE_EDGE

A-fires: "Implement a date range input component with two date picker fields."
  → "implement" fires IMPLEMENT. "input" fires UI. "component" fires UI.
  → CompiledPredicateIds: ["UI", "IMPLEMENT"] → FLOW_EXECUTING
