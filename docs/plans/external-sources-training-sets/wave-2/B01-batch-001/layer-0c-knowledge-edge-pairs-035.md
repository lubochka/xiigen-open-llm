# Layer 0-C — Knowledge-Edge Pairs (035)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B01-0C-035-001 — "card sorting" and "usability testing" are gap methodology terms

```
source_id: xiigen-b01-0c-035-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A-gap: "Run card sorting and usability testing sessions to validate the product card layout."
  → "card sorting" = UX research methodology (confirmed gap from file 024 reference).
  → "usability" NOT in any predicate lexicon.
  → "testing" alone does NOT fire TEST or any other predicate in B01.
  → "run" = gap.
  → CompiledPredicateIds: [] → KNOWLEDGE_EDGE

A-fires: "Create a product card component and prototype the layout for usability review."
  → "create" fires IMPLEMENT. "component" fires UI. "prototype" fires UI. "layout" fires UI.
  → CompiledPredicateIds: ["UI", "IMPLEMENT"] → FLOW_EXECUTING

---

## B01-0C-035-002 — "spacing" and "color" both fire UI regardless of request intent

```
source_id: xiigen-b01-0c-035-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A-fires (design token context): "Adjust the spacing and color of the tooltip to match the brand guide."
  → "spacing" fires UI. "color" fires UI.
  → "adjust" NOT in IMPLEMENT lexicon.
  → CompiledPredicateIds: ["UI"] → FLOW_EXECUTING (sub-phase 1.1; documentation TransitionType)

Note: pure design-token adjustment with no build/create/implement term → UI only, documentation TransitionType.

A-contrast: "Implement the tooltip with the correct spacing and color tokens from the design system."
  → "implement" fires IMPLEMENT. "spacing" fires UI. "color" fires UI. "design-system" fires UI.
  → CompiledPredicateIds: ["UI", "IMPLEMENT"] → FLOW_EXECUTING (feature_addition TransitionType)
