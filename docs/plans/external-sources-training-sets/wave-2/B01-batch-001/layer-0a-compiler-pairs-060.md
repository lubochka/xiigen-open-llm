# Layer 0-A — Compiler Pairs (060)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: compiler

---

## B01-0A-060-001 — Complete predicate reference for round-3 (files 037–060)

```
source_id: xiigen-b01-0a-060-ref-r3-complete
source_license_state: first_party
pair_kind: compiler
```

Round-3 predicate firing summary:
  "accordion panel": fires via "component", "interaction"
  "dropdown menu": fires via "component", "input"
  "sidebar navigation": "sidebar" and "navigation" are BOTH gap terms — fires only via "component" or "screen"
  "progress bar": fires via "component"
  "loading skeleton": fires via "component" or "screen" (via "skeleton screen")
  "empty state": fires via "component", "button", "screen"
  "error boundary": fires via "component", "ui"
  "infinite scroll": "scroll" is confirmed gap — fires only via IMPLEMENT verb or "component"
  "virtual list": fires via "component"
  "drag-and-drop": "drag", "drop" are confirmed gap — fires via "accessibility", "visual", "widget"
  "resizable panel": fires via "component", "responsive"
  "charts/graphs": fires via "component", "responsive", "theme"
  "map component": fires via "component", "input"
  "rich text editor": fires via "component"
  "file upload": fires via "widget", "form"
  "video player": fires via "component", "ui", "screen", "layout"
  "audio player": fires via "component", "accessibility"
  "code editor": fires via "component", "theme", "design-system"
  "color picker": fires via "component", "color", "input" — strong UI presence
  "rating stars": fires via "component", "accessibility", "input"
  "comment thread": fires via "component", "button", "widget", "input"
  "social share": fires via "component", "button", "widget", "input"

---

## B01-0A-060-002 — "build a complete round-3 component library" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-060-002
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Build a complete UI component library covering accordion, dropdown, color picker, rating, and social share components."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.95

Predicate evidence:
  UI fires on: "ui", "component", "color"
  IMPLEMENT fires on: "build"
  TransitionType: feature_addition.

---

## B01-0A-060-003 — Gap vocabulary introduced in round-3

```
source_id: xiigen-b01-0a-060-r3-gap-list
source_license_state: first_party
pair_kind: compiler
```

New gap terms confirmed in round-3 (files 037–060):

| Term | Why it does NOT fire |
|---|---|
| accordion | Domain noun; not in UI lexicon |
| dropdown | Domain noun; not in UI lexicon |
| sidebar | Confirmed gap from round-2 |
| progress bar | Domain noun; not in UI lexicon |
| skeleton | Domain noun; not in UI lexicon |
| empty state | Domain phrase; not in any lexicon |
| error boundary | Technical pattern name; not in UI lexicon |
| infinite | Domain modifier; not in any lexicon |
| virtual | Domain modifier; not in any lexicon |
| drag / drop | Interaction verbs; not in any predicate lexicon |
| sortable | Adjective; not in any predicate lexicon |
| waveform | Domain noun; not in any predicate lexicon |
| rating | Domain noun; not in any predicate lexicon |
| social / share | Domain nouns; not in UI lexicon |
| make | Gap verb; not in IMPLEMENT lexicon |
| resolve | Gap verb; not in IMPLEMENT lexicon |
| evaluate | Gap verb; not in any predicate lexicon |
| scale | Gap verb; not in any predicate lexicon |

---

## B01-0A-060-004 — "build complete factory reference" fires UI+IMPLEMENT+DOCUMENT+ARCHITECTURE

```
source_id: xiigen-b01-0a-060-004
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Build the complete round-3 component library, document the design system specification, define the component API schemas, and implement all accessibility requirements."
  CompiledPredicateIds: ["UI", "IMPLEMENT", "DOCUMENT", "ARCHITECTURE"]
  Confidence: 0.95

Predicate evidence:
  UI fires on: "component", "design-system", "accessibility"
  IMPLEMENT fires on: "build", "implement"
  DOCUMENT fires on: "document", "specification", "requirements"
  ARCHITECTURE fires on: "API", "schema"
  Four predicates → four independent routing decisions.
  TransitionType: feature_addition (IMPLEMENT present).
