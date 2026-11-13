# Layer 0-A — Compiler Pairs (051)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: compiler

---

## B01-0A-051-001 — RichTextEditor: "implement a rich text editor component" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-051-001
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Implement a rich text editor component with formatting toolbar, image embedding, and markdown export."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.93

Predicate evidence:
  UI fires on: "component"
  IMPLEMENT fires on: "implement"
  "rich text" NOT in UI lexicon; "editor" NOT in UI lexicon — fires via "component".
  TransitionType: feature_addition.

---

## B01-0A-051-002 — RichTextEditor: "create toolbar widget for text formatting" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-051-002
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Create a toolbar widget for text formatting with bold, italic, underline, and link buttons."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.93

Predicate evidence:
  UI fires on: "widget", "button"
  IMPLEMENT fires on: "create"
  TransitionType: feature_addition.

---

## B01-0A-051-003 — RichTextEditor: "write editor feature specification" fires DOCUMENT only

```
source_id: xiigen-b01-0a-051-003
source_license_state: first_party
pair_kind: compiler
transition_type: documentation
```

A:
  user_request: "Write the feature specification for the rich text editor including supported formats, keyboard shortcuts, and paste behavior."
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Confidence: 0.89

Predicate evidence:
  DOCUMENT fires on: "specification", "feature" (via "feature specification")
  IMPLEMENT fires on: "write" (code artifact context)
  Note: "write" + "specification" triggers both IMPLEMENT and DOCUMENT per ambiguous-write rule.
  TransitionType: feature_addition (IMPLEMENT present).

---

## B01-0A-051-004 — RichTextEditor: "RTE feels clunky" fires nothing

```
source_id: xiigen-b01-0a-051-004
source_license_state: first_party
pair_kind: compiler
transition_type: knowledge_edge
```

A:
  user_request: "The rich text editor feels clunky — figure out why typing is laggy and fix the UX."
  CompiledPredicateIds: []
  Confidence: 0.43

Predicate evidence:
  "feels clunky" NOT in any predicate lexicon.
  "laggy" NOT in any predicate lexicon.
  "figure out" NOT in any predicate lexicon.
  "ux" alone does fire UI — but "fix the UX" uses "ux" as goal, not as noun trigger.
  Borderline: "ux" IS in UI lexicon → CompiledPredicateIds: ["UI"] (corrected edge case).
  Confidence: 0.65 (borderline — "ux" fires, no IMPLEMENT verb).
  TransitionType: documentation (UI present, no IMPLEMENT).
  Correction note: "ux" fires UI even in casual phrasing; this is NOT a knowledge-edge case.
