# Layer 0-A — Compiler Pairs (056)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: compiler

---

## B01-0A-056-001 — ColorPicker: "build a color picker component" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-056-001
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Build a color picker component with hue slider, saturation field, and hex/RGB input fields."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.95

Predicate evidence:
  UI fires on: "component", "color", "input"
  IMPLEMENT fires on: "build"
  Three UI lexicon hits: "component", "color", "input" — all confirmed UI terms.
  TransitionType: feature_addition.

---

## B01-0A-056-002 — ColorPicker: "create color swatch palette widget" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-056-002
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Create a color swatch palette widget for the design-system theme editor."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.95

Predicate evidence:
  UI fires on: "widget", "color", "design-system", "theme"
  IMPLEMENT fires on: "create"
  Four UI lexicon hits; very high confidence.
  TransitionType: feature_addition.

---

## B01-0A-056-003 — ColorPicker: "define color token schema for design system" fires ARCHITECTURE only

```
source_id: xiigen-b01-0a-056-003
source_license_state: first_party
pair_kind: compiler
transition_type: documentation
```

A:
  user_request: "Define the color token schema for the design system including semantic, brand, and neutral palettes."
  CompiledPredicateIds: ["ARCHITECTURE"]
  Confidence: 0.88

Predicate evidence:
  ARCHITECTURE fires on: "schema"
  "define" NOT in IMPLEMENT lexicon (gap verb).
  Note: "color" IS in UI lexicon BUT "define the color token schema" has no UI verb — UI does NOT fire without a UI action verb in this context.
  Single predicate → single routing decision.
  TransitionType: documentation.

---

## B01-0A-056-004 — ColorPicker: "color picker is not accessible to colorblind users" fires UI

```
source_id: xiigen-b01-0a-056-004
source_license_state: first_party
pair_kind: compiler
transition_type: documentation
```

A:
  user_request: "The color picker is not accessible to colorblind users — review the color contrast ratios."
  CompiledPredicateIds: ["UI"]
  Confidence: 0.78

Predicate evidence:
  UI fires on: "color", "accessibility" (via "accessible")
  No IMPLEMENT verb present ("review" NOT in IMPLEMENT lexicon).
  TransitionType: documentation (UI present, IMPLEMENT absent).
