# Layer 0-C — Knowledge-Edge Pairs (027)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B01-0C-027-001 — "animate" and "transition" are gap terms

```
source_id: xiigen-b01-0c-027-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A-gap: "Add smooth animation transitions to the hero section scroll reveal effect."
  → "animate" NOT in any predicate lexicon.
  → "transition" NOT in any predicate lexicon (despite being a system concept).
  → "scroll" NOT in any lexicon.
  → "reveal" NOT in any lexicon.
  → CompiledPredicateIds: [] → KNOWLEDGE_EDGE

A-fires: "Build a hero section with animated entrance using CSS interaction tokens."
  → "build" fires IMPLEMENT. "interaction" fires UI.
  → CompiledPredicateIds: ["UI", "IMPLEMENT"] → FLOW_EXECUTING

Teaching point: "transition" as a user-facing concept (CSS animation) does NOT fire any predicate.
The system concept "SelectedLogicalTransition" is an internal type — not a lexicon trigger.

---

## B01-0C-027-002 — "plan" and "strategy" are gap terms in component context

```
source_id: xiigen-b01-0c-027-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A-gap: "Plan the hero section redesign journey and map the user engagement strategy."
  → "plan" NOT in any predicate lexicon.
  → "redesign" NOT in IMPLEMENT lexicon (gap).
  → "journey" NOT in any predicate lexicon.
  → "strategy" NOT in any predicate lexicon.
  → CompiledPredicateIds: [] → KNOWLEDGE_EDGE

A-fires: "Create a hero section with a visually strong layout and clear CTA button."
  → "create" fires IMPLEMENT. "layout" fires UI. "visual" fires UI. "button" fires UI.
  → CompiledPredicateIds: ["UI", "IMPLEMENT"] → FLOW_EXECUTING
