# Layer 0-C — Knowledge-Edge Pairs (011)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B01-0C-011-001 — UI predicate vocabulary: complete reference

```
source_id: xiigen-b01-0c-011-001
source_title: XIIGen B01 knowledge-edge-011 — UI predicate vocabulary complete reference
source_license_state: first_party
pair_kind: knowledge_edge
coverage_status: covered
```

UI predicate vocabulary grouped by category:

Screen/page terms:
  screen — any screen reference fires UI
  page — any page reference fires UI
  modal — popup dialog screens
  interface — any mention of user interface

Layout terms:
  layout — arrangement and spatial organization
  spacing — whitespace and margin/padding
  typography — font scales, weights, line heights
  color — color systems, palettes, tokens
  visual — visual design generally

Component terms:
  component — UI building blocks
  button — interactive button elements
  input — form input elements
  form — form containers and field groups

System/pattern terms:
  design-system — token libraries, style systems
  theme — dark/light/custom themes
  responsive — adaptive layouts for different screen sizes

UX process terms:
  ux — user experience as an abbreviation
  accessibility — WCAG, ARIA, keyboard navigation
  wireframe — low-fidelity screen sketches
  prototype — interactive or clickable mockups
  interaction — user interaction patterns and states

---

## B01-0C-011-002 — DOCUMENT predicate vocabulary: complete reference

```
source_id: xiigen-b01-0c-011-002
source_title: XIIGen B01 knowledge-edge-011 — DOCUMENT predicate vocabulary complete reference
source_license_state: first_party
pair_kind: knowledge_edge
coverage_status: covered
```

DOCUMENT predicate vocabulary grouped by category:

Creation verbs:
  document — the verb "to document"
  describe — the verb "to describe"
  define — the verb "to define"

Artifact nouns:
  spec / specification — technical specification
  story — user story
  narrative — narrative description
  feature-brief — feature brief document
  use-case / use case — use case description

Requirement terms:
  requirement / requirements — product requirements
  criteria — acceptance criteria
  functional — functional requirements
  behavior — behavioral specification
  epics — agile epics
  capability / capabilities — capability specifications

---

## B01-0C-011-003 — Vocabulary shared between lexicons (context-dependent)

```
source_id: xiigen-b01-0c-011-003
source_title: XIIGen B01 knowledge-edge-011 — shared vocabulary analysis
source_license_state: first_party
pair_kind: knowledge_edge
coverage_status: covered
```

Terms that appear in exactly one lexicon (no ambiguity):

UI-only: screen, page, modal, form, layout, spacing, typography, color, visual,
  component, button, input, design-system, theme, responsive, accessibility,
  wireframe, prototype, interaction, ux, interface

DOCUMENT-only: document, describe, define, specification, story, narrative,
  feature-brief, use-case, requirement, criteria, functional, behavior, epics, capability

IMPLEMENT-only: implement, code, build, develop, library, handler, middleware,
  controller, backend, method, refactor

Terms in BOTH IMPLEMENT and non-UI/DOCUMENT contexts:
  write — IMPLEMENT lexicon; also could indicate authoring but always fires IMPLEMENT
  create — IMPLEMENT lexicon
  feature — IMPLEMENT lexicon (not to be confused with "feature-brief" in DOCUMENT)

The "feature" distinction:
  "feature" alone → IMPLEMENT fires
  "feature-brief" → DOCUMENT fires
  "feature brief" (two words) → DOCUMENT fires (treated as "feature-brief")
